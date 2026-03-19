<h1 align="center">
    Outward Mods Communicator Menu
</h1>
<br/>
<div align="center">
  <img src="https://raw.githubusercontent.com/GymMed/Outward-Mods-Communicator-Menu/refs/heads/main/preview/images/Logo.png" alt="Logo"/>
</div>

<div align="center">
	<a href="https://thunderstore.io/c/outward/p/GymMed/Mods_Communicator_Menu/">
		<img src="https://img.shields.io/thunderstore/dt/GymMed/Mods_Communicator_Menu" alt="Thunderstore Downloads">
	</a>
	<a href="https://github.com/GymMed/Outward-Mods-Communicator-Menu/releases/latest">
		<img src="https://img.shields.io/thunderstore/v/GymMed/Mods_Communicator_Menu" alt="Thunderstore Version">
	</a>
	<a href="https://github.com/GymMed/Outward-Mods-Communicator/releases/latest">
		<img src="https://img.shields.io/badge/Mods_Communicator-v1.2.0-9966ff" alt="Mods Communicator Version">
	</a>
	<a href="https://github.com/GymMed/Outward-Chat-Commands-Manager/releases/latest">
		<img src="https://img.shields.io/badge/Chat_Commands_Manager-v0.1.0-33ccff" alt="Chat Commands Manager Version">
	</a>
</div>

<div align="center">
	A visual in-game menu interface for the Outward Mods Communicator event system. Browse, inspect, and publish events directly from a user-friendly UI.
</div>

## Overview

**Outward Mods Communicator Menu** gives you a graphical interface to interact with the Mods Communicator event system. Instead of typing complex chat commands, you can use this menu to:

- Browse all registered events from every mod
- See which mods publish which events
- View which mods subscribe to events
- Send (publish) events with custom parameters
- Toggle the menu with a hotkey or chat command

## Requirements

This mod requires the following dependencies to be installed:

| Dependency | Purpose |
|------------|---------|
| [Outward Mods Communicator](https://github.com/GymMed/Outward-Mods-Communicator) | Core event bus system |
| [Chat Commands Manager](https://github.com/GymMed/Outward-Chat-Commands-Manager) | Enables `/MCMenu` command |
| SideLoader | Keybinding support |
| [UniverseLib](https://github.com/sinai-dev/UniverseLib) | UI framework |

## How to Use

### Opening the Menu

You can open the menu in two ways:

1. **Chat Command**: Type `/MCMenu` in the in-game chat
2. **Keyboard Shortcut**: Press your configured hotkey (needs to be set through settings)

### Menu Tabs

The menu has three main tabs:

<details>
<summary><strong>Publish Tab</strong></summary>

<details>
<summary>
Use this tab to send events to mods. 
</summary>
  <img src="https://raw.githubusercontent.com/GymMed/Outward-Mods-Communicator-Menu/refs/heads/main/preview/images/mcm1.png" alt="Logo"/>
</details>

**How to publish an event:**

1. Enter the **Mod GUID** (e.g., `gymmed.loot_manager_*`)
<details>
<summary>
2. Enter the **Event Name** (e.g., `AddLoot`)
</summary>
    <img src="https://raw.githubusercontent.com/GymMed/Outward-Mods-Communicator-Menu/refs/heads/main/preview/images/mcm2.png" alt="Logo"/>
</details>

<details>
<summary>
3. Click on **registered parameters** to add them, or create custom **dynamic parameters**
</summary>
    <img src="https://raw.githubusercontent.com/GymMed/Outward-Mods-Communicator-Menu/refs/heads/main/preview/images/mcm3.png" alt="Logo"/>
</details>
4. Click **Publish** to send the event

**Parameters:**
- **Registered Parameters**: Parameters that the event was originally designed to accept. Click on them to add to your payload.
- **Dynamic Parameters**: Custom parameters not in the original event schema. Specify the name, type, and value.

</details>

<details>
<summary><strong>Subscribers Tab</strong></summary>

View which mods are listening to which events. This shows you:

- Which mods have subscribed to events
- What callback methods they use
- The event names they're listening for

<details>
<summary>
This helps you understand how different mods communicate and react to each other.
</summary>
  <img src="https://raw.githubusercontent.com/GymMed/Outward-Mods-Communicator-Menu/refs/heads/main/preview/images/mcm4.png" alt="Logo"/>
</details>

</details>

<details>
<summary><strong>Publishers Tab</strong></summary>

See which mods have published events. This shows:

- Mod GUIDs that have published events
- The events they've sent
- Payload data that was sent

<details>
<summary>
Useful for debugging and understanding the event flow between mods.
</summary>
  <img src="https://raw.githubusercontent.com/GymMed/Outward-Mods-Communicator-Menu/refs/heads/main/preview/images/mcm5.png" alt="Logo"/>
</details>

</details>

## Features

### Type Support

When entering parameter values, the menu supports various data types:

| Type | Example |
|------|---------|
| Primitives | `string`, `int`, `float`, `bool`, `double` |
| Enums | `Character.Factions.Bandits`, `ItemDrop.ItemData.ItemType.Weapon` |
| Collections | Lists and arrays of any supported type |
| Vectors | `Vector2`, `Vector3`, `Vector4` |

### Input Methods

Parameters can be entered in two ways:

1. **Positional**: Values entered in order matching the parameter definitions
2. **Named**: Using `--parameterName=value` format

### Validation

The menu provides real-time validation:

- Warns if the event isn't registered
- Shows parsing errors for invalid values
- Displays success messages after publishing

## Installation

### Manual Install

1. Create the directory: `Outward\BepInEx\plugins\OutwardModsCommunicatorMenu\`
2. Extract the mod archive to an empty folder
3. Copy the contents from the `plugins\` folder to the directory you created
4. Your folder should look like: `Outward\BepInEx\plugins\OutwardModsCommunicatorMenu\OutwardModsCommunicatorMenu.dll`
5. Launch the game

### Thunderstore Install

Install through the Thunderstore mod manager by searching for "Mods Communicator Menu".

## Troubleshooting

**Menu doesn't open:**
- Ensure all dependencies are installed
- Check that the mod is loaded (look for log messages in `BepInEx\LogOutput.log`)

**Events won't publish:**
- Verify the mod GUID and event name are correct
- Check the parameter types match what the event expects

**Parameters not showing:**
- Not all events have registered parameters
- Use dynamic parameters to add custom data

## Technical Details

The menu is built using:

- **UniverseLib**: For creating the in-game UI panels
- **Event Bus**: Connects to Outward Mods Communicator's event system
- **Harmony**: For game method patching

## Support

If you encounter issues or have questions:

- Check the [GitHub Issues](https://github.com/GymMed/Outward-Mods-Communicator-Menu/issues)
- Leave feedback on the [Thunderstore page](https://thunderstore.io/c/outward/p/GymMed/Mods-Communicator-Menu/)

---

*If you like this mod, consider leaving a star on GitHub!*
