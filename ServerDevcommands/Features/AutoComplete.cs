using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
namespace ServerDevcommands;
using NamedOptionsFetchers = Dictionary<string, Func<int, List<string>>>;
using OptionsFetcher = Func<int, int, List<string>>;
using SimpleOptionsFetcher = Func<int, List<string>>;
///<summary>Provides improved autocomplete (options/some info for each parameter, support for named parameters).</summary>
public static class AutoComplete
{
  private static readonly Dictionary<string, OptionsFetcher> OptionsFetchers = [];
  private static readonly Dictionary<string, NamedOptionsFetchers> OptionsNamedFetchers = [];
  ///<summary>Returns options, either from the custom or the default options fetcher.</summary>
  public static List<string>? GetOptions(string command, int index)
  {
    command = command.ToLower();
    if (OptionsFetchers.ContainsKey(command)) return OptionsFetchers[command](index, 0);
    if (index == 0 && Terminal.commands.TryGetValue(command, out var cmd) && cmd.m_tabOptionsFetcher != null)
      return cmd.m_tabOptionsFetcher();
    return null;
  }
  private static List<string> GetOptions(string command, int index, int subIndex, string namedParameter)
  {
    command = command.ToLower();
    if (namedParameter != "")
    {
      if (OptionsNamedFetchers.TryGetValue(command, out var namedOptions))
      {
        if (namedOptions.TryGetValue(namedParameter.ToLower(), out var namedFetcher))
        {
          return namedFetcher(index) ?? ParameterInfo.None;
        }
      }
      return ParameterInfo.InvalidNamed(namedParameter);
    }
    if (OptionsFetchers.ContainsKey(command)) return OptionsFetchers[command](index, subIndex) ?? ParameterInfo.None;
    if (Terminal.commands.TryGetValue(command, out var cmd) && cmd.m_tabOptionsFetcher != null)
      return cmd.m_tabOptionsFetcher() ?? ParameterInfo.Missing;
    return ParameterInfo.Missing;
  }
  ///<summary>Registers a new custom options fetcher.</summary>
  public static void Register(string command, OptionsFetcher fetcher, NamedOptionsFetchers namedFetchers)
  {
    OptionsFetchers[command.ToLower()] = fetcher;
    OptionsNamedFetchers[command.ToLower()] = namedFetchers.ToDictionary(kvp => kvp.Key.ToLower(), kvp => kvp.Value);
  }
  ///<summary>Registers a new custom options fetcher.</summary>
  public static void Register(string command, SimpleOptionsFetcher fetcher, NamedOptionsFetchers namedFetchers)
  {
    OptionsFetchers[command.ToLower()] = (int index, int subIndex) => fetcher(index);
    OptionsNamedFetchers[command.ToLower()] = namedFetchers.ToDictionary(kvp => kvp.Key.ToLower(), kvp => kvp.Value);
  }
  ///<summary>Registers a new custom options fetcher.</summary>
  public static void Register(string command, OptionsFetcher fetcher)
  {
    OptionsFetchers[command] = fetcher;
  }
  ///<summary>Registers a new custom options fetcher.</summary>
  public static void Register(string command, SimpleOptionsFetcher fetcher)
  {
    OptionsFetchers[command] = (int index, int subIndex) => fetcher(index);
  }

  ///<summary>Registers an options fetcher without parameters.</summary>
  public static void RegisterEmpty(string command)
  {
    Register(command, (int index) => ParameterInfo.None);
  }
  ///<summary>Registers an options fetcher for an admion action.</summary>
  public static void RegisterAdmin(string command)
  {
    Register(command, (int index) =>
    {
      if (index == 0) return ParameterInfo.Create("Name / IP / UserId");
      return ParameterInfo.None;
    });
  }
  ///<summary>Registers an options fetcher with only the default fetcher.</summary>
  public static void RegisterDefault(string command)
  {
    Register(command, (int index) =>
    {
      if (index == 0) return Terminal.commands[command].m_tabOptionsFetcher();
      return ParameterInfo.None;
    });
  }
  public static Dictionary<string, int> Offsets = [];
  public static List<string> GetOptions(string[] parameters)
  {
    var commandName = parameters.First();
    if (Offsets.TryGetValue(commandName, out var offset) && parameters.Length - 1 > offset)
    {
      parameters = parameters.Skip(offset + 1).ToArray();
      commandName = parameters.First();
    }
    if (parameters.Length < 2)
    {
      if (commandName.StartsWith("_", StringComparison.OrdinalIgnoreCase))
        return Terminal.commands.Keys.Where(cmd => cmd.StartsWith("_", StringComparison.OrdinalIgnoreCase)).ToList();
      else
        return Terminal.commands.Keys.Where(cmd => !cmd.StartsWith("_", StringComparison.OrdinalIgnoreCase)).ToList();
    }
    parameters = parameters.Skip(1).ToArray();
    var parameter = parameters.Last();
    var name = GetName(parameter);
    var subIndex = GetSubIndex(parameter);
    int index;
    if (name != "")
    {
      // Named parameter can appear anywhere so makes more sense to return their internal index.
      index = subIndex;
    }
    else
    {
      // Ignore named parameters for the index.
      index = TerminalUtils.GetPositionalParameters(parameters).Count() - 1;
      if (Settings.Substitution != "")
      {
        var substitutions = TerminalUtils.GetAmountOfSubstitutions(parameters);
        for (var i = 0; i < parameters.Length; i++)
        {
          if (substitutions <= 0) break; // Early break if there are no substitutions.
          var par = parameters[i];
          var count = CountSubstitution(par);
          // Ignore substituded parameters for the index.
          index -= count;
          substitutions -= count;
          if (substitutions <= 0)
          {
            // For substituded, use the name/index where it appears.
            name = GetName(par);
            if (name != "")
            {
              // Cases to handle:
              // 1. =$$ foo,bar -> substitutions: 0, parameter: 1, par: 0 => 1
              // 2. =$$,$$ foo ->  substitutions: -1, parameter: 0, par: 1 => 0
              // 3. =$$,$$ foo bar -> substitutions: 0, parameter: 0, par: 1 => 1
              // 4. =foo,$$ bar -> substitutions: 0, parameter: 0, par: 1 => 1
              index = substitutions + GetSubIndex(parameter) + GetSubIndex(par);
            }
            else
            {
              index = i;
            }
          }
        }
      }
    }
    return GetOptions(commandName, index, subIndex, name);
  }

  private static string GetName(string parameter)
  {
    var split = parameter.Split('=');
    if (split.Length < 2) return "";
    return split[0];
  }
  private static int GetSubIndex(string parameter) => parameter.Split(',').Length - 1;

  private static int CountSubstitution(string parameter)
  {
    var count = 0;
    var len = Settings.Substitution.Length;
    var index = -len;
    while ((index = parameter.IndexOf(Settings.Substitution, index + len)) > -1)
    {
      count += 1;
    }
    return count;
  }
}


[HarmonyPatch(typeof(Terminal.ConsoleCommand), nameof(Terminal.ConsoleCommand.GetTabOptions))]
public class GetTabOptionsWithImprovedAutoComplete
{
  static bool Prefix(Terminal __instance, ref List<string> __result)
  {
    if (__instance == Chat.instance) return true;
    if (!Settings.ImprovedAutoComplete) return true;

    // While executing, options are used to make the first parameter case insensitive.
    // Own implementation is used so return nothing to prevent the default one from doing anything.
    if (TerminalUtils.IsExecuting)
    {
      __result = [];
    }
    else
    {
      var text = Console.instance.m_input.text;
      var parameters = text.Split(';').Last().Split(' ');
      __result = AutoComplete.GetOptions(parameters);
    }
    return false;
  }
}

[HarmonyPatch(typeof(Terminal), nameof(Terminal.tabCycle))]
public class TabCycleWithImprovedAutoComplete
{
  static void Prefix(Terminal __instance, ref List<string> options, ref string word, bool usePrefix)
  {
    if (usePrefix)
    {
      if (word.StartsWith("_", StringComparison.OrdinalIgnoreCase))
        options = options.Where(cmd => cmd.StartsWith("_", StringComparison.OrdinalIgnoreCase)).ToList();
      else
        options = options.Where(cmd => !cmd.StartsWith("_", StringComparison.OrdinalIgnoreCase)).ToList();
    }
    if (!Settings.ImprovedAutoComplete || __instance == Chat.instance) return;
    // Auto complete is parameter specific, so need to use the current word instead of always using the first.
    word = TerminalUtils.GetLastWord(__instance);
    if (options == null) return;
    // Cycling the help text wouldn't make any sense.
    options = options.Where(option => !option.Contains("?")).ToList();
  }
}

[HarmonyPatch(typeof(Terminal), nameof(Terminal.updateSearch))]
public class UpdateSearchWithImprovedAutoComplete
{
  static void Prefix(Terminal __instance, ref string word)
  {
    if (!Settings.ImprovedAutoComplete || !__instance.m_search || __instance == Chat.instance) return;
    // Auto complete is parameter specific, so need to use the current word instead of always using the first.
    word = TerminalUtils.GetLastWord(__instance);

  }
  static void Postfix(Terminal __instance, List<string> options, ref string word, bool usePrefix)
  {
    if (usePrefix)
    {
      if (word.StartsWith("_", StringComparison.OrdinalIgnoreCase))
        options = options.Where(cmd => cmd.StartsWith("_", StringComparison.OrdinalIgnoreCase)).ToList();
      else
        options = options.Where(cmd => !cmd.StartsWith("_", StringComparison.OrdinalIgnoreCase)).ToList();
    }
    if (!Settings.ImprovedAutoComplete || options == null || !__instance.m_search || __instance == Chat.instance) return;
    if (Settings.CommandDescriptions && options == __instance.m_commandList)
    {
      if (Terminal.commands.TryGetValue(word, out var command))
        options = ParameterInfo.Create(command.Description);
    }
    var helpText = options.All(option => option.StartsWith("?"));
    // Always show the help text since there isn't any real search option.
    if (helpText)
      __instance.m_search.text = "<color=white>" + string.Join(", ", options.Select(option => option.Substring(1))) + "</color>";
  }
}

[HarmonyPatch(typeof(Terminal), nameof(Terminal.UpdateInput))]
public class PlainInputForAutoComplete
{
  static bool Prefix(Terminal __instance)
  {
    // Chat doesn't have autocomplete so no need to do anything.
    if (__instance == Chat.instance) return true;
    // Safe-guard because actions need different kind of input.
    if (ZInput.GetKeyDown(KeyCode.Return) && ZInput.GetKeyDown(KeyCode.Tab)) return false;
    // For execution, keep the actual input so that the history is saved properly.
    if (ZInput.GetKeyDown(KeyCode.Return)) return true;
    // Cycling commands doesn't need any modifications.
    if (ZInput.GetButtonDown("ChatUp") || ZInput.GetButtonDown("ChatDown")) return true;
    // Copy paste thing requires the actual input.
    if (ZInput.GetKey(KeyCode.LeftControl) || ZInput.GetKey(KeyCode.RightControl)) return true;
    ToCurrentInput(__instance);
    return true;
  }
  static void Postfix(Terminal __instance)
  {
    // Chat doesn't have autocomplete so no need to do anything.
    if (__instance == Chat.instance) return;
    // Same logic as on Prefix.
    if (ZInput.GetKeyDown(KeyCode.Return) || ZInput.GetButtonDown("ChatUp") || ZInput.GetButtonDown("ChatDown")) return;
    if (ZInput.GetKey(KeyCode.LeftControl) || ZInput.GetKey(KeyCode.RightControl)) return;
    ToActualInput(__instance);
  }

  // Logic for input:
  // - Discard any previous commands (separated by ';') so other code doesn't have to consider ';' at all.
  // - Convert aliases to plain text to get parameter options working.
  // - Substitutions don't have to be handled (no need to worry about converting back).
  // - At end of handling, convert plain to aliases and restore discarded commands.
  private static int Anchor = 0;
  private static int Focus = 0;
  private static void ToCurrentInput(Terminal terminal)
  {
    var input = terminal.m_input;
    Anchor = input.selectionAnchorPosition;
    Focus = input.selectionFocusPosition;
    DiscardPreviousCommands(input);
    Aliasing.RemoveAlias(input);
  }
  private static void ToActualInput(Terminal terminal)
  {
    var input = terminal.m_input;
    Aliasing.RestoreAlias(input);
    RestorePreviousCommands(input);
    // Modifies the input and removes selection, so don't set it back.
    if (ZInput.GetKeyDown(KeyCode.Tab))
    {
      input.selectionAnchorPosition = input.text.Length;
      input.selectionFocusPosition = input.text.Length;
    }
    else
    {
      input.selectionAnchorPosition = Anchor;
      input.selectionFocusPosition = Focus;
    }
  }


  private static string[]? PreviousCommands = null;
  private static int CurrentCommand = -1;
  private static int DiscardCaretDelta = 0;
  ///<summary>Discarding previous commands makes handling much simpler.</summary>
  private static void DiscardPreviousCommands(Fishlabs.GuiInputField input)
  {
    if (!Settings.MultiCommand) return;
    PreviousCommands = input.text.Split(';');
    DiscardCaretDelta = input.caretPosition;
    for (CurrentCommand = 0; CurrentCommand < PreviousCommands.Length; CurrentCommand++)
    {
      var command = PreviousCommands[CurrentCommand];
      if (input.caretPosition > command.Length)
        input.caretPosition -= command.Length;
      else break;
      // ';" character.
      input.caretPosition--;
    }
    DiscardCaretDelta -= input.caretPosition;
    if (CurrentCommand >= PreviousCommands.Length)
    {
      PreviousCommands = null;
      CurrentCommand = -1;
    }
    else
      input.text = PreviousCommands[CurrentCommand];
  }
  private static void RestorePreviousCommands(Fishlabs.GuiInputField input)
  {
    if (PreviousCommands == null) return;
    PreviousCommands[CurrentCommand] = input.text;
    input.text = string.Join(";", PreviousCommands);
    if (DiscardCaretDelta != 0) input.caretPosition += DiscardCaretDelta;
    PreviousCommands = null;
    CurrentCommand = -1;
  }
}

