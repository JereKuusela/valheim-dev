# Server Devcommands

Allows devcommands and utilities for server admins.

Some features and commands require also installing the mod on the server (event, randomevent, resetkeys, skiptime, sleep, stopevent).

Install on the admin client and optionally on the server (modding [guide](https://youtu.be/L9ljm2eKLrk)).

Check [wiki](https://valheim.fandom.com/wiki/Console_Commands) for available commands and how to use them. Remember to add your steamID64 / playfab ID to the adminlist.txt.

# Usage

See [documentation](https://github.com/JereKuusela/valheim-dev/blob/main/README.md).

# Credits

Thanks for Azumatt for creating the mod icon!

Sources: [GitHub](https://github.com/JereKuusela/valheim-dev)

Donations: [Buy me a computer](https://www.buymeacoffee.com/jerekuusela)

# Changelog

- v1.43
  - Fixes automatic parrying triggering on the player when the shield breaks.

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

- v1.40
  - Changes yaml files to not be created if they would be empty.
  - Fixes error when deleting yaml files when the mod is running.

- v1.39
  - Adds a new setting `No item usage with god mode` (default true).
  - Adds a new command `calm` to calm nearby aggravated creatures.
  - Adds a new command `repair` to repair nearby damaged structures.
  - Fixes `say` and `s` commands to work as the server (for Cron Jobs mod).
  - Fixes `No eitr usage with god mode` not working without any eitr.

- v1.38
  - Adds a new setting `No eitr usage with god mode` (default true).

- v1.37
  - Adds a new setting `No Mistlands mist with god mode` (default false).
  - Restores "no pushback with god mode".

