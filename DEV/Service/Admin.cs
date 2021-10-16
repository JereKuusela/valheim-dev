using HarmonyLib;

namespace Service {

  ///<summary>Static accessors for easier usage.</summary>
  public static class Admin {
    public static IAdmin Instance = new DefaultAdmin();
    public static bool Enabled => Patch.Cheat(Console.instance);
    public static bool Checking {
      get => Instance.Checking;
      set => Instance.Checking = value;
    }
    public static void Check(Terminal terminal) => Instance.Check(terminal);
    public static void Verify(string text) => Instance.Verify(text);
  }

  public interface IAdmin {
    bool Checking { get; set; }
    void Check(Terminal terminal);
    void Verify(string text);
  }

  ///<summary>Default implementation. Can be extended and overwritten.</summary>
  public class DefaultAdmin : IAdmin {
    private Terminal Terminal;
    public virtual void Check(Terminal terminal) {
      if (!ZNet.instance) return;
      Terminal = terminal;
      Checking = true;
      // Automatically pass locally.
      if (ZNet.instance.IsServer())
        Terminal.TryRunCommand("devcommands");
      else
        ZNet.instance.Unban("admintest");

    }
    public virtual void Verify(string text) {
      if (text == "Unbanning user admintest")
        Terminal.TryRunCommand("devcommands");
      else {
        Checking = false;
        Terminal.AddString("Unauthorized to use devcommands.");
      }
    }
    public virtual bool Checking { get; set; }
  }

  [HarmonyPatch(typeof(ZNet), "RPC_RemotePrint")]
  public class ZNet_RPC_RemotePrint {
    public static bool Prefix(string text) {
      if (!Admin.Checking) return true;
      Admin.Verify(text);
      return false;
    }
  }

  // Replace devcommands check with a custom one.
  [HarmonyPatch(typeof(Terminal), "TryRunCommand")]
  public class TryRunCommand {
    public static bool Prefix(Terminal __instance, string text) {
      string[] array = text.Split(' ');
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

  // Cheats must be disabled when joining servers (so that locally enabling doesn't work).
  [HarmonyPatch(typeof(ZNet), "Start")]
  public class ZNet_Start {
    public static void Postfix() {
      if (Admin.Enabled) Console.instance.TryRunCommand("devcommands");
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
