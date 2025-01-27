using HarmonyLib;
namespace ServerDevcommands;
[HarmonyPatch(typeof(ZNet), nameof(ZNet.Start))]
public class RootUsers
{

  public static void Update()
  {
    if (!ZNet.instance || !ZNet.m_isServer) return;
    try
    {
      var rootUsers = Parse.Split(Settings.configRootUsers.Value);
      foreach (var user in rootUsers) ZNet.instance.m_adminList.Add(user);
    }
    catch
    {
      ServerDevcommands.Log.LogWarning("Adding root users to the admin list caused an exception. This should be ok.");
    }
  }

  static void Postfix() => Update();
}
