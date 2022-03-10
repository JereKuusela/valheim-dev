using HarmonyLib;

namespace ServerDevcommands {

  ///<summary>Static accessors for easier usage.</summary>
  public static class Admin {
    public static IAdmin Instance = new DefaultAdmin();
    ///<summary>Admin status.</summary>
    public static bool Enabled {
      get => Instance.Enabled;
      set => Instance.Enabled = value;
    }
    ///<summary>Is admin status currently being checked.</summary>
    public static bool Checking {
      get => Instance.Checking;
      set => Instance.Checking = value;
    }
    ///<summary>Checks for admin status. Terminal is used for the output.</summary>
    public static void ManualCheck(Terminal terminal = null) => Instance.ManualCheck(terminal);
    ///<summary>Verifies the admin status with a given text. Shouldn't be called directly.</summary>
    public static void Verify(string text) => Instance.Verify(text);
    ///<summary>Automatic check at the start of joining servers. Shouldn't be called directly.</summary>
    public static void AutomaticCheck(Terminal terminal = null) => Instance.AutomaticCheck(terminal);
    ///<summary>Resets the admin status when joining servers. Shouldn't be called directly.</summary>
    public static void Reset(Terminal terminal = null) => Instance.Reset(terminal);
  }

  public interface IAdmin {
    bool Enabled { get; set; }
    bool Checking { get; set; }
    void ManualCheck(Terminal terminal = null);
    void Verify(string text);
    void AutomaticCheck(Terminal terminal = null);
    void Reset(Terminal terminal = null);
  }

  ///<summary>Admin checker. Can be extended by overloading OnSuccess and OnFail.</summary>
  public class DefaultAdmin : IAdmin {
    public virtual bool Enabled { get; set; }
    protected Terminal Context;
    ///<summary>Admin status is checked by issuing a dummy unban command.</summary>
    protected void Check(Terminal terminal = null) {
      if (!ZNet.instance) return;
      Context = terminal ?? Console.instance;
      Checking = true;
      if (ZNet.instance.IsServer())
        OnSuccess(Context);
      else
        ZNet.instance.Unban("admintest");
    }
    public void Verify(string text) {
      if (text == "Unbanning user admintest")
        OnSuccess(Context);
      else
        OnFail(Context);
    }

    public virtual void AutomaticCheck(Terminal terminal = null) {
      Enabled = false;
      Check(terminal);
    }
    public virtual bool Checking { get; set; }
    protected virtual void OnSuccess(Terminal terminal) {
      Checking = false;
      Enabled = true;
    }
    protected virtual void OnFail(Terminal terminal) {
      Checking = false;
      Enabled = false;
    }

    public void ManualCheck(Terminal terminal = null) {
      Check(terminal);
    }
    public void Reset(Terminal terminal = null) {
      OnFail(terminal);
    }
  }

  [HarmonyPatch(typeof(ZNet), nameof(ZNet.RPC_RemotePrint))]
  public class ZNet_RPC_RemotePrint {
    public static bool Prefix(string text) {
      if (!Admin.Checking) return true;
      Admin.Verify(text);
      return false;
    }
  }

  ///<summary>Check admin status on connect to ensure features are enabled/disabled when changing servers.</summary>
  [HarmonyPatch(typeof(ZNet), nameof(ZNet.RPC_PeerInfo))]
  public class AdminReset {
    public static void Postfix() {
      if (ZNet.m_connectionStatus == ZNet.ConnectionStatus.Connected) Admin.Reset(Console.instance);
    }
  }  ///<summary>Check admin status on connect to ensure features are enabled/disabled when changing servers.</summary>
  [HarmonyPatch(typeof(Player), nameof(Player.OnSpawned))]
  public class AdminCheck {
    public static void Postfix() {
      if (!Admin.Checking) Admin.AutomaticCheck(Console.instance);
    }
  }
}
