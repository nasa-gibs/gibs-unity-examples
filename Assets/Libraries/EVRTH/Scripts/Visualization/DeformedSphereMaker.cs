using System;
using System.Collections;
using System.Collections.Generic;
using EVRTH.Scripts.Geometry;
using EVRTH.Scripts.GIBS;
using EVRTH.Scripts.GlobeNS;
using EVRTH.Scripts.Utility;
using UnityEngine;

namespace EVRTH.Scripts.Visualization
{
    /// <summary>
    /// Creates the geometric visualization of science data.
    /// </summary>
    public class DeformedSphereMaker : DataVisualizer
    {
        public float padding;
        public float maxHeight;
        public float latStep;
        public float lonStep;

        public float sphereRadius = 1.0f;

        public float minEarthRadius = 0.5f;
        public float maxEarthRadius = 1.0f;

        public bool useHeatmapTextureForVertexColors;

        public Layer myLayer;

        protected GeographicGridTessellator tessellator;

        public DateTime dateCurrentlyShown;

        private readonly Ellipsoid globeEllipsoidType = Ellipsoid.ScaledWgs84;
        public int StackPartitions { get; set; }
        public int SlicePartitions { get; set; }

        //public bool averagingDataRangeBounds;

        // Counts how many tiles are contributing to each vertex, so that
        // averages can be adequately incorporated
        private int[] globeVertexContributionCount;

        public float colorAlpha;

        public float rotationalOffset;

        // Sets whether the default visualization (i.e., what the visualization looks like
        // before any actual downloaded data is applied) is a large, full radius sphere, as opposed to
        // a small, minimum radius sphere
        public bool defaultFullSizeGlobe;

        // Globe is transparent by default
        public bool transparentByDefault;

        private void Start()
        {
            StackPartitions = 100;
            SlicePartitions = 100;
            Reset();
            if (globe == null)
            {
                globe = FindObjectOfType<AbstractGlobe>();
            }
            transform.Rotate(Vector3.forward, rotationalOffset);
        }

        private void Update()
        {
            if (transformToWrap != null)
            {
                sphereRadius = transformToWrap.GetComponent<SphereCollider>().radius;
                transform.position = transformToWrap.position;
            }
        }

        private Mesh GetFreshGlobeMesh()
        {
            Mesh mesh;
            // Generate a big ol' globe mesh!
            if (tessellator == null)
            {
                tessellator = new GeographicGridTessellator();
            }


            if (defaultFullSizeGlobe)
            {
                Ellipsoid scaledEllipsoid = new Ellipsoid(maxEarthRadius * globeEllipsoidType.Radii.x,
                    maxEarthRadius * globeEllipsoidType.Radii.y, maxEarthRadius * globeEllipsoidType.Radii.z);
                mesh = tessellator.GenerateGlobe(scaledEllipsoid, StackPartitions, SlicePartitions);
            }
            else
            {
                Ellipsoid scaledEllipsoid = new Ellipsoid(minEarthRadius * globeEllipsoidType.Radii.x,
                    minEarthRadius * globeEllipsoidType.Radii.y, minEarthRadius * globeEllipsoidType.Radii.z);
                mesh = tessellator.GenerateGlobe(scaledEllipsoid, StackPartitions, SlicePartitions);
            }

            mesh.name = "Deformed Globe";

            // Add vertex colors
            Color[] colors = new Color[mesh.vertexCount];

            for (int i = 0; i < colors.Length; i++)
            {
                if (transparentByDefault)
                {
                    colors[i] = Color.clear;
                }
                else
                {
                    colors[i] = Color.white;
                }
            }

            mesh.colors = colors;

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }

        public override void Reset()
        {
            Mesh mesh = GetFreshGlobeMesh();
            GetComponent<MeshFilter>().mesh = mesh;

            // Make an entry of 0 for each vertex

            if (globeVertexContributionCount == null)
            {
                // Initialize
                globeVertexContributionCount = new int[mesh.vertexCount];
            }
            else
            {
                // Zero out
                for (int i = 0; i < globeVertexContributionCount.Length; i++)
                {
                    globeVertexContributionCount[i] = 0;
                }
            }

            // Reset for new layer
            myLayer = null;
        }

        // When a new tile has been downloaded, use its texture and properties to calculate
        // a deformation which is then applied to the mesh attached to this object
        public override IEnumerator DeformBasedOnNewTile(Tile tile, float min, float max)
        {
            // Ensure that Globe has been set up properly before any deformations occur
            if (tessellator == null)
            {
                yield return null;
            }

            float minValue = min;
            float maxValue = max;

            // Values for colormap translation
            float heightRange = maxValue - minValue;
            float colorToHeightRStep = heightRange / 255.0f;
            float colorToHeightGStep = colorToHeightRStep / 255.0f;
            float colorToHeightBStep = colorToHeightGStep / 255.0f;

            Mesh mesh = GetComponent<MeshFilter>().mesh;

            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;
            Color[] colors = mesh.colors;

            // Get a list of vertices to edit
            VertexAndUvContainer vertexAndUvContainer =
                tessellator.LatLongBoundingBoxToGlobeVertexListAndUvs(tile.bBox, StackPartitions, SlicePartitions);
            List<int> vertexIndicesToEdit = vertexAndUvContainer.vertexIndices;
            List<Vector2> textureUvs = vertexAndUvContainer.uvs;

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // Cycle through points and deform according to the tile

            int vertsToProcessPerFrame = vertexIndicesToEdit.Count / 10; //numFramesToSpan;
            int vertsProcessedThisFrame = 0;
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            for (int i = 0; i < vertexIndicesToEdit.Count; i++)
            {
                int vertexIndex = vertexIndicesToEdit[i];

                Color pixelColor = tile.scienceDataTexture.GetPixelBilinear(textureUvs[i].x, textureUvs[i].y);

                float value = ColormappedPixelColorToScienceValue(pixelColor, minValue, colorToHeightRStep,
                    colorToHeightGStep, colorToHeightBStep);

                if (value > 0.0f)
                {
                    int contributionsAlreadyMadeToThisVertex = 0;

                    try
                    {
                        contributionsAlreadyMadeToThisVertex = globeVertexContributionCount[vertexIndex];
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("Exception " + e.Message + " Was trying to get element " + vertexIndex + " of " + globeVertexContributionCount.Length + " elements!!");
                    }

                    float percentageThroughRange = (value - minValue) / (maxValue - minValue);
                    float vertexHeight = minEarthRadius + percentageThroughRange * (maxEarthRadius - minEarthRadius);

                    if (contributionsAlreadyMadeToThisVertex > 0)
                    {
                        // Average with other values
                        float currentVertexHeight = vertices[vertexIndex].magnitude;
                        float averageVertexHeight =
                            (currentVertexHeight * contributionsAlreadyMadeToThisVertex + vertexHeight) /
                            ((float) contributionsAlreadyMadeToThisVertex + 1);
                        vertices[vertexIndex] = vertices[vertexIndex].normalized * averageVertexHeight;
                    }
                    else
                    {
                        vertices[vertexIndex] = vertices[vertexIndex].normalized * vertexHeight;
                    }
                    colors[vertexIndex] = pixelColor;

                    // Adjust alpha
                    colors[vertexIndex].a = colorAlpha;

                    globeVertexContributionCount[vertexIndex] = globeVertexContributionCount[vertexIndex] + 1;

                    if (vertsProcessedThisFrame > vertsToProcessPerFrame)
                    {
                        vertsProcessedThisFrame = 0;
                        yield return null;
                    }
                }
            }

            // Finally, made a record of this contribution
            mesh.Clear();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.colors = colors;

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }



        public override void GenerateVisualization()
        {
        }
        private static float ColormappedPixelColorToScienceValue(Color pixelColor, float minHeight, float rStep, float gStep, float bStep)
        {
            float height = minHeight + (pixelColor.r * rStep + pixelColor.g * gStep + pixelColor.b * bStep) * 255.0f;
            return height;
        }

    }
}
