using System.Collections;
using System.Collections.Generic;
using EVRTH.Scripts.GlobeNS;
using EVRTH.Scripts.Visualization;
using UnityEngine;

namespace EVRTH.Scripts.Utility
{
    public class LayerPresetLoader : MonoBehaviour
    {
        public Globe globe;
        private LayerApplier layerApplier;
        public Date date;
        [Space]
        [Space]

        [HideInInspector]
        public List<Preset> presets = new List<Preset>();
        [HideInInspector]
        public int currentPreset;

        private IEnumerator Start()
        {
            WaitForEndOfFrame w = new WaitForEndOfFrame();
            while (!globe.parsedAvailableLayers)
            {
                yield return w;
            }
            layerApplier = globe.GetComponent<LayerApplier>();
        }

        public void ApplyPreset(int toApply)
        {
            print("Applying preset " + toApply);
            if (presets.Count > toApply && toApply >= 0)
            {
                currentPreset = toApply;
                layerApplier.dataVisualizer0.Reset();
                layerApplier.dataVisualizer1.Reset();
                Preset set = presets[toApply];
                for (int i = 0; i < set.layersInPreset.Count; i++)
                {
                    layerApplier.ApplyLayer(set.layersInPreset[i],date.ToDateTime,LayerApplier.LayerVisualizationStyle.Flat,i);
                }
                for (int i = 0; i < 2; i++)
                {
                    if(!string.IsNullOrEmpty(set.volumetricLayers[i]))
                        layerApplier.ApplyLayer(set.volumetricLayers[i], date.ToDateTime, LayerApplier.LayerVisualizationStyle.Volumetric, i);
                }
            }
        }
    }
}
