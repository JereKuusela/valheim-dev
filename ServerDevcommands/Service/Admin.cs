using HarmonyLib;
using Splatform;
namespace ServerDevcommands;
///<summary>Admin checker for devcommands.</summary>
public static class Admin
{
  ///<summary>Is admin status currently being checked.</summary>
  public static bool Checking { get; set; }

  ///<summary>Admin status is checked by issuing a dummy unban command.</summary>
  private static void Check()
  {
    if (!ZNet.instance) return;
    Checking = true;
    if (ZNet.instance.IsServer())
      OnSuccess();
    else
    {
      PermissionManager.Instance.ResetToDefaults();
      ZNet.instance.Unban("admintest");
    }
  }

  ///<summary>Verifies the admin status with a given text. Shouldn't be called directly.</summary>
  public static void Verify(string text)
  {
    if (text == "Unbanning user admintest")
      OnSuccess();
    else
      OnFail();
  }

  ///<summary>Receives permissions from the server via RPC. Shouldn't be called directly.</summary>
  public static void ReceivePermissions(ZRpc rpc, ZPackage pkg)
  {
    PermissionManager.Instance.Read(pkg);
  }

  ///<summary>Automatic check at the start of joining servers. Shouldn't be called directly.</summary>
  public static void AutomaticCheck()
  {
    if (!Settings.AutoDevcommands) return;
    Check();
  }

  private static void OnSuccess()
  {
    Checking = false;
    DevcommandsCommand.Set(true);
    Console.instance.AddString("Authorized to use devcommands.");
    ServerExecution.RequestIds();
  }

  private static void OnFail()
  {
    Checking = false;
    DevcommandsCommand.Set(false);
    Console.instance.AddString("Unauthorized to use devcommands.");
  }

  ///<summary>Checks for admin status. Terminal is used for the output.</summary>
  public static void ManualCheck()
  {
    Check();
  }

  ///<summary>Resets the admin status when joining servers. Shouldn't be called directly.</summary>
  public static void Reset()
  {
    Checking = false;
    DevcommandsCommand.Set(false);
  }
}

[HarmonyPatch(typeof(ZNet), nameof(ZNet.RPC_RemotePrint))]
public class ZNet_RPC_RemotePrint
{
  static bool Prefix(string text)
  {
    if (!Admin.Checking) return true;
    Admin.Verify(text);
    return false;
  }
}

///<summary>Check admin status on connect to ensure features are enabled/disabled when changing servers.</summary>
[HarmonyPatch(typeof(Game), nameof(Game.Awake))]
public class AdminReset
{
  static void Postfix()
  {
    Admin.Reset();
  }
}  ///<summary>Check admin status on connect to ensure features are enabled/disabled when changing servers.</summary>
[HarmonyPatch(typeof(Player), nameof(Player.OnSpawned))]
public class AdminCheck
{
  static void Postfix()
  {
    if (!Admin.Checking) Admin.AutomaticCheck();
  }
}


[HarmonyPatch(typeof(ZNet), nameof(ZNet.ListContainsId))]
public class ListContainsId
{
  static void Prefix(ref string idString)
  {
    if (idString.Contains("_")) return;
    idString = PlatformUserID.GetPlatformPrefix(ZNet.instance.m_steamPlatform) + idString;
  }
}
