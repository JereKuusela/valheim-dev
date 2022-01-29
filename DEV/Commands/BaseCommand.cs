using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace DEV {

  ///<summary>Contains functions for parsing arguments, etc.</summary>
  public abstract class BaseCommand {
    public static void AddMessage(Terminal context, string message) {
      context.AddString(message);
      Player.m_localPlayer?.Message(MessageHud.MessageType.TopLeft, message);
    }
    public static GameObject GetPrefab(string name) {
      var prefab = ZNetScene.instance.GetPrefab(name);
      if (!prefab)
        Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "Missing object " + name, 0, null);
      return prefab;
    }

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
    public static string[] AddPlayerPosXZ(string[] args, int count) {
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
    public static ZNet.PlayerInfo FindPlayer(string name) {
      var players = ZNet.instance.m_players;
      var player = players.FirstOrDefault(player => player.m_name == name);
      if (!player.m_characterID.IsNone()) return player;
      player = players.FirstOrDefault(player => player.m_name.ToLower().StartsWith(name.ToLower()));
      if (!player.m_characterID.IsNone()) return player;
      return players.FirstOrDefault(player => player.m_name.ToLower().Contains(name.ToLower()));
    }

    public static Quaternion ParseAngleYXZ(string arg) => ParseAngleYXZ(arg, Quaternion.identity);
    public static Quaternion ParseAngleYXZ(string arg, Quaternion defaultValue) {
      var values = TrySplit(arg, ",");
      var angle = Vector3.zero;
      angle.y = TryParameterFloat(values, 0, defaultValue.eulerAngles.y);
      angle.x = TryParameterFloat(values, 1, defaultValue.eulerAngles.x);
      angle.z = TryParameterFloat(values, 2, defaultValue.eulerAngles.z);
      return Quaternion.Euler(angle);
    }
    public static Vector3 TryVectorXZY(string arg) => TryVectorXZY(arg, Vector3.zero);
    public static Vector3 TryVectorXZY(string arg, Vector3 defaultValue) {
      var values = TrySplit(arg, ",");
      var vector = Vector3.zero;
      vector.x = TryParameterFloat(values, 0, defaultValue.x);
      vector.z = TryParameterFloat(values, 1, defaultValue.z);
      vector.y = TryParameterFloat(values, 2, defaultValue.y);
      return vector;
    }
    public static Vector3 TryScale(string arg) {
      var values = TrySplit(arg, ",");
      var vector = Vector3.one;
      if (values.Length == 1) {
        var value = TryFloat(arg, 1);
        vector = new Vector3(value, value, value);
      } else {
        vector = TryVectorXZY(arg, vector);
      }
      // Sanity check.
      if (vector.x == 0) vector.x = 1;
      if (vector.y == 0) vector.y = 1;
      if (vector.z == 0) vector.z = 1;
      return vector;
    }
    public static Vector3 TryParameterOffset(string[] args, int index) {
      if (args.Length <= index) return Vector3.zero;
      return TryVectorXZY(args[index], Vector3.zero);
    }
    public static Vector3 TryParameterRotation(string[] args, int index) {
      if (args.Length <= index) return Vector3.zero;
      return TryVectorYXZ(args[index], Vector3.zero);
    }
    public static Vector3 TryParameterScale(string[] args, int index) {
      if (args.Length <= index) return Vector3.one;
      return TryScale(args[index]);
    }
    public static Vector3 TryVectorYXZ(string arg) => TryVectorYXZ(arg, Vector3.zero);
    public static Vector3 TryVectorYXZ(string arg, Vector3 defaultValue) {
      var values = TrySplit(arg, ",");
      var vector = Vector3.zero;
      vector.y = TryParameterFloat(values, 0, defaultValue.y);
      vector.x = TryParameterFloat(values, 1, defaultValue.x);
      vector.z = TryParameterFloat(values, 2, defaultValue.z);
      return vector;
    }

    public static string PrintVectorXZY(Vector3 vector) => vector.x.ToString(CultureInfo.InvariantCulture) + "," + vector.z.ToString(CultureInfo.InvariantCulture) + "," + vector.y.ToString(CultureInfo.InvariantCulture);
    public static string PrintVectorYXZ(Vector3 vector) => vector.y.ToString(CultureInfo.InvariantCulture) + "," + vector.x.ToString(CultureInfo.InvariantCulture) + "," + vector.z.ToString(CultureInfo.InvariantCulture);

    public static string PrintAngleYXZ(Quaternion quaternion) => PrintVectorYXZ(quaternion.eulerAngles);

    ///<summary>Sends command to the server so that it can be executed there.</summary>
    public static void SendCommand(string command) {
      var server = ZNet.instance.GetServerRPC();
      Console.instance.AddString("Sending command: " + command);
      if (server != null) server.Invoke(ServerCommands.RPC_Command, new object[] { command });
    }
    ///<summary>Sends command to the server so that it can be executed there.</summary>
    public static void SendCommand(IEnumerable<string> args) => SendCommand(string.Join(" ", args));
    ///<summary>Sends command to the server so that it can be executed there.</summary>
    public static void SendCommand(Terminal.ConsoleEventArgs args) => SendCommand(args.Args);

    ///<summary>Returns the hovered object within 50 meters.</summary>
    public static ZNetView GetHovered(Terminal.ConsoleEventArgs args) {
      if (Player.m_localPlayer == null) return null;
      var interact = Player.m_localPlayer.m_maxInteractDistance;
      Player.m_localPlayer.m_maxInteractDistance = 50f;
      Player.m_localPlayer.FindHoverObject(out var obj, out var creature);
      Player.m_localPlayer.m_maxInteractDistance = interact;
      if (obj == null) {
        AddMessage(args.Context, "Nothing is being hovered.");
        return null;
      }
      var view = obj.GetComponentInParent<ZNetView>();
      if (view == null) {
        AddMessage(args.Context, "Nothing is being hovered.");
        return null;
      }
      return view;
    }
  }

}