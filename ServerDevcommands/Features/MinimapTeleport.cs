using HarmonyLib;
using UnityEngine;
using System;
using System.Linq;
namespace ServerDevcommands;

[HarmonyPatch(typeof(Minimap))]
public class MinimapTeleport
{

  private static bool TryTeleport(Minimap map, KeyCode mainKey)
  {
    if (!Console.instance || !Console.instance.IsCheatsEnabled()) return false;
    if (mainKey == Settings.MapTeleport.MainKey && Settings.MapTeleport.Modifiers.All(static k => ZInput.GetKey(k)))
    {
      var target = map.ScreenToWorldPoint(ZInput.mousePosition);
      target.y = Player.m_localPlayer.transform.position.y;
      Heightmap.GetHeight(target, out var height);
      target.y = Math.Max(0f, height);
      Player.m_localPlayer.TeleportTo(target, Player.m_localPlayer.transform.rotation, true);
      return true;
    }
    return false;
  }

  [HarmonyPatch(nameof(Minimap.OnMapLeftClick)), HarmonyPrefix]
  static bool OnMapLeftClick(Minimap __instance) => !TryTeleport(__instance, KeyCode.Mouse0);
  [HarmonyPatch(nameof(Minimap.OnMapRightClick)), HarmonyPrefix]
  static bool OnMapRightClick(Minimap __instance) => !TryTeleport(__instance, KeyCode.Mouse1);
  [HarmonyPatch(nameof(Minimap.OnMapMiddleClick)), HarmonyPrefix]
  static bool OnMapMiddleClick(Minimap __instance)
  {
    if (TryTeleport(__instance, KeyCode.Mouse2)) return false;
    var vector = __instance.ScreenToWorldPoint(ZInput.mousePosition);
    Chat.instance.SendPing(vector);
    return false;
  }
}
