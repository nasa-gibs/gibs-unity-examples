using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalipsoInstantiator : MonoBehaviour
{
    GeojsonData calipsoData = GeojsonParser.CalipsoReader();
    public Vector2[] startCoords;
    public Vector2[] endCoords;
    public Texture profiles;
    public DataCurtain curtain;

    // Start is called before the first frame update
    void Awake()
    {
        for (int i = 0; i < calipsoData.features.Length; i++)
        {
            profiles = Resources.Load <Texture> (calipsoData.features[i].properties.img);
            curtain.Generate(calipsoData.features[i].properties.y_range[1], calipsoData.features[i].properties.x_range[1], calipsoData.features[i].properties.y_range[2], calipsoData.features[i].properties.x_range[2], profiles);
        }
    }
}
