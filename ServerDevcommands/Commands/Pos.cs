using UnityEngine;
namespace ServerDevcommands;
///<summary>Adds support for other player's position. </summary>
public class PosCommand {
  public PosCommand() {
    new Terminal.ConsoleCommand("pos", "[name/precision] [precision] - Prints the position of a player. If name is not given, prints the current position.", (args) => {
      var position = Player.m_localPlayer?.transform.position;
      var precision = 0;
      if (args.Length >= 2) {
        precision = args.TryParameterInt(1, 0);
        if (precision == 0) {
          var info = Helper.FindPlayer(args[1]);
          position = info.m_characterID.IsNone() ? null : (Vector3?)info.m_position;
        }
      }
      if (args.Length >= 3)
        precision = args.TryParameterInt(2, 0);

      if (position.HasValue)
        Helper.AddMessage(args.Context, $"Player position (X,Z,Y): ({position.Value.x.ToString($"F{precision}")}, {position.Value.z.ToString($"F{precision}")}, {position.Value.y.ToString($"F{precision}")})");
      else
        Helper.AddMessage(args.Context, "Error: Unable to find the player.");
    }, true, true, optionsFetcher: () => ParameterInfo.PlayerNames);
    AutoComplete.Register("pos", (int index) => {
      if (index == 0) return ParameterInfo.PlayerNames;
      if (index == 1) return ParameterInfo.Create("Precision", "a positive integer");
      return ParameterInfo.None;
    });
  }
}
