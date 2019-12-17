using System;
using System.Collections.Generic;
using EVRTH.Scripts.GlobeNS;
using EVRTH.Scripts.Utility;
using UnityEngine;

// This class allows the easy application of layers onto the globe. It manages all of the
// visualization control for that globe.
namespace EVRTH.Scripts.Visualization
{
    public class LayerApplier : MonoBehaviour
    {
        public AbstractGlobe globe;
        public ScienceDataIngestor scienceDataIngestor0;
        public ScienceDataIngestor scienceDataIngestor1;
        public DataVisualizer dataVisualizer0;
        public DataVisualizer dataVisualizer1;

        public Transform globeColliderTransform;

        public enum LayerVisualizationStyle
        {
            Flat,
            Volumetric
        }

        public struct AppliedLayer
        {
            public string layerName;
            public DateTime selectedDateTime;
            public LayerVisualizationStyle style;
            public int layerIndex;
        }

        private readonly List<AppliedLayer> appliedLayers = new List<AppliedLayer>();

        public event Action<string, DateTime, LayerVisualizationStyle, int> OnLayerApplied;

        public static LayerApplier Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
        }

        public string GetInitialLayerId()
        {
            return globe.GetInitialLayerId();
        }

        public void ClearGlobe()
        {
            // Stop active downloads
            globe.StopAllDownloading();

            // Clear flat layers
            globe.ClearLayers();

            // Clear volumetric layers
            dataVisualizer0.Reset();
            scienceDataIngestor0.ClearDataState();
            dataVisualizer1.Reset();
            scienceDataIngestor1.ClearDataState();

            appliedLayers.Clear();
        }

        public void ClearDataLayers()
        {
            // Clear volumetric layers
            dataVisualizer0.Reset();
            scienceDataIngestor0.ClearDataState();
            dataVisualizer1.Reset();
            scienceDataIngestor1.ClearDataState();

            int layerCounter = 0;
            while (layerCounter < appliedLayers.Count)
            {
                if (appliedLayers[layerCounter].style != LayerVisualizationStyle.Flat)
                {
                    appliedLayers.RemoveAt(layerCounter);
                }
                else
                {
                    layerCounter++;
                }
            }
        }

        public void ChangeDateForDisplayedLayers(DateTime newDateTime)
        {
            // Make a copy of the currently applied layers, so we're not iterating through
            // a list that we're changing on-the-fly
            AppliedLayer[] appliedLayerListCopy = new AppliedLayer[appliedLayers.Count];
            appliedLayers.CopyTo(appliedLayerListCopy);

            // Cycle through all of the layers and re-apply them with a new dateTime
            for (int i = 0; i < appliedLayerListCopy.Length; i++)
            {
                AppliedLayer layer = appliedLayerListCopy[i];
                ApplyLayer(layer.layerName, newDateTime, layer.style, layer.layerIndex);
            }
        }

        public void SetUpAnimation()
        {

        }

        private void AddToAppliedLayerList(string layerName, DateTime selectedDateTime, LayerVisualizationStyle style,
            int layerIndex)
        {
            AppliedLayer layer = new AppliedLayer
            {
                layerName = layerName,
                selectedDateTime = selectedDateTime,
                style = style,
                layerIndex = layerIndex
            };

            // Clear matching layer from list, if necessary
            appliedLayers.RemoveAll(x => (x.layerIndex == layerIndex) && (x.style == style));

            // Add to list
            appliedLayers.Add(layer);
        }

        public void ApplyLayer(string layerName, DateTime selectedDateTime, LayerVisualizationStyle style,
            int layerIndex = 0)
        {
            Debug.Log("Applying layer " + layerName + layerIndex + " in style " + style);

            switch (style)
            {
                case LayerVisualizationStyle.Flat:
                    // Load specified layer directly onto the globe geometry
                    globe.LoadLayer(layerIndex, layerName, selectedDateTime);
                    break;
                case LayerVisualizationStyle.Volumetric:
                    // Load specified layer as an extruded mesh from the globe
                    switch (layerIndex)
                    {
                        case 0:
                            if (scienceDataIngestor0.isActiveAndEnabled && dataVisualizer0.isActiveAndEnabled)
                            {
                                dataVisualizer0.SetVisualizationSources(globe, globeColliderTransform,
                                    scienceDataIngestor0);
                                dataVisualizer0.Reset();
                                scienceDataIngestor0.IngestDataForDate(selectedDateTime, layerName, dataVisualizer0);
                            }
                            break;
                        case 1:
                            if (scienceDataIngestor1.isActiveAndEnabled && dataVisualizer1.isActiveAndEnabled)
                            {
                                dataVisualizer1.SetVisualizationSources(globe, globeColliderTransform,
                                    scienceDataIngestor1);
                                dataVisualizer1.Reset();
                                scienceDataIngestor1.IngestDataForDate(selectedDateTime, layerName, dataVisualizer1);
                            }
                            break;
                    }
                    break;
            }

            AddToAppliedLayerList(layerName, selectedDateTime, style, layerIndex);

            if (OnLayerApplied != null)
                OnLayerApplied.Invoke(layerName, selectedDateTime, style, layerIndex);
        }

        public float GetGlobeLayerLoadingTransition(string layerId)
        {
            GlobeLayerInfo gli = null;
            foreach (GlobeLayerInfo globeLayerInfo in globe.LayerInfo)
            {
                if (globeLayerInfo.name == layerId)
                {
                    gli = globeLayerInfo;
                    break;
                }
            }

            if (gli == null)
            {
                return 0f;
            }
            return gli.status == LayerStatus.Complete ? 1f : gli.transitionProgress;
        }
    }
}
