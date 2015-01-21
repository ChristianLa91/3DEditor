#region File Description
//-----------------------------------------------------------------------------
// SpherePrimitive.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace TrianglePicking
{
    /// <summary>
    /// Geometric primitive class for drawing spheres.
    /// </summary>
    public class SpherePrimitive
    {
        VertexModelMesh mVertexModelMesh;
        Color           mColor;

        /// <summary>
        /// Constructs a new sphere primitive,
        /// with the specified size and tessellation level.
        /// </summary>
        public SpherePrimitive(VertexModelMesh vertexModelMesh,
                               float diameter, int tessellation, Color color)
        {
            mVertexModelMesh = vertexModelMesh;
            mColor           = color;

            if (tessellation < 3)
                throw new ArgumentOutOfRangeException("tessellation");

            int verticalSegments = tessellation;
            int horizontalSegments = tessellation * 2;

            float radius = diameter / 2;

            // Start with a single vertex at the bottom of the sphere.
            mVertexModelMesh.AddVertex(Vector3.Down * radius, mColor, new Vector2());

            // Create rings of vertices at progressively higher latitudes.
            for (int i = 0; i < verticalSegments - 1; i++)
            {
                float latitude = ((i + 1) * MathHelper.Pi /
                                            verticalSegments) - MathHelper.PiOver2;

                float dy = (float)Math.Sin(latitude);
                float dxz = (float)Math.Cos(latitude);

                // Create a single ring of vertices at this latitude.
                for (int j = 0; j < horizontalSegments; j++)
                {
                    float longitude = j * MathHelper.TwoPi / horizontalSegments;

                    float dx = (float)Math.Cos(longitude) * dxz;
                    float dz = (float)Math.Sin(longitude) * dxz;

                    Vector3 normal = new Vector3(dx, dy, dz);

                    mVertexModelMesh.AddVertex(normal * radius, mColor, new Vector2());
                }
            }

            // Finish with a single vertex at the top of the sphere.
            mVertexModelMesh.AddVertex(Vector3.Up * radius, mColor, new Vector2());

            // Create a fan connecting the bottom vertex to the bottom latitude ring.
            for (int i = 0; i < horizontalSegments; i++)
            {
                mVertexModelMesh.AddIndexData((short)0);
                mVertexModelMesh.AddIndexData((short)(1 + (i + 1) % horizontalSegments));
                mVertexModelMesh.AddIndexData((short)(1 + i));
            }

            // Fill the sphere body with triangles joining each pair of latitude rings.
            for (int i = 0; i < verticalSegments - 2; i++)
            {
                for (int j = 0; j < horizontalSegments; j++)
                {
                    int nextI = i + 1;
                    int nextJ = (j + 1) % horizontalSegments;

                    mVertexModelMesh.AddIndexData((short)(1 + i * horizontalSegments + j));
                    mVertexModelMesh.AddIndexData((short)(1 + i * horizontalSegments + nextJ));
                    mVertexModelMesh.AddIndexData((short)(1 + nextI * horizontalSegments + j));
                    
                    mVertexModelMesh.AddIndexData((short)(1 + i * horizontalSegments + nextJ));
                    mVertexModelMesh.AddIndexData((short)(1 + nextI * horizontalSegments + nextJ));
                    mVertexModelMesh.AddIndexData((short)(1 + nextI * horizontalSegments + j));
                }
            }

            int currentVertex = mVertexModelMesh.GetVertexVectors().Length;

            // Create a fan connecting the top vertex to the top latitude ring.
            for (int i = 0; i < horizontalSegments; i++)
            {
                mVertexModelMesh.AddIndexData((short)(currentVertex - 1));
                mVertexModelMesh.AddIndexData((short)(currentVertex - 2 - (i + 1) % horizontalSegments));
                mVertexModelMesh.AddIndexData((short)(currentVertex - 2 - i));
            }
        }
    }
}
