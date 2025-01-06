using UnityEngine;
namespace ServerDevcommands;
public class MappingCommand
{
  private static bool ParseArgs(Terminal.ConsoleEventArgs args, out float x, out float z, out float radius)
  {
    x = 0;
    z = 0;
    radius = 0;
    if (args.Length < 2)
    {
      args.Context.AddString("Error: Missing coordinate X");
      return false;
    }
    if (args.Length < 3)
    {
      args.Context.AddString("Error: Missing coordinate Z");
      return false;
    }
    x = Parse.Float(args[1], float.MinValue);
    if (x == float.MinValue)
    {
      args.Context.AddString("Error: Invalid format for X coordinate.");
      return false;
    }
    z = Parse.Float(args[2], float.MinValue);
    if (z == float.MinValue)
    {
      args.Context.AddString("Error: Invalid format for Z coordinate.");
      return false;
    }
    if (args.Length > 3)
    {
      radius = Parse.Float(args[3], float.MinValue);
      if (radius == float.MinValue)
      {
        args.Context.AddString("Error: Invalid format for radius.");
        return false;
      }
    }
    return true;
  }
  private static void Register(string command)
  {
    AutoComplete.Register(command, (int index) =>
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
    Register("resetpins");
    new Terminal.ConsoleCommand("resetpins", "[x] [z] [radius=0] - Removes pins from the map at a given position with a given radius.", (args) =>
    {
      if (!ParseArgs(args, out var x, out var z, out var radius)) return;
      Vector3 position = new(x, 0, z);
      var removed = 0;
      while (Minimap.instance.RemovePin(position, radius))
        removed++;
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
