using HarmonyLib;
namespace ServerDevcommands;
[HarmonyPatch(typeof(Container))]
public class AccessPrivateChests
{
  [HarmonyPatch(nameof(Container.CheckAccess)), HarmonyPostfix]
  static bool CheckAccess(bool result) => result || Settings.AccessPrivateChests;


  [HarmonyPatch(nameof(Container.RPC_OpenRespons)), HarmonyPrefix]
  static void RPC_OpenRespons(ref bool granted) => granted |= Settings.AccessPrivateChests;
}

