# Dedicated server devcommands

This client side mod allows devcommands for server admins.

Some features and commands require also installing the mod on the server (event, randomevent, resetkeys, skiptime, sleep, stopevent).

# Manual Installation:

1. Install the [BepInExPack Valheim](https://valheim.thunderstore.io/package/denikson/BepInExPack_Valheim)
2. Download the latest zip
3. Extract it in the \<GameDirectory\>\BepInEx\plugins\ folder.
4. Add your steamID64 to adminlist.txt (if not already).

Check [wiki](https://valheim.fandom.com/wiki/Console_Commands) for available commands and how to use them.

# Features

## Improved key bindings

`bind` command supports a new named parameter `keys=` that allows configuring which keys must or must not be down when pressing the bound key ([key codes](https://docs.unity3d.com/ScriptReference/KeyCode.html)).

For example:

- `bind j god`: Toggles god mode when pressing J.
- `bind j god keys=leftalt`: Toggles god mode when pressing J while left alt is down.
- `bind j god keys=-leftalt`: Toggles god mode when pressing J while left alt is not down.
- `bind j god keys=leftalt,h`: Toggles god mode when pressing J while both left alt and h are down.

After removing this mod, these binds very likely stop working or lead to unexpected behavior. Recommended to clear all binds with the `resetbinds` command.

## Enhanced commands

- `pos` command allows getting the position of any player.
	- `pos`: Returns your position.
	- `pos jay`: Returns the position of a player named Jay,Heyjay or whatever is the closest match.
- `event` command allows setting the event coordinates.
	- `event army_eikthyr`: Starts an event at your position.
	- `event army_eikthyr 100 -100`: Starts an event at coordinates 100,-100.

## Enhanced map

The large map shows coordinates of the cursor when hovered. This can be useful for any commands that require coordinates. The feature can be toggled with `dev_config map_coordinates` command.

If this mod is also installed on the server, admins will also receive position of players who have set their position as private.

These players are shown on the map with a ticked off icon and will also be available for the `pos` command.

The feature can be toggled with `dev_config private_players` command or by editing the config.

- New command "spawn_object" that parameters for position, rotation and scale (not supported for all objects). Also automatically snaps to the ground. Position and rotation are relative to the player. Parameters refPos and refRot can be used to override it.
  - spawn_object X rot=90: Spawns object X with 90 degree rotation.
	- spawn_object X rot=90 scale=10: Spawns object X with 90 degree rotation and 10x size.
	- spawn_object X rot=0,180: Spawns object X upside down.
	- spawn_object X scale=1,10,1: Spawns object X stretched upwards (only supported for a few things like trees).
	- spawn_object X amount=10: Spawns 10 of object X (items are autostacked, 10000 Coins won't crash the game).
	- spawn_object X level=4 crafter=Hero durability=0: Spawns a broken level 4 of item X with crafter name set to Hero.
	- spawn_object X variant=1: Spawns a variant 1 of item X (like shields or linen cape).
- New command "spawn_location" that allows spawning points of interests with a fixed seed and rotation.
  - spawn_location X: Spawns location X at player's position.
  - spawn_location X seed=0 rot=90: Spawns location X with seed 0 and 90 degree rotation.
  - spawn_location X pos=1000,1000: Spawns location X at coordinates 1000,1000.
  - spawn_location X pos=100,100,50: Spawns location X at coordinates 100,50,100 (no snap to the ground).
- New command "undo_spawn" which reverts commands "spawn_object" and "spawn_locations".
- New command "redo_spawn" which restored reverted spawns.
- New command "target" that allows modifying the hovered object or all objects within a radius.
  - target tame: Tames the target.
  - target wild: Untames the target.
	- target baby: Prevents growth for an offspring.
	- target sleep: Makes the target sleep (only works for naturally sleeping creatures).
	- target info: Prints information about the target.
	- target remove: Removes the target.
	- target stars: Sets amount of stars for the target.
	- target health: Sets target health (and maximum health for creatures).
	- target wild radius=10: Makes all creatures wild within 10 meters.
	- target tame id=Grey* radius=100: Tames all greylings, greydwarves, etc. within 100 meters.



# Changelog

- v1.6:
	- Improved autocomplete (different options per parameter).

- v1.5:
	- Adds modifier key support to key bindings.
	- Adds new parameter to the pos command (allows getting position of any player).
	- Adds support for showing private player positions (requires also server side).
	- Changes setkey command to work client side.
	- Fixes console spam.
	- Fixes console commands not working in the character selection.
	- Attempts to further improve the admin check reliability.

- v1.4:
	- Adds server side support for event, randomevent, resetkeys, setkey, skiptime, sleep, stopevent.

- v1.3: 
	- Refactores the code to hopefully make it work more reliably.
	- Adds autocomplete to chat window also for cheat commands.

- v1.2: 
	- Adds support for Hearth and Home update.

- v1.1: 
	- Improves admin check.

- v1.0: 
	- Initial release.