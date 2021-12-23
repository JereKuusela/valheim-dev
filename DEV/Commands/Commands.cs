using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace DEV {

  public partial class Commands {
    public static int TryInt(string arg, int defaultValue = 1) {
      if (!int.TryParse(arg, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
        return defaultValue;
      return result;
    }
    public static uint TryUInt(string arg, uint defaultValue = 1) {
      if (!uint.TryParse(arg, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
        return defaultValue;
      return result;
    }
    public static long TryLong(string arg, long defaultValue = 1) {
      if (!long.TryParse(arg, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
        return defaultValue;
      return result;
    }
    public static int TryParameterInt(string[] args, int index, int defaultValue = 1) {
      if (args.Length <= index) return defaultValue;
      return TryInt(args[index], defaultValue);
    }
    public static long TryParameterLong(string[] args, int index, long defaultValue = 1) {
      if (args.Length <= index) return defaultValue;
      return TryLong(args[index], defaultValue);
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
    public static uint TryParameterUInt(string[] args, int index, uint defaultValue = 0) {
      if (args.Length <= index) return defaultValue;
      return TryUInt(args[index], defaultValue);
    }
    public static string TryParameterString(string[] args, int index, string defaultValue = "") {
      if (args.Length <= index) return defaultValue;
      return args[index];
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

    private static Quaternion ParseAngleYXZ(string arg) => ParseAngleYXZ(arg, Quaternion.identity);
    private static Quaternion ParseAngleYXZ(string arg, Quaternion defaultValue) {
      var values = TrySplit(arg, ",");
      var angle = Vector3.zero;
      angle.y = TryParameterFloat(values, 0, defaultValue.eulerAngles.y);
      angle.x = TryParameterFloat(values, 1, defaultValue.eulerAngles.x);
      angle.z = TryParameterFloat(values, 2, defaultValue.eulerAngles.z);
      return Quaternion.Euler(angle);
    }
    private static Vector3 ParsePositionXZY(string arg) => ParsePositionXZY(arg, Vector3.zero);
    private static Vector3 ParsePositionXZY(string arg, Vector3 defaultValue) {
      var values = TrySplit(arg, ",");
      var vector = Vector3.zero;
      vector.x = TryParameterFloat(values, 0, defaultValue.x);
      vector.z = TryParameterFloat(values, 1, defaultValue.z);
      vector.y = TryParameterFloat(values, 2, defaultValue.y);
      return vector;
    }
    private static Vector3 ParsePositionYXZ(string arg) => ParsePositionYXZ(arg, Vector3.zero);
    private static Vector3 ParsePositionYXZ(string arg, Vector3 defaultValue) {
      var values = TrySplit(arg, ",");
      var vector = Vector3.zero;
      vector.y = TryParameterFloat(values, 0, defaultValue.y);
      vector.x = TryParameterFloat(values, 1, defaultValue.x);
      vector.z = TryParameterFloat(values, 2, defaultValue.z);
      return vector;
    }

    private static string PrintVectorXZY(Vector3 vector) => vector.x.ToString(CultureInfo.InvariantCulture) + "," + vector.z.ToString(CultureInfo.InvariantCulture) + "," + vector.y.ToString(CultureInfo.InvariantCulture);
    private static string PrintVectorYXZ(Vector3 vector) => vector.y.ToString(CultureInfo.InvariantCulture) + "," + vector.x.ToString(CultureInfo.InvariantCulture) + "," + vector.z.ToString(CultureInfo.InvariantCulture);

    private static string PrintAngleYXZ(Quaternion quaternion) => PrintVectorYXZ(quaternion.eulerAngles);

    public static void SendCommand(string command) {
      var server = ZNet.instance.GetServerRPC();
      Console.instance.AddString("Sending command: " + command);
      if (server != null) server.Invoke(ServerCommands.RPC_Command, new object[] { command });
    }
    private static void SendCommand(IEnumerable<string> args) => SendCommand(string.Join(" ", args));
    private static void SendCommand(Terminal.ConsoleEventArgs args) => SendCommand(args.Args);
  }

}