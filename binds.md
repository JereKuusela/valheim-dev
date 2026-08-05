# Key bindings

Server Devcommands fully replaces the original key binding system. This ensures that if the mod is removed, no stale binds will remain.

Binds are stored in `binds.yaml` in the BepInEx config folder. The file is automatically generated with the current key binds.

The file can be edited directly or with the `bind` and `unbind` commands.

## File format

Each bind is an object with these fields:

- `keys`: Keys that trigger the bind, separated by commas.
  - Minus sign means the key must not be held.
  - `wheel` means mouse wheel input.
- `state`: Optional mode filter such as `build`.
  - Minus sign means the mode must not be active.
- `command`: Command that runs when the keys are pressed.
- `offCommand`: Optional command that runs when the keys are released.

Typically only `keys` and `command` are used.

When multiple binds are valid, only the most specific one runs (the one with more required keys). For example, if both `j` and `j,leftalt` are bound, pressing J + LeftAlt only runs the second bind.

## Mouse wheel

Regular mouse actions are prevented when a wheel bind is triggered. This affects mods like Comfy Gizmo that use mouse wheel for some actions.

Binds with mouse wheel automatically add the wheel value as the last argument. This allows commands to react properly to both scroll directions.

For example, `bind wheel,o say` can print `3` when scrolling up and `-3` when scrolling down (depends on mouse settings).

Substitution `$$` can be used to insert the wheel value in the middle of the command. For example, `bind wheel,o say $$ test` would print `3 test` when scrolling up and `-3 test` when scrolling down.

## Console commands

- `bind [keys] [command] [parameters]` adds a bind.
  - Keys are comma-separated.
  - Negative sign means the key must not be held.
- `unbind [keycode]` removes all binds that require the key.
- `printbinds` shows current binds.
- `resetbinds` removes all binds.

Examples:

- `bind j god`
- `bind j,leftalt debugmode`
- `bind j,-leftalt god`
- `unbind j`
