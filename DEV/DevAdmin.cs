using System;
using System.Linq;
using HarmonyLib;
using Service;
using UnityEngine;

namespace DEV {

  public class DevAdmin : DefaultAdmin {
    public override bool Enabled => Terminal.m_cheat || ZNet.m_isServer;
    protected override void OnSuccess(Terminal terminal) {
      base.OnSuccess(terminal);
      if (!Enabled) {
        // Devcommands during admin check means that the check passed.
        Checking = true;
        terminal.TryRunCommand("devcommands");
      }
    }
    protected override void OnFail(Terminal terminal) {
      base.OnSuccess(terminal);
      // Ensure devcommands are disabled on failure.
      if (Enabled) terminal.TryRunCommand("devcommands");
      terminal.AddString("Unauthorized to use devcommands.");
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
        if (Enum.TryParse<KeyCode>(key, true, out var keyCode)) {
          if (!Input.GetKey(keyCode)) return false;
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
      // Let other commands pass normally.
      if (array[0] != "devcommands") {
        return true;
      }
      // Devcommands during admin check means that the check passed.
      if (Admin.Checking) {
        Admin.Checking = false;
        return true;
      }
      // Disabling doesn't require admin check.
      if (Admin.Enabled) return true;
      // Otherwise go through the admin check.
      Admin.Check(__instance);
      return false;
    }
  }

  [HarmonyPatch(typeof(Console), "IsConsoleEnabled")]
  public class IsConsoleEnabled {
    public static void Postfix(ref bool __result) {
      __result = true;
    }
  }
  // Must be patched because contains a "is server check".
  [HarmonyPatch(typeof(Terminal), "IsCheatsEnabled")]
  public class IsCheatsEnabled {
    public static void Postfix(ref bool __result) {
      __result = __result || Admin.Enabled;
    }
  }
  // Must be patched because contains a "is server check".
  [HarmonyPatch(typeof(Terminal.ConsoleCommand), "IsValid")]
  public class IsValid {
    public static void Postfix(ref bool __result) {
      __result = __result || Admin.Enabled;
    }
  }
  // Enable autocomplete for secrets (as it still only shows cheats with devcommands).
  [HarmonyPatch(typeof(Terminal), "Awake")]
  public class AutoCompleteSecrets {
    public static void Postfix(ref bool ___m_autoCompleteSecrets) {
      ___m_autoCompleteSecrets = true;
    }
  }
}
