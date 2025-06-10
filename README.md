## .:[ Join Our Discord For Support ]:.

<a href="https://discord.com/invite/U7AuQhu"><img src="https://discord.com/api/guilds/651838917687115806/widget.png?style=banner2"></a>

# [CS2] ESP-Players-GoldKingZ (1.0.1)

Show Glow/Esp To Players With Flags

![glowesp](https://github.com/user-attachments/assets/8a954561-5aca-4a43-bd3a-0de2f1a0a8e3)


---

## 📦 Dependencies
[![Metamod:Source](https://img.shields.io/badge/Metamod:Source-2d2d2d?logo=sourceengine)](https://www.sourcemm.net)

[![CounterStrikeSharp](https://img.shields.io/badge/CounterStrikeSharp-83358F)](https://github.com/roflmuffin/CounterStrikeSharp)

[![MySQL](https://img.shields.io/badge/MySQL-4479A1?logo=mysql&logoColor=white)](https://dev.mysql.com/doc/connector-net/en/) [Included in zip]

[![JSON](https://img.shields.io/badge/JSON-000000?logo=json)](https://www.newtonsoft.com/json) [Included in zip]


---

## 📥 Installation

### Plugin Installation
1. Download the latest `ESP-Players-GoldKingZ.x.x.x.zip` release
2. Extract contents to your `csgo` directory
3. Configure settings in `ESP-Players-GoldKingZ/config/config.json`
4. Restart your server

---

## 🛠️ `config.json`


<details open>
<summary><b>Main Config</b> (Click to expand 🔽)</summary>

| Property | Description | Values | Required |
|:---------|:------------|:-------|:---------|
| `DisableOnWarmUp` | Disable ESP On WarmUp? | `true` = Yes, `false` = No | - |
| `DisableGlowOnGOTV` | Disable Glow In Demo GOTV/HLTV? | `true` = Yes, `false` = No | - |
| `UserTimerCheckPlayersGlow` | Use Timer To Check Player Glow (Useful If Use Custom Models)? | `true` = Yes, `false` = No | - |
| `Show_ESP_For` | Show ESP For? | `0`-Any<br>`1`-Dead Players Only<br>`2`-Spec Players Only | `Discord_WebHook` |  
| `ShowOnlyEnemyTeam` | Show ESP Only Enemy Team? | `true` = Only enemies, `false` = All players | `Show_ESP_For` = 0/1 |
| `GlowType` | Glow Only When Crosshair Near To Player Glow? | `true` = Near, `false` = Always visible | - |
| `GlowRange` | Max Range To Show Player Glow | Number (e.g. `5000`) | - |
| `Glow_Color_CT` | Glow color for Counter-Terrorists | (Red, Green, Blue, Alpha) color (e.g. `0, 190, 255, 255`) | - |
| `Glow_Color_T` | Glow color for Terrorists | (Red, Green, Blue, Alpha) color (e.g. `243, 0, 93, 255`) | - |
| `DefaultToggleGlow` | Default Glow To New Players? | `true` = Yes, `false` = No | - |
| `Toggle_Glow_CommandsInGame` | Commands To Enable/Disable ESP | e.g. `!esp,!glow`<br>`""` = Disable | - |
| `Toggle_Glow_Flags` | Access control (SteamID / Flag / Group) | See example below | `Toggle_Glow_CommandsInGame` ≠ `""` |
| `Toggle_Glow_Hide` | Hide Chat After Toggle? | `0`-No<br>`1`-Yes, But Only After Toggle Successfully<br>`2`-Yes, Hide All The Time | `Toggle_Glow_CommandsInGame` ≠ `""` |
| `Cookies_Enable` | Save player data locally with cookies? | `true` = Yes, `false` = No | - |
| `Cookies_AutoRemovePlayerOlderThanXDays` | Auto-delete inactive cookies after X days | Number (`0` = Never) | `Cookies_Enable=true` |
| `MySql_Enable` | Save player data to MySQL database? | `true` = Yes, `false` = No | - |
| `MySql_Host` | MySQL server hostname | Text (e.g. `localhost`) | `MySql_Enable=true` |
| `MySql_Database` | MySQL database name | Text | `MySql_Enable=true` |
| `MySql_Username` | MySQL username | Text | `MySql_Enable=true` |
| `MySql_Password` | MySQL password | Text | `MySql_Enable=true` |
| `MySql_Port` | MySQL port | Number (e.g. `3306`) | `MySql_Enable=true` |
| `MySql_AutoRemovePlayerOlderThanXDays` | Auto-remove inactive MySQL entries after X days | Number (`0` = Never) | `MySql_Enable=true` |

---

### 🔹 `Toggle_Glow_Flags` Example

```text
SteamID: 76561198206086993,76561198974936845 | Flag: @css/vips,@css/admins | Group: #css/vips,#css/admins

```

</details>

<details>
<summary><b>Utilities Config</b> (Click to expand 🔽)</summary>

| Property | Description | Values | Required |  
|----------|-------------|--------|----------|
| `EnableDebug` | Debug Mode | `true`-Enable<br>`false`-Disable | - |

</details>

---


## 📜 Changelog

<details>
<summary><b>📋 View Version History</b> (Click to expand 🔽)</summary>

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
