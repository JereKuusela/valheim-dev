- v1.110
  - Fixes for new game update. Thanks Haloa and others!

- v1.109
  - Adds support for showing text input when `<value_title>` is used in the command.
  - Adds support for not adding commands to the console history if they start with a whitespace character (space or tab).
  - Fixes console not being force enabled.
  - Fixes permission check failing if done when the player spawns (affects latest Infinity Hammer).
  - Improves file loading system to support sub-folders and patterns.
  - Removes dependency from Steamworks so should now work on non-Steam hosts.

- v1.108
  - Fixes server side commands checking admin status of the server, instead of the player executing the command.

- v1.107
  - Adds new field `admin` to the permissions.yaml to support character specific admin status.
  - Adds support for permissions for all characters with the same host name (character id is optional).
  - Fixes devcommands status not being used (admin status always enabled cheat access).
  - Fixes `move_spawn` command not working for custom spawn locations.
  - Fixes `move_spawn` command causing error if no spawn was found (now attempts to make a new one).

- v1.106
  - Fixes server side remote commands returning excessive output.
  - Fixes autocomplete issue.
