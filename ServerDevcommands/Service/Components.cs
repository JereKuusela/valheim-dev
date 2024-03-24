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
  private static Dictionary<string, Type>? nameToType;

  private static Dictionary<string, Type> LoadNames()
  {
    var dict = new Dictionary<string, Type>();
    foreach (var type in Types)
    {
      dict[type.Name.ToLowerInvariant()] = type;
    }
    return dict;
  }
  private static Dictionary<string, Type> NameToType => nameToType ??= LoadNames();
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
    }).Distinct().ToArray();
  }
  private static Type[] GetTypes(HashSet<string> components) => components.Select(c => NameToType.TryGetValue(c.ToLowerInvariant(), out var t) ? t : throw new InvalidOperationException($"Type {c} not recognized.")).ToArray();

  private static Dictionary<string, HashSet<string>> PrefabComponents = [];
  private static void SearchComponents()
  {
    PrefabComponents = ZNetScene.instance.m_namedPrefabs.ToDictionary(
      kvp => kvp.Value.name,
      kvp =>
      {
        kvp.Value.GetComponentsInChildren(ZNetView.m_tempComponents);
        return ZNetView.m_tempComponents.Select(s => s.GetType().Name.ToLowerInvariant()).ToHashSet();
      }
    );
  }
  public static string[] PrefabsByComponent(string component)
  {
    if (PrefabComponents.Count == 0) SearchComponents();
    var lower = component.ToLowerInvariant();
    return PrefabComponents.Where(kvp => kvp.Value.Contains(lower)).Select(kvp => kvp.Key).ToArray();
  }
  public static string[] PrefabsByField(string component, string field, string value)
  {
    var prefabs = PrefabsByComponent(component);
    var type = Types.FirstOrDefault(t => t.Name.ToLowerInvariant() == component);
    if (type == null) return [];
    return prefabs.Where(prefab =>
    {
      var component = ZNetScene.instance.GetPrefab(prefab).GetComponentInChildren(type);
      if (!component) return false;
      var fieldInfo = component.GetType().GetField(field);
      if (fieldInfo == null) return false;
      var fieldValue = fieldInfo.GetValue(component);
      return fieldValue.ToString() == value;
    }).ToArray();
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
  public static bool HasComponent(ZNetView view, HashSet<string> components)
  {
    var types = GetTypes(components);
    return HasType(view, types);
  }
}
