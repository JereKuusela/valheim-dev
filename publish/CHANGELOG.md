- v1.112
  - Fixed for the new update.

- v1.111
  - Adds setting to disable cheat tracking when using commands (enabled by default).
  - Adds `clearcheats` command to clear the character cheat status.
  - More fixes. Thanks JPValheim!

- v1.110
  - Fixes for the new game update. Thanks Haloa, leandrogg, andrewstevenson91 and endimonan!

- v1.109
  - Adds support for showing text input when `<value_title>` is used in the command.
  - Adds support for not adding commands to the console history if they start with a whitespace character (space or tab).
  - Fixes console not being force enabled.
  - Fixes permission check failing if done when the player spawns (affects latest Infinity Hammer).
  - Improves file loading system to support sub-folders and patterns.
  - Removes dependency from Steamworks so should now work on non-Steam hosts.

- v1.108
  - Fixes server side commands checking admin status of the server, instead of the player executing the command.
