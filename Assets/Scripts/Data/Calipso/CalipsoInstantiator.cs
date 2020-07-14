using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalipsoInstantiator : MonoBehaviour
{
    public Vector2[] startCoords;
    public Vector2[] endCoords;
    public Texture[] profiles;
    public DataCurtain curtain;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < startCoords.Length; i++)
        {
            curtain.Generate(startCoords[i].y, startCoords[i].x, endCoords[i].y, endCoords[i].x, profiles[i]);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
