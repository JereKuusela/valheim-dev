using System;
using System.Reflection;
using BepInEx.Bootstrap;
using HarmonyLib;

namespace EWP;

public static class Api
{
  public const string GUID = "expand_world_prefabs";
  private static bool isSetup = false;

  private static MethodInfo? registerGroupHandlerMethod;

  private static void SetupIfNeeded()
  {
    if (isSetup) return;
    isSetup = true;
    if (!Chainloader.PluginInfos.TryGetValue(GUID, out var plugin)) return;
    Setup(plugin.Instance.GetType().Assembly);
  }

  private static void Setup(Assembly assembly)
  {
    if (assembly == null) return;
    var type = assembly.GetType("ExpandWorld.Prefab.Api");
    if (type == null) return;
    registerGroupHandlerMethod = AccessTools.Method(type, "RegisterGroupHandler", [typeof(string), typeof(Func<string, long, string, bool>)]);
  }

  public static void RegisterGroupHandler(string key, Func<string, long, string, bool> handler)
  {
    SetupIfNeeded();
    registerGroupHandlerMethod?.Invoke(null, [key, handler]);
  }
}