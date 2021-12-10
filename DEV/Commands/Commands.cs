using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace DEV {

  public partial class Commands {
    public static int TryInt(string arg, int defaultValue = 1) {
      if (!int.TryParse(arg, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
        return defaultValue;
      return result;
    }
    public static int TryParameterInt(string[] args, int index, int defaultValue = 1) {
      if (args.Length <= index) return defaultValue;
      return TryInt(args[index], defaultValue);
    }
    public static float TryFloat(string arg, float defaultValue = 1) {
      if (!float.TryParse(arg, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
        return defaultValue;
      return result;
    }
    public static float TryParameterFloat(string[] args, int index, float defaultValue = 1f) {
      if (args.Length <= index) return defaultValue;
      return TryFloat(args[index], defaultValue);
    }
    public static string[] TrySplit(string arg, string separator) => arg.Split(',').Select(s => s.Trim()).ToArray();
    public static string[] TryParameterSplit(string[] args, int index, string separator) {
      if (args.Length <= index) return new string[0];
      return TrySplit(args[index], separator);
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