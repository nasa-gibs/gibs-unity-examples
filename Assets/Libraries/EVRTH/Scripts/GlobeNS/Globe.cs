using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using EVRTH.Scripts.Geometry;
using EVRTH.Scripts.GIBS;
using EVRTH.Scripts.Utility;
using EVRTH.Scripts.Visualization;
using EVRTH.Scripts.WMS;
using UnityEngine;

namespace EVRTH.Scripts.GlobeNS
{
    public class Globe : AbstractGlobe
    {
        /// properties
        public Dictionary<string, Layer> availableLayers;

        //readonly and initialization
        public string initialLayer = "BlueMarble_NextGeneration";
        public DateTime initialDateTime = new DateTime(2016, 9, 18);
        public const int MinZoom = 1;
        public const int MaxZoom = 8;

        public bool forceZoomLevel;
        public int zoomLevelToForce;

        //stored components

        public GameObject tileTemplate;
        [HideInInspector]
        public GeographicGridTessellator tessellator = new GeographicGridTessellator();
        public TileTextureCache tileTextureCache;
        [HideInInspector]
        public TileFactory tileFactory;

        [HideInInspector]
        public DownloadManager downloader;

        public Camera cullingCamera;

        public int updateTileVisibilityCoroutineCount;

        /// <summary>
        /// Name of a layer drawn as an overlay on the globe.
        /// </summary>
        public string selectedOverlay = "";

        //convenience lookups
        public Layer CurrentLayer
        {
            get { return availableLayers[layers[0].name]; }
        }
        public int MaxDepth
        {
            get { return CurrentLayer.MaxDepth; }
        }

        private Plane[] cachedFrustum;
        private int cachedFrustumFrameCount;

        /// <summary>
        /// Camera frustum in world space for this frame.
        /// </summary>
        public Plane[] FrustumForThisFrame
        {
            get
            {
                if (cachedFrustumFrameCount != Time.frameCount)
                {
                    Matrix4x4 cameraMatrix = cullingCamera.projectionMatrix * cullingCamera.worldToCameraMatrix;
                    cachedFrustum = GeometryUtility.CalculateFrustumPlanes(cameraMatrix);
                    cachedFrustumFrameCount = Time.frameCount;
                }
                return cachedFrustum;
            }
        }

        // Maximum number of layers supported by the globe shader.
        public static readonly int MaxLayers = 3;
        public GlobeLayerInfo[] layers;

        //Status properties
        [HideInInspector]
        public bool cachedShowGrid;
        public float crossFadeDuration = 0.5f;
        [HideInInspector]
        public int parentTilesPerFrameForWhichToUpdateVisibility;
        [HideInInspector]
        public int parentTilesUpdated;
        [HideInInspector]
        public int refreshesThisFrame;

        public GlobeTile[] rootTiles;

        public Action<string> onLayerRemoved;

        internal bool parsedAvailableLayers;

        /// <summary>
        /// Overall status of layer loading. If any layer is loading returns Loading.
        /// If any layer is transitioning returns Transitioning. Returns Complete only if
        /// all layers are Complete.
        /// </summary>
        public LayerStatus Status
        {
            get
            {
                for (int i = 0; i < layers.Length; i++)
                {
                    if (layers[i].status == LayerStatus.Loading)
                    {
                        return LayerStatus.Loading;
                    }

                    if (layers[i].status == LayerStatus.Transitioning)
                    {
                        return LayerStatus.Transitioning;
                    }
                }
                return LayerStatus.Complete;
            }
        }

        public override string GetInitialLayerId()
        {
            return initialLayer;
        }

        public Vector3 LatLonToPosition(LatLon latLong)
        {
            return GeographicGridTessellator.LatLonToPosition(latLong, Ellipsoid.ScaledWgs84);
        }

        private void Awake()
        {
            cullingCamera = FindVrCamera() ?? Camera.main;

            layers = new GlobeLayerInfo[MaxLayers];

            for (int i = 0; i < MaxLayers; i++)
            {
                // Reset layer
                layers[i] = new GlobeLayerInfo();
            }
        }

        public override void ClearLayers()
        {
            for (int i = 0; i < MaxLayers; i++)
            {
                // Reset layer
                layers[i] = new GlobeLayerInfo();
            }

            LoadLayer(initialLayer, initialDateTime);
        }

        private Camera FindVrCamera()
        {
            GameObject cameraRig = GameObject.Find("[CameraRig]");
            return cameraRig != null ? cameraRig.GetComponentsInChildren<Camera>().FirstOrDefault(cam => !cam.orthographic) : null;
        }

        private IEnumerator Start()
        {
            //instantiate stored components
            downloader = GetComponent<DownloadManager>();
            tileFactory = GetComponent<TileFactory>();
            if (tileTextureCache == null)
            {
                tileTextureCache =
                    GetComponent<TileTextureCache>(); //GameObject.Find("TileTextureCache").GetComponent<TileTextureCache>();
            }

            //start processes
            //Load the local text files used to initialize the LayerLoader
            //this is done before the information is needed because you cannot access TextAsset from a thread
            LayerLoader.Init();

            //Kick off the main xml parsing in a thread to keep the application from hanging, especially for vr
            new Thread(() =>
            {
                //get the available layers by parsing the xml
                availableLayers = LayerLoader.GetLayers();
                //let the main thread know that it is safe to try to access the layers
                parsedAvailableLayers = true;
            }).Start();
            WaitForEndOfFrame w = new WaitForEndOfFrame();
            //wait for the xml to finish parsing
            while (!parsedAvailableLayers)
            {
                yield return w;
            }

            LoadLayer(initialLayer, initialDateTime);
            CreateRootTiles();
            StartCoroutine(UpdateTileVisibility());
        }

        private void Update()
        {
            UpdateLayerLoading();

            refreshesThisFrame = 0;
        }

        private void UpdateLayerLoading()
        {
            bool transitionStarting = false;
            for (int i = 0; i < layers.Length; i++)
            {
                // If the layer is waiting for texture and all the root texture are now available,
                // advance the loading state.
                if (layers[i].status == LayerStatus.Loading && layers[i].rootTilesLoaded == rootTiles.Length)
                {
                    layers[i].status = LayerStatus.Transitioning;
                    transitionStarting = true;
                }
            }

            // If a transition is beginning this frame, update all tiles to ensure
            // that only the root tiles are visible during the transition.
            if (transitionStarting)
            {
                for (int i = 0; i < rootTiles.Length; i++)
                {
                    UpdateTileAndChildren(rootTiles[i]);
                }
            }

            for (int i = 0; i < layers.Length; i++)
            {
                if (layers[i].status == LayerStatus.Transitioning)
                {
                    UpdateLayerTransition(i, layers[i], crossFadeDuration);
                }
            }
        }

        public override void StopAllDownloading()
        {
            downloader.Clear();
        }

        public override Layer GetLayer(string layerName)
        {
            return availableLayers[layerName];
        }

        private void UpdateLayerTransition(int layerIndex, GlobeLayerInfo layer, float blendDuration)
        {
            float blendAmount = layer.elapsedTransitionTime / blendDuration;
            float smoothedBlendAmount = Mathf.SmoothStep(0.0f, 1.0f, blendAmount);

            for (int j = 0; j < rootTiles.Length; j++)
            {
                rootTiles[j].SetTransitionProgress(layerIndex, blendAmount);
            }

            layer.transitionProgress = smoothedBlendAmount;

            if (blendAmount >= 1f)
            {
                layer.status = LayerStatus.Complete;
                layer.rootTilesLoaded = 0;
            }

            layer.elapsedTransitionTime += Time.deltaTime;
        }

        private IEnumerator UpdateTileVisibility()
        {
            updateTileVisibilityCoroutineCount++;

            while (Application.isPlaying)
            {
                parentTilesUpdated = 0;
                for (int i = 0; i < rootTiles.Length; i++)
                {
                    UpdateTileAndChildren(rootTiles[i]);
                    parentTilesUpdated++;

                    // Take a break if we've run out of tiles to check
                    if (parentTilesUpdated >= parentTilesPerFrameForWhichToUpdateVisibility)
                    {
                        parentTilesUpdated = 0;
                        yield return null;
                    }
                }
                yield return null;
            }
        }

        private void UpdateTileAndChildren(GlobeTile tile)
        {
            tile.UpdateStoredVisibility();
            tile.UpdateAppearance();

            UpdateChildren(tile);
        }

        private void UpdateChildren(GlobeTile tile)
        {
            Transform tileTf = tile.transform;

            for (int childIdx = 0; childIdx < tileTf.childCount; childIdx++)
            {
                GlobeTile childTile = tileTf.GetChild(childIdx).GetComponent<GlobeTile>();
                childTile.UpdateStoredVisibility();
                childTile.UpdateAppearance();

                UpdateChildren(childTile);
            }
        }

        private void CreateRootTiles()
        {
            int width = CurrentLayer.tileMatrixSet[MinZoom].matrixWidth;
            int height = CurrentLayer.tileMatrixSet[MinZoom].matrixHeight;
            rootTiles = new GlobeTile[width * height];

            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    rootTiles[row * width + col] = CreateTile(transform, new Wmts { row = row, col = col, zoom = MinZoom });
                }
            }
        }

        public override void LoadLayer(string layerName, DateTime date)
        {
            LoadLayer(0, layerName, date);
        }

        public override void LoadLayer(int index, string layerName, DateTime date)
        {
            if (index >= MaxLayers)
            {
                throw new ArgumentException("Too many layers");
            }

            GlobeLayerInfo globeLayer = layers[index];
            globeLayer.name = layerName;
            globeLayer.date = date;
            globeLayer.status = LayerStatus.Loading;
            globeLayer.rootTilesLoaded = 0;
            globeLayer.transitionProgress = 0f;
            globeLayer.elapsedTransitionTime = 0f;
            globeLayer.wmsLayer = availableLayers[layerName];
        }

        public void GetLayerNames(string[] layerArray)
        {
            int index = 0;
            while (index < MaxLayers)
            {
                layerArray[index] = layers[index].name;
                if (string.IsNullOrEmpty(layerArray[index]))
                {
                    layerArray[index] = "";
                }
                index++;
            }
        }

        public override GlobeLayerInfo[] LayerInfo
        {
            get { return layers; }
        }

        // TODO should use TileFactory
        private GlobeTile CreateTile(Transform parent, Wmts coords)
        {
            GameObject tileGo = Instantiate(tileTemplate);

            // high level tile mesh needs to be scaled for radius of globe -pete
            tileGo.transform.SetParentClearRelativeTransform(parent.transform, Vector3.zero, Quaternion.identity, Vector3.one * 0.5f);
            tileGo.name = coords.ToString();

            GlobeTile tile = tileGo.GetComponent<GlobeTile>();
            tile.coords = coords;
            tile.globe = this;
            tile.bBox = CurrentLayer.Wmts2Bbox(coords);

            return tile;
        }

        public static int TessellationDivisions(int level)
        {
            // TODO: this should be computed based on size of the globe, but for now just some fixed tessellation levels.
            return level <= 2 ? 20 : 5;
        }
    }
}
