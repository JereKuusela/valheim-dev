using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using HarmonyLib;
namespace ServerDevcommands;

[HarmonyPatch]
public class BindManager
{
  public static string FileName = "binds.yaml";
  public static string FilePath = Path.Combine(Paths.ConfigPath, FileName);

  public static string FromData(BindData data)
  {
    if (data.tag != "") return "";
    if (data.key == "wheel") data.key = Settings.MouseWheelBindKey.ToString().ToLower();
    var modifiers = data.modifiers != "" ? $" keys={data.modifiers.Replace(" ", "")}" : "";
    return $"{data.key}{modifiers} {data.command}";
  }
  public static BindData ToData(string command)
  {
    var args = command.Split(' ');
    BindData data = new()
    {
      key = args[0]
    };
    if (data.key.ToLower() == Settings.MouseWheelBindKey.ToString().ToLower())
      data.key = "wheel";
    foreach (var arg in args)
    {
      var split = arg.Split('=');
      if (split.Length == 1) continue;
      if (split[0] == "keys")
        data.modifiers = split[1];
    }
    var cmd = args.Skip(1).Where(static arg => !arg.StartsWith("keys=", StringComparison.OrdinalIgnoreCase));
    data.command = string.Join(" ", cmd);
    return data;
  }

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
    var data = Terminal.m_bindList.Select(ToData).ToArray();
    if (data.Length == 0)
    {
      if (File.Exists(FilePath)) File.Delete(FilePath);
      return;
    }
    var yaml = Yaml.Serializer().Serialize(data);
    File.WriteAllText(FilePath, yaml);
  }
  public static void FromFile()
  {
    try
    {
      var data = Yaml.Read(FilePath, Yaml.Deserialize<List<BindData>>);
      Terminal.m_bindList = data.Select(FromData).Where(static s => s != "").ToList();
      ServerDevcommands.Log.LogInfo($"Reloading {Terminal.m_bindList.Count} bind data.");
      Terminal.updateBinds();
    }
    catch (Exception e)
    {
      ServerDevcommands.Log.LogError(e.StackTrace);
    }
  }
  public static void SetupWatcher()
  {
    Yaml.SetupWatcher(FileName, FromFile);
  }
}
