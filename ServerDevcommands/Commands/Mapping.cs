using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace ServerDevcommands;
public class MappingCommand
{
  private static bool ParseArgs(Terminal.ConsoleEventArgs args, out float x, out float z, out float radius, int offset = 0)
  {
    x = 0;
    z = 0;
    radius = 0;
    if (args.Length < 2 + offset)
    {
      args.Context.AddString("Error: Missing coordinate X");
      return false;
    }
    if (args.Length < 3 + offset)
    {
      args.Context.AddString("Error: Missing coordinate Z");
      return false;
    }
    x = Parse.Float(args[1 + offset], float.MinValue);
    if (x == float.MinValue)
    {
      args.Context.AddString("Error: Invalid format for X coordinate.");
      return false;
    }
    z = Parse.Float(args[2 + offset], float.MinValue);
    if (z == float.MinValue)
    {
      args.Context.AddString("Error: Invalid format for Z coordinate.");
      return false;
    }
    if (args.Length > 3 + offset)
    {
      radius = Parse.Float(args[3 + offset], float.MinValue);
      if (radius == float.MinValue)
      {
        args.Context.AddString("Error: Invalid format for radius.");
        return false;
      }
    }
    return true;
  }
  private static bool TryParsePinTypes(string arg, out List<Minimap.PinType> types)
  {
    types = [];
    foreach (var name in Parse.Split(arg))
    {
      if (Parse.TryFloat(name, out _) || !Enum.TryParse(name, true, out Minimap.PinType type)) return false;
      types.Add(type);
    }
    return types.Count > 0;
  }
  private static void Register(string command)
  {
    AutoComplete.Register(command, index =>
    {
      if (index == 0) return ParameterInfo.Create("X coordinate.");
      if (index == 1) return ParameterInfo.Create("Z coordinate.");
      if (index == 2) return ParameterInfo.Create("Radius (default 0).");
      return ParameterInfo.None;
    });
  }
  ///<summary>Explores/unexplores a circle. Copypaste from the base game code</summary>
  private static void ExploreRadius(Terminal terminal, Vector3 p, float radius, bool explore)
  {
    var minimap = Minimap.instance;
    var limit = (int)Mathf.Ceil(radius / minimap.m_pixelSize);
    minimap.WorldToPixel(p, out var x, out var y);
    var explored = 0;
    for (var i = y - limit; i <= y + limit; i++)
    {
      if (i < 0 || i >= minimap.m_textureSize) continue;
      for (var j = x - limit; j <= x + limit; j++)
      {
        if (j < 0 || j >= minimap.m_textureSize) continue;
        if (new Vector2(j - x, i - y).magnitude > limit) continue;
        if (ExploreSpot(j, i, explore)) explored++;
      }
    }
    if (explored > 0) minimap.m_fogTexture.Apply();
    Helper.AddMessage(terminal, explored + " spots " + (explore ? "explored" : "unexplored") + ".");
  }

  ///<summary>Explores/unexplores a spot. Copypaste from the base game code.</summary>
  private static bool ExploreSpot(int x, int y, bool explore)
  {
    var minimap = Minimap.instance;
    if (explore == minimap.m_explored[y * minimap.m_textureSize + x]) return false;
    Color pixel = minimap.m_fogTexture.GetPixel(x, y);
    pixel.r = explore ? 0 : byte.MaxValue;
    minimap.m_fogTexture.SetPixel(x, y, pixel);
    minimap.m_explored[y * minimap.m_textureSize + x] = explore;
    return true;
  }
  public MappingCommand()
  {
    AutoComplete.Register("resetpins", (index, subIndex, args) =>
    {
      if (index == 0) return [.. Enum.GetNames(typeof(Minimap.PinType))];
      if (args.Length > 0 && TryParsePinTypes(args[0], out _)) index--;
      if (index == 0) return ParameterInfo.Create("X coordinate.");
      if (index == 1) return ParameterInfo.Create("Z coordinate.");
      if (index == 2) return ParameterInfo.Create("Radius (default 0).");
      return ParameterInfo.None;
    });
    new Terminal.ConsoleCommand("resetpins", "[types] [x] [z] [radius=0] - Removes pins from the map. Needs the types (comma separated), the position or both.", (args) =>
    {
      if (args.Length < 2)
      {
        args.Context.AddString("Error: Missing the types or the position.");
        return;
      }
      var types = new List<Minimap.PinType>();
      var offset = TryParsePinTypes(args[1], out types) ? 1 : 0;
      var filterPosition = args.Length > 1 + offset;
      float x = 0, z = 0, radius = 0;
      if (filterPosition && !ParseArgs(args, out x, out z, out radius, offset)) return;
      Vector3 position = new(x, 0, z);
      var removed = 0;
      foreach (var pin in Minimap.instance.m_pins.ToList())
      {
        if (!pin.m_save) continue;
        if (offset > 0 && !types.Contains(pin.m_type)) continue;
        if (filterPosition && Utils.DistanceXZ(position, pin.m_pos) > radius) continue;
        Minimap.instance.RemovePin(pin);
        removed++;
      }
      Helper.AddMessage(args.Context, removed + " pins removed.");
    });
    Register("exploremap");
    new Terminal.ConsoleCommand("exploremap", "[x] [z] [radius=0] - Reveals part of the map. Without parameters, reveals the whole map.", (args) =>
    {
      if (args.Length == 1)
      {
        Minimap.instance.ExploreAll();
        return;
      }
      if (!ParseArgs(args, out var x, out var z, out var radius)) return;
      Vector3 position = new(x, 0, z);
      ExploreRadius(args.Context, position, radius, true);
    }, isCheat: true);
    Register("resetmap");
    new Terminal.ConsoleCommand("resetmap", "[x] [z] [radius=0] - Hides part of the map. Without parameters, hides the whole map.", (args) =>
    {
      if (args.Length == 1)
      {
        Minimap.instance.Reset();
        return;
      }
      if (!ParseArgs(args, out var x, out var z, out var radius)) return;
      Vector3 position = new(x, 0, z);
      ExploreRadius(args.Context, position, radius, false);
    });
  }
}
