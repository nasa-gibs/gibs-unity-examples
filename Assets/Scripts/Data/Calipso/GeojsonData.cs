using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Series of serializable classes that handle reading the properties collection within geojson objects

[Serializable]
public class GeojsonData
{
    public string type;
    public Properties[] properties;
}

[Serializable]
public class Properties
{
    public float[] x_range;
    public string img;
    public string start_time;
    public string end_time;
    public string csv;
    public float[] y_range;
    public float[] z_range;
}

[Serializable]
public class GeojsonDataCollection
{
    public GeojsonData[] collection;
}

/*float[] startLat;
float[] startLong;
float[] endLat;
float[] endLong;
Texture[] profileImg;*/
