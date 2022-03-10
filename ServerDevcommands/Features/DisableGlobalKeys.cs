using HarmonyLib;

namespace ServerDevcommands {
  [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.RPC_SetGlobalKey))]
  public class DisableGlobalKeys {
    public static bool Prefix(string name) => !Settings.IsGlobalKeyDisabled(name);
  }
}
