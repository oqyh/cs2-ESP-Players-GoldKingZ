using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Utils;
using System.Text.RegularExpressions;
using ESP_Players.Config;
using System.Drawing;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Admin;
using System.Security.Cryptography;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Entities;

namespace ESP_Players;

public class Helper
{
    public static void RegisterCssCommands(string[] commands, string description, CommandInfo.CommandCallback callback)
    {
        foreach (var cmd in commands)
        {
            if (cmd.StartsWith("css_"))
            {
                MainPlugin.Instance.AddCommand(cmd, description, callback);
            }
        }
    }

    public static void RemoveCssCommands(string[] commands, CommandInfo.CommandCallback callback)
    {
        foreach (var cmd in commands)
        {
            if (cmd.StartsWith("css_"))
            {
                MainPlugin.Instance.RemoveCommand(cmd, callback);
            }
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
            message = message.Replace($"{{{i}}}", args[i].ToString());
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
    public static void AdvancedServerPrintToChatAll(string message, params object[] args)
    {
        if (string.IsNullOrEmpty(message)) return;

        for (int i = 0; i < args.Length; i++)
        {
            message = message.Replace($"{{{i}}}", args[i].ToString());
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
                    Server.PrintToChatAll(" " + trimmedPart);
                }
            }
        }
        else
        {
            message = message.ReplaceColorTags();
            Server.PrintToChatAll(message);
        }
    }
    
    public static List<CCSPlayerController> GetPlayersController(bool IncludeBots = false, bool IncludeHLTV = false, bool IncludeNone = true, bool IncludeSPEC = true, bool IncludeCT = true, bool IncludeT = true)
    {
        return Utilities
            .FindAllEntitiesByDesignerName<CCSPlayerController>("cs_player_controller")
            .Where(p =>
                p != null &&
                p.IsValid &&
                p.Connected == PlayerConnectedState.PlayerConnected &&
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
            p.Connected == PlayerConnectedState.PlayerConnected && 
            (IncludeBots || !p.IsBot) &&
            (IncludeHLTV || !p.IsHLTV) &&
            ((IncludeCT && p.TeamNum == (byte)CsTeam.CounterTerrorist) || 
            (IncludeT && p.TeamNum == (byte)CsTeam.Terrorist) || 
            (IncludeSPEC && p.TeamNum == (byte)CsTeam.Spectator))
        );
    }

    public static bool IsPlayerInGroupPermission(CCSPlayerController player, string groups)
    {
        if (string.IsNullOrEmpty(groups) || player == null || !player.IsValid)return false;

        return groups.Split('|')
        .Select(segment => segment.Trim())
        .Any(trimmedSegment => Permission_CheckPermissionSegment(player, trimmedSegment));
    }
    private static bool Permission_CheckPermissionSegment(CCSPlayerController player, string segment)
    {
        if (string.IsNullOrEmpty(segment))return false;

        int colonIndex = segment.IndexOf(':');
        if (colonIndex == -1 || colonIndex == 0)return false;

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
        return flags.Split(',')
        .Select(flag => flag.Trim())
        .Where(trimmedFlag => trimmedFlag.StartsWith("@"))
        .Any(trimmedFlag => MyPlayerHasPermissions(player, trimmedFlag));
    }
    private static bool Permission_CheckGroups(CCSPlayerController player, string groups)
    {
        return groups.Split(',')
        .Select(group => group.Trim())
        .Where(trimmedGroup => trimmedGroup.StartsWith("#"))
        .Any(trimmedGroup => MyPlayerInGroup(player, trimmedGroup));
    }
    public static bool MyPlayerHasPermissions(CCSPlayerController player, params string[] flags)
    {
        if (player == null) return true;

        if (!player.IsValid || player.Connected != PlayerConnectedState.PlayerConnected || player.IsBot || player.IsHLTV) return false;

        var playerData = AdminManager.GetPlayerAdminData(player);
        if (playerData == null) return false;

        foreach (var domain in playerData.Flags)
        {
            if (string.IsNullOrEmpty(domain.Key)) continue;

            var domainFlags = flags
            .Where(flag => flag.StartsWith($"@{domain.Key}/"))
            .ToArray();

            if (domainFlags.Length == 0) continue;

            if (!playerData.DomainHasFlags(domain.Key, domainFlags, true))
            {
                return false;
            }
        }
        return true;
    }
    public static bool MyPlayerInGroup(CCSPlayerController? player, params string[] groups)
    {
        if (player == null) return true;
        
        if (!player.IsValid || player.Connected != PlayerConnectedState.PlayerConnected || player.IsBot || player.IsHLTV)return false;
        
        return MyPlayerInGroup(player.AuthorizedSteamID, groups);
    }
    public static bool MyPlayerInGroup(SteamID? steamId, params string[] groups)
    {
        if (steamId == null)return false;

        var playerData = AdminManager.GetPlayerAdminData(steamId);
        if (playerData == null)return false;

        var groupsToCheck = groups.ToHashSet();
        foreach (var domain in playerData.Flags)
        {
            if (string.IsNullOrEmpty(domain.Key)) continue;

            if (playerData.Flags[domain.Key].Contains("@" + domain.Key + "/*"))
            {
                groupsToCheck.ExceptWith(groups.Where(group => group.Contains(domain.Key + '/')));
            }
        }
        return playerData.Groups.IsSupersetOf(groupsToCheck);
    }
    
    public static void ClearVariables()
    {
        var g_Main = MainPlugin.Instance.g_Main;

        if (g_Main.Timer != null)
        {
            g_Main.Timer.Kill();
            g_Main.Timer = null!;
        }

        SavePlayersValues();
    }
    public static void SavePlayersValues()
    {
        var g_Main = MainPlugin.Instance.g_Main;
        foreach(var alldata in g_Main.Player_Data.Values)
        {
            if(alldata == null)
            {
                g_Main.Player_Data.Clear();
                return;
            }
            
            if(alldata.Toggle_ESP < 0)
            {
                var player_SteamID = alldata.SteamId;
                var player_Toggle_ESP = alldata.Toggle_ESP;
                
                Cookies.SaveToJsonFile(
                    player_SteamID,
                    player_Toggle_ESP,
                    DateTime.Now
                );

                if (Configs.GetConfigData().MySql_Enable)
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {                        
                            await MySqlDataManager.SaveToMySqlAsync(new Globals_Static.PersonData 
                            {
                                PlayerSteamID = player_SteamID,
                                Toggle_ESP = player_Toggle_ESP,
                                DateAndTime = DateTime.Now
                            });
                        }
                        catch (Exception ex)
                        {
                            DebugMessage($"SavePlayerValues error: {ex.Message}");
                        }
                    });
                }
            }

            if(alldata.ModelRelay != null && alldata.ModelRelay.IsValid)
            {
                alldata.ModelRelay.Remove();
            }

            if(alldata.ModelGlow != null && alldata.ModelGlow.IsValid)
            {
                alldata.ModelGlow.Remove();
            }
        }

        g_Main.Player_Data.Clear();

        if (Configs.GetConfigData().Cookies_Enable)
        {
            try 
            {
                Cookies.FetchAndRemoveOldJsonEntries();
            }
            catch (Exception ex)
            {
                DebugMessage($"Cookie cleanup error: {ex.Message}");
            }
        }

        if (Configs.GetConfigData().MySql_Enable)
        {
            _ = Task.Run(async () =>
            {
                try
                {                        
                    await MySqlDataManager.DeleteOldPlayersAsync();
                }
                catch (Exception ex)
                {
                    DebugMessage($"MySQL cleanup error: {ex.Message}");
                }
            });
        }
    }

    public static void DebugMessage(string message, bool prefix = true)
    {
        if (!Configs.GetConfigData().EnableDebug) return;

        Console.ForegroundColor = ConsoleColor.Magenta;
        string output = prefix ? $"[ESP Players]: {message}" : message;
        Console.WriteLine(output);
        
        Console.ResetColor();
    }

    public static void CheckPlayerInGlobals(CCSPlayerController player)
    {
        if(!player.IsValid(true))return;

        if (!MainPlugin.Instance.g_Main.Player_Data.ContainsKey(player))
        {
            MainPlugin.Instance.g_Main.Player_Data.Add(player, new Globals.PlayerDataClass(player, null!, "", null!, null!, player.SteamID, Configs.GetConfigData().DefaultToggleGlow ? 1 : 2, DateTime.Now, DateTime.Now));
        }
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

    public static async Task LoadPlayerData(CCSPlayerController player)
    {
        try
        {
            var g_Main = MainPlugin.Instance.g_Main;
            if (!player.IsValid(true) || g_Main.Player_Data.ContainsKey(player)) return;
            
            var steamId = player.SteamID;

            await Server.NextFrameAsync(() => 
            {
                if (!player.IsValid(true)) return;

                CheckPlayerInGlobals(player);
            });

            if (Configs.GetConfigData().Cookies_Enable)
            {
                await Server.NextFrameAsync(() => 
                {
                    if (!player.IsValid()) return;

                    var cookieData = Cookies.RetrievePersonDataById(steamId);
                    if (cookieData.PlayerSteamID != 0)
                    {
                        UpdatePlayerData(player, cookieData);
                    }
                    
                });
            }
            

            if (Configs.GetConfigData().MySql_Enable)
            {
                try
                {
                    var mysqlData = await MySqlDataManager.RetrievePersonDataByIdAsync(steamId);
                    
                    await Server.NextFrameAsync(() => 
                    {
                        if (!player.IsValid()) return;

                        if (mysqlData.PlayerSteamID != 0)
                        {
                            UpdatePlayerData(player, mysqlData);
                        }
                    });
                }
                catch (Exception ex)
                {
                    DebugMessage($"Error in MySql LoadPlayerData: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            DebugMessage($"LoadPlayerData error: {ex.Message}");
        }
    }

    private static void UpdatePlayerData(CCSPlayerController player, Globals_Static.PersonData data)
    {
        if (!player.IsValid() || !MainPlugin.Instance.g_Main.Player_Data.ContainsKey(player))return;
        if (!MainPlugin.Instance.g_Main.Player_Data.TryGetValue(player, out var handle))return;


        if(data.Toggle_ESP < 0 )
        {
            handle.Toggle_ESP = data.Toggle_ESP;

            Cookies.SaveToJsonFile(
                handle.SteamId,
                handle.Toggle_ESP,
                DateTime.Now
            );
        }
    }
    
    public static void SetGlowPlayer(CCSPlayerController player)
    {
        var g_Main = MainPlugin.Instance.g_Main;
        if (!player.IsValid(true) || player.PlayerPawn.Value == null || !g_Main.Player_Data.TryGetValue(player, out var handle))return;
        
        string modelName = player.PlayerPawn.Value!.CBodyComponent!.SceneNode!.GetSkeletonInstance().ModelState.ModelName;
        if(handle.ModelRelay != null && handle.ModelRelay.IsValid && handle.ModelGlow != null && handle.ModelGlow.IsValid)
        {
            if(!string.IsNullOrEmpty(handle.ModelName) && !string.IsNullOrEmpty(modelName) && handle.ModelName != modelName)
            {
                if(handle.ModelRelay != null && handle.ModelRelay.IsValid)
                {
                    handle.ModelRelay.Remove();
                }

                if(handle.ModelGlow != null && handle.ModelGlow.IsValid)
                {
                    handle.ModelGlow.Remove();
                }
            }
            return;
        }

        handle.ModelRelay = Utilities.CreateEntityByName<CDynamicProp>("prop_dynamic")!;
        if (handle.ModelRelay == null)return;

        
        if(!string.IsNullOrEmpty(modelName))
        {
            handle.ModelName = player.PlayerPawn.Value!.CBodyComponent!.SceneNode!.GetSkeletonInstance().ModelState.ModelName;
        }
        handle.ModelRelay.DispatchSpawn();
        handle.ModelRelay.SetModel(modelName);
        handle.ModelRelay.Spawnflags = 256u;
        handle.ModelRelay.RenderMode = RenderMode_t.kRenderNone;

        handle.ModelGlow = Utilities.CreateEntityByName<CDynamicProp>("prop_dynamic")!;
        if (handle.ModelGlow == null)return;

        handle.ModelGlow.Render = Color.FromArgb(1, 0, 0, 0);
        handle.ModelGlow.DispatchSpawn();
        handle.ModelGlow.SetModel(modelName);
        handle.ModelGlow.Spawnflags = 256u;
        
        if(player.TeamNum == (byte)CsTeam.CounterTerrorist)
        {
            handle.ModelGlow.Glow.GlowColorOverride = Configs.GetConfigData().Glow_Color_CT.ToColor();
        }else if(player.TeamNum == (byte)CsTeam.Terrorist)
        {
            handle.ModelGlow.Glow.GlowColorOverride = Configs.GetConfigData().Glow_Color_T.ToColor();
        }
        handle.ModelGlow.Glow.GlowRange = Configs.GetConfigData().GlowRange;
        handle.ModelGlow.Glow.GlowTeam = -1;
        handle.ModelGlow.Glow.GlowType = Configs.GetConfigData().GlowType?2:3;
        handle.ModelGlow.Glow.GlowRangeMin = 100;

        handle.ModelRelay.AcceptInput("FollowEntity", player.PlayerPawn.Value, handle.ModelRelay, "!activator");
        handle.ModelGlow.AcceptInput("FollowEntity", handle.ModelRelay, handle.ModelGlow, "!activator");
    }
}