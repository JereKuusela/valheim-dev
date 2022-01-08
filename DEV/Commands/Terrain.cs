using System.Collections.Generic;
using Service;
using UnityEngine;


namespace DEV {

  public class TerrainCommand : BaseCommands {
    public TerrainCommand() {
      new Terminal.ConsoleCommand("terrain", "[raise/lower/reset/level/paint=value] [radius=0] [smooth=0] [blockcheck] [square] - Terrain manipulation.", delegate (Terminal.ConsoleEventArgs args) {
        if (Player.m_localPlayer == null) {
          AddMessage(args.Context, "Unable to find the player.");
          return;
        }
        var pos = Player.m_localPlayer.transform.position;
        var height = ZoneSystem.instance.GetGroundHeight(pos);
        var pars = ParseArgs(args, height);
        var heightMaps = new List<Heightmap>();
        Heightmap.FindHeightmap(pos, pars.Radius, heightMaps);
        var compilerIndices = Terrain.GetCompilerIndices(heightMaps, pos, pars.Radius, pars.Square, pars.BlockCheck);
        var before = Terrain.GetData(compilerIndices);
        if (pars.Set.HasValue)
          Terrain.SetTerrain(compilerIndices, pos, pars.Radius, pars.Smooth, pars.Set.Value);
        if (pars.Delta.HasValue)
          Terrain.RaiseTerrain(compilerIndices, pos, pars.Radius, pars.Smooth, pars.Delta.Value);
        if (pars.Level.HasValue)
          Terrain.LevelTerrain(compilerIndices, pos, pars.Radius, pars.Smooth, pars.Level.Value);
        if (pars.Paint != "") {
          if (pars.Paint == "dirt")
            Terrain.PaintTerrain(compilerIndices, pos, pars.Radius, Color.red);
          if (pars.Paint == "paved")
            Terrain.PaintTerrain(compilerIndices, pos, pars.Radius, Color.blue);
          if (pars.Paint == "cultivated")
            Terrain.PaintTerrain(compilerIndices, pos, pars.Radius, Color.green);
          if (pars.Paint == "grass")
            Terrain.PaintTerrain(compilerIndices, pos, pars.Radius, Color.black);
        }

        var after = Terrain.GetData(compilerIndices);
        UndoManager.Add(new UndoTerrain(before, after, pos, pars.Radius));

      }, true, true, optionsFetcher: () => Operations);
    }
    private static TerrainParameters ParseArgs(Terminal.ConsoleEventArgs args, float height) {
      var parameters = new TerrainParameters();
      foreach (var arg in args.Args) {
        var split = arg.Split('=');
        if (split[0] == "reset")
          parameters.Set = 0f;
        if (split[0] == "square")
          parameters.Square = true;
        if (split[0] == "blockcheck")
          parameters.BlockCheck = true;
        if (split[0] == "level")
          parameters.Level = height;
        if (split.Length < 2) continue;
        if (split[0] == "radius")
          parameters.Radius = Mathf.Min(64f, TryFloat(split[1], 0f));
        if (split[0] == "paint")
          parameters.Paint = split[1];
        if (split[0] == "raise")
          parameters.Delta = TryFloat(split[1], 0f);
        if (split[0] == "lower")
          parameters.Delta = -TryFloat(split[1], 0f);
        if (split[0] == "smooth")
          parameters.Smooth = TryFloat(split[1], 0f);
        if (split[0] == "level")
          parameters.Level = TryFloat(split[1], height);
      }
      return parameters;
    }
    private static List<string> Operations = new List<string>(){
      "raise",
      "lower",
      "level",
      "reset",
      "paint",
      "blockcheck",
      "square",
    };

  }

  public class TerrainParameters {
    public float Radius = 1f;
    public float? Set = null;
    public float? Delta = null;
    public float? Level = null;
    public float Smooth = 0;
    public string Paint = "";
    public bool Square = false;
    public bool BlockCheck = false;
  }

}
