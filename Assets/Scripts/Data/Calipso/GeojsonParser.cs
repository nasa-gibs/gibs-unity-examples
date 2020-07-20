using EVRTH.Scripts.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GeojsonParser 
{
    public static GeojsonData CalipsoReader() 
    {
        string path = Application.dataPath + "/Data/output/calipso.geojson";
        string jsonString = File.ReadAllText(path);
        GeojsonData calipsoData = JsonUtility.FromJson<GeojsonData>(jsonString);

        return calipsoData;
    }
}
