
using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using HarmonyLib;

namespace ServerDevcommands;


public class PermissionLoader
{
  public static string RPC_Permissions = "DEV_Permissions";
  public static PermissionData Data = new();
  public static string FileName = "permissions.yaml";
  public static string FilePath = Path.Combine(Paths.ConfigPath, FileName);
  private static bool SkipReload = false;

  public static PermissionEntry? GetOrCreatePeerEntry(string hostname, string characterId, string playerName)
  {
    if (Helper.IsClient()) return null;
    var key = PermissionData.PeerKey(hostname, characterId);
    if (key == "") return null;

    Data.UpdatePeer(hostname, characterId, playerName);
    return Data.GetOrCreate(key);
  }

  public static void Save()
  {
    if (Helper.IsClient()) return;

    SkipReload = true;
    try
    {
      ToFile();
    }
    finally
    {
      SkipReload = false;
    }
  }

  public static void SendPeerPermissions(string hostname, string characterId)
  {
    var zNet = ZNet.instance;
    if (zNet == null || !zNet.IsServer())
      return;

    var key = PermissionData.PeerKey(hostname, characterId);
    if (key == "")
      return;
    var peer = zNet.GetPeerByHostName(hostname);
    SendPermissions(peer.m_rpc, hostname, characterId);
  }


  public static void CreateFile()
  {
    if (File.Exists(FilePath)) return;
    Data.GetOrCreate("Everyone");
    ToFile();
  }

  public static void SendPermissions(ZRpc rpc, string hostname, string characterId)
  {
    var permissions = Data.Resolve(hostname, characterId);
    var pkg = new ZPackage();
    permissions.Write(pkg);
    rpc.Invoke(RPC_Permissions, pkg);
  }

  private static void SendChangedPermissions(HashSet<string> changedKeys, bool updateAllPlayers)
  {
    if (changedKeys.Count == 0) return;
    var zNet = ZNet.instance;
    if (zNet == null || !zNet.IsServer()) return;

    foreach (var peer in zNet.m_peers)
    {
      if (peer == null || peer.m_rpc == null || !peer.IsReady() || peer.m_characterID == ZDOID.None)
        continue;

      var hostname = peer.m_rpc.GetSocket().GetHostName();
      var zdo = ZDOMan.instance.GetZDO(peer.m_characterID);
      if (zdo == null) continue;
      var characterId = zdo.GetLong(ZDOVars.s_playerID).ToString();
      var key = PermissionData.PeerKey(hostname, characterId);
      if (key == "")
        continue;
      if (!updateAllPlayers && !changedKeys.Contains(key))
        continue;

      SendPermissions(peer.m_rpc, hostname, characterId);
    }
  }

  public static void ToFile()
  {
    if (Helper.IsClient()) return;
    File.WriteAllText(FilePath, PermissionYaml.SerializeEntries(Data.Entries));
  }

  public static void FromFile()
  {
    if (Helper.IsClient()) return;
    if (SkipReload) return;
    string value = File.ReadAllText(FilePath);
    var list = PermissionYaml.DeserializeEntries(value);
    var loadedData = new PermissionData(list);
    var changedKeys = PermissionData.ChangedKeys(Data, loadedData);
    var updateAllPlayers = PermissionData.HasGroupChanges(changedKeys, Data, loadedData);
    Data = loadedData;
    SendChangedPermissions(changedKeys, updateAllPlayers);
    ServerDevcommands.Log.LogInfo($"Reloading {Data.Count} permission data.");
  }
  public static void SetupWatcher()
  {
    CreateFile();
    Yaml.SetupWatcher(FileName, FromFile);
  }
  public static void UpdatePeer(ZRpc rpc, string characterId)
  {
    if (characterId == "" || characterId == "0") return;
    var peer = ZNet.instance.GetPeer(rpc);
    if (peer == null) return;
    var hostname = rpc.GetSocket().GetHostName();
    var updated = Data.UpdatePeer(hostname, characterId, peer.m_playerName);
    if (updated) Save();
  }
}


// Clients use unban to test permissions.
[HarmonyPatch(typeof(ZNet), nameof(ZNet.RPC_Unban))]
public class RPC_Unban
{

  static void Postfix(ZNet __instance, ZRpc rpc, string user)
  {
    if (!user.StartsWith("admintest_", StringComparison.Ordinal)) return;
    if (!__instance.IsServer()) return;
    var kvp = Parse.Kvp(user, '_');
    var characterId = kvp.Value;
    PermissionLoader.UpdatePeer(rpc, characterId);
    var hostname = rpc.GetSocket().GetHostName();
    PermissionLoader.SendPermissions(rpc, hostname, characterId);
  }
}