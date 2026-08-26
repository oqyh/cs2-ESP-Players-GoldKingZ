using CounterStrikeSharp.API.Core;

namespace ESP_Players_GoldKingZ.Shared;

public interface IESPPlayersApi
{
    /// <summary>
    /// True if ESP is active (own toggle OR admin-given). personalToggleOnly = true checks only the player's own toggle.
    /// </summary>
    bool IsESPActive(CCSPlayerController player, bool personalToggleOnly = false);

    /// <summary>
    /// True if ESP is active (own toggle OR admin-given). personalToggleOnly = true checks only the player's own toggle.
    /// </summary>
    bool IsESPActive(int slot, bool personalToggleOnly = false);

    /// <summary>
    /// True if the player matches the rule. Use a full permissionsString OR the named segments (full string wins).
    /// </summary>
    bool HasPermission(CCSPlayerController player, string permissionsString = "", string SteamIDs = "", string Flags = "", string Groups = "");

    /// <summary>
    /// True if the player matches the rule. Use a full permissionsString OR the named segments (full string wins).
    /// </summary>
    bool HasPermission(int slot, string permissionsString = "", string SteamIDs = "", string Flags = "", string Groups = "");

    /// <summary>
    /// Grants (true) or revokes (false) admin-style ESP. Fires OnESPChanged only when the state changes.
    /// </summary>
    void SetESP(CCSPlayerController player, bool state);

    /// <summary>
    /// Grants (true) or revokes (false) admin-style ESP. Fires OnESPChanged only when the state changes.
    /// </summary>
    void SetESP(int slot, bool state);

    /// <summary>
    /// Subscribes a handler fired when a player's granted-ESP changes. Safe to call twice (no double-subscribe).
    /// </summary>
    void Hook(Action<CCSPlayerController, bool> onESPChanged);

    /// <summary>
    /// Removes a handler added via Hook. Call in your plugin's Unload.
    /// </summary>
    void Unhook(Action<CCSPlayerController, bool> onESPChanged);
}