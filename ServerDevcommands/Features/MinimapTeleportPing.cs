using HarmonyLib;
using UnityEngine;
namespace ServerDevcommands;

[HarmonyPatch(typeof(Minimap), nameof(Minimap.OnMapMiddleClick))]
public class Minimap_TeleportPing
{
  static void Prefix(ref bool __state)
  {
    Minimap_PreventPing.PreventPing = Player.m_debugMode && Console.instance && Console.instance.IsCheatsEnabled() && Input.GetKey(KeyCode.LeftControl);
  }
  static void Postfix()
  {
    Minimap_PreventPing.PreventPing = false;
  }
}

[HarmonyPatch(typeof(Chat), nameof(Chat.SendPing))]
public class Minimap_PreventPing
{
  public static bool PreventPing = false;
  static bool Prefix() => !PreventPing;
}