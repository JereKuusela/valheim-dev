using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace DEV {
  public static class TerminalUtils {
    public static string GetLastWord(Terminal obj) => obj.m_input.text.Split(' ').Last().Split('=').Last().Split('|').Last();
    public static IEnumerable<string> GetPositionalParameters(string[] parameters) {
      return parameters.Where(par => !par.Contains("="));
    }
    public static IEnumerable<string> GetSubstitutions(string[] parameters) {
      // Substitutions can only appear after the last substitution.
      // This allows using commands like "target remove id=$" without 'remove' always replacing '$'.
      // However still "target id=$ remove" won't work.
      var start = -1;
      for (var i = 0; i < parameters.Length; i++) {
        if (parameters[i].Contains("$")) start = i + 1;
      }
      if (start == -1) return new string[0];
      return parameters.Skip(start).Where(par => !par.Contains("="));
    }
    private static string ReplaceFirst(string text, string search, string replace) {
      var pos = text.IndexOf(search);
      if (pos < 0) return text;
      return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
    }
    public static string Substitute(string input) {
      if (!input.Contains("$")) return input;
      var substitutions = GetSubstitutions(input.Split(' '));
      foreach (var parameter in substitutions) {
        input = ReplaceFirst(ReplaceFirst(input, "$", parameter), " " + parameter, "");
      }
      input = string.Join(" ", input.Split(' ').Where(par => !par.Contains("$")));
      return input;
    }
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
      if (!text.StartsWith("bind ")) {
        if (!CheckModifierKeys(text)) return false;
        text = RemoveModifierKeys(text);
      }
      string[] array = text.Split(' ');
      if (ZNet.instance && !ZNet.instance.IsServer() && IsServerSide(array[0])) {
        BaseCommands.SendCommand(text);
        return false;
      }
      return true;
    }
  }


}
