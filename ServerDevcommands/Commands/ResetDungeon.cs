using System.Linq;
using UnityEngine;

namespace ServerDevcommands;
public class ResetDungeonCommand
{
  private static readonly int ProxyHash = "LocationProxy".GetStableHashCode();
  private static readonly int PlayerHash = "Player".GetStableHashCode();
  public ResetDungeonCommand()
  {
    Helper.Command("resetdungeon", "- Resets the nearest camp or dungeon within 20 meters.", (args) =>
    {
      var dgs = Object.FindObjectsOfType<DungeonGenerator>();
      var player = Helper.GetPlayer();
      var dg = dgs.OrderBy(dg => Utils.DistanceXZ(dg.transform.position, player.transform.position)).FirstOrDefault();
      if (!dg) throw new System.Exception("No dungeon found.");
      if (Utils.DistanceXZ(dg.transform.position, player.transform.position) > 20f) throw new System.Exception("The dungeon is too far.");
      var zone = ZoneSystem.GetZone(dg.transform.position);
      var index = ZDOMan.instance.SectorToIndex(zone);
      if (index < 0 || index >= ZDOMan.instance.m_objectsBySector.Length) throw new System.Exception("No objects found.");
      var objs = ZDOMan.instance.m_objectsBySector[index];
      var proxy = objs.FirstOrDefault(x => x.GetPrefab() == ProxyHash);
      if (proxy != null)
      {
        var locHash = proxy.GetInt(ZDOVars.s_location);
        if (ZoneSystem.instance.m_locationsByHash.TryGetValue(locHash, out var zoneLoc))
        {
          zoneLoc.m_prefab.Load();
          var loc = zoneLoc.m_prefab.Asset.GetComponent<Location>();
          var hasOffset = loc && loc.m_useCustomInteriorTransform && loc.m_interiorTransform && loc.m_generator;
          if (hasOffset)
            dg.m_originalPosition = loc.m_generator.transform.localPosition;
          zoneLoc.m_prefab.Release();
        }
      }
      foreach (var zdo in objs)
      {
        if (dg.m_algorithm == DungeonGenerator.Algorithm.Dungeon && zdo.GetPosition().y < 3000) continue;
        if (dg.m_algorithm == DungeonGenerator.Algorithm.CampGrid && Utils.DistanceXZ(zdo.GetPosition(), dg.transform.position) > dg.m_campRadiusMax) continue;
        if (zdo.GetPrefab() == PlayerHash || zdo.GetPrefab() == ProxyHash) continue;
        if (zdo == dg.m_nview.GetZDO()) continue;
        ZDOMan.instance.DestroyZDO(zdo);
      }
      dg.m_nview.ClaimOwnership();
      dg.Generate(ZoneSystem.SpawnMode.Full);
      Helper.AddMessage(args.Context, $"Reset dungeon {dg.name} at {Helper.PrintVectorXZY(dg.transform.position)} with m_originalPosition {dg.m_originalPosition.y}.");
    });
    AutoComplete.RegisterEmpty("resetdungeon");
  }
}
