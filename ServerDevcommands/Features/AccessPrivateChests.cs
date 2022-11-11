using HarmonyLib;
namespace ServerDevcommands;
[HarmonyPatch(typeof(Container), nameof(Container.CheckAccess))]
public class AccessPrivateChests
{
  static void Postfix(ref bool __result)
  {
    __result |= Settings.AccessPrivateChests;
  }
}


