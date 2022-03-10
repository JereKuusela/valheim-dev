using HarmonyLib;

namespace ServerDevcommands {
  [HarmonyPatch(typeof(Player), nameof(Player.LateUpdate))]
  public class NoObjectCollision {
    public static void Postfix(Player __instance) {
      if (__instance != Player.m_localPlayer) return;
      __instance.m_collider.enabled = !Settings.FlyNoClip || !__instance.IsDebugFlying();

    }
  }
  [HarmonyPatch(typeof(Character), nameof(Character.UnderWorldCheck))]
  public class NoGroundCollision {
    public static bool Prefix(Character __instance) =>
    !Settings.FlyNoClip || __instance != Player.m_localPlayer || !__instance.IsDebugFlying();

  }
  [HarmonyPatch(typeof(GameCamera), nameof(GameCamera.GetCameraPosition))]
  public class NoCameraCapping {
    public static void Prefix(GameCamera __instance) {
      if (Player.m_localPlayer && Player.m_localPlayer.IsDebugFlying() && Settings.FlyNoClip)
        __instance.m_minWaterDistance = float.MinValue;
      else
        __instance.m_minWaterDistance = OriginalCamera.MinWaterDistance;
    }
  }
  [HarmonyPatch(typeof(GameCamera), nameof(GameCamera.Awake))]
  public class OriginalCamera {
    public static float MinWaterDistance = float.MinValue;
    public static void Postfix(GameCamera __instance) {
      MinWaterDistance = __instance.m_minWaterDistance;
    }
  }
  [HarmonyPatch(typeof(GameCamera), nameof(GameCamera.CollideRay2))]
  public class NoCameraClipping {
    public static bool Prefix(GameCamera __instance) {
      return !Settings.FlyNoClip || !Player.m_localPlayer || !Player.m_localPlayer.IsDebugFlying();
    }
  }
}
