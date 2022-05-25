using HarmonyLib;
namespace ServerDevcommands;
[HarmonyPatch(typeof(PrivateArea), nameof(PrivateArea.HaveLocalAccess))]
public class AccessWardedAreas {
  static void Postfix(ref bool __result) {
    __result |= Settings.AccessWardedAreas;
  }
}

