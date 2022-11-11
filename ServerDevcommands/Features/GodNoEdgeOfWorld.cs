using HarmonyLib;
namespace ServerDevcommands;
[HarmonyPatch(typeof(Player), nameof(Player.EdgeOfWorldKill))]
public class GodNoEdgeOfWorld
{
  static bool Prefix(Player __instance) => !Settings.GodModeNoEdgeOfWorld || !__instance.InGodMode();
}
