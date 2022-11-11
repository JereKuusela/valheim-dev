using HarmonyLib;
namespace ServerDevcommands;
[HarmonyPatch(typeof(ZDOMan), nameof(ZDOMan.SendZDOs))]
public class SendZDOsWithGhostMode
{
  public static float? Previous = null;
  static void Prefix()
  {
    if (!Player.m_localPlayer || !Player.m_localPlayer.m_nview) return;
    if (Player.m_localPlayer.InGhostMode() && Settings.GhostInvisibility)
    {
      var zdo = Player.m_localPlayer.m_nview.GetZDO();
      if (zdo == null) return; // Null when quitting the game.
      Previous = Player.m_localPlayer.m_nview.GetZDO().m_position.y;
      Player.m_localPlayer.m_nview.GetZDO().m_position.y = 10000f;
      var seMan = Player.m_localPlayer.GetSEMan();
      if (seMan != null)
      {
        // Status effects add separate entities. Instead of also moving them, just easier to remove.
        foreach (var se in seMan.GetStatusEffects()) se.RemoveStartEffects();
      }
    }
  }
  static void Postfix()
  {
    if (!Player.m_localPlayer || !Player.m_localPlayer.m_nview) return;
    if (Previous.HasValue)
    {
      Player.m_localPlayer.m_nview.GetZDO().m_position.y = Previous.Value;
      Previous = null;
    }
  }
}

[HarmonyPatch(typeof(ZNet), nameof(ZNet.SendPeriodicData))]
public class SendPeriodicDataWithGhostMode
{
  public static bool? Previous = null;
  static void Prefix(ZNet __instance)
  {
    if (!Player.m_localPlayer || !Player.m_localPlayer.m_nview) return;
    if (Player.m_localPlayer.InGhostMode() && Settings.GhostInvisibility)
    {
      Previous = __instance.m_publicReferencePosition;
      __instance.m_publicReferencePosition = false;
    }
  }
  static void Postfix(ZNet __instance)
  {
    if (Previous.HasValue)
    {
      __instance.m_publicReferencePosition = Previous.Value;
      Previous = null;
    }
  }
}
