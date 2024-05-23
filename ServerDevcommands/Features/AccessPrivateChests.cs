using HarmonyLib;
namespace ServerDevcommands;
[HarmonyPatch(typeof(Container), nameof(Container.CheckAccess))]
public class AccessPrivateChests
{
  static bool Postfix(bool result) => result || Settings.AccessPrivateChests;
}



