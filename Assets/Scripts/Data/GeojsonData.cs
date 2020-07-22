using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Series of serializable classes that handle reading the properties collection within geojson objects


[Serializable]
public class GeojsonData
{
    public string type { get; set; }
    public List<Feature> features { get; set; }
}

[Serializable]
public class Feature
{
    public Geometry geometry { get; set; }
    public string type { get; set; }
    public Properties properties { get; set; }
}

[Serializable]
public class Geometry
{
    public string type { get; set; }
    public List<List<double>> coordinates { get; set; }
}

[Serializable]
public class Properties
{
    public string height { get; set; }
    public List<double> x_range { get; set; }
    public string img { get; set; }
    public string start_time { get; set; }
    public string end_time { get; set; }
    public string csv { get; set; }
    public List<double> y_range { get; set; }
    public List<int> z_range { get; set; }
}


