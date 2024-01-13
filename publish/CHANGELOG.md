- v1.70
  - Fixes the commands `raiseskill` and `resetskill` crashing the game.

- v1.69
  - Fixes the first parameter not being case insensitive (for example `env clear`).
  - Improves autocomplete of nested commands.

- v1.68
  - Fixes the `tp` command not working.
  - Fixes issues with latest Comfy Gizmo.
  - Internal changes for World Edit Commands, Infinity Hammer and Infinity Tools mods.
  - Removes the bind tagging system as obsolete.
  - Removes the setting "Disable parameter warnings" as obsolete.
  - Removes the ping from the `find` command when a single object was found.

- v1.67
  - Adds a new command `search_component` for debugging purposes.
  - Changes the timer from `wait` command to be separate for each command.
  - Fixed for the new patch.

- v1.66
  - Adds a new command `tp` to teleport players.
  - Fixes Ctrl+A not working in the console.

- v1.65
  - Fixes noclip flying not resetting water level.

- v1.64
  - Fixes `find` command not adding pins on single player.

- v1.63
  - Fixes underground check being disabled for creatures, thanks Tristan!

- v1.62
  - Fixed for new patch.
  - Removes admin implementation of commands `resetkeys`, `sleep`, `skiptime` and `genloc` (vanilla supports this now).

- v1.61
  - Fixes tab completion not working with aliases.

- v1.60
  - Adds a new setting "Wrapping" to allow using space bar in command parameters.
  - Reworks the substitution system to only work with aliases (less interference with normal commands).
  - Removes the setting "Debug console" as obsolete.

- v1.59
  - Adds a new setting "Improved chat" to allow turning off most chat changes.
  - Improves compatibility with KGChat.
  - Removes the setting "Best command match" as obsolete.

- v1.58
  - Fixes world modifiers cleaned up on world load.
  - Fixes `resetkeys` command not working.
