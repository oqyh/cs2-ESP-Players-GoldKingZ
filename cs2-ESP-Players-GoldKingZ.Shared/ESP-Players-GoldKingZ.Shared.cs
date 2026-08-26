using CounterStrikeSharp.API.Core.Capabilities;

namespace ESP_Players_GoldKingZ.Shared;

public static class ESPPlayersApi
{
    public const string CapabilityName = "goldkingz:espplayers";
    public static readonly PluginCapability<IESPPlayersApi> Capability = new(CapabilityName);
    public static IESPPlayersApi? Get() => Capability.Get();
}