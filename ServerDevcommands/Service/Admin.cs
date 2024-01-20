using HarmonyLib;
namespace ServerDevcommands;

[HarmonyPatch(typeof(ZNet), nameof(ZNet.Awake))]
public class AdminReset
{
  // Dedicated should always be able to use devcommands.
  // Otherwise reset becase it depends on admin status and AutoDevcommands setting.
  static void Postfix(ZNet __instance) => Terminal.m_cheat = __instance.IsDedicated();
}

// This is the most reliable way, since admin list is not synced on single player.
[HarmonyPatch(typeof(Player), nameof(Player.OnSpawned))]
public class AdminCheck
{
  static void Postfix()
  {
    if (Settings.AutoDevcommands)
      DevcommandsCommand.Set(IsAdmin());
  }

  public static bool IsAdmin()
  {
    if (ZNet.instance.IsServer()) return true;
    var list = ZNet.instance.GetAdminList();
    var id = UserInfo.GetLocalUser().NetworkUserId;
    if (list.Contains(id)) return true;
    if (id.StartsWith(PrivilegeManager.GetPlatformPrefix(PrivilegeManager.Platform.Steam)))
      id = id.Substring(PrivilegeManager.GetPlatformPrefix(PrivilegeManager.Platform.Steam).Length);
    if (list.Contains(id)) return true;
    if (!id.Contains("_"))
      id = PrivilegeManager.GetPlatformPrefix(PrivilegeManager.Platform.Steam) + id;
    return list.Contains(id);

  }
}

// But good to handle this too if admin list changes while in game.
[HarmonyPatch(typeof(ZNet), nameof(ZNet.RPC_AdminList))]
public class RPC_AdminList
{
  static void Postfix()
  {
    // Will be handled by Player.OnSpawned.
    if (!Player.m_localPlayer) return;
    var admin = AdminCheck.IsAdmin();
    // No need to set if already set.
    if (admin == Terminal.m_cheat) return;
    // Only enable if AutoDevcommands is enabled.
    // But always disabled if not authorized.
    if (admin && !Settings.AutoDevcommands) return;
    DevcommandsCommand.Set(admin);
    if (admin)
      Console.instance.AddString("Authorized to use devcommands.");
  }
}
