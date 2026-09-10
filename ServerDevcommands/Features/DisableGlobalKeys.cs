using System.Linq;
using HarmonyLib;
namespace ServerDevcommands;
[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.RPC_SetGlobalKey))]
public class DisableGlobalKeys
{
  static bool Prefix(string name) => !Settings.IsGlobalKeyDisabled(name);

  public static void RemoveDisabled()
  {
    if (!ZNet.instance || !ZNet.instance.IsServer()) return;
    var zs = ZoneSystem.instance;
    if (!zs) return;
    var toRemove = zs.m_globalKeys.Any(Settings.IsGlobalKeyDisabled);
    if (!toRemove) return;
    // Valheim 1.0: m_globalKeys is readonly; remove disabled keys in place instead of reassigning.
    zs.m_globalKeys.RemoveWhere(Settings.IsGlobalKeyDisabled);
    zs.m_globalKeysValues = zs.m_globalKeysValues.Where(key => !Settings.IsGlobalKeyDisabled(key.Key)).ToDictionary(key => key.Key, static key => key.Value);
    zs.SendGlobalKeys(ZRoutedRpc.Everybody);
  }
}
