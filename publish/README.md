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

- v1.36
	- Disabled "no pushback with god mode" until PTB is released.

- v1.35
	- Update for Mistlands PTB.
