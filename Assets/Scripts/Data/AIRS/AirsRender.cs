using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Instantiates spheres to represent AIRS data points 

public class AirsRender : MonoBehaviour
{
    public GameObject gibsObject;
    public GameObject[] dataPoint;

    void Awake()
    {
        CSVData airsData = CSVParser.AirsReader();
        int dataLength = airsData.x.Count;
        dataPoint = new GameObject[dataLength];

        //Converts List<float> of AIRS coordinates into Vector3 object, instantiates data points accordingly 
        for (int i = 0; i < dataLength; i++)
        {
            dataPoint[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            dataPoint[i].transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            Renderer airsRenderer = dataPoint[i].GetComponent<Renderer>();
            float colorValue = airsData.val[i];
            
            float sphereRadius = GetComponent<SphereCollider>().radius;
            float actualRadius = 3948.8F;
            float elevationScale = .70F;

            colorPoints(airsRenderer, colorValue);
            Vector3 airsCords = Coordinates.LatLongToXYZ(airsData.x[i], airsData.y[i], airsData.z[i], actualRadius, sphereRadius, elevationScale);
            Instantiate(dataPoint[i], transform.position + airsCords, Quaternion.identity);
            //dataPoint[i].transform.parent = gibsObject.transform;
        }
    }

    void colorPoints(Renderer airsRenderer, float colorValue)
    {
        //Applies color depending on val value of the datapoint
        if (colorValue > 0.0F && colorValue < 10.0F)
            airsRenderer.material.SetColor("_Color", new Color(0,0,255)); //dark blue
        if (colorValue > 10.0F && colorValue < 20.0F)
            airsRenderer.material.SetColor("_Color", new Color(0,128,255)); //light blue
        if (colorValue > 20.0F && colorValue < 30.0F)
            airsRenderer.material.SetColor("_Color", new Color(0,255,255)); //cyan
        if (colorValue > 30.0F && colorValue < 40.0F)
            airsRenderer.material.SetColor("_Color", new Color(0,255,128)); //green 1
        if (colorValue > 40.0F && colorValue < 50.0F)
            airsRenderer.material.SetColor("_Color", new Color(128,255,0)); //green 2
        if (colorValue > 50.0F && colorValue < 60.0F)
            airsRenderer.material.SetColor("_Color", new Color(255,255,0)); //yellow
        if (colorValue > 60.0F && colorValue < 70.0F)
            airsRenderer.material.SetColor("_Color", new Color(255,128,0)); //orange
        if (colorValue > 70.0F && colorValue < 80.0F)
            airsRenderer.material.SetColor("_Color", new Color(255,0,0)); //red
        if (colorValue > 80.0F && colorValue < 90.0F)
            airsRenderer.material.SetColor("_Color", new Color(255,0,127)); //magenta
        if (colorValue > 90.0F && colorValue < 100.0F)
            airsRenderer.material.SetColor("_Color", new Color(127,0,255)); //violet
    }

}

