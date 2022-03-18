using HarmonyLib;

namespace ServerDevcommands {
  [HarmonyPatch(typeof(Player), nameof(Player.Update))]
  public class DisableDebugModeKeys {
    public static void Prefix(ref bool __state) {
      __state = Player.m_debugMode;
      if (Settings.DisableDebugModeKeys) Player.m_debugMode = false;
    }
    public static void Postfix(bool __state) {
      Player.m_debugMode = __state;
    }
  }
}
