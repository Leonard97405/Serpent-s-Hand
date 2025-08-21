using System;
using System.Collections.Generic;
using System.ComponentModel;
using LabApi.Events.CustomHandlers;
using LabApi.Features;
using LabApi.Loader.Features.Plugins;

namespace SerpentHands
{
    public class Plugin : Plugin<Config>
    {
        public static Plugin Singleton = null;
        public override void Enable()
        {
            Singleton = this;
            Singleton.Config._hand.SpawnChance = Singleton.Config.SpawnChance;
            Singleton.Config._hand.SpawnDelay = Singleton.Config.SpawnDelay;
            Singleton.Config._hand.MinPlayersRequired = Singleton.Config.MinPlayer;
            NetRoleManager.NetRoleManager.Instance.RegisterRole(Config._hand);
            CustomHandlersManager.RegisterEventsHandler(new RoleEvents());
        }

        public override void Disable()
        {
            Singleton = null;
            CustomHandlersManager.UnregisterEventsHandler(new RoleEvents());
        }

        public override string Name { get; } = "Serpents Hands";
        public override string Description { get; } = "Ruolo custom serpents hand";
        public override string Author { get; } = "Lenard";
        public override Version Version { get; } = new Version(1, 0, 0);
        public override Version RequiredApiVersion { get; } = new Version(LabApiProperties.CompiledVersion);
    }

    public class Config
    {
        public SerpentsHand _hand = new SerpentsHand();
        public int SpawnChance { get; set; } = 80;
        public int MinPlayer { get; set; } = 12;
        public float SpawnDelay { get; set; } = 420;

        [Description("79 dovrebbe sopravvivere il recontainment automatico fino a quando un SH è vivo?")]
        public bool survive79 { get; set; } = false;

        [Description("Rapporto Player Serpent su 100%")]
        public int RappSH { get; set; } = 20;

    }

    
}