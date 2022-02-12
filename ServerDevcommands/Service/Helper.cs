using System.Globalization;
using System.Linq;
using UnityEngine;

namespace ServerDevcommands {

  ///<summary>Contains functions for parsing arguments, etc.</summary>
  public abstract class Helper {
    public static void AddMessage(Terminal context, string message) {
      context.AddString(message);
      Player.m_localPlayer?.Message(MessageHud.MessageType.TopLeft, message);
    }
    public static GameObject GetPrefab(string name) {
      name = name.ToLower();
      var realName = ParameterInfo.Ids.Find(id => id.ToLower() == name);
      if (string.IsNullOrEmpty(realName))
        Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "Missing object " + name, 0, null);
      var prefab = ZNetScene.instance.GetPrefab(realName);
      if (!prefab)
        Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "Missing object " + name, 0, null);
      return prefab;
    }
    public static string GetPrefabName(int hash) {
      var prefab = ZNetScene.instance.GetPrefab(hash);
      if (!prefab) return "";
      return Utils.GetPrefabName(prefab);
    }

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

    public static float RandomValue(Range<float> range) => UnityEngine.Random.Range(range.Min, range.Max);
    public static int RandomValue(Range<int> range) => UnityEngine.Random.Range(range.Min, range.Max + 1);
    public static Vector3 RandomValue(Range<Vector3> range) {
      var x = UnityEngine.Random.Range(range.Min.x, range.Max.x);
      var y = UnityEngine.Random.Range(range.Min.y, range.Max.y);
      var z = UnityEngine.Random.Range(range.Min.z, range.Max.z);
      return new Vector3(x, y, z);
    }
    public static Quaternion RandomValue(Range<Quaternion> range) {
      var x = UnityEngine.Random.Range(range.Min.x, range.Max.x);
      var y = UnityEngine.Random.Range(range.Min.y, range.Max.y);
      var z = UnityEngine.Random.Range(range.Min.z, range.Max.z);
      var w = UnityEngine.Random.Range(range.Min.w, range.Max.w);
      return new Quaternion(x, y, z, w);
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
    public static string PrintVectorXZY(Vector3 vector) => vector.x.ToString(CultureInfo.InvariantCulture) + "," + vector.z.ToString(CultureInfo.InvariantCulture) + "," + vector.y.ToString(CultureInfo.InvariantCulture);
    public static string PrintVectorYXZ(Vector3 vector) => vector.y.ToString(CultureInfo.InvariantCulture) + "," + vector.x.ToString(CultureInfo.InvariantCulture) + "," + vector.z.ToString(CultureInfo.InvariantCulture);
    public static string PrintAngleYXZ(Quaternion quaternion) => PrintVectorYXZ(quaternion.eulerAngles);

  }
}