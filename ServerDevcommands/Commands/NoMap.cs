using UnityEngine;
namespace ServerDevcommands;
///<summary>Changes by default only to affect the server..</summary>
public class NoMapCommand {
  public NoMapCommand() {
    new Terminal.ConsoleCommand("nomap", "[set] [server = 1] - Toggles or sets the nomap mode for the server. If server is 0, the mode is only changed for the character.", (Terminal.ConsoleEventArgs args) => {
      var set = false;
      if (args.Length < 2)
        set = !ZoneSystem.instance.GetGlobalKey("nomap");
      else
        set = args[1] == "1";

      if (Player.m_localPlayer && args.Length > 2 && args[2] == "0") {
        string key = "mapenabled_" + Player.m_localPlayer.GetPlayerName();
        PlayerPrefs.SetFloat(key, set ? 1f : 0);
      } else {
        if (set)
          ZoneSystem.instance.SetGlobalKey("nomap");
        else
          ZoneSystem.instance.RemoveGlobalKey("nomap");
      }
      Helper.AddMessage(args.Context, "Map " + (set ? "disabled" : "enabled"));
    }, true, true);
  }
}
