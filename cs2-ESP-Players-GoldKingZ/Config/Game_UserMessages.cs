using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using System.Text;
using ESP_Players_GoldKingZ.Config;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Core.Translations;
using Microsoft.VisualBasic;
using System.Text.RegularExpressions;
using CounterStrikeSharp.API.Modules.UserMessages;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using Newtonsoft.Json.Linq;
using CounterStrikeSharp.API.Modules.Commands.Targeting;

namespace ESP_Players_GoldKingZ;

public class Game_UserMessages
{
    public HookResult HookPlayerChat_UserMessages(CCSPlayerController? player, string message, UserMessage? um = null)
    {
        if(player == null || !player.IsValid) return HookResult.Continue;

        if (Configs.Instance.Reload_Plugin.Reload_Plugin_CommandsInGame.ConvertCommands(true)?.Any(c => message.Equals(c.Trim(), StringComparison.OrdinalIgnoreCase)) == true)
        {
            Handle_ReloadPlugin(player, null!, um!);
        }

        if (Configs.Instance.Give_ESP.Give_ESP_CommandsInGame.ConvertCommands(true)?.Any(c => message.Equals(c.Trim(), StringComparison.OrdinalIgnoreCase) || message.StartsWith(c.Trim() + " ", StringComparison.OrdinalIgnoreCase)) == true)
        {
            Handle_CommandsAction_Give_ESP(player, null!, um!, message);
        }

        if (Configs.Instance.Toggle_ESP.Toggle_ESP_CommandsInGame.ConvertCommands(true)?.Any(c => message.Equals(c.Trim(), StringComparison.OrdinalIgnoreCase)) == true)
        {
            Handle_CommandsAction_Toggle_ESP(player, null!, um!);
        }

        return HookResult.Continue;
    }

    #region Commands Hook

    public void CommandsAction_ReloadPlugin(CCSPlayerController? player, CommandInfo info)
    {
        if(player == null || !player.IsValid) return;

        Handle_ReloadPlugin(player, info, null!);
    }

    public void CommandsAction_Give_ESP(CCSPlayerController? player, CommandInfo info)
    {
        if(player == null || !player.IsValid) return;

        string fullCommand = info.GetCommandString;
        Handle_CommandsAction_Give_ESP(player, info, null!, fullCommand);
    }

    public void CommandsAction_Toggle_ESP(CCSPlayerController? player, CommandInfo info)
    {
        if(player == null || !player.IsValid) return;

        Handle_CommandsAction_Toggle_ESP(player, info, null!);
    }
    
    #endregion Commands Hook




    #region Handles

    public static void Handle_ReloadPlugin(CCSPlayerController player, CommandInfo commandInfo = null!, UserMessage um = null!)
    {
        if (!MainPlugin.Instance.g_Main.Player_Data.TryGetValue(player.Slot, out var playerData)) return;

        bool onetime = (DateTime.Now - playerData.EventPlayerChat).TotalSeconds > 0.4;
        if (onetime) playerData.EventPlayerChat = DateTime.Now;


        var cfg = Configs.Instance.Reload_Plugin;

        if (cfg.Reload_Plugin_Flags.HasValidPermissionData() && !Helper.IsPlayerInGroupPermission(player, cfg.Reload_Plugin_Flags))
        {
            if (onetime)
            {
                Helper.AdvancedPlayerPrintToChat(player, commandInfo, MainPlugin.Instance.Localizer["PrintToChatToPlayer.ReloadPlugin.Not.Allowed"]);
            }
        }
        else
        {
            if (onetime)
            {
                Server.NextFrame(() =>
                {
                    Helper.RemoveRegisterCommandsAndHooks();
                    Helper.ClearVariables();
                    Configs.Load(MainPlugin.Instance.ModuleDirectory, true);
                    Helper.RegisterCommandsAndHooks();
                    Helper.ReloadPlayersGlobals();
                });

                Helper.AdvancedPlayerPrintToChat(player, commandInfo, MainPlugin.Instance.Localizer["PrintToChatToPlayer.ReloadPlugin.Successfully"]);
            }

            Helper.MuteCommands(um, cfg.Reload_Plugin_Hide);
        }

        Helper.MuteCommands(um, cfg.Reload_Plugin_Hide, true);
    }

    public static void Handle_CommandsAction_Give_ESP(CCSPlayerController player, CommandInfo commandInfo = null!, UserMessage um = null!, string command = "")
    {
        if (MainPlugin.Instance._prefs == null || !MainPlugin.Instance._prefs.TryGetValue(player.Slot, out var prefs) || !MainPlugin.Instance.g_Main.Player_Data.TryGetValue(player.Slot, out var playerData)) return;

        command = command.Trim();
        int space = command.IndexOf(' ');
        string usedCommand = space >= 0 ? command.Substring(0, space).Trim() : command;
        string args = space >= 0 ? command.Substring(space + 1).Trim() : "";

        bool onetime = (DateTime.Now - playerData.EventPlayerChat).TotalSeconds > 0.4;
        if (onetime) playerData.EventPlayerChat = DateTime.Now;

        var cfg = Configs.Instance.Give_ESP;

        if (cfg.Give_ESP_Flags.HasValidPermissionData() && !Helper.IsPlayerInGroupPermission(player, cfg.Give_ESP_Flags))
        {
            if (onetime)
            {
                Helper.AdvancedPlayerPrintToChat(player, commandInfo, MainPlugin.Instance.Localizer["PrintToChatToPlayer.Give.ESP.Not.Allowed"]);
            }
        }
        else
        {
            if (onetime)
            {
                string StateWord(bool on) => MainPlugin.Instance.Localizer[on
                    ? "PrintToChatToPlayer.Give.ESP.State.Enabled"
                    : "PrintToChatToPlayer.Give.ESP.State.Disabled"];

                string[] parts = args.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length == 0)
                {
                    Helper.AdvancedPlayerPrintToChat(player, commandInfo, MainPlugin.Instance.Localizer["PrintToChatToPlayer.Give.ESP.Usage"], usedCommand);
                }
                else
                {
                    string targetQuery;
                    string? valueArg = null;

                    string last = parts[^1];
                    bool lastIsValue = last.Equals("1") || last.Equals("0")
                        || last.Equals("true", StringComparison.OrdinalIgnoreCase)
                        || last.Equals("false", StringComparison.OrdinalIgnoreCase);

                    if (parts.Length > 1 && lastIsValue)
                    {
                        valueArg = last;
                        targetQuery = string.Join(' ', parts[..^1]);
                    }
                    else
                    {
                        targetQuery = string.Join(' ', parts);
                    }

                    TargetResult targetResult = new Target(targetQuery).GetTarget(player);

                    var targets = targetResult.Players
                        .Where(p => p != null && p.IsValid && !p.IsBot && !p.IsHLTV)
                        .ToList();

                    if (targets.Count == 0)
                    {
                        Helper.AdvancedPlayerPrintToChat(player, commandInfo, MainPlugin.Instance.Localizer["PrintToChatToPlayer.Give.ESP.NoTarget"], targetQuery);
                    }
                    else if (valueArg == null)
                    {
                        var withData = targets
                            .Where(t => MainPlugin.Instance.g_Main.Player_Data.ContainsKey(t.Slot))
                            .ToList();

                        if (withData.Count == 0)
                        {
                            Helper.AdvancedPlayerPrintToChat(player, commandInfo, MainPlugin.Instance.Localizer["PrintToChatToPlayer.Give.ESP.NoTarget"], targetQuery);
                        }
                        else if (withData.Count <= 5)
                        {
                            foreach (var target in withData)
                            {
                                MainPlugin.Instance.g_Main.Player_Data.TryGetValue(target.Slot, out var tData);
                                Helper.AdvancedPlayerPrintToChat(player, commandInfo, MainPlugin.Instance.Localizer["PrintToChatToPlayer.Give.ESP.Status"], target.PlayerName, StateWord(tData!.Gived_ESP));
                            }
                        }
                        else
                        {
                            Helper.AdvancedPlayerPrintToChat(player, commandInfo, MainPlugin.Instance.Localizer["PrintToChatToPlayer.Give.ESP.Check.Console"]);

                            Helper.AdvancedPlayerPrintToConsole(player, MainPlugin.Instance.Localizer["PrintToConsoleToPlayer.Give.ESP.List.Header"], withData.Count);
                            foreach (var target in withData)
                            {
                                MainPlugin.Instance.g_Main.Player_Data.TryGetValue(target.Slot, out var tData);
                                var Gived_ESP_Localizer = tData!.Gived_ESP ? MainPlugin.Instance.Localizer["PrintToConsoleToPlayer.Give.ESP.List.Enabled"] : MainPlugin.Instance.Localizer["PrintToConsoleToPlayer.Give.ESP.List.Disabled"];
                                Helper.AdvancedPlayerPrintToConsole(player, Gived_ESP_Localizer, target.PlayerName);
                            }
                        }
                    }
                    else
                    {
                        bool newValue = valueArg.Equals("1") || valueArg.Equals("true", StringComparison.OrdinalIgnoreCase);
                        var changedPlayers = new List<CCSPlayerController>();

                        foreach (var target in targets)
                        {
                            if (!MainPlugin.Instance.g_Main.Player_Data.TryGetValue(target.Slot, out var tData)) continue;

                            tData.Gived_ESP = newValue;
                            changedPlayers.Add(target);

                            if (!target.IsBot && target.Slot != player.Slot)
                            {
                                Helper.AdvancedPlayerPrintToChat(target, null!, MainPlugin.Instance.Localizer["PrintToChatToPlayer.Give.ESP.Received"], player.PlayerName, StateWord(newValue));
                            }
                        }

                        if (changedPlayers.Count == 0)
                        {
                            Helper.AdvancedPlayerPrintToChat(player, commandInfo, MainPlugin.Instance.Localizer["PrintToChatToPlayer.Give.ESP.NoTarget"], targetQuery);
                        }
                        else if (changedPlayers.Count <= 5)
                        {
                            foreach (var target in changedPlayers)
                            {
                                Helper.AdvancedPlayerPrintToChat(player, commandInfo, MainPlugin.Instance.Localizer["PrintToChatToPlayer.Give.ESP.Set"], target.PlayerName, StateWord(newValue));
                            }
                        }
                        else
                        {
                            Helper.AdvancedPlayerPrintToChat(player, commandInfo, MainPlugin.Instance.Localizer["PrintToChatToPlayer.Give.ESP.Check.Console"]);

                            Helper.AdvancedPlayerPrintToConsole(player, MainPlugin.Instance.Localizer["PrintToConsoleToPlayer.Give.ESP.List.Header"], changedPlayers.Count);
                            foreach (var target in changedPlayers)
                            {
                                var newValue_Localizer = newValue ? MainPlugin.Instance.Localizer["PrintToConsoleToPlayer.Give.ESP.List.Enabled"] : MainPlugin.Instance.Localizer["PrintToConsoleToPlayer.Give.ESP.List.Disabled"];
                                Helper.AdvancedPlayerPrintToConsole(player, newValue_Localizer, target.PlayerName);
                            }
                        }
                    }
                }
            }

            Helper.MuteCommands(um, cfg.Give_ESP_Hide);
        }

        Helper.MuteCommands(um, cfg.Give_ESP_Hide, true);
    }

    public static void Handle_CommandsAction_Toggle_ESP(CCSPlayerController player, CommandInfo commandInfo = null!, UserMessage um = null!)
    {
        if (MainPlugin.Instance._prefs == null || !MainPlugin.Instance._prefs.TryGetValue(player.Slot, out var prefs) || !MainPlugin.Instance.g_Main.Player_Data.TryGetValue(player.Slot, out var playerData)) return;

        bool onetime = (DateTime.Now - playerData.EventPlayerChat).TotalSeconds > 0.4;
        if (onetime) playerData.EventPlayerChat = DateTime.Now;


        var cfg = Configs.Instance.Toggle_ESP;

        if (cfg.Toggle_ESP_Flags.HasValidPermissionData() && !Helper.IsPlayerInGroupPermission(player, cfg.Toggle_ESP_Flags))
        {
            if (onetime)
            {
                Helper.AdvancedPlayerPrintToChat(player, commandInfo, MainPlugin.Instance.Localizer["PrintToChatToPlayer.Toggle.ESP.Not.Allowed"]);
            }
        }else
        {
            if (onetime)
            {
                prefs.Toggle_ESP = !prefs.Toggle_ESP;
                if(prefs.Toggle_ESP)
                {
                    Helper.AdvancedPlayerPrintToChat(player, commandInfo, MainPlugin.Instance.Localizer["PrintToChatToPlayer.Toggle.ESP.Enabled"]);
                }else if(!prefs.Toggle_ESP)
                {
                    Helper.AdvancedPlayerPrintToChat(player, commandInfo, MainPlugin.Instance.Localizer["PrintToChatToPlayer.Toggle.ESP.Disabled"]);
                }
            }

            Helper.MuteCommands(um, cfg.Toggle_ESP_Hide);
        }

        Helper.MuteCommands(um, cfg.Toggle_ESP_Hide, true);
    }

    #endregion Handles
}