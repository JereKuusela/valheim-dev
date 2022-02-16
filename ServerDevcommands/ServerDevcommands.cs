using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace ServerDevcommands {
  [BepInPlugin("valheim.jerekuusela.server_devcommands", "ServerDevcommands", "1.10.0.0")]
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
      TryRunCommand.TickQueue(Time.deltaTime);
    }
  }

  [HarmonyPatch(typeof(Terminal), "InitTerminal")]
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
      DefaultAutoComplete.Register();
      Settings.RegisterCommands();
    }
  }
}
