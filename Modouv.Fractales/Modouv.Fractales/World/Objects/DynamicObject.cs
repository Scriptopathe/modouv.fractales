// Copyright (C) 2013, 2014 Alvarez Josué
//
// This code is free software; you can redistribute it and/or modify it
// under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2.1 of the License, or (at
// your option) any later version.
//
// This code is distributed in the hope that it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
// FITNESS FOR A PARTICULAR PURPOSE.  See the GNU Lesser General Public
// License (LICENSE.txt) for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with this library; if not, write to the Free Software Foundation,
// Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//
// The developer's email is jUNDERSCOREalvareATetudDOOOTinsa-toulouseDOOOTfr (for valid email, replace 
// capital letters by the corresponding character)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
namespace Modouv.Fractales.World.Objects
{
    /// <summary>
    /// Représente un objet 3D à dessiner.
    /// </summary>
    public class DynamicObject : IObject3D
    {
        #region Variables
        /// <summary>
        /// Données de dessin du modèle.
        /// </summary>
        ModelData m_model;
        /// <summary>
        /// Indique si la matrice de transformation obtenue par GetTransform() a besoin d'un refresh.
        /// </summary>
        bool m_transformNeedRefresh = true;
        Vector3 m_position;
        Vector3 m_rotation;
        Vector3 m_scale;
        /// <summary>
        /// Représente une matrice qui combine les transformations Scale, Rotation et Position.
        /// </summary>
        Matrix m_transform;
        /// <summary>
        /// Représente une matrice qui combine les transformations Scale et Position.
        /// Utilisée pour la transformation de la BoundingBox.
        /// </summary>
        Matrix m_partialTransform;
        #endregion
        /// <summary>
        /// Obtient ou définit le Model utilisé pour dessiner cet objet.
        /// </summary>
        public ModelData Model
        {
            get { return m_model; }
            set
            {
                m_model = value;

                if (m_model != null)
                {
                    // Calcule le Min/Max de la bounding box.
                    Vector3[] vertices = new Vector3[m_model.Vertices.VertexCount];
                    m_model.Vertices.GetData<Vector3>(vertices);
                    ComputeUntransformedBoundingBoxMinMax(vertices);

                    // On supprime la référence à vertices et on lance la Garbage Collection car on a potentiellement alloué
                    // quelques bon Mo !!
                    vertices = null;
                    GC.Collect();
                }
            }
        }
        /// <summary>
        /// Pixel shader du modèle.
        /// </summary>
        public Effect Shader { get; set; }
        /// <summary>
        /// Position du modèle.
        /// </summary>
        public Vector3 Position 
        {
            get { return m_position; }
            set { m_position = value; m_transformNeedRefresh = true; }
        }
        /// <summary>
        /// Rotation du modèle.
        /// </summary>
        public Vector3 Rotation 
        {
            get { return m_rotation; }
            set { m_rotation = value; m_transformNeedRefresh = true; }
        }
        /// <summary>
        /// Agrandissement du modèle.
        /// </summary>
        public Vector3 Scale
        {
            get { return m_scale; }
            set { m_scale = value; m_transformNeedRefresh = true; }
        }
        /// <summary>
        /// Si vrai active le frustrum culling pour cet objet.
        /// </summary>
        public bool CullingEnabled { get; set; }
        int i = 0;

        #region BoundingBox
        BoundingBox m_boundingBox;
        BoundingSphere m_boundingSphere;
        /// <summary>
        /// Détermine la position des vertices Min et Max de notre BoundingBox.
        /// Ces vertices sont non transformées mais elles sont générées une fois pour toute à partir du modèle.
        /// </summary>
        /// <param name="vertexData"></param>
        /// <param name="worldTransform"></param>
        /// <returns></returns>
        void ComputeUntransformedBoundingBoxMinMax(Vector3[] vertexData)
        {
            m_boundingBox = BoundingBox.CreateFromPoints(vertexData);
        }
        /// <summary>
        /// Force la bounding box aabb à être utilisée pour les collisions.
        /// </summary>
        /// <param name="aabb"></param>
        public void ForceCullingBoundingBox(BoundingBox aabb)
        {
            m_boundingBox = aabb;
        }
        /// <summary>
        /// Retourne la BoundingBox transformée de cette instance.
        /// </summary>
        /// <param name="world"></param>
        /// <returns></returns>
        public BoundingBox GetTransformedBoundingBox()
        {
            return new BoundingBox(m_boundingBox.Min * Scale + Position, m_boundingBox.Max * Scale + Position);
        }
        #endregion


        /// <summary>
        /// Retourne la matrice de transformation subie par cet objet 3D.
        /// </summary>
        /// <returns></returns>
        public Matrix GetTransform()
        {
            if (m_transformNeedRefresh)
            {
                m_transform = Matrix.CreateScale(Scale) * Matrix.CreateRotationX(Rotation.X) *
                              Matrix.CreateRotationY(Rotation.Y) * Matrix.CreateRotationZ(Rotation.Z) *
                              Matrix.CreateTranslation(Position);
                m_partialTransform = Matrix.CreateScale(Scale) * Matrix.CreateTranslation(Position);
                m_transformNeedRefresh = false;
            }
            return m_transform;
        }
        /// <summary>
        /// Retourne la matrice partielle (Scale + Position) de transformation subie par cet objet 3D.
        /// Utilisée pour la transformations de BoundingBox.
        /// </summary>
        /// <returns></returns>
        public Matrix GetPartialTransform() { GetTransform(); return m_partialTransform; }
        /// <summary>
        /// Retourne la bounding box non transformée de cet objet.
        /// </summary>
        /// <param name="world"></param>
        /// <returns></returns>
        public BoundingBox GetUntransformedBoundingBox(GameWorld world)
        {
            return new BoundingBox(m_boundingBox.Min, m_boundingBox.Max);
        }

        #region DEBUG
        short[] bBoxIndices = {
            0, 1, 1, 2, 2, 3, 3, 0, // Front edges
            4, 5, 5, 6, 6, 7, 7, 4, // Back edges
            0, 4, 1, 5, 2, 6, 3, 7 // Side edges connecting front and back
        };
        VertexBuffer bBoxBuffer;
        IndexBuffer bBoxIBuffer;
        void DEBUG_DrawBoundingBox(GameWorld world, BoundingBox box)
        {
            if (bBoxBuffer == null)
            {
                Vector3[] vertices = box.GetCorners();
                Generation.ModelGenerator.VertexPositionColorNormalTexture[] primitiveList = new Generation.ModelGenerator.VertexPositionColorNormalTexture[vertices.Length];

                // Crée les index et Vertex buffers.
                for (int i = 0; i < vertices.Length; i++)
                {
                    primitiveList[i] = new Generation.ModelGenerator.VertexPositionColorNormalTexture(vertices[i], Color.White, new Vector3(i/40.0f, i/40.0f, 0));
                }
                VertexBuffer buffer = new VertexBuffer(Game1.Instance.GraphicsDevice, Generation.ModelGenerator.VertexPositionColorNormalTexture.VertexDeclaration, vertices.Count(), BufferUsage.None);
                IndexBuffer iBuffer = new IndexBuffer(Game1.Instance.GraphicsDevice, IndexElementSize.SixteenBits, bBoxIndices.Count(), BufferUsage.None);
                buffer.SetData<Generation.ModelGenerator.VertexPositionColorNormalTexture>(primitiveList);
                iBuffer.SetData<short>(bBoxIndices);

                bBoxBuffer = buffer;
                bBoxIBuffer = iBuffer;
            }

            // Dessine la BBox.
            Game1.Instance.GraphicsDevice.SetVertexBuffer(bBoxBuffer);
            Game1.Instance.GraphicsDevice.Indices = bBoxIBuffer;
            Shader.Parameters["xWorldViewProjection"].SetValue(world.World);
            Shader.CurrentTechnique.Passes[0].Apply();
            Game1.Instance.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                0, 0,
                bBoxBuffer.VertexCount, 0, bBoxIndices.Count() / 3);
        }
        public bool DebugView
        {
            get;
            set;
        }
        #endregion
        /// <summary>
        /// Dessine le modèle à l'aide des matrices et du shader donnés.
        /// </summary>
        /// <param name="world"></param>
        /// <param name="projection"></param>
        /// <param name="view"></param>
        /// <param name="shader"></param>
        public void Draw(GameWorld world)
        {
            Matrix worldMatrix = GetTransform();
            world.SetupShader(Shader, worldMatrix);

            bool draw = true;
            // Si le culling est activé, on regarde si oui ou non l'objet doit être dessiné.
            if (CullingEnabled)
            {
                // Crée la bounding box de l'objet.
                BoundingBox aabb = GetTransformedBoundingBox();
                var bsphere = BoundingSphere.CreateFromBoundingBox(aabb);
                draw = (world.GetFrustrum().Contains(bsphere) != ContainmentType.Disjoint);
                if (DebugView)
                    Debug.Renderers.BoundingBoxRenderer.Render(aabb, Game1.Instance.GraphicsDevice, world.View, world.Projection, Color.Red);
            }
            else if (DebugView)
            {
                BoundingBox aabb = GetTransformedBoundingBox();
                Debug.Renderers.BoundingBoxRenderer.Render(aabb, Game1.Instance.GraphicsDevice, world.View, world.Projection, Color.Red);
            }


            // Si l'objet doit être dessiné, on le dessine.
            if (draw)
            {
                Game1.Instance.GraphicsDevice.SetVertexBuffer(Model.Vertices);
                Game1.Instance.GraphicsDevice.Indices = Model.Indices;
                Shader.CurrentTechnique.Passes[0].Apply();
                Game1.Instance.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                    0, 0,
                    Model.Vertices.VertexCount, 0, Model.Indices.IndexCount / 3);
            }
            else
            {

            }
        }
        
        /// <summary>
        /// Crée une nouvelle instance de Object3D.
        /// </summary>
        public DynamicObject()
        {
            Scale = new Vector3(1, 1, 1);
        }

        /// <summary>
        /// Crée une nouvelle instance de Object3D en spécifiant un modèle et un shader.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="shader"></param>
        public DynamicObject(ModelData model, Effect shader)
        {
            Model = model;
            Shader = shader;
            CullingEnabled = true;
        }

        #region Dispose
        /// <summary>
        /// Libère les ressources utilisées par cet objet.
        /// </summary>
        public void Dispose()
        {

        }
        #endregion
    }
}
