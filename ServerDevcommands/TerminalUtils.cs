using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
namespace ServerDevcommands;
#pragma warning disable IDE0046
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

  private static string ReplaceValues(string text, string search, Queue<string> replace)
  {
    var pos = text.IndexOf(search);
    while (pos >= 0 && replace.Count > 0)
    {
      var value = replace.Dequeue();
      text = text.Substring(0, pos) + value + text.Substring(pos + search.Length);
      pos = text.IndexOf(search);
    }
    return text;
  }

  public static bool CanSubstitute(string input) => input.Contains(Settings.Substitution);

  public static string Substitute(string alias, string command)
  {
    if (Settings.Substitution == "") return alias + " " + command;
    if (alias.StartsWith("alias")) return alias + " " + command;
    if (!CanSubstitute(alias)) return alias + " " + command;
    var substitutions = new Queue<string>(command.Split(' '));
    alias = ReplaceValues(alias, Settings.Substitution, substitutions);
    // Removes any extra substitutions that didn't receive values so "cmd par=$$,$$" works with "foo 3".
    alias = alias.Replace($",{Settings.Substitution}", "");
    // Removes any extra substitutions that didn't receive values so "cmd $$ $$" works with "foo 3".
    if (CanSubstitute(alias))
      alias = string.Join(" ", alias.Split(' ').Where(s => !s.Contains(Settings.Substitution)));
    return alias + " " + string.Join(" ", substitutions);

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
    if (!Settings.ImprovedChat && __instance == Chat.instance) return true;
    var isComposite = TerminalUtils.IsComposite(text);
    // Some commands (like alias or bind) are expected to be executed as they are.
    if (TerminalUtils.SkipProcessing(text)) return true;
    // Composites need aliasing for each part.
    text = string.Join(";", MultiCommands.Split(text).Select(s => Aliasing.Plain(s)));
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
    if (!CommandQueue.CanRun())
    {
      CommandQueue.Add(__instance, text);
      return false;
    }
    return true;
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
    if (Input.GetKeyDown(KeyCode.Return) && Input.GetKeyDown(KeyCode.Tab)) return false;
    // For execution, keep the actual input so that the history is saved properly.
    if (Input.GetKeyDown(KeyCode.Return)) return true;
    // Cycling commands doesn't need any modifications.
    if (ZInput.GetButtonDown("ChatUp") || ZInput.GetButtonDown("ChatDown")) return true;
    TerminalUtils.ToCurrentInput(__instance);
    return true;
  }
  static void Postfix(Terminal __instance)
  {
    // Chat doesn't have autocomplete so no need to do anything.
    if (__instance == Chat.instance) return;
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
[HarmonyPatch(typeof(Terminal.ConsoleEventArgs), MethodType.Constructor, typeof(string), typeof(Terminal))]
public class Wrapping
{
  static void Postfix(Terminal.ConsoleEventArgs __instance)
  {
    if (string.IsNullOrWhiteSpace(Settings.Wrapping)) return;
    if (!__instance.FullLine.Contains(Settings.Wrapping)) return;
    List<string> pieces = [];
    var store = "";
    foreach (var arg in __instance.Args)
    {
      if (store == "")
      {
        if (arg.Contains(Settings.Wrapping))
        {
          var matchingWrap = arg.Count(c => c == Settings.Wrapping[0]) % 2 == 0;
          if (matchingWrap && arg.EndsWith(Settings.Wrapping, StringComparison.OrdinalIgnoreCase))
          {
            // Special case for wrapped without spaces.
            var startWrapper = arg.IndexOf(Settings.Wrapping);
            var endWrapper = arg.LastIndexOf(Settings.Wrapping);
            var wrapped = arg.Remove(endWrapper, 1).Remove(startWrapper, 1);
            pieces.Add(wrapped);
          }
          else
            store = arg;
        }
        else
          pieces.Add(arg);
      }
      else
      {
        store += " " + arg;
        if (arg.EndsWith(Settings.Wrapping, StringComparison.OrdinalIgnoreCase))
        {
          var startWrapper = store.IndexOf(Settings.Wrapping);
          var endWrapper = store.LastIndexOf(Settings.Wrapping);
          var wrapped = store.Remove(endWrapper, 1).Remove(startWrapper, 1);
          pieces.Add(wrapped);
          store = "";
        }
      }
    }
    __instance.Args = [.. pieces];
  }
}
