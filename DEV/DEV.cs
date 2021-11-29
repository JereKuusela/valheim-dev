using BepInEx;
using HarmonyLib;
using Service;

namespace DEV {
  [BepInPlugin("valheim.jerekuusela.dev", "DEV", "1.4.0.0")]
  public class ESP : BaseUnityPlugin {
    public void Awake() {
      Harmony harmony = new Harmony("valheim.jerekuusela.dev");
      harmony.PatchAll();
      Admin.Instance = new DevAdmin();
    }
  }
}
