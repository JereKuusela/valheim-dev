using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Bootstrap;
using Service;
using SoftReferenceableAssets;
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
    var baseType = typeof(MonoBehaviour);
    return [.. assemblies.SelectMany(s =>
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
    }).Distinct()];
  }
  private static Type[] GetTypes(HashSet<string> components) => components.Select(c => NameToType.TryGetValue(c.ToLowerInvariant(), out var t) ? t : throw new InvalidOperationException($"Type {c} not recognized.")).ToArray();

  private static Dictionary<string, HashSet<string>> PrefabComponents = [];
  private static void SearchComponents()
  {
    var result = new Dictionary<string, HashSet<string>>();
    foreach (var kvp in ZNetScene.instance.m_namedPrefabs)
    {
      if (!kvp.Value) continue;
      kvp.Value.GetComponentsInChildren(ZNetView.m_tempComponents);
      result[kvp.Value.name] = [.. ZNetView.m_tempComponents.Where(s => s).Select(s => s.GetType().Name.ToLowerInvariant())];
    }
    PrefabComponents = result;
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
    return [.. prefabs.Where(prefab =>
    {
      var prefabObject = ZNetScene.instance.GetPrefab(prefab);
      if (!prefabObject) return false;
      var component = prefabObject.GetComponentInChildren(type);
      if (!component) return false;
      var fieldInfo = component.GetType().GetField(field);
      if (fieldInfo == null) return false;
      var fieldValue = fieldInfo.GetValue(component);
      return fieldValue?.ToString() == value;
    })];
  }
  private static readonly Dictionary<string, HashSet<string>> locationPrefabComponents = [];
  private static bool loadingLocations;
  private static List<SoftReference<GameObject>> UnreadLocationPrefabs() => [.. ZoneSystem.instance.m_locations.Where(Helper.IsValid)
    .Select(location => location.m_prefab)
    .Where(prefab => !locationPrefabComponents.ContainsKey(prefab.Name))
    .GroupBy(prefab => prefab.Name).Select(group => group.First())];
  public static void LocationsByComponent(Terminal context, string component, Action<string[]> done)
  {
    if (loadingLocations) throw new InvalidOperationException("Locations are still loading. Try again in a moment.");
    loadingLocations = true;
    Console.instance.StartCoroutine(LoadLocations(context, component, done));
  }
  private static string[] FilterLocations(string component) => [.. ZoneSystem.instance.m_locations.Where(Helper.IsValid)
    .Select(location => location.m_prefab.Name)
    .Where(name => locationPrefabComponents.TryGetValue(name, out var components) && components.Contains(component))
    .Distinct()];
  private static System.Collections.IEnumerator LoadLocations(Terminal context, string component, Action<string[]> done)
  {
    var unread = UnreadLocationPrefabs();
    var progress = "";
    var slice = System.Diagnostics.Stopwatch.StartNew();
    try
    {
      for (var i = 0; i < unread.Count; i++)
      {
        if (i == 0 || slice.ElapsedMilliseconds >= 8)
        {
          context.m_chatBuffer.Remove(progress);
          progress = $"Reading location prefabs, {unread.Count - i}/{unread.Count} remaining, the result follows.";
          context.AddString(progress);
          yield return null;
          slice.Restart();
        }
        var prefab = unread[i];
        prefab.Load();
        prefab.Asset.GetComponentsInChildren(ZNetView.m_tempComponents);
        locationPrefabComponents[prefab.Name] = [.. ZNetView.m_tempComponents.Where(s => s).Select(s => s.GetType().Name.ToLowerInvariant())];
        prefab.Release();
      }
    }
    finally
    {
      loadingLocations = false;
      context.m_chatBuffer.Remove(progress);
    }
    done(FilterLocations(component));
  }
  public static string[] Get(ZNetView view)
  {
    view.GetComponentsInChildren(ZNetView.m_tempComponents);
    return ZNetView.m_tempComponents.Where(s => s).Select(s => s.GetType().Name).ToArray();
  }
  public static bool HasType(ZNetView view, Type[] types)
  {
    view.GetComponentsInChildren(ZNetView.m_tempComponents);
    return ZNetView.m_tempComponents.Any(s => s && types.Contains(s.GetType()));
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
