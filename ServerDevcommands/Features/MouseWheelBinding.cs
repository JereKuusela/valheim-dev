using System.Globalization;
using System.Reflection;
using HarmonyLib;
namespace ServerDevcommands;
public class MouseWheelBinding
{
  public static void Execute(float ticks)
  {
    if (ticks == 0f) return;
    if (!Chat.instance) return;
    var binds = BindManager.GetBestWheelCommands();
    if (binds.Count == 0) return;
    var ticksStr = ticks.ToString(CultureInfo.InvariantCulture);
    foreach (var bind in binds) Chat.instance.TryRunCommand(TerminalUtils.Substitute(bind.Command, ticksStr), true, true);
  }
  public static bool CouldExecute()
  {
    if (ZInput.GetMouseScrollWheel() == 0f) return false;
    return BindManager.CouldWheelExecute();
  }
  public static int ExecuteCount()
  {
    return BindManager.GetBestWheelCount();
  }
}

[HarmonyPatch(typeof(Player), nameof(Player.UpdatePlacement))]
public class PreventRotation
{
  static void Prefix(Player __instance, ref int __state)
  {
    __state = __instance.m_placeRotation;
  }
  static void Postfix(Player __instance, int __state)
  {
    if (MouseWheelBinding.CouldExecute())
      __instance.m_placeRotation = __state;
  }
}

public class ComfyGizmoPatcher
{
  public static void DoPatching(Assembly assembly)
  {
    if (assembly == null) return;
    ServerDevcommands.Log.LogInfo("\"ComfyGizmo\" detected. Patching \"Rotate\" for mouse wheel binding.");
    Harmony harmony = new("valheim.jerekuusela.server_devcommand.comfygizmo");
    var mOriginal = AccessTools.Method(assembly.GetType("ComfyGizmo.RotationManager"), "Rotate");
    var mPrefix = SymbolExtensions.GetMethodInfo(() => Prefix());
    harmony.Patch(mOriginal, new(mPrefix));
  }

  static bool Prefix() => !MouseWheelBinding.CouldExecute();
}

public class GizmoReloadedPatcher
{
  public static void DoPatching(Assembly assembly)
  {
    if (assembly == null) return;
    ServerDevcommands.Log.LogInfo("\"GizmoReloaded\" detected. Patching \"HandleAxisInput\" for mouse wheel binding.");
    Harmony harmony = new("valheim.jerekuusela.server_devcommand.m3to.mods.GizmoReloaded");
    var mOriginal = AccessTools.Method(assembly.GetType("GizmoReloaded.Plugin"), "HandleAxisInput");
    var mPrefix = SymbolExtensions.GetMethodInfo(() => Prefix());
    harmony.Patch(mOriginal, new(mPrefix));
  }

  static bool Prefix() => !MouseWheelBinding.CouldExecute();
}
