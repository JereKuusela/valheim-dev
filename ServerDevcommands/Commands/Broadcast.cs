using System.Collections.Generic;
using System.Linq;
using Service;
namespace ServerDevcommands;
///<summary>New command for toggling HUD.</summary>
public class BroadcastCommand
{
  private static readonly List<string> Types = ["center", "side"];
  private static readonly List<string> Modifiers = ["<b", "<color", "<i", "<size"];
  public BroadcastCommand()
  {
    Helper.Command("broadcast", "[center/side] [message] - Broadcasts a message.", (args) =>
    {
      Helper.ArgsCheck(args, 2, "Missing the center/side parameter.");
      Helper.ArgsCheck(args, 3, "Missing the message");
      var type = args[2] == "side" ? MessageHud.MessageType.TopLeft : MessageHud.MessageType.Center;
      var message = string.Join(" ", args.Args.Skip(2));
      MessageHud.instance.MessageAll(type, message);
    });
    AutoComplete.Register("broadcast", index =>
    {
      if (index == 0) return Types;
      return Modifiers;
    }, new() {
      { "<b", index => ParameterInfo.Create("Bolds the text.") },
      { "<i", index => ParameterInfo.Create("Italics the text.") },
      { "<color", index => ParameterInfo.Colors },
      { "<size", index => ParameterInfo.Create("Size in pixels.") }
    });
    Helper.Command("message", "[player] [center/side] [message] - Sends a message to a player.", (args) =>
    {
      Helper.ArgsCheck(args, 2, "Missing the player parameter.");
      Helper.ArgsCheck(args, 3, "Missing the center/side parameter.");
      Helper.ArgsCheck(args, 4, "Missing the message");
      var players = PlayerInfo.FindPlayers(Parse.Split(args[1]));
      if (players.Count == 0) throw new System.Exception($"Player {args[1]} not found.");
      var type = args[3] == "side" ? MessageHud.MessageType.TopLeft : MessageHud.MessageType.Center;
      var message = string.Join(" ", args.Args.Skip(3));
      foreach (var player in players)
      {
        ZRoutedRpc.instance.InvokeRoutedRPC(player.PeerId, "ShowMessage", (int)type, message);
      }
    });
    AutoComplete.Register("message", index =>
    {
      if (index == 0) return ParameterInfo.PlayerNames;
      if (index == 1) return Types;
      return Modifiers;
    }, new() {
      { "<b", index => ParameterInfo.Create("Bolds the text.") },
      { "<i", index => ParameterInfo.Create("Italics the text.") },
      { "<color", index => ParameterInfo.Colors },
      { "<size", index => ParameterInfo.Create("Size in pixels.") }
    });
  }
}
