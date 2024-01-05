using System;
using System.Linq;
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
      var maxLines = args.TryParameterInt(2, 5);
      var locations = ZoneSystem.instance.m_locations.Where(location =>
      {
        if (!location.m_prefab) return false;
        location.m_prefab.GetComponentsInChildren<MonoBehaviour>(ZNetView.m_tempComponents);
        return ZNetView.m_tempComponents.Any(s => s.GetType().Name.ToLowerInvariant() == component);
      }).Select(location => location.m_prefab.name);
      var result = args[1] == "location" ? locations.ToArray() : ComponentInfo.PrefabsByComponent(component);
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
    AutoComplete.Register("search_component", (int index) =>
    {
      if (index == 0) return ["object", "location"];
      if (index == 1) return ParameterInfo.Create("Search component");
      if (index == 2) return ParameterInfo.Create("Max lines", "number (default 5)");
      return ParameterInfo.None;
    });
  }
}
