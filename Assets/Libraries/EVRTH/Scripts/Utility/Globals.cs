using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;



#region Enums
public enum DownloadStatus
{
    InProgress,
    Error
}


internal enum LoadStatus
{
    NotStarted,
    InProgress,
    Complete
}


public enum LayerStatus
{
    Loading,       // Waiting for root tiles to load.
    Transitioning, // Fading between different sets of tiles.
    Complete       // Root tiles are loaded. Child tiles may still be streaming in.
}

public enum SimulationMode
{
    Undefined,
    Cupola,
    Workspace
}

#endregion

#region Structs

internal struct GlobeTileLayerInfo
{
    public string name;
    public DateTime date;
    public LoadStatus status;
}

internal struct Pair<T>
{
    public T item1;
    public T item2;

    public Pair(T item1, T item2)
    {
        this.item1 = item1;
        this.item2 = item2;
    }
}

[Serializable]
public struct Wmts
{
    public int row;
    public int col;
    public int zoom;

    public override string ToString()
    {
        return "Row: " + row + "  Col: " + col + "  Zoom: " + zoom;
    }
}

public struct DataRange
{

    public float min;
    public float max;
    public bool transparent;

}

public struct VertexAndUvContainer
{
    public List<int> vertexIndices;
    public List<Vector2> uvs;
}

#endregion


#region Delegates

public delegate void TextureDownloadHandler(string layer, DateTime date, Texture texture); 

#endregion

public class Methodtimer : IDisposable
{
    private Stopwatch sw;
    private readonly string msg;
    public Methodtimer(string message)
    {
        msg = message;
        sw = new Stopwatch();
        sw.Start();
    }
    public void Dispose()
    {
        sw.Stop();
        UnityEngine.Debug.Log(msg + " " + sw.Elapsed);
        sw = null;
    }
}


