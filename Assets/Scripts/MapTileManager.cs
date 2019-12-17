using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapTileManager : MonoBehaviour {
    private Renderer[] tilesRenderer;
    private float percentLoaded = 0;

	void Awake () {
        tilesRenderer = GetComponentsInChildren<Renderer>();
	}

    public bool CheckTilesLoaded(){
        int tilesNotLoaded = 0;
        foreach (Renderer tileRenderer in tilesRenderer)
        {
            if (tileRenderer.gameObject.transform.childCount == 0)
            {
                if (!tileRenderer.enabled)
                {
                    tilesNotLoaded++;
                }
            }
        }
        if (tilesNotLoaded == 0){
            return true;
        }
        else{
            percentLoaded = 1f - ((float)tilesNotLoaded / (float)tilesRenderer.Length);
            return false;
        }
    }

    public float PercentTilesLoaded(){
        return Mathf.Round(percentLoaded * 100);
    }

    public void SaveTextureAllTiles(){
        foreach (Renderer tileRenderer in tilesRenderer){
            tileRenderer.gameObject.GetComponent<MapTile>().SaveTexture();
        }
    }

    public void SetABToggleAllTiles(bool toggle)
    {
        foreach (Renderer tileRenderer in tilesRenderer)
        {
            tileRenderer.gameObject.GetComponent<MapTile>().SetABToggle(toggle);
        }
    }

    public void SetABPositionAllTiles(Vector3 abPosition)
    {
        foreach (Renderer tileRenderer in tilesRenderer)
        {
            tileRenderer.gameObject.GetComponent<MapTile>().SetABPosition(abPosition);
        }
    }

    public void SetABRadiusAllTiles(float radius)
    {
        foreach (Renderer tileRenderer in tilesRenderer)
        {
            tileRenderer.gameObject.GetComponent<MapTile>().SetABRadius(radius);
        }
    }
}
