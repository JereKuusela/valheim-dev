using UnityEngine;

namespace Service {

  ///<summary>Contains functions for parsing arguments, etc.</summary>
  public abstract class Helper {
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
  }
}