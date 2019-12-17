using System;
using EVRTH.Scripts.GIBS;

namespace EVRTH.Scripts.Utility
{
    /// <summary>
    /// Maintains information on how a layer is rendered on the globe.
    /// </summary>
    [Serializable]
    public class GlobeLayerInfo
    {
        public string name;
        public DateTime date;
        public Layer wmsLayer;
        public int guid;

        public int rootTilesLoaded;

        public LayerStatus status;
        public float transitionProgress;
        public float elapsedTransitionTime;

        public GlobeLayerInfo()
        {
            // Treat empty layers as complete by default
            status = LayerStatus.Complete;
        }
    }
}
