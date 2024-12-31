using System.Collections.Generic;
using System.Linq;
namespace ServerDevcommands;
///<summary>New command for toggling HUD.</summary>
public class BroadcastCommand
{
  private static readonly List<string> Types = ["center", "side"];
  private static readonly List<string> Modifiers = ["<b", "<color", "<i", "<size"];
  public BroadcastCommand()
  {
    Helper.Command("broadcast", "[center/side] [message] - Broadcasts a message.", static (args) =>
    {
      Helper.ArgsCheck(args, 2, "Missing the center/side parameter.");
      Helper.ArgsCheck(args, 3, "Missing the message");
      var type = MessageHud.MessageType.Center;
      if (args[1] == "side") type = MessageHud.MessageType.TopLeft;
      var message = string.Join(" ", args.Args.Skip(2));
      MessageHud.instance.MessageAll(type, message);
    });
    AutoComplete.Register("broadcast", static (int index) =>
    {
      if (index == 0) return Types;
      return Modifiers;
    }, new() {
      { "<b", static (int index) => ParameterInfo.Create("Bolds the text.") },
      { "<i", static (int index) => ParameterInfo.Create("Italics the text.") },
      { "<color", static (int index) => ParameterInfo.Colors },
      { "<size", static (int index) => ParameterInfo.Create("Size in pixels.") }
    });
  }
}
