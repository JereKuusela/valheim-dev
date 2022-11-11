using UnityEngine;
namespace ServerDevcommands;
///<summary>Adds support for running as the server.</summary>
public class NoMapCommand
{
  public NoMapCommand()
  {
    AutoComplete.Register("nomap", (int index) =>
    {
      if (index == 0) return ParameterInfo.Create("1 = disable map, 0 = enable map, no value = toggle");
      return ParameterInfo.None;
    });
    new Terminal.ConsoleCommand("nomap", "[set] - Toggles or sets the nomap mode. Can be executed on the server.", (args) =>
    {
      var disableMap = false;
      var isServer = ZNet.instance && ZNet.instance.IsServer();
      var player = Player.m_localPlayer;
      if (!player && !isServer) return;
      if (args.Length < 2)
      {
        if (isServer) disableMap = !ZoneSystem.instance.GetGlobalKey("nomap");
        else if (player)
        {
          string key = "mapenabled_" + Player.m_localPlayer.GetPlayerName();
          disableMap = PlayerPrefs.GetFloat(key, 1f) == 1f;
        }
      }
      else
        disableMap = args[1] == "1";
      if (isServer)
      {
        if (disableMap)
          ZoneSystem.instance.SetGlobalKey("nomap");
        else
          ZoneSystem.instance.RemoveGlobalKey("nomap");
      };
      if (player)
      {
        string key = "mapenabled_" + Player.m_localPlayer.GetPlayerName();
        PlayerPrefs.SetFloat(key, disableMap ? 0f : 1f);
      };
      var target = "client and server";
      if (player && !isServer) target = "client";
      if (!player && isServer) target = "server";
      Helper.AddMessage(args.Context, $"Map {(disableMap ? "disabled" : "enabled")} for the {target}.");
    });
  }
}
