# Commands

Server Devcommands extends command parsing and execution in both console and chat.

When a command is entered, these steps happen in order:

1. Some special commands are executed "as is" without any processing.
2. If command requires values from UI input, the execution is delayed until the values are given.
3. Aliases are expanded to actual commands.
4. Multi-commands are split into separate commands, and executed in sequence.
5. Logic expressions are handled.
6. Command existence and permissions are validated.
7. Wrapped values are joined into one parameter before command action runs.

## No processing

Some commands handle raw input, so they are executed without any processing. For example `alias`, `bind` or `server`.

## Values from UI input

If command contains `<input>`, an UI input is shown to the user. The command execution is delayed until the value is given.

Title of the input can be changed by putting a name after `input_`. For example, `<input_Player_name>` would show "Player Name" as the title.

If command requires multiple values, they are resolved one by one in appearance order. For example, `tp <input_player_name> <input_coordinates>` would first ask for player name and then coordinates.

## Alias expansion

New commands can be created by defining aliases.  Aliases are stored in `alias.yaml` in the BepInEx config folder.

- `alias [name] [command]` adds or updates an alias.
- `alias` prints aliases.
- `alias [name]` removes the alias.

Aliases are usually done to shorten long commands. They can also include parameters and other aliases.

Substitution `$$` can be used to insert parameters in the middle of the command.

Examples:

- `alias s_o spawn_object` allows using `s_o` instead of `spawn_object`.
- `alias wolf spawn wolf` allows using `wolf 5` to spawn 5 wolves
- `alias star1 spawn $$ $$ 2` allows using `star1 wolf` to spawn starred wolf or `star1 goblin 5` to spawn 5 starred goblins.
- `alias wolf1 star1 wolf` allows using `wolf1` to spawn a single starred wolf.

## Multiple commands

Multiple commands can be executed sequentially by separating them with `;`.

The `wait [milliseconds]` command allows delaying the next command.

Examples:

- `debugmode; fly` enables debug mode and then flying.
- `fly; wait 10000; fly` enables flying, waits 10 seconds and then disables flying.

## Logic parsing

Logic parsing is a simple ternary-style shortcut:

- Format: `condition?when_true:when_false`
- If both `?` and `:` are not present in the same token, value is left unchanged.
- Condition is evaluated with boolean parsing (`1/t/true/yes/on` are true, `0/f/false/no/off` are false).
- Unknown conditions are treated as false.

How it is applied:

- For standalone tokens, the whole token is parsed.
- For named tokens (`key=value`), only the value part is parsed.
- Tokens with multiple `=` (for example `a=b=c`) are left unchanged.

Examples:

- `fly true?on:off` becomes `fly on`.
- `spawn wolf amount=true?5:1` becomes `spawn wolf amount=5`.
- `spawn wolf amount=maybe?5:1` becomes `spawn wolf amount=1` (unknown condition is false).

In multi-command input (`;`), each command token list is parsed independently when it executes.

## Wrapping (quoted parameters)

Wrapped values are joined into one parameter. This is needed when parameters contain spaces.

Example:

- `broadcast center "<color=red>Hello world</color>"`
