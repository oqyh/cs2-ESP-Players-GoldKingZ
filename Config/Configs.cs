using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Text;

namespace ESP_Players.Config
{
    [AttributeUsage(AttributeTargets.Property)]
    public class RangeAttribute : Attribute
    {
        public int Min { get; }
        public int Max { get; }
        public int Default { get; }
        public string Message { get; }

        public RangeAttribute(int min, int max, int defaultValue, string message)
        {
            Min = min;
            Max = max;
            Default = defaultValue;
            Message = message;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class CommentAttribute : Attribute
    {
        public string Comment { get; }

        public CommentAttribute(string comment)
        {
            Comment = comment;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class BreakLineAttribute : Attribute
    {
        public string BreakLine { get; }

        public BreakLineAttribute(string breakLine)
        {
            BreakLine = breakLine;
        }
    }
    public static class Configs
    {
        private static readonly string ConfigDirectoryName = "config";
        private static readonly string ConfigFileName = "config.json";

        private static string? _configFilePath;
        private static ConfigData? _configData;

        private static readonly JsonSerializerOptions SerializationOptions = new()
        {
            Converters =
            {
                new JsonStringEnumConverter()
            },
            WriteIndented = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
        };

        public static bool IsLoaded()
        {
            return _configData is not null;
        }

        public static ConfigData GetConfigData()
        {
            if (_configData is null)
            {
                throw new Exception("Config not yet loaded.");
            }
            
            return _configData;
        }

        public static ConfigData Load(string modulePath)
        {
            var configFileDirectory = Path.Combine(modulePath, ConfigDirectoryName);
            if(!Directory.Exists(configFileDirectory))
            {
                Directory.CreateDirectory(configFileDirectory);
            }

            _configFilePath = Path.Combine(configFileDirectory, ConfigFileName);
            var defaultConfig = new ConfigData();
            if (File.Exists(_configFilePath))
            {
                try
                {
                    _configData = JsonSerializer.Deserialize<ConfigData>(File.ReadAllText(_configFilePath), SerializationOptions);
                }
                catch (JsonException)
                {
                    _configData = MergeConfigWithDefaults(_configFilePath, defaultConfig);
                }
                
                _configData!.Validate();
            }
            else
            {
                _configData = defaultConfig;
                _configData.Validate();
            }

            SaveConfigData(_configData);
            return _configData;
        }

        private static ConfigData MergeConfigWithDefaults(string path, ConfigData defaults)
        {
            var mergedConfig = new ConfigData();
            var jsonText = File.ReadAllText(path);
            
            var readerOptions = new JsonReaderOptions 
            { 
                AllowTrailingCommas = true,
                CommentHandling = JsonCommentHandling.Skip 
            };

            using var doc = JsonDocument.Parse(jsonText, new JsonDocumentOptions
            {
                AllowTrailingCommas = true,
                CommentHandling = JsonCommentHandling.Skip
            });
            
            foreach (var jsonProp in doc.RootElement.EnumerateObject())
            {
                var propInfo = typeof(ConfigData).GetProperty(jsonProp.Name);
                if (propInfo == null) continue;

                try
                {
                    var jsonValue = JsonSerializer.Deserialize(
                        jsonProp.Value.GetRawText(), 
                        propInfo.PropertyType,
                        new JsonSerializerOptions
                        {
                            Converters = { new JsonStringEnumConverter() },
                            ReadCommentHandling = JsonCommentHandling.Skip
                        }
                    );
                    propInfo.SetValue(mergedConfig, jsonValue);
                }
                catch (JsonException)
                {
                    propInfo.SetValue(mergedConfig, propInfo.GetValue(defaults));
                }
            }
            
            return mergedConfig;
        }

        private static void SaveConfigData(ConfigData configData)
        {
            if (_configFilePath is null)
                throw new Exception("Config not yet loaded.");

            var json = JsonSerializer.Serialize(configData, SerializationOptions);
            
            var lines = json.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var newLines = new List<string>();

            foreach (var line in lines)
            {
                var match = Regex.Match(line, @"^\s*""(\w+)""\s*:.*");
                bool isPropertyLine = false;
                PropertyInfo? propInfo = null;

                if (match.Success)
                {
                    string propName = match.Groups[1].Value;
                    propInfo = typeof(ConfigData).GetProperty(propName);

                    var breakLineAttr = propInfo?.GetCustomAttribute<BreakLineAttribute>();
                    if (breakLineAttr != null)
                    {
                        string breakLine = breakLineAttr.BreakLine;

                        if (breakLine.Contains("{space}"))
                        {
                            breakLine = breakLine.Replace("{space}", "").Trim();

                            if (breakLineAttr.BreakLine.StartsWith("{space}"))
                            {
                                newLines.Add("");
                            }

                            newLines.Add("// " + breakLine);
                            newLines.Add("");
                        }
                        else
                        {
                            newLines.Add("// " + breakLine);
                        }
                    }

                    var commentAttr = propInfo?.GetCustomAttribute<CommentAttribute>();
                    if (commentAttr != null)
                    {
                        var commentLines = commentAttr.Comment.Split('\n');
                        foreach (var commentLine in commentLines)
                        {
                            newLines.Add("// " + commentLine.Trim());
                        }
                    }

                    isPropertyLine = true;
                }

                newLines.Add(line);

                if (isPropertyLine && propInfo?.GetCustomAttribute<CommentAttribute>() != null)
                {
                    newLines.Add("");
                }
            }

            var adjustedLines = new List<string>();
            foreach (var line in newLines)
            {
                adjustedLines.Add(line);
                if (Regex.IsMatch(line, @"^\s*\],?\s*$"))
                {
                    adjustedLines.Add("");
                }
            }

            File.WriteAllText(_configFilePath, string.Join(Environment.NewLine, adjustedLines), Encoding.UTF8);
        }

        public class ConfigData
        {
            private string? _Version;
            private string? _Link;
            [BreakLine("----------------------------[ ↓ Plugin Info ↓ ]----------------------------{space}")]
            public string Version
            {
                get => _Version!;
                set
                {
                    _Version = value;
                    if (_Version != MainPlugin.Instance.ModuleVersion)
                    {
                        Version = MainPlugin.Instance.ModuleVersion;
                    }
                }
            }

            public string Link
            {
                get => _Link!;
                set
                {
                    _Link = value;
                    if (_Link != "https://github.com/oqyh/cs2-ESP-Players-GoldKingZ")
                    {
                        Link = "https://github.com/oqyh/cs2-ESP-Players-GoldKingZ";
                    }
                }
            }

            [BreakLine("{space}----------------------------[ ↓ Main Config ↓ ]----------------------------{space}")]
            [Comment("Disable ESP On WarmUp?\ntrue = Yes\nfalse = No")]
            public bool DisableOnWarmUp { get; set; }
            
            [Comment("Disable Glow In Demo GOTV/HLTV?\ntrue = Yes\nfalse = No")]
            public bool DisableGlowOnGOTV { get; set; }

            [Comment("Use Timer To Check Player Glow (Useful If Use Custom Models)?\ntrue = Yes\nfalse = No")]
            public bool UserTimerCheckPlayersGlow { get; set; }

            [Comment("Show ESP For?\n0 = Any\n1 = Dead Players Only\n2 = Spec Players Only")]
            [Range(0, 2, 0, "[ESP Players] Show_ESP_For: is invalid, setting to default value (0) Please Choose From 0 To 2.\n[ESP Players] 0 = Any\n[ESP Players] 1 = Dead Players\n[ESP Players] 2 = Spec Players")]
            public int Show_ESP_For { get; set; }

            [Comment("Required [Show_ESP_For = 0/1]\nShow ESP Only Enemy Team?\ntrue = Yes (Disable Teammate ESP)\nfalse = No (Show All)")]
            public bool ShowOnlyEnemyTeam { get; set; }

            [Comment("Glow Only When Crosshair Near To Player Glow?\ntrue = Yes\nfalse = No (Show All The Time)")]
            public bool GlowType { get; set; }

            [Comment("Whats Max Range To Show Player Glow")]
            public int GlowRange { get; set; }

            [Comment("How Would You Like Glow Color Counter Terrorist (CT) Players By (Red, Green, Blue, Alpha) Use This Site [https://rgbacolorpicker.com/]")]
            public string Glow_Color_CT { get; set; }

            [Comment("How Would You Like Glow Color Terrorist (T) Players By (Red, Green, Blue, Alpha) Use This Site [https://rgbacolorpicker.com/]")]
            public string Glow_Color_T { get; set; }

            [Comment("Default Glow To New Players?\ntrue = On\nfalse = Off")]
            public bool DefaultToggleGlow { get; set; }

            [Comment("Commands To Enable/Disable ESP\nNote: If The Command Starts With '!' or 'css_' You Will Have The Ability To Toggle It On The Console And Chat\n\"\" = Disable")]
            public string Toggle_Glow_CommandsInGame { get; set; }

            [Comment("Required [Toggle_Glow_CommandsInGame]\nFlags Or Group Or SteamID To Enable/Disable ESP\nExample:\n\"SteamID: 76561198206086993,76561198974936845 | Flag: @css/vips,@css/admins | group:#css/vips,#css/admins\"\n\"\" = To Allow Everyone")]
            public string Toggle_Glow_Flags { get; set; }

            [Comment("Required [Toggle_Glow_CommandsInGame]\nHide Chat After Toggle?:\n0 = No\n1 = Yes, But Only After Toggle Successfully\n2 = Yes, Hide All The Time")]
            [Range(0, 2, 0, "[ESP Players] Toggle_Glow_Hide: is invalid, setting to default value (0) Please Choose From 0 To 2.\n[ESP Players] 0 = No\n[ESP Players] 1 = Yes, But Only After Toggle Successfully\n[ESP Players] 2 = Yes, Hide All The Time")]
            public int Toggle_Glow_Hide { get; set; }

            [BreakLine("{space}----------------------------[ ↓ Locally Config ↓ ]----------------------------{space}")]         
            [Comment("Save Players Data By Cookies Locally (In ../ESP-Players-GoldKingZ/cookies/)?\ntrue = Yes\nfalse = No")]
            public bool Cookies_Enable { get; set; }

            [Comment("Required [Cookies_Enable = true]\nAuto Delete Inactive Players More Than X (Days) Old\n0 = Dont Auto Delete")]
            public int Cookies_AutoRemovePlayerOlderThanXDays { get; set; }


            [BreakLine("{space}----------------------------[ ↓ MySql Config ↓ ]----------------------------{space}")]
            [Comment("Save Players Data Into MySql?\ntrue = Yes\nfalse = No")]
            public bool MySql_Enable { get; set; }

            [Comment("MySql Host")]
            public string MySql_Host { get; set; }

            [Comment("MySql Database")]
            public string MySql_Database { get; set; }

            [Comment("MySql Username")]
            public string MySql_Username { get; set; }

            [Comment("MySql Password")]
            public string MySql_Password { get; set; }

            [Comment("MySql Port")]
            public uint MySql_Port { get; set; }

            [Comment("Required [MySql_Enable = true]\nAuto Delete Inactive Players More Than X (Days) Old\n0 = Dont Auto Delete")]
            public int MySql_AutoRemovePlayerOlderThanXDays { get; set; }

            [BreakLine("{space}----------------------------[ ↓ Utilities  ↓ ]----------------------------{space}")]
            [Comment("Enable Debug Plugin In Server Console (Helps You To Debug Issues You Facing)?\ntrue = Yes\nfalse = No")]
            public bool EnableDebug { get; set; }

            public ConfigData()
            {
                Version = MainPlugin.Instance.ModuleVersion;
                Link = "https://github.com/oqyh/cs2-ESP-Players-GoldKingZ";

                DisableOnWarmUp = false;
                DisableGlowOnGOTV = false;
                UserTimerCheckPlayersGlow = false;
                Show_ESP_For = 0;
                ShowOnlyEnemyTeam = true;
                GlowType = false;
                GlowRange = 5000;
                Glow_Color_CT = "0, 190, 255, 255";
                Glow_Color_T = "243, 0, 93, 255";
                DefaultToggleGlow = false;
                Toggle_Glow_CommandsInGame = "!glow,!esp,css_esp,css_glow,!showplayers";
                Toggle_Glow_Flags = "SteamID: 76561198206086993,76561198974936845 | Flag: @css/vips,@css/admins | Group: #css/vips,#css/admins";
                Toggle_Glow_Hide = 0;

                Cookies_Enable = true;
                Cookies_AutoRemovePlayerOlderThanXDays = 7;

                MySql_Enable = false;
                MySql_Host = "MySql_Host";
                MySql_Database = "MySql_Database";
                MySql_Username = "MySql_Username";
                MySql_Password = "MySql_Password";
                MySql_Port = 3306;
                MySql_AutoRemovePlayerOlderThanXDays = 7;

                EnableDebug = false;
            }
            public void Validate()
            {
                foreach (var prop in GetType().GetProperties())
                {
                    var rangeAttr = prop.GetCustomAttribute<RangeAttribute>();
                    if (rangeAttr != null && prop.PropertyType == typeof(int))
                    {
                        int value = (int)prop.GetValue(this)!;
                        if (value < rangeAttr.Min || value > rangeAttr.Max)
                        {
                            prop.SetValue(this, rangeAttr.Default);
                            Helper.DebugMessage(rangeAttr.Message,false);
                        }
                    }
                }
            }
        }
    }
}
