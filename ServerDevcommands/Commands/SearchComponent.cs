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
    Helper.Command("search_component", "[object/location] [component] [log] - Prints object or location ids having the component. For objects, component,field,value also matches the field value. The flag log writes the result to the client log file instead of the console.", (args) =>
    {
      Helper.ArgsCheck(args, 2, "Missing the object/location");
      Helper.ArgsCheck(args, 3, "Missing the component");
      var values = args.Args.Skip(1).ToArray();
      var log = SearchOutput.TakeLog(ref values, 2);
      var kind = values[0].ToLowerInvariant();
      if (kind != "object" && kind != "location") throw new InvalidOperationException("The first argument must be \"object\" or \"location\".");
      if (!ZNetScene.instance || !ZoneSystem.instance) throw new InvalidOperationException("Join a world first.");
      var split = values[1].Split(',');
      if (split.Length == 2) throw new InvalidOperationException("Use \"component,field,value\".");
      if (split.Length > 3) throw new InvalidOperationException("Too many parts in the component. Use \"component,field,value\".");
      var component = split[0].ToLowerInvariant();
      var type = ComponentInfo.Types.FirstOrDefault(t => t.Name.ToLowerInvariant() == component);
      if (type == null) throw new InvalidOperationException($"Unknown component \"{split[0]}\".");
      var empty = split.Length == 3 ? null : $"No {kind} has the component \"{type.Name}\".";
      void print(string[] result) => SearchOutput.Print(args.Context, "search_component", $"\"{values[1]}\"", result, log, empty);
      if (kind == "location")
      {
        if (split.Length == 3) throw new InvalidOperationException("Locations support only the component name.");
        ComponentInfo.LocationsByComponent(args.Context, component, print);
        return;
      }
      if (split.Length == 3)
      {
        if (type.GetField(split[1]) == null) throw new InvalidOperationException($"Component {type.Name} has no field \"{split[1]}\".");
        print(ComponentInfo.PrefabsByField(component, split[1], split[2]));
      }
      else print(ComponentInfo.PrefabsByComponent(component));
    });
    AutoComplete.Register("search_component", (index, subIndex, args) =>
    {
      if (index == 0) return ["object", "location"];
      if (index == 1)
      {
        if (subIndex == 0) return ParameterInfo.Components;
        if (subIndex == 1) return ComponentFields(args.Length > 1 ? args[1].Split(',')[0] : "");
        if (subIndex == 2) return ParameterInfo.Create("Value");
        return ParameterInfo.None;
      }
      if (index == 2) return ParameterInfo.Create("log", "Write the result to the client log file instead of the console.");
      return ParameterInfo.None;
    });
  }
  private static List<string> ComponentFields(string component)
  {
    var type = ComponentInfo.Types.FirstOrDefault(t => t.Name.ToLowerInvariant() == component.ToLowerInvariant());
    if (type == null) return ParameterInfo.Create("Field");
    return [.. type.GetFields(BindingFlags.Instance | BindingFlags.Public).Where(f => f.FieldType.IsPrimitive || f.FieldType == typeof(string) || f.FieldType.IsEnum).Select(f => f.Name)];
  }
}


public class SearchItemCommand
{
  public SearchItemCommand()
  {
    Helper.Command("search_item", "[field,value] [field,value] ... [log] - Prints items having the field values. The flag log writes the result to the client log file instead of the console.", (args) =>
    {
      Helper.ArgsCheck(args, 2, "Missing the search term");
      var terms = args.Args.Skip(1).ToArray();
      var log = SearchOutput.TakeLog(ref terms, 1, true);
      if (!ObjectDB.instance) throw new InvalidOperationException("Join a world first.");
      var items = ObjectDB.instance.m_items;
      foreach (var term in terms)
      {
        var split = term.Split(',');
        if (split.Length != 2) throw new InvalidOperationException($"Invalid search term \"{term}\". Use \"field,value\".");
        if (typeof(ItemDrop.ItemData.SharedData).GetField(split[0], BindingFlags.Instance | BindingFlags.Public) == null)
          throw new InvalidOperationException($"Unknown item field \"{split[0]}\".");
        items = ItemDropsByField(items, split[0], split[1].ToLowerInvariant());
      }
      SearchOutput.Print(args.Context, "search_item", $"\"{string.Join(" ", terms)}\"", items.Select(item => item.name).ToList(), log);
    });
    List<string> itemFields = [];
    AutoComplete.Register("search_item", (index, subIndex) =>
    {
      if (itemFields.Count == 0) itemFields = GenerateAutocomplete();
      if (subIndex == 0) return [.. itemFields, "log"];
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
