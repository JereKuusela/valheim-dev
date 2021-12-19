using UnityEngine;
namespace DEV {
  public partial class Commands {
    public static void AddManipulate() {
      new Terminal.ConsoleCommand("move", "[x,z,y] [player/object/world] - Moves hovered object relative to selected origin (relative to player if not given).", delegate (Terminal.ConsoleEventArgs args) {
        if (args.Length < 2) return;
        var player = Player.m_localPlayer;
        if (player == null) return;
        var obj = player.GetHoverObject();
        if (obj == null) {
          args.Context.AddString("Error: Nothing being hovered");
          return;
        }
        var view = obj.GetComponentInParent<ZNetView>();
        if (view == null) {
          args.Context.AddString("Error: No net view.");
          return;
        }
        var zdo = view.GetZDO();
        var position = view.transform.position;
        var relative = ParsePositionXZY(args.Args[1]);
        var origin = args.Length > 2 ? args[2].ToLower() : "player";
        var rotation = player.transform.rotation;
        if (origin == "world")
          rotation = Quaternion.identity;
        if (origin == "object")
          rotation = view.transform.rotation;
        position += rotation * Vector3.forward * relative.x;
        position += rotation * Vector3.right * relative.z;
        position += rotation * Vector3.up * relative.y;
        zdo.SetPosition(position);
        view.transform.position = position;
      }, true, false, true, false, false);
      new Terminal.ConsoleCommand("rotate", "[amount/reset] [x,y,z] [player/object/world] - Rotates hovered object around a given axis relative to selected origin (relative to player y axis if not given).", delegate (Terminal.ConsoleEventArgs args) {
        if (args.Length < 2) return;
        var player = Player.m_localPlayer;
        if (player == null) return;
        var obj = player.GetHoverObject();
        if (obj == null) {
          args.Context.AddString("Error: Nothing being hovered");
          return;
        }
        var view = obj.GetComponentInParent<ZNetView>();
        if (view == null) {
          args.Context.AddString("Error: No net view.");
          return;
        }
        var zdo = view.GetZDO();
        if (args[1].ToLower() == "reset") {
          zdo.SetRotation(Quaternion.identity);
          view.transform.rotation = Quaternion.identity;
          return;
        }
        var amount = args.TryParameterFloat(1, 0);
        var axis = args.Length > 2 ? args[2].ToLower() : "y";
        var origin = args.Length > 3 ? args[3].ToLower() : "player";
        var originRotation = player.transform.rotation;
        if (origin == "world")
          originRotation = Quaternion.identity;
        if (origin == "object")
          originRotation = view.transform.rotation;
        var axisVector = originRotation * Vector3.up;
        if (axis == "x")
          axisVector = originRotation * Vector3.forward;
        if (axis == "z")
          axisVector = originRotation * Vector3.right;

        var rotation = view.transform.rotation * Quaternion.AngleAxis(amount, axisVector);
        zdo.SetRotation(rotation);
        view.transform.rotation = rotation;
      }, true, false, true, false, false);
      new Terminal.ConsoleCommand("scale", "[x,z,y] - Scales hovered object.", delegate (Terminal.ConsoleEventArgs args) {
        if (args.Length < 2) return;
        if (Player.m_localPlayer == null) return;
        var obj = Player.m_localPlayer.GetHoverObject();
        if (obj == null) {
          args.Context.AddString("Error: Nothing being hovered");
          return;
        }
        var view = obj.GetComponentInParent<ZNetView>();
        if (view == null) {
          args.Context.AddString("Error: No net view.");
          return;
        }
        var scale = ParsePositionXZY(args.Args[1], Vector3.one);
        if (view.m_syncInitialScale)
          view.SetLocalScale(scale);
      }, true, false, true, false, false);
    }
  }
}
