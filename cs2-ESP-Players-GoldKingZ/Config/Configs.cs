using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
 
namespace ESP_Players_GoldKingZ.Config
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)] public class CommentAttribute : Attribute { public string Text; public CommentAttribute(string t) => Text = t; }
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)] public class InfoAttribute : Attribute { public string Key; public InfoAttribute(string k) => Key = k; }
    [AttributeUsage(AttributeTargets.Property)] public class BreakLineAttribute : Attribute { public string Text; public BreakLineAttribute(string t) => Text = t; }
    [AttributeUsage(AttributeTargets.Property)] public class RangeAttribute : Attribute { public double Min, Max; public RangeAttribute(double min, double max) { Min = min; Max = max; } }
 
    public enum Format { Flag, Command }
    public static class Formats
    {
        public static string[] Get(Format f) => f == Format.Flag
            ? new[] { "SteamIDs", "Flags", "Groups" }
            : new[] { "Console_Commands", "Chat_Commands" };
    }
 
    [AttributeUsage(AttributeTargets.Property)]
    public class StringAttribute : Attribute
    {
        public string[] Keys;
        public StringAttribute(params string[] keys) => Keys = keys;
        public StringAttribute(Format format) => Keys = Formats.Get(format);
    }

    public class Reload_Plugin
    {
        [Comment("Note: Console_Commands Can Be Execute Via Both Console And Chat By (! or css_)")]
        [Comment("Making Both Console_Commands And Chat_Commands Empty = Disable")]
        [String("Console_Commands", "Chat_Commands")]
        public string Reload_Plugin_CommandsInGame { get; set; } = "Console_Commands: css_reloadesp | Chat_Commands: ";

        [Comment("If [Reload_Plugin_CommandsInGame] Pass, Is There Any Specified Restricted Flags, Groups, SteamIDs")]
        [Comment("Example:")]
        [Comment("\"SteamIDs: 76561198206086993,STEAM_0:1:507335558 | Flags: @css/root,@css/admin | Groups: #css/root,#css/admin\"")]
        [Comment("\"SteamIDs:  | Flags:  | Groups: \" = To Allow Everyone")]
        [String("SteamIDs", "Flags", "Groups")]
        public string Reload_Plugin_Flags { get; set; } = "SteamIDs: 76561198206086993,STEAM_0:1:507335558 | Flags: @css/root,@css/admin | Groups: #css/root,#css/admin";

        [Comment("If [Reload_Plugin_Flags] Pass, Hide Chat After Execute Reload_Plugin_CommandsInGame?:")]
        [Comment("0 = No")]
        [Comment("1 = Yes, But Only After Toggle Successfully")]
        [Comment("2 = Yes, Hide All The Time")]
        [Range(0, 2)]
        public int Reload_Plugin_Hide { get; set; } = 0;
    }

    public class Give_ESP
    {
        [Comment("Note: Console_Commands Can Be Execute Via Both Console And Chat By (! or css_)")]
        [Comment("Making Both Console_Commands And Chat_Commands Empty = Disable")]
        [String("Console_Commands", "Chat_Commands")]
        public string Give_ESP_CommandsInGame { get; set; } = "Console_Commands: css_giveesp | Chat_Commands: ";

        [Comment("If [Give_ESP_CommandsInGame] Is Used, Flags Or Group Or SteamID")]
        [Comment("Example:")]
        [Comment("\"SteamIDs: 76561198206086993,STEAM_0:1:507335558 | Flags: @css/root,@css/admin | Groups: #css/root,#css/admin\"")]
        [Comment("\"SteamIDs:  | Flags:  | Groups: \" = To Allow Everyone")]
        [String("SteamIDs", "Flags", "Groups")]
        public string Give_ESP_Flags { get; set; } = "SteamIDs: 76561198206086993,STEAM_0:1:507335558 | Flags: @css/root,@css/admin | Groups: #css/root,#css/admin";

        [Comment("If [Give_ESP_CommandsInGame] Is Used, Hide Chat After Execute Give_ESP_CommandsInGame?:")]
        [Comment("0 = No")]
        [Comment("1 = Yes, But Only After Toggle Successfully")]
        [Comment("2 = Yes, Hide All The Time")]
        [Range(0, 2)]
        public int Give_ESP_Hide { get; set; } = 2;

        [Comment("Save Given ESP Players On Disconnect/Reconnect?")]
        [Comment("true  = Yes, Keep Enable Given ESP To Players On Disconnect/Reconnect But Disable It On Map Change")]
        [Comment("false = No Dont Save (One-Time Session), Disable Given ESP To Players When Disconnect Or Map Change")]
        public bool Give_ESP_SaveOnDisconnect { get; set; } = true;
    }
    
    public class Toggle_ESP
    {
        [Comment("Note: Console_Commands Can Be Execute Via Both Console And Chat By (! or css_)")]
        [Comment("Making Both Console_Commands And Chat_Commands Empty = Disable")]
        [String("Console_Commands", "Chat_Commands")]
        public string Toggle_ESP_CommandsInGame { get; set; } = "Console_Commands: css_esp,css_glow,!showplayers | Chat_Commands: ";

        [Comment("If [Toggle_ESP_CommandsInGame] Is Used, Flags Or Group Or SteamID")]
        [Comment("Example:")]
        [Comment("\"SteamIDs: 76561198206086993,STEAM_0:1:507335558 | Flags: @css/root,@css/admin | Groups: #css/root,#css/admin\"")]
        [Comment("\"SteamIDs:  | Flags:  | Groups: \" = To Allow Everyone")]
        [String("SteamIDs", "Flags", "Groups")]
        public string Toggle_ESP_Flags { get; set; } = "SteamIDs: 76561198206086993,STEAM_0:1:507335558 | Flags: @css/root,@css/admin | Groups: #css/root,#css/admin";

        [Comment("If [Toggle_ESP_CommandsInGame] Is Used, Hide Chat After Execute Toggle_ESP_CommandsInGame?:")]
        [Comment("0 = No")]
        [Comment("1 = Yes, But Only After Toggle Successfully")]
        [Comment("2 = Yes, Hide All The Time")]
        [Range(0, 2)]
        public int Toggle_ESP_Hide { get; set; } = 0;

        [Comment("Default Glow Toggle To New Players?")]
        [Comment("true = On")]
        [Comment("false = Off (New Players When Join They Will Not See Any ESP They Need To Toggle Toggle_ESP_CommandsInGame)")]
        public bool Default_Toggle_ESP { get; set; } = false;
    }

    

    public class MySqlServer
    {
        [Comment("MySQL Server address (hostname or IP)")]
        public string Server { get; set; } = "localhost";

        [Comment("MySQL Server port")]
        public int Port { get; set; } = 3306;

        [Comment("MySQL Database name")]
        public string Database { get; set; } = "MySql_Database";

        [Comment("MySQL Username")]
        public string Username { get; set; } = "MySql_Username";

        [Comment("MySQL Password")]
        public string Password { get; set; } = "MySql_Password";
    }

    public class MySqlConfig
    {
        [Comment("MySQL Servers You Can Add As Many As You like")]
        public List<MySqlServer> MySql_Servers { get; set; } = new List<MySqlServer>
        {
            new MySqlServer
            {
                Server = "localhost",
                Port = 3306,
                Database = "Database",
                Username = "Username",
                Password = "Password"
            },
            new MySqlServer
            {
                Server = "localhost2",
                Port = 3306,
                Database = "Database2",
                Username = "Username2",
                Password = "Password2"
            }
        };
    }

    public class Config
    {
        [BreakLine("----------------------------[ ↓ Plugin Info ↓ ]----------------------------{nextline}")]
        [Info("Version")]
        [Info("Github")]
        public object __InfoSection { get; set; } = null!;

        [BreakLine("----------------------------[ ↓ Main Config ↓ ]----------------------------{nextline}")]

        [Comment("Reload ESP-Players Plugin")]
        public Reload_Plugin Reload_Plugin { get; set; } = new();

        [Comment("Give ESP To Players On/Off")]
        public Give_ESP Give_ESP { get; set; } = new();

        [Comment("Toggle On/Off ESP")]
        public Toggle_ESP Toggle_ESP { get; set; } = new();

        [Comment("Disable ESP On WarmUp?")]
        [Comment("true = Yes")]
        [Comment("false = No")]
        public bool DisableOnWarmUp { get; set; } = false;

        [Comment("Disable Glow In Demo GOTV/HLTV?")]
        [Comment("true = Yes")]
        [Comment("false = No")]
        public bool DisableGlowOnGOTV { get; set; } = true;

        [Comment("Show ESP For?")]
        [Comment("0 = Any")]
        [Comment("1 = Dead Players Only")]
        [Comment("2 = Spec Players Only")]
        [Range(0, 2)]
        public int Show_ESP_For { get; set; } = 0;

        [Comment("Required [Show_ESP_For = 0/1]")]
        [Comment("Show ESP Only Enemy Team?")]
        [Comment("true = Yes (Disable Teammate ESP)")]
        [Comment("false = No (Show All)")]
        public bool ShowOnlyEnemyTeam { get; set; } = true;

        [BreakLine("----------------------------[ ↓ Glow Config ↓ ]----------------------------{nextline}")]

        [Comment("Glow Only When Crosshair Near To Player Glow?")]
        [Comment("true = Yes")]
        [Comment("false = No (Show All The Time)")]
        public bool GlowType { get; set; } = false;

        [Comment("Whats Max Range To Show Player Glow")]
        public int GlowRange { get; set; } = 5000;

        [Comment("How Would You Like Glow Color Counter Terrorist (CT) Players By (Red, Green, Blue, Alpha) Use This Site [https://rgbacolorpicker.com/]")]
        public string Glow_Color_CT { get; set; } = "0, 190, 255, 255";

        [Comment("How Would You Like Glow Color Terrorist (T) Players By (Red, Green, Blue, Alpha) Use This Site [https://rgbacolorpicker.com/]")]
        public string Glow_Color_T { get; set; } = "243, 0, 93, 255";
        
        [BreakLine("----------------------------[ ↓ Locally Config (ClientPrefs-GoldKingZ API) ↓ ]----------------------------{nextline}")]

        [Comment("Save Players Data By Cookies Locally (In ../plugins/ClientPrefs-GoldKingZ/ESP-Players-GoldKingZ/)?")]
        [Comment("0 = No")]
        [Comment("1 = Yes, But Save Data On Players Disconnect (Warning Performance)")]
        [Comment("2 = Yes, But Save Data On Map Change (Recommended)")]
        [Range(0, 2)]
        public int Cookies_Enable { get; set; } = 2;

        [Comment("If [Cookies_Enable = 1 or 2], Auto Delete Inactive Players More Than X (Days) Old")]
        [Comment("0 = Dont Auto Delete")]
        public int Cookies_AutoRemoveInactivePlayersOlderThanDays { get; set; } = 7;

        [BreakLine("----------------------------[ ↓ MySql Config (ClientPrefs-GoldKingZ API) ↓ ]----------------------------{nextline}")]
        
        [Comment("Save Players Data Into MySql?")]
        [Comment("0 = No")]
        [Comment("1 = Yes, But Save Data On Players Disconnect (Warning Performance)")]
        [Comment("2 = Yes, But Save Data On Map Change (Recommended)")]
        [Range(0, 2)]
        public int MySql_Enable { get; set; } = 0;

        [Comment("Connection Timeout In Seconds")]
        [Range(5, 60)]
        public int MySql_ConnectionTimeout { get; set; } = 30;

        [Comment("Retry Attempts When Connection Fails")]
        [Range(1, 5)]
        public int MySql_RetryAttempts { get; set; } = 3;

        [Comment("Delay Between Retries In Seconds")]
        [Range(1, 10)]
        public int MySql_RetryDelay { get; set; } = 2;

        [Comment("MySql Config")]
        public MySqlConfig MySql_Config { get; set; } = new MySqlConfig();

        [Comment("Auto Delete Inactive Players More Than X (Days) Old")]
        [Comment("0 = Dont Auto Delete")]
        public int MySql_AutoRemoveInactivePlayersOlderThanDays { get; set; } = 7;

        [BreakLine("----------------------------[ ↓ Utilities ↓ ]----------------------------{nextline}")]
        
        [Comment("Enable Debug Plugin In Server Console (Helps You To Debug Issues You Facing)?")]
        [Comment("true = Yes")]
        [Comment("false = No")]
        public bool EnableDebug { get; set; } = false;
    }

    public static class Configs
    {
        public static string Version => $"Version : {MainPlugin.Instance?.ModuleVersion ?? "Unknown"}";
        public static string Github = "https://github.com/oqyh/cs2-ESP-Players-GoldKingZ";
        public static Config Instance { get; private set; } = new();
        static string _file = "";
        static readonly JsonSerializerOptions Opts = new()
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };
 
        static readonly JsonSerializerOptions Pretty = new(Opts) { WriteIndented = true };
 
        public static void Load(string moduleDirectory, bool reload = false)
        {
            _file = System.IO.Path.Combine(moduleDirectory ?? ".", "config", "config.json");
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(_file)!);
 
            Instance = new Config();
            Walk(Instance, ReadFile(), "");
            Save();
 
            if (reload) Warn($"config.json {(File.Exists(_file) ? "Reloaded" : "Created")}");
        }
 
        public static void Save()
        {
            try { File.WriteAllText(_file, "{\n" + Render(Instance, 2) + "\n}\n"); }
            catch (Exception e) { Helper.DebugMessage($"Cant Save config.json ({e.Message})", true); }
        }
 
        static void Warn(string message) => Helper.DebugMessage(message, true);
 
        static JsonObject? ReadFile()
        {
            if (!File.Exists(_file)) return null;
 
            string text;
            try { text = File.ReadAllText(_file); } catch { return null; }
 
            var noComments = string.Join("\n", text.Split('\n').Where(l => !l.TrimStart().StartsWith("//")));
            var commas = Regex.Replace(noComments, @"([}\]""\d]|true|false|null)(\s*\r?\n\s*)([""{\[])", "$1,$2$3");
 
            foreach (var attempt in new[] { text, noComments, commas })
            {
                try
                {
                    if (JsonNode.Parse(attempt, documentOptions: new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true }) is not JsonObject obj) continue;
 
                    if (attempt == commas && commas != noComments) Warn("config.json Had A Wrong Format (Missing \",\"), Auto Fixed, Your Values Are Kept");
                    return obj;
                }
                catch { }
            }
 
            Warn("config.json Had A Wrong Format, Auto Fixed, Every Readable Setting Is Kept");
            return Salvage(noComments);
        }
        static JsonObject Salvage(string text)
        {
            var known = new HashSet<string>(typeof(Config).GetProperties().Select(p => p.Name), StringComparer.OrdinalIgnoreCase);
            var obj = new JsonObject();
            var i = 0;
 
            while (i < text.Length)
            {
                if (text[i] != '"') { i++; continue; }
 
                var keyEnd = StringEnd(text, i);
                var key = text[(i + 1)..(keyEnd - 1)];
                var j = keyEnd;
 
                while (j < text.Length && char.IsWhiteSpace(text[j])) j++;
 
                if (!known.Contains(key) || j >= text.Length || text[j] != ':') { i = keyEnd; continue; }
 
                j++;
                while (j < text.Length && char.IsWhiteSpace(text[j])) j++;
 
                JsonNode? node = null;
 
                try { node = JsonNode.Parse(text[j..ValueEnd(text, j)].Trim()); } catch { }
 
                if (node != null) obj.TryAdd(key, node);
                else Warn($"\"{key}\" Is Not Readable, Default Value Will Be Used");
 
                i = j;
            }
 
            return obj;
        }
 
        static int StringEnd(string t, int i)
        {
            for (var j = i + 1; j < t.Length; j++)
                if (t[j] == '\\') j++;
                else if (t[j] == '"') return j + 1;
 
            return t.Length;
        }
 
        static int ValueEnd(string t, int i)
        {
            if (t[i] == '"') return StringEnd(t, i);
 
            if (t[i] is not ('{' or '['))
            {
                var k = i;
                while (k < t.Length && t[k] is not (',' or '\n' or '}' or ']')) k++;
                return k;
            }
 
            for (int j = i, depth = 0; j < t.Length; j++)
            {
                if (t[j] == '"') j = StringEnd(t, j) - 1;
                else if (t[j] is '{' or '[') depth++;
                else if (t[j] is '}' or ']' && --depth == 0) return j + 1;
            }
 
            return t.Length;
        }
 
        static IEnumerable<PropertyInfo> Props(object obj) => obj.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanRead && p.CanWrite);
 
        static void Walk(object? target, JsonObject? json, string path)
        {
            if (target == null || target is string) return;
            if (target is System.Collections.IDictionary map) { foreach (var v in map.Values) Walk(v, null, path); return; }
            if (target is System.Collections.IEnumerable list) { foreach (var v in list) Walk(v, null, path); return; }
            if (!target.GetType().IsClass) return;
 
            object? defaults = null;
 
            foreach (var p in Props(target))
            {
                var name = path + p.Name;
                var type = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;
                var node = json?.FirstOrDefault(kv => string.Equals(kv.Key, p.Name, StringComparison.OrdinalIgnoreCase)).Value;
 
                if (node is JsonObject child && p.GetValue(target) is { } nested && type.IsClass && type != typeof(string) && nested is not System.Collections.IEnumerable)
                {
                    Walk(nested, child, name + " -> ");
                }
                else if (node != null)
                {
                    try { if (node.Deserialize(p.PropertyType, Opts) is { } value) p.SetValue(target, value); }
                    catch { Warn($"\"{name}\": {node.ToJsonString()} Is Not Valid, Kept Default ({p.GetValue(target)})"); }
                }
 
                var val = p.GetValue(target);
 
                if (p.GetCustomAttribute<StringAttribute>() is { } str && type == typeof(string))
                {
                    defaults ??= Activator.CreateInstance(target.GetType());
                    p.SetValue(target, FixKeys(val as string, str.Keys, p.Name, defaults == null ? null : p.GetValue(defaults) as string));
                }
                else if (p.GetCustomAttribute<RangeAttribute>() is { } range && val is IConvertible num)
                {
                    var d = num.ToDouble(null);
                    if (d < range.Min || d > range.Max)
                    {
                        defaults ??= Activator.CreateInstance(target.GetType());
 
                        var back = defaults == null ? Convert.ChangeType(Math.Clamp(d, range.Min, range.Max), type) : p.GetValue(defaults);
                        p.SetValue(target, back);
 
                        Warn($"\"{name}\": {d} Is Not Valid, Changed To Default ({back}) (Allowed {range.Min} To {range.Max})");
 
                        foreach (var c in p.GetCustomAttributes<CommentAttribute>().Where(c => Regex.IsMatch(c.Text.Trim(), @"^-?\d+\s*=")))
                            Warn("   " + c.Text.Trim());
                    }
                }
                else if (node == null || val is System.Collections.IEnumerable) Walk(val, null, name + " -> ");
            }
        }
 
        static string Render(object obj, int indent)
        {
            var pad = new string(' ', indent);
            var blocks = new List<(string Text, bool IsProp)>();
 
            foreach (var p in Props(obj))
            {
                var infos = p.GetCustomAttributes<InfoAttribute>().ToList();
                var lines = new List<string>();
 
                if (p.GetCustomAttribute<BreakLineAttribute>() is { } br) lines.AddRange(Comments(br.Text, pad));
                foreach (var i in infos) lines.AddRange(Comments(i.Key switch { "Version" => Version, "Github" => Github, _ => i.Key }, pad));
                foreach (var c in p.GetCustomAttributes<CommentAttribute>()) lines.AddRange(Comments(c.Text, pad));
 
                var val = infos.Count > 0 ? null : p.GetValue(obj);
                if (val != null) lines.Add($"{pad}\"{p.Name}\": {Value(val, indent)}");
 
                if (lines.Count > 0) blocks.Add((string.Join("\n", lines), val != null));
            }
 
            var last = blocks.FindLastIndex(b => b.IsProp);
            return string.Join("\n\n", blocks.Select((b, i) => b.IsProp && i < last ? b.Text + "," : b.Text));
        }
 
        static string Value(object val, int indent)
        {
            var pad = new string(' ', indent);
 
            if (val is not string && val is not System.Collections.IEnumerable && val.GetType().IsClass)
                return $"\n{pad}{{\n{Render(val, indent + 2)}\n{pad}}}";
 
            var json = JsonSerializer.Serialize(val, Pretty).Split('\n');
            return string.Join("\n", json.Select((l, i) => (i == 0 ? "" : pad) + l.TrimEnd()));
        }
 
        static IEnumerable<string> Comments(string? text, string pad)
        {
            if (string.IsNullOrWhiteSpace(text)) yield break;
 
            foreach (var raw in text.Replace("\r", "").Split('\n'))
            {
                var t = raw.Trim();
                var before = t.StartsWith("{nextline}");
                var after = t.EndsWith("{nextline}");
 
                t = t.Replace("{nextline}", "").Trim();
 
                if (t.Length == 0) { yield return before || after ? "" : pad + "//"; continue; }
 
                if (before) yield return "";
                yield return pad + "// " + t;
                if (after) yield return "";
            }
        }
 
        public static string GetStringValue(string? input, string key) =>
            Split(input, new[] { key }).FirstOrDefault(s => string.Equals(s.Key, key, StringComparison.OrdinalIgnoreCase)).Val?.Trim() ?? "";
 
        public static string FixString(string? value, Format format, string name = "Value") => FixKeys(value, Formats.Get(format), name);
 
        public static string FixKeys(string? current, string[] keys, string name = "Value", string? fallback = null)
        {
            var segs = Split(current, keys);
            var used = segs.Any(s => s.Has);
 
            if (!used)
            {
                var empty = fallback ?? string.Join(" | ", keys.Select(k => $"{k}: "));
 
                if (empty != (current ?? ""))
                {
                    Warn($"\"{name}\": \"{current}\" Is Empty, Changed To Default:");
                    Warn($"   \"{empty}\"");
                }
 
                return empty;
            }
 
            var parts = keys.Select((key, i) =>
            {
                var hit = segs.FindIndex(s => string.Equals(s.Key, key, StringComparison.OrdinalIgnoreCase));
                if (hit >= 0) return $"{key}: {segs[hit].Val}";
 
                var renamed = i < segs.Count && segs[i].Has && !keys.Any(k => string.Equals(segs[i].Key, k, StringComparison.OrdinalIgnoreCase));
                return $"{key}: {(renamed ? segs[i].Val : "")}";
            });
 
            var result = string.Join(" | ", parts);
            if (used && result != (current ?? ""))
            {
                Warn($"\"{name}\": \"{current}\" Is Not Valid, Changed To:");
                Warn($"   \"{result}\"");
            }
            return result;
        }
 
        static List<(string Key, string Val, bool Has)> Split(string? input, string[] keys) =>
            Regex.Split(input ?? "", @"\s*\|\s*|\s*(?=\b(?:" + string.Join("|", keys.Select(Regex.Escape)) + @")\s*:)", RegexOptions.IgnoreCase)
                 .Where(s => s.Length > 0)
                 .Select(s =>
                 {
                     var i = s.IndexOf(':');
                     if (i < 0) return ("", s.Trim(), false);
 
                     var val = s[(i + 1)..].TrimEnd();
                     return (s[..i].Trim(), val.StartsWith(" ") ? val[1..] : val, true);
                 }).ToList();
    }
}