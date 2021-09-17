using BepInEx;
using HarmonyLib;

namespace DEV {
  [BepInPlugin("valheim.jerekuusela.dev", "DEV", "1.2.0.0")]
  public class ESP : BaseUnityPlugin {
    public void Awake() {
      Harmony harmony = new Harmony("valheim.jerekuusela.dev");
      harmony.PatchAll();
    }
  }
  public class Cheats {
    public static bool CheckingAdmin = false;
    public static bool Enabled = false;
  }
  [HarmonyPatch(typeof(ZNet), "RPC_RemotePrint")]
  public class ZNet_RPC_RemotePrint {
    public static bool Prefix(string text) {
      if (Cheats.CheckingAdmin) {
        if (text == "Unbanning user admintest") {
          Console.instance.TryRunCommand("devcommands");
        } else {
          Console.instance.Print("Unauthorized to use devcommands.");
        }
        Cheats.CheckingAdmin = false;

        return false;
      }
      return true;
    }
  }

  // Replace devcommands check with a custom one.
  [HarmonyPatch(typeof(Terminal), "TryRunCommand")]
  public class TryRunCommand {
    public static bool Prefix(string text) {
      string[] array = text.Split(' ');
      // Let other commands pass normally.
      if (array[0] != "devcommands") {
        return true;
      }

      // Devcommands during admin check means that the check passed.
      if (Cheats.CheckingAdmin) {
        Cheats.Enabled = true;
        return true;
      }

      // Disabling doesn't require admin check.
      if (Cheats.Enabled) {
        Cheats.Enabled = false;
        return true;
      }
      // Automatically passs locally.
      if (ZNet.instance && ZNet.instance.IsServer()) {
        Cheats.Enabled = true;
        return true;
      }
      if (ZNet.instance) {
        Cheats.CheckingAdmin = true;
        ZNet.instance.Unban("admintest");
      }
      return false;
    }
  }

  // Cheats must be disabled when joining servers (so that locally enabling doesn't work).
  [HarmonyPatch(typeof(ZNet), "Start")]
  public class ZNet_Start {
    public static void Postfix() {
      Cheats.Enabled = false;
    }
  }

  [HarmonyPatch(typeof(Console), "IsConsoleEnabled")]
  public class IsConsoleEnabled {
    public static bool Prefix(ref bool __result) {
      __result = true;
      return false;
    }
  }
  // Must be patched because contains a "is server check".
  [HarmonyPatch(typeof(Terminal), "IsCheatsEnabled")]
  public class IsCheatsEnabled {
    public static bool Prefix(ref bool __result) {
      if (Cheats.Enabled) {
        __result = true;
        return false;
      }
      return true;
    }
  }
  // Must be patched because contains a "is server check".
  [HarmonyPatch(typeof(Terminal.ConsoleCommand), "IsValid")]
  public class IsValid {
    public static bool Prefix(ref bool __result) {
      if (Cheats.Enabled) {
        __result = true;
        return false;
      }
      return true;
    }
  }
}
