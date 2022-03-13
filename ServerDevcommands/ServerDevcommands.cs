using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace ServerDevcommands {
  [BepInPlugin("valheim.jerekuusela.server_devcommands", "ServerDevcommands", "1.12.0.0")]
  public class ServerDevcommands : BaseUnityPlugin {
    public static ManualLogSource Log;
    public void Awake() {
      Log = Logger;
      Harmony harmony = new Harmony("valheim.jerekuusela.server_devcommands");
      harmony.PatchAll();
      Admin.Instance = new DevCommandsAdmin();
      Settings.Init(Config);
      Console.SetConsoleEnabled(true);
    }

    public void LateUpdate() {
      CommandQueue.TickQueue(Time.deltaTime);
    }
  }

  [HarmonyPatch(typeof(Terminal), nameof(Terminal.InitTerminal))]
  public class SetCommands {
    public static void Postfix() {
      new DevcommandsCommand();
      new ConfigCommand();
      new StartEventCommand();
      new PosCommand();
      new AliasCommand();
      new SearchCommand();
      new UndoRedoCommand();
      new TutorialToggleCommand();
      new ResolutionCommand();
      new WaitCommand();
      new ServerCommand();
      new HUDCommand();
      new NoMapCommand();
      DefaultAutoComplete.Register();
      Settings.RegisterCommands();
    }
  }
}
