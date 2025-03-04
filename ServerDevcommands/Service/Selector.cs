
using System;
using System.Collections.Generic;
using System.Linq;
using ServerDevcommands;
using UnityEngine;

namespace Service;

public class Hovered(ZNetView obj, int index)
{
  public ZNetView Obj = obj;
  public int Index = index;
}
public static class Selector
{
  ///<summary>Helper to check object validity.</summary>
  public static bool IsValid(ZNetView view) => view && IsValid(view.GetZDO());
  ///<summary>Helper to check object validity.</summary>
  public static bool IsValid(ZDO zdo) => zdo != null && zdo.IsValid();

  ///<summary>Returns the hovered object.</summary>
  public static ZNetView? GetHovered(float range, string[] included, string[] excluded) => GetHovered(range, included, [], excluded);
  public static ZNetView? GetHovered(float range, string[] included, HashSet<string> types, string[] excluded)
  {
    var hovered = GetHovered(Player.m_localPlayer, range, included, types, excluded);
    if (hovered == null) return null;
    return hovered.Obj;
  }


  public static int GetPrefabFromHit(RaycastHit hit) => hit.collider.GetComponentInParent<ZNetView>().GetZDO().GetPrefab();

  public static Hovered? GetHovered(Player obj, float maxDistance, string[] included, string[] excluded, bool allowOtherPlayers = false) => GetHovered(obj, maxDistance, included, [], excluded, allowOtherPlayers);
  public static Hovered? GetHovered(Player obj, float maxDistance, string[] included, HashSet<string> types, string[] excluded, bool allowOtherPlayers = false)
  {
    allowOtherPlayers |= included.Contains("Player");
    var includedPrefabs = GetAllPrefabs(included);
    if (included.Length > 0 && includedPrefabs.Count == 0) throw new InvalidOperationException("No valid prefabs found.");
    var excludedPrefabs = GetExcludedPrefabs(excluded);
    var raycast = Math.Max(maxDistance + 5f, 50f);
    var mask = LayerMask.GetMask(
    [
      "item",
      "piece",
      "piece_nonsolid",
      "Default",
      "static_solid",
      "Default_small",
      "character",
      "character_net",
      "terrain",
      "vehicle",
      "character_trigger" // Added to remove spawners with ESP mod.
    ]);
    var hits = Physics.RaycastAll(GameCamera.instance.transform.position, GameCamera.instance.transform.forward, raycast, mask);
    Array.Sort(hits, (RaycastHit x, RaycastHit y) => x.distance.CompareTo(y.distance));
    foreach (var hit in hits)
    {
      if (Vector3.Distance(hit.point, obj.m_eye.position) >= maxDistance) continue;
      var netView = hit.collider.GetComponentInParent<ZNetView>();
      if (!IsValid(netView)) continue;
      if (includedPrefabs.Count > 0 && !includedPrefabs.Contains(netView.GetZDO().GetPrefab())) continue;
      if (excludedPrefabs.Contains(netView.GetZDO().GetPrefab())) continue;
      if (hit.collider.GetComponent<EffectArea>()) continue;
      var player = netView.GetComponentInChildren<Player>();
      if (player == obj) continue;
      if (!allowOtherPlayers && player) continue;
      if (types.Count > 0 && !ComponentInfo.HasComponent(netView, types)) continue;
      var index = -1;
      if (netView.TryGetComponent(out MineRock5 mineRock5))
        index = mineRock5.GetAreaIndex(hit.collider);
      if (netView.TryGetComponent(out MineRock mineRock))
        index = mineRock.GetAreaIndex(hit.collider);
      var room = hit.collider.GetComponentInParent<Room>();
      if (room)
      {
        var tr = netView.transform;
        for (int i = 0; i < tr.childCount; i++)
        {
          if (tr.GetChild(i).gameObject == room.gameObject)
          {
            index = i;
            break;
          }
        }
      }
      return new(netView, index);
    }
    return null;
  }
  private static float GetX(float x, float y, float angle) => Mathf.Cos(angle) * x - Mathf.Sin(angle) * y;
  private static float GetY(float x, float y, float angle) => Mathf.Sin(angle) * x + Mathf.Cos(angle) * y;
  public static bool Within(Vector3 position, Vector3 center, float angle, Range<float> width, Range<float> depth, float height)
  {
    var dx = position.x - center.x;
    var dz = position.z - center.z;
    var distanceX = GetX(dx, dz, angle);
    var distanceZ = GetY(dx, dz, angle);
    if (!WithinHeight(position, center, height)) return false;
    return Helper.Within(width, depth, Mathf.Abs(distanceX), Mathf.Abs(distanceZ));
  }
  public static bool Within(Vector3 position, Vector3 center, Range<float> radius, float height)
  {
    if (!WithinHeight(position, center, height)) return false;
    return Helper.Within(radius, Utils.DistanceXZ(position, center));
  }
  private static bool WithinHeight(Vector3 position, Vector3 center, float height)
  {
    var cy = center.y;
    var y = position.y;
    // Default range is 1000 to avoid selecting objects from dungeons.
    if (Helper.IsZero(height)) return Mathf.Abs(y - cy) <= 1000f;
    // But if the user selects a bigger range then than should be used.
    var min = Mathf.Min(cy, cy + height);
    var max = Mathf.Max(cy, cy + height);
    return y >= min && y <= max;
  }
  private static bool IsIncluded(string id, string name)
  {
    if (id.StartsWith("*", StringComparison.Ordinal) && id.EndsWith("*", StringComparison.Ordinal))
      return name.Contains(id.Substring(1, id.Length - 3));
    if (id.StartsWith("*", StringComparison.Ordinal)) return name.EndsWith(id.Substring(1), StringComparison.Ordinal);
    if (id.EndsWith("*", StringComparison.Ordinal)) return name.StartsWith(id.Substring(0, id.Length - 2), StringComparison.Ordinal);
    return id == name;
  }
  public static HashSet<int> GetExcludedPrefabs(string[] ids)
  {
    HashSet<int> prefabs = [];
    foreach (var id in ids)
      prefabs.UnionWith(GetExcludedPrefabs(id));
    return prefabs;
  }
  private static HashSet<int> GetExcludedPrefabs(string id)
  {
    if (id == "") return [];
    id = id.ToLower();
    return ZNetScene.instance.m_namedPrefabs.Values
      .Where(prefab => IsIncluded(id, prefab.name.ToLower()))
      .Select(prefab => prefab.name.GetStableHashCode())
      .ToHashSet();
  }
  public static HashSet<int> GetPrefabs(string[] ids)
  {
    if (ids.Length == 0) return GetSafePrefabs("");
    HashSet<int> prefabs = [];
    foreach (var id in ids)
      prefabs.UnionWith(GetSafePrefabs(id));
    return prefabs;
  }
  public static HashSet<int> GetAllPrefabs(string[] ids)
  {
    if (ids.Length == 0) return [];
    HashSet<int> prefabs = [];
    foreach (var id in ids)
      prefabs.UnionWith(GetAllPrefabs(id));
    return prefabs;
  }
  // Reason for this is to make area selections less error prone (for example accidentaly removing players or _ZoneCtrl).
  private static HashSet<int> GetSafePrefabs(string id)
  {
    id = id.ToLower();
    IEnumerable<GameObject> values = ZNetScene.instance.m_namedPrefabs.Values;
    if (id != "player")
      values = values.Where(prefab => prefab.name != "Player");
    if (id == "*" || id == "")
      values = values.Where(prefab => !prefab.name.StartsWith("_", StringComparison.Ordinal));
    else if (id.Contains("*"))
      values = values.Where(prefab => IsIncluded(id, prefab.name.ToLower()));
    else
      values = values.Where(prefab => prefab.name.ToLower() == id);
    return values.Select(prefab => prefab.name.GetStableHashCode()).ToHashSet();
  }
  private static HashSet<int> GetAllPrefabs(string id)
  {
    id = id.ToLower();
    IEnumerable<GameObject> values = ZNetScene.instance.m_namedPrefabs.Values;
    if (id == "*" || id == "")
    {
      // Empty on purpose.
    }
    else if (id.Contains("*"))
      values = values.Where(prefab => IsIncluded(id, prefab.name.ToLower()));
    else
      values = values.Where(prefab => prefab.name.ToLower() == id);
    return values.Select(prefab => prefab.name.GetStableHashCode()).ToHashSet();
  }
  public static ZNetView[] GetNearby(string[] included, HashSet<string> types, string[] excluded, Vector3 center, Range<float> radius, float height)
  {
    var includedPrefabs = GetPrefabs(included);
    if (included.Length > 0 && includedPrefabs.Count == 0) throw new InvalidOperationException("No valid prefabs found.");
    var excludedPrefabs = GetExcludedPrefabs(excluded);
    bool checker(Vector3 pos) => Within(pos, center, radius, height);
    return GetNearby(includedPrefabs, types, excludedPrefabs, checker);
  }
  public static ZNetView[] GetNearby(string[] included, HashSet<string> types, string[] excluded, Vector3 center, float angle, Range<float> width, Range<float> depth, float height)
  {
    var includedPrefabs = GetPrefabs(included);
    if (included.Length > 0 && includedPrefabs.Count == 0) throw new InvalidOperationException("No valid prefabs found.");
    var excludedPrefabs = GetExcludedPrefabs(excluded);
    bool checker(Vector3 pos) => Within(pos, center, angle, width, depth, height);
    return GetNearby(includedPrefabs, types, excludedPrefabs, checker);
  }
  public static ZNetView[] GetNearby(HashSet<int> included, HashSet<string> types, HashSet<int> excluded, Func<Vector3, bool> checker)
  {
    var scene = ZNetScene.instance;
    var objects = ZNetScene.instance.m_instances.Values;
    var views = objects.Where(IsValid);
    if (included.Count > 0)
      views = views.Where(view => included.Contains(view.GetZDO().GetPrefab()));
    if (excluded.Count > 0)
      views = views.Where(view => !excluded.Contains(view.GetZDO().GetPrefab()));
    views = views.Where(view => checker(view.GetZDO().GetPosition()));
    if (types.Count > 0)
      views = ComponentInfo.HaveComponent(views, types);
    var objs = views.ToArray();
    if (objs.Length == 0) throw new InvalidOperationException("Nothing is nearby.");
    return objs;
  }
  private static KeyValuePair<int, int> RaftParent = ZDO.GetHashZDOID("MBParent");

  public static ZNetView[] GetConnectedRaft(ZNetView baseView, HashSet<int> included, HashSet<int> excluded)
  {
    var id = baseView.GetZDO().GetZDOID(RaftParent);
    var instances = ZNetScene.instance.m_instances.Values;
    var linq = instances.Where(IsValid);
    if (included.Count > 0)
      linq = linq.Where(view => included.Contains(view.GetZDO().GetPrefab()));
    if (excluded.Count > 0)
      linq = linq.Where(view => !excluded.Contains(view.GetZDO().GetPrefab()));
    linq = linq.Where(view => view.GetZDO().m_uid == id || view.GetZDO().GetZDOID(RaftParent) == id);
    return linq.ToArray();
  }
  ///<summary>Returns connected WearNTear objects.</summary>
  public static ZNetView[] GetConnected(ZNetView baseView, string[] included, string[] excluded)
  {
    var includedPrefabs = GetPrefabs(included);
    var excludedPrefabs = GetExcludedPrefabs(excluded);
    if (baseView.GetZDO().GetZDOID(RaftParent) != ZDOID.None) return GetConnectedRaft(baseView, includedPrefabs, excludedPrefabs);
    var baseWear = baseView.GetComponent<WearNTear>() ?? throw new InvalidOperationException("Connected doesn't work for this object.");
    HashSet<ZNetView> views = [baseView];
    Queue<WearNTear> todo = new();
    todo.Enqueue(baseWear);
    while (todo.Count > 0)
    {
      var wear = todo.Dequeue();
      if (wear.m_colliders == null) wear.SetupColliders();
      foreach (var boundData in wear.m_bounds)
      {
        var boxes = Physics.OverlapBoxNonAlloc(boundData.m_pos, boundData.m_size, WearNTear.s_tempColliders, boundData.m_rot, WearNTear.s_rayMask);
        for (int i = 0; i < boxes; i++)
        {
          var collider = WearNTear.s_tempColliders[i];
          if (collider.isTrigger || collider.attachedRigidbody != null || wear.m_colliders.Contains(collider)) continue;
          var wear2 = collider.GetComponentInParent<WearNTear>();
          if (!wear2 || !IsValid(wear2.m_nview)) continue;
          if (views.Contains(wear2.m_nview)) continue;
          if (excludedPrefabs.Contains(wear2.m_nview.GetZDO().GetPrefab())) continue;
          views.Add(wear2.m_nview);
          todo.Enqueue(wear2);
        }
      }
    }
    IEnumerable<ZNetView> linq = views;
    if (includedPrefabs.Count > 0)
      linq = linq.Where(view => includedPrefabs.Contains(view.GetZDO().GetPrefab()));
    return linq.ToArray();
  }
}