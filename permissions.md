# Permissions

The permission system is optional and is meant for dedicated servers where owners want more detailed control.

If you are happy with the default admin-based system, you can ignore this feature.

## Idea

By default, admins have full access and non-admins only have default access.

The permission file allows adding overrides on top of that default:

- `yes`: Can be used to allow a specific feature or command for non-admins.
- `no`: Can be used to disable a specific feature or command for admins.
- `force`: Can be used to forcefully enable a specific feature for both admins and non-admins.
  - Note: Players can always uninstall this mod so you can't truly force anything on them.

Permission data is stored in `permissions.yaml` in the BepInEx config folder.

The file is automatically generated and updated:

- On first setup, it is created with `Everyone` entry that disables `permissions` command for everyone.
  - Note: The command is disabled by default to prevent abuse.
  - You can specifically enable it for yourself or just keep editing the file manually.
- Entries are automatically added for players that are using Server Devcommands mod.
- Entries are added per character (host name and character id).
  - Note: Character id can be easily spoofed, so make sure the host name is correct when granting permissions.

## Basic usage

Server Devcommands mod uses `serverdevcommands` section for its features. For other mods, you have to check their documentation to see if they support the permission system and which section they use.

Know mods:

- Server Devcommands: `serverdevcommands`
- ESP: `esp_stats` and `esp_visuals`

Server Devcommands supports these features inside its section:

- `disablestartshout`
- `fly`
- `ghost`
- `god`
- `hideshoutpings`
- `ignorenomap`
- `ignorewards`
- `mapcoordinates`
- `minimapcoordinates`
- `noclipcamera`
- `nocost`
- `nodrops`
- `showprivateplayers`

Note: All section and features names are case-insensitive, so you can mix uppercase letters as you like.

Command section works for all console commands, regardless of the mod. So there is no specific list for that.

### Basic example

```yaml
- name: Everyone
  commands:
    permissions: no

- id: Steam_123456789
  name: Jere
  character: 987654321
  serverdevcommands:
    fly: force
  commands:
    event: no
```

## Groups

Groups can be used to create a set of permissions which avoids repeating the same permissions for multiple characters.

For example, you can create a `moderators` group that has some basic permissions and then just add characters to that group.

Each character can be in multiple groups and each group can have multiple parent groups. This allows creating complex hierarchies if needed.

Character specific permissions always override group permissions. So if a character is in a group that allows `fly` but the character entry has `fly: no`, the character won't be able to fly.

Note: Group `Everyone` is always applied even when not explicitly listed. This allows setting server-wide defaults to new characters.

Groups are normal entries that have a `name` but no `id`/`character`.

### Group example

```yaml
- name: moderators
  serverdevcommands:
    fly: yes
    ghost: yes
  commands:
    event: no
- id: Steam_123456789
  name: Jere
  character: 987654321
  groups:
    - moderators
  commands:
    event: yes
```

## Permissions command

`permissions` command can be used to manage permissions without directly editing the file.

The command is disabled by default, so you may need to enable it first for your character by editing the file.

Operations:

- `add_group <group> <character id or name>`
- `remove_group <group> <character id or name>`
- `clear_groups <character id or name>`
- `add_command <command> <character id or name>`
- `ban_command <command> <character id or name>`
- `clear_command <command> <character id or name>`
- `add_feature <section> <feature> <character id or name>`
- `ban_feature <section> <feature> <character id or name>`
- `force_feature <section> <feature> <character id or name>`
- `clear_feature <section> <feature> <character id or name>`
- `clear_all <character id or name>`

Note: The command works for offline players too, as long as they have an entry in the permission file.

## Advanced usage

Here are some advanced cases to use the permission system more efficiently.

### Feature wildcards

Feature name `*` can be used to set default value for all features of that section.

For example to allow all features for non-admins.

### Command parameters

Commands can also include parameter values. For example `find Boar: yes` would only allow searching for boars, without giving access to the full command.

This doesn't work for all commands, as their parameters can have flexible position.
