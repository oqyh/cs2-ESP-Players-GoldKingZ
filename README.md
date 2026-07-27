## .:[ Join Our Discord For Support ]:.

<a href="https://discord.com/invite/U7AuQhu"><img src="https://discord.com/api/guilds/651838917687115806/widget.png?style=banner2"></a>

# [CS2] ESP-Players-GoldKingZ (1.0.3)

Show Glow/Esp To Players With Flags

![glowesp](https://github.com/user-attachments/assets/8a954561-5aca-4a43-bd3a-0de2f1a0a8e3)


---

## 📦 Dependencies

[![Metamod:Source](https://img.shields.io/badge/Metamod:Source-REQUIRED_TO_DOWNLOAD-red?logo=sourceengine&labelColor=2d2d2d)](https://www.sourcemm.net)

[![CounterStrikeSharp](https://img.shields.io/badge/CounterStrikeSharp-REQUIRED_TO_DOWNLOAD-red?logo=github&labelColor=83358F)](https://github.com/roflmuffin/CounterStrikeSharp)

[![ClientPrefs-GoldKingZ](https://img.shields.io/badge/ClientPrefs--GoldKingZ-REQUIRED_TO_DOWNLOAD-red?logo=github&labelColor=FFD700)](https://github.com/oqyh/cs2-ClientPrefs-GoldKingZ/releases)

[![JSON](https://img.shields.io/badge/JSON-INCLUDED_IN_ZIP-brightgreen?logo=json&labelColor=000000)](https://www.newtonsoft.com/json)

---

## 📥 Installation

### Plugin Installation
1. Download the latest `ESP-Players-GoldKingZ.x.x.x.zip` release
2. Extract contents to your `csgo` directory
3. Configure settings in `ESP-Players-GoldKingZ/config/config.json`
4. Restart your server

---


## ⚙️ Configuration

> [!IMPORTANT]
> **Main Configuration**  
> `../ESP-Players-GoldKingZ/config/config.json`  

## 🛠️ `config/config.json`
<details open>
<summary><b>Main Config</b> (Click to expand 🔽)</summary>
  
| Property | Description | Values | Required |
|----------|-------------|--------|----------|
| `Reload_Plugin_CommandsInGame` | Commands to reload the plugin (console/chat by `!` or `css_`) | `Console_Commands:` `Chat_Commands:`<br>Both empty = Disable | - |
| `Reload_Plugin_Flags` | Restrict reload command to SteamIDs, Flags, Groups | `SteamIDs:` `Flags:` `Groups:`<br>All empty = Allow everyone | `Reload_Plugin_CommandsInGame` |
| `Reload_Plugin_Hide` | Hide chat after executing reload command | `0`-No<br>`1`-Only after successful toggle<br>`2`-Hide all the time | `Reload_Plugin_Flags` |
| `Give_ESP_CommandsInGame` | Commands to give ESP to players on/off (console/chat by `!` or `css_`) | `Console_Commands:` `Chat_Commands:`<br>Both empty = Disable | - |
| `Give_ESP_Flags` | Restrict give command to SteamIDs, Flags, Groups | `SteamIDs:` `Flags:` `Groups:`<br>All empty = Allow everyone | `Give_ESP_CommandsInGame` |
| `Give_ESP_Hide` | Hide chat after executing give command | `0`-No<br>`1`-Only after successful toggle<br>`2`-Hide all the time | `Give_ESP_Flags` |
| `Give_ESP_SaveToPrefs` | Save ESP given by admins into ClientPrefs | `true`-Yes (keeps after reconnect/map change)<br>`false`-No (session only) | `Cookies_Enable/MySql_Enable` |
| `Toggle_ESP_CommandsInGame` | Commands to toggle ESP on/off (console/chat by `!` or `css_`) | `Console_Commands:` `Chat_Commands:`<br>Both empty = Disable | - |
| `Toggle_ESP_Flags` | Restrict toggle command to SteamIDs, Flags, Groups | `SteamIDs:` `Flags:` `Groups:`<br>All empty = Allow everyone | `Toggle_ESP_CommandsInGame` |
| `Toggle_ESP_Hide` | Hide chat after executing toggle command | `0`-No<br>`1`-Only after successful toggle<br>`2`-Hide all the time | `Toggle_ESP_Flags` |
| `DefaultToggleGlow` | Default glow toggle for new players | `true`-On<br>`false`-Off (must toggle manually) | - |
| `DisableOnWarmUp` | Disable ESP during warmup | `true`/`false` | - |
| `DisableGlowOnGOTV` | Disable glow in demo GOTV/HLTV | `true`/`false` | - |
| `Show_ESP_For` | Who can see ESP | `0`-Any<br>`1`-Dead players only<br>`2`-Spec players only | - |
| `ShowOnlyEnemyTeam` | Show ESP only for enemy team | `true`-Yes (disable teammate ESP)<br>`false`-No (show all) | `Show_ESP_For=0 or 1` |
 
</details>
<details>
<summary><b>Glow Config</b> (Click to expand 🔽)</summary>
  
| Property | Description | Values | Required |
|----------|-------------|--------|----------|
| `GlowType` | Glow only when crosshair is near a player | `true`-Yes<br>`false`-No (show all the time) | - |
| `GlowRange` | Max range to show player glow | e.g. `5000` | - |
| `Glow_Color_CT` | Glow color for CT players (R, G, B, A) | e.g. `0, 190, 255, 255`<br>[Color Picker](https://rgbacolorpicker.com/) | - |
| `Glow_Color_T` | Glow color for T players (R, G, B, A) | e.g. `243, 0, 93, 255`<br>[Color Picker](https://rgbacolorpicker.com/) | - |
 
</details>
<details>
<summary><b>Locally Config</b> (ClientPrefs-GoldKingZ API) (Click to expand 🔽)</summary>
  
| Property | Description | Values | Required |
|----------|-------------|--------|----------|
| `Cookies_Enable` | Save player data locally by cookies | `0`-No<br>`1`-On disconnect (Warning Performance)<br>`2`-On map change (Recommended) | - |
| `Cookies_AutoRemoveInactivePlayersOlderThanDays` | Auto delete inactive players (days) | `0`-Don't delete<br>`1`+ days | `Cookies_Enable=1 or 2` |
 
</details>
<details>
<summary><b>MySql Config</b> (ClientPrefs-GoldKingZ API) (Click to expand 🔽)</summary>
  
| Property | Description | Values | Required |
|----------|-------------|--------|----------|
| `MySql_Enable` | Save player data to MySQL | `0`-No<br>`1`-On disconnect (Warning Performance)<br>`2`-On map change (Recommended) | - |
| `MySql_ConnectionTimeout` | Connection timeout (seconds) | e.g. `30` | `MySql_Enable=1 or 2` |
| `MySql_RetryAttempts` | Retry attempts on connection failure | e.g. `3` | `MySql_Enable=1 or 2` |
| `MySql_RetryDelay` | Delay between retries (seconds) | e.g. `2` | `MySql_Enable=1 or 2` |
| `MySql_Servers` | MySQL server configurations (add as many as you like) | Array of server objects<br>(`Server`, `Port`, `Database`, `Username`, `Password`) | `MySql_Enable=1 or 2` |
| `MySql_AutoRemoveInactivePlayersOlderThanDays` | Auto delete inactive players (days) | `0`-Don't delete<br>`1`+ days | `MySql_Enable=1 or 2` |
 
</details>
<details>
<summary><b>Utilities Config</b> (Click to expand 🔽)</summary>
  
| Property | Description | Values | Required |
|----------|-------------|--------|----------|
| `EnableDebug` | Enable debug in server console (helps debug issues) | `true`/`false` | - |
 
</details>

---


## 📜 Changelog

<details>
<summary><b>📋 View Version History</b> (Click to expand 🔽)</summary>

### [1.0.3]
- Reorganize config.json Layout
- Added Give_ESP_CommandsInGame (Give ESP To Players On/Off)
- Added Give_ESP_Flags
- Added Give_ESP_Hide
- Added Give_ESP_SaveToPrefs (Save ESP Given By Admins Into ClientPrefs)
- Change Lang `PrintToChatToPlayer.Toggle.Not.Allowed` To `PrintToChatToPlayer.Toggle.ESP.Not.Allowed`
- Change Lang `PrintToChatToPlayer.Toggle.Enabled` To `PrintToChatToPlayer.Toggle.ESP.Enabled`
- Change Lang `PrintToChatToPlayer.Toggle.Disabled` To `PrintToChatToPlayer.Toggle.ESP.Disabled`



### [1.0.2]
- Upgraded to .NET 10
- CleanUp + Optimization
- Fix DisableGlowOnGOTV Bug On Demo
- Remove Debug From Release For Optimization
- Remove UserTimerCheckPlayersGlow Its Default Using Refresh Timer
- Added API ClientPrefs-GoldKingZ 1.0.3 Into Plugin
- Added Reload_Plugin_CommandsInGame
- Added Reload_Plugin_Flags
- Added Reload_Plugin_Hide
- Added ConVar `gkz_esp` To Enable/Disable Plugin Can Execute Only By Server Side
- Rename Toggle_Glow_CommandsInGame To Toggle_ESP_CommandsInGame
- Rename Toggle_Glow_Flags To Toggle_ESP_Flags
- Rename Toggle_Glow_Hide To Toggle_ESP_Hide
- Rename SQL + MySQL Config By Using ClientPrefs-GoldKingZ API

### [1.0.1]
- Includ Missing Config Folder In Repository
- Some Rework
- Fix Some Bugs
- Fix Config.json
- Fix Toggle_Glow_Flags CounterStrikeSharp Excluding Root By Default
- Fix Glow_Color_CT Now Support alpha (Red, Green, Blue, Alpha)
- Fix Glow_Color_T Now Support alpha (Red, Green, Blue, Alpha)
- Added DisableGlowOnGOTV
- Added Show_ESP_For (Dead Players Only , Spec Players Only)
- Added Toggle_Glow_Hide

### [1.0.0]
- Initial plugin release

</details>

---
