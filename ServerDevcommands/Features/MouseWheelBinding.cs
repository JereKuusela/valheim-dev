using System.Globalization;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
namespace ServerDevcommands;
public class MouseWheelBinding {
  ///<summary>Runs any bound commands.</summary>
  public static void Execute(float ticks) {
    if (ticks == 0f || !Settings.MouseWheelBinding) return;
    if (Terminal.m_binds.TryGetValue(KeyCode.None, out var commands)) {
      foreach (var command in commands) {
        var input = TerminalUtils.Substitute(command, ticks.ToString(CultureInfo.InvariantCulture));
        Chat.instance.TryRunCommand(input);
      }
    }
  }
  ///<summary>Returns whether any commands could run with the current modifier keys.</summary>
  public static bool CouldExecute() {
    if (!Settings.MouseWheelBinding) return false;
    if (Terminal.m_binds.TryGetValue(KeyCode.None, out var commands))
      return commands.Any(BindCommand.Valid);
    return false;
  }
}

[HarmonyPatch(typeof(Player), nameof(Player.UpdatePlacement))]
public class PreventRotation {
  static void Prefix(Player __instance, ref int __state) {
    __state = __instance.m_placeRotation;
  }
  static void Postfix(Player __instance, int __state) {
    if (MouseWheelBinding.CouldExecute())
      __instance.m_placeRotation = __state;
  }
}
[HarmonyPatch(typeof(Player), nameof(Player.UpdatePlacementGhost))]
public class PreventGhostRotation {
  static void Prefix(Player __instance, ref Quaternion __state) {
    if (__instance.m_placementGhost)
      __state = __instance.m_placementGhost.transform.rotation;
  }
  static void Postfix(Player __instance, Quaternion __state) {
    if (__instance.m_placementGhost && MouseWheelBinding.CouldExecute())
      __instance.m_placementGhost.transform.rotation = __state;
  }
}

public class ComfyGizmoPatcher {
  public static void DoPatching(Assembly assembly) {
    if (assembly == null) return;
    ServerDevcommands.Log.LogInfo("\"ComfyGizmo\" detected. Patching \"HandleAxisInput\" for mouse wheel binding.");
    Harmony harmony = new("valheim.jerekuusela.server_devcommand.comfygizmo");
    var mOriginal = AccessTools.Method(assembly.GetType("Gizmo.ComfyGizmo"), "HandleAxisInput");
    var mPrefix = SymbolExtensions.GetMethodInfo(() => Prefix());
    harmony.Patch(mOriginal, new(mPrefix));
  }

  static bool Prefix() => !MouseWheelBinding.CouldExecute();
}

public class GizmoReloadedPatcher {
  public static void DoPatching(Assembly assembly) {
    if (assembly == null) return;
    ServerDevcommands.Log.LogInfo("\"GizmoReloaded\" detected. Patching \"HandleAxisInput\" for mouse wheel binding.");
    Harmony harmony = new("valheim.jerekuusela.server_devcommand.m3to.mods.GizmoReloaded");
    var mOriginal = AccessTools.Method(assembly.GetType("GizmoReloaded.Plugin"), "HandleAxisInput");
    var mPrefix = SymbolExtensions.GetMethodInfo(() => Prefix());
    harmony.Patch(mOriginal, new(mPrefix));
  }

  static bool Prefix() => !MouseWheelBinding.CouldExecute();
}
