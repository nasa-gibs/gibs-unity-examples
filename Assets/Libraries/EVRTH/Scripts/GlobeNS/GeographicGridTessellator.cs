using System.Collections.Generic;
using EVRTH.Scripts.Geometry;
using EVRTH.Scripts.WMS;
using UnityEngine;

namespace EVRTH.Scripts.GlobeNS
{
    /// <summary>
    /// Based on Geographic Grid Ellipsoid Tessellator from "3D Engine Design for Virtual Globes", section 4.1.4, but modified to generate a
    /// separate mesh for tiles enclosed by lat/lon bounding boxes, instead of one mesh for the entire globe.
    /// </summary>
    public class GeographicGridTessellator
    {
        private const float skirtHeightFactor = 0.0025f;
        private const float eps = 1e-3f;

        public static Vector3 LatLonToPosition(LatLon latlon, Ellipsoid ellipsoid)
        {
            return LatLonToPosition(latlon.Latitude, latlon.Longitude, ellipsoid);
        }

        public static Vector3 LatLonToPosition(float lat, float lon, Ellipsoid ellipsoid)
        {
            // To avoid singularities at the poles, don't let the latitude quite reach 90 degrees
            lat = Mathf.Clamp(lat, -90 + eps, 90 - eps);

            // Phi is like latitude, but ranges from [0, pi]. Add 90 degrees and convert to radians to get phi.
            float phi = (lat + 90f) * Mathf.Deg2Rad;
            float sinPhi = Mathf.Sin(phi);

            // Generate positions on Ellipsoid following Eq. 4.4 from "3D Engine Design for Virtual Globes":
            // x = a cos(theta) sin(phi)
            // y = a sin(theta) sing(phi)
            // z = c cos(phi)
            float xSinPhi = ellipsoid.Radii.x * sinPhi;
            float ySinPhi = ellipsoid.Radii.y * sinPhi;
            float zCosPhi = ellipsoid.Radii.z * Mathf.Cos(phi);

            float theta = lon * Mathf.Deg2Rad;
            return new Vector3(Mathf.Cos(theta) * xSinPhi, Mathf.Sin(theta) * ySinPhi, zCosPhi);
        }

        // TODO may want to treat the poles as a tri-fan instead of a tri-strip, unless we do something more sophisticated to deal with the singularity at the poles.
        public Mesh GenerateSector(LatLonBoundingBox bbox, Ellipsoid ellipsoid, int numberOfStackPartitions, int numberOfSlicePartitions)
        {
            Mesh mesh = new Mesh();
            LatLonBoundingBox geometryBBox = bbox.Clamp();

            float deltaLat = geometryBBox.DeltaLat / numberOfStackPartitions;
            float deltaLon = geometryBBox.DeltaLon / numberOfSlicePartitions;

            // Create positions
            int numberOfVertices = NumberOfVertices(numberOfStackPartitions, numberOfSlicePartitions);
            List<Vector3> positions = new List<Vector3>(numberOfVertices);

            // Generates geometry
            for (int i = 0; i <= numberOfStackPartitions; i++)
            {
                float lat = i * deltaLat + geometryBBox.minLat;

                // To avoid singularities at the poles, don't let the latitude quite reach 90 degrees
                lat = Mathf.Clamp(lat, -90 + eps, 90 - eps);

                // Phi is like latitude, but ranges from [0, pi]. Add 90 degrees and convert to radians to get phi.
                float phi = (lat + 90f) * Mathf.Deg2Rad;
                float sinPhi = Mathf.Sin(phi);

                // Generate positions on Ellipsoid following Eq. 4.4 from "3D Engine Design for Virtual Globes":
                // x = a cos(theta) sin(phi)
                // y = a sin(theta) sing(phi)
                // z = c cos(phi)
                float xSinPhi = ellipsoid.Radii.x * sinPhi;
                float ySinPhi = ellipsoid.Radii.y * sinPhi;
                float zCosPhi = ellipsoid.Radii.z * Mathf.Cos(phi);

                for (int j = 0; j <= numberOfSlicePartitions; j++)
                {
                    float lon = j * deltaLon + geometryBBox.minLon;

                    float theta = lon * Mathf.Deg2Rad;
                    Vector3 pos = new Vector3(Mathf.Cos(theta) * xSinPhi, Mathf.Sin(theta) * ySinPhi, zCosPhi);
                    positions.Add(pos);
                }
            }

            // Compute normals & texture coordinates
            List<Vector3> normals = new List<Vector3>(numberOfVertices);
            List<Vector2> uvs = new List<Vector2>(numberOfVertices);
            for (int i = 0; i < positions.Count; i++)
            {
                Vector3 normal = ellipsoid.GeodeticSurfaceNormal(positions[i]);

                normals.Add(normal);
                uvs.Add(GetUvFromNormal(geometryBBox, bbox, ellipsoid, normal));
            }

            // Compute triangle indices
            List<int> indices = new List<int>(NumberOfTriangles(numberOfSlicePartitions, numberOfStackPartitions) * 3);

            // Generate a triangle strip for each row
            for (int i = 0; i < numberOfStackPartitions; ++i)
            {
                int topRowOffset = i * (numberOfSlicePartitions + 1);
                int bottomRowOffset = (i + 1) * (numberOfSlicePartitions + 1);

                for (int j = 0; j < numberOfSlicePartitions; ++j)
                {
                    indices.Add(bottomRowOffset + j);
                    indices.Add(bottomRowOffset + j + 1);
                    indices.Add(topRowOffset + j + 1);

                    indices.Add(bottomRowOffset + j);
                    indices.Add(topRowOffset + j + 1);
                    indices.Add(topRowOffset + j);
                }
            }

            List<Color> colors = new List<Color>();
            while (colors.Count < positions.Count)
            {
                colors.Add(Color.white);
            }

            // Generate skirt geometry to help hide cracks between adjacent levels of detail
            AddSkirt(positions, normals, uvs, indices, numberOfStackPartitions, numberOfSlicePartitions, ellipsoid.Radii.x * skirtHeightFactor);
	
            // Add colors verts for the skirt so we can fade it on if needed
            while (colors.Count < positions.Count)
            {
                colors.Add(new Color(0,0,0,0));
            }

            mesh.SetVertices(positions);
            mesh.SetColors(colors);
            mesh.uv = uvs.ToArray();
            mesh.SetNormals(normals);
            mesh.triangles = indices.ToArray();

            return mesh;
        }

        /// <summary>
        /// Get the texture coordinates for the globe point with a given normal.
        /// </summary>
        /// <returns>A 2D vector containing the texture coordinates corresponding to the given normal vector.</returns>
        public static Vector2 GetUvFromNormal(LatLonBoundingBox geometryBBox, LatLonBoundingBox textureBBox,
            Ellipsoid ellipsoid, Vector3 normal)
        {
            // Compute texture coordinates on a full globe texture
            Vector2 tex = ComputeGlobalTextureCoordinate(normal);

            Vector2 minTexCoords = Vector2.zero;
            Vector2 maxTexCoords = Vector2.one;

            Vector2 deltaTexCoord = new Vector2(1.0f, 1.0f);

            if (geometryBBox != null)
            {
                minTexCoords = ComputeGlobalTextureCoords(geometryBBox.Min, ellipsoid);
                maxTexCoords = ComputeGlobalTextureCoords(geometryBBox.Max, ellipsoid);

                // If min or max longitude is too close to the international date line then clamp the texture coordinate to 0 or 1.
                if (geometryBBox.minLon < -180f + eps)
                {
                    minTexCoords.x = 0f;
                }
                if (geometryBBox.maxLon > 180f - eps)
                {
                    maxTexCoords.x = 1f;
                }

                deltaTexCoord = maxTexCoords - minTexCoords;

                // Handle wrap around issues at the dateline
                if (tex.x > maxTexCoords.x + eps || tex.x < minTexCoords.x - eps)
                {
                    tex.x = 1 - tex.x;
                }
            }

            // Take the full globe texture coords and scale to the region of the globe covered by this tile.
            float u = (tex - minTexCoords).x / deltaTexCoord.x;
            float v = (tex - minTexCoords).y / deltaTexCoord.y;

            // If the texture bounding box is different than the geometry bounding box
            // (for example, because extent of the texture extends beyond the valid lat-lon range),
            // then we need to scale the tex coords to use only the valid part of the texture.
            // This operation has no effect if they bounding boxes are equal.
            if (geometryBBox != null && textureBBox != null)
            {
                u = u * geometryBBox.DeltaLon / textureBBox.DeltaLon;
                v = v * geometryBBox.DeltaLat / textureBBox.DeltaLat;
            }

            return new Vector2(u, v);
        }

        /// <summary>
        /// Generate geometry for an ellipsoid globe.
        /// </summary>
        /// <param name="ellipsoid">Ellipsoid model to use to generate geometry</param>
        /// <param name="numberOfStackPartitions">Number of horizontal tessellations</param>
        /// <param name="numberOfSlicePartitions">Number of vertical tessellations</param>
        /// <returns></returns>
        // TODO this method is mostly redundant with GenerateSector. See if we can consolidate this code
        public Mesh GenerateGlobe(Ellipsoid ellipsoid, int numberOfStackPartitions, int numberOfSlicePartitions)
        {
            Mesh mesh = new Mesh();

            // Precompute sin/cos
            float[] cosTheta = new float[numberOfSlicePartitions];
            float[] sinTheta = new float[numberOfSlicePartitions];

            for (int i = 0; i < numberOfSlicePartitions; ++i)
            {
                float theta = 2 * Mathf.PI * ((float)i / numberOfSlicePartitions);
                cosTheta[i] = Mathf.Cos(theta);
                sinTheta[i] = Mathf.Sin(theta);
            }

            // Create positions
            List<Vector3> positions = new List<Vector3> {new Vector3(0, 0, ellipsoid.Radii.z)};

            // Add top vertex

            // For each latitude stack...
            for (int i = 1; i < numberOfStackPartitions; ++i)
            {
                float phi = Mathf.PI * ((float)i / numberOfStackPartitions);
                float sinPhi = Mathf.Sin(phi);

                float xSinPhi = ellipsoid.Radii.x * sinPhi;
                float ySinPhi = ellipsoid.Radii.y * sinPhi;
                float zCosPhi = ellipsoid.Radii.z * Mathf.Cos(phi);

                // For each longitude slice, add a position
                for (int j = 0; j < numberOfSlicePartitions; ++j)
                {
                    positions.Add(new Vector3(cosTheta[j] * xSinPhi, sinTheta[j] * ySinPhi, zCosPhi));
                }
            }
            // Add bottom vertex
            positions.Add(new Vector3(0, 0, -ellipsoid.Radii.z));

            // Compute normals and UVs
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();

            for (int i = 0; i < positions.Count; i++)
            {
                Vector3 normal = ellipsoid.GeodeticSurfaceNormal(positions[i]);
                normals.Add(normal);
                uvs.Add(GetUvFromNormal(null, null, ellipsoid, normal));
            }

            // Compute triangle indices
            List<int> indices = new List<int>(NumberOfTriangles(numberOfSlicePartitions, numberOfStackPartitions) * 3);

            // Triangle fan top row
            for (int j = 1; j < numberOfSlicePartitions; ++j)
            {
                indices.Add(0);
                indices.Add(j);
                indices.Add(j + 1);
            }
            indices.Add(0);
            indices.Add(numberOfStackPartitions);
            indices.Add(1);

            // Middle rows are triangle strips
            for (int i = 0; i < numberOfStackPartitions - 2; ++i)
            {
                int topRowOffset = i * numberOfSlicePartitions + 1;
                int bottomRowOffset = (i + 1) * numberOfSlicePartitions + 1;

                for (int j = 0; j < numberOfSlicePartitions - 1; ++j)
                {
                    indices.Add(bottomRowOffset + j);
                    indices.Add(bottomRowOffset + j + 1);
                    indices.Add(topRowOffset + j + 1);

                    indices.Add(bottomRowOffset + j);
                    indices.Add(topRowOffset + j + 1);
                    indices.Add(topRowOffset + j);
                }
                indices.Add(bottomRowOffset + numberOfSlicePartitions - 1);
                indices.Add(bottomRowOffset);
                indices.Add(topRowOffset);

                indices.Add(bottomRowOffset + numberOfSlicePartitions - 1);
                indices.Add(topRowOffset);
                indices.Add(topRowOffset + numberOfSlicePartitions - 1);
            }

            // Triangle fan bottom row
            int lastPosition = positions.Count - 1;
            for (int j = lastPosition - 1; j > lastPosition - numberOfSlicePartitions; --j)
            {
                indices.Add(lastPosition);
                indices.Add(j);
                indices.Add(j - 1);
            }
            indices.Add(lastPosition);
            indices.Add(lastPosition - numberOfSlicePartitions);
            indices.Add(lastPosition - 1);

            mesh.SetVertices(positions);
            mesh.uv = uvs.ToArray();
            mesh.SetNormals(normals);
            mesh.triangles = indices.ToArray();

            return mesh;
        }

        // Gets a list of globe sphere vertices contained within a latlon bounding box
        public VertexAndUvContainer LatLongBoundingBoxToGlobeVertexListAndUvs(LatLonBoundingBox bbox, int numberOfStackPartitions, int numberOfSlicePartitions)
        {
            // Get the fraction of the range (from 0.0 to 1.0) that this latlon bbox covers for latitude and longitude
            float latitudeStartFraction = (bbox.minLat + 90.0f) / 180.0f;
            float latitudeEndFraction = (bbox.maxLat + 90.0f) / 180.0f;
            float longitudeStartFraction = (bbox.minLon + 180.0f) / 360.0f;
            float longitudeEndFraction = (bbox.maxLon + 180.0f) / 360.0f;

            List<int> vertexIndicesOfInterest = new List<int>();
            List<Vector2> textureUvsForVertexIndicesOfInterest = new List<Vector2>();

            // Add top/bottom vertices if necessary
            if (latitudeStartFraction == 0.0f)
            {
                // Add top vertex index
                vertexIndicesOfInterest.Add(0);
                // Longitude is arbitrary
                textureUvsForVertexIndicesOfInterest.Add(new Vector2(0.0f, 0.0f));
            }

            if (latitudeEndFraction == 1.0f)
            {
                // Add bottom vertex index
                vertexIndicesOfInterest.Add((numberOfStackPartitions - 1) * numberOfSlicePartitions + 2 - 1);
                // Longitude is arbitrary
                textureUvsForVertexIndicesOfInterest.Add(new Vector2(1.0f, 1.0f));
            }

            // Go from the first latitude stack to the last one covered by this fraction
            for (int latitudeStackIndex = Mathf.CeilToInt(latitudeStartFraction * (numberOfStackPartitions - 2.0f)); latitudeStackIndex <= Mathf.FloorToInt(latitudeEndFraction * (numberOfStackPartitions - 1.0f)); latitudeStackIndex++)
            {
                // Go from the first longitude slice to the last one covered by this fraction
                for (int longitudeSliceIndex = Mathf.CeilToInt(longitudeStartFraction * numberOfSlicePartitions); longitudeSliceIndex <= Mathf.FloorToInt(longitudeEndFraction * numberOfStackPartitions); longitudeSliceIndex++)
                {
                    // Offset of 1 because GeographicGridTessellator adds a single top vertex first...
                    int vertexIndex = 1 + (latitudeStackIndex - 1) * numberOfSlicePartitions + longitudeSliceIndex;

                    float latitudeOfThisVertex = latitudeStackIndex * (180.0f / (numberOfStackPartitions - 1.0f)) - 90.0f;
                    float longitudeOfThisVertex = longitudeSliceIndex * (360.0f / numberOfSlicePartitions) - 180.0f;

                    // Calculate the lat/long percentages within this box at which the vertex is located
                    float latPercent = (latitudeOfThisVertex - bbox.minLat) / bbox.DeltaLat;
                    float lonPercent = (longitudeOfThisVertex - bbox.minLon) / bbox.DeltaLon;

                    // Double-check that the vertex is included in the tile!
                    if (latPercent >= 0.0f && latPercent <= 1.0f && lonPercent >= 0.0f && lonPercent <= 1.0f && vertexIndex >= 0)
                    {
                        vertexIndicesOfInterest.Add(vertexIndex);
                        textureUvsForVertexIndicesOfInterest.Add(new Vector2(lonPercent, latPercent));
                    }
                }
            }

            VertexAndUvContainer vertexAndUvContainer =
                new VertexAndUvContainer
                {
                    vertexIndices = vertexIndicesOfInterest,
                    uvs = textureUvsForVertexIndicesOfInterest
                };

            return vertexAndUvContainer;
        }

        public static Vector2 ComputeGlobalTextureCoords(LatLon latlon, Ellipsoid ellipsoid)
        {
            Vector3 pos = LatLonToPosition(latlon, ellipsoid);
            Vector3 normal = ellipsoid.GeodeticSurfaceNormal(pos);

            return ComputeGlobalTextureCoordinate(normal);
        }

        public static Vector2 ComputeGlobalTextureCoordinate(Vector3 normal)
        {
            // Compute texture coordinates on a full globe texture in geographic projection using Eq. 4.5 from "3D Engine Design for Virtual Globes"
            float s = Mathf.Atan2(normal.y, normal.x) / (2 * Mathf.PI) + 0.5f;
            float t = 1 - (Mathf.Asin(normal.z) / Mathf.PI + 0.5f);

            return new Vector2(s, t);
        }

        private int NumberOfVertices(int numberOfSlicePartitions, int numberOfStackPartitions)
        {
            return (numberOfStackPartitions + 1) * (numberOfSlicePartitions + 1);
        }

        private int NumberOfTriangles(int numberOfSlicePartitions, int numberOfStackPartitions)
        {
            int numberOfTriangles = 2 * numberOfStackPartitions * numberOfSlicePartitions;
            return numberOfTriangles;
        }

        private IEnumerable<Pair<int>> GetEdges(int numberOfStackPartitions, int numberOfSlicePartitions)
        {
            // Bottom edge
            for (int i = 0; i < numberOfSlicePartitions; i++)
            {
                yield return new Pair<int>(i, i + 1);
            }

            // Top edge
            int offset = numberOfStackPartitions * (numberOfSlicePartitions + 1);
            for (int i = 0; i < numberOfSlicePartitions; i++)
            {
                yield return new Pair<int>(offset + i + 1, offset + i);
            }

            // Left edge
            int step = numberOfSlicePartitions + 1;
            for (int i = 0; i < numberOfStackPartitions; i++)
            {
                yield return new Pair<int>((i + 1) * step, i * step);
            }

            // Right edge
            step = numberOfSlicePartitions + 1;
            offset = numberOfSlicePartitions;
            for (int i = 0; i < numberOfStackPartitions; i++)
            {
                yield return new Pair<int>(i * step + offset, (i + 1) * step + offset);
            }
        }

        /// <summary>
        /// Add a vertical skirt around the tile. This method assumes a regular triangular tessellation to find the edges of the tile.
        /// </summary>
        /// <param name="positions">Mesh positions. Skirt positions will be added to this list.</param>
        /// <param name="normals">Mesh normals. Skirt normals will be added to this list.</param>
        /// <param name="uvs">Mesh texture coordinates. Skirt uvs will be added to this list.</param>
        /// <param name="indices">Mesh triangles. Skirt triangles will be added to this list.</param>
        /// <param name="numberOfStackPartitions">Number of vertical partitions in the tessellated tile.</param>
        /// <param name="numberOfSlicePartitions">Number of horizontal partitions in the tessellated tile.</param>
        /// <param name="height">Height of the skirt.</param>
        private void AddSkirt(List<Vector3> positions, List<Vector3> normals, List<Vector2> uvs, List<int> indices,
            int numberOfStackPartitions, int numberOfSlicePartitions, float height)
        {
            // TODO this method generates duplicate vertices for the mesh. We could make this a little smarter by reusing verts already in the mesh,
            // TODO and adding the minimum number of verts to form the skirt.

            int vertCount = positions.Count;
            foreach (Pair<int> segment in GetEdges(numberOfStackPartitions, numberOfSlicePartitions))
            {
                Vector3 v0 = positions[segment.item1];
                Vector3 v1 = positions[segment.item2];

                positions.Add(v0);
                positions.Add(v1);

                // Move vertices toward the center of the globe
                v0 += height * -normals[segment.item1];
                v1 += height * -normals[segment.item2];

                positions.Add(v0);
                positions.Add(v1);

                // Duplicate normals and texture coordinates
                for (int i = 0; i < 2; i++)
                {
                    uvs.Add(uvs[segment.item1]);
                    uvs.Add(uvs[segment.item2]);

                    normals.Add(normals[segment.item1]);
                    normals.Add(normals[segment.item2]);
                }

                indices.Add(vertCount + 1);
                indices.Add(vertCount + 2);
                indices.Add(vertCount);

                indices.Add(vertCount + 2);
                indices.Add(vertCount + 1);
                indices.Add(vertCount + 3);

                vertCount += 4;
            }
        }
    }
}
