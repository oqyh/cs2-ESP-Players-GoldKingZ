using CounterStrikeSharp.API.Core;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Drawing;


namespace ESP_Players;

public static class Extension
{
    public static bool IsValid([NotNullWhen(true)] this CCSPlayerController? player, bool IncludeBots = false, bool IncludeHLTV = false)
    {
        if (player == null || !player.IsValid)
            return false;

        if (!IncludeBots && player.IsBot)
            return false;

        if (!IncludeHLTV && player.IsHLTV)
            return false;

        return true;
    }

    public static bool IsAlive(this CCSPlayerController? player)
    {
        if (player == null || !player.IsValid
        || player.PlayerPawn == null || !player.PlayerPawn.IsValid
        || player.PlayerPawn.Value == null || !player.PlayerPawn.Value.IsValid) return false;

        return player.PlayerPawn.Value.LifeState == (byte)LifeState_t.LIFE_ALIVE;
    }

    public static int ToggleOnOff(this int value)
    {
        return value switch
        {
            1 => -2,
            2 => -1,
            -1 => -2,
            -2 => -1,
            _ => value
        };
    }

    private const ulong Steam64Offset = 76561197960265728UL;
    public static (string steam2, string steam3, string steam32, string steam64) GetPlayerSteamID(this ulong steamId64)
    {
        uint id32 = (uint)(steamId64 - Steam64Offset);
        var steam32 = id32.ToString();
        uint y = id32 & 1;
        uint z = id32 >> 1;
        var steam2 = $"STEAM_0:{y}:{z}";
        var steam3 = $"[U:1:{steam32}]";
        var steam64 = steamId64.ToString();
        return (steam2, steam3, steam32, steam64);
    }

    public static string[] GetCommands(this string csv, bool toChat = false)
    {
        return csv
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(cmd => cmd.Trim())
            .Select(cmd =>
                toChat
                    ? cmd.StartsWith("css_")
                        ? "!" + cmd[4..]
                        : cmd
                    : cmd.StartsWith("!")
                        ? "css_" + cmd[1..]
                        : cmd
            )
            .Select(cmd => cmd.ToLowerInvariant())
            .Distinct()
            .ToArray();
    }
    
    public static Color ToColor(this string colorString)
    {
        if (string.IsNullOrWhiteSpace(colorString))
        {
            Helper.DebugMessage("Color string cannot be empty or whitespace.");
            return Color.Transparent;
        }

        var components = colorString
            .Replace(',', ' ')
            .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (components.Length != 3 && components.Length != 4)
        {
            Helper.DebugMessage("Invalid color format. Expected 3 or 4 components.");
            return Color.Transparent;
        }

        if (!int.TryParse(components[0], out int r) || r < 0 || r > 255)
        {
            Helper.DebugMessage("Invalid Red component value. Must be 0-255.");
            return Color.Transparent;
        }
        if (!int.TryParse(components[1], out int g) || g < 0 || g > 255)
        {
            Helper.DebugMessage("Invalid Green component value. Must be 0-255.");
            return Color.Transparent;
        }
        if (!int.TryParse(components[2], out int b) || b < 0 || b > 255)
        {
            Helper.DebugMessage("Invalid Blue component value. Must be 0-255.");
            return Color.Transparent;
        }

        byte a = 255;
        if (components.Length == 4)
        {
            string alphaComponent = components[3];
            if (alphaComponent.Contains('.'))
            {
                if (!double.TryParse(alphaComponent, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double alphaDouble))
                {
                    Helper.DebugMessage("Invalid alpha format. Must be 0-255 or 0.0-1.0.");
                    return Color.Transparent;
                }
                if (alphaDouble < 0 || alphaDouble > 1)
                {
                    Helper.DebugMessage("Alpha value must be between 0.0 and 1.0 when using decimal format.");
                    return Color.Transparent;
                }
                a = (byte)(alphaDouble * 255);
            }
            else
            {
                if (!int.TryParse(alphaComponent, out int alphaInt) || alphaInt < 0 || alphaInt > 255)
                {
                    Helper.DebugMessage("Invalid alpha value. Must be 0-255 or 0.0-1.0.");
                    return Color.Transparent;
                }
                a = (byte)alphaInt;
            }
        }
        return Color.FromArgb(a, r, g, b);
    }
}