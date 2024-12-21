using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace BossKillInfo
{
    [ApiVersion(2, 1)]
    public class BossKillInfoPlugin : TerrariaPlugin
    {
        public override string Name => "BossKillInfo";
        public override string Author => "Keyou";
        public override string Description => "Displays a message with boss kill information and players who hit the boss.";
        public override Version Version => new Version(1, 0);

        private Dictionary<int, Dictionary<string, int>> bossDamage;

        public BossKillInfoPlugin(Main game) : base(game)
        {
        }

        public override void Initialize()
        {
            ServerApi.Hooks.NpcStrike.Register(this, OnNpcStrike);
            ServerApi.Hooks.NpcKilled.Register(this, OnNpcKilled);
            bossDamage = new Dictionary<int, Dictionary<string, int>>();
        }

        private void OnNpcStrike(NpcStrikeEventArgs args)
        {
            if (args.Npc.boss)
            {
                if (!bossDamage.ContainsKey(args.Npc.whoAmI))
                {
                    bossDamage[args.Npc.whoAmI] = new Dictionary<string, int>();
                }

                string playerName = TShock.Players[args.Player.whoAmI].Name;
                if (!bossDamage[args.Npc.whoAmI].ContainsKey(playerName))
                {
                    bossDamage[args.Npc.whoAmI][playerName] = 0;
                }

                bossDamage[args.Npc.whoAmI][playerName] += args.Damage;
            }
        }

        private void OnNpcKilled(NpcKilledEventArgs args)
        {
            if (args.npc.boss && bossDamage.ContainsKey(args.npc.whoAmI))
            {
                var totalDamage = bossDamage[args.npc.whoAmI].Values.Sum();
                var message = $"[[C/FF0000:{args.npc.FullName}]]";

                foreach (var player in bossDamage[args.npc.whoAmI])
                {
                    var percentage = player.Value / (float)totalDamage * 100;
                    message += $"\n- [C/00FF00:{player.Key}] damage: [C/FFFF00:{player.Value}] ([C/00FFFF:{percentage:F1}%])";
                }

                TSPlayer.All.SendMessage(message, Microsoft.Xna.Framework.Color.LimeGreen);
                bossDamage.Remove(args.npc.whoAmI);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.NpcStrike.Deregister(this, OnNpcStrike);
                ServerApi.Hooks.NpcKilled.Deregister(this, OnNpcKilled);
            }
            base.Dispose(disposing);
        }
    }
}