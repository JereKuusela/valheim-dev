using GUIFramework;

namespace ServerDevcommands;
public static class Aliasing
{
  private static string Alias = "";
  public static void RemoveAlias(GuiInputField input)
  {
    if (!Settings.Aliasing) return;
    Alias = GetAlias(input.text);
    if (Alias == string.Empty) return;
    // Inputting only the alias should behave like a command instead
    // instantly providing autocomplete for the plain text.
    if (Alias == input.text)
    {
      Alias = string.Empty;
      return;
    }
    var plain = Plain(Alias);
    input.text = plain + input.text.Substring(Alias.Length);
    input.caretPosition += plain.Length - Alias.Length;
  }
  public static void RestoreAlias(GuiInputField input)
  {
    if (!Settings.Aliasing) return;
    if (Alias == string.Empty) return;
    var plain = Plain(Alias);
    // Whole text can change.
    if (!input.text.StartsWith(plain)) return;
    input.caretPosition -= plain.Length - Alias.Length;
    input.text = Alias + input.text.Substring(plain.Length);
  }

  ///<summary>Converts a given command to plain text (without aliases).</summary>
  public static string Plain(string command) => Plain(command, 10);
  private static string Plain(string command, int rounds)
  {
    if (!Settings.Aliasing) return command;
    // This functions gets constantly called so this can help with the performance.
    if (command == "") return "";
    if (TerminalUtils.SkipProcessing(command)) return command;
    if (rounds == 0) return command;
    foreach (var key in Settings.AliasKeys)
    {
      if (command.Length < key.Length) continue;
      if (command == key) return Plain(Settings.GetAliasValue(key), rounds - 1);
      if (command != key)
      {
        if (!command.StartsWith(key)) continue;
        var nextChar = command[key.Length];
        if (nextChar != ' ' && nextChar != ',' && nextChar != '=' && nextChar != ';') continue;
      }
      var alias = Settings.GetAliasValue(key);
      command = TerminalUtils.Substitute(alias, command.Substring(key.Length + 1));
      return Plain(command, rounds - 1);

    }
    return command;
  }
  ///<summary>Returns the alias of a given command (or empty string if not any).</summary>
  private static string GetAlias(string command)
  {
    // This functions gets constantly called so this can help with the performance.
    if (command == "") return "";
    if (TerminalUtils.SkipProcessing(command)) return command;
    foreach (var key in Settings.AliasKeys)
    {
      if (command.Length < key.Length) continue;
      if (command != key)
      {
        if (!command.StartsWith(key)) continue;
        var nextChar = command[key.Length];
        if (nextChar != ' ' && nextChar != ',' && nextChar != '=') continue;
      }
      return key;
    }
    return "";
  }
}
