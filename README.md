# Welcome to Among Us Revamped!
<br>

</p>
<p align="center">

<center>
<a href="https://discord.gg/83Zhzhyhya" target="_blank"><img src="https://img.shields.io/badge/Discord%20-%231DA1F2.svg?&style=for-the-badge&logo=discord&logoColor=white&color=5662f6"/></a>
<a href="https://github.com/apemv/AmongUsRevamped/releases/latest" target="_blank"><img src="https://img.shields.io/badge/Latest Version%20-%231DA1F2.svg?&style=for-the-badge&logo=github&logoColor=white&color=181717"/></a>
</center>



</p>

# 💡 How To Install:
- <b>If you have BepInEx, download AUR.dll in releases and move it into Among Us → BepinEx → Plugins</b>
- <b> If you don't have BepInEx:</b>
- Download the latest .zip file in the releases tab
- Extract the contents of the zip into your Among Us folder
- Paste the copied files directly into your Among Us game folder
- Shown below are the folder locations:
   - **Steam:** Right-click Among Us in your `Library` → `Manage` → `Browse local files`
   - **Epic Launcher:** Right-click Among Us in your Library → `Manage` → Click the folder icon in the Installation box
   - **Itch.io:** Open the Itch.io app → Right-click Among Us in your Library → `Manage` → `Open folder in Explorer`
   - **Xbox App:** Open the Xbox app → Right-click Among Us in your library → `Manage` → `Files` → `Browse...`
   - **Microsoft Store:** Check [this support article](https://answers.microsoft.com/en-us/xbox/forum/all/where-can-i-find-the-gamefiles-of-a-game/5cb9a0c3-7948-4316-abc5-f27d1767b932) on how to find and access your Among Us folder.
 - Your game folder should look similar to this after installation:
<img src="https://github.com/astra1dev/AUnlocker/assets/90265231/14226f03-a003-4efc-b27b-6df53fb394d6" width=410 height=240>****
‎ 

# 🎮 Features (v1.8.0):
## <b> 🌟 Custom Abilities:</b>
- Mayor (Crewmate)
- The Mayor has a configurable amount of extra votes
- Jester (Crewmate)
- The Jester wins alone by getting ejected, can win with Crewmates
- Tasker (Crewmate)
- The Crewmates win if the Tasker completes tasks and is alive
- Workhorse (Crewmate)
- The Workhorse gains additional votes per completed task
- Tyrant (Impostor)
- The Tyrant has a configurable amount of extra votes
- Stealer (Impostor)
- The Stealer gains additional votes per kill
- Juggernaut (Impostor)
- The Impostors win if the Juggernaut gains a set amount of kills
- For the enabled ability list, type /r
- To check ability descriptions, type /r {ability}

## <b>♠️ Client Side settings:</b>
- Game Master (Spectator)
- Show FPS
- Dark Theme
- Toggle Lobby Music
- Show/Hide Info When Dead
## <b>⚙️ Technical settings:</b>
- Kick/Ban players under a certain level
- Kick/Ban invalid FriendCodes
- Kick/Ban players who say "start"
- Denyword system
- Banlist system
- Denyname system
- Multi layer moderator system
- To add Admins, Moderators and VIP's, type the user's Friendcode in the designated files located in AUR-DATA
- Enable No Game End
- Custom starting countdown
## <b>♻️ Automation settings:</b>
- Automatically start game
- Automatically rejoin lobby
- Automatically send winner info
- Enable (configurable) Welcome Messages
- The Welcome Template is in Templates.txt
## <b>🔧 Gameplay Settings:</b>
- Dead impostors can sabotage
- Disable critical sabotages (invividual)
- Disable door sabotages
- Disable meeting calls first 30s
- Disable devices (per map, individual)
- Override decontamination time (per map)
- Disable spore triggers on The Fungle
- % Tasks completed for Crewmates to win
- Enable/Disable any task
- All players have same tasks
- Hide and Seek: Custom Seeker count
## <b>🏆 Custom Gamemodes:</b>
- 0 Kill Cooldown
- Shift and Seek
- Speedrun
## <b>❓ Other Improvements:</b>
- Command Helper
- Creatable and customizable Templates
- Eht Dleks (Reverse Skeld) Map
- Ability to cancel starting countdown
- Improved main menu
- Faster startup time
- Custom anticheat system
- Zoom out in lobby or when dead
- See player kill/task count and Role when dead
- Higher and more accurate Option ranges
- Always visible lobby timer
- <b>🎉 And this all works in public lobbies!</b>

### Hotkeys
| HotKey              | Function                                    | Usable Scene                       |
| ------------------- | ------------------------------------------- | ---------------------------------- |
| `Shift`+`L`+`Enter` | Force End Game                              | In Game                            |
| `Shift`+`M`+`Enter` | Skip meeting when active                    | In Game                            |
| `C`                 | Cancel game start                           | In Starting Countdown              |
| `Shift`             | Start the game immediately                  | In Starting Countdown              |
| `Shift`             | 5x Option increment                         | In Options Menu                    |
| `Ctrl`              | Noclip in lobby                             | In Lobby                           |
| `Ctrl`              | 10x Option increment                        | In Options Menu                    |

### Chat Commands
| Command                                     | Function                                          |
| ------------------------------------------- | ------------------------------------------------- |
| /help                                       | Show command help                                 |
| /l<br>/lastgame                             | Send last game info                               |
| /r                                          | Send current gamemode/roles description           |
| /r {ability}                                | Send specific Ability info                        |
| /0kc<br>/sns</br>/sp                        | Send respective gamemode description              |
| /ban {name}                                 | Ban a player by name                              |
| /kick {name}                                | Kick a player by name                             |
| /cban {color}                               | Ban a player by color                             |
| /ckick {color}                              | Kick a player by color                            |
| /dump                                       | Copy your LogOutput file                          |
| /eg<br>/endgame                             | Instantly end an ongoing game                     |
| /em<br>/endmeeting                          | Instantly end an ongoing meeting                  |
| /aur<br>/socials                            | Send AUR socials (GitHub, Discord)                |
| /col<br>/color                              | Set color. Can be used to bypass color limit      | 
| /reload                                     | Reset the *Language*.JSON file to default         |
| /s<br>/start                                | Start game countdown                              |
| /vip<br>/moderator<br>/admin                | Add a user to respective list by name             |
| /t {template}                               | Send a Template from Templates.txt                |
‎

**Credit to these mods and their developers for their code:**
### :star: [AUnlocker](https://github.com/astra1dev/AUnlocker)
> - Original NumberOptionsPatch
> - Always Show Lobby Timer
### :star: [EHR (Formerly TOHE+)](https://github.com/Gurge44/EndlessHostRoles)
> - Task Patching
### :star: [TOH](https://github.com/tukasa0001/TownOfHost) :
> - Ban Manager
> - OptionHolder & ClientOptionItem
### :star: [Super New Roles](https://github.com/ykundesu/SuperNewRoles) (SNR):
> - Credentials menu
### :star: [TOHY](https://github.com/Yumenopai/TownOfHost_Y) :
> - Zoom
### :star: [MoreGamemodes](https://github.com/Rabek009/MoreGamemodes)
> - Some Chat Patches
> - OptionItem System
---
This project is licensed under the GNU General Public License version 3.0. For more details, please refer to the [LICENSE](https://github.com/apemv/AmongUsRevamped/blob/main/LICENSE) file.
---

<b>This mod is not affiliated with Among Us or Innersloth LLC, and the content contained therein is not endorsed or otherwise sponsored by Innersloth LLC. Portions of the materials contained herein are property of Innersloth LLC. © Innersloth LLC.</b>
