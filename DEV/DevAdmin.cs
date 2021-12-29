using HarmonyLib;
using Service;

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
    public static bool Prefix(Terminal __instance, string text) {
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
