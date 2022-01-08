using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DEV {
  using CompilerIndices = Dictionary<TerrainComp, Indices>;
  public class HeightIndex {
    public int Index;
    public float Distance;
  }
  public class Indices {
    public IEnumerable<HeightIndex> HeightIndices;
    public IEnumerable<int> PaintIndices;
  }

  public static class Terrain {
    public static void Save(TerrainComp compiler) {
      compiler.GetComponent<ZNetView>()?.ClaimOwnership();
      compiler.m_operations++;
      // These are only used to remove grass which isn't really needed.
      compiler.m_lastOpPoint = Vector3.zero;
      compiler.m_lastOpRadius = 0f;
      compiler.Save();
      compiler.m_hmap.Poke(false);
    }
    public static CompilerIndices GetCompilerIndices(List<Heightmap> heightMaps, Vector3 centerPos, float radius, bool square, bool checkBlock) {
      return heightMaps.Select(hmap => hmap.GetAndCreateTerrainCompiler()).ToDictionary(comp => comp, comp => {
        return new Indices() {
          HeightIndices = GetHeightIndices(comp, centerPos, radius, square, checkBlock).ToArray(),
          PaintIndices = GetPaintIndices(comp, centerPos, radius, square, checkBlock).ToArray()
        };
      }).Where(kvp => kvp.Value.HeightIndices.Count() + kvp.Value.PaintIndices.Count() > 0).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }
    public static void SetTerrain(CompilerIndices compilerIndices, Vector3 pos, float radius, float smooth, float amount) {
      Action<TerrainComp, int, float> action = (compiler, index, distance) => {
        var multipier = CalculateSmooth(smooth, distance);
        compiler.m_levelDelta[index] = amount * multipier;
        compiler.m_smoothDelta[index] = 0f;
        compiler.m_modifiedHeight[index] = compiler.m_levelDelta[index] != 0f;
      };
      DoHeightOperation(compilerIndices, pos, radius, action);
    }
    private static float CalculateSmooth(float smooth, float distance) => (1f - distance) >= smooth ? 1f : (1f - distance) / smooth;
    public static void RaiseTerrain(CompilerIndices compilerIndices, Vector3 pos, float radius, float smooth, float amount) {
      Action<TerrainComp, int, float> action = (compiler, index, distance) => {
        var multipier = CalculateSmooth(smooth, distance);
        compiler.m_levelDelta[index] += multipier * amount + compiler.m_smoothDelta[index];
        compiler.m_smoothDelta[index] = 0f;
        compiler.m_modifiedHeight[index] = compiler.m_levelDelta[index] != 0f;
      };
      DoHeightOperation(compilerIndices, pos, radius, action);
    }
    public static void LevelTerrain(CompilerIndices compilerIndices, Vector3 pos, float radius, float smooth, float height) {
      Action<TerrainComp, int, float> action = (compiler, index, distance) => {
        var multipier = CalculateSmooth(smooth, distance);
        compiler.m_levelDelta[index] += multipier * (height - compiler.m_hmap.m_heights[index]);
        compiler.m_smoothDelta[index] = 0f;
        compiler.m_modifiedHeight[index] = compiler.m_levelDelta[index] != 0f;
      };
      DoHeightOperation(compilerIndices, pos, radius, action);
    }
    public static void PaintTerrain(CompilerIndices compilerIndices, Vector3 pos, float radius, Color color) {
      Action<TerrainComp, int> action = (compiler, index) => {
        compiler.m_paintMask[index] = color;
        compiler.m_modifiedPaint[index] = true;
      };
      DoPaintOperation(compilerIndices, pos, radius, action);
    }

    ///<summary>Returns terrain data of given indices</summary>
    public static Dictionary<Vector3, TerrainUndoData> GetData(CompilerIndices compilerIndices) {
      return compilerIndices.ToDictionary(kvp => kvp.Key.transform.position, kvp => {
        return new TerrainUndoData() {
          Heights = kvp.Value.HeightIndices.Select(heightIndex => new HeightUndoData() {
            Index = heightIndex.Index,
            HeightModified = kvp.Key.m_modifiedHeight[heightIndex.Index],
            Level = kvp.Key.m_levelDelta[heightIndex.Index],
            Smooth = kvp.Key.m_smoothDelta[heightIndex.Index]
          }).ToArray(),
          Paints = kvp.Value.PaintIndices.Select(index => new PaintUndoData() {
            Index = index,
            PaintModified = kvp.Key.m_modifiedPaint[index],
            Paint = kvp.Key.m_paintMask[index],
          }).ToArray(),
        };
      });
    }

    private static Vector3 VertexToWorld(Heightmap hmap, int x, int y) {
      var vector = hmap.transform.position;
      vector.x += (x - hmap.m_width / 2 + 0.5f) * hmap.m_scale;
      vector.z += (y - hmap.m_width / 2 + 0.5f) * hmap.m_scale;
      return vector;
    }

    public static void ApplyData(Dictionary<Vector3, TerrainUndoData> data, Vector3 pos, float radius) {
      foreach (var kvp in data) {
        var compiler = TerrainComp.FindTerrainCompiler(kvp.Key);
        if (!compiler) continue;
        foreach (var value in kvp.Value.Heights) {
          compiler.m_smoothDelta[value.Index] = value.Smooth;
          compiler.m_levelDelta[value.Index] = value.Level;
          compiler.m_modifiedHeight[value.Index] = value.HeightModified;
        }
        foreach (var value in kvp.Value.Paints) {
          compiler.m_modifiedPaint[value.Index] = value.PaintModified;
          compiler.m_paintMask[value.Index] = value.Paint;
        }
        Save(compiler);
      }
      ClutterSystem.instance?.ResetGrass(pos, radius);
    }
    private static void DoHeightOperation(CompilerIndices compilerIndices, Vector3 pos, float radius, Action<TerrainComp, int, float> action) {
      foreach (var kvp in compilerIndices) {
        var compiler = kvp.Key;
        var indices = kvp.Value.HeightIndices;
        foreach (var heightIndex in indices) action(compiler, heightIndex.Index, heightIndex.Distance / radius);
        Save(compiler);
      }
      ClutterSystem.instance?.ResetGrass(pos, radius);
    }
    private static void DoPaintOperation(CompilerIndices compilerIndices, Vector3 pos, float radius, Action<TerrainComp, int> action) {
      foreach (var kvp in compilerIndices) {
        var compiler = kvp.Key;
        var indices = kvp.Value.PaintIndices;
        foreach (var index in indices) action(compiler, index);
        Save(compiler);
      }
      ClutterSystem.instance?.ResetGrass(pos, radius);
    }
    private static IEnumerable<HeightIndex> GetHeightIndices(TerrainComp compiler, Vector3 centerPos, float radius, bool square, bool checkBlock) {
      var indices = new List<HeightIndex>();
      compiler.m_hmap.WorldToVertex(centerPos, out var x, out var y);
      var maxDistance = radius / compiler.m_hmap.m_scale;
      var delta = Mathf.CeilToInt(maxDistance);
      var max = compiler.m_width + 1;
      var center = new Vector2((float)x, (float)y);
      for (int i = y - delta; i <= y + delta; i++) {
        if (i < 0 || i >= max) continue;
        for (int j = x - delta; j <= x + delta; j++) {
          if (j < 0 || j >= max) continue;
          var distance = square ? Math.Max(Math.Abs(j - x), Math.Abs(i - y)) : Vector2.Distance(center, new Vector2((float)j, (float)i));
          if (distance > maxDistance) continue;
          if (checkBlock) {
            var pos = VertexToWorld(compiler.m_hmap, j, i);
            if (ZoneSystem.instance.IsBlocked(pos)) continue;
          }
          indices.Add(new HeightIndex() {
            Index = i * max + j,
            Distance = distance * compiler.m_hmap.m_scale
          });
        }
      }
      return indices;
    }

    private static IEnumerable<int> GetPaintIndices(TerrainComp compiler, Vector3 centerPos, float radius, bool square, bool checkBlock) {
      centerPos = new Vector3(centerPos.x - 0.5f, centerPos.y, centerPos.z - 0.5f);
      var indices = new List<int>();
      compiler.m_hmap.WorldToVertex(centerPos, out var x, out var y);
      var maxDistance = radius / compiler.m_hmap.m_scale;
      var delta = Mathf.CeilToInt(maxDistance);
      var max = compiler.m_width;
      var center = new Vector2((float)x, (float)y);
      for (int i = y - delta; i <= y + delta; i++) {
        if (i < 0 || i >= max) continue;
        for (int j = x - delta; j <= x + delta; j++) {
          if (j < 0 || j >= max) continue;
          if (!square) {
            var distance = Vector2.Distance(center, new Vector2((float)j, (float)i));
            if (distance > maxDistance) continue;
          }
          if (checkBlock) {
            var pos = VertexToWorld(compiler.m_hmap, j, i);
            if (ZoneSystem.instance.IsBlocked(pos)) continue;
          }
          indices.Add(i * max + j);
        }
      }
      return indices;
    }
  }
}