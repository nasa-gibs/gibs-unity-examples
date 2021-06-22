using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EVRTH.Scripts.Visualization;

public class MapTile : MonoBehaviour {
    //TODO: Rip AB comparison out of this class, currently easiest way to access renderer

    public float offsetX = 0f, offsetY = 0f, scaleX = 1f, scaleY = 1f;

    private Renderer mapRenderer;
    private GameObject mirroredTile, globe;
    private Renderer mirroredRenderer;
    public bool rendererUpdated = false;
    private string[] textureNames = {"_NewTex", "_Overlay1", "_Overlay2", "_OldTex", "_OldOverlay1", "_OldOverlay2"};
    private bool init = false;

    void Awake () {
        mapRenderer = GetComponent<Renderer>();
        globe = GameObject.Find("WMTSBall - VIIRS CorrectedReflectance");
	}
	
    void Update () {
        if (init == false)
        {
            FindSection();
        }
        else if (init == true){
            UpdateMaterial();
        }
	}

    private void FindSection(){
        SetOffsets();
        foreach (Renderer child in globe.GetComponentsInChildren<Renderer>()){
            if (child.gameObject.name == name)
            {
                mirroredTile = child.gameObject;
                mirroredRenderer = child;
                init = true;
                SetABToggle(false);
            }
        }
    }

    private void UpdateMaterial(){
        if (mirroredRenderer.enabled && rendererUpdated == true && mirroredRenderer.material.GetFloat("_Blend") > 0.9f) { return; }
        else if (!mirroredRenderer.enabled){
            rendererUpdated = false;
            mapRenderer.enabled = false;
            return;
        }
        if (mirroredRenderer.material.GetTexture("_NewTex")){
            mapRenderer.material.SetTexture("_NewTex", mirroredRenderer.material.GetTexture("_NewTex"));
            mapRenderer.material.SetFloat("_Blend", mirroredRenderer.material.GetFloat("_Blend"));
        }
        if (mirroredRenderer.material.GetTexture("_Overlay1"))
        {
            mapRenderer.material.SetTexture("_Overlay1", mirroredRenderer.material.GetTexture("_Overlay1"));
        }
        if (mirroredRenderer.material.GetTexture("_Overlay2"))
        {
            mapRenderer.material.SetTexture("_Overlay2", mirroredRenderer.material.GetTexture("_Overlay2"));
        }
        mapRenderer.enabled = true;
        rendererUpdated = true;
    }

    private void SetOffsets(){
        foreach (string textureName in textureNames){
            mapRenderer.material.SetTextureOffset(textureName, new Vector2(offsetX, offsetY));
            mapRenderer.material.SetTextureScale(textureName, new Vector2(scaleX, scaleY));
        }
    }

    public void SaveTexture(){
        mapRenderer.material.SetTexture("_OldTex", mapRenderer.material.GetTexture("_NewTex"));
        mapRenderer.material.SetTexture("_OldOverlay1", mapRenderer.material.GetTexture("_Overlay1"));
        mapRenderer.material.SetTexture("_OldOverlay2", mapRenderer.material.GetTexture("_Overlay2"));

        mirroredRenderer.material.SetTexture("_OldTex", mirroredRenderer.material.GetTexture("_NewTex"));
        mirroredRenderer.material.SetTexture("_OldOverlay1", mirroredRenderer.material.GetTexture("_Overlay1"));
        mirroredRenderer.material.SetTexture("_OldOverlay2", mirroredRenderer.material.GetTexture("_Overlay2"));
    }

    public void SetABToggle(bool toggle){
        if (init == false) { return; }
        if (toggle){
            mapRenderer.material.SetInt("_Toggle", 1);
            mirroredRenderer.material.SetInt("_Toggle", 1);
        }
        else{
            mapRenderer.material.SetInt("_Toggle", 0);
            mirroredRenderer.material.SetInt("_Toggle", 0);
        }
    }

    public void SetABPosition(Vector3 abPosition){
        if (init == false) { return; }
        mapRenderer.material.SetVector("_SpyPos", abPosition);
        mirroredRenderer.material.SetVector("_SpyPos", abPosition);
    }

    public void SetABRadius(float radius){
        if (init == false) { return; }
        mapRenderer.material.SetFloat("_Radius", radius);
        mirroredRenderer.material.SetFloat("_Radius", radius * globe.transform.localScale.x * 4);
    }
}
