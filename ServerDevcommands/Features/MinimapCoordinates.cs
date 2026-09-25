using System.Linq;
using HarmonyLib;
using UnityEngine;
namespace ServerDevcommands;

[HarmonyPatch(typeof(Minimap), nameof(Minimap.Awake))]
public class Minimap_Alignment
{
  static void Postfix(Minimap __instance)
  {
    // Removes the need of padding.
    __instance.m_biomeNameSmall.alignment = TMPro.TextAlignmentOptions.TopRight;
    __instance.m_biomeNameLarge.alignment = TMPro.TextAlignmentOptions.TopRight;
  }
}

[HarmonyPatch(typeof(Minimap), nameof(Minimap.UpdateBiome))]
public class Minimap_ShowPos
{
  private static void AddText(TMPro.TMP_Text input, string text)
  {
    if (input.text.Contains(text)) return;
    input.text += text;
  }
  private static void CleanUp(TMPro.TMP_Text input, string text)
  {
    if (text == "" || !input.text.Contains(text)) return;
    input.text = input.text.Replace(text, "");
  }
  private static string Format(Vector3 position)
  {
    var name = Player.m_localPlayer ? Player.m_localPlayer.GetPlayerName() : "Unknown";
    var id = Player.m_localPlayer ? Player.m_localPlayer.GetPlayerID() : ZDOID.None.UserID;
    return string.Format(
      Settings.Format(Settings.MinimapFormat),
      "Not available", name, id, position.x, position.y, position.z
    );
  }
  private static string GetText(Vector3 position, float? distance = null)
  {
    var zone = ZoneSystem.GetZone(position);
    var positionText = Format(position);
    var zoneText = "zone: " + zone.x + "/" + zone.y;
    var distanceText = distance.HasValue ? $"\ndistance: {distance.Value:F0}" : "";
    var altBiomes = WorldGenerator.instance.GetBiomeSector(position.x, position.y).AltBiomes.Select(b => b.m_name).ToList();
    var altBiomeText = Settings.ShowAlternativeBiomes && altBiomes.Count > 0
      ? "\n" + string.Join(", ", altBiomes)
      : "";
    return $"\n{zoneText}\n{positionText}{distanceText}{altBiomeText}";
  }
  private static string PreviousSmallText = "";
  private static string PreviousLargeText = "";
  static void Postfix(Minimap __instance, Player player)
  {
    var mode = __instance.m_mode;
    if (PreviousSmallText != "")
    {
      CleanUp(__instance.m_biomeNameSmall, PreviousSmallText);
      PreviousSmallText = "";
    }
    if (PreviousLargeText != "")
    {
      CleanUp(__instance.m_biomeNameLarge, PreviousLargeText);
      PreviousLargeText = "";
    }
    var position = GameCamera.InFreeFly() && Utils.GetMainCamera() ? Utils.GetMainCamera().transform.position : player.transform.position;
    if (mode == Minimap.MapMode.Small && Settings.MiniMapCoordinates)
    {
      var text = GetText(position);
      AddText(__instance.m_biomeNameSmall, text);
      PreviousSmallText = text;
    }
    if (mode == Minimap.MapMode.Large && Settings.MapCoordinates)
    {
      position = __instance.ScreenToWorldPoint(ZInput.IsMouseActive() ? Input.mousePosition : new((Screen.width / 2f), (Screen.height / 2f)));
      position.y = WorldGenerator.instance.GetHeight(position.x, position.z);
      var text = GetText(position, Utils.DistanceXZ(player.transform.position, position));
      AddText(__instance.m_biomeNameLarge, text);
      PreviousLargeText = text;
    }
  }
}
