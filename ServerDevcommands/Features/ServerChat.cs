using HarmonyLib;
using Splatform;
using UnityEngine;

namespace ServerDevcommands;
[HarmonyPatch(typeof(Chat), nameof(Chat.SendText))]
public class ServerChat
{
  static void Postfix(Talker.Type type, string text)
  {
    if (Player.m_localPlayer) return;
    UserInfo info = new() { Name = "Server", UserId = PlatformManager.DistributionPlatform.LocalUser.PlatformUserID };
    ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, "ChatMessage", [
      Vector3.zero,
      (int)type,
      info,
      text,
    ]);
  }
}
