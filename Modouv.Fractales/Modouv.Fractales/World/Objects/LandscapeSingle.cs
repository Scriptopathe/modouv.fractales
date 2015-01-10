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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace Modouv.Fractales.World.Objects
{
    /// <summary>
    /// Représente un paysage.
    /// </summary>
    public class LandscapeSingle
    {
        #region Variables
        /// <summary>
        /// Modèle utilisé pour la génération du landscape.
        /// </summary>
        DynamicObject m_object;
        /// <summary>
        /// Heightmap du paysage.
        /// </summary>
        float[,] m_heightmap;
        /// <summary>
        /// Facteur multiplicatif en largeur / longueur.
        /// </summary>
        float m_hscale;
        /// <summary>
        /// Facteur multiplicatif en hauteur.
        /// </summary>
        float m_vscale;
        #endregion

        #region Properties
        public float VScale { get { return m_vscale; } set { m_vscale = value; m_object.Scale = new Vector3(m_object.Scale.X, m_object.Scale.Y, value); } }
        public float HScale { get { return m_hscale; } set { m_hscale = value; m_object.Scale = new Vector3(value, value, m_object.Scale.Z); } }
        public float[,] Heightmap { get { return m_heightmap; } set { m_heightmap = value; Regenerate(); } }
        /// <summary>
        /// Indices du modèle.
        /// Cela permet de recalculer les normales à postériori si la heightmapp est modifiée.
        /// </summary>
        public int[] Indices { get;  set; }
        public DynamicObject Object { get { return m_object; } }
        #endregion

        #region Methods
        /// <summary>
        /// Crée une nouvelle instance de Landscape, vide.
        /// </summary>
        public LandscapeSingle()
        {
            m_object = new DynamicObject();
            m_object.CullingEnabled = false;
        }
        /// <summary>
        /// Crée une nouvelle instance 
        /// </summary>
        public LandscapeSingle(float hscale, float vscale, float[,] heightmap, Vector3 position, Effect shader)
        {
            m_hscale = hscale;
            m_vscale = vscale;
            m_heightmap = heightmap;
            m_object = new DynamicObject();
            m_object.Position = position;
            m_object.Rotation = new Vector3(0, 0, 0);
            m_object.Shader = shader;
            m_object.Scale = new Vector3(hscale, hscale, vscale);
            m_object.CullingEnabled = false;
            Regenerate();

        }
        /// <summary>
        /// Re-génère le paysage.
        /// </summary>
        void Regenerate()
        {
            m_object.Model = Generation.ModelGenerator.GenerateModel(Heightmap, 1, 1);

            // On remplit les indices
            Indices = new int[m_object.Model.Indices.IndexCount];
            m_object.Model.Indices.GetData<int>(Indices);

            // Création forcée d'une bounding box.
            int bbsize = m_heightmap.GetLength(0)/2;
            int bbheight = 2000;
            m_object.ForceCullingBoundingBox(new BoundingBox(new Vector3(-bbsize, -bbsize, -bbheight), new Vector3(bbsize, bbsize, bbheight)));
        }
        /// <summary>
        /// Retourne la position 3D
        /// </summary>
        /// <param name="verticeX"></param>
        /// <param name="verticeY"></param>
        /// <returns></returns>
        public Vector3 GetVerticePosition(int verticeX, int verticeY)
        {
            
            // Offset 2D de la première vertice.
            Vector2 verticePos2D =  new Vector2(m_object.Position.X, m_object.Position.Y) +
                new Vector2((verticeX - Heightmap.GetLength(0) / 2), (verticeY - Heightmap.GetLength(1) / 2));
            Vector3 verticePos3D = new Vector3(verticePos2D, Heightmap[verticeX, verticeY]);
            Vector3 verticePos3DTransformed = Vector3.Transform(verticePos3D, m_object.GetTransform());
            return verticePos3DTransformed;
        }
        /// <summary>
        /// Dessine le paysage.
        /// </summary>
        /// <param name="time"></param>
        public void Draw(GameWorld world)
        {
            m_object.Draw(world);
        }
        #endregion
    }
}
