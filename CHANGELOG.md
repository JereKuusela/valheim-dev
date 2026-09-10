- v1.109.3 (Deep North prototype handoff; 2026-09-10)
  - Move synthetic server-chat player injection to `ZNet.WritePlayerInfo`, where Deep North writes the player-list packet.
  - Keep plugin and publish-manifest versions aligned.

- v1.109.2 (Deep North prototype handoff; 2026-09-09)
  - Update synthetic server-chat identity and packet serialization for Deep North.
  - Refresh session-bound identity and guard unavailable server/backend state.

- v1.109.1 (Deep North prototype handoff iteration 2; 2026-09-09)
  - Cancel delayed command groups when their originating world session ends.
  - Clear the command execution flag after exceptions as well as success.

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

- v1.105
  - Fixes major performance issue in the permission system.
