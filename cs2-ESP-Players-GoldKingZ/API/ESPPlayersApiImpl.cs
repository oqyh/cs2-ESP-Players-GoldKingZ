using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using ESP_Players_GoldKingZ.Shared;

namespace ESP_Players_GoldKingZ;

public class ESPPlayersApiImpl : IESPPlayersApi
{
    private event Action<CCSPlayerController, bool>? _onESPChanged;

    public void Hook(Action<CCSPlayerController, bool> onESPChanged)
    {
        if (onESPChanged == null) return;
        _onESPChanged -= onESPChanged;
        _onESPChanged += onESPChanged;
    }

    public void Unhook(Action<CCSPlayerController, bool> onESPChanged)
    {
        if (onESPChanged == null) return;
        _onESPChanged -= onESPChanged;
    }

    public void RaiseESPChanged(CCSPlayerController player, bool state)
        => _onESPChanged?.Invoke(player, state);

    private static CCSPlayerController? FromSlot(int slot)
    {
        var p = Utilities.GetPlayerFromSlot(slot);
        return (p != null && p.IsValid) ? p : null;
    }

    public bool IsESPActive(CCSPlayerController player, bool personalToggleOnly = false)
    {
        if (player == null || !player.IsValid) return false;

        var _prefs = MainPlugin.Instance._prefs;
        var g_Main = MainPlugin.Instance.g_Main;

        bool toggle = _prefs != null && _prefs.TryGetValue(player.Slot, out var prefs) && prefs.Toggle_ESP;
        if (personalToggleOnly)
            return toggle;

        bool given = g_Main.Player_Data.TryGetValue(player.Slot, out var data) && data.Gived_ESP;
        return toggle || given;
    }
    public bool IsESPActive(int slot, bool personalToggleOnly = false)
        => FromSlot(slot) is { } p && IsESPActive(p, personalToggleOnly);

    private static string BuildPermissionString(string steamIds, string flags, string groups)
    {
        var parts = new List<string>(3);
        if (!string.IsNullOrWhiteSpace(steamIds)) parts.Add($"SteamIDs: {steamIds.Trim()}");
        if (!string.IsNullOrWhiteSpace(flags))    parts.Add($"Flags: {flags.Trim()}");
        if (!string.IsNullOrWhiteSpace(groups))   parts.Add($"Groups: {groups.Trim()}");
        return string.Join(" | ", parts);
    }

    public bool HasPermission(CCSPlayerController player, string permissionsString = "", string SteamIDs = "", string Flags = "", string Groups = "")
    {
        if (player == null || !player.IsValid) return false;

        string final = !string.IsNullOrWhiteSpace(permissionsString)
            ? permissionsString
            : BuildPermissionString(SteamIDs, Flags, Groups);

        if (!final.HasValidPermissionData()) return false;
        return Helper.IsPlayerInGroupPermission(player, final);
    }
    public bool HasPermission(int slot, string permissionsString = "", string SteamIDs = "", string Flags = "", string Groups = "")
        => FromSlot(slot) is { } p && HasPermission(p, permissionsString, SteamIDs, Flags, Groups);

    public void SetESP(CCSPlayerController player, bool state)
    {
        if (player == null || !player.IsValid) return;

        var g_Main = MainPlugin.Instance.g_Main;
        Helper.CheckPlayerInGlobals(player);
        if (!g_Main.Player_Data.TryGetValue(player.Slot, out var data)) return;
        if (data.Gived_ESP == state) return;

        data.Gived_ESP = state;
        RaiseESPChanged(player, state);
    }
    public void SetESP(int slot, bool state)
    {
        if (FromSlot(slot) is { } p) SetESP(p, state);
    }
}