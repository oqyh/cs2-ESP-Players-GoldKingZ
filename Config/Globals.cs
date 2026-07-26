using System.Drawing;
using CounterStrikeSharp.API.Core;

namespace ESP_Players_GoldKingZ;

public class Globals
{
    public CounterStrikeSharp.API.Modules.Timers.Timer Timer = null!;

    public class PlayerDataClass
    {
        public CCSPlayerController Player { get; set; }
        public string Player_ModelName  { get; set; }
        public CDynamicProp ModelRelay { get; set; }
        public CDynamicProp ModelGlow { get; set; }
        public DateTime EventPlayerChat { get; set; }

        public PlayerDataClass(CCSPlayerController Playerr, string Player_ModelNamee, CDynamicProp ModelRelayy, CDynamicProp ModelGloww, DateTime EventPlayerChatt)
        {
            Player = Playerr;
            Player_ModelName = Player_ModelNamee;
            ModelRelay = ModelRelayy;
            ModelGlow = ModelGloww;
            EventPlayerChat = EventPlayerChatt;
        }
    }
    public Dictionary<int, PlayerDataClass> Player_Data = new Dictionary<int, PlayerDataClass>();

    public void Clear()
    {
        Timer?.Kill();
        Timer = null!;

        Helper.RemoveAllPlayersGlow();
        
    }
}