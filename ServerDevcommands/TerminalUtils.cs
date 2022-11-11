using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
namespace ServerDevcommands;
public static class TerminalUtils
{
  // Logic for input:
  // - Discard any previous commands (separated by ';') so other code doesn't have to consider ';' at all.
  // - Convert aliases to plain text to get parameter options working.
  // - Substitutions don't have to be handled (no need to worry about converting back).
  // - At end of handling, convert plain to aliases and restore discarded commands.
  private static int Anchor = 0;
  private static int Focus = 0;
  public static void ToCurrentInput(Terminal terminal)
  {
    var input = terminal.m_input;
    Anchor = input.selectionAnchorPosition;
    Focus = input.selectionFocusPosition;
    MultiCommands.DiscardPreviousCommands(input);
    Aliasing.RemoveAlias(input);
  }
  public static void ToActualInput(Terminal terminal)
  {
    var input = terminal.m_input;
    Aliasing.RestoreAlias(input);
    MultiCommands.RestorePreviousCommands(input);
    // Modifies the input and removes selection, so don't set it back.
    if (!Input.GetKeyDown(KeyCode.Tab))
    {
      input.selectionAnchorPosition = Anchor;
      input.selectionFocusPosition = Focus;
    }
  }
  public static string GetLastWord(Terminal obj) => obj.m_input.text.Split(' ').Last().Split('=').Last().Split(',').Last();
  public static IEnumerable<string> GetPositionalParameters(string[] parameters)
  {
    return parameters.Where(par => !par.Contains("="));
  }
  public static void GetSubstitutions(string[] parameters, out IEnumerable<string> mainPars, out IEnumerable<string> substitutions)
  {
    // Substitutions can only appear after the last substitution.
    // This allows using commands like "target remove id=$" without 'remove' always replacing '$'.
    // However still "target id=$ remove" won't work.
    var start = -1;
    for (var i = 0; i < parameters.Length; i++)
    {
      if (parameters[i].Contains(Settings.Substitution)) start = i + 1;
    }
    if (start == -1)
    {
      mainPars = parameters;
      substitutions = new string[0];
      return;
    }
    mainPars = parameters.Take(start);
    substitutions = parameters.Skip(start);
  }

  public static int GetAmountOfSubstitutions(string[] parameters)
  {
    var start = -1;
    for (var i = 0; i < parameters.Length; i++)
    {
      if (parameters[i].Contains(Settings.Substitution)) start = i + 1;
    }
    if (start == -1) return 0;
    return parameters.Skip(start).Where(par => !par.Contains("=")).Count();
  }

  private static string ReplaceFirst(string text, string search, string replace)
  {
    var pos = text.IndexOf(search);
    if (pos < 0) return text;
    return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
  }
  public static string Substitute(string input, string value)
  {
    if (CanSubstitute(input) && !value.Contains("="))
      return ReplaceFirst(input, Settings.Substitution, value);
    else
      return input + " " + value;
  }

  public static bool CanSubstitute(string input) => input.Contains(Settings.Substitution);
  public static string Substitute(string input)
  {
    if (Settings.Substitution == "") return input;
    if (input.StartsWith("alias ")) return input;
    if (!CanSubstitute(input)) return input;
    GetSubstitutions(input.Split(' '), out var mainPars, out var substitutions);
    input = string.Join(" ", mainPars);
    foreach (var parameter in substitutions)
      input = Substitute(input, parameter);
    // Removes any extra substitutions that didn't receive values so "cmd par=$$,$$" works with "foo 3".
    input = input.Replace($",{Settings.Substitution}", "");
    // Remove any trailing substitution that didn't receive a parameter so "cmd $$ $$" works with "foo 3".
    var parameters = input.Split(' ');
    while (parameters.Length > 0 && parameters.Last().Contains(Settings.Substitution))
      parameters = parameters.Take(parameters.Length - 1).ToArray();
    input = string.Join(" ", parameters);
    return input;
  }
  public static bool SkipProcessing(string command) => ParameterInfo.SpecialCommands.Any(cmd => command.StartsWith($"{cmd} ", StringComparison.OrdinalIgnoreCase));
  public static bool IsComposite(string command)
  {
    command = Aliasing.Plain(command);
    return ParameterInfo.CompositeCommands.Any(cmd => command.StartsWith($"{cmd} ", StringComparison.OrdinalIgnoreCase));
  }

  public static bool IsExecuting = false;
}

[HarmonyPatch(typeof(Terminal), nameof(Terminal.TryRunCommand))]
public class TryRunCommand
{
  static string CheckLogic(string text)
  {
    var args = text.Split(' ').Select(arg =>
    {
      var split = arg.Split('=');
      if (split.Length == 1) return Parse.Logic(arg);
      if (split.Length > 2) return arg;
      return $"{split[0]}={Parse.Logic(split[1])}";
    });
    return string.Join(" ", args);
  }
  static void SplitOff(Terminal __instance, string text)
  {
    // Must be split off one by one because later commands can be composite (and shouldn't be split).
    var split = MultiCommands.Split(text);
    var first = split.Take(1).First();
    var second = string.Join(";", split.Skip(1));
    __instance.TryRunCommand(first);
    __instance.TryRunCommand(second);
  }
  static bool Prefix(Terminal __instance, ref string text)
  {
    var isComposite = TerminalUtils.IsComposite(text);
    // Some commands (like alias or bind) are expected to be executed as they are.
    if (TerminalUtils.SkipProcessing(text)) return true;
    // Composites need aliasing for each part.
    text = string.Join(";", MultiCommands.Split(text).Select(s => TerminalUtils.Substitute(Aliasing.Plain(s))));
    // Multiple commands in an alias.
    if (!isComposite && MultiCommands.IsMulti(text))
    {
      SplitOff(__instance, text);
      return false;
    }
    if (!isComposite && !BindCommand.Valid(text)) return false;
    if (!isComposite)
    {
      text = BindCommand.CleanUp(text);
      text = CheckLogic(text);
    }
    // Server side checks this already at the server side execution.
    if (Player.m_localPlayer && !DisableCommands.CanRun(text)) return false;
    if (CommandQueue.CanRun())
    {
      string[] array = text.Split(' ');
      if (ZNet.instance && !ZNet.instance.IsServer() && Settings.IsServerCommand(array[0]))
      {
        ServerExecution.Send(text);
        return false;
      }
    }
    else
    {
      CommandQueue.Add(__instance, text);
      return false;
    }
    return true;
  }
}

[HarmonyPatch(typeof(Terminal), nameof(Terminal.UpdateInput))]
public class AliasInput
{
  private static string LastActual = "";
  static bool Prefix(Terminal __instance, ref string __state)
  {
    // Safe-guard because actions need different kind of input.
    if (Input.GetKeyDown(KeyCode.Return) && Input.GetKeyDown(KeyCode.Tab)) return false;
    // For execution, keep the actual input so that the history is saved properly.
    if (Input.GetKeyDown(KeyCode.Return)) return true;
    // Cycling commands doesn't need any modifications.
    if (ZInput.GetButtonDown("ChatUp") || ZInput.GetButtonDown("ChatDown")) return true;
    TerminalUtils.ToCurrentInput(__instance);
    if (Settings.DebugConsole)
    {
      var actual = __instance.m_input.text;
      actual = TerminalUtils.Substitute(actual);
      if (actual != LastActual)
        ServerDevcommands.Log.LogInfo("Command: " + actual);
      LastActual = actual;
    }
    return true;
  }
  static void Postfix(Terminal __instance)
  {
    // Same logic as on Prefix.
    if (Input.GetKeyDown(KeyCode.Return) || ZInput.GetButtonDown("ChatUp") || ZInput.GetButtonDown("ChatDown")) return;
    TerminalUtils.ToActualInput(__instance);
  }
}


///<summary>Needed to temporarily disable better autocomplete to provide case insensitivity for the first parameter.</summary>
[HarmonyPatch(typeof(Terminal.ConsoleCommand), nameof(Terminal.ConsoleCommand.RunAction))]
public class RunAction
{
  static void Prefix() => TerminalUtils.IsExecuting = true;
  static void Postfix() => TerminalUtils.IsExecuting = false;
}

[HarmonyPatch(typeof(Terminal), nameof(Terminal.Awake))]
public class UnlockCharacterLimit
{
  static void Postfix(Terminal __instance)
  {
    if (__instance.m_input) __instance.m_input.characterLimit = 0;
  }
}
