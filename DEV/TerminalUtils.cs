using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace DEV {
  public static class TerminalUtils {
    // Logic for input:
    // - Discard any previous commands (separated by ';') so other code doesn't have to consider ';' at all.
    // - Convert aliases to plain text to get parameter options working.
    // - Substitutions don't have to be handled (no need to worry about converting back).
    // - At end of handling, convert plain to aliases and restore discarded commands.
    private static int Anchor = 0;
    private static int Focus = 0;
    public static void ToCurrentInput(Terminal terminal) {
      var input = terminal.m_input;
      Anchor = input.selectionAnchorPosition;
      Focus = input.selectionFocusPosition;
      MultiCommands.DiscardPreviousCommands(input);
      Aliasing.RemoveAlias(input);
    }
    public static void ToActualInput(Terminal terminal) {
      var input = terminal.m_input;
      Aliasing.RestoreAlias(input);
      MultiCommands.RestorePreviousCommands(input);
      // Modifies the input and removes selection, so don't set it back.
      if (!Input.GetKeyDown(KeyCode.Tab)) {
        input.selectionAnchorPosition = Anchor;
        input.selectionFocusPosition = Focus;
      }
    }
    public static string GetLastWord(Terminal obj) => obj.m_input.text.Split(' ').Last().Split('=').Last().Split(',').Last();
    public static IEnumerable<string> GetPositionalParameters(string[] parameters) {
      return parameters.Where(par => !par.Contains("="));
    }
    public static void GetSubstitutions(string[] parameters, out IEnumerable<string> mainPars, out IEnumerable<string> substitutions) {
      // Substitutions can only appear after the last substitution.
      // This allows using commands like "target remove id=$" without 'remove' always replacing '$'.
      // However still "target id=$ remove" won't work.
      var start = -1;
      for (var i = 0; i < parameters.Length; i++) {
        if (parameters[i].Contains("$")) start = i + 1;
      }
      if (start == -1) {
        mainPars = parameters;
        substitutions = new string[0];
        return;
      }
      mainPars = parameters.Take(start);
      substitutions = parameters.Skip(start);
    }

    public static int GetAmountOfSubstitutions(string[] parameters) {
      var start = -1;
      for (var i = 0; i < parameters.Length; i++) {
        if (parameters[i].Contains("$")) start = i + 1;
      }
      if (start == -1) return 0;
      return parameters.Skip(start).Where(par => !par.Contains("=")).Count();
    }

    private static string ReplaceFirst(string text, string search, string replace) {
      var pos = text.IndexOf(search);
      if (pos < 0) return text;
      return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
    }
    public static string Substitute(string input) {
      if (input.StartsWith("alias ")) return input;
      if (!input.Contains("$")) return input;
      GetSubstitutions(input.Split(' '), out var mainPars, out var substitutions);
      input = string.Join(" ", mainPars);
      foreach (var parameter in substitutions) {
        if (parameter.Contains("=") || !input.Contains("$")) {
          input += " " + parameter;
        } else
          input = ReplaceFirst(input, "$", parameter);
      }
      // Removes any extra substitutions that didn't receive values so "cmd par=$,$" works with "foo 3".
      input = input.Replace(",$", "");
      // Remove any trailing substitution that didn't receive a parameter so "cmd $ $" works with "foo 3".
      var parameters = input.Split(' ');
      while (parameters.Length > 0 && parameters.Last().Contains("$"))
        parameters = parameters.Take(parameters.Length - 1).ToArray();
      input = string.Join(" ", parameters);
      return input;
    }
    public static bool SkipProcessing(string command) => command.StartsWith("bind ") || command.StartsWith("alias ");
  }

  // Replace devcommands check with a custom one.
  [HarmonyPatch(typeof(Terminal), "TryRunCommand")]
  public class TryRunCommand {
    private static bool IsServerSide(string command) {
      return command == "randomevent" || command == "stopevent" || command == "genloc" || command == "sleep" || command == "skiptime";
    }
    ///<summary>Only executes the command when specified keys are down.</summary>
    private static bool CheckModifierKeys(string command) {
      if (!command.Contains("keys=")) return true;
      var args = command.Split(' ');
      var arg = args.First(arg => arg.StartsWith("keys=")).Split('=');
      if (arg.Length < 2) return true;
      var keys = arg[1].Split(',');
      foreach (var key in keys) {
        if (key.StartsWith("-")) {
          if (Enum.TryParse<KeyCode>(key.Substring(1), true, out var keyCode)) {
            if (Input.GetKey(keyCode)) return false;
          }

        } else {
          if (Enum.TryParse<KeyCode>(key, true, out var keyCode)) {
            if (!Input.GetKey(keyCode)) return false;
          }

        }
      }
      return true;
    }
    private static string RemoveModifierKeys(string command) =>
      string.Join(" ", command.Split(' ').Where(arg => !arg.StartsWith("keys=")));

    public static bool Prefix(Terminal __instance, ref string text) {
      // Alias and bind can contain any kind of commands so avoid any processing.
      if (TerminalUtils.SkipProcessing(text)) return true;
      // Multiple commands in actual input.
      if (MultiCommands.IsMulti(text)) {
        foreach (var cmd in MultiCommands.Split(text)) __instance.TryRunCommand(cmd);
        return false;
      }
      if (Settings.Aliasing)
        text = Aliasing.Plain(text);
      if (Settings.Substitution)
        text = TerminalUtils.Substitute(text);
      // Multiple commands in an alias.
      if (MultiCommands.IsMulti(text)) {
        foreach (var cmd in MultiCommands.Split(text)) __instance.TryRunCommand(cmd);
        return false;
      }
      if (!CheckModifierKeys(text)) return false;
      text = RemoveModifierKeys(text);
      string[] array = text.Split(' ');
      if (ZNet.instance && !ZNet.instance.IsServer() && IsServerSide(array[0])) {
        ServerCommand.Send(text);
        return false;
      }
      return true;
    }
  }

  [HarmonyPatch(typeof(Terminal), "UpdateInput")]
  public class AliasInput {
    private static string LastActual = "";
    public static bool Prefix(Terminal __instance, ref string __state) {
      // Safe-guard because actions need different kind of input.
      if (Input.GetKeyDown(KeyCode.Return) && Input.GetKeyDown(KeyCode.Tab)) return false;
      // For execution, keep the actual input so that the history is saved properly.
      if (Input.GetKeyDown(KeyCode.Return)) return true;
      // Cycling commands doesn't need any modifications.
      if (ZInput.GetButtonDown("ChatUp") || ZInput.GetButtonDown("ChatDown")) return true;
      TerminalUtils.ToCurrentInput(__instance);
      if (Settings.DebugConsole) {
        var actual = __instance.m_input.text;
        if (Settings.Substitution)
          actual = TerminalUtils.Substitute(actual);
        if (actual != LastActual)
          DEV.Log.LogInfo("Command: " + actual);
        LastActual = actual;
      }
      return true;
    }
    public static void Postfix(Terminal __instance) {
      // Same logic as on Prefix.
      if (Input.GetKeyDown(KeyCode.Return) || ZInput.GetButtonDown("ChatUp") || ZInput.GetButtonDown("ChatDown")) return;
      TerminalUtils.ToActualInput(__instance);
    }
  }
}
