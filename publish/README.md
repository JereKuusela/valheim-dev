# Server Devcommands

Allows devcommands and utilities for server admins.

Some features and commands require also installing the mod on the server (event, randomevent, resetkeys, skiptime, sleep, stopevent).

Install on the admin client and optionally on the server (modding [guide](https://youtu.be/L9ljm2eKLrk)).

Check [wiki](https://valheim.fandom.com/wiki/Console_Commands) for available commands and how to use them. Remember to add your steamID64 to the adminlist.txt.

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
	- `bind j,leftalt,h debugmode`: Toggles debug mode when pressing J while both left alt and h are down.
	- `bind j keys=leftalt,h`: Alternative way.

By default the best match is used. Which means that with above binds, toggling debugmode won't also toggle the god mode.

It's also possible to use negative modifiers. For example `bind j,-leftalt god` won't toggle god mode while left alt is pressed. However using these is not usually needed. 

### Bind modes

The list of keys can also contain item names to enable/disable commands when a certain item is equipped.

For example `bind j,hammer god` would only toggle god mode when the hammer is equipped.

List of other special modes:
- `build`: True when in build mode.

### Mouse wheel

Mouse wheel allows binding too with custom keycode `wheel` (by default simulates the keycode `none`). It's important to use modifier keys because the binding will block build rotation.

The mouse wheel appends the wheel direction and amount to the command. For example `bind wheel,o say` would say 0.1 or -0.1 in the chat when scrolling the mouse wheel while pressing the O key.

Note: After removing this mod, these binds very likely stop working or lead to unexpected behavior. Recommended to clear all binds with the `resetbinds` command.

### Debug flying

The same system also works for rebinding the debug flying. For example:

- `devconfig fly_down_key space,leftcontrol`: Changes to fly down when both left control and space are pressed.

### Bind tagging

Binds can be tagged with `tag=[name]` parameter. The tag can be used to directly `unbind` these commands.

This is mostly indended to be used by other mods.

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
- `goto [x,z,y]` or ``goto x z y` teleports to the coordinates. If y is not given, teleports to the ground level.
- `goto` teleports to the ground level.
- `goto [y]` teleports to the altitude (dungeons are at 5000 altitude).
- `hud` allows toggling the HUD visibility.
- `hud [value]` allows directly setting the HUD visibility.
- `move_spawn [x,z,y = player's coordinates]` allows moving the default spawn point.
- `nomap [value]` allows directly setting the nomap mode and works with `server` command.
- `pos [player name / precision] [precision]` allows getting the position of any player.
	- `pos 1`: Returns your position with 1 decimal precision.
	- `pos jay`: Returns the position of a player named Jay,Heyjay or whatever is the closest match.
- `raiseskill * [amount]` allows raising all skills. 
- `resetskill [skill]` allows reseting skills.
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
- `unbind [keycode] [amount=0] [silent]` allows specifying how many binds are removed. Also prints removed binds, unless the third parameter is given.
	- `unbind wheel`: Removes all binds from the mouse wheel.
	- `unbind wheel 0`: Removes all binds from the mouse wheel.
	- `unbind wheel 1`: Removes the last bind from the mouse wheel.
	- `unbind wheel 3`: Removes the last 3 binds from the mouse wheel.
- `unbind [tag] [silent]` removes all binds with a given tag. Also prints removed binds, unless the third parameter is given.
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

- Use `dev_config` and `server dev_config` commands.
- Use the [Configuration manager](https://valheim.thunderstore.io/package/Azumatt/Official_BepInEx_ConfigurationManager/) if installed.
- Manually edit the `server_devcommands.cfg` in the config folder.

## General

- Access private chests (default: `true`, key: `access_private_chests`): Allows opening private chests.
- Access warded areas (default: `true`, key: `access_warded_areas`): Allows accessing warded areas.
- Always dodge with god mode (default: `false`, key: `god_always_dodge`): Automatically dodges with the god mode.
- Always parry with god mode (default: `false`, key: `god_always_parry`): Automatically parries with the god mode when not blocking.
- Automatic debug mode (default: `false`, key: `auto_debugmode`): Automatically turns debug mode on/off when devcommands are enabled or disabled.
- Automatic devcommands (default: `true`, key: `auto_devcommands`): Automatically tries to enable devcommands when joining servers.
- Automatic fly mode (default: `false`, key: `auto_fly`): Automatically turns fly mode on/off when devcommands are enabled or disabled.
- Automatic ghost mode (default: `false`, key: `auto_ghost`): Automatically turns ghost mode on/off when devcommands are enabled or disabled.
- Automatic god mode (default: `false`, key: `auto_god`): Automatically turns god mode on/off when devcommands are enabled or disabled.
- Automatic item pick up (default: `true`, key: `automatic_item_pick_up`): Sets the default value for the automatic item pick up feature.
- Automatic no cost mode (default: `false`, key: `auto_nocost`): Automatically turns no cost mode on/off when devcommands are enabled or disabled.
- Debug mode fast teleport (default: `true`, key: `debug_fast_teleport`): Makes teleporting much faster.
- Disabled global keys (default: ` `, key: `disable_global_key`): Global keys separated by , that can't be set (server side).
- Disable no map (default: `false`, key: `disable_no_map`): Disables no map having effect.
- Disable random events (default: `false`, key: `disable_events`): Prevents random events from happening (server side setting).
- Disable start shout (default: `false`, key: `disable_start_shout`): Removes the initial shout message when joining the server.
- Disable tutorials (default: `false`, key: `disable_tutorials`): Prevents the raven from appearing.
- Disable unlock messages (default: `false`, key: `disable_unlock_messges`): Disables messages about new pieces and items.
- Hide shout pings (default: `false`, key: `hide_shout_pings`): Forces shout pings at the world center.
- Invisible to players with ghost mode (default: `false`, key: `ghost_invisibility`): Invisible to other players with ghost mode.
- Max undo steps (default: `50`, key: `max_undo_steps`): How many undo actions are stored.
- No clip clear environment (default: `true`, key: `no_clip_clear_environment`): Removes any forced environments when the noclip is enabled. This disables any dark dungeon environments and prevents them from staying on when exiting the dungeon.
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
- Best command match (default: `true`, key: `best_command_match`): Executes only the commands with the most modifiers keys pressed. Simplifies key binding because negative modifier keys don't have to be used.
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
- Mouse wheel bind key (default: `none`, key: `mouse_wheel_bind_key`): The simulated keycode when using the mouse wheel.
- Multiple commands per line (default: `true`, key: `multiple_commands`): Enables multiple commands per line (when separate by `;`).
- Root users: Steam IDs separated by , that can execute blacklisted commands. Can't be set with `dev_config` command.
- Substitution system (default: `true`, key: `substitution`): Enables parameter substitution (with `$`).
- Server side commands (default: `randomevent,stopevent,genloc,sleep,skiptime`, key: `server_commands`): Names of commands that should be automatically executed on the server. `event` command is not included because it has a custom server-side support.

# Changelog

- v1.24
	- Adds equipped tool support to key bindings.
	- Adds build mode support to key bindings.
	- Adds a new setting `disable_unlock_messages`.
	- Internal change to support Infinity Hammer mod.

- v1.23
	- Adds a new setting `disable_no_map` to override the nomap rule.
	- Adds a new setting `hide_shout_pings` to force the ping at the world center.
	- Fixes some issues with the autocomplete.

- v1.22
	- Adds a new setting `debug_fast_teleport` to make all teleporting faster with the debug mode.
	- Improves the `goto` command to allow easier teleporting to the ground or high in the sky. 
	- Adds tag support for `bind` (needed by some mods).
	- Changes the fly mode to stop animations and prevent crouching and jumping.
	- Improves the `unbind` command to directly remove binds with a tag.
	- Removes the console input character limit (too small for some commands).
	- Fixes no clip never triggering ship exit (so the ship just keeps going).
	- Fixes mouse wheel binds not overriding fly ascend/descend.

- v1.21
	- Adds a new setting `best_command_match` to only execute the commands with the most modifier keys pressed (default `true`).
	- Fixes some commands printing "Error: Player not found.".
	- Replaces the setting `mouse_wheel_binding` with a new setting `mouse_wheel_bind_key` that instead allows settting the simulated key.
	- Fixes root users not working with the new save system.
	- Changes the guid which resets the config.

- v1.20
	- Adds a new setting `access_private_chests` which allows opening all private chests (default `true`).
	- Adds a new setting `access_warded_areas` which allows ignoring wards (default `true`).
	- Adds a new setting `max_undo_steps`(default `50`).
	- Adds a new command `resetskill` to reset skills more easily.
	- Adds a new parameter `*` to the `raiseskill` command to raise (or lower) all skills.

Thanks for Azumatt for creating the mod icon!