using HarmonyLib;
namespace ServerDevcommands;
[HarmonyPatch(typeof(Player), nameof(Player.Awake))]
public class AutomaticPickUp
{
  static void Postfix(Player __instance)
  {
    __instance.m_enableAutoPickup = Settings.AutomaticItemPickUp;
  }
}
