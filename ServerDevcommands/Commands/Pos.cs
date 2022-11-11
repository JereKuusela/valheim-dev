namespace ServerDevcommands;
///<summary>Adds support for other player's position. </summary>
public class PosCommand
{
  public PosCommand()
  {
    Helper.Command("pos", "[name/precision] [precision] - Prints the position of a player. If name is not given, prints the current position.", (args) =>
    {
      var player = Helper.GetPlayer();
      var pos = player.transform.position;
      var precision = 0;
      if (args.Length >= 2)
      {
        precision = args.TryParameterInt(1, 0);
        if (precision == 0)
        {
          var info = Helper.FindPlayer(args[1]);
          pos = info.m_position;
        }
      }
      if (args.Length >= 3)
        precision = args.TryParameterInt(2, 0);
      Helper.AddMessage(args.Context, $"Player position (X,Z,Y): ({pos.x.ToString($"F{precision}")}, {pos.z.ToString($"F{precision}")}, {pos.y.ToString($"F{precision}")})");
    }, () => ParameterInfo.PlayerNames);
    AutoComplete.Register("pos", (int index) =>
    {
      if (index == 0) return ParameterInfo.PlayerNames;
      if (index == 1) return ParameterInfo.Create("Precision", "a positive integer");
      return ParameterInfo.None;
    });
  }
}
