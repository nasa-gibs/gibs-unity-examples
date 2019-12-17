using System;
using System.Collections;
using System.Collections.Generic;
using EVRTH.Scripts.GlobeNS;
using EVRTH.Scripts.Utility;
using EVRTH.Scripts.WMS;
using UnityEngine;

namespace EVRTH.Scripts.Visualization
{
    /// <summary>
    /// Gets science data from GIBS and passes it to the appropriate visualizer/s
    /// </summary>
    public class ScienceDataIngestor : MonoBehaviour
    {
        private string layerName;
        public string textureFormatString;
        public string stylesString;

        public bool averageWithPreviousDays;

        public float latStep;
        public float lonStep;

        public bool bilinearlyFilterScienceDataValues;

        public int daysToUse;

        public int colorMissingFromDictCount;
        public int zeroColorValuesFound;

        private bool tileStructuresCreated;

        private DownloadManager downloadManager;
        private AbstractGlobe globe;

        private Dictionary<string, List<Tile>> layerTiles;

        private void Start()
        {

            downloadManager = transform.parent.GetComponent<DownloadManager>();
            globe = transform.parent.GetComponent<AbstractGlobe>();
            layerTiles = new Dictionary<string, List<Tile>>();
        }

        public void ClearDataState()
        {
            // Delete all children of this transform
            List<Transform> children = new List<Transform>();
            foreach (Transform child in transform)
            {
                children.Add(child);
            }
            children.ForEach(child => Destroy(child.gameObject));
        }

        public bool AllTilesDownloaded()
        {
            if (!tileStructuresCreated)
            {
                return false;
            }

            foreach (Tile t in transform.GetComponentsInChildren<Tile>())
            {
                if (!t.loadComplete)
                {
                    return false;
                }
            }
            return true;
        }

        public void IngestDataForDate(DateTime loadTime, string layerToLoad, DataVisualizer deformedSphereMaker)
        {
            List<DataVisualizer> deformedSphereMakers = new List<DataVisualizer> {deformedSphereMaker};
            IngestDataForDate(loadTime, layerToLoad, deformedSphereMakers);
        }

        public void IngestDataForDate(DateTime loadTime, string layerToLoad, List<DataVisualizer> deformedSphereMakers, bool reuseAlreadyDownloadedTileSets = false)
        {
            if (!reuseAlreadyDownloadedTileSets)
            {
                // First clear all sets of tiles
                foreach (Transform child in transform)
                {
                    Destroy(child.gameObject);
                }
            }

            layerName = layerToLoad;

            LatLonBoundingBox initialMapTileBBox = new LatLonBoundingBox(-90.0f, -90.0f + latStep, -180.0f, -180.0f + lonStep);

            for (int i = 0; i < daysToUse; i++)
            {
                DateTime currentDate = loadTime.AddDays(-1 * i);
                string dataContainerObjectName = currentDate.ToString("M.dd.yyyy") + " Data";
                if (reuseAlreadyDownloadedTileSets)
                {
                    // Only add containers that we don't already have
                    if (!layerTiles.ContainsKey(dataContainerObjectName))
                    {
                        layerTiles.Add(dataContainerObjectName,new List<Tile>());
                        // Create tiles into which textures can be downloaded
                        CreateTileStructures(currentDate, initialMapTileBBox, layerTiles[dataContainerObjectName], deformedSphereMakers);

                        tileStructuresCreated = true;

                        // Download the textures
                        LoadData(currentDate, layerTiles[dataContainerObjectName]);
                    }
                    else
                    {
                        // We already have this data and don't need to download it. Let's immediately use it for deformation.
                        //Transform tileParentTransform = transform.Find(dataContainerObjectName);
                        StartCoroutine(DeformBasedOnChildTiles(layerTiles[dataContainerObjectName], deformedSphereMakers));
                    }
                }
                else
                {
                    // Add a tile container
                    // Create tiles into which textures can be downloaded
                    layerTiles.Add(dataContainerObjectName, new List<Tile>());
                    CreateTileStructures(currentDate, initialMapTileBBox, layerTiles[dataContainerObjectName], deformedSphereMakers);

                    tileStructuresCreated = true;

                    // Download the textures
                    LoadData(currentDate, layerTiles[dataContainerObjectName]);
                }

            
            }
        }
        private IEnumerator DeformBasedOnChildTiles(List<Tile> tiles, List<DataVisualizer> deformedSphereMakers)
        {
            foreach (Tile t in tiles)
            {
                // Use each tile to deform the deformSphereMakers
                for (int j = 0; j < deformedSphereMakers.Count; j++)
                {
                    StartCoroutine(deformedSphereMakers[j].DeformBasedOnNewTile(t, 0, 100));
                    yield return 0;
                }
                yield return 0;
            }
        }

        public void LoadData(DateTime date, List<Tile> tiles)
        {
            for (int i = 0; i < tiles.Count; i++)
            {
                tiles[i].LoadData(layerName, date);
            }
        }

        private void CreateTileStructures(DateTime dateTime, LatLonBoundingBox initialMapTileBBox, List<Tile> dataContainer, List<DataVisualizer> deformedSphereMakers)
        {
            for (float minLat = initialMapTileBBox.minLat; minLat + initialMapTileBBox.DeltaLat <= 90.0f; minLat += initialMapTileBBox.DeltaLat)
            {
                for (float minLon = initialMapTileBBox.minLon; minLon + initialMapTileBBox.DeltaLon <= 180.0f; minLon += initialMapTileBBox.DeltaLon)
                {
                    LatLonBoundingBox bbox = new LatLonBoundingBox(minLat, minLat + initialMapTileBBox.DeltaLat, minLon, minLon + initialMapTileBBox.DeltaLon);
                    CreateTile(bbox, dataContainer, dateTime, deformedSphereMakers);
                }
            }
        }

        private void CreateTile(LatLonBoundingBox bbox, List<Tile> dataContainer , DateTime dateTime, List<DataVisualizer> deformedSphereMakers)
        {
            Tile tile = new Tile
            {
                bBox = bbox,
                textureFormatString = textureFormatString,
                stylesString = stylesString,
                selectedLayer = layerName,
                selectedDate = dateTime,
                globe =  globe,
                downloadManager = downloadManager,
                deformedSphereMakers = deformedSphereMakers
            }; 

            tile.DetermineAndSetUrl();
            dataContainer.Add(tile);
        }
    }
}