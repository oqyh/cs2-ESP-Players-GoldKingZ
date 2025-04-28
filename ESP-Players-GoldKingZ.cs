using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Events;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Utils;
using ESP_Players.Config;
using System.Drawing;
using System;

namespace ESP_Players;

public class MainPlugin : BasePlugin
{
    public override string ModuleName => "Show Glow/Esp To Players With Flags";
    public override string ModuleVersion => "1.0.0";
    public override string ModuleAuthor => "Gold KingZ";
    public override string ModuleDescription => "https://github.com/oqyh";
    public static MainPlugin Instance { get; set; } = new();
    public Globals g_Main = new();
    private readonly PlayerChat _PlayerChat = new();

    public override void Load(bool hotReload)
    {
        Instance = this;
        Configs.Load(ModuleDirectory);

        RegisterEventHandler<EventPlayerConnectFull>(OnEventPlayerConnectFull);
        RegisterEventHandler<EventPlayerSpawn>(OnEventPlayerSpawn, HookMode.Post);
        RegisterEventHandler<EventPlayerDeath>(OnEventPlayerDeath);
        RegisterEventHandler<EventBotTakeover>(OnEventBotTakeover);
        RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);

        RegisterListener<Listeners.CheckTransmit>(OnCheckTransmit);
        RegisterListener<Listeners.OnMapEnd>(OnMapEnd);

        AddCommandListener("say", OnPlayerChat, HookMode.Post);
        AddCommandListener("say_team", OnPlayerChatTeam, HookMode.Post);

        string[] Glow_CommandsInGames = Configs.GetConfigData().Glow_CommandsInGame.Split(',');
        foreach (var cmd in Glow_CommandsInGames.Where(cmd => cmd.StartsWith("css_", StringComparison.OrdinalIgnoreCase)))
        {
            AddCommand(cmd, "full update test", CommandsAction);
        }
    }


    public void CommandsAction(CCSPlayerController? player, CommandInfo info)
    {
        if (!player.IsValid()) return;

        if (!string.IsNullOrEmpty(Configs.GetConfigData().Glow_Flags) && !Helper.IsPlayerInGroupPermission(player, Configs.GetConfigData().Glow_Flags))
        {
            Helper.AdvancedPlayerPrintToChat(player, info, Localizer["PrintChatToPlayer.Toggle.Not.Allowed"]);
        }
        else
        {
            if (g_Main.Player_Data.ContainsKey(player))
            {
                g_Main.Player_Data[player].Toggle_ESP = g_Main.Player_Data[player].Toggle_ESP.ToggleOnOff();

                if (g_Main.Player_Data[player].Toggle_ESP == -1)
                {
                    Helper.AdvancedPlayerPrintToChat(player, info, Localizer["PrintChatToPlayer.Toggle.Enabled"]);
                }
                else if (g_Main.Player_Data[player].Toggle_ESP == -2)
                {
                    Helper.AdvancedPlayerPrintToChat(player, info, Localizer["PrintChatToPlayer.Toggle.Disabled"]);
                }
            }
        }
    }

    private void OnCheckTransmit(CCheckTransmitInfoList infoList)
    {
        foreach ((CCheckTransmitInfo info, CCSPlayerController? player) in infoList)
        {
            if (!player.IsValid(true)) continue;

            if (!g_Main.Player_Data.TryGetValue(player, out var handle)) continue;

            foreach (var getplayers in g_Main.Player_Data.Values)
            {
                if (getplayers == null) continue;

                var targetPlayer = getplayers.Player;
                if (!targetPlayer.IsValid(true)) continue;

                var ModelGlow = getplayers.ModelGlow;
                var ModelRelay = getplayers.ModelRelay;

                bool shouldRemoveGlow = false;

                if (Configs.GetConfigData().ShowOnlyEnemyTeam)
                {
                    
                    if (player.TeamNum == targetPlayer.TeamNum)
                    {
                        shouldRemoveGlow = true;
                    }
                }

                if (ModelGlow != null && ModelGlow.IsValid)
                {
                    if (shouldRemoveGlow || handle.Toggle_ESP == 2 || handle.Toggle_ESP == -2)
                    {
                        info.TransmitEntities.Remove(ModelGlow);
                    }
                }

                if (ModelRelay != null && ModelRelay.IsValid)
                {
                    if (shouldRemoveGlow || handle.Toggle_ESP == 2 || handle.Toggle_ESP == -2)
                    {
                        info.TransmitEntities.Remove(ModelRelay);
                    }
                }
                
            }
        }
    }

    private HookResult OnPlayerChat(CCSPlayerController? player, CommandInfo info)
	{
        if (!player.IsValid())return HookResult.Continue;

        _PlayerChat.OnPlayerChat(player, info, false);

        return HookResult.Continue;
    }
    private HookResult OnPlayerChatTeam(CCSPlayerController? player, CommandInfo info)
	{
        if (!player.IsValid())return HookResult.Continue;

        _PlayerChat.OnPlayerChat(player, info, true);

        return HookResult.Continue;
    }

    

    public HookResult OnEventBotTakeover(EventBotTakeover @event, GameEventInfo info)
    {
        if (@event == null)return HookResult.Continue;

        var player = @event.Userid;
        if (player == null || !player.IsValid) return HookResult.Continue;

        CCSPlayerController? takenOverBot = null;

        foreach (var allplayers in Utilities.GetPlayers())
        {
            if (allplayers == null || !allplayers.IsValid) continue;

            if (player == allplayers.OriginalControllerOfCurrentPawn.Value)
            {
                takenOverBot = allplayers;
                break;
            }
        }

        if (takenOverBot != null && takenOverBot.IsValid)
        {
            if (g_Main.Player_Data.ContainsKey(player))
            {
                g_Main.Player_Data[player].Player_Controlled_Bot = takenOverBot;
            }
        }
        return HookResult.Continue;
    }

    public HookResult OnEventPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
    {
        if (@event == null)return HookResult.Continue;

        var player = @event.Userid;
        if (!player.IsValid())return HookResult.Continue;

        _ = HandlePlayerConnectionsAsync(player);

        return HookResult.Continue;
    }
    private async Task HandlePlayerConnectionsAsync(CCSPlayerController Getplayer)
    {
        try
        {
            var player = Getplayer;
            if (!player.IsValid()) return;

            await Helper.LoadPlayerData(player);
        }
        catch (Exception ex)
        {
            Helper.DebugMessage($"HandlePlayerConnectionsAsync error: {ex.Message}");
        }
    }

    private HookResult OnEventPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
    {
        if(@event == null || Configs.GetConfigData().DisableOnWarmUp && Helper.IsWarmup())return HookResult.Continue;

        var player = @event.Userid;
        if (!player.IsValid(true)) return HookResult.Continue;

        Helper.CheckPlayerInGlobals(player);

        if(Configs.GetConfigData().UserTimerCheckPlayersGlow)
        {
            if(g_Main.Timer == null)
            {
                g_Main.Timer = AddTimer(1.0f, () => ESP_Timer(), TimerFlags.REPEAT | TimerFlags.STOP_ON_MAPCHANGE);
            }
        }else
        {
            Helper.SetGlowPlayer(player);
        }
        
        

        return HookResult.Continue;
    }
    
    public static void ESP_Timer()
    {
        foreach(var players in Helper.GetPlayersController(true,false,false,false,true,true))
        {
            if(Configs.GetConfigData().DisableOnWarmUp && Helper.IsWarmup() || !players.IsValid(true) || !players.IsAlive())continue;

            Helper.SetGlowPlayer(players);
        }
    }
    
    private HookResult OnEventPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        if (@event == null) return HookResult.Continue;

        var player = @event.Userid;
        if (player.IsValid(true))
        {
            if (g_Main.Player_Data.TryGetValue(player, out var playerData))
            {
                if (playerData.ModelGlow != null && playerData.ModelGlow.IsValid)
                {
                    playerData.ModelGlow.Remove();
                }
                if (playerData.ModelRelay != null && playerData.ModelRelay.IsValid)
                {
                    playerData.ModelRelay.Remove();
                }

                if (playerData.Player_Controlled_Bot != null && playerData.Player_Controlled_Bot.IsValid)
                {
                    var bot = playerData.Player_Controlled_Bot;

                    if (g_Main.Player_Data.TryGetValue(bot, out var botData))
                    {
                        if (botData.ModelGlow != null && botData.ModelGlow.IsValid)
                        {
                            botData.ModelGlow.Remove();
                        }
                        if (botData.ModelRelay != null && botData.ModelRelay.IsValid)
                        {
                            botData.ModelRelay.Remove();
                        }
                    }
                }
            }
        }
        return HookResult.Continue;
    }

    public HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        if (@event == null) return HookResult.Continue;

        var player = @event.Userid;
        if (!player.IsValid()) return HookResult.Continue;

        if (g_Main.Player_Data.ContainsKey(player))
        {
            var ModelGlow = g_Main.Player_Data[player].ModelGlow;
            var ModelRelay = g_Main.Player_Data[player].ModelRelay;

            if(ModelGlow != null && ModelGlow.IsValid)
            {
                ModelGlow.Remove();
            }

            if(ModelRelay != null && ModelRelay.IsValid)
            {
                ModelRelay.Remove();
            }

            g_Main.Player_Data.Remove(player);
        }

        return HookResult.Continue;
    }
    
    public void OnMapEnd()
    {
        try
        {
            Helper.ClearVariables();
        }
        catch (Exception ex)
        {
            Helper.DebugMessage($"Map end cleanup error: {ex.Message}");
        }
    }

    public override void Unload(bool hotReload)
    {
        string[] Glow_CommandsInGames = Configs.GetConfigData().Glow_CommandsInGame.Split(',');
        foreach (var cmd in Glow_CommandsInGames.Where(cmd => cmd.StartsWith("css_", StringComparison.OrdinalIgnoreCase)))
        {
            RemoveCommand(cmd, CommandsAction);
        }

        try
        {
            Helper.ClearVariables();
        }
        catch (Exception ex)
        {
            Helper.DebugMessage($"Unload cleanup error: {ex.Message}");
        }
    }

    

    /* [ConsoleCommand("css_test", "test")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void test(CCSPlayerController? player, CommandInfo commandInfo)
    {
        if (!player.IsValid())return;
    } */
    
}