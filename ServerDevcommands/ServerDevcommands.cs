using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
namespace ServerDevcommands;
[BepInDependency("com.rolopogo.gizmo.comfy", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("m3to.mods.GizmoReloaded", BepInDependency.DependencyFlags.SoftDependency)]
[BepInPlugin("valheim.jerekuusela.server_devcommands", "ServerDevcommands", "1.21.0.0")]
public class ServerDevcommands : BaseUnityPlugin {
  private static ManualLogSource? Logs;
  public static ManualLogSource Log => Logs!;
  public void Awake() {
    Logs = Logger;
    Harmony harmony = new("valheim.jerekuusela.server_devcommands");
    harmony.PatchAll();
    Admin.Instance = new DevCommandsAdmin();
    Settings.Init(Config);
    Console.SetConsoleEnabled(true);
  }
  public void Start() {
    if (Chainloader.PluginInfos.TryGetValue("com.rolopogo.gizmo.comfy", out var info))
      ComfyGizmoPatcher.DoPatching(info.Instance.GetType().Assembly);
    if (Chainloader.PluginInfos.TryGetValue("m3to.mods.GizmoReloaded", out info))
      GizmoReloadedPatcher.DoPatching(info.Instance.GetType().Assembly);
  }

  public void LateUpdate() {
    CommandQueue.TickQueue(Time.deltaTime);
    MouseWheelBinding.Execute(Input.GetAxis("Mouse ScrollWheel"));
  }
}

[HarmonyPatch(typeof(Terminal), nameof(Terminal.InitTerminal))]
public class SetCommands {
  static void Postfix() {
    new DevcommandsCommand();
    new ConfigCommand();
    new StartEventCommand();
    new PosCommand();
    new AliasCommand();
    new SearchIdCommand();
    new UndoRedoCommand();
    new ResolutionCommand();
    new WaitCommand();
    new ServerCommand();
    new HUDCommand();
    new NoMapCommand();
    new BindCommand();
    new BroadcastCommand();
    new SeedCommand();
    new MoveSpawn();
    new MappingCommand();
    new WindCommand();
    new EnvCommand();
    new GotoCommand();
    new ResetSkillCommand();
    new RaiseSkillCommand();
    DefaultAutoComplete.Register();
    Settings.RegisterCommands();
  }
}
