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

- v1.35
	- Update for Mistlands PTB.

- v1.34
	- Fixes rectangle shape not working on World Edit Commands and Infinity Hammer mods.

- v1.33
	- Adds a new command `inventory` for inventory management.
	- Removes the ping when map teleporting.

- v1.32
	- Fixes compatibility issue with Comfy Gizmo.

- v1.31
	- Fixes the autocomplete of `env` command.
	- Fixes the black screen.

- v1.30
	- Improves compatibility with mods that add new skills.

- v1.29
	- Adds an `alias.yaml` file to store aliases.
	- Adds a `binds.yaml` file to store key binds.
	- Changes the `substitution`setting to allow setting the subtitution value instead of only enabling.

- v1.28
	- Adds support for multiple `keys=` parameters in bindings.
	- Changes the `wait` command to update at start of the frame.
	- Fixes aliasing not sometimes working for aliases without parameters.

- v1.27
	- Internal change to support World Edit Commands mod.

- v1.26
	- Adds parameter `last` to the `goto` command to allow easily returning to the previous position.
	- Adds a new setting `no_clip_view` to always no clip the camera.
	- Fixes mouse wheel binds resetting placement rotation.
