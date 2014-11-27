using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TrianglePicking
{
    class VertexModelMesh
    {
        private VertexPositionColorTexture[] mVertexDataArray = new VertexPositionColorTexture[1];
        private int mVertexDataLenght = 0;
        private Texture2D mTexture;

        private short[] mIndexDataArray = new short[1];
        private int mIndexDataLenght = 0;

        private List<BasicEffect> mBasicEffects = new List<BasicEffect>();

        // Custom rasterizer state for drawing in wireframe.
        static RasterizerState WireFrame = new RasterizerState
        {
            FillMode = FillMode.WireFrame,
            CullMode = CullMode.CullCounterClockwiseFace
        };

        public VertexModelMesh()
        {
            
        }

        public VertexModelMesh(Texture2D texture)
        {
            mTexture = texture;
        }

        public void AddVertex(Vector3 position, Color color, Vector2 textureCoordinate)
        {
            if (mVertexDataLenght > 0)
            {
                Array.Resize(ref mVertexDataArray, mVertexDataArray.Length + 1);
            }

            mVertexDataArray[mVertexDataArray.Length - 1] = new VertexPositionColorTexture(position, color, textureCoordinate);

            mVertexDataLenght++;
        }

        public void AddIndexData(Int16 index)
        {
            if (mIndexDataLenght > 0)
            {
                Array.Resize(ref mIndexDataArray, mIndexDataArray.Length + 1);
            }

            mIndexDataArray[mIndexDataArray.Length - 1] = index;

            mIndexDataLenght++;
        }

        public void AddEffect(BasicEffect effect)
        {
            mBasicEffects.Add(effect);
        }

        public void SetTexture(Texture2D texture)
        {
            mTexture = texture;
        }

        public List<BasicEffect> GetEffects()
        {
            return mBasicEffects;
        }

        public Vector3[] GetVertexVectorsFromIndice()
        {
            Vector3[] vector3Array = new Vector3[mIndexDataArray.Length];
            for (int i = 0; i < vector3Array.Length; i++)
                vector3Array[i] = mVertexDataArray[mIndexDataArray[i]].Position;
            return vector3Array;
        }

        public Vector3[] GetVertexVectors()
        {
            Vector3[] vector3Array = new Vector3[mVertexDataArray.Length];
            for (int i = 0; i < vector3Array.Length; i++)
                vector3Array[i] = mVertexDataArray[i].Position;
            return vector3Array;
        }

        public void SetVertices(VertexPositionColorTexture[] vertices)
        {
            mVertexDataArray = vertices;
        }

        public short GetVertexIndex(int indiceIndex)
        {
            return mIndexDataArray[indiceIndex];
        }

        public void SetVertexIndice(short[] indice)
        {
            mIndexDataArray = indice;
        }

        public void SetVertex(int index, Vector3 position)
        {
            mVertexDataArray[mIndexDataArray[index]].Position = position;
        }

        public void Draw(GraphicsDevice device, BasicEffect effect)
        {
            // Activate the line drawing BasicEffect.
            effect.CurrentTechnique.Passes[0].Apply();

            // Draw textured box
            device.RasterizerState = RasterizerState.CullNone;  // vertex order doesn't matter
            device.BlendState = BlendState.NonPremultiplied;    // use alpha blending
            //device.DepthStencilState    = DepthStencilState.None;  // don't bother with the depth/stencil buffer

            device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, mVertexDataArray, 0, mVertexDataArray.Length, mIndexDataArray, 0, mIndexDataArray.Length/3);

            // Reset renderstates to their default values.
            device.RasterizerState = RasterizerState.CullCounterClockwise;
            device.DepthStencilState = DepthStencilState.Default;
        }

        public void DrawWireFrame(GraphicsDevice device, BasicEffect effect)
        {

            // Set line drawing renderstates. We disable backface culling
            // and turn off the depth buffer because we want to be able to
            // see the picked triangle outline regardless of which way it is
            // facing, and even if there is other geometry in front of it.
            device.RasterizerState = WireFrame;
            device.DepthStencilState = DepthStencilState.Default;

            // Activate the line drawing BasicEffect.
            effect.TextureEnabled = false;
            effect.VertexColorEnabled = false;
            
            effect.CurrentTechnique.Passes[0].Apply();

            device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, mVertexDataArray, 0, mVertexDataArray.Length, mIndexDataArray, 0, mIndexDataArray.Length / 3);

            // Reset renderstates to their default values.
            device.RasterizerState = RasterizerState.CullCounterClockwise;
            device.DepthStencilState = DepthStencilState.Default;

            effect.VertexColorEnabled = true;

            effect.CurrentTechnique.Passes[0].Apply();
        }

        public VertexModelMesh Clone()
        {
            VertexModelMesh vertexModelMeshCopy = new VertexModelMesh();
            VertexPositionColorTexture[] verticesClone = new VertexPositionColorTexture[mVertexDataArray.Length];
            short[] vertexIndiceClone = new short[mIndexDataArray.Length];
            
            for (int i = 0; i < verticesClone.Length; i++)
            {
                verticesClone[i] = new VertexPositionColorTexture(mVertexDataArray[i].Position, mVertexDataArray[i].Color, mVertexDataArray[i].TextureCoordinate);
            }

            for (int i = 0; i < vertexIndiceClone.Length; i++)
            {
                vertexIndiceClone[i] = mIndexDataArray[i];
            }

            vertexModelMeshCopy.SetVertices(verticesClone);
            vertexModelMeshCopy.SetVertexIndice(vertexIndiceClone);
            return vertexModelMeshCopy;
        }
    }
}
