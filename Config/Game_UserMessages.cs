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
                    Configs.Load(MainPlugin.Instance.ModuleDirectory);
                    Helper.RegisterCommandsAndHooks();
                    Helper.ReloadPlayersGlobals();
                });

                Helper.AdvancedPlayerPrintToChat(player, commandInfo, MainPlugin.Instance.Localizer["PrintToChatToPlayer.ReloadPlugin.Successfully"]);
            }

            Helper.MuteCommands(um, cfg.Reload_Plugin_Hide);
        }

        Helper.MuteCommands(um, cfg.Reload_Plugin_Hide, true);
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
                Helper.AdvancedPlayerPrintToChat(player, commandInfo, MainPlugin.Instance.Localizer["PrintToChatToPlayer.Toggle.Not.Allowed"]);
            }
        }else
        {
            if (onetime)
            {
                prefs.Toggle_ESP = !prefs.Toggle_ESP;
                if(prefs.Toggle_ESP)
                {
                    Helper.AdvancedPlayerPrintToChat(player, commandInfo, MainPlugin.Instance.Localizer["PrintToChatToPlayer.Toggle.Enabled"]);
                }else if(!prefs.Toggle_ESP)
                {
                    Helper.AdvancedPlayerPrintToChat(player, commandInfo, MainPlugin.Instance.Localizer["PrintToChatToPlayer.Toggle.Disabled"]);
                }
            }

            Helper.MuteCommands(um, cfg.Toggle_ESP_Hide);
        }

        Helper.MuteCommands(um, cfg.Toggle_ESP_Hide, true);
    }

    #endregion Handles
}