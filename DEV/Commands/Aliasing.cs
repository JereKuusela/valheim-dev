using System.Linq;
using HarmonyLib;

namespace DEV {
  public class AliasCommand : BaseCommands {
    ///<summary>Converts a given command to plain text (without aliases).</summary>
    public static string Plain(string command, int round = 0) {
      // This functions gets constantly called so this can help with the performance.
      if (command == "") return "";
      if (command.StartsWith("alias ")) return command;
      if (round == 10) return command;
      foreach (var key in Settings.AliasKeys) {
        if (command.Length < key.Length) continue;
        if (command != key) {
          if (!command.StartsWith(key)) continue;
          var nextChar = command[key.Length];
          if (nextChar != ' ' && nextChar != '|' && nextChar != '=') continue;
        }
        command = Settings.GetAlias(key) + command.Substring(key.Length);
        return Plain(command, round + 1);

      }
      return command;
    }
    ///<summary>Returns the alias of a given command (or empty string if not any).</summary>
    public static string GetAlias(string command) {
      // This functions gets constantly called so this can help with the performance.
      if (command == "") return "";
      if (command.StartsWith("alias ")) return ""; ;
      foreach (var key in Settings.AliasKeys) {
        if (command.Length < key.Length) continue;
        if (command != key) {
          if (!command.StartsWith(key)) continue;
          var nextChar = command[key.Length];
          if (nextChar != ' ' && nextChar != '|' && nextChar != '=') continue;
        }
        return key;
      }
      return "";
    }
    ///<summary>Adds an alias as an actual command to provide best support..</summary>
    public static void AddCommand(string key, string value) {
      var baseCommand = AliasCommand.Plain(value).Split(' ').First();
      if (Terminal.commands.TryGetValue(baseCommand, out var command))
        new Terminal.ConsoleCommand(key, command.Description, command.action, command.IsCheat, command.IsNetwork, command.OnlyServer, command.IsSecret, command.AllowInDevBuild, command.m_tabOptionsFetcher);
      else
        new Terminal.ConsoleCommand(key, "", delegate (Terminal.ConsoleEventArgs args) { });
    }

    public AliasCommand() {
      new Terminal.ConsoleCommand("alias", "[command] - Sets a command alias.", delegate (Terminal.ConsoleEventArgs args) {
        if (args.Length < 2) {
          args.Context.AddString(string.Join("\n", Settings.AliasKeys.Select(key => key + " -> " + Settings.GetAlias(key))));
        } else if (args.Length < 3) {
          Settings.RemoveAlias(args[1]);
          if (Terminal.commands.ContainsKey(args[1])) Terminal.commands.Remove(args[1]);
          args.Context.updateCommandList();
        } else {
          var value = string.Join(" ", args.Args.Skip(2));
          Settings.AddAlias(args[1], value);
          AddCommand(args[1], value);
          args.Context.updateCommandList();
        }
      });
    }

  }

  [HarmonyPatch(typeof(Terminal), "TryRunCommand")]
  public class TryRunCommandWithAliasing {

    private static string ReplaceFirst(string text, string search, string replace) {
      var pos = text.IndexOf(search);
      if (pos < 0) return text;
      return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
    }
    public static void Prefix(ref string text, Terminal __instance) {
      text = AliasCommand.Plain(text);
      text = TerminalUtils.Substitute(text);
    }
  }
  [HarmonyPatch(typeof(Terminal), "InputText")]
  public class InputText {

    private static string ReplaceFirst(string text, string search, string replace) {
      var pos = text.IndexOf(search);
      if (pos < 0) return text;
      return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
    }
    public static void Prefix(Terminal __instance) {
      var input = __instance.m_input;
      input.text = TerminalUtils.Substitute(input.text);
    }
  }
  [HarmonyPatch(typeof(Terminal), "UpdateInput")]
  public class AliasInput {
    private static string LastActual = "";
    public static void Prefix(Terminal __instance, ref string __state) {
      __state = AliasCommand.GetAlias(__instance.m_input.text);
      if (__state == string.Empty) return;
      var alias = AliasCommand.Plain(__state);
      __instance.m_input.text = __instance.m_input.text.Replace(__state, alias);
      __instance.m_input.caretPosition += alias.Length - __state.Length;
      if (Settings.DebugConsole) {
        var actual = TerminalUtils.Substitute(__instance.m_input.text);
        if (actual != LastActual)
          DEV.Log.LogInfo("Command: " + actual);
        LastActual = actual;
      }
    }
    public static void Postfix(Terminal __instance, string __state) {
      if (__state == string.Empty) return;
      var alias = AliasCommand.Plain(__state);
      if (__instance.m_input.text.StartsWith(alias)) {
        __instance.m_input.caretPosition -= alias.Length - __state.Length;
        __instance.m_input.text = __instance.m_input.text.Replace(alias, __state);
      }
    }
  }
}
