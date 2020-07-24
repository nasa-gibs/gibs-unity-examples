using System;
using System.Collections.Generic;

[Serializable]
public class CSVData
{
    // takes in data for x, y, z, and color value respectively
    public List<float> x = new List<float>();
    public List<float> y = new List<float>();
    public List<float> z = new List<float>();
    public List<float> val = new List<float>();
}