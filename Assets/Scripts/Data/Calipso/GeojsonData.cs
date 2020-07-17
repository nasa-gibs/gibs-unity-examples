using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Series of serializable classes that handle reading the properties collection within geojson objects
// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(mysonResponse); 

public class GeojsonData
{
    public string type { get; set; }
    public Feature[] features { get; set; }
}

public class Feature
{
    public Geometry geometry { get; set; }
    public string type { get; set; }
    public Properties properties { get; set; }
}

public class Geometry
{
    public string type { get; set; }
    public float[][] coordinates { get; set; }
}

public class Properties
{
    public float[] x_range { get; set; }
    public string img { get; set; }
    public string start_time { get; set; }
    public string end_time { get; set; }
    public string csv { get; set; }
    public float[] y_range { get; set; }
    public int[] z_range { get; set; }
}

