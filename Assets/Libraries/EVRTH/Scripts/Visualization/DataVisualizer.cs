using System.Collections;
using EVRTH.Scripts.Geometry;
using EVRTH.Scripts.GlobeNS;
using EVRTH.Scripts.Utility;
using UnityEngine;

namespace EVRTH.Scripts.Visualization
{
    /// <summary>
    /// base class for science data visualizers
    /// Used for creating visualizations of layers other than simply drawing them
    /// </summary>
    public abstract class DataVisualizer : MonoBehaviour
    {
        protected AbstractGlobe globe;
        protected Transform transformToWrap;
        protected ScienceDataIngestor dataIngestor;

        public virtual void SetVisualizationSources(AbstractGlobe nGlobe, Transform target, ScienceDataIngestor nDataIngestor)
        {
            globe = nGlobe;
            transformToWrap = target;
            dataIngestor = nDataIngestor;
        }

        public abstract void GenerateVisualization();

        public virtual void Reset()
        {

        }

        public LatLongValue GetLatLonFrom3DPoint(Vector3 point)
        {
            return LatLongValue.GetLatLonFrom3DPoint(point, transformToWrap);
        }

        public virtual IEnumerator DeformBasedOnNewTile(Tile tile, float min, float max)
        {
            yield return 0;
        }
    }
}
