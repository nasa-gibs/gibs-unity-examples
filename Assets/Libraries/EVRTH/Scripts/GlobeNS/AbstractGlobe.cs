using System;
using EVRTH.Scripts.GIBS;
using EVRTH.Scripts.Utility;
using UnityEngine;

namespace EVRTH.Scripts.GlobeNS
{
    public abstract class AbstractGlobe : MonoBehaviour
    {
        public string selectedLayer;
        public DateTime selectedDate;

        public abstract string GetInitialLayerId();

        public abstract void LoadLayer(string layerName, DateTime date);

        public abstract void LoadLayer(int layerIndex, string layerName, DateTime date);

        public abstract void ClearLayers();

        public abstract void StopAllDownloading();

        public abstract Layer GetLayer(string layerName);

        public abstract GlobeLayerInfo[] LayerInfo
        {
            get;
        }
    }
}
