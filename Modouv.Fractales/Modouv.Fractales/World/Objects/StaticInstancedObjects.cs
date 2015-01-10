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
using System.Threading.Tasks;
namespace Modouv.Fractales.World.Objects
{
    /// <summary>
    /// Représente une collection d'objets statique NE pouvant PAS bouger.
    /// Pour paramétrer correctement une instance de StaticInstancedObjects, il faut :
    ///     - Affecter un modèle à StaticInstancedObjects.Model et un shader à StaticInstancedObjects.Shader.
    ///     - Donner une liste de transformations à effectuer à DynamicInstancedObjects.Transforms.
    /// La liste des transformations NE peut PAS être modifiée pendant l'exécution.
    /// </summary>
    public class StaticInstancedObjects : IObject3D
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
        /// <summary>
        /// Si vrai, active le frustrum culling pour les objets dessinés par cette collection.
        /// Si activé, le frustum culling est calculé pour CHAQUE instance du modèle dessinée.
        /// 
        /// Ne pas activer pour des groupes de taille trop grande.
        /// </summary>
        bool m_instanceCullingEnabled;
        bool m_isComputingInstanceBuffer;
        #endregion

        #region Properties
        /// <summary>
        /// Si vrai, active le frustrum culling pour les objets dessinés par cette collection.
        /// Si activé, le frustum culling est calculé pour CHAQUE instance du modèle dessinée.
        /// 
        /// Ne pas activer pour des groupes de taille trop grande.
        /// </summary>
        public bool InstanceCullingEnabled
        {
            get { return m_instanceCullingEnabled; }
            set
            {
                if (value != m_instanceCullingEnabled)
                {
                    m_instanceCullingEnabled = value;
                    if (!value)
                    {
                        // Si on désactive le culling il va falloir mettre en cache, donc
                        // on supprime le buffer actuel qui ne sert plus à rien.
                        if (m_instanceBuffer != null)
                            m_instanceBuffer.Dispose();
                        m_instanceBuffer = null;
                    }
                }
            }
        }
        /// <summary>
        /// Si vrai, active le frustum culling pour cette collection d'instance.
        /// Si activé, le frustum culling est calculé pour la BoundingBox contenant tous les objets
        /// du groupe.
        /// 
        /// Cela évite de calculer le culling pour chacune des instances si elles ne sont pas à l'écran.
        /// 
        /// Il est préférable de l'activer.
        /// </summary>
        public bool CullingEnabled { get; set; }
        /// <summary>
        /// Données de dessin du modèle.
        /// </summary>
        public ModelData Model
        {
            get { return m_model; }
            protected set { m_model = value; }
        }
        /// <summary>
        /// Pixel shader du modèle.
        /// </summary>
        public Effect Shader { get; set; }
        /// <summary>
        /// Obtient ou définit les transformations associées à chaque objet 3D.
        /// </summary>
        public Transform[] Transforms
        {
            get { return m_transforms.ToArray(); }
            set
            {
                m_transforms = value;
                ComputeTransformedBoundingBox();
            }
        }
        /// <summary>
        /// Instance buffer, contient les transformations associées à chaque "instance" du modèle.
        /// </summary>
        VertexBuffer m_instanceBuffer;
        #endregion

        #region Culling / Instancing
        const int CORES = 1;
        const bool PARALLEL_INSTANCE_BUFFER = false;
        object _instanceCountmutex = new object();
        /// <summary>
        /// Variable de débug permettant de savoir si la collection d'objets a été dessinée.
        /// </summary>
        bool m_isDrawn;
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
                Matrix transform = Transforms[i].GetTransform();
                bool draw = true;
                // on regarde si oui ou non l'objet doit être dessiné.
                // Crée la bounding sphere de l'objet.
                BoundingSphere bsphere = GetTransformedModelBoundingSphere(Transforms[i]);
                draw = worldFrustum.Contains(bsphere) != ContainmentType.Disjoint;// || aabb.Contains(world.GetFrustrum()) != ContainmentType.Disjoint;
                if (draw && DebugView)
                {
                    Debug.Renderers.BoundingSphereRenderer.Render(GetTransformedModelBoundingSphere(Transforms[i]), Game1.Instance.GraphicsDevice, world.View, world.Projection, Color.White);
                }
                    
                // On n'ajoute l'instance que si le culling ne l'a pas exclue.
                if (draw)
                {
                    lock (_instanceCountmutex)
                    {
                        _instances[m_instanceCount] = (new VertexInstance(transform, Transforms[i].AdditionalData1, Transforms[i].TextureOffset));
                        m_instanceCount++;
                    }
                }
            }
            
        }
        /// <summary>
        /// Génère les vertex et instance buffers.
        /// </summary>
        public void GenerateInstanceBuffer(GameWorld world)
        {
            // Si l'InstanceCulling est activé, on doit mettre à jour le buffer.
            if (InstanceCullingEnabled)
            {
                VertexInstance[] instances = new VertexInstance[Transforms.Count()];
                _instances = instances;
                m_instanceCount = 0; // Nombre réel d'instances à dessiner.

                // Version single-thread
                PerformCull(world, 0, m_transforms.Length);
                

                // Si le buffer n'existe pas ou qu'il n'a pas la même taille que le nombre de d'élements, on le recrée.
                if (m_instanceBuffer == null || m_instanceBuffer.VertexCount != Transforms.Length)
                {
                    m_instanceBuffer = new VertexBuffer(Game1.Instance.GraphicsDevice, VertexInstance.VertexDeclaration, m_transforms.Length, BufferUsage.WriteOnly);
                    if(m_bindings != null && m_bindings[1].VertexBuffer != null)
                        m_bindings[1].VertexBuffer.Dispose(); // on supprime l'ancien buffer.
                    m_bindings[1] = new VertexBufferBinding(m_instanceBuffer, 0, 1);
                }
                m_instanceBuffer.SetData<VertexInstance>(instances);
            }
            else // !InstanceCullingEnabled
            {
                // Si l'instance culling est désactivé, on met le buffer en cache.
                if (m_instanceBuffer == null)
                {
                    if (PARALLEL_INSTANCE_BUFFER)
                    {
                        // Calcule les instance buffers sur un thread à part.
                        System.Threading.Thread task = new System.Threading.Thread(new System.Threading.ThreadStart(delegate()
                        {
                            VertexInstance[] instances = new VertexInstance[Transforms.Count()];
                            for (int i = 0; i < instances.Length; i++)
                            {
                                instances[i] = new VertexInstance(Transforms[i].GetTransform(), Transforms[i].AdditionalData1, Transforms[i].TextureOffset);

                            }

                            m_instanceBuffer = new VertexBuffer(Game1.Instance.GraphicsDevice, VertexInstance.VertexDeclaration, m_transforms.Count(), BufferUsage.WriteOnly);
                            m_bindings[1] = new VertexBufferBinding(m_instanceBuffer, 0, 1);
                            m_instanceBuffer.SetData<VertexInstance>(instances);
                            m_isComputingInstanceBuffer = false;
                        }));
                        m_isComputingInstanceBuffer = true;
                        task.Priority = System.Threading.ThreadPriority.Highest;
                        task.Start();
                    }
                    else
                    {
                        VertexInstance[] instances = new VertexInstance[Transforms.Count()];
                        for (int i = 0; i < instances.Length; i++)
                        {
                            instances[i] = new VertexInstance(Transforms[i].GetTransform(), Transforms[i].AdditionalData1, Transforms[i].TextureOffset);

                        }

                        m_instanceBuffer = new VertexBuffer(Game1.Instance.GraphicsDevice, VertexInstance.VertexDeclaration, m_transforms.Count(), BufferUsage.WriteOnly);
                        m_bindings[1] = new VertexBufferBinding(m_instanceBuffer, 0, 1);
                        m_instanceBuffer.SetData<VertexInstance>(instances);
                    }
                    
                }

                // Affiche les Bounding Spheres si en mode DebugView.
                if (DebugView)
                {
                    for (int i = 0; i < Transforms.Length; i++)
                    {
                        Debug.Renderers.BoundingSphereRenderer.Render(GetTransformedModelBoundingSphere(Transforms[i]), Game1.Instance.GraphicsDevice, world.View, world.Projection, Color.White);
                        Debug.Renderers.BoundingBoxRenderer.Render(GetTransformedModelBoundingBox(Transforms[i]), Game1.Instance.GraphicsDevice, world.View, world.Projection, Color.Honeydew);
                    }
                }
                if(!m_isComputingInstanceBuffer)
                    m_instanceCount = m_instanceBuffer.VertexCount;
            }
        }
        #endregion

        /// <summary>
        /// Dessine le modèle à l'aide des matrices et du shader donnés.
        /// </summary>
        public void Draw(GameWorld world)
        {
            Draw(world, true);
        }
        /// <summary>
        /// Dessine le modèle à l'aide des matrices et du shader donnés.
        /// Si generateInstanceBuffer vaut true, le culling est effectué et l'instance buffer régénéré.
        /// Sinon, l'instance buffer précédent est gardé.
        /// </summary>
        public void Draw(GameWorld world, bool generateInstanceBuffer)
        {
            if (m_transforms.Length == 0 || m_isComputingInstanceBuffer)
                return;

            m_instanceCount = 0;
            m_isDrawn = !CullingEnabled || world.GetFrustrum().Contains(m_boundingBox) != ContainmentType.Disjoint;

            if(m_isDrawn && DebugView)
                Debug.Renderers.BoundingBoxRenderer.Render(m_boundingBox, Game1.Instance.GraphicsDevice, world.View, world.Projection, Color.White);

            if (m_isDrawn)
            {
                // Génère l'instance buffer si demandé.
                if(generateInstanceBuffer)
                    GenerateInstanceBuffer(world);

                // Si aucune instance n'est à dessiner (toutes exclues par culling), on ne dessine rien.
                if (m_instanceCount == 0)
                    return;

                BeginRasterizerStateChange();
                Game1.Instance.GraphicsDevice.SetVertexBuffers(m_bindings);
                Game1.Instance.GraphicsDevice.Indices = Model.Indices;
                world.SetupShader(Shader, Matrix.Identity);

                Shader.CurrentTechnique.Passes[0].Apply();
                Game1.Instance.GraphicsDevice.DrawInstancedPrimitives(PrimitiveType.TriangleList,
                    0, 0,
                    Model.Vertices.VertexCount, 0, Model.Indices.IndexCount / 3, m_instanceCount);
                EndRasterizerStateChange();
            }
        }

        /// <summary>
        /// Crée une nouvelle instance de Object3D en spécifiant un modèle et un shader.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="shader"></param>
        public StaticInstancedObjects(ModelData model, Transform[] transforms, Effect shader)
        {
            ctor(model, transforms, shader);
        }

        /// <summary>
        /// Constructeur privé.
        /// </summary>
        private void ctor(ModelData model, Transform[] transforms, Effect shader)
        {
            m_bindings = new VertexBufferBinding[2];
            Model = model;
            Shader = shader;
            m_transforms = transforms;

            // Vérifie que le modèle est correct.
            if (m_model == null || m_model.Indices == null || m_model.Vertices == null)
                throw new Exception("Le modèle doit être créé correctement avant de construire une instance de StaticInstancedObjects");

            // Calcule le Min/Max de la bounding box du modèle.
            m_bindings[0] = new VertexBufferBinding(m_model.Vertices, 0);

            SetupDefaultRasterizerState();
            GC.Collect();
        }

        /* -------------------------------------------------------------------------------------------------
         * Code relatif à l'insertion du Frustum Culling
         * -----------------------------------------------------------------------------------------------*/
        #region Culling
        BoundingSphere m_modelBoundingSphere;
        /// <summary>
        /// Bounding box du modèle, non transformée.
        /// </summary>
        BoundingBox m_modelBoundingBox;
        /// <summary>
        /// Représente la bounding box de cette collection d'objets.
        /// Elle est crée à partir de la bounding box de chacun des ces objets transformées
        /// par leurs donnée d'instance.
        /// 
        /// Cette BoundingBox est normalement calculée une seule fois.
        /// </summary>
        BoundingBox m_boundingBox;

        /// <summary>
        /// Force la bounding box aabb à être utilisée pour les collisions.
        /// </summary>
        /// <param name="aabb"></param>
        public void SetModelBoundingBox(BoundingBox aabb)
        {
            m_modelBoundingBox = aabb;
            m_modelBoundingSphere = BoundingSphere.CreateFromBoundingBox(aabb);
            ComputeTransformedBoundingBox();
        }

        /// <summary>
        /// Détermine la position des vertices Min et Max de cette collection d'objets, transformée
        /// par les transformations respectives de chaque instance.
        /// </summary>
        void ComputeTransformedBoundingBox()
        {
            var boundingBoxMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var boundingBoxMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            for (int i = 0; i < m_transforms.Count(); i++)
            {
                BoundingBox box = GetTransformedModelBoundingBox(m_transforms[i]);
                boundingBoxMin = Vector3.Min(box.Min, boundingBoxMin);
                boundingBoxMax = Vector3.Max(box.Max, boundingBoxMax);
            }
            m_boundingBox = new BoundingBox(boundingBoxMin, boundingBoxMax);
        }
        /// <summary>
        /// Retourne la BoundingBox transformée de cette instance.
        /// </summary>
        /// <param name="world"></param>
        /// <returns></returns>
        public BoundingBox GetTransformedBoundingBox()
        {
            return m_boundingBox;
        }
        /// <summary>
        /// Transforme la bounding box calculée précédemment avec la matrice donnée.
        /// </summary>
        /// <param name="worldTransform"></param>
        /// <returns></returns>
        BoundingBox GetTransformedModelBoundingBox(Transform transform)
        {
            return BoundingBox.CreateFromSphere(GetTransformedModelBoundingSphere(transform));
        }
        /// <summary>
        /// Transforme la bounding box calculée précédemment avec la matrice donnée.
        /// </summary>
        /// <param name="worldTransform"></param>
        /// <returns></returns>
        BoundingSphere GetTransformedModelBoundingSphere(Transform transform)
        {
            return new BoundingSphere(
                m_modelBoundingSphere.Center + transform.Position,
                m_modelBoundingSphere.Radius * Math.Max(Math.Max(transform.Scale.X, transform.Scale.Y), transform.Scale.Z));
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
            RasterizerState = new RasterizerState();
            RasterizerState.FillMode = oldRasterizer.FillMode;
            RasterizerState.CullMode = CullMode.None;
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
            state.FillMode = Game1.Instance.GraphicsDevice.RasterizerState.FillMode;
            RasterizerState = state;
        }
        #endregion

        #region DEBUG
        public int InstancesDrawn { get { return m_instanceCount; } }
        public bool IsDrawn { get { return m_isDrawn; } }
        public bool DebugView { get; set; }
        #endregion

        #region Dispose
        /// <summary>
        /// Libère les ressources allouées par cet objet.
        /// </summary>
        public void Dispose()
        {
            if(m_instanceBuffer != null)
                m_instanceBuffer.Dispose();
        }
        #endregion
    }
}
