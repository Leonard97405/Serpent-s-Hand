using System.Collections.Generic;
using System.Linq;
using CustomPlayerEffects;
using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Models.Hints;
using HintServiceMeow.Core.Utilities;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.Scp049Events;
using LabApi.Events.Arguments.Scp079Events;
using LabApi.Events.Arguments.Scp096Events;
using LabApi.Events.Arguments.Scp106Events;
using LabApi.Events.Arguments.Scp173Events;
using LabApi.Events.CustomHandlers;
using LabApi.Features.Wrappers;
using MapGeneration;
using MEC;
using NetRoleManager;
using PlayerRoles;
using PlayerRoles.PlayableScps.Scp079;
using UnityEngine;
using VoiceChat;

namespace SerpentHands
{
    public class SerpentsHand : CustomRole
    {
        public override string RoleId { get; set; } = "sh";
        public override string Name { get; set; } = "Serpent Hand";
        public override string Description { get; set; } = "Collabora con gli scp per uccidere tutti gli umani";
        public override RoleTypeId RoleTypeId { get; set; } = RoleTypeId.Tutorial;
        public override int SpawnChance { get; set; } =0;
        public override int MaxPlayersAllowed { get; set; } = 0;
        public override float SpawnDelay { get; set; } =0;

        public override SpawnProperty[] SpawnProperties { get; set; } = new[]
        {
            new SpawnProperty()
            {
                Chance = 100,
                Offset = new Vector3(-63f,-8,-50.8f),
                RoomName = RoomName.Outside,
            }
        };
    

        public override void OnRoleAdded(Player player)
        {
            RoleEvents.sHs.Add(player);
        }
    }

    public class RoleEvents : CustomEventsHandler
    {
        public static List<Player> sHs = new List<Player>();
        private DynamicHint dH = new DynamicHint()
        {
            Id = "serpentId",
            Text = "Gli <color=red>SCP</color> sono tuoi teammate, non puoi danneggiarli.",
            TargetY = 900,
            FontSize = 20,
            SyncSpeed = HintSyncSpeed.UnSync,
        };

        private DynamicHint pH = new DynamicHint()
        {
            Id = "scpId",
            Text = "I <color=#F309AC>Serpent's Hand</color> sono tuoi alleati, non puoi danneggiarli",
            TargetY = 900,
            FontSize = 20,
            SyncSpeed = HintSyncSpeed.UnSync
        };
        public override void OnPlayerHurting(PlayerHurtingEventArgs ev)
        {
            if (ev.Player.Team == Team.SCPs &&
                sHs.Contains(ev.Attacker))
            {
                ev.IsAllowed = false;
                PlayerDisplay pD = PlayerDisplay.Get(ev.Attacker);
                if (!pD.HasHint(dH.Id))
                {
                    pD.AddHint(dH);
                    Timing.CallDelayed(2f, () => pD.RemoveHint(dH));
                }
            }
            else if (sHs.Contains(ev.Player) && ev.Attacker.Team == Team.SCPs)
            {
                ev.IsAllowed = false;
                PlayerDisplay pD = PlayerDisplay.Get(ev.Attacker);
                if (!pD.HasHint(pH.Id))
                {
                    pD.AddHint(pH);
                    Timing.CallDelayed(2f, () => pD.RemoveHint(pH));
                }                
            }
        }

        public override void OnPlayerActivatingGenerator(PlayerActivatingGeneratorEventArgs ev)
        {
            if (NetRoleManager.NetRoleManager.Instance.HasCustomRole(ev.Player, Plugin.Singleton.Config._hand.RoleId))
            {
                ev.IsAllowed = false;
            }
        }

        public override void OnScp049UsingSense(Scp049UsingSenseEventArgs ev)
        {
            if (sHs.Contains(ev.Target)) ev.IsAllowed = false;
        }

        public override void OnScp106TeleportingPlayer(Scp106TeleportingPlayerEvent ev)
        {
            if (sHs.Contains(ev.Target)) ev.IsAllowed = false;
        }

        public override void OnScp079Recontaining(Scp079RecontainingEventArgs ev)
        {
            if (ev.Activator != null)
            {
               if( NetRoleManager.NetRoleManager.Instance.HasCustomRole(ev.Activator, Plugin.Singleton.Config._hand.RoleId))
                {
                    ev.IsAllowed = false;
                }
            }else if (ev.Activator == null && Plugin.Singleton.Config.survive79 && Player.ReadyList.Any(p=> NetRoleManager.NetRoleManager.Instance.HasCustomRole(p,Plugin.Singleton.Config._hand.RoleId)))
            {
                ev.IsAllowed = false;
            } 
        }

        public override void OnServerRoundStarted()
        {
            Timing.CallDelayed(Plugin.Singleton.Config.SpawnDelay - 20, () =>
            {
                Plugin.Singleton.Config._hand.MaxPlayersAllowed =
                    Player.ReadyList.Where(p => p.Role != RoleTypeId.Overwatch).Count() /
                     100 * Plugin.Singleton.Config.RappSH;
            });
        }

        public override void OnPlayerDying(PlayerDyingEventArgs ev)
        {
            if(sHs.Contains(ev.Player))
            {
                if (!Player.ReadyList.Any(p =>
                        sHs.Contains(p) ||
                        (p.Team == Team.SCPs && p.Role != RoleTypeId.Scp079)))
                {
                    Player P =Player.ReadyList.FirstOrDefault(p => p.Role == RoleTypeId.Scp079);
                    if (P != null) P.Kill("Ricontenuto");

                }

                sHs.Remove(ev.Player);
            }
        }

        public override void OnScp096AddingTarget(Scp096AddingTargetEventArgs ev)
        {
            if (NetRoleManager.NetRoleManager.Instance.HasCustomRole(ev.Target, Plugin.Singleton.Config._hand.RoleId))
                ev.IsAllowed = false;
        }

        public override void OnScp173AddingObserver(Scp173AddingObserverEventArgs ev)
        {
            if (NetRoleManager.NetRoleManager.Instance.HasCustomRole(ev.Target, Plugin.Singleton.Config._hand.RoleId))
                ev.IsAllowed = false;
        }

        public override void OnPlayerUpdatingEffect(PlayerEffectUpdatingEventArgs ev)
        {
            AmnesiaVision z = new AmnesiaVision();
            Stained s = new Stained();
            Corroding c = new Corroding();
            if (sHs.Contains(ev.Player) && (ev.Effect.name == c.name || ev.Effect.name == s.name || ev.Effect.name == z.name)) ev.IsAllowed = false;
        }

        private static List<Player> escapingP = new List<Player>();
        public override void OnPlayerEscaping(PlayerEscapingEventArgs ev)
        {
            if (escapingP.Contains(ev.Player) || !ev.Player.IsDisarmed) return;
            if (NetRoleManager.NetRoleManager.Instance.HasCustomRole(ev.Player.DisarmedBy,
                    Plugin.Singleton.Config._hand.RoleId))
            {
                escapingP.Add(ev.Player);
                ev.Player.SetRole(RoleTypeId.Tutorial);
                NetRoleManager.NetRoleManager.Instance.AssignRole(ev.Player,Plugin.Singleton.Config._hand);
                escapingP.Remove(ev.Player);
            }
        }
    }
}