using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Bootstrap;
using UnityEngine;

namespace ServerDevcommands;

public class ComponentInfo
{
  private static Type[]? types;
  public static Type[] Types => types ??= LoadTypes();
  private static Type[] LoadTypes()
  {
    List<Assembly> assemblies = [Assembly.GetAssembly(typeof(ZNetView)), .. Chainloader.PluginInfos.Values.Where(p => p.Instance != null).Select(p => p.Instance.GetType().Assembly)];
    var assembly = Assembly.GetAssembly(typeof(ZNetView));
    var baseType = typeof(MonoBehaviour);
    return assemblies.SelectMany(s =>
    {
      try
      {
        return s.GetTypes();
      }
      catch (ReflectionTypeLoadException e)
      {
        return e.Types.Where(t => t != null);
      }
    }).Where(t =>
    {
      try
      {
        return baseType.IsAssignableFrom(t);
      }
      catch
      {
        return false;
      }
    }).ToArray();
  }
  private static Type[] GetTypes(HashSet<string> components) => Types.Where(t => components.Contains(t.Name.ToLowerInvariant())).ToArray();

  private static Dictionary<string, HashSet<Type>> PrefabComponents = [];
  private static void SearchComponents()
  {
    PrefabComponents = ZNetScene.instance.m_namedPrefabs.ToDictionary(
      kvp => kvp.Value.name,
      kvp =>
      {
        kvp.Value.GetComponentsInChildren<MonoBehaviour>(ZNetView.m_tempComponents);
        return ZNetView.m_tempComponents.Select(s => s.GetType()).ToHashSet();
      }
    );
  }
  public static string[] PrefabsByComponent(string component)
  {
    if (PrefabComponents.Count == 0) SearchComponents();
    return PrefabComponents.Where(kvp => kvp.Value.Any(type => type.Name.ToLowerInvariant() == component)).Select(kvp => kvp.Key).ToArray();
  }
  public static string[] Get(ZNetView view)
  {
    view.GetComponentsInChildren<MonoBehaviour>(ZNetView.m_tempComponents);
    return ZNetView.m_tempComponents.Select(s => s.GetType().Name).ToArray();
  }
  public static bool HasType(ZNetView view, Type[] types)
  {
    view.GetComponentsInChildren<MonoBehaviour>(ZNetView.m_tempComponents);
    return ZNetView.m_tempComponents.Any(s => types.Contains(s.GetType()));
  }
  public static IEnumerable<ZNetView> HaveComponent(IEnumerable<ZNetView> views, HashSet<string> components)
  {
    var types = GetTypes(components);
    return views.Where(view => HasType(view, types));
  }
}
