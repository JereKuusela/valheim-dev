using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
namespace ServerDevcommands;
public class SearchComponentCommand
{
  public SearchComponentCommand()
  {
    Helper.Command("search_component", "[object/location] [component] [max_lines=5] - Prints object or location ids having the component.", (args) =>
    {
      Helper.ArgsCheck(args, 2, "Missing the object/location");
      Helper.ArgsCheck(args, 3, "Missing the component");
      var component = args[2].ToLowerInvariant();
      var field = "";
      var value = "";
      var split = component.Split(',');
      if (split.Length == 3)
      {
        component = split[0];
        field = split[1];
        value = split[2];
      }
      var maxLines = args.TryParameterInt(2, 5);
      var locations = ZoneSystem.instance.m_locations.Where(Helper.IsValid).Where(l =>
      {
        l.m_prefab.Load();
        l.m_prefab.Asset.GetComponentsInChildren(ZNetView.m_tempComponents);
        l.m_prefab.Release();
        return ZNetView.m_tempComponents.Any(s => s.GetType().Name.ToLowerInvariant() == component);
      }).Select(l => l.m_prefab.Name);
      var result = args[1] == "location" ? locations.ToArray() : field == "" ? ComponentInfo.PrefabsByComponent(component) : ComponentInfo.PrefabsByField(component, field, value);
      if (result.Length > 100)
      {
        args.Context.AddString("Over 100 results, printing to the log file.");
        ZLog.Log("\n" + string.Join("\n", result));
        return;
      }
      var bufferSize = (int)Math.Ceiling((float)result.Length / maxLines);
      var buffer = new string[bufferSize];
      for (int i = 0; i < result.Length; i += bufferSize)
      {
        if (result.Length - i < bufferSize)
        {
          bufferSize = result.Length - i;
          buffer = new string[bufferSize];
        }
        Array.Copy(result, i, buffer, 0, bufferSize);
        args.Context.AddString(string.Join(", ", buffer));
      }
    });
    AutoComplete.Register("search_component", index =>
    {
      if (index == 0) return ["object", "location"];
      if (index == 1) return ParameterInfo.Create("Search component");
      if (index == 2) return ParameterInfo.Create("Max lines", "number (default 5)");
      return ParameterInfo.None;
    });

  }
}


public class SearchItemCommand
{
  public SearchItemCommand()
  {
    Helper.Command("search_item", "[field,value] [field,value] ...  - Searches items.", (args) =>
    {
      Helper.ArgsCheck(args, 2, "Missing the search term");
      var items = ObjectDB.instance.m_items;
      for (int i = 1; i < args.Length; i++)
      {
        var split = args[i].Split(',');
        if (split.Length != 2)
        {
          args.Context.AddString("Invalid search term: " + args[i]);
          continue;
        }
        var field = split[0];
        var value = split[1].ToLowerInvariant();
        items = ItemDropsByField(items, field, value);
      }
      var names = items.Select(item => item.name).ToList();
      if (names.Count > 50)
      {
        args.Context.AddString("Over 50 results, printing to the log file.");
        Debug.Log("Search item results:\n" + string.Join("\n", names));
        return;
      }
      foreach (var item in names) args.Context.AddString(item);
    });
    List<string> itemFields = [];
    AutoComplete.Register("search_item", (index, subIndex) =>
    {
      if (itemFields.Count == 0) itemFields = GenerateAutocomplete();
      if (subIndex == 0) return itemFields;
      if (subIndex == 1) return ParameterInfo.Create("Value");
      return ParameterInfo.None;
    });

  }
  readonly static HashSet<Type> validTypes = [typeof(bool), typeof(int), typeof(float), typeof(string)];
  private static List<string> GenerateAutocomplete() => typeof(ItemDrop.ItemData.SharedData).GetFields(BindingFlags.Instance | BindingFlags.Public).Where(f => validTypes.Contains(f.FieldType) || f.FieldType.IsEnum).Select(p => p.Name).ToList();
  private static List<GameObject> ItemDropsByField(List<GameObject> items, string field, string value) => items.Where(item =>
  {
    var drop = item.GetComponent<ItemDrop>();
    if (drop == null) return false;
    var data = drop.m_itemData;
    if (data == null) return false;
    var shared = data.m_shared;
    if (shared == null) return false;
    var f = typeof(ItemDrop.ItemData.SharedData).GetField(field, BindingFlags.Instance | BindingFlags.Public);
    if (f == null) return false;
    var v = f.GetValue(shared);
    return v.ToString().ToLowerInvariant() == value;
  }).ToList();
}
