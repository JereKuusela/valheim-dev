using UnityEngine;
namespace ServerDevcommands;
///<summary>Adds support for running as the server.</summary>
public class NoMapCommand
{
  public NoMapCommand()
  {
    AutoComplete.Register("nomap", index =>
    {
      return index == 0 ? ParameterInfo.Create("1 = disable map, 0 = enable map, no value = toggle") : ParameterInfo.None;
    });
    new Terminal.ConsoleCommand("nomap", "[set] - Toggles or sets the nomap mode. Can be executed on the server.", (args) =>
    {
      var disableMap = false;
      var isServer = ZNet.instance && ZNet.instance.IsServer();
      var player = Player.m_localPlayer;
      if (!player && !isServer) return;
      if (args.Length < 2)
      {
        if (isServer) disableMap = !ZoneSystem.instance.GetGlobalKey(GlobalKeys.NoMap);
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
          ZoneSystem.instance.SetGlobalKey(GlobalKeys.NoMap);
        else
          ZoneSystem.instance.RemoveGlobalKey(GlobalKeys.NoMap);
      }
      ;
      if (player)
      {
        string key = "mapenabled_" + Player.m_localPlayer.GetPlayerName();
        PlayerPrefs.SetFloat(key, disableMap ? 0f : 1f);
      }
      ;
      var target = "client and server";
      if (player && !isServer) target = "client";
      if (!player && isServer) target = "server";
      Helper.AddMessage(args.Context, $"Map {(disableMap ? "disabled" : "enabled")} for the {target}.");
    });
  }
}
