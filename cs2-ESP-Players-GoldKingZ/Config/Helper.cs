using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Utils;
using System.Text.RegularExpressions;
using ESP_Players_GoldKingZ.Config;
using System.Drawing;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Admin;
using System.Security.Cryptography;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Entities;
using Microsoft.VisualBasic;
using CounterStrikeSharp.API.Modules.Entities.Constants;

namespace ESP_Players_GoldKingZ;

public class Helper
{
    public static void RegisterCssCommands(string[]? commands, string description, CommandInfo.CommandCallback callback)
    {
        if (commands == null || commands.Length == 0) return;

        foreach (var cmd in commands)
        {
            if (string.IsNullOrEmpty(cmd)) continue;
            MainPlugin.Instance.AddCommand(cmd, description, callback);
        }
    }


    public static void RemoveCssCommands(string[]? commands, CommandInfo.CommandCallback callback)
    {
        if (commands == null || commands.Length == 0) return;

        foreach (var cmd in commands)
        {
            if (string.IsNullOrEmpty(cmd)) continue;
            MainPlugin.Instance.RemoveCommand(cmd, callback);
        }
    }

    public static void RegisterCssListener(string[]? commands, CommandInfo.CommandListenerCallback callback)
    {
        if (commands == null || commands.Length == 0) return;

        foreach (var cmd in commands)
        {
            if (string.IsNullOrEmpty(cmd)) continue;
            MainPlugin.Instance.AddCommandListener(cmd, callback, HookMode.Pre);
        }
    }

    public static void RemoveCssListener(string[]? commands, CommandInfo.CommandListenerCallback callback)
    {
        if (commands == null || commands.Length == 0) return;

        foreach (var cmd in commands)
        {
            if (string.IsNullOrEmpty(cmd)) continue;
            MainPlugin.Instance.RemoveCommandListener(cmd, callback, HookMode.Pre);
        }
    }

    public static void AdvancedPlayerPrintToChat(CCSPlayerController player, CounterStrikeSharp.API.Modules.Commands.CommandInfo commandInfo, string message, params object[] args)
    {
        if (string.IsNullOrEmpty(message)) return;

        for (int i = 0; i < args.Length; i++)
        {
            message = message.Replace($"{{{i}}}", args[i]?.ToString() ?? "");
        }

        if (Regex.IsMatch(message, "{nextline}", RegexOptions.IgnoreCase))
        {
            string[] parts = Regex.Split(message, "{nextline}", RegexOptions.IgnoreCase);
            foreach (string part in parts)
            {
                string trimmedPart = part.Trim();
                trimmedPart = trimmedPart.ReplaceColorTags();
                if (!string.IsNullOrEmpty(trimmedPart))
                {
                    if (commandInfo != null && commandInfo.CallingContext == CounterStrikeSharp.API.Modules.Commands.CommandCallingContext.Console)
                    {
                        player.PrintToConsole(" " + trimmedPart);
                    }
                    else
                    {
                        player.PrintToChat(" " + trimmedPart);
                    }
                }
            }
        }
        else
        {
            message = message.ReplaceColorTags();
            if (commandInfo != null && commandInfo.CallingContext == CounterStrikeSharp.API.Modules.Commands.CommandCallingContext.Console)
            {
                player.PrintToConsole(message);
            }
            else
            {
                player.PrintToChat(message);
            }
        }
    }

    public static void AdvancedPlayerPrintToConsole(CCSPlayerController player, string message, params object[] args)
    {
        if (string.IsNullOrEmpty(message)) return;

        for (int i = 0; i < args.Length; i++)
        {
            message = message.Replace($"{{{i}}}", args[i].ToString() ?? "");
        }
        if (Regex.IsMatch(message, "{nextline}", RegexOptions.IgnoreCase))
        {
            string[] parts = Regex.Split(message, "{nextline}", RegexOptions.IgnoreCase);
            foreach (string part in parts)
            {
                string trimmedPart = part.Trim();
                trimmedPart = trimmedPart.ReplaceColorTags();
                if (!string.IsNullOrEmpty(trimmedPart))
                {
                    player.PrintToConsole(" " + trimmedPart);
                }
            }
        }
        else
        {
            message = message.ReplaceColorTags();
            player.PrintToConsole(message);
        }
    }
    //----
    public static bool IsPlayerInGroupPermission(CCSPlayerController player, string groups)
    {
        if (string.IsNullOrEmpty(groups) || player == null || !player.IsValid)
            return false;

        return groups.Split('|')
            .Select(segment => segment.Trim())
            .Any(trimmedSegment => Permission_CheckPermissionSegment(player, trimmedSegment));
    }

    private static bool Permission_CheckPermissionSegment(CCSPlayerController player, string segment)
    {
        if (string.IsNullOrEmpty(segment)) return false;

        int colonIndex = segment.IndexOf(':');
        if (colonIndex == -1 || colonIndex == 0) return false;

        string prefix = segment.Substring(0, colonIndex).Trim().ToLower();
        string values = segment.Substring(colonIndex + 1).Trim();

        return prefix switch
        {
            "steamid" or "steamids" or "steam" or "steams" => Permission_CheckSteamIds(player, values),
            "flag" or "flags" => Permission_CheckFlags(player, values),
            "group" or "groups" => Permission_CheckGroups(player, values),
            _ => false
        };
    }

    private static bool Permission_CheckSteamIds(CCSPlayerController player, string steamIds)
    {
        if (string.IsNullOrEmpty(steamIds)) return false;

        steamIds = steamIds.Replace("[", "").Replace("]", "");

        var (steam2, steam3, steam32, steam64) = player.SteamID.GetPlayerSteamID();
        var steam3NoBrackets = steam3.Trim('[', ']');

        return steamIds
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(id => id.Trim())
            .Any(trimmedId =>
                string.Equals(trimmedId, steam2, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(trimmedId, steam3NoBrackets, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(trimmedId, steam32, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(trimmedId, steam64, StringComparison.OrdinalIgnoreCase)
            );
    }

    private static bool Permission_CheckFlags(CCSPlayerController player, string flags)
    {
        if (player == null || !player.IsValid ||
            player.Connected != PlayerConnectedState.Connected ||
            player.IsBot || player.IsHLTV)
            return false;

        if (string.IsNullOrEmpty(flags))
            return false;

        var playerData = AdminManager.GetPlayerAdminData(player);
        if (playerData == null)
            return false;

        var requiredFlags = flags
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(f => f.Trim())
            .ToList();

        if (playerData._flags != null &&
            requiredFlags.Any(reqFlag =>
                playerData._flags.Contains(reqFlag, StringComparer.OrdinalIgnoreCase)))
            return true;

        var allFlags = playerData.GetAllFlags();
        return allFlags != null &&
            requiredFlags.Any(reqFlag =>
                allFlags.Contains(reqFlag, StringComparer.OrdinalIgnoreCase));
    }

    private static bool Permission_CheckGroups(CCSPlayerController player, string groups)
    {
        if (player == null || !player.IsValid ||
            player.Connected != PlayerConnectedState.Connected ||
            player.IsBot || player.IsHLTV)
            return false;

        if (string.IsNullOrEmpty(groups))
            return false;

        var playerData = AdminManager.GetPlayerAdminData(player);
        if (playerData == null || playerData.Groups == null || !playerData.Groups.Any())
            return false;

        return groups
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(g => g.Trim())
            .Any(reqGroup => playerData.Groups.Contains(reqGroup, StringComparer.OrdinalIgnoreCase));
    }

    public static List<CCSPlayerController> GetPlayersController(bool IncludeBots = false, bool IncludeHLTV = false, bool IncludeNone = true, bool IncludeSPEC = true, bool IncludeCT = true, bool IncludeT = true)
    {
        return Utilities
            .FindAllEntitiesByDesignerName<CCSPlayerController>("cs_player_controller")
            .Where(p =>
                p != null &&
                p.IsValid &&
                p.Connected == PlayerConnectedState.Connected &&
                (IncludeBots || !p.IsBot) &&
                (IncludeHLTV || !p.IsHLTV) &&
                ((IncludeCT && p.TeamNum == (byte)CsTeam.CounterTerrorist) ||
                (IncludeT && p.TeamNum == (byte)CsTeam.Terrorist) ||
                (IncludeNone && p.TeamNum == (byte)CsTeam.None) ||
                (IncludeSPEC && p.TeamNum == (byte)CsTeam.Spectator)))
            .ToList();
    }
    public static int GetPlayersCount(bool IncludeBots = false, bool IncludeHLTV = false, bool IncludeSPEC = true, bool IncludeCT = true, bool IncludeT = true)
    {
        return Utilities.GetPlayers().Count(p =>
            p != null &&
            p.IsValid &&
            p.Connected == PlayerConnectedState.Connected &&
            (IncludeBots || !p.IsBot) &&
            (IncludeHLTV || !p.IsHLTV) &&
            ((IncludeCT && p.TeamNum == (byte)CsTeam.CounterTerrorist) ||
            (IncludeT && p.TeamNum == (byte)CsTeam.Terrorist) ||
            (IncludeSPEC && p.TeamNum == (byte)CsTeam.Spectator))
        );
    }

    public static void ClearVariables()
    {
        var g_Main = MainPlugin.Instance.g_Main;

        MainPlugin.Instance.StopEspRuntime();
        try
        {
            g_Main.Clear();
        }
        finally
        {
            MainPlugin.Instance.ClearTransmitCache();
        }
    }

    
    public static void DebugMessage(string message, bool important = false, Con? prefixColor = null)
    {
        const string prefix = "[ESP Players]";
        if (!Configs.Instance.EnableDebug && !important) return;
        Con defaultColor = important ? Con.Red : Con.Magenta;
        prefixColor ??= Con.Purple;
        Con.WriteLine($"{prefixColor}{prefix}: {defaultColor}{message}{Con.Reset}");
    }

    public static void MuteCommands(CounterStrikeSharp.API.Modules.UserMessages.UserMessage? um, int Config, bool Fully = false)
    {
        if (um == null) return;
        if ((!Fully && Config > 0) || (Fully && Config == 2))
        {
            um.Recipients.Clear();
        }
    }

    public static void CheckPlayerInGlobals(CCSPlayerController player)
    {
        if(player == null || !player.IsValid) return;

        var g_Main = MainPlugin.Instance.g_Main;
        if (g_Main.Player_Data.TryGetValue(player.Slot, out var existing))
        {
            uint controllerHandle = player.EntityHandle.Raw;
            if (existing.ControllerHandle != controllerHandle)
            {
                RemoveGlowData(existing);
                MainPlugin.Instance.ForgetViewer(player.Slot);
                existing.ControllerHandle = controllerHandle;
            }

            existing.Player = player;
            return;
        }

        g_Main.Player_Data.Add(player.Slot, new Globals.PlayerDataClass(
            player, "", false, null!, null!, DateTime.MinValue));
    }

    public static CCSGameRules? GetGameRules()
    {
        try
        {
            var gameRulesEntities = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules");
            return gameRulesEntities.First().GameRules;
        }
        catch
        {
            return null;
        }
    }
    
    public static bool IsWarmup()
    {
        return GetGameRules()?.WarmupPeriod ?? false;
    }

    public static void RemoveGlow(CCSPlayerController player)
    {
        if (!MainPlugin.Instance.g_Main.Player_Data.TryGetValue(player.Slot, out var data)
            || data.ControllerHandle != player.EntityHandle.Raw) return;

        RemoveGlowData(data);
    }

    internal static void RemoveGlowData(Globals.PlayerDataClass data)
    {
        var glow = data.ModelGlow;
        var relay = data.ModelRelay;
        data.ModelGlow = null!;
        data.ModelRelay = null!;
        data.Player_ModelName = "";
        data.PawnHandle = 0;
        RemoveEspProp(glow);
        RemoveEspProp(relay);
    }

    private static void RemoveEspProp(CDynamicProp? prop)
    {
        try
        {
            MainPlugin.Instance.RemoveEspEntity(prop);
        }
        catch (Exception ex)
        {
            DebugMessage($"ESP entity cleanup failed: {ex.Message}", true);
        }
    }

    private static void SetGlowPlayer(Globals.PlayerDataClass data, bool hasViewer,
        Color ctColor, Color tColor)
    {
        var plugin = MainPlugin.Instance;
        var player = hasViewer ? data.Player.Get_CCSPlayerController_ControlledBot() : null;
        var pawn = player?.PlayerPawn.Value;
        if (pawn == null || !pawn.IsValid || pawn.LifeState != (byte)LifeState_t.LIFE_ALIVE)
        {
            RemoveGlowData(data);
            return;
        }

        string model = pawn.CBodyComponent?.SceneNode?.GetSkeletonInstance().ModelState.ModelName ?? "";
        if (string.IsNullOrEmpty(model))
        {
            RemoveGlowData(data);
            return;
        }

        uint pawnHandle = pawn.EntityHandle.Raw;
        byte team = player!.TeamNum;
        if (data.PawnHandle == pawnHandle && data.Player_ModelName == model
            && plugin.IsEspEntityAlive(data.ModelRelay) && plugin.IsEspEntityAlive(data.ModelGlow))
        {
            if (team != data.GlowTeam)
            {
                ApplyGlowColor(data.ModelGlow, team, ctColor, tColor);
                Utilities.SetStateChanged(data.ModelGlow, "CBaseModelEntity", "m_Glow");
                data.GlowTeam = team;
            }
            return;
        }

        RemoveGlowData(data);

        try
        {
            data.ModelRelay = Utilities.CreateEntityByName<CDynamicProp>("prop_dynamic")!;
            if (data.ModelRelay == null) return;
            plugin.TrackEspEntity(data.ModelRelay);
            data.ModelRelay.CBodyComponent!.SceneNode!.Owner!.Entity!.Flags &= ~(1u << 2);
            data.ModelRelay.SetModel(model);
            data.ModelRelay.DispatchSpawn();
            if (data.ModelRelay.Entity != null) data.ModelRelay.Entity.Name = "MR_" + Guid.NewGuid().ToString("N");
            data.ModelRelay.Spawnflags = 256u;
            data.ModelRelay.RenderMode = RenderMode_t.kRenderNone;
            data.ModelRelay.Collision.CollisionGroup = (byte)CollisionGroup.COLLISION_GROUP_NONE;

            data.ModelGlow = Utilities.CreateEntityByName<CDynamicProp>("prop_dynamic")!;
            if (data.ModelGlow == null)
            {
                RemoveGlowData(data);
                return;
            }
            plugin.TrackEspEntity(data.ModelGlow);
            data.ModelGlow.CBodyComponent!.SceneNode!.Owner!.Entity!.Flags &= ~(1u << 2);
            data.ModelGlow.Render = Color.FromArgb(1, 0, 0, 0);
            data.ModelGlow.SetModel(model);
            data.ModelGlow.DispatchSpawn();
            if (data.ModelGlow.Entity != null) data.ModelGlow.Entity.Name = "MG_" + Guid.NewGuid().ToString("N");
            data.ModelGlow.Spawnflags = 256u;
            ApplyGlowColor(data.ModelGlow, team, ctColor, tColor);
            data.ModelGlow.Glow.GlowRange = Configs.Instance.GlowRange;
            data.ModelGlow.Glow.GlowTeam = -1;
            data.ModelGlow.Glow.GlowType = Configs.Instance.GlowType ? 2 : 3;
            data.ModelGlow.Glow.GlowRangeMin = 100;

            data.ModelRelay.AcceptInput("FollowEntity", pawn, data.ModelRelay, "!activator");
            data.ModelGlow.AcceptInput("FollowEntity", data.ModelRelay, data.ModelGlow, "!activator");
            data.Player_ModelName = model;
            data.PawnHandle = pawnHandle;
            data.GlowTeam = team;
        }
        catch (Exception ex)
        {
            RemoveGlowData(data);
            DebugMessage($"ESP entity setup failed: {ex.Message}", true);
        }
    }

    private static void ApplyGlowColor(CDynamicProp glow, byte team, Color ctColor, Color tColor)
    {
        glow.Glow.GlowColorOverride = team switch
        {
            (byte)CsTeam.CounterTerrorist => ctColor,
            (byte)CsTeam.Terrorist => tColor,
            _ => Color.FromArgb(0)
        };
    }

    public static void StartTimer()
    {
        var plugin = MainPlugin.Instance;
        plugin.g_Main.Timer ??= plugin.CreateEspTimer();
    }

    private static bool CanReceiveGlow(CCSPlayerController player, byte team, Config.Config config)
        => (player.IsHLTV ? !config.DisableGlowOnGOTV : !player.IsBot)
            && (config.Show_ESP_For != 2 || team <= (byte)CsTeam.Spectator);

    internal static void ESP_Timer(IReadOnlyDictionary<int, uint>? spawns = null)
    {
        var config = Configs.Instance;
        if (config.DisableOnWarmUp && IsWarmup()) return;

        var players = Utilities.GetPlayers();
        Span<int> viewersByTeam = stackalloc int[256];
        viewersByTeam.Clear();
        int viewerCount = 0;
        foreach (var viewer in players)
        {
            byte team = viewer.TeamNum;
            if (!CanReceiveGlow(viewer, team, config)) continue;

            viewersByTeam[team]++;
            viewerCount++;
        }

        bool onlyEnemy = MainPlugin.ShowOnlyEnemyTeam(config);
        var dataBySlot = MainPlugin.Instance.g_Main.Player_Data;
        var ctColor = config.Glow_Color_CT.ToColor();
        var tColor = config.Glow_Color_T.ToColor();
        foreach (var player in players)
        {
            if (player.IsHLTV) continue;
            int slot = player.Slot;
            if (spawns != null && (!spawns.TryGetValue(slot, out uint handle)
                || player.EntityHandle.Raw != handle)) continue;

            CheckPlayerInGlobals(player);
            byte team = player.TeamNum;
            int excluded = onlyEnemy ? viewersByTeam[team] : CanReceiveGlow(player, team, config) ? 1 : 0;
            SetGlowPlayer(dataBySlot[slot], viewerCount > excluded, ctColor, tColor);
        }
    }

    public static void ReloadPlayersGlobals()
    {
        foreach (var player in Utilities.GetPlayers())
        {
            if (!player.IsHLTV) CheckPlayerInGlobals(player);
        }
        MainPlugin.Instance.ScheduleEspReconcile();
    }

    public static void RemoveAllPlayersGlow()
    {
        var players = MainPlugin.Instance.g_Main.Player_Data;
        foreach (var data in players.Values) RemoveGlowData(data);
        players.Clear();
    }

    public static void RegisterCommandsAndHooks()
    {
        if (!MainPlugin.Instance.StartEspHooks()) return;
        MainPlugin.Instance.RegisterListener<Listeners.OnEntityDeleted>(MainPlugin.Instance.OnEspEntityDeleted);
        MainPlugin.Instance.RegisterListener<Listeners.OnMapStart>(MainPlugin.Instance.OnMapStart);
        MainPlugin.Instance.RegisterListener<Listeners.OnClientPutInServer>(MainPlugin.Instance.OnClientPutInServer);
        MainPlugin.Instance.RegisterListener<Listeners.CheckTransmit>(MainPlugin.Instance.OnCheckTransmit);
        MainPlugin.Instance.RegisterListener<Listeners.OnMapEnd>(MainPlugin.Instance.OnMapEnd);

        MainPlugin.Instance.RegisterEventHandler<EventRoundStart>(MainPlugin.Instance.OnEventRoundStart);
        MainPlugin.Instance.RegisterEventHandler<EventPlayerDeath>(MainPlugin.Instance.OnEventPlayerDeath);
        MainPlugin.Instance.RegisterListener<Listeners.OnClientDisconnect>(MainPlugin.Instance.OnEspClientDisconnect);
        MainPlugin.Instance.RegisterEventHandler<EventPlayerSpawn>(MainPlugin.Instance.OnEspSpawn);
        MainPlugin.Instance.RegisterEventHandler<EventPlayerTeam>(MainPlugin.Instance.OnEspTeam);
        MainPlugin.Instance.RegisterEventHandler<EventBotTakeover>(MainPlugin.Instance.OnEspBotTakeover);

        MainPlugin.Instance.AddCommandListener("say", MainPlugin.Instance.OnPlayerSay, HookMode.Post);
        MainPlugin.Instance.AddCommandListener("say_team", MainPlugin.Instance.OnPlayerSay_Team, HookMode.Post);
        MainPlugin.Instance.HookUserMessage(118, MainPlugin.Instance.OnUserMessage_OnSayText2, HookMode.Pre);

        RegisterCssCommands(Configs.Instance.Reload_Plugin.Reload_Plugin_CommandsInGame.ConvertCommands(), "Commands To Reload ESP Plugin", MainPlugin.Instance.Game_UserMessages.CommandsAction_ReloadPlugin);
        RegisterCssCommands(Configs.Instance.Toggle_ESP.Toggle_ESP_CommandsInGame.ConvertCommands(), "Commands To Toggle On/Off ESP", MainPlugin.Instance.Game_UserMessages.CommandsAction_Toggle_ESP);
        RegisterCssCommands(Configs.Instance.Give_ESP.Give_ESP_CommandsInGame.ConvertCommands(), "Commands To Give ESP To Players On/Off", MainPlugin.Instance.Game_UserMessages.CommandsAction_Give_ESP);

        StartTimer();
    }

    public static void RemoveRegisterCommandsAndHooks()
    {
        if (!MainPlugin.Instance.StopEspHooks()) return;
        MainPlugin.Instance.RemoveListener<Listeners.OnEntityDeleted>(MainPlugin.Instance.OnEspEntityDeleted);
        MainPlugin.Instance.RemoveListener<Listeners.OnMapStart>(MainPlugin.Instance.OnMapStart);
        MainPlugin.Instance.RemoveListener<Listeners.OnClientPutInServer>(MainPlugin.Instance.OnClientPutInServer);
        MainPlugin.Instance.RemoveListener<Listeners.CheckTransmit>(MainPlugin.Instance.OnCheckTransmit);
        MainPlugin.Instance.RemoveListener<Listeners.OnMapEnd>(MainPlugin.Instance.OnMapEnd);

        MainPlugin.Instance.DeregisterEventHandler<EventRoundStart>(MainPlugin.Instance.OnEventRoundStart);
        MainPlugin.Instance.DeregisterEventHandler<EventPlayerDeath>(MainPlugin.Instance.OnEventPlayerDeath);
        MainPlugin.Instance.RemoveListener<Listeners.OnClientDisconnect>(MainPlugin.Instance.OnEspClientDisconnect);
        MainPlugin.Instance.DeregisterEventHandler<EventPlayerSpawn>(MainPlugin.Instance.OnEspSpawn);
        MainPlugin.Instance.DeregisterEventHandler<EventPlayerTeam>(MainPlugin.Instance.OnEspTeam);
        MainPlugin.Instance.DeregisterEventHandler<EventBotTakeover>(MainPlugin.Instance.OnEspBotTakeover);

        MainPlugin.Instance.RemoveCommandListener("say", MainPlugin.Instance.OnPlayerSay, HookMode.Post);
        MainPlugin.Instance.RemoveCommandListener("say_team", MainPlugin.Instance.OnPlayerSay_Team, HookMode.Post);
        MainPlugin.Instance.UnhookUserMessage(118, MainPlugin.Instance.OnUserMessage_OnSayText2, HookMode.Pre);

        RemoveCssCommands(Configs.Instance.Reload_Plugin.Reload_Plugin_CommandsInGame.ConvertCommands(), MainPlugin.Instance.Game_UserMessages.CommandsAction_ReloadPlugin);
        RemoveCssCommands(Configs.Instance.Toggle_ESP.Toggle_ESP_CommandsInGame.ConvertCommands(), MainPlugin.Instance.Game_UserMessages.CommandsAction_Toggle_ESP);
        RemoveCssCommands(Configs.Instance.Give_ESP.Give_ESP_CommandsInGame.ConvertCommands(), MainPlugin.Instance.Game_UserMessages.CommandsAction_Give_ESP);
    }
}