using System.Linq;

namespace ServerDevcommands;
///<summary>Prints players and their ids.</summary>
public class PlayersCommand
{
  public PlayersCommand()
  {
    Helper.Command("players", "- Prints online players.", (args) =>
    {
      var players = ZNet.instance.GetPlayerList().Select(p => $"{p.m_name}: {p.m_host}");
      args.Context.AddString(string.Join("\n", players));
    });
    AutoComplete.RegisterEmpty("players");
  }
}
