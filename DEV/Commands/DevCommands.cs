using HarmonyLib;

namespace DEV {
  ///<summary>Replaces the default devcommands with an admin check.</summary>
  public class DevCommandsCommand : BaseCommand {
    public static void Toggle(Terminal terminal) {
      var value = !Terminal.m_cheat;
      terminal?.AddString("Dev commands: " + value.ToString());
      Gogan.LogEvent("Cheat", "CheatsEnabled", value.ToString(), 0L);
      Set(value);
    }
    public static void Set(Terminal terminal, bool value) {
      terminal?.AddString("Dev commands: " + value.ToString());
      Gogan.LogEvent("Cheat", "CheatsEnabled", value.ToString(), 0L);
      Set(value);
    }
    public static void Set(bool value) {
      if (Terminal.m_cheat == value) return;
      Terminal.m_cheat = value;
      Console.instance?.updateCommandList();
      Chat.instance?.updateCommandList();
      if (Settings.AutoDebugMode)
        Player.m_debugMode = Terminal.m_cheat;
      if (Settings.AutoGodMode)
        Player.m_localPlayer?.SetGodMode(Terminal.m_cheat);
      if (Settings.AutoGhostMode)
        Player.m_localPlayer?.SetGhostMode(Terminal.m_cheat);
      if (Settings.AutoFly && Player.m_localPlayer) {
        Player.m_localPlayer.m_debugFly = Player.m_debugMode;
        Player.m_localPlayer.m_nview.GetZDO().Set("DebugFly", Player.m_debugMode);
      }
      if (Settings.AutoNoCost && Player.m_localPlayer)
        Player.m_localPlayer.m_noPlacementCost = Player.m_debugMode;
    }

    public DevCommandsCommand() {
      new Terminal.ConsoleCommand("devcommands", "Toggles cheats", delegate (Terminal.ConsoleEventArgs args) {
        if (Terminal.m_cheat) {
          Set(args.Context, false);
        } else if (ZNet.instance && ZNet.instance.IsServer()) {
          Toggle(args.Context);
        } else {
          args.Context.AddString("Authenticating for devcommands...");
          Admin.ManualCheck();
        }
      });
      AutoComplete.RegisterEmpty("devcommands");
    }
  }
  ///<summary>Custom admin check to update devcommands.</summary>
  public class DevAdmin : DefaultAdmin {
    protected override void OnSuccess(Terminal terminal) {
      base.OnSuccess(terminal);
      DevCommandsCommand.Toggle(terminal);
    }
    protected override void OnFail(Terminal terminal) {
      base.OnFail(terminal);
      DevCommandsCommand.Set(false);
      terminal.AddString("Unauthorized to use devcommands.");
    }
    public override void AutomaticCheck(Terminal terminal) {
      Enabled = false;
      DevCommandsCommand.Set(false);
      if (Settings.AutoDevcommands)
        Check(terminal);
    }
  }

  [HarmonyPatch(typeof(Terminal), "IsCheatsEnabled")]
  public class IsCheatsEnabledWithoutServerCheck {
    public static void Postfix(ref bool __result) {
      __result = Terminal.m_cheat;
    }
  }
  [HarmonyPatch(typeof(Terminal.ConsoleCommand), "IsValid")]
  public class IsValidWithoutServerCheck {
    public static void Postfix(ref bool __result) {
      __result = __result || Terminal.m_cheat;
    }
  }
  // Probably needed to provide autocomplete for the chat window.
  [HarmonyPatch(typeof(Terminal), "Awake")]
  public class AutoCompleteSecrets {
    public static void Postfix(ref bool ___m_autoCompleteSecrets) {
      ___m_autoCompleteSecrets = true;
    }
  }
  [HarmonyPatch(typeof(Player), "HaveStamina")]
  public class HaveStaminaWithGodMode {
    public static bool Prefix(Player __instance, ref bool __result) {
      var noUsage = Settings.GodModeNoStamina && __instance.InGodMode();
      __result = noUsage;
      return !noUsage;
    }
  }
  [HarmonyPatch(typeof(Player), "UseStamina")]
  public class UseStaminaWithGodMode {
    public static bool Prefix(Player __instance) {
      var noUsage = Settings.GodModeNoStamina && __instance.InGodMode();
      return !noUsage;
    }
  }
  [HarmonyPatch(typeof(Character), "AddStaggerDamage")]
  public class AddStaggerDamage {
    public static bool Prefix(Character __instance) {
      var noStaggering = Settings.GodModeNoStagger && __instance.InGodMode() && __instance.IsPlayer();
      return !noStaggering;
    }
  }
}
