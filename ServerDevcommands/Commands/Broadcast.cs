using System.Collections.Generic;
using System.Linq;

namespace ServerDevcommands {
  ///<summary>New command for toggling HUD.</summary>
  public class BroadcastCommand {
    private static List<string> Types = new List<string> { "center", "side" };
    private static List<string> Modifiers = new List<string> { "<b", "<color", "<i", "<size" };
    public BroadcastCommand() {
      new Terminal.ConsoleCommand("broadcast", "[center/side] [message] - Broadcasts a message.", delegate (Terminal.ConsoleEventArgs args) {
        if (args.Length < 3) return;
        var type = MessageHud.MessageType.Center;
        if (args[1] == "side") type = MessageHud.MessageType.TopLeft;
        var message = string.Join(" ", args.Args.Skip(2));
        MessageHud.instance.MessageAll(type, message);
      });
      AutoComplete.Register("broadcast", (int index) => {
        if (index == 0) return Types;
        return Modifiers;
      }, new Dictionary<string, System.Func<int, List<string>>>{
        {"<b", (int index) => ParameterInfo.Create("Bolds the text.") },
        {"<i", (int index) => ParameterInfo.Create("Italics the text.") },
        {"<color", (int index) => ParameterInfo.Colors },
        {"<size", (int index) => ParameterInfo.Create("Size in pixels.") }
      });
    }
  }
}
