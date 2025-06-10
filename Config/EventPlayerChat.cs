using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using ESP_Players.Config;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Core.Capabilities;

namespace ESP_Players;

public class SayText2
{
    public HookResult OnSayText2(CounterStrikeSharp.API.Modules.UserMessages.UserMessage? um, CCSPlayerController? player, string message, bool TeamChat)
    {
        if (!player.IsValid()) return HookResult.Continue;

        if (!MainPlugin.Instance.g_Main.Player_Data.TryGetValue(player, out var playerData)) return HookResult.Continue;

        bool onetime = (DateTime.Now - playerData.EventPlayerChat).TotalSeconds > 0.1;
        if (onetime)
        {
           playerData.EventPlayerChat = DateTime.Now; 
        }


        if (Configs.GetConfigData().Toggle_Glow_CommandsInGame.GetCommands(true).Any(command => message.Equals(command.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            if (!string.IsNullOrEmpty(Configs.GetConfigData().Toggle_Glow_Flags) && !Helper.IsPlayerInGroupPermission(player, Configs.GetConfigData().Toggle_Glow_Flags))
            {
                if (!message.StartsWith("!") && onetime) Helper.AdvancedPlayerPrintToChat(player, null!, MainPlugin.Instance.Localizer["PrintChatToPlayer.Toggle.Not.Allowed"]);
                if (Configs.GetConfigData().Toggle_Glow_Hide == 2)
                {
                    if (um != null) um.Recipients.Clear();
                }
            }
            else
            {
                if (!message.StartsWith("!") && onetime)
                {
                    playerData.Toggle_ESP = playerData.Toggle_ESP.ToggleOnOff();
                    if (playerData.Toggle_ESP == -1)
                    {
                        Helper.AdvancedPlayerPrintToChat(player, null!, MainPlugin.Instance.Localizer["PrintChatToPlayer.Toggle.Enabled"]);
                    }
                    else if (playerData.Toggle_ESP == -2)
                    {
                        Helper.AdvancedPlayerPrintToChat(player, null!, MainPlugin.Instance.Localizer["PrintChatToPlayer.Toggle.Disabled"]);
                    }
                }
                if (Configs.GetConfigData().Toggle_Glow_Hide > 0)
                {
                    if (um != null) um.Recipients.Clear();
                }
            }
        }

        return HookResult.Continue;
    }
    

    public void CommandsAction_ESP(CCSPlayerController? player, CommandInfo info)
    {
        if (!player.IsValid()) return;

        Helper.CheckPlayerInGlobals(player);

        if (!MainPlugin.Instance.g_Main.Player_Data.TryGetValue(player, out var playerData)) return;


        if (!string.IsNullOrEmpty(Configs.GetConfigData().Toggle_Glow_Flags) && !Helper.IsPlayerInGroupPermission(player, Configs.GetConfigData().Toggle_Glow_Flags))
        {
            Helper.AdvancedPlayerPrintToChat(player, info, MainPlugin.Instance.Localizer["PrintChatToPlayer.Toggle.Not.Allowed"]);
        }
        else
        {
            playerData.Toggle_ESP = playerData.Toggle_ESP.ToggleOnOff();
            if (playerData.Toggle_ESP == -1)
            {
                Helper.AdvancedPlayerPrintToChat(player, info, MainPlugin.Instance.Localizer["PrintChatToPlayer.Toggle.Enabled"]);
            }
            else if (playerData.Toggle_ESP == -2)
            {
                Helper.AdvancedPlayerPrintToChat(player, info, MainPlugin.Instance.Localizer["PrintChatToPlayer.Toggle.Disabled"]);
            }
        }
    }
}