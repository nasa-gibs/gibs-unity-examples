using System;
using System.Collections.Generic;
using EVRTH.Scripts.Geometry;
using EVRTH.Scripts.GIBS;
using EVRTH.Scripts.Utility;
using EVRTH.Scripts.WMS;
using UnityEngine;

namespace EVRTH.Scripts.GlobeNS
{
    public class GlobeTile : MonoBehaviour
    {
        private readonly string[] shaderOldTextureNames = { "_OldTex", "_OldOverlay1", "_OldOverlay2" };
        private readonly string[] shaderTextureNames = { "_NewTex", "_Overlay1", "_Overlay2" };
        private readonly string[] shaderBlendNames = { "_Blend", "_Overlay1Blend", "_Overlay2Blend" };

        /// <summary>
        /// Minimum value for the view angle scaling factor uses to select levels of detail.
        /// This factor is used to push tiles viewed straight on toward higher resolution
        /// and tiles viewed from the side toward lower resolution. However, if the factor
        /// gets too small it can push tiles at the edge of the globe too far toward low res.
        /// </summary>
        private const float minViewAngleFactor = 0.5f;

        //properties
        [HideInInspector]
        public Globe globe;
        public Wmts coords;
        private GlobeTile[] children;
        private int childCount; // Number of children that have been instantiated
        public string url;

        //stored components
        public Material mapMaterial;
        public Material wireFrameMaterial;
        [HideInInspector]
        public Bounds cullingBounds;
        public LatLonBoundingBox bBox;

        public LatLonBoundingBox FullBoundingBox
        {
            get
            {
                return globe.CurrentLayer.Wmts2Bbox(coords);
            }
        }

        //readonly and initialization
        public readonly int width = 512;
        public readonly int height = 512;

        //status properties
        [HideInInspector]
        public bool mustRefreshDebug;
        public bool manualRefreshRequested;

        private bool wasPreviouslyCulled;
        private bool storedVisibility;

        private float pixelSize;
        private float pixelDistanceScale;
        private Vector3 tileCenterOnGlobe;

        private readonly GlobeTileLayerInfo[] layers = new GlobeTileLayerInfo[Globe.MaxLayers];

        /// <summary>
        /// Geodetic surface normal, in local coordinates.
        /// </summary>
        private Vector3 surfaceNormal;

        public bool AllLayersLoaded
        {
            get
            {
                for (int i = 0; i < layers.Length; i++)
                {
                    if (layers[i].status != LoadStatus.Complete)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public bool CanRender
        {
            get
            {
                return AllLayersLoaded && !MustRefresh() || IsTopLevelTile(); // Can always render root nodes
            }
        }

        public bool ChildrenCanRender
        {
            get
            {
                if (children == null)
                {
                    return false;
                }

                foreach (GlobeTile child in children)
                {
                    if (child == null || !child.CanRender)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public bool ShouldHaveChildren
        {
            get
            {
                return coords.zoom < globe.MaxDepth;
            }
        }

        private void Start()
        {
            // Default all layers to complete status. Empty layers are treated as complete, and will be
            // marked in progress as when they are assigned content.
            for (int i = 0; i < layers.Length; i++)
            {
                layers[i].status = LoadStatus.Complete;
            }

            CreateGeometry(this);

            // Compute and cache expensive parts of the LOD calculation
            ComputeExpensiveLodParams();

            UpdateStoredVisibility();
            SetVisible(storedVisibility);
        }

        private void ComputeExpensiveLodParams()
        {
            // In order to compute how big a geometric error will appear on screen, we need to know how big a pixel is
            // at a given distance from the camera.  There is a linear relationship between pixel size and distance from camera,
            // so we precompute the expensive ratio here and later multiply this by distance from camera to get pixel size.
            float radiansPerPixel = Mathf.Deg2Rad * (globe.cullingCamera.fieldOfView / globe.cullingCamera.pixelHeight);
            pixelDistanceScale = Mathf.Tan(radiansPerPixel);

            radiansPerPixel = (bBox.DeltaLat / height) * Mathf.Deg2Rad;
            pixelSize = Mathf.Tan(radiansPerPixel);

            // TODO: .5 factor is to match the size of the tiles vs the globe object. We should either eliminate this difference or centralize it.
            tileCenterOnGlobe = globe.LatLonToPosition(bBox.Center) * 0.5f;
        }

        private bool IsTopLevelTile()
        {
            return (transform.parent.GetComponent<GlobeTile>() == null);
        }

        public void UpdateAppearance()
        {
            // Only make adjustments to visibility criteria once we're finished with the crossfade for this load,
            // to avoid race condition pop-ins
            bool visible = storedVisibility;

            if (globe.Status == LayerStatus.Complete ||
                //globe.wireFrameMode ||
                IsTopLevelTile() ||
                wasPreviouslyCulled)
            {
                SetVisible(visible);
            }

            wasPreviouslyCulled = false;

            if (globe.Status == LayerStatus.Transitioning)
            {
                // Special behavior; only render top-level
                SetVisible(IsTopLevelTile());
            }
            else
            {
                // Normal behavior

                // Prioritize top-level tiles when we're still waiting for a cross-fade
                if (globe.Status == LayerStatus.Loading && MustRefresh())
                {
                    if (IsTopLevelTile())
                    {
                        Refresh();
                    }
                }
                // Cross-fade has occurred; perform normal logic
                else if (MustRefresh())
                {
                    if (!IsTopLevelTile())
                    {
                        if (!transform.parent.GetComponent<GlobeTile>().MeetsLodCriteria())
                        {
                            Refresh();
                        }
                    }
                    else
                    {
                        // Top level tile; refresh!
                        Refresh();
                    }
                }
            }

            mustRefreshDebug = MustRefresh();
        }

        public void AddChild(GlobeTile tile)
        {
            if (children == null)
            {
                children = new GlobeTile[4];
            }

            children[childCount] = tile;
            childCount += 1;
        }

        public bool MeetsLodCriteria()
        {
            if (globe.forceZoomLevel)
            {
                return (coords.zoom >= globe.zoomLevelToForce);
            }

            Camera cam = globe.cullingCamera;

            Vector3 tileCenter = globe.transform.TransformPoint(tileCenterOnGlobe);

            float cameraResolution = (cam.transform.position - tileCenter).magnitude * pixelDistanceScale;

            // Compute a scale factor based on the angle between the view vector and the tile's normal vector. This pushes tiles
            // the viewer is looking at directly toward higher resolution and tiles that are seen edge on toward lower resolution.
            Vector3 cameraDir = cam.transform.forward;
            Vector3 cameraDirLs = transform.InverseTransformDirection(cameraDir);
            float viewAngleFactor = Mathf.Max(Mathf.Abs(Vector3.Dot(cameraDirLs, surfaceNormal)), minViewAngleFactor);

            bool meetsLodCriteria = cameraResolution > pixelSize * viewAngleFactor * globe.transform.lossyScale.x;

            return meetsLodCriteria;
        }

        public void UpdateStoredVisibility()
        {
            storedVisibility = GetVisibilityBasedOnLodCriteria();
        }

        private bool GetVisibilityBasedOnLodCriteria()
        {
            bool visible = false;

            if (MeetsLodCriteria())
            {
                // We're good enough LOD, hurray! Let's make sure our parents aren't good enough, too
                // (we don't want to steal their thunder)
                if (!IsTopLevelTile())
                {
                    if (!transform.parent.GetComponent<GlobeTile>().MeetsLodCriteria())
                    {
                        // We're not stealing their thunder! Let's draw ourselves
                        if (CanRender)
                        {
                            visible = true;
                        }
                    }
                }
                else
                {
                    // We have no parents (;-;) There's no parental thunder to steal, so
                    // we'll do the rendering
                    visible = true;
                }
            }
            else if (transform.childCount == 0)
            {
                // Oh geez, we don't meet the LOD criteria, but we don't have any children who can help us out,
                // so we'll draw ourselves, if we can
                visible = CanRender;

                // Generate child nodes if they have not already been generated
                if (ShouldHaveChildren && children == null)
                {
                    CreateChildren();
                }
            }
            else
            {
                // We don't meet the LOD criteria, but we have children. We'll do nothing and push the problem
                // on to the next generation... if our children can render, that is!
                if (ChildrenCanRender)
                {
                }
                else
                {
                    if (CanRender)
                    {
                        visible = true;
                    }

                }
            }

            return visible;
        }

        public void Refresh()
        {
            GlobeLayerInfo[] globeLayers = globe.layers;
            for (int i = 0; i < layers.Length; i++)
            {
                GlobeLayerInfo globeLayer = globeLayers[i];
                GlobeTileLayerInfo myLayer = layers[i];

                if (myLayer.name != globeLayer.name || myLayer.date != globeLayer.date || manualRefreshRequested)
                {
                    layers[i].name = globeLayer.name;
                    layers[i].date = globeLayer.date;

                    if (string.IsNullOrEmpty(globeLayer.name))
                    {
                        ClearTexture(i);
                    }
                    else
                    {
                        if (globeLayer.wmsLayer != null)
                        {
                            layers[i].status = LoadStatus.InProgress;
                            RequestTexture(globeLayer.wmsLayer, layers[i].date, LoadTexture, i);
                        }	
                    }
                }
            }

            manualRefreshRequested = false;
            globe.refreshesThisFrame++;
        }

        private bool MustRefresh()
        {
            GlobeLayerInfo[] globeLayers = globe.layers;
            for (int i = 0; i < layers.Length; i++)
            {
                if (layers[i].name != globeLayers[i].name ||
                    layers[i].date != globeLayers[i].date)
                {
                    return true;
                }
            }
            return manualRefreshRequested;
        }

        private void CreateGeometry(GlobeTile tile)
        {
            int tessellation = Globe.TessellationDivisions(coords.zoom);

            Mesh tileMesh = tile.globe.tessellator.GenerateSector(tile.FullBoundingBox, Ellipsoid.ScaledWgs84, tessellation, tessellation);
            gameObject.GetComponent<MeshFilter>().mesh = tileMesh;
            cullingBounds = tileMesh.bounds;

            Vector3 tileCenter = GeographicGridTessellator.LatLonToPosition(tile.bBox.Center, Ellipsoid.ScaledWgs84);
            Vector3 normal = Ellipsoid.ScaledWgs84.GeodeticSurfaceNormal(tileCenter);

            Vector3 normalWs = globe.transform.TransformDirection(normal);
            surfaceNormal = transform.InverseTransformDirection(normalWs); // Store in local space
        }

        private void CreateChildren()
        {
            children = new GlobeTile[4];

            List<Wmts> newCoords = new List<Wmts>
            {
                new Wmts {row = coords.row * 2, col = coords.col * 2, zoom = coords.zoom + 1},
                new Wmts {row = coords.row * 2, col = (coords.col * 2) + 1, zoom = coords.zoom + 1},
                new Wmts {row = (coords.row * 2) + 1, col = coords.col * 2, zoom = coords.zoom + 1},
                new Wmts {row = (coords.row * 2) + 1, col = (coords.col * 2) + 1, zoom = coords.zoom + 1}
            };
            foreach (Wmts newCoord in newCoords)
            {
                globe.tileFactory.CreateTileAsync(this, newCoord);
            }
        }

        public void SetVisible(bool isEnabled)
        {
            GetComponent<Renderer>().enabled = isEnabled;
            GetComponent<MeshRenderer>().enabled = isEnabled;
        }

        public void LoadTexture(string layerNameToSet, DateTime dateTimeToSet, Texture myTexture)
        {
            // Make sure that the texture we've receive is for a layer currently on the globe
            int layerIndex = FindLayer(layerNameToSet, dateTimeToSet);
            if (layerIndex == -1)
            {
                Debug.LogFormat("Can't find layer {0} for this tile at {1}", layerNameToSet, dateTimeToSet.ToLongDateString());
                return;
            }

            //if (!globe.wireFrameMode)
            //{
                string textureParam = shaderTextureNames[layerIndex];

                Debug.Assert(GetComponent<Renderer>().material.HasProperty(textureParam), "Shader does not support property " + textureParam);
                Material currentMaterial = GetComponent<Renderer>().material;
                Texture currentTexture = GetComponent<Renderer>().material.GetTexture(textureParam);
                Vector2 currentTextureOffset = currentMaterial.GetTextureOffset(textureParam);
                Vector2 currentTextureScale = currentMaterial.GetTextureScale(textureParam);

                // If this is a root tile let the globe know that we're loaded
                if (IsTopLevelTile())
                {
                    globe.layers[layerIndex].rootTilesLoaded++;
                }
                float value = IsTopLevelTile() ? 0f : 1f;
                PrepareForLayerTransition(layerIndex, myTexture, currentTexture,  currentTextureOffset, currentTextureScale, value);
            //}
            //else
            //{
            //    GetComponent<Renderer>().material = wireFrameMaterial;
            //}
            layers[layerIndex].status = LoadStatus.Complete;
        }
	
        public void ClearTexture(int layerIndex)
        {
            //if (!globe.wireFrameMode)
            //{
                string textureParam = shaderTextureNames[layerIndex];

                Debug.Assert(GetComponent<Renderer>().material.HasProperty(textureParam), "Shader does not support property " + textureParam);
                Material currentMaterial = GetComponent<Renderer>().material;
                Texture currentTexture = GetComponent<Renderer>().material.GetTexture(textureParam);
                Vector2 currentTextureOffset = currentMaterial.GetTextureOffset(textureParam);
                Vector2 currentTextureScale = currentMaterial.GetTextureScale(textureParam);

                // If this is a root tile let the globe know that we're loaded
                if (IsTopLevelTile())
                {
                    globe.layers[layerIndex].rootTilesLoaded++;
                }
                float value = IsTopLevelTile() ? 0f : 1f;
                PrepareForLayerTransition(layerIndex, null, currentTexture,  currentTextureOffset, currentTextureScale, value);
            //}
            //else
            //{
            //    GetComponent<Renderer>().material = wireFrameMaterial;
            //}
            layers[layerIndex].status = LoadStatus.Complete;
        }

        private int FindLayer(string layerName, DateTime date)
        {
            for (int i = 0; i < layers.Length; i++)
            {
                if (layers[i].name == layerName && layers[i].date == date)
                {
                    return i;
                }
            }
            return -1;
        }

        private void PrepareForLayerTransition(int layerIndex, Texture newTexture, Texture currentTexture, Vector2 currentTextureOffset, Vector2 currentTextureScale, float transitionValue)
        {
            Material currentMaterial =  GetComponent<Renderer>().material;

            // Move current texture to old texture slot and swap over to showing it
            string oldTexParam = shaderOldTextureNames[layerIndex];
            currentMaterial.SetTexture(oldTexParam, currentTexture);
            currentMaterial.SetTextureOffset(oldTexParam, currentTextureOffset);
            currentMaterial.SetTextureScale(oldTexParam, currentTextureScale);

            // Move new texture to new texture slot
            string newTexParam = shaderTextureNames[layerIndex];
            currentMaterial.SetTexture(newTexParam, newTexture);
		
            // Determine what the scales and offsets should be based on zoom level comparisons
            if (globe.layers[layerIndex].wmsLayer.MaxDepth < coords.zoom)
            {
                int zoomDifference = (coords.zoom - globe.layers[layerIndex].wmsLayer.MaxDepth);
                float zoomScale = Mathf.Pow(0.5f, zoomDifference);

                currentMaterial.SetTextureOffset(newTexParam, new Vector2((coords.col*zoomScale)%1.0f, (1.0f - (coords.row*zoomScale)%1.0f) - zoomScale));
                currentMaterial.SetTextureScale(newTexParam, new Vector2(zoomScale, zoomScale));
            }
            else
            {
                currentMaterial.SetTextureOffset(newTexParam, Vector2.zero);
                currentMaterial.SetTextureScale(newTexParam, Vector2.one);
            }

            // Start the cross fade animation from 0
            SetTransitionProgress(layerIndex, transitionValue);
        }

        public void SetTransitionProgress(int layerIndex, float progress)
        {
            string blendParam = shaderBlendNames[layerIndex];
            GetComponent<Renderer>().material.SetFloat(blendParam, progress);
        }

        private void RequestTexture(Layer layer, DateTime date, TextureDownloadHandler handler, int layerNum)
        {
            url = layer.WmtsToUrl(coords, date);
            //Debug.Log(url);
            // Load from cache if possible
            if (globe.tileTextureCache.ExistsInCache(url))
            {
                handler(layer.identifier, date, globe.tileTextureCache.GetTexture(url));
            }
            else
            {
                globe.downloader.RequestTexture(url, layer.identifier, date, handler, queueNum: layerNum + 1);
            }
        }
    }
}
