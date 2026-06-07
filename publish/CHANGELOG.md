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

- v1.104
  - Fixes permissions.yaml not being created on start up.

- v1.103
  - Adds granular permission system to allow features for non-admins and disable features for admins.
  - Fixes vanilla bug that sometimes spams error about "view direction zero" when flying. Thanks Haloa!
  - Fixes various autocomplete issues (especially with Better Continentds mod). Thanks Kurios.ZeuS!
  - Improves console behavior. Thanks Haloa!
