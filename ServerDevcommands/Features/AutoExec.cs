using HarmonyLib;

namespace ServerDevcommands {
  [HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.Awake))]
  public class FejdStartupAwake {
    public static void Postfix() {
      if (Settings.AutoExecBoot != "") Console.instance.TryRunCommand(Settings.AutoExecBoot);
    }
  }
  [HarmonyPatch(typeof(Game), nameof(Game.Awake))]
  public class AutoExec {
    public static void Postfix() {
      if (Settings.AutoExec != "") Console.instance.TryRunCommand(Settings.AutoExec);
    }
  }
}