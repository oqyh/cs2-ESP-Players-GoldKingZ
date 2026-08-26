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
using ESP_Players_GoldKingZ.Config;
using System.Drawing;
using System;
using ClientPrefs_GoldKingZ.Shared;
using System.Text;
using CounterStrikeSharp.API.Core.Capabilities;

namespace ESP_Players_GoldKingZ;

public sealed class ClientPrefs
{
    public bool Toggle_ESP { get; set; } = Configs.Instance.Toggle_ESP.Default_Toggle_ESP;
}

public class MainPlugin : BasePlugin
{
    public override string ModuleName => "Show Glow/Esp To Players With Flags";
    public override string ModuleVersion => "1.0.4";
    public override string ModuleAuthor => "Gold KingZ";
    public override string ModuleDescription => "https://github.com/oqyh";
    public static MainPlugin Instance { get; set; } = new();
    public Globals g_Main = new();
    public readonly Game_UserMessages Game_UserMessages = new();
    public IPrefsStore<ClientPrefs>? _prefs;
    public ESPPlayersApiImpl _api = null!;
    public FakeConVar<bool> g_EnablePlugin = new("gkz_esp", "ESP Plugin [true = Enable / false = Disable]", true);

    public override void Load(bool hotReload)
    {
        Instance = this;
        RegisterFakeConVars(typeof(ConVar));
        Configs.Load(ModuleDirectory, hotReload);

        _api = new ESPPlayersApiImpl();
        Capabilities.RegisterPluginCapability(Shared.ESPPlayersApi.Capability, () => _api);

        Helper.RemoveRegisterCommandsAndHooks();
        Helper.ClearVariables();
        Helper.RegisterCommandsAndHooks();

        g_EnablePlugin.ValueChanged += (sender, value) =>
        {
            if (value)
            {
                Helper.RegisterCommandsAndHooks();
                Helper.ReloadPlayersGlobals();
                Helper.DebugMessage($"{Con.Green}Plugin Has Been Enabled By ConVar [{Con.Purple}{g_EnablePlugin.Name}{Con.Green}] Set To {Con.Purple}{g_EnablePlugin.Value}", true);
            }
            else
            {
                Helper.RemoveRegisterCommandsAndHooks();
                Helper.ClearVariables();
                Helper.DebugMessage($"{Con.LightRed}Plugin Has Been Disabled By ConVar [{Con.Purple}{g_EnablePlugin.Name}{Con.LightRed}] Set To {Con.Purple}{g_EnablePlugin.Value}", true);
            }
        };

        if (hotReload)
        {
            Helper.RemoveRegisterCommandsAndHooks();
            Helper.ClearVariables();
            Helper.RegisterCommandsAndHooks();
            Helper.ReloadPlayersGlobals();
        }
    }

    public override void OnAllPluginsLoaded(bool hotReload)
    {
        var api = ClientPrefsApi.Get();
        if (api == null)
        {
            Helper.DebugMessage("Missing ClientPrefs-GoldKingZ API!", true);
        }else
        {
            _prefs = api.CreatePrefs<ClientPrefs>(this, new ClientPrefsOptions
            {
                PrefsAPI_CookiesEnable = (PrefsAPI_SaveMode)Configs.Instance.Cookies_Enable,
                PrefsAPI_CookiesAutoRemoveInactivePlayersOlderThanDays = Configs.Instance.Cookies_AutoRemoveInactivePlayersOlderThanDays,

                PrefsAPI_MySqlEnable = (PrefsAPI_SaveMode)Configs.Instance.MySql_Enable,
                PrefsAPI_MySqlAutoRemoveInactivePlayersOlderThanDays = Configs.Instance.MySql_AutoRemoveInactivePlayersOlderThanDays,
                PrefsAPI_MySqlConnectionTimeout = Configs.Instance.MySql_ConnectionTimeout,
                PrefsAPI_MySqlRetryAttempts = Configs.Instance.MySql_RetryAttempts,
                PrefsAPI_MySqlRetryDelay = Configs.Instance.MySql_RetryDelay,
                PrefsAPI_MySqlConfig = new ClientPrefs_GoldKingZ.Shared.MySqlConfig
                {
                    MySql_Servers = Configs.Instance.MySql_Config.MySql_Servers
                        .Select(s => new ClientPrefs_GoldKingZ.Shared.MySqlServer
                        {
                            Server   = s.Server,
                            Port     = s.Port,
                            Database = s.Database,
                            Username = s.Username,
                            Password = s.Password,
                        }).ToList()
                },
                PrefsAPI_DebugEnable = Configs.Instance.EnableDebug
            });
        }

        if (hotReload)
        {
            _prefs?.Refresh();
        }
    }

    public void OnMapStart(string mapname)
    {
        Helper.StartTimer();
    }

    public HookResult OnEventRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        if (@event == null) return HookResult.Continue;

        Helper.StartTimer();

        return HookResult.Continue;
    }
    
    public void OnClientPutInServer(int playerSlot)
    {
        var player = Utilities.GetPlayerFromSlot(playerSlot);
        if(player == null || !player.IsValid)return;

        Helper.CheckPlayerInGlobals(player);
    }

    public HookResult OnEventPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        if (@event == null) return HookResult.Continue;

        var player = @event.Userid;
        if(player == null || !player.IsValid) return HookResult.Continue;

        Helper.RemoveGlow(player);
        
        return HookResult.Continue;
    }

    public void OnCheckTransmit(CCheckTransmitInfoList infoList)
    {
        bool onlyEnemy = Configs.Instance.ShowOnlyEnemyTeam && (Configs.Instance.Show_ESP_For == 0 || Configs.Instance.Show_ESP_For == 1);

        foreach ((CCheckTransmitInfo info, CCSPlayerController? player) in infoList)
        {
            if (player == null || !player.IsValid) continue;

            bool shouldRemoveGlow = false;

            if (_prefs == null)
            {
                shouldRemoveGlow = true;
            }
            else if (player.IsHLTV)
            {
                if (Configs.Instance.DisableGlowOnGOTV)
                {
                    shouldRemoveGlow = true;
                }
            }
            else
            {
                bool hasToggle = _prefs!.TryGetValue(player.Slot, out var prefs) && prefs.Toggle_ESP;
                bool hasGivenESP = g_Main.Player_Data.TryGetValue(player.Slot, out var viewerData) && viewerData.Gived_ESP;

                if (!hasToggle && !hasGivenESP)
                {
                    shouldRemoveGlow = true;
                }
            }

            if (!shouldRemoveGlow && Configs.Instance.Show_ESP_For == 1 && player.IsAlive(true))
            {
                shouldRemoveGlow = true;
            }

            if (!shouldRemoveGlow && Configs.Instance.Show_ESP_For == 2 && player.TeamNum > (int)CsTeam.Spectator)
            {
                shouldRemoveGlow = true;
            }

            int viewerSlot = player.Slot;
            byte viewerTeam = player.TeamNum;

            foreach (var kv in g_Main.Player_Data)
            {
                var data = kv.Value;
                if (data == null) continue;

                var ModelGlow = data.ModelGlow;
                var ModelRelay = data.ModelRelay;

                bool hasGlow = ModelGlow != null && ModelGlow.IsValid;
                bool hasRelay = ModelRelay != null && ModelRelay.IsValid;
                if (!hasGlow && !hasRelay) continue;

                bool remove = shouldRemoveGlow;

                if (!remove)
                {
                    if (kv.Key == viewerSlot)
                    {
                        remove = true;
                    }
                    else
                    {
                        var targetPlayer = data.Player;
                        if (targetPlayer == null || !targetPlayer.IsValid)
                        {
                            remove = true;
                        }
                        else if (onlyEnemy && viewerTeam == targetPlayer.TeamNum)
                        {
                            remove = true;
                        }
                    }
                }

                if (!remove) continue;

                if (hasGlow) info.TransmitEntities.Remove(ModelGlow!);
                if (hasRelay) info.TransmitEntities.Remove(ModelRelay!);
            }
        }
    }
    
    public HookResult OnPlayerSay(CCSPlayerController? player, CommandInfo info)
    {
        return HandlePlayerMessage(player, info.ArgString.Trim('"'));
    }

    public HookResult OnPlayerSay_Team(CCSPlayerController? player, CommandInfo info)
    {
        return HandlePlayerMessage(player, info.ArgString.Trim('"'));
    }

    public HookResult OnUserMessage_OnSayText2(CounterStrikeSharp.API.Modules.UserMessages.UserMessage um)
    {
        var player = Utilities.GetPlayerFromIndex(um.ReadInt("entityindex"));
        return HandlePlayerMessage(player, Encoding.UTF8.GetString(um.ReadBytes("param2")), um);
    }

    private HookResult HandlePlayerMessage(CCSPlayerController? player, string? rawMessage, CounterStrikeSharp.API.Modules.UserMessages.UserMessage? um = null)
    {
        if (player == null || !player.IsValid || string.IsNullOrWhiteSpace(rawMessage)) return HookResult.Continue;

        string message = rawMessage.Trim();
        Game_UserMessages.HookPlayerChat_UserMessages(player, message, um);

        return HookResult.Continue;
    }

    public HookResult OnEventPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        if (@event == null) return HookResult.Continue;

        var player = @event.Userid;
        if(player == null || !player.IsValid) return HookResult.Continue;

        Helper.RemoveGlow(player);

        if(!Configs.Instance.Give_ESP.Give_ESP_SaveOnDisconnect)
        {
            if (g_Main.Player_Data.ContainsKey(player.Slot))
            {
                g_Main.Player_Data.Remove(player.Slot);
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
            Helper.DebugMessage($"OnMapEnd Error: {ex.Message}", true);
        }
    }

    public override void Unload(bool hotReload)
    {
        try
        {
            _prefs?.Unload();
            Helper.RemoveRegisterCommandsAndHooks();
            Helper.ClearVariables();
        }
        catch (Exception ex)
        {
            Helper.DebugMessage($"Unload Error: {ex.Message}", true);
        }

        if (hotReload)
        {
            try
            {
                Helper.RemoveRegisterCommandsAndHooks();
                Helper.ClearVariables();
            }
            catch (Exception ex)
            {
                Helper.DebugMessage($"Unload hotReload Error: {ex.Message}", true);
            }
        }
    }

    /* [ConsoleCommand("css_test", "testttt")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void test(CCSPlayerController? player, CommandInfo commandInfo)
    {
        if (player == null || !player.IsValid) return;
        
    } */
    
}