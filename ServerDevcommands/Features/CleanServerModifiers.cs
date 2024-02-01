using HarmonyLib;

namespace ServerDevcommands;

[HarmonyPatch(typeof(ZoneSystem))]
public class CleanServerModifiers
{
  [HarmonyPatch(nameof(ZoneSystem.GlobalKeyAdd)), HarmonyPostfix]
  static void GlobalKeyAdd()
  {
    Clean();
  }
  [HarmonyPatch(nameof(ZoneSystem.GlobalKeyRemove)), HarmonyPostfix]
  static void GlobalKeyRemove()
  {
    Clean();
  }
  private static void Clean()
  {
    if (ZNet.World?.m_startingGlobalKeys == null) return;
    var keys = ZNet.World.m_startingGlobalKeys;
    for (var i = keys.Count - 1; i >= 0; i -= 1)
    {
      var key1 = keys[i].Split(' ')[0].ToLowerInvariant();
      for (var j = i - 1; j >= 0; j -= 1)
      {
        var key2 = keys[j].Split(' ')[0].ToLowerInvariant();
        if (key1 == key2)
        {
          keys.RemoveAt(j);
          i -= 1;
        }
      }
    }
  }
}
