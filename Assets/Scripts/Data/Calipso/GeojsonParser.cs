using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GeojsonParser 
{
    public GeojsonData CalipsoReader() 
    {
        string path = Application.dataPath + "Data/output/calipso.geojson";
        string jsonString = File.ReadAllText(path);
        GeojsonData[] calipsoData = GeojsonHelper.FromJson<GeojsonData>(jsonString);

        return calipsoData[5];
    }

    //hardcoded function used for demonstration purposes while GeoJson parser is being developed
    public GeojsonData TestReader()
    {
        GeojsonData calipsoData = new GeojsonData();


        return calipsoData;
    }

}
