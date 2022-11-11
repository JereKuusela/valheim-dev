using HarmonyLib;
using UnityEngine;

namespace ServerDevcommands;
[HarmonyPatch(typeof(Chat), nameof(Chat.SendText))]
public class HideShoutPing
{
  static void Prefix(ref Vector3 __state)
  {
    if (!Player.m_localPlayer || !Settings.HideShoutPings) return;
    __state = Player.m_localPlayer.m_head.position;
    Player.m_localPlayer.m_head.position = new();
  }
  static void Postfix(Player __instance, Vector3 __state)
  {
    if (!Player.m_localPlayer || !Settings.HideShoutPings) return;
    Player.m_localPlayer.m_head.position = __state;
  }
}
