using HarmonyLib;
using UnityEngine;

namespace ServerDevcommands;
[HarmonyPatch(typeof(Chat), nameof(Chat.SendText))]
public class ServerChat
{
  static void Postfix(Talker.Type type, string text)
  {
    if (Player.m_localPlayer) return;
    UserInfo info = new() { Gamertag = "Server", Name = "Server", NetworkUserId = "Server" };
    ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, "ChatMessage", [
      Vector3.zero,
      (int)type,
      info,
      text,
      "Server"
    ]);
  }
}
