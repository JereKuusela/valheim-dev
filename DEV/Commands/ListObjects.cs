using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DEV {
  public partial class Commands {
    private static bool IsIncluded(string id, string name) {
      if (id.StartsWith("*") && id.EndsWith("*")) {
        return name.Contains(id.Substring(1, id.Length - 3));
      }
      if (id.StartsWith("*")) return name.EndsWith(id.Substring(1));
      if (id.EndsWith("*")) return name.StartsWith(id.Substring(0, id.Length - 2));
      return id == name;
    }
    public static IEnumerable<int> GetPrefabs(string id) {
      IEnumerable<GameObject> values = ZNetScene.instance.m_namedPrefabs.Values;
      if (id == "*") { } // Pass
      else if (id.Contains("*"))
        values = values.Where(prefab => IsIncluded(id, prefab.name));
      else
        values = values.Where(prefab => prefab.name == id);
      return values.Where(prefab => prefab.name != "Player").Select(prefab => prefab.name.GetStableHashCode());
    }
    public static IEnumerable<ZDO> GetZDOs(string id, float distance) {
      var codes = GetPrefabs(id);
      var sector = Player.m_localPlayer ? Player.m_localPlayer.m_nview.GetZDO().GetSector() : new Vector2i(0, 0);
      var index = ZDOMan.instance.SectorToIndex(sector);
      IEnumerable<ZDO> zdos = ZDOMan.instance.m_objectsBySector[index];
      zdos = zdos.Where(zdo => codes.Contains(zdo.GetPrefab()));
      var position = Player.m_localPlayer ? Player.m_localPlayer.transform.position : Vector3.zero;
      if (distance > 0)
        return zdos.Where(zdo => Utils.DistanceXZ(zdo.GetPosition(), position) <= distance);
      return zdos;
    }
  }
}
