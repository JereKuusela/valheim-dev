using UnityEngine;

namespace DEV {

  ///<summary>Adds support for other player's position. </summary>
  public class PosCommand : BaseCommand {
    public PosCommand() {
      new Terminal.ConsoleCommand("pos", "[name] - Prints the position of a player. If name is not given, prints the current position.", delegate (Terminal.ConsoleEventArgs args) {
        var position = Player.m_localPlayer?.transform.position;
        if (args.Length >= 2) {
          var info = FindPlayer(args[1]);
          position = info.m_characterID.IsNone() ? null : (Vector3?)info.m_position;
        }
        if (position.HasValue)
          AddMessage(args.Context, "Player position (X,Y,Z):" + position.Value.ToString("F0"));
        else
          AddMessage(args.Context, "Error: Unable to find the player.");
      }, true, true, optionsFetcher: () => ParameterInfo.PlayerNames);
      AutoComplete.Register("pos", (int index, string parameter) => {
        if (parameter != "") return ParameterInfo.InvalidNamed;
        if (index == 0) return ParameterInfo.PlayerNames;
        return ParameterInfo.None;
      });
    }
  }
}
