using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GeojsonParser : MonoBehaviour
{
    void Start() {
        string path = Application.dataPath + "/calipso.geojson";
        string jsonString = File.ReadAllText(path);
        //GeojsonData[] = new GeojsonData;
        GeojsonData calipsoData = JsonUtility.FromJson<GeojsonData>(jsonString);
    }

}
