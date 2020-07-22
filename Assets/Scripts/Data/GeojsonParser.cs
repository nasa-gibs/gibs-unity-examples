using EVRTH.Scripts.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GeojsonParser 
{
    public static GeojsonData Reader(string path) 
    {
        string jsonString = File.ReadAllText(path);
        GeojsonData data = JsonUtility.FromJson<GeojsonData>(jsonString);

        return data;
    }
}
