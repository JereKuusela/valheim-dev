using System.Collections.Generic;
using System.Linq;
namespace ServerDevcommands;
///<summary>New command for toggling HUD.</summary>
public class BroadcastCommand
{
  private static List<string> Types = new() { "center", "side" };
  private static List<string> Modifiers = new() { "<b", "<color", "<i", "<size" };
  public BroadcastCommand()
  {
    Helper.Command("broadcast", "[center/side] [message] - Broadcasts a message.", (args) =>
    {
      Helper.ArgsCheck(args, 2, "Missing the center/side parameter.");
      Helper.ArgsCheck(args, 3, "Missing the message");
      var type = MessageHud.MessageType.Center;
      if (args[1] == "side") type = MessageHud.MessageType.TopLeft;
      var message = string.Join(" ", args.Args.Skip(2));
      MessageHud.instance.MessageAll(type, message);
    });
    AutoComplete.Register("broadcast", (int index) =>
    {
      if (index == 0) return Types;
      return Modifiers;
    }, new() {
      { "<b", (int index) => ParameterInfo.Create("Bolds the text.") },
      { "<i", (int index) => ParameterInfo.Create("Italics the text.") },
      { "<color", (int index) => ParameterInfo.Colors },
      { "<size", (int index) => ParameterInfo.Create("Size in pixels.") }
    });
  }
}
