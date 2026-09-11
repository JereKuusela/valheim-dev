using HarmonyLib;
namespace ServerDevcommands;

[HarmonyPatch(typeof(PlayerProfile), nameof(PlayerProfile.s_bypassCheatChecks), MethodType.Getter)]
public class CheatTracking
{
  static bool Prefix(ref bool __result)
  {
    if (!Settings.DisableCheatTracking || !PermissionManager.Instance.CanCheat) return true;
    __result = true;
    return false;
  }
}
