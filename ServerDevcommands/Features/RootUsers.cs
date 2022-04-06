using HarmonyLib;
namespace ServerDevcommands;
[HarmonyPatch(typeof(ZNet), nameof(ZNet.Awake))]
public class RootUsers {

  public static void Update() {
    if (!ZNet.instance || !ZNet.m_isServer) return;
    var rootUsers = Parse.Split(Settings.configRootUsers.Value);
    foreach (var user in rootUsers) ZNet.instance.m_adminList.Add(user);
  }

  static void Postfix() => Update();
}
