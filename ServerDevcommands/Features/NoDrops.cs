using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace ServerDevcommands {
  [HarmonyPatch(typeof(CharacterDrop), nameof(CharacterDrop.GenerateDropList))]
  public class GenerateDropList {
    public static bool Prefix(ref List<KeyValuePair<GameObject, int>> __result) {
      if (Settings.NoDrops) {
        __result = new List<KeyValuePair<GameObject, int>>();
        return false;
      }
      return true;
    }
  }
}
