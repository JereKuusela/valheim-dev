using System;
using System.Linq;

namespace ServerDevcommands;
///<summary>Prints players and their ids.</summary>
public class PlayerListCommand {
  private string Format(ZNetPeer player) {
    return String.Format(Settings.Format(Settings.PlayerListFormat),
      player.m_playerName, player.m_socket.GetHostName(), player.m_characterID.UserID, player.m_refPos.x, player.m_refPos.y, player.m_refPos.z
    );
  }
  private string Format() {
    var pos = ZNet.instance.m_referencePosition;
    return String.Format(Settings.Format(Settings.PlayerListFormat),
      Game.instance.GetPlayerProfile().GetName(), PrivilegeManager.GetNetworkUserId(), ZNet.instance.m_characterID, pos.x, pos.y, pos.z
    );
  }
  private void Execute(Terminal context) {
    var players = ZNet.instance.GetPeers().Select(Format).ToList();
    if (ZNet.instance && !ZNet.instance.IsDedicated())
      players.Add(Format());
    context.AddString(string.Join("\n", players));
  }
  public PlayerListCommand() {
    Helper.Command("playerlist", "- Prints online players.", (args) => {
      if (ZNet.instance.IsServer()) Execute(args.Context);
      else ServerExecution.Send(args.Args);
    });
    AutoComplete.RegisterEmpty("playerlist");
  }
}
