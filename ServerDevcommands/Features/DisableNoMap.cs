using HarmonyLib;
namespace ServerDevcommands;

[HarmonyPatch(typeof(Game), nameof(Game.UpdateNoMap))]
public class DisableNoMap
{
  static void Postfix()
  {
    if (!Player.m_localPlayer || !Settings.DisableNoMap) return;
    Game.m_noMap = false;
    Minimap.instance.SetMapMode(Minimap.MapMode.Small);
  }
}