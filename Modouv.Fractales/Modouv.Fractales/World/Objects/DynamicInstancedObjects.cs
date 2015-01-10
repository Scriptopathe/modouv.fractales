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
    /// Type de vertex permettant de stocker une matrice de transformation et un offset de texture.
    /// </summary>
    public struct VertexInstance
    {

        public Matrix Matrix;
        public Vector2 TextureOffset;
        public Vector4 AdditionalData1;

        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            // World view projection
            new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 10),
            new VertexElement(sizeof(float) * 4, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 11),
            new VertexElement(sizeof(float) * 8, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 12),
            new VertexElement(sizeof(float) * 12, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 13),
            // Texture offset
            new VertexElement(sizeof(float) * 16, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 14),
            // Additional data
            new VertexElement(sizeof(float) * 18, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 15)
        );

        public VertexInstance(Matrix worldMatrix)
        {
            Matrix = Matrix.Transpose(worldMatrix);
            TextureOffset = Vector2.Zero;
            AdditionalData1 = Vector4.Zero;
        }

        public VertexInstance(Matrix worldMatrix, Vector4 additionalData1, Vector2 textureOffset)
        {
            Matrix = Matrix.Transpose(worldMatrix);
            TextureOffset = textureOffset;
            AdditionalData1 = additionalData1;
        }
    }
    public struct Transform
    {
        /// <summary>
        /// Position du modèle.
        /// </summary>
        public Vector3 Position { get; set;}
        /// <summary>
        /// Rotation du modèle.
        /// </summary>
        public Vector3 Rotation { get; set; }
        /// <summary>
        /// Agrandissement du modèle.
        /// </summary>
        public Vector3 Scale { get; set; }
        public Vector4 AdditionalData1;
        public Vector4 AdditionalData2;
        public Vector2 TextureOffset;
        public Matrix GetTransform()
        {
            return Matrix.CreateScale(Scale) * Matrix.CreateRotationX(Rotation.X) *
                Matrix.CreateRotationY(Rotation.Y) * Matrix.CreateRotationZ(Rotation.Z) *
                Matrix.CreateTranslation(Position);
        }
    }
    /// <summary>
    /// Représente une collection d'objets dynamiques pouvant bouger.
    /// Pour paramétrer correctement une instance de DynamicInstancedObjects, il faut :
    ///     - Affecter un modèle à DynamicInstancedObjects.Model.
    ///     - Donner une liste de transformations à effectuer à DynamicInstancedObjects.Transforms.
    ///     - Un état de rasterizer. (par défaut pas de culling) à DynamicInstancedObjects.RasterizerState.
    /// La liste des transformations peut être modifiée pendant l'exécution.
    /// </summary>
    public class DynamicInstancedObjects : IDisposable
    {
        #region Variables
        VertexBufferBinding[] m_bindings;
        /// <summary>
        /// Stocke les transformations effectuées sur chaque instance du modèle à dessiner.
        /// </summary>
        Transform[] m_transforms;
        /// <summary>
        /// Données de dessin du modèle.
        /// </summary>
        ModelData m_model;
        #endregion

        #region Properties
        /// <summary>
        /// Paramètre de dimmensionnement de la bounding box : permet d'ajuster la bounding box pour obtenir de meilleurs résultats.
        /// </summary>
        float BBScale { get; set; }
        /// <summary>
        /// Si vrai, active le frustrum culling pour les objets dessinés par cette collection.
        /// Si activé, le frustum culling est calculé pour CHAQUE instance du modèle dessinée.
        /// 
        /// Ne pas activer pour des groupes de taille trop grande.
        /// </summary>
        public bool InstanceCullingEnabled { get; set; }
        /// <summary>
        /// Définit ou obtient les données de dessin du modèle.
        /// /!\ Redéfinir le modèle effectue des opérations qui pompent du CPU (recalculer la bounding box du mesh).
        /// </summary>
        public ModelData Model
        {
            get { return m_model; }
            set
            {
                m_model = value;

                if (m_model != null)
                {
                    // Calcule la bounding box
                    ComputeUntransformedModelBoundingBox();

                    // On supprime la référence à vertices et on lance la Garbage Collection car on a potentiellement alloué
                    // quelques bon Mo !!
                    m_bindings[0] = new VertexBufferBinding(m_model.Vertices, 0);
                    GC.Collect();
                }
            }
        }
        /// <summary>
        /// Pixel shader du modèle.
        /// </summary>
        public Effect Shader { get; set; }
        /// <summary>
        /// Transformations de chaque objet 3D.
        /// </summary>
        public Transform[] Transforms
        {
            get { return m_transforms; }
            set
            {
                m_transforms = value;
            }
        }
        /// <summary>
        /// Instance buffer, contient les transformations associées à chaque "instance" du modèle.
        /// </summary>
        VertexBuffer m_instanceBuffer;
        #endregion

        #region Culling / Instancing
        const int CORES = 1;
        object _instanceCountmutex = new object();
        /// <summary>
        /// Nombre d'instances qui devront vraiment dessinées.
        /// </summary>
        int m_instanceCount;
        /// <summary>
        /// Variable partagée entre le threads de GenerateInstanceBuffer contenant les instances actuelles.
        /// </summary>
        VertexInstance[] _instances;
        void PerformCull(GameWorld world, int startIndex, int endIndex)
        {
            BoundingFrustum worldFrustum = world.GetFrustrum();
            // Ajoute les instances à la liste si elles doivent être dessinés.
            for (int i = startIndex; i < endIndex; i++)
            {
                bool draw = true;
                // Si le culling est activé, on regarde si oui ou non l'objet doit être dessiné.
                if (InstanceCullingEnabled)
                {
                    // Crée la bounding box de l'objet.
                    BoundingSphere aabb = GetTransformedModelBoundingSphere(Transforms[i]);
                    draw = worldFrustum.Contains(aabb) != ContainmentType.Disjoint;// || aabb.Contains(world.GetFrustrum()) != ContainmentType.Disjoint;
                    if (draw && DebugView)
                    {
                        Debug.Renderers.BoundingSphereRenderer.Render(GetTransformedModelBoundingSphere(Transforms[i]), Game1.Instance.GraphicsDevice, world.View, world.Projection, Color.Blue);
                    }
                }
                // On n'ajoute l'instance que si le culling ne l'a pas exclue.
                if (draw)
                {
                    lock (_instanceCountmutex)
                    {
                        Matrix transform = Transforms[i].GetTransform();
                        _instances[m_instanceCount] = (new VertexInstance(transform, Transforms[i].AdditionalData1, Transforms[i].TextureOffset));
                        m_instanceCount++;
                    }
                }
            }
        }
        /// <summary>
        /// Génère les vertex et instance buffers.
        /// </summary>
        void GenerateInstanceBuffer(GameWorld world)
        {
            VertexInstance[] instances = new VertexInstance[Transforms.Count()];
            _instances = instances;
            m_instanceCount = 0; // Nombre réel d'instances à dessiner.

            // Si le nombre de coeurs pour le calcul est supérieur à 1, on lance la version multi-thread.
            if (CORES != 1)
            {
                // Décrit les opérations effectuées par les threads.
                System.Threading.ParameterizedThreadStart start = new System.Threading.ParameterizedThreadStart((object interval) =>
                {
                    Point intervalPt = (Point)interval;
                    PerformCull(world, intervalPt.X, intervalPt.Y);
                });

                // Lance les threads
                System.Threading.Thread[] threads = new System.Threading.Thread[CORES-1];
                int length = Transforms.Count() / CORES;
                for (int i = 1; i < CORES; i++)
                {
                    threads[i-1] = new System.Threading.Thread(start);
                    threads[i-1].Priority = System.Threading.ThreadPriority.Highest;
                    threads[i-1].Start(new Point(length * i, length * (i + 1)));
                }

                PerformCull(world, 0, length);
                // Attends que tout le monde ait fini.
                bool allThreadsFinished = false;
                TimeSpan refreshInterval = new TimeSpan(10); // 10µs
                while (!allThreadsFinished)
                {
                    allThreadsFinished = true;
                    for (int i = 0; i < CORES-1; i++)
                    {
                        if (threads[i].ThreadState == System.Threading.ThreadState.Running)
                            allThreadsFinished = false;
                    }
                    System.Threading.Thread.Sleep(refreshInterval);
                }
            }
            else
            {
                // Version single-thread
                PerformCull(world, 0, m_transforms.Count());
            }
            // Si le buffer n'existe pas ou qu'il n'a pas la même taille que le nombre de d'élements, on le recrée.
            if (m_instanceBuffer == null || m_instanceBuffer.VertexCount != Transforms.Count())
            {
                if (m_instanceBuffer != null)
                    m_instanceBuffer.Dispose();

                m_instanceBuffer = new VertexBuffer(Game1.Instance.GraphicsDevice, VertexInstance.VertexDeclaration, m_transforms.Count(), BufferUsage.WriteOnly);
                m_bindings[1] = new VertexBufferBinding(m_instanceBuffer, 0, 1);
            }
            m_instanceBuffer.SetData<VertexInstance>(instances);
        }
        #endregion

        /// <summary>
        /// Dessine le modèle à l'aide des matrices et du shader donnés.
        /// </summary>
        public void Draw(GameWorld world)
        {
            // Génère l'instance buffer en prenant en compte le culling.
            GenerateInstanceBuffer(world);

            // Si aucune instance n'est à dessiner (toutes exclues par culling), on ne dessine rien.
            if (m_instanceCount == 0)
                return;

            BeginRasterizerStateChange();
            world.SetupShader(Shader, world.World);
            Shader.CurrentTechnique.Passes[0].Apply();
            Game1.Instance.GraphicsDevice.DrawInstancedPrimitives(PrimitiveType.TriangleList,
                0, 0,
                Model.Vertices.VertexCount, 0, Model.Indices.IndexCount / 3, m_instanceCount);
            EndRasterizerStateChange();
        }

        /// <summary>
        /// Crée une nouvelle instance de Object3D en spécifiant un modèle et un shader.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="shader"></param>
        public DynamicInstancedObjects(ModelData model, Effect shader, int instanceCount)
        {
            m_bindings = new VertexBufferBinding[2];
            Model = model;
            Shader = shader;
            Transforms = new Transform[instanceCount];
            InstanceCullingEnabled = true;
            SetupDefaultRasterizerState();
            
        }
        /// <summary>
        /// Crée une nouvelle instance de Object3D en spécifiant un modèle et un shader.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="shader"></param>
        public DynamicInstancedObjects(ModelData model, Transform[] transforms,  Effect shader)
        {
            m_bindings = new VertexBufferBinding[2];
            Model = model;
            Shader = shader;
            Transforms = transforms;
            InstanceCullingEnabled = true;
            SetupDefaultRasterizerState();
        }

        /* -------------------------------------------------------------------------------------------------
         * Code relatif à l'insertion du Frustum Culling
         * -----------------------------------------------------------------------------------------------*/
        #region Culling
        BoundingBox m_modelBoundingBox;
        BoundingSphere m_modelBoundingSphere;
        /// <summary>
        /// Détermine la position des vertices Min et Max de notre BoundingBox.
        /// Ces vertices sont non transformées mais elles sont générées une fois pour toute à partir du modèle.
        /// /!\ Cette méthode peut consommer beaucoup de temps.
        /// </summary>
        /// <param name="vertexData"></param>
        /// <param name="worldTransform"></param>
        /// <returns></returns>
        void ComputeUntransformedModelBoundingBox()
        {
            Vector3[] vertexData = new Vector3[m_model.Vertices.VertexCount];
            m_model.Vertices.GetData<Vector3>(vertexData);
            Vector3 min = new Vector3(float.MaxValue);
            Vector3 max = new Vector3(float.MinValue);
            foreach (Vector3 vect in vertexData)
            {
                min = Vector3.Min(vect, min);
                max = Vector3.Max(vect, max);
            }
            m_modelBoundingBox = new BoundingBox(min, max);
            m_modelBoundingSphere = BoundingSphere.CreateFromBoundingBox(m_modelBoundingBox);
        }
        /// <summary>
        /// Transforme la bounding box calculée précédemment avec la matrice donnée.
        /// </summary>
        /// <param name="worldTransform"></param>
        /// <returns></returns>
        BoundingBox GetTransformedModelBoundingBox(Transform worldTransform)
        {
            return BoundingBox.CreateFromSphere(GetTransformedModelBoundingSphere(worldTransform));
        }
        /// <summary>
        /// Transforme la bounding box calculée précédemment avec la matrice donnée.
        /// </summary>
        /// <param name="worldTransform"></param>
        /// <returns></returns>
        BoundingSphere GetTransformedModelBoundingSphere(Transform transform)
        {
            return new BoundingSphere(Vector3.Transform(m_modelBoundingSphere.Center, Matrix.CreateTranslation(transform.Position)),
                BBScale * m_modelBoundingSphere.Radius * Math.Max(Math.Max(transform.Scale.X, transform.Scale.Y), transform.Scale.Z));
        }
        #endregion

        #region Rasterizer
        public RasterizerState RasterizerState { get; set; }
        /// <summary>
        /// Applique le changement d'état de rasterizer.
        /// </summary>
        void BeginRasterizerStateChange()
        {
            var oldRasterizer = Game1.Instance.GraphicsDevice.RasterizerState;
            Game1.Instance.GraphicsDevice.RasterizerState = RasterizerState;
            RasterizerState = oldRasterizer;
        }
        /// <summary>
        /// Restore les paramètres antérieurs à l'appel de BeginRasterizerStateChange()
        /// </summary>
        void EndRasterizerStateChange()
        {
            var thisRasterizer = Game1.Instance.GraphicsDevice.RasterizerState;
            Game1.Instance.GraphicsDevice.RasterizerState = RasterizerState;
            RasterizerState = thisRasterizer;
        }
        /// <summary>
        /// Applique les paramètres par défaut au rasterizer.
        /// </summary>
        void SetupDefaultRasterizerState()
        {
            RasterizerState state = new RasterizerState();
            state.CullMode = CullMode.None;
            state.FillMode = FillMode.Solid;
            RasterizerState = state;
        }
        #endregion

        #region DEBUG
        public int InstancesDrawn { get { return m_instanceCount; } }
        public bool DebugView { get; set; }
        #endregion

        #region Dispose
        /// <summary>
        /// Supprime les ressources allouées par cet objet.
        /// </summary>
        public void Dispose()
        {
            if(!m_instanceBuffer.IsDisposed)
                m_instanceBuffer.Dispose();
        }
        public void Dispose(bool disposing)
        {
            Dispose();
        }
        #endregion
    }
}
