using System.Linq;
using HarmonyLib;

namespace ServerDevcommands;

[HarmonyPatch(typeof(Game), nameof(Game.EverybodyIsTryingToSleep))]
public class GhostIgnoreSleep
{
  static bool Postfix(bool result)
  {
    if (result) return result;
    var zdos = ZNet.instance.GetAllCharacterZDOS();
    if (zdos.Count == 0) return false;
    var someoneSleeping = zdos.Any(zdo => zdo.GetBool(ZDOVars.s_inBed));
    if (!someoneSleeping) return false;
    return zdos.All(zdo => zdo.GetBool(ZDOVars.s_inBed) || zdo.GetBool(Ghost.HashIgnoreSleep));
  }
}

[HarmonyPatch(typeof(Player), nameof(Player.SetGhostMode))]
public class Ghost
{

  public static readonly int HashIgnoreSleep = "DEV_GhostIgnoreSleep".GetHashCode();
  public static readonly int HashGhost = "DEV_Ghost".GetHashCode();

  public static bool IsGhost(Player player) => player.InGhostMode() || player.m_nview?.GetZDO()?.GetBool(HashGhost) == true;
  static void Postfix(Player __instance)
  {
    if (__instance != Player.m_localPlayer) return;

    __instance.m_nview?.GetZDO()?.Set(HashGhost, __instance.InGhostMode());
    if (Settings.GhostIgnoreSleep)
      __instance.m_nview?.GetZDO()?.Set(HashIgnoreSleep, __instance.InGhostMode());
  }
}