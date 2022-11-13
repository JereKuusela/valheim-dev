using System;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace ServerDevcommands;
///<summary>Contains functions for parsing arguments, etc.</summary>
public abstract class Helper
{
  public static void AddMessage(Terminal context, string message)
  {
    context.AddString(message);
    Player.m_localPlayer?.Message(MessageHud.MessageType.TopLeft, message);
  }
  public static GameObject GetPrefab(string name)
  {
    name = name.ToLower();
    var realName = ParameterInfo.ObjectIds.Find(id => id.ToLower() == name);
    if (string.IsNullOrEmpty(realName))
      Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "Missing object " + name, 0, null);
    var prefab = ZNetScene.instance.GetPrefab(realName);
    if (!prefab)
      Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "Missing object " + name, 0, null);
    return prefab;
  }
  public static string GetPrefabName(int hash)
  {
    var prefab = ZNetScene.instance.GetPrefab(hash);
    if (!prefab) return "";
    return Utils.GetPrefabName(prefab);
  }


  public static bool Within(Range<float> range, float value)
  {
    if (range.Min == range.Max) return value <= range.Max;
    return range.Min <= value && value <= range.Max;
  }
  public static bool Within(Range<float> range1, Range<float> range2, float value1, float value2)
  {
    if (value1 > range1.Max) return false;
    if (value2 > range2.Max) return false;
    if (range1.Min == range1.Max || value1 >= range1.Min) return true;
    if (range2.Min == range2.Max || value2 >= range2.Min) return true;
    return false;
  }
  public static float RandomValue(Range<float> range) => UnityEngine.Random.Range(range.Min, range.Max);
  public static int RandomValue(Range<int> range) => UnityEngine.Random.Range(range.Min, range.Max + 1);
  public static Vector3 RandomValue(Range<Vector3> range)
  {
    var x = UnityEngine.Random.Range(range.Min.x, range.Max.x);
    var y = UnityEngine.Random.Range(range.Min.y, range.Max.y);
    var z = UnityEngine.Random.Range(range.Min.z, range.Max.z);
    return new(x, y, z);
  }
  public static Quaternion RandomValue(Range<Quaternion> range)
  {
    var x = UnityEngine.Random.Range(range.Min.x, range.Max.x);
    var y = UnityEngine.Random.Range(range.Min.y, range.Max.y);
    var z = UnityEngine.Random.Range(range.Min.z, range.Max.z);
    var w = UnityEngine.Random.Range(range.Min.w, range.Max.w);
    return new(x, y, z, w);
  }

  public static string[] AddPlayerPosXZ(string[] args, int count)
  {
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
  public static string[] AddPlayerPosXZY(string[] args, int count)
  {
    if (args.Length < count) return args;
    if (Player.m_localPlayer == null) return args;
    var parameters = args.ToList();
    var pos = Player.m_localPlayer.transform.position;
    if (parameters.Count < count + 1)
      parameters.Add(pos.x.ToString(CultureInfo.InvariantCulture) + "," + pos.z.ToString(CultureInfo.InvariantCulture) + "," + pos.y.ToString(CultureInfo.InvariantCulture));
    return parameters.ToArray();
  }
  public static ZNet.PlayerInfo FindPlayer(string name)
  {
    var players = ZNet.instance.m_players;
    var player = players.FirstOrDefault(player => player.m_name == name);
    if (!player.m_characterID.IsNone()) return player;
    player = players.FirstOrDefault(player => player.m_name.ToLower().StartsWith(name.ToLower()));
    if (!player.m_characterID.IsNone()) return player;
    player = players.FirstOrDefault(player => player.m_name.ToLower().Contains(name.ToLower()));
    if (!player.m_characterID.IsNone()) return player;
    throw new InvalidOperationException("Unable to find the player.");
  }
  public static string PrintVectorXZY(Vector3 vector) => vector.x.ToString(CultureInfo.InvariantCulture) + "," + vector.z.ToString(CultureInfo.InvariantCulture) + "," + vector.y.ToString(CultureInfo.InvariantCulture);
  public static string PrintVectorYXZ(Vector3 vector) => vector.y.ToString(CultureInfo.InvariantCulture) + "," + vector.x.ToString(CultureInfo.InvariantCulture) + "," + vector.z.ToString(CultureInfo.InvariantCulture);
  public static string PrintAngleYXZ(Quaternion quaternion) => PrintVectorYXZ(quaternion.eulerAngles);
  public static void AddError(Terminal context, string message)
  {
    AddMessage(context, $"Error: {message}");
  }
  public static bool? IsDown(string key)
  {
    if (key.StartsWith("-", StringComparison.OrdinalIgnoreCase))
      return Enum.TryParse<KeyCode>(key.Substring(1), true, out var code) ? !Input.GetKey(code) : null;
    else
      return Enum.TryParse<KeyCode>(key, true, out var code) ? Input.GetKey(code) : null;
  }
  public static Player GetPlayer()
  {
    var player = Player.m_localPlayer;
    if (!player) throw new InvalidOperationException("No player.");
    return player;
  }
  public static void ArgsCheck(Terminal.ConsoleEventArgs args, int amount, string message)
  {
    if (args.Length < amount) throw new InvalidOperationException(message);
  }
  public static void Command(string name, string description, Terminal.ConsoleEvent action, Terminal.ConsoleOptionsFetcher? fetcher = null)
  {
    new Terminal.ConsoleCommand(name, description, Helper.Catch(action), isCheat: true, isNetwork: true, optionsFetcher: fetcher);
  }
  public static Terminal.ConsoleEvent Catch(Terminal.ConsoleEvent action) =>
    (args) =>
    {
      try
      {
        action(args);
      }
      catch (InvalidOperationException e)
      {
        Helper.AddError(args.Context, e.Message);
      }
    };
}
