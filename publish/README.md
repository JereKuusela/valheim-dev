# Dedicated server devcommands

This client side mod allows devcommands for server admins.

Some commands require also installing the mod on the server (event, randomevent, resetkeys, skiptime, sleep, stopevent).

# Manual Installation:

1. Install the [BepInExPack Valheim](https://valheim.thunderstore.io/package/denikson/BepInExPack_Valheim)
2. Download the latest zip
3. Extract it in the \<GameDirectory\>\BepInEx\plugins\ folder.
4. Add your steamID64 to adminlist.txt (if not already).

Check [wiki](https://valheim.fandom.com/wiki/Console_Commands) for available commands and how to use them.

# Features

- Adds optional coordinates to the event command to support dedicated server running the command.
- New command "spawn_object" that parameters for rotation and scale (not supported for all objects). Also automatically snaps to the ground.
  - spawn_object X rot=90: Spawns object X with 90 degree rotation.
	- spawn_object X rot=90 scale=10: Spawns object X with 90 degree rotation and 10x size.
	- spawn_object X rot=0,180: Spawns object X upside down.
	- spawn_object X scale=1,10,1: Spawns object X stretched upwards.
	- spawn_object X amount=10: Spawns 10 of object X.
- New command "spawn_location" that allows spawning points of interests with a fixed seed and rotation.
  - spawn_location X: Spawns location X at player's position.
  - spawn_location X seed=0 rot=90: Spawns location X with seed 0 and 90 degree rotation.
  - spawn_location X pos=1000,1000: Spawns location X at coordinates 1000,1000.
  - spawn_location X pos=100,100,50: Spawns location X at coordinates 100,50,100 (no snap to the ground).
- New command "undo_spawn" which reverts commands "spawn_object" and "spawn_locations".
- New command "redo_spawn" which restored reverted spawns.


# Changelog
- v1.5.0:
  - Changed setkey command to work client side.
	- New command "spawn_object" with rotation and scale.
	- New command "spawn_location" to spawn points of interests.
	- New commanad "undo_spawn".
	- New command "redo_spawn".
- v1.4.0:
	- Added server side support for event, randomevent, resetkeys, setkey, skiptime, sleep, stopevent.
- v1.3.0: 
	- Refactored the code to hopefully make it work more reliably.
	- Added autocomplete to chat window also for cheat commands.
- v1.2.0: 
	- Added support for Hearth and Home update.
- v1.1.0: 
	- Improved admin check
- v1.0.0: 
	- Initial release