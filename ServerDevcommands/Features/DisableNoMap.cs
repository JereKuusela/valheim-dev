using HarmonyLib;
using UnityEngine;
namespace ServerDevcommands;
[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.GetGlobalKey))]
public class DisableNoMapKey
{
  static bool Prefix(string name, ref bool __result)
  {
    if (Settings.DisableNoMap && name == "nomap")
    {
      __result = false;
      return false;
    }
    return true;
  }
}

[HarmonyPatch(typeof(Minimap), nameof(Minimap.SetMapMode))]
public class DisableNoMap
{
  static void Prefix()
  {
    if (!Player.m_localPlayer || !Settings.DisableNoMap) return;
    if (PlayerPrefs.GetFloat("mapenabled_" + Player.m_localPlayer.GetPlayerName(), 1f) == 0f)
      PlayerPrefs.SetFloat("mapenabled_" + Player.m_localPlayer.GetPlayerName(), -1f);
  }
  static void Postfix()
  {
    if (!Player.m_localPlayer || !Settings.DisableNoMap) return;
    if (PlayerPrefs.GetFloat("mapenabled_" + Player.m_localPlayer.GetPlayerName(), 1f) == -1f)
      PlayerPrefs.SetFloat("mapenabled_" + Player.m_localPlayer.GetPlayerName(), 0f);
  }
}