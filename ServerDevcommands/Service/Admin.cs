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
    PermissionManager.Instance.ResetToDefaults();
    if (ZNet.instance.IsServer())
      OnSuccess();
    else
    {
      // Player ID is not sent on connection, so it must be sent here to avoid race condition.
      ZNet.instance.Unban("admintest_" + GetPlayerId());
    }
  }
  private static long GetPlayerId()
  {
    var id = Game.instance.GetPlayerProfile().GetPlayerID();
    if (id != 0) return id;
    id = Player.m_localPlayer?.GetPlayerID() ?? 0;
    return id;
  }
  ///<summary>Verifies the admin status with a given text. Shouldn't be called directly.</summary>
  public static bool Verify(string text)
  {
    if (text == "Unbanning user admintest_" + GetPlayerId())
      OnSuccess();
    else if (text == "You are not admin")
      OnFail();
    else
      return false;
    return true;
  }

  ///<summary>Receives permissions from the server via RPC. Shouldn't be called directly.</summary>
  public static void ReceivePermissions(ZRpc rpc, ZPackage pkg)
  {
    PermissionManager.Instance.Read(pkg);
    if (PermissionManager.Instance.CanCheat)
      OnSuccess();
    else
      OnFail();
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
    PermissionManager.Instance.SetAdmin(true);
    DevcommandsCommand.Set(true);
    Console.instance.AddString("Authorized to use devcommands.");
    ServerExecution.RequestIds();
  }

  private static void OnFail()
  {
    Checking = false;
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
    bool wasCheck = Admin.Verify(text);
    return !wasCheck;
  }
}

[HarmonyPatch(typeof(Game), nameof(Game.Start))]
public class AdminReset
{
  static void Postfix()
  {
    Admin.Reset();
  }
}

[HarmonyPatch(typeof(Player), nameof(Player.OnSpawned))]
public class AdminCheck
{
  static void Postfix()
  {
    if (Game.instance.m_firstSpawn)
      Admin.AutomaticCheck();
    else
      DevcommandsCommand.EnableAutoFeatures();
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
