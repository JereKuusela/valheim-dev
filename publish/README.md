# Server Devcommands

This client side mod allows devcommands and utilities for server admins.

Some features and commands require also installing the mod on the server (event, randomevent, resetkeys, skiptime, sleep, stopevent).

# Manual Installation:

1. Install the [BepInExPack Valheim](https://valheim.thunderstore.io/package/denikson/BepInExPack_Valheim)
2. Download the latest zip.
3. Extract it in the \<GameDirectory\>\BepInEx\plugins\ folder.
4. Add your steamID64 to adminlist.txt (if not already).

Check [wiki](https://valheim.fandom.com/wiki/Console_Commands) for available commands and how to use them.

# Features

- Console is enabled without having to set the start parameter.
- Cheat commands can also be used from the chat window (with autocomplete).
- Autocomplete works for every parameter, always providing some information.
- Multiple commands can be executed at the same time (when separated with `;`).
- New commands can be created with `alias` command.
- Modifier keys work when binding commands to keys.
- `devcommands` is used automatically (if the admin check passes).
- `debugmode`, `fly`, `ghost`, `god`, `nocost` and other commands can be configured to be used automatically.
- God mode removes stamina usage, knockback and staggering.
- Ghost mode can make you invisible also to the other players.
- Fly mode can remove collision to pass through everything.
- Minor tweaks to existing commands, some new commands and other useful admin features.

## Improved key bindings

Keybindings now work with modifier keys ([key codes](https://docs.unity3d.com/ScriptReference/KeyCode.html)).

- `bind [keycode,modifier1,modifier2,...] [command] [parameter]`: Adds a new key binding with modifier keys.
	- `bind j god`: Toggles god mode when pressing J.
	- `bind j,leftalt god`: Toggles god mode when pressing J while left alt is down.
	- `bind j,-leftalt god`: Toggles god mode when pressing J while left alt is not down.
	- `bind j,leftalt,h god`: Toggles god mode when pressing J while both left alt and h are down.	
	- `bind j god keys=leftalt,h`: Same as above but with an alternative way.

Mouse wheel allows binding too with custom keycode `wheel` (internally uses the `none` keycode). It's important to use modifier keys because the binding will block build rotation.

The mouse wheel appends the wheel direction and amount to the command. For example `bind wheel,o say` would say 0.1 or -0.1 in the chat when scrolling the mouse wheel while pressing the O key.

Note: After removing this mod, these binds very likely stop working or lead to unexpected behavior. Recommended to clear all binds with the `resetbinds` command.

### Debug flying

The same system also works for rebinding the debug flying. For example:

- `devconfig fly_down_key space,leftcontrol`: Changes to fly down when both left control and space are pressed.
- `devconfig fly_up_key space,-leftcontrol`:  Changes to fly up when space is pressed while left control isn't.

## Command aliasing

New commands can be created to shorten command names or to set parameter values.

This is intended to be used with other mods that add more complex commands than in the base game.

- `alias [name] [value]`: Adds a new command alias.
- `alias`: Prints all aliases.
- `alias [name]`: Removes the given alias.

Examples:

- `alias dm debugmode`: Adds a new command `dm` as a shorter version of `debugmode`.
- `alias spawn5 spawn $ 5 $`: Adds a new command `spawn5` with the spawn amount fixed at 5.
- `alias maxskill raiseskill $ 100`: Adds a new command `skill_max` that raises the given skill to max level.
- `alias resetskill raiseskill $ -100`: Adds a new command `skill_reset` that resets the given skill.
- `alias cheat debugmode;nocost;fly`: Adds a new command `cheat` to quickly toggle cheats (if you don't want to use the config).

## Enhanced commands

- `bind [key,modifier1,modifier2,...] [command]` allows specifying modifier keys (see Improved key bindings section).
- `broadcast [center/side] [message]` allows broadcasting custom messages to all players.
	- `broadcast center <color=red><size=20><i><b>Hello!</b></i></size></color>`: Broadcasts a small red message with bolding and italics.
- `devcommands` includes an admin check to allow using on servers.
- `dev_config [name] [value]` toggles settings.
	- `dev_config auto_fly`: Toggles the auto fly setting.
	- `dev_config auto_fly 0`: Disables the auto_fly setting.
	- `dev_config auto_fly 1`: Enables the auto fly setting.
	- `server dev_config disable_debug_mode_keys defeated_eikthyr,defeated_gdking,defeated_bonemass,defeated_dragon,defeated_goblinking`: Prevents boss kill global keys being set on the server.
	- `server dev_config disable_debug_mode_keys`: Prints currently disabled global keys.
- `env` prints the current environment.
- `event [event] [x] [z]` allows setting the event coordinates.
	- `event army_eikthyr`: Starts an event at your position.
	- `event army_eikthyr 100 -100`: Starts an event at coordinates 100,-100.
- `exploremap [x] [z] [radius]` allows revealing only a part of the map.
- `hud` allows toggling the HUD visibility.
- `hud [value]` allows directly setting the HUD visibility.
- `move_spawn [x,z,y = player's coordinates]` allows moving the default spawn point.
- `nomap [value]` allows directly setting the nomap mode and works with `server` command.
- `pos [player name / precision] [precision]` allows getting the position of any player.
	- `pos 1`: Returns your position with 1 decimal precision.
	- `pos jay`: Returns the position of a player named Jay,Heyjay or whatever is the closest match.
- `redo` restores an action added to the undo/redo manager.
- `resetmap [x] [z] [radius]` allows hiding only a part of the map.
- `resetpins [x] [z] [radius]` allows removing pins from the map.
- `resolution` prints the screen properties.
- `resolution [mode] [width] [height] [refresh rate]` sets the screen properties.
	- `resolution exclusive`: Sets to full screen with the maximum supported resolution and refresh rate.
	- `resolution max`: Sets to maximized window with the maximum supported resolution.
	- `resolution full`: Sets to full screen window with the maximum supported resolution.
	- `resolution window 1920 1080`: Sets to windowed with HD resolution.
- `search_id [term] [max_lines=5]` allows searching the object ID list.
	- `search_id wolf`: Prints all object IDs that contain word "wolf".
	- `search_id fx_ 10`: Prints all object IDs that contain word "fx_" on up to 10 lines.
- `seed` prints the world seed.
- `server [command]` executes given command on the server.
	- `server dev_config disable_command event`: Disables usage of `event` command for non-root users.
	- `server dev_config disable_events 1`: Disables random events.
- `unbind [keycode] [amount=0]` allows specifying how many binds are removed. Also prints removed binds.
	- `unbind wheel`: Removes all binds from the mouse wheel.
	- `unbind wheel 0`: Removes all binds from the mouse wheel.
	- `unbind wheel 1`: Removes the last bind from the mouse wheel.
	- `unbind wheel 3`: Removes the last 3 binds from the mouse wheel.
- `undo` reverts an action added to the undo/redo manager.
- `wait [milliseconds]`delays the execution of the next commands.
- `wind` prints the current wind strength.

## Enhanced map

The large map shows coordinates of the cursor when hovered. This can be useful for any commands that require coordinates.

If configured, the minimap can also show the player's coordinates.

If this mod is also installed on the server, admins can also receive position of players who have set their position as private (disabled by default).

These players are shown on the map with a ticked off icon and will also be available for the `pos` command.

# Configuration

Three ways to edit the settings:

- Use `dev_config` and `server dev_config` commands to instantly toggle values.
- Use the [Configuration manager](https://github.com/BepInEx/BepInEx.ConfigurationManager/releases/tag/v16.4) if installed to instantly toggle values for the client.
- Manually edit the `valheim.jerekuusela.dev.cfg` in the config folder (requires restarting the client / server).

Recommended way is to use the commands since you can configure the server and also `bind` them to keys.

## General

- Always dodge with god mode (default: `false`, key: `god_always_dodge`): Automatically dodges with the god mode.
- Always parry with god mode (default: `false`, key: `god_always_parry`): Automatically parries with the god mode when not blocking.
- Automatic debug mode (default: `false`, key: `auto_debugmode`): Automatically turns debug mode on/off when devcommands are enabled or disabled.
- Automatic devcommands (default: `true`, key: `auto_devcommands`): Automatically tries to enable devcommands when joining servers.
- Automatic fly mode (default: `false`, key: `auto_fly`): Automatically turns fly mode on/off when devcommands are enabled or disabled.
- Automatic ghost mode (default: `false`, key: `auto_ghost`): Automatically turns ghost mode on/off when devcommands are enabled or disabled.
- Automatic god mode (default: `false`, key: `auto_god`): Automatically turns god mode on/off when devcommands are enabled or disabled.
- Automatic item pick up (default: `true`, key: `automatic_item_pick_up`): Sets the default value for the automatic item pick up feature.
- Automatic no cost mode (default: `false`, key: `auto_nocost`): Automatically turns no cost mode on/off when devcommands are enabled or disabled.
- Disabled global keys (default: ` `, key: `disable_global_key`): Global keys separated by , that can't be set (server side).
- Disable random events (default: `false`, key: `disable_events`): Prevents random events from happening (server side setting).
- Disable start shout (default: `false`, key: `disable_start_shout`): Removes the initial shout message when joining the server.
- Disable tutorials (default: `false`, key: `disable_tutorials`): Prevents the raven from appearing.
- Invisible to players with ghost mode (default: `false`, key: `ghost_invisibility`): Invisible to other players with ghost mode.
- No clip with fly mode (default: `false`, key: `fly_no_clip`): Removes collision check with fly mode.
- No creature drops (default: `false`, key: `no_drops`): Prevents creatures from dropping loot, can be useful if people accidentally spawn very high star creatures. Only works when as the zone owner.
- No edge of world pull with god mode (default: `true`, key: `gpd_no_edge`): Removes the pull for an even godlier god mode.
- No knockback with god mode (default: `true`, key: `god_no_knockback`): Removes knockback for an even godlier god mode.
- No staggering with god mode (default: `true`, key: `god_no_stagger`): Removes staggering for an even godlier god mode.
- No stamina usage with god mode (default: `true`, key: `god_no_stamina`): Removes the stamina usage for an even godlier god mode.
- No weight limit with god mode (default: `false`, key: `god_no_weight_limit`): Removes the weight limit for an even godlier god mode.
- Show map coordinates (default: `true`, key: `map_coordinates`): Shows cursor coordinates when hovering the map.
- Show minimap coordinates (default: `false`, key: `minimap_coordinates`): Shows player coordinates on the minimap.
- Show private players (default: `false`, key: `private_players`): Shows players on the map even if they have set their position as private. Must be enabled both client and server side to work (admins can individually keep the feature off even when enabled from the server). 

## Console

Recommended to keep all features on, unless there are errors or mod conflicts.

- Alias system (default: `true`, key: `aliasing`): Enables command aliasing.
- Auto exec (key: `auto_exec`): Executes the given command when joining a server (before admin is checked).
- Auto exec boot (key: `auto_exec_boot`): Executes the given command when starting the game.
- Auto exec dev off (key: `auto_exec_dev_off`): Executes the given command when disabling devcommands.
- Auto exec dev on (key: `auto_exec_dev_off`): Executes the given command when enabling devcommands.
- Command aliases: Saved command aliases.
- Command descriptions (default: `true`, key: `command_descriptions`): Shows command descriptions as autocomplete.
- Debug console (default: `false`, key: `debug_console`): Prints debug output to the console related to aliasing and parameter substitution.
- Disable debug mode keys (default: `false`, key: `disable_debug_mode_keys`): Removes debug mode key bindings for killall, removedrops, fly and no cost.
- Disable messages (default: `false`, key: `disable_messages`): Prevents messages from commands.
- Disable parameter warnings (default: `false`, key: `disable_warnings`): Removes warning texts from some command parameter descriptions.
- Disabled commands (default: `dev_config disable_command`, key: `disable_command`): Command names separated by , that can't be executed. Mainly useful on the server to prevent some server-side commands.
- Fly down key (default: `leftcontrol`, key: `fly_down_key`): Changes the key command for flying down. Multiple keys are supported (see Bind section for more info).
- Fly up key (default: `space`, key: `fly_up_key`): Changes the key command for flying up. Multiple keys are supported (see Bind section for more info).
- Improved auto complete (default: `true`, key: `improved_autocomplete`): Enables parameter info or options for every parameter.
- Mouse wheel binding (default: `true`, key: `mouse_wheel_binding`): Enables binding to the mouse wheel with `wheel` keycode.
- Multiple commands per line (default: `true`, key: `multiple_commands`): Enables multiple commands per line (when separate by `;`).
- Root users: Steam IDs separated by , that can execute blacklisted commands. Can't be set with `dev_config` command.
- Substitution system (default: `true`, key: `substitution`): Enables parameter substitution (with `$`).
- Server side commands (default: `randomevent,stopevent,genloc,sleep,skiptime`, key: `server_commands`): Names of commands that should be automatically executed on the server. `event` command is not included because it has a custom server-side support.

# Changelog

- v1.18
	- Adds a new parameter to the `pos` command that allows specifying the precision.
	- Changes the format of the `pos` command from x,y,z to x,z,y (matches better World Edit Commands mod).
	- Changes the `fly_no_clip` to automatically removed forced environments (like in Frost Caves).

- v1.17
	- Internal changes for World Edit Commands mod.
	- Improves `fly_no_clip` compatibility with noclip mods.
	- Improves `disable_debug_mode_keys` compatibility with mods having debug mode specific keys.

- v1.16
	- Adds a new setting `god_no_edge` to disable the edge of world pull with god mode.
	- Referts the `nomap change` in v1.11. Now only affects the client when used on dedicated servers. 
	- Fixes `nomap` command showing inversed output.
	- Fixes automatic debug mode, etc. not working when joining worlds multiple times.

- v1.15
	- Adds position and radius parameters to commands `exploremap` and `resetmap`.
	- Adds a new command `resetpins` to remove map pins.
	- Adds distance to the map coordinates.
	- Changes the `env` command to print the current environment if used without the parameter.
	- Changes the `wind` command to print the wind strength if used without the parameters.
	- Improves autocomplete for `forcedelete`, `spawn` and `resetsharedmap` commands.
	- Fixes devcommands getting toggled off and on when dying.
	- Fixes map and minimap coordinates conflicting with other mods.

- v1.14
	- Fixes root users not being automatically recognized as admins (server side).

- v1.13
	- Adds a new setting `automatic_item_pick_up` to set the default value for the automatic pickup feature.
	- Adds a new setting `disable_messages` to prevent messages from commands.
	- Fixes the `dev_config` command output having extra " characters.
	- Fixes the autocomplete showing error when no autocomplete is available.
	- Fixes an error with the automatic parry.
	- Fixes incompatibility with mods affecting the underwater camera.

- v1.12
	- Adds a new command `broadcast` to broadcast messages.
	- Adds a new command `move_spawn` to change the server's default spawn point.
	- Adds a new command `seed` to print the seed.
	- Adds a new setting `disable_start_shout` to remove the initial shout message (default: `false`).
	- Adds a new setting `disable_debug_mode_keys` to remove hardcoded debug mode key bindings (default: `false`).
	- Adds a new setting `god_always_parry` to always parry with the god mode (default: `false`).
	- Adds a new setting `god_always_dodge` to always dodge with the god mode (default: `false`).
	- Adds a new setting `disable_tutorials` to prevent the raven from appearing (default: `false`).
	- Adds a new setting `god_no_weight_limit` to remove weight limit with the god mode (default: `false`).
	- Adds a new setting `fly_up_key` to allow rebinding it (default: `jump`).
	- Adds a new setting `fly_down_key` to allow rebinding it (default: `leftcontrol`).
	- Adds a new parameter to the `unbind` command which allows only removing some amount of binds.
	- Adds a new setting `mouse_wheel_binding` for the mouse wheel binding (default: `true`).
	- Adds support for binding commands to the mouse wheel (with `wheel` key code).
	- Changes default scale format from x,z,y to x,y,z (for other mods).
	- Changes the `bind` command to accept modifier keys on the first parameter (keycode,modifier1,modifier2,).
	- Changes the `unbind` command to print removed binds.
	- Changes the `search` command to `search_id` command.
	- Changes the setting `disable_global_key` to also remove disabled keys when edited (server side).
	- Improves the autocomplete for the `alias`, `bind` and `server` commands.
	- Improves the `dev_config` command to allow directly setting flags with values 1 and 0.
	- Improves the `dev_config` command to work better when giving multiple values to some commands.
	- Improves the `dev_config` command to print the current value for non-flags if no parameter is given.
	- Improves the autocomplete support (for other mods relying on this feature).
	- Removes the setting `command_delay` as obsolete since `wait` command works much better.
	- Removes the custom command `tutorialtoggle` as obsolete since `disable_tutorials` setting works much better.
	- Fixes command tab cycling breaking when cycling to an alias.
	- Fixes incorrect autocomplete for aliases.
	- Fixes modifier keys working incorrectly with multiple commands.
	- Fixes the `wait` command not working (parameter was kiloseconds instead of milliseconds).

- v1.11
	- Adds a new command `wait` to delay execution of the next command.
	- Adds a new command `server` to execute any command on the server.
	- Adds a new command `hud` to set or toggle the HUD visibility.
	- Adds a new setting `server_commands` to automatically execute given commands on the server.
	- Adds a new setting `disable_command` to allow disabling commands (server side).
	- Adds a new setting `disable_global_key` to prevent global keys from being set (server side).
	- Adds a new setting to add root users to the server (bypasses the blacklist).
	- Adds a new setting `fly_no_clip` to disable collision while flying.
	- Adds a new setting `minimap_coordinates` to show player coordinates on the minimap.
	- Adds y-coordinate to the map coordinates.
	- Adds improved autocomplete for new commands added in the Frost Caves update.
	- Changes the command `nomap` to only affect the server by default.
	- Removes `auto_debugmode` requirement from `auto_fly` and `auto_nocost`.
	- Removes the command `dev_server_config` as redundant.
	- Fixes incompatibility with some stamina related mods.
	- Fixes `resolution` command description.

- v1.10
	- Adds a new setting `command_delay` to add delay when multiple commands are executed.
	- Renames the file from DEV.dll to ServerDevcommands.dll.
	- Changes the default value of the `ghost_invibisility` to false.
	- Fixes incompatibility with Mountain Caves public beta test.

- v1.9:
	- Adds a new command `resolution` to print or set screen properties.
	- Adds a new setting `command_descriptions` to show command descriptions instead of the autocomplete.
	- Adds a new setting `ghost_invibisility` to turn invisible to other players with the ghost mode.
	- Adds a new setting `god_no_knockback` to disable knockback with the god mode.
	- Adds a new setting `auto_exec_boot` that automatically executes the given command when starting the game.
	- Adds a new setting `auto_exec` that automatically executes the given command when joining a server.
	- Adds a new setting `auto_exec_dev_on` that automatically executes the given command when enabling `devcommands`.
	- Adds a new setting `auto_exec_dev_off` that automatically executes the given command when disabling `devcommands`.
	- Adds a parameter to command `tutorialtoggle` which directly sets the value (instead of toggling).
	- Changes the command `tutorialtoggle` to work without needing Hugin to appear first.
	- Changes the command `alias` to add the plain text as the description instead of the original command description.
	- Fixes case insensitivity being broken for some commands.
	- Fixes server side commands not working.

- v1.8
	- Improves undo/redo system to work with Infinity Hammer mod.
	- Fixes selection not working when multiple commands system is on.
	- Fixes selection issues when using tab to cycle options.

- v1.7
	- Adds an undo/redo system (currently only for other mods to use).
	- New icon (thanks Azumatt!).
	- Improves autocomplete for default commands.
	- Fixes possible server crash if private players are enabled (another attempt).

- v1.6
	- Adds a better autocomplete that provides options and information for all parameters.
	- Adds an alias system which allows creating simpler commands out of existing ones.
	- Adds a parameter substitution system which allows mapping command parameters.
	- Adds support for multiple commands per line.
	- Adds a new command for setting server config values.
	- Adds a new command to search object ids.
	- Improves the admin check to support more features.
	- Adds a setting for automatic admin check (enabled by default).
	- Adds a setting for automatic debugmode.
	- Adds a setting for automatic god mode.
	- Adds a setting for automatic fly mode.
	- Adds a setting for automatic ghost mode.
	- Adds a setting for automatic no cost mode.
	- Adds a setting for improved autocomplete (enabled by default).
	- Adds a setting for command aliasing (enabled by default).
	- Adds a setting for command parameter substitution (enabled by default).
	- Adds a setting for multiple commands per line (enabled by default).
	- Adds a setting to remove stamina usage with god mode (enabled by default).
	- Adds a setting to remove staggering with god mode (enabled by default).
	- Adds a setting to disable creature drops.
	- Adds a setting to disable random events.
	- Changes the default value of "show private player positions" to false.
	- Fixes a server crash (caused by too many players connecting if private position feature was on).

- v1.5
	- Adds a modifier key support to key bindings.
	- Adds a new parameter to the pos command (allows getting position of any player).
	- Adds support for showing private player positions (requires also server side).
	- Changes the setkey command to work client side.
	- Fixes console spam.
	- Fixes console commands not working in the character selection.
	- Attempts to further improve the admin check reliability.

- v1.4
	- Adds server side support for event, randomevent, resetkeys, setkey, skiptime, sleep, stopevent.

- v1.3
	- Refactores the code to hopefully make it work more reliably.
	- Adds autocomplete to chat window also for cheat commands.

- v1.2
	- Adds support for Hearth and Home update.

- v1.1
	- Improves admin check.

- v1.0
	- Initial release.
	
Thanks for Azumatt for creating the mod icon!