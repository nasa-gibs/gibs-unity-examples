using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalipsoInstantiator : MonoBehaviour
{

    public DataCurtain curtain;
    public Texture[] profiles;

    // Start is called before the first frame update
    void Awake()
    {
        string path = Application.dataPath + "/Resources/Data/output/calipso.geojson";
        GeojsonData calipsoData = GeojsonParser.Reader(path);

        for (int i = 0; i < calipsoData.features.Count; i++)
        {
            //profiles = Resources.Load<Texture>("/Data/" + calipsoData.features[i].properties.img);
            curtain.Generate(calipsoData.features[i].properties.y_range[0], calipsoData.features[i].properties.x_range[0], calipsoData.features[i].properties.y_range[1], calipsoData.features[i].properties.x_range[1], profiles[i]);
        }
    }
}
