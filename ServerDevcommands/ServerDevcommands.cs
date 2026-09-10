using System.IO;
using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;
using Service;
using UnityEngine;
namespace ServerDevcommands;

[BepInDependency(COMFY_GIZMO_GUID, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(RELOADED_GIZMO_GUID, BepInDependency.DependencyFlags.SoftDependency)]
[BepInPlugin(GUID, NAME, VERSION)]
public class ServerDevcommands : BaseUnityPlugin
{
  public const string GUID = "server_devcommands";
  public const string NAME = "Server Devcommands";
  public const string VERSION = "1.110";
  public const string COMFY_GIZMO_GUID = "bruce.valheim.comfymods.gizmo";
  public const string RELOADED_GIZMO_GUID = "m3to.mods.GizmoReloaded";
  public void Awake()
  {
    Log.Init(Logger);
    Settings.Init(Config);
    Harmony harmony = new(GUID);
    harmony.PatchAll();

    try
    {
      SetupWatcher();
      PermissionLoader.SetupWatcher();
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
    EWP.Api.RegisterGroupHandler("serverdevcommands", PermissionApi.HasGroup);
  }

  public void LateUpdate()
  {
    MultiCommands.Execute(Time.deltaTime);
    MouseWheelBinding.Execute(ZInput.GetMouseScrollWheel() * 20f);
    if (AliasManager.ToBeSaved) AliasManager.ToFile();
    if (BindManager.ToBeSaved) BindManager.ToFile();

    if (!ZNet.instance)
    {
      ParameterInfo.SetServerLocationIds(null);
      ParameterInfo.SetServerVegetationIds(null);
    }
    CommandInputResolver.CheckActiveRequest();
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
      Config.Reload();
    }
    catch
    {
      Log.Error($"There was an issue loading your {Config.ConfigFilePath}");
      Log.Error("Please check your config entries for spelling and format!");
    }
  }
}

[HarmonyPatch(typeof(Terminal), nameof(Terminal.InitTerminal)), HarmonyPriority(Priority.HigherThanNormal)]
public class InitializeTerminal
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
    new PermissionsCommand();
    new ShutdownCommand();
    DefaultAutoComplete.Register();
    AliasManager.Init();
    BindManager.Init();
  }
}

[HarmonyPatch(typeof(Chat), nameof(Chat.Awake))]
public class InitializeChat
{
  static void Postfix()
  {
    // Chat.Awake loads binds from player profile, so need to handle them after that.
    BindManager.Load();
  }
}

[HarmonyPatch(typeof(Console), nameof(Console.IsConsoleEnabled))]
public class IsConsoleEnabled
{
  static void Postfix(ref bool __result)
  {
    __result = true;
  }
}
