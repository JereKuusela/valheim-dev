using System;
using System.Linq;
using UnityEngine;

namespace ServerDevcommands {
  public class ModifierKeys {
    ///<summary>Returns whether the command is valid with current modifier keys.</summary>
    public static bool IsValid(string command) {
      if (!command.Contains("keys=")) return true;
      var args = command.Split(' ');
      var arg = args.First(arg => arg.StartsWith("keys=")).Split('=');
      if (arg.Length < 2) return true;
      var keys = Parse.Split(arg[1]);
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

    ///<summary>Removes keys parameter from the command.</summary>
    public static string CleanUp(string command) =>
      string.Join(" ", command.Split(' ').Where(arg => !arg.StartsWith("keys=")));
  }
}
