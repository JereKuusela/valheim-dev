using HarmonyLib;
using UnityEngine;

namespace ServerDevcommands {

  [HarmonyPatch(typeof(Minimap), nameof(Minimap.UpdateBiome))]

  public class Minimap_ShowPos {
    // Text doesn't always get updated so extra stuff must be reseted manually.
    private static string previousLargeText = "";
    public static void Prefix(Minimap __instance) {
      __instance.m_biomeNameLarge.text = previousLargeText;

    }
    public static void Postfix(Minimap __instance, Player player) {
      previousLargeText = __instance.m_biomeNameLarge.text;
      if (!Settings.MapCoordinates) return;
      var mode = __instance.m_mode;
      var position = player.transform.position;
      if (mode == Minimap.MapMode.Large)
        position = __instance.ScreenToWorldPoint(ZInput.IsMouseActive() ? Input.mousePosition : new Vector3((float)(Screen.width / 2), (float)(Screen.height / 2)));
      var zone = ZoneSystem.instance.GetZone(position);
      var positionText = "x: " + position.x.ToString("F0") + " y: " + position.y.ToString("F0") + " z: " + position.z.ToString("F0");
      var zoneText = "zone: " + zone.x + "/" + zone.y;
      var largeText = "\n\n" + previousLargeText + "\n" + zoneText + "\n" + positionText;
      __instance.m_biomeNameLarge.text = largeText;
    }
  }
}