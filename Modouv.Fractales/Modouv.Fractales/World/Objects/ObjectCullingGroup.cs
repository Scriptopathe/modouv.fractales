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
    /// Groupe d'objets permettant l'optimisation du culling :
    /// En faisant des grands groupes subdivisés en plusieurs petits groupes, on
    /// arrive à optimiser le culling puisque moins d'intersections doivent être calculées.
    /// </summary>
    public class ObjectCullingGroup : IObject3D
    {
        #region Variables
        IObject3D[] m_objects;
        /// <summary>
        /// Obtient ou définit les objects à dessiner.
        /// </summary>
        public IObject3D[] Objects
        {
            get { return m_objects; }
            set { m_objects = value; ComputeTransformedBoundingBox(); }
        }
        /// <summary>
        /// Représente la bounding box de cette collection d'objets.
        /// Elle est crée à partir de la bounding box de chacun des ces objets transformées
        /// par leurs donnée d'instance.
        /// 
        /// Cette BoundingBox est normalement calculée une seule fois.
        /// </summary>
        BoundingBox m_boundingBox;
        #endregion

        #region Properties

        #endregion

        #region Constructor
        /// <summary>
        /// Crée une nouvelle instance de ObjectGroup contenant les objets spécifiés.
        /// </summary>
        /// <param name="objects"></param>
        public ObjectCullingGroup(IObject3D[] objects)
        {
            Objects = objects;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Détermine la position des vertices Min et Max de cette collection d'objets, transformée
        /// par les transformations respectives de chaque instance.
        /// </summary>
        void ComputeTransformedBoundingBox()
        {
            var boundingBoxMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var boundingBoxMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            for (int i = 0; i < m_objects.Length; i++)
            {
                BoundingBox box = m_objects[i].GetTransformedBoundingBox();
                boundingBoxMin = Vector3.Min(box.Min, boundingBoxMin);
                boundingBoxMax = Vector3.Max(box.Max, boundingBoxMax);
            }
            m_boundingBox = new BoundingBox(boundingBoxMin, boundingBoxMax);
        }

        /// <summary>
        /// Retourne la bounding box transformée contenant les bounding boxes des IObject3D enfants.
        /// </summary>
        public BoundingBox GetTransformedBoundingBox()
        {
            return m_boundingBox;
        }

        /// <summary>
        /// Dessine tous les parents si la BoundingBox de cette collection est visible par le monde.
        /// </summary>
        /// <param name="world"></param>
        public void Draw(GameWorld world)
        {
            BoundingBox box = GetTransformedBoundingBox();

            // Vérifie que la bounding box contenant les IObject3D enfants soit visible.
            if (world.GetFrustrum().Contains(box) != ContainmentType.Disjoint)
            {
                // Dessine les bounding box si en debug view.
                if (DebugView)
                {
                    Debug.Renderers.BoundingBoxRenderer.Render(box, Game1.Instance.GraphicsDevice,
                        world.View, world.Projection, Color.White);
                }
                // Note : les objects implémentent le culling de leurs propres sous objets.
                foreach (IObject3D obj in m_objects)
                {
                    obj.Draw(world);
                }
            }
        }
        /// <summary>
        /// Dessine tous les parents si la BoundingBox de cette collection est visible par le monde.
        /// </summary>
        /// <param name="world"></param>
        public void DrawWithoutCull(GameWorld world)
        {
            BoundingBox box = GetTransformedBoundingBox();

            // Note : les objects implémentent le culling de leurs propres sous objets.
            foreach (IObject3D obj in m_objects)
            {
                if (obj is ObjectCullingGroup)
                {
                    ((ObjectCullingGroup)obj).DrawWithoutCull(world);
                }
                else
                    obj.Draw(world);
            }
            
        }
        #endregion

        #region DEBUG
        public bool DebugView { get; set; }
        #endregion

        #region Dispose
        /// <summary>
        /// Libère les ressources allouées par cet objet.
        /// </summary>
        public void Dispose()
        {
            foreach(IObject3D obj in m_objects)
            {
                obj.Dispose();
            }
        }
        #endregion
    }
}
