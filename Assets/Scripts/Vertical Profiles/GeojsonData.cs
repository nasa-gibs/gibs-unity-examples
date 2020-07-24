using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Series of serializable classes that handle reading the properties collection within geojson objects

[Serializable]
public class GeojsonData
{
    public string type;
    public List<Feature> features;
}

[Serializable]
public class Feature
{
    public Geometry geometry;
    public string type;
    public Properties properties;
}

[Serializable]
public class Geometry
{
    public string type;
    public List<List<float>> coordinates;
}

[Serializable]
public class Properties
{
    public List<float> x_range;
    public string img;
    public string start_time;
    public string end_time;
    public string csv;
    public List<float> y_range;
    public List<int> z_range;
}


