using System;
using System.Collections.Generic;
using System.Linq;
namespace ServerDevcommands;
///<summary>Feature for disabling commands.</summary>
public class DisableCommands
{

  private static string[] DisallowedCommands = [];
  public static HashSet<string> RootUsers = [];

  public static bool CanRun(string command, ZRpc? rpc = null)
  {
    if (rpc != null && rpc.m_socket != null && RootUsers.Contains(rpc.m_socket.GetHostName())) return true;
    if (DisallowedCommands.Contains(command.ToLower())) return false;
    if (DisallowedCommands.Any(black => command.StartsWith(black + " ", StringComparison.OrdinalIgnoreCase))) return false;
    return true;
  }

  public static void UpdateCommands(string rootUsers, string blackList)
  {
    RootUsers = [.. Parse.Split(rootUsers)];
    DisallowedCommands = Parse.Split(blackList);
  }
}
