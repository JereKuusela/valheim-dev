using System.Collections.Generic;
using UnityEngine;

namespace ServerDevcommands;
///<summary>Repairs nearby structures.</summary>
public class RepairCommand
{
  public RepairCommand()
  {
    AutoComplete.Register("repair", index =>
    {
      if (index == 0) return ParameterInfo.Create("Radius", "a positive integer");
      return ParameterInfo.None;
    });
    Helper.Command("repair", "[radius=20] - Repair structures within given  meters.", (args) =>
    {
      var player = Helper.GetPlayer();
      var pos = player.transform.position;
      var repaired = 0;
      var radius = args.TryParameterFloat(1, 20f);
      List<Piece> pieces = [];
      foreach (var piece in Piece.s_allPieces)
      {
        if (Vector3.Distance(pos, piece.transform.position) > radius) continue;
        if (piece.GetComponent<WearNTear>()?.Repair() == true)
          repaired++;
      }
      Helper.AddMessage(args.Context, $"Repaired {repaired} structures.");
    });
  }
}

