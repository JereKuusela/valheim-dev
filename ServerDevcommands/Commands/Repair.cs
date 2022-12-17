using System.Collections.Generic;

namespace ServerDevcommands;
///<summary>Repairs nearby structures.</summary>
public class RepairCommand
{
  public RepairCommand()
  {
    AutoComplete.RegisterEmpty("repair");
    Helper.Command("repair", "- Repair structures within 20 meters.", (args) =>
    {
      var player = Helper.GetPlayer();
      var pos = player.transform.position;
      var repaired = 0;
      List<Piece> pieces = new List<Piece>();
      Piece.GetAllPiecesInRadius(pos, 20f, pieces);
      foreach (var piece in pieces)
      {
        if (piece.GetComponent<WearNTear>()?.Repair() == true)
          repaired++;
      }
      Helper.AddMessage(args.Context, $"Repaired {repaired} structures.");
    });
  }
}

