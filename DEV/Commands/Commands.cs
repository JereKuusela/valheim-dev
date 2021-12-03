using System.Collections.Generic;
using System.Globalization;
using System.Linq;
namespace DEV {

  public partial class Commands {
    public static int TryParameterInt(string[] args, int index, int defaultValue = 1) {
      int result;
      if (args.Length <= index || !int.TryParse(args[index], NumberStyles.Integer, CultureInfo.InvariantCulture, out result)) {
        return defaultValue;
      }
      return result;
    }
    public static float TryParameterFloat(string[] args, int index, float defaultValue = 1f) {
      float result;
      if (args.Length <= index || !float.TryParse(args[index], NumberStyles.Float, CultureInfo.InvariantCulture, out result)) {
        return defaultValue;
      }
      return result;
    }
    public static string[] TrySplit(string[] args, int index, string separator) {
      if (args.Length <= index)
        return new string[0];
      return args[index].Split(',').Select(s => s.Trim()).ToArray();
    }
    private static string[] AddPlayerPosXZ(string[] args, int count) {
      if (args.Length < count) return args;
      if (Player.m_localPlayer == null) return args;
      var parameters = args.ToList();
      var pos = Player.m_localPlayer.transform.position;
      if (parameters.Count < count + 1)
        parameters.Add(pos.x.ToString(CultureInfo.InvariantCulture));
      if (parameters.Count < count + 2)
        parameters.Add(pos.z.ToString(CultureInfo.InvariantCulture));
      return parameters.ToArray();
    }

    public static void SendCommand(string command) {
      var server = ZNet.instance.GetServerRPC();
      Console.instance.AddString("Sending command: " + command);
      if (server != null) server.Invoke(ServerCommands.RPC_Command, new object[] { command });
    }
    private static void SendCommand(IEnumerable<string> args) => SendCommand(string.Join(" ", args));
    private static void SendCommand(Terminal.ConsoleEventArgs args) => SendCommand(args.Args);
  }

}