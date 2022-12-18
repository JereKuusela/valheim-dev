using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using HarmonyLib;
namespace ServerDevcommands;

[HarmonyPatch]
public class AliasManager
{
  public static string FileName = "alias.yaml";
  public static string FilePath = Path.Combine(Paths.ConfigPath, FileName);

  [HarmonyPatch(typeof(Chat), nameof(Chat.Awake)), HarmonyPostfix]
  public static void ChatAwake()
  {
    if (File.Exists(FilePath))
      FromFile();
    else
      ToFile();
  }
  public static bool ToBeSaved = false;
  public static void ToFile()
  {
    ToBeSaved = false;
    var data = Settings.AliasKeys.ToDictionary(key => key, key => Settings.GetAliasValue(key));
    if (data.Count == 0)
    {
      if (File.Exists(FilePath)) File.Delete(FilePath);
      return;
    }
    var yaml = Data.Serializer().Serialize(data);
    File.WriteAllText(FilePath, yaml);
  }
  public static void FromFile()
  {
    try
    {
      var data = Data.Read(FilePath, Data.Deserialize<Dictionary<string, string>>);
      Settings.AddAlias(data);
      ServerDevcommands.Log.LogInfo($"Reloading {data.Count} alias data.");
    }
    catch (Exception e)
    {
      ServerDevcommands.Log.LogError(e.StackTrace);
    }
  }
  public static void SetupWatcher()
  {
    Data.SetupWatcher(FileName, FromFile);
  }
}
