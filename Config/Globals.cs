using CounterStrikeSharp.API.Core;

namespace ESP_Players;

public static class Globals_Static
{
    public class PersonData
    {
        public ulong PlayerSteamID { get; set; }
        public int Toggle_ESP { get; set; }
        public DateTime DateAndTime { get; set; }
    }
}

public class Globals
{
    public CounterStrikeSharp.API.Modules.Timers.Timer Timer = null!;

    public class PlayerDataClass
    {
        public CCSPlayerController Player { get; set; }
        public CCSPlayerController Player_Controlled_Bot { get; set; }
        public string ModelName  { get; set; }
        public CDynamicProp ModelRelay { get; set; }
        public CDynamicProp ModelGlow { get; set; }
        public ulong SteamId { get; set; }
        public int Toggle_ESP { get; set; }
        public DateTime Time { get; set; }
        public DateTime EventPlayerChat { get; set; }

        public PlayerDataClass(CCSPlayerController Playerr, CCSPlayerController Player_Controlled_Bott, string ModelNamee, CDynamicProp ModelRelayy, CDynamicProp ModelGloww, ulong SteamIdd, int Toggle_ESPP, DateTime Timee, DateTime EventPlayerChatt)
        {
            Player = Playerr;
            Player_Controlled_Bot = Player_Controlled_Bott;
            ModelName = ModelNamee;
            ModelRelay = ModelRelayy;
            ModelGlow = ModelGloww;
            SteamId = SteamIdd;
            Toggle_ESP = Toggle_ESPP;
            Time = Timee;
            EventPlayerChat = EventPlayerChatt;
        }
    }
    public Dictionary<CCSPlayerController, PlayerDataClass> Player_Data = new Dictionary<CCSPlayerController, PlayerDataClass>();

}