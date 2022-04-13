using UnityEngine;
namespace ServerDevcommands;
///<summary>Adds support for running as the server.</summary>
public class NoMapCommand {
  public NoMapCommand() {
    new Terminal.ConsoleCommand("nomap", "[set] - Toggles or sets the nomap mode. Can be executed on the server.", (Terminal.ConsoleEventArgs args) => {
      var set = false;
      if (args.Length < 2) {
        if (ZNet.instance && ZNet.instance.IsServer()) set = !ZoneSystem.instance.GetGlobalKey("nomap");
        else if (Player.m_localPlayer) {
          string key = "mapenabled_" + Player.m_localPlayer.GetPlayerName();
          set = PlayerPrefs.GetFloat(key, 1f) != 1f;
        }
      } else
        set = args[1] == "1";
      if (ZNet.instance && ZNet.instance.IsServer()) {
        if (set)
          ZoneSystem.instance.SetGlobalKey("nomap");
        else
          ZoneSystem.instance.RemoveGlobalKey("nomap");
      }
      if (Player.m_localPlayer) {
        string key = "mapenabled_" + Player.m_localPlayer.GetPlayerName();
        PlayerPrefs.SetFloat(key, set ? 1f : 0);
      }
      Helper.AddMessage(args.Context, "Map " + (set ? "disabled" : "enabled"));
    });
  }
}
