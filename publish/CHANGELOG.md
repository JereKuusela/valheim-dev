- v1.54
  - Adds total count to the `find` command.
  - Adds a new setting for changing the `find` command format.
  - Adds a new setting to ignore sleep check with ghost mode.
  - Changes the default amount of shown items in the `find` command to 10.
  - Fixes the default value of "god mode no item usage" being true.

- v1.53
  - Adds a setting for the no spawns in ghost mode feature.
  - Adds support for disabling the command logging by emptying the format.

- v1.52
  - Fix for Server Devcommands mod.

- v1.51
  - Adds a radius parameter to the `calm` and `repair` commands.
  - Updated for the new game version.

- v1.50
  - Adds a new command `pull` to pull nearby items.
  - Fixes disabling of underground check not checking is the creature the player or not.
  - Removes caching for location and event auto complete (for Expand World).

- v1.49
  - Changes the `playerlist` command only work server side.

- v1.48
  - Changes the custom command `players` to `playerlist` to not interfere with the vanilla commmand.

- v1.47
  - Adds new settings to change format of the command logging, `players` command and minimap coordinates.

- v1.46
  - Adds a new command `players` to print online players.
  - Changes the `find` command to server side.

- v1.45
  - Adds server side command logging when installed on both client and server.

- v1.44
  - Fixes the server side chat feature not working.

- v1.43
  - Fixes automatic parrying triggering on the player when the shield breaks.
  - Fixes error with the new update.

- v1.42
  - Internal change to support Infinity Hammer mod.
  - Changes the command `resetskill` to reset all skills with the `All` argument (to match `raiseskill` behavior).
  - Changes the ghost mode to prevent spawns (only when your client is controlling the area).
  - Fixes the `search_id` command.
  - Fixes the "No weight limit with god mode" setting appearing to work on other players (with mods that allow inspecting other players).
  - Removes the customized `raiseskill` command (vanilla supports now the `All` argument).
  - Removes the setting `Disable tutorials` as obsolete.
  - Removes the support for tool specific keybinds as obsolete.

- v1.41
  - Adds duration and intensity to the `addstatus` command.
  - Adds player name support to `goto` command.
  - Fixes the setting `No item usage with god mode` preventing usage of fermenters.
  - Fixes the setting `No eitr usage with god mode` showing small amount of eitr for some UI mods.
