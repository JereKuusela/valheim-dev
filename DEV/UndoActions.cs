using System.Collections.Generic;
using Service;
using UnityEngine;


namespace DEV {
  public class HeightUndoData {
    public float Smooth = 0f;
    public float Level = 0f;
    public int Index = -1;
    public bool HeightModified = false;

  }

  public class PaintUndoData {
    public bool PaintModified = false;
    public Color Paint = Color.black;
    public int Index = -1;
  }
  public class TerrainUndoData {
    public HeightUndoData[] Heights;
    public PaintUndoData[] Paints;
  }

  public class UndoTerrain : UndoAction {

    private Dictionary<Vector3, TerrainUndoData> Before = null;
    private Dictionary<Vector3, TerrainUndoData> After = null;
    public Vector3 Position;
    public float Radius;
    public UndoTerrain(Dictionary<Vector3, TerrainUndoData> before, Dictionary<Vector3, TerrainUndoData> after, Vector3 position, float radius) {
      Before = before;
      After = after;
      Position = position;
      Radius = radius;
    }
    public void Undo() {
      Terrain.ApplyData(Before, Position, Radius);
    }

    public void Redo() {
      Terrain.ApplyData(After, Position, Radius);
    }
  }
}