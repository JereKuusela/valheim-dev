using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
namespace ServerDevcommands;
[HarmonyPatch(typeof(CharacterDrop), nameof(CharacterDrop.GenerateDropList))]
public class GenerateDropList
{
  static bool Prefix(ref List<KeyValuePair<GameObject, int>> __result)
  {
    if (Settings.NoDrops)
    {
      __result = new();
      return false;
    }
    return true;
  }
}
