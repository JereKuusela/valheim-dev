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
    zs.m_globalKeys = zs.m_globalKeys.Where(key => !Settings.IsGlobalKeyDisabled(key)).ToHashSet();
    zs.m_globalKeysValues = zs.m_globalKeysValues.Where(key => !Settings.IsGlobalKeyDisabled(key.Key)).ToDictionary(key => key.Key, static key => key.Value);
    zs.SendGlobalKeys(ZRoutedRpc.Everybody);
  }
}
