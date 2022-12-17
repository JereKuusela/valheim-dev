using HarmonyLib;
using UnityEngine;

namespace ServerDevcommands;
[HarmonyPatch(typeof(Chat), nameof(Chat.SendText))]
public class ServerChat
{
  static void Postfix(Talker.Type type, string text)
  {
    if (Player.m_localPlayer) return;
    ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, "ChatMessage", new object[] {
      Vector3.zero,
      (int)type,
      "Server",
      text,
      "Server"
    });
  }
}
