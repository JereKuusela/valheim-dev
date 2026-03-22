
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using BepInEx;
using HarmonyLib;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ServerDevcommands;

public class PermissionEntry
{

  [DefaultValue("")]
  public string id = "";
  [DefaultValue("")]
  public string name = "";
  [DefaultValue("")]
  public string character = "";
  [DefaultValue("")]
  public string group = "";
  // Feature has format "key: value" where key is the feature and value is yes/no/force (defaults to yes if omitted).
  public Dictionary<string, List<string>>? features = null;
  // Command has format "command: value" where value is yes/no/force (defaults to yes if omitted).
  public List<string>? commands = null;
}


public class PermissionData
{
  public static Dictionary<string, PermissionEntry> Data = [];
  public static string FileName = "permissions.yaml";
  public static string FilePath = Path.Combine(Paths.ConfigPath, FileName);
  private static bool SkipReload = false;



  public static void CreateFile()
  {
    if (File.Exists(FilePath)) return;
    File.WriteAllText(FilePath, "");
  }
  public static void ToFile()
  {
    if (Helper.IsClient()) return;
    var yaml = Serializer().Serialize(Data);
    File.WriteAllText(FilePath, yaml);
  }

  public static void FromFile()
  {
    if (Helper.IsClient()) return;
    if (SkipReload) return;
    string value = File.ReadAllText(FilePath);
    Data = Deserialize<Dictionary<string, PermissionEntry>>(value, "Data") ?? [];
    ServerDevcommands.Log.LogInfo($"Reloading {Data.Count} permission data.");
  }
  public static void SetupWatcher()
  {
    Yaml.SetupWatcher(FileName, FromFile);
  }

  public static IDeserializer Deserializer() => new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance)
    .WithYamlFormatter(formatter).Build();
  public static IDeserializer DeserializerUnSafe() => new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance)
  .WithYamlFormatter(formatter).IgnoreUnmatchedProperties().Build();
  public static ISerializer Serializer() => new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).DisableAliases()
    .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitDefaults).WithYamlFormatter(formatter).Build();

  private static readonly YamlFormatter formatter = new() { NumberFormat = NumberFormatInfo.InvariantInfo };

  public static T Deserialize<T>(string raw, string fileName) where T : new()
  {
    try
    {
      return Deserializer().Deserialize<T>(raw);
    }
    catch (Exception ex1)
    {
      ServerDevcommands.Log.LogError($"{fileName}: {ex1.Message}");
      try
      {
        return DeserializerUnSafe().Deserialize<T>(raw);
      }
      catch (Exception)
      {
        return new();
      }
    }
  }

  public static Dictionary<long, T> Read<T>(string pattern)
  {
    Dictionary<long, T> ret = [];
    foreach (var name in Directory.GetFiles(Paths.ConfigPath, pattern))
    {
      var data = Deserialize<Dictionary<long, T>>(File.ReadAllText(name), name);
      if (data == null) continue;
      foreach (var kvp in data)
        ret[kvp.Key] = kvp.Value;
    }
    return ret;
  }
  private static bool UpdateName(string id, string name)
  {
    if (Data.TryGetValue(id, out var entry))
    {
      if (entry.name == name) return false;
      entry.name = name;
      return true;
    }
    Data[id] = new() { name = name };
    return true;
  }
  private static bool UpdateNetworkId(string character, string id)
  {
    if (Data.TryGetValue(character, out var entry))
    {
      if (entry.id == id) return false;
      entry.id = id;
      return true;
    }
    Data[character] = new() { id = id };
    return true;
  }
  public static void UpdatePeer(ZNetPeer peer)
  {
    var zm = ZDOMan.instance;
    if (zm == null) return;
    if (!ZNet.instance || !ZNet.instance.IsServer()) return;
    var updated = false;
    updated |= UpdateName("Everyone", "Everyone");
    var zdo = zm.GetZDO(peer.m_characterID);
    if (zdo == null) return;
    var id = zdo.GetLong(ZDOVars.s_playerID);
    var name = zdo.GetString(ZDOVars.s_playerName);
    if (id == 0 || name == "") return;
    updated |= UpdateName(id.ToString(), name);
    updated |= UpdateNetworkId(id.ToString(), peer.m_rpc.GetSocket().GetHostName());
    SkipReload = true;
    if (updated) ToFile();
    SkipReload = false;
  }
}

[HarmonyPatch(typeof(ZNet), nameof(ZNet.OnNewConnection))]
public class OnNewConnection
{
  static void Postfix(ZNetPeer peer)
  {
    PermissionData.UpdatePeer(peer);
  }
}

// Clients use unban to test permissions.
[HarmonyPatch(typeof(ZNet), nameof(ZNet.RPC_Unban))]
public class RPC_Unban
{
  public static string RPC_Permissions = "DEV_Permissions";

  static void Prefix(ZNet __instance, ZRpc rpc, string user)
  {
    if (user != "admintest") return;
    if (!__instance.IsServer()) return;
    var hostname = rpc.m_socket.GetHostName();
    if (!PermissionData.Data.TryGetValue(hostname, out var entry)) return;
    if (entry.features == null && entry.commands == null) return;

    var permissions = new PermissionManager(entry);
    var pkg = new ZPackage();
    permissions.Write(pkg);
    rpc.Invoke(RPC_Permissions, pkg);
  }
}