using System.Globalization;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
namespace ServerDevcommands;
public class MouseWheelBinding {
  ///<summary>Runs any bound commands.</summary>
  public static void Execute(float ticks) {
    if (ticks == 0f) return;
    if (!Chat.instance) return;
    if (!Terminal.m_binds.TryGetValue(Settings.MouseWheelBindKey, out var commands)) return;
    commands = commands.Where(BindCommand.Valid).ToList();
    if (commands.Count == 0) return;
    if (Settings.BestCommandMatch) {
      var max = commands.Max(BindCommand.CountKeys);
      commands = commands.Where(cmd => BindCommand.CountKeys(cmd) == max).ToList();
    }
    var ticksStr = ticks.ToString(CultureInfo.InvariantCulture);
    commands = commands.Select(BindCommand.CleanUp).Select(cmd => TerminalUtils.Substitute(cmd, ticksStr)).ToList();
    foreach (var cmd in commands) Chat.instance.TryRunCommand(cmd, true, true);
  }
  ///<summary>Returns whether any commands could run with the current modifier keys.</summary>
  public static bool CouldExecute() {
    if (Terminal.m_binds.TryGetValue(Settings.MouseWheelBindKey, out var commands))
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
