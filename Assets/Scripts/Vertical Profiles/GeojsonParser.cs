using EVRTH.Scripts.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class GeojsonParser 
{
    public static GeojsonData Reader(string path) 
    {
        GeojsonData data = new GeojsonData();

        //Reads filepath passed from an instantiator script, validates if file was read correctl
        if (File.Exists(path))
        {
            Debug.Log("Geojson File Loaded: " + path);
            string jsonString = File.ReadAllText(path);
            data = JsonUtility.FromJson<GeojsonData>(jsonString);

            //Checks to make sure that the Geojson data was deserialized correctly
            if (data.features == null)
            {
                Debug.Log("Data object is null");
            }
        }
        else
        {
            Debug.Log("Cannot find file in " + path);
        }

        return data;
    }
}
