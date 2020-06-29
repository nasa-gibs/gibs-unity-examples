using System;
using System.Collections.Generic;

[Serializable]
public class SerialData
{
    // takes in data for x, y, z, and color value respectively
    public List<float> latitude = new List<float>();
    public List<float> longitude = new List<float>();
    public List<float> sphereRadius = new List<float>();
    public List<float> val = new List<float>();
}