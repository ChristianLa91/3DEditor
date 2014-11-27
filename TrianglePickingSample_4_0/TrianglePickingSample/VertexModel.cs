using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace TrianglePicking
{
    class VertexModel
    {
        private List<VertexModelMesh> mVertexModelMeshes;
        private List<VertexModelBone> mVertexModelBones;
        private object mTag;

        public VertexModel()
        {
            mVertexModelMeshes  = new List<VertexModelMesh>();
            mVertexModelBones   = new List<VertexModelBone>();
            mTag                = new object();
        }

        public void AddMesh(VertexModelMesh vertexModelMesh)
        {
            mVertexModelMeshes.Add(vertexModelMesh);
        }

        public List<VertexModelMesh> GetMeshes()
        {
            return mVertexModelMeshes;
        }

        public void SetMeshes(List<VertexModelMesh> meshes)
        {
            mVertexModelMeshes = meshes;
        }

        public object Tag { get { return mTag; } set { mTag = value; } }

        public Microsoft.Xna.Framework.BoundingSphere GetBoundingSphere()
        {
            float xMin = float.MaxValue, xMax = float.MinValue, yMin = float.MaxValue, yMax = float.MinValue, zMin = float.MaxValue, zMax = float.MinValue;

            foreach (VertexModelMesh mesh in mVertexModelMeshes)
            {
                foreach (Vector3 vertex in mesh.GetVertexVectors())
                {
                    if (vertex.X < xMin)
                        xMin = vertex.X;
                    if (vertex.X > xMax)
                        xMax = vertex.X;
                    if (vertex.Y < yMin)
                        yMin = vertex.Y;
                    if (vertex.Y > yMax)
                        yMax = vertex.Y;
                    if (vertex.Z < zMin)
                        zMin = vertex.Z;
                    if (vertex.Z > zMax)
                        zMax = vertex.Z;
                }
            }

            BoundingSphere boundingSphere = new BoundingSphere(new Vector3(xMin + (xMax - xMin) / 2, yMin + (yMax - yMin) / 2, zMin + (zMax - zMin) / 2), MathHelper.Max(MathHelper.Max((xMax - xMin) / 2, (yMax - yMin) / 2), (zMax - zMin) / 2) * 2f);
            return boundingSphere;
        }

        public VertexModel Clone()
        {
            VertexModel vertexModelClone = new VertexModel();
            List<VertexModelMesh> vertexMeshesClone = new List<VertexModelMesh>();

            VertexModelMesh vertexModelMeshClone;

            foreach (VertexModelMesh vertexModelMesh in mVertexModelMeshes)
            {
                vertexModelMeshClone = vertexModelMesh.Clone();
                vertexMeshesClone.Add(vertexModelMeshClone);
            }

            vertexModelClone.SetMeshes(vertexMeshesClone);

            return vertexModelClone;
        }
    }
}
