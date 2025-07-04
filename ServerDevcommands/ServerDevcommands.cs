﻿using System.IO;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
namespace ServerDevcommands;
[BepInDependency(COMFY_GIZMO_GUID, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(RELOADED_GIZMO_GUID, BepInDependency.DependencyFlags.SoftDependency)]
[BepInPlugin(GUID, NAME, VERSION)]
public class ServerDevcommands : BaseUnityPlugin
{
  public const string GUID = "server_devcommands";
  public const string NAME = "Server Devcommands";
  public const string VERSION = "1.97";
  public const string COMFY_GIZMO_GUID = "bruce.valheim.comfymods.gizmo";
  public const string RELOADED_GIZMO_GUID = "m3to.mods.GizmoReloaded";
  private static ManualLogSource? Logs;
  public static ManualLogSource Log => Logs!;
  public void Awake()
  {
    Logs = Logger;
    Harmony harmony = new(GUID);
    harmony.PatchAll();
    Admin.Instance = new DevCommandsAdmin();
    Settings.Init(Config);
    Console.SetConsoleEnabled(true);
    try
    {
      SetupWatcher();
      AliasManager.SetupWatcher();
      BindManager.SetupWatcher();
    }
    catch
    {
      //
    }
  }
  public void Start()
  {
    if (Chainloader.PluginInfos.TryGetValue(COMFY_GIZMO_GUID, out var info))
      ComfyGizmoPatcher.DoPatching(info.Instance.GetType().Assembly);
    if (Chainloader.PluginInfos.TryGetValue(RELOADED_GIZMO_GUID, out info))
      GizmoReloadedPatcher.DoPatching(info.Instance.GetType().Assembly);
  }

  public void LateUpdate()
  {
    MultiCommands.Execute(Time.deltaTime);
    MouseWheelBinding.Execute(ZInput.GetMouseScrollWheel() * 20f);
    if (AliasManager.ToBeSaved) AliasManager.ToFile();
    if (BindManager.ToBeSaved) BindManager.ToFile();
  }

#pragma warning disable IDE0051
  private void OnDestroy()
  {
    Config.Save();
  }
#pragma warning restore IDE0051
  private void SetupWatcher()
  {
    FileSystemWatcher watcher = new(Path.GetDirectoryName(Config.ConfigFilePath), Path.GetFileName(Config.ConfigFilePath));
    watcher.Changed += ReadConfigValues;
    watcher.Created += ReadConfigValues;
    watcher.Renamed += ReadConfigValues;
    watcher.IncludeSubdirectories = true;
    watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
    watcher.EnableRaisingEvents = true;
  }

  private void ReadConfigValues(object sender, FileSystemEventArgs e)
  {
    if (!File.Exists(Config.ConfigFilePath)) return;
    try
    {
      Log.LogDebug("ReadConfigValues called");
      Config.Reload();
    }
    catch
    {
      Log.LogError($"There was an issue loading your {Config.ConfigFilePath}");
      Log.LogError("Please check your config entries for spelling and format!");
    }
  }
}

[HarmonyPatch(typeof(Terminal), nameof(Terminal.InitTerminal)), HarmonyPriority(Priority.HigherThanNormal)]
public class SetCommands
{
  private static bool Initialized = false;
  static void Postfix()
  {
    if (Initialized) return;
    Initialized = true;
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
    new InventoryCommand();
    new CalmCommand();
    new RepairCommand();
    new AddStatusCommand();
    new PlayerListCommand();
    new FindCommand();
    new PullCommand();
    new ResetDungeonCommand();
    new TeleportCommand();
    new SearchComponentCommand();
    new SearchItemCommand();
    new RPCCommand();
    new DmgCommand();
    new KillCommand();
    new ShutdownCommand();
    DefaultAutoComplete.Register();
    AliasManager.Init();
    Settings.RegisterCommands();
  }
}
