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
    public override string ModuleVersion => "1.0.1";
    public override string ModuleAuthor => "Gold KingZ";
    public override string ModuleDescription => "https://github.com/oqyh";
    public static MainPlugin Instance { get; set; } = new();
    public Globals g_Main = new();
    private readonly SayText2 OnSayText2 = new();

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
        RegisterListener<Listeners.OnMapStart>(OnMapStart);
        RegisterListener<Listeners.OnMapEnd>(OnMapEnd);

        HookUserMessage(118, OnUserMessage_OnSayText2, HookMode.Pre);

        AddCommandListener("say", OnPlayerSay, HookMode.Post);
        AddCommandListener("say_team", OnPlayerSay_Team, HookMode.Post);

        Helper.RegisterCssCommands(Configs.GetConfigData().Toggle_Glow_CommandsInGame.GetCommands(), "Commands To Enable/Disable Glow", OnSayText2.CommandsAction_ESP);

        

        if (hotReload)
        {
            if (Configs.GetConfigData().MySql_Enable)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await MySqlDataManager.CreateTableIfNotExistsAsync();
                    }
                    catch (Exception ex)
                    {
                        Helper.DebugMessage($"hotReload error: {ex.Message}");
                    }
                });
            }
        }
    }

    public void OnMapStart(string mapname)
    {
        if (Configs.GetConfigData().MySql_Enable)
        {
            _ = Task.Run(async () =>
            {
                try
                {                        
                    await MySqlDataManager.CreateTableIfNotExistsAsync();
                }
                catch (Exception ex)
                {
                    Helper.DebugMessage($"OnMapStart error: {ex.Message}");
                }
            });
        }
    }

    private void OnCheckTransmit(CCheckTransmitInfoList infoList)
    {
        foreach ((CCheckTransmitInfo info, CCSPlayerController? player) in infoList)
        {
            if (!player.IsValid(true, Configs.GetConfigData().DisableGlowOnGOTV)) continue;

            Globals.PlayerDataClass? handle = null;
            if (!player.IsHLTV && !g_Main.Player_Data.TryGetValue(player, out handle)) continue;

            foreach (var getplayers in g_Main.Player_Data.Values)
            {
                if (getplayers == null) continue;

                var targetPlayer = getplayers.Player;
                if (!targetPlayer.IsValid(true)) continue;

                var ModelGlow = getplayers.ModelGlow;
                var ModelRelay = getplayers.ModelRelay;

                bool shouldRemoveGlow = false;
                if (Configs.GetConfigData().DisableGlowOnGOTV && player.IsHLTV)
                {
                    shouldRemoveGlow = true;
                }
                
                if (Configs.GetConfigData().Show_ESP_For == 1)
                {
                    if (player.IsAlive())
                    {
                        shouldRemoveGlow = true;
                    }
                }
                else if (Configs.GetConfigData().Show_ESP_For == 2)
                {
                    if (player.TeamNum != (int)CsTeam.Spectator)
                    {
                        shouldRemoveGlow = true;
                    }
                }
                
                if (Configs.GetConfigData().ShowOnlyEnemyTeam && (Configs.GetConfigData().Show_ESP_For == 0 || Configs.GetConfigData().Show_ESP_For == 1))
                {
                    if (player.TeamNum == targetPlayer.TeamNum)
                    {
                        shouldRemoveGlow = true;
                    }
                }

                if (ModelGlow != null && ModelGlow.IsValid)
                {
                    if (shouldRemoveGlow || handle != null && handle.Toggle_ESP == 2 || handle != null && handle.Toggle_ESP == -2)
                    {
                        info.TransmitEntities.Remove(ModelGlow);
                    }
                }

                if (ModelRelay != null && ModelRelay.IsValid)
                {
                    if (shouldRemoveGlow || handle != null && handle.Toggle_ESP == 2 || handle != null && handle.Toggle_ESP == -2)
                    {
                        info.TransmitEntities.Remove(ModelRelay);
                    }
                }
            }
        }
    }

    private HookResult OnPlayerSay(CCSPlayerController? player, CommandInfo info)
    {
        if (!player.IsValid()) return HookResult.Continue;
        Helper.CheckPlayerInGlobals(player);

        var eventmessage = info.ArgString;
        eventmessage = eventmessage.Trim().Trim('"').Trim();

        if (string.IsNullOrWhiteSpace(eventmessage)) return HookResult.Continue;

        OnSayText2.OnSayText2(null, player, eventmessage, false);

        return HookResult.Continue;
    }
    private HookResult OnPlayerSay_Team(CCSPlayerController? player, CommandInfo info)
    {
        if (!player.IsValid()) return HookResult.Continue;
        Helper.CheckPlayerInGlobals(player);

        var eventmessage = info.ArgString;
        eventmessage = eventmessage.Trim().Trim('"').Trim();
    
        if (string.IsNullOrWhiteSpace(eventmessage)) return HookResult.Continue;

        OnSayText2.OnSayText2(null, player, eventmessage, true);

        return HookResult.Continue;
    }
    private HookResult OnUserMessage_OnSayText2(CounterStrikeSharp.API.Modules.UserMessages.UserMessage um)
    {
        var entityindex = um.ReadInt("entityindex");
        var player = Utilities.GetPlayerFromIndex(entityindex);
        if (!player.IsValid()) return HookResult.Continue;
        Helper.CheckPlayerInGlobals(player);

        var messagename = um.ReadString("messagename");
        var message = um.ReadString("param2");
        message = message.Trim();
        if (string.IsNullOrWhiteSpace(message)) return HookResult.Continue;

        bool TeamChat = false;
        if (messagename.Equals("Cstrike_Chat_CT") || messagename.Equals("Cstrike_Chat_CT_Loc") || messagename.Equals("Cstrike_Chat_T") || messagename.Equals("Cstrike_Chat_T_Loc")
        || messagename.Equals("Cstrike_Chat_Spec") || messagename.Equals("Cstrike_Chat_CT_Dead") || messagename.Equals("Cstrike_Chat_T_Dead"))
        {
            TeamChat = true;
        }

        OnSayText2.OnSayText2(um, player, message, TeamChat);
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
            Helper.DebugMessage($"OnMapEnd error: {ex.Message}");
        }
    }

    public override void Unload(bool hotReload)
    {
        Helper.RemoveCssCommands(Configs.GetConfigData().Toggle_Glow_CommandsInGame.GetCommands(), OnSayText2.CommandsAction_ESP);

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
        if (!player.IsValid()) return;
    } */
    
}