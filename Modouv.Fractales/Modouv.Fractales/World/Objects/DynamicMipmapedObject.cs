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
    public class DynamicMipmapedObject : IObject3D
    {
        #region Variables
        /// <summary>
        /// Données de dessin du modèle.
        /// </summary>
        ModelMipmap m_model;
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
        /// <summary>
        /// Indique si l'objet a été dessiné au cours de la dernière frame.
        /// </summary>
        bool m_lastFrameDrawn;
        #endregion
        /// <summary>
        /// Obtient ou définit le Model utilisé pour dessiner cet objet.
        /// </summary>
        public ModelMipmap Model
        {
            get { return m_model; }
            set
            {
                m_model = value;
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
        /// Obtient ou définit la qualité max du modèle quand l'ajustement de la qualité est 
        /// automatique.
        /// </summary>
        public ModelMipmap.ModelQuality MinModelAutoQuality
        {
            get;
            set;
        }
        /// <summary>
        /// Si vrai active le frustrum culling pour cet objet.
        /// </summary>
        public bool CullingEnabled { get; set; }
        int i = 0;

        #region BoundingBox
        BoundingBox m_boundingBox;
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
        public bool DebugView
        {
            get;
            set;
        }
        #endregion
        
        /// <summary>
        /// Dessine l'objet dans le monde donné.
        /// </summary>
        /// <param name="world"></param>
        public void Draw(GameWorld world)
        {
            Draw(world, false);
        }
        /// <summary>
        /// Dessine l'objet dans le monde donné.
        /// </summary>
        public void Draw(GameWorld world, bool useLastDrawCulling)
        {
            // Crée la bounding box de l'objet.
            BoundingBox aabb = GetTransformedBoundingBox();
            var bsphere = BoundingSphere.CreateFromBoundingBox(aabb);

            // Détermine la qualité de l'objet.
            ModelMipmap.ModelQuality quality = ModelMipmap.ModelQuality.Low;
            float distThresholdHigh     = (world.FarPlane - world.NearPlane) / 8;
            float distThresholdMedium   = (world.FarPlane - world.NearPlane) / 4;
            float distance = Vector3.Distance(world.Camera.Position, bsphere.Center) - bsphere.Radius;
            if (distance < distThresholdHigh || MinModelAutoQuality == ModelMipmap.ModelQuality.High)
                quality = ModelMipmap.ModelQuality.High;
            else if (distance < distThresholdMedium || MinModelAutoQuality == ModelMipmap.ModelQuality.Medium)
                quality = ModelMipmap.ModelQuality.Medium;

            // Récupère les transformations et mets en place le shader
            Matrix worldMatrix = GetTransform();
            world.SetupShader(Shader, worldMatrix);

            bool draw = true;
            // Si le culling est activé, on regarde si oui ou non l'objet doit être dessiné.
            if (CullingEnabled)
            {
                draw = useLastDrawCulling ? m_lastFrameDrawn : (world.GetFrustrum().Contains(bsphere) != ContainmentType.Disjoint);
                if (DebugView)
                {
                    Debug.Renderers.BoundingBoxRenderer.Render(aabb, Game1.Instance.GraphicsDevice, world.View, world.Projection, Color.Red);
                    Debug.Renderers.BoundingSphereRenderer.Render(bsphere, Game1.Instance.GraphicsDevice, world.View, world.Projection, Color.White);
                }
            }
            else if (DebugView)
            {
                Debug.Renderers.BoundingBoxRenderer.Render(aabb, Game1.Instance.GraphicsDevice, world.View, world.Projection, Color.Red);
                Debug.Renderers.BoundingSphereRenderer.Render(bsphere, Game1.Instance.GraphicsDevice, world.View, world.Projection, Color.White);
            }

            // Indique pour les frames suivantes que l'objet a été dessiné.
            m_lastFrameDrawn = draw;

            // Si l'objet doit être dessiné, on le dessine.
            if (draw)
            {
                Game1.Instance.GraphicsDevice.SetVertexBuffer(Model[quality].Vertices);
                Game1.Instance.GraphicsDevice.Indices = Model[quality].Indices;
                Shader.CurrentTechnique.Passes[0].Apply();
                Game1.Instance.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                    0, 0,
                    Model[quality].Vertices.VertexCount, 0, Model[quality].Indices.IndexCount / 3);
            }
        }
        /// <summary>
        /// Dessine l'objet avec la qualité donnée.
        /// </summary>
        /// <param name="quality">Qualité avec laquelle dessiner le monde</param>
        /// <param name="useLastFrameCulling">Si vrai, ne recalcule pas le culling de cet objet, et utilise les résultats du dernier appel
        /// pour décider si l'objet doit être dessiné. A utiliser pour économiser du CPU.</param>
        public void Draw(GameWorld world, ModelMipmap.ModelQuality quality, bool useLastDrawCulling=false)
        {
            // Crée la bounding box de l'objet.
            BoundingBox aabb = GetTransformedBoundingBox();
            var bsphere = BoundingSphere.CreateFromBoundingBox(aabb);

            // Récupère les transformations et mets en place le shader
            Matrix worldMatrix = GetTransform();
            world.SetupShader(Shader, worldMatrix);

            bool draw = true;
            // Si le culling est activé, on regarde si oui ou non l'objet doit être dessiné.
            if (CullingEnabled)
            {
                draw = useLastDrawCulling ? m_lastFrameDrawn : (world.GetFrustrum().Contains(bsphere) != ContainmentType.Disjoint);
                if (DebugView)
                {
                    Debug.Renderers.BoundingBoxRenderer.Render(aabb, Game1.Instance.GraphicsDevice, world.View, world.Projection, Color.White);
                    Debug.Renderers.BoundingSphereRenderer.Render(bsphere, Game1.Instance.GraphicsDevice, world.View, world.Projection, Color.White);
                }
            }
            else if (DebugView)
            {
                Debug.Renderers.BoundingBoxRenderer.Render(aabb, Game1.Instance.GraphicsDevice, world.View, world.Projection, Color.White);
                Debug.Renderers.BoundingSphereRenderer.Render(bsphere, Game1.Instance.GraphicsDevice, world.View, world.Projection, Color.White);
            }

            // Indique pour les frames suivantes que l'objet a été dessiné.
            m_lastFrameDrawn = draw;

            // Si l'objet doit être dessiné, on le dessine.
            if (draw)
            {
                Game1.Instance.GraphicsDevice.SetVertexBuffer(Model[quality].Vertices);
                Game1.Instance.GraphicsDevice.Indices = Model[quality].Indices;
                Shader.CurrentTechnique.Passes[0].Apply();
                Game1.Instance.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                    0, 0,
                    Model[quality].Vertices.VertexCount, 0, Model[quality].Indices.IndexCount / 3);
            }
        }
        /// <summary>
        /// Crée une nouvelle instance de Object3D.
        /// </summary>
        public DynamicMipmapedObject()
        {
            Model = new ModelMipmap();
            Scale = new Vector3(1, 1, 1);
            CullingEnabled = true;
        }

        #region Dispose
        /// <summary>
        /// Supprime les ressources non managées allouées par cet objet.
        /// </summary>
        public void Dispose()
        {
            m_model.Dispose();
        }
        #endregion
    }
}
