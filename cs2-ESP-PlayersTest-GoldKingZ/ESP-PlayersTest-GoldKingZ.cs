using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using ESP_Players_GoldKingZ.Shared;

namespace ESP_PlayersTest_GoldKingZ;

// ============================================================================
// ESP-Players-GoldKingZ.Shared — CONSUMER DEMO
//
// SETUP:
//   1. Grab the API in OnAllPluginsLoaded (load order isn't guaranteed).
//   2. _esp.Hook(handler)   in OnAllPluginsLoaded  — subscribe to ESP changes.
//   3. _esp.Unhook(handler) in Unload              — always clean up.
//
// QUICK REFERENCE:
//   !testesp_status               — is ESP active for you (toggle OR admin-given)
//   !testesp_status_toggle        — is ESP active from YOUR OWN toggle only
//   !testesp_give                 — grant yourself ESP via API  (SetESP true)
//   !testesp_take                 — revoke your ESP via API      (SetESP false)
//   !testesp_perm                 — run every HasPermission calling style
//   OnESPChanged(player, state)   — fires on any grant/revoke (admin cmd or API)
//
// API SURFACE:
//   ESPPlayersApi.Get()                                   — null if plugin not loaded
//   IsESPActive(player | slot, personalToggleOnly=false)  — toggle OR admin-given
//   HasPermission(player | slot, "SteamIDs:..|Flags:..|Groups:..")   — full string
//   HasPermission(player | slot, Flags:"@css/admin")                 — named segment
//   SetESP(player | slot, bool state)                     — grant/revoke, fires event
//   Hook(handler) / Unhook(handler)                       — (un)subscribe callback
// ============================================================================

public class ESPPlayersTest : BasePlugin
{
    public override string ModuleName => "Show Glow/Esp To Players With Flags (API Test)";
    public override string ModuleVersion => "1.0.4";
    public override string ModuleAuthor => "Gold KingZ";
    public override string ModuleDescription => "https://github.com/oqyh";

    private IESPPlayersApi? _esp;

    // Grab the capability AFTER all plugins loaded — load order between plugins isn't guaranteed.
    public override void OnAllPluginsLoaded(bool hotReload)
    {
        _esp = ESPPlayersApi.Get();
        if (_esp == null)
        {
            Console.WriteLine("[ESPTest] ESP-Players API not found. Is the main plugin installed and loaded?");
            return;
        }

        _esp.Hook(OnESPChanged);
        Console.WriteLine("[ESPTest] Hooked into ESP-Players API successfully.");
    }

    // Always unsubscribe on unload — CS2Sharp hot-reloads plugins individually.
    public override void Unload(bool hotReload)
    {
        _esp?.Unhook(OnESPChanged);
    }

    // Fired whenever a player's granted-ESP state changes (admin command or any plugin's SetESP).
    private void OnESPChanged(CCSPlayerController player, bool state)
    {
        Console.WriteLine($"[ESPTest] ESP for {player.PlayerName} changed -> {(state ? "ON" : "OFF")}");
        // Store use-case examples: refund credits, start an expiry timer, update a HUD, etc.
    }

    // ---- READ: is ESP active for the caller (toggle OR admin-given) ----
    [ConsoleCommand("css_testesp_status", "Check if you have ESP active")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnStatus(CCSPlayerController? player, CommandInfo cmd)
    {
        if (player == null || !player.IsValid || _esp == null) return;
        cmd.ReplyToCommand($"[ESPTest] IsESPActive = {_esp.IsESPActive(player)}");
    }

    // ---- READ: is ESP active from the player's OWN toggle only (ignores admin grants) ----
    [ConsoleCommand("css_testesp_status_toggle", "Check if you toggled ESP on yourself")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnStatusToggle(CCSPlayerController? player, CommandInfo cmd)
    {
        if (player == null || !player.IsValid || _esp == null) return;
        cmd.ReplyToCommand($"[ESPTest] IsESPActive (personalToggleOnly) = {_esp.IsESPActive(player, personalToggleOnly: true)}");
    }

    // ---- WRITE: grant ESP to yourself through the API ----
    [ConsoleCommand("css_testesp_give", "Give yourself ESP through the API")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnGive(CCSPlayerController? player, CommandInfo cmd)
    {
        if (player == null || !player.IsValid || _esp == null) return;
        _esp.SetESP(player, true);
        cmd.ReplyToCommand("[ESPTest] SetESP(true) called — watch console for the OnESPChanged callback.");
    }

    // ---- WRITE: revoke ESP from yourself through the API ----
    [ConsoleCommand("css_testesp_take", "Remove your ESP through the API")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnTake(CCSPlayerController? player, CommandInfo cmd)
    {
        if (player == null || !player.IsValid || _esp == null) return;
        _esp.SetESP(player, false);
        cmd.ReplyToCommand("[ESPTest] SetESP(false) called — watch console for the OnESPChanged callback.");
    }

    // ---- PERMISSION: every calling style in one command ----
    [ConsoleCommand("css_testesp_perm", "Test the permission-string checker")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnPerm(CCSPlayerController? player, CommandInfo cmd)
    {
        if (player == null || !player.IsValid || _esp == null) return;
        
        // single named segment:
        cmd.ReplyToCommand($"[ESPTest] Flags only    -> {_esp.HasPermission(player, Flags: "@css/root,@css/admin")}");
        cmd.ReplyToCommand($"[ESPTest] SteamIDs only -> {_esp.HasPermission(player, SteamIDs: "76561198206086993,STEAM_0:1:507335558")}");
        cmd.ReplyToCommand($"[ESPTest] Groups only   -> {_esp.HasPermission(player, Groups: "#css/admin")}");

        // combined named segments:
        cmd.ReplyToCommand($"[ESPTest] Flags+Groups  -> {_esp.HasPermission(player, Flags: "@css/admin", Groups: "#css/vip")}");

        // full string (e.g. straight from a config value):
        const string full = "SteamIDs: 76561198206086993,STEAM_0:1:507335558 | Flags: @css/root,@css/admin | Groups: #css/root,#css/admin";
        cmd.ReplyToCommand($"[ESPTest] Full string   -> {_esp.HasPermission(player, full)}");
    }
}