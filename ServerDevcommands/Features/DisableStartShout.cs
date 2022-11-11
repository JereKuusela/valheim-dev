using HarmonyLib;
namespace ServerDevcommands;
[HarmonyPatch(typeof(Game), nameof(Game.SpawnPlayer))]
public class DisableStartShout
{
  static void Postfix(Game __instance)
  {
    // First spawn flag is only used for the initial shout. So prematurely disabling that works.
    if (Settings.DisableStartShout) __instance.m_firstSpawn = false;
  }
}
