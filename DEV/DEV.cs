using BepInEx;
using HarmonyLib;

namespace DEV {
  [BepInPlugin("valheim.jerekuusela.dev", "DEV", "1.3.0.0")]
  public class ESP : BaseUnityPlugin {
    public void Awake() {
      Harmony harmony = new Harmony("valheim.jerekuusela.dev");
      harmony.PatchAll();
    }
  }
}
