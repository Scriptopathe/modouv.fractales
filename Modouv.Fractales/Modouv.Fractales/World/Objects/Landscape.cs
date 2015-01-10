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
    public class Landscape
    {
        #region Constantes
        const int LANDSCAPE_GRID_SIZE = 16;
        #endregion

        #region Variables
        /// <summary>
        /// Shader utilisé pour tous les objets.
        /// </summary>
        Effect m_shader;
        /// <summary>
        /// Modèle utilisé pour la génération du landscape.
        /// </summary>
        List<DynamicMipmapedObject> m_objects;
        /// <summary>
        /// Heightmap du paysage.
        /// </summary>
        float[,] m_heightmap;
        /// <summary>
        /// Normal map du paysage.
        /// </summary>
        Vector3[,] m_normalMap;
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
        public Effect Shader
        {
            get { return m_shader; }
            set 
            {
                foreach (DynamicMipmapedObject obj in m_objects)
                {
                    obj.Shader = value;
                }
                m_shader = value;
            }
        }
        /// <summary>
        /// Position du coin supérieur gauche du paysage.
        /// </summary>
        public Vector3 Position { get; set; }
        /// <summary>
        /// Echelle horizontale du paysage.
        /// Une grande valeur aura pour effet un paysage plus large.
        /// </summary>
        public float VScale {
            get { return m_vscale; }
            set
            {
                m_vscale = value;
                foreach (DynamicMipmapedObject obj in m_objects)
                {
                    obj.Scale = new Vector3(obj.Scale.X, obj.Scale.Y, value);
                }
            }
        }
        /// <summary>
        /// Echelle verticale du paysage.
        /// Une grande valeur aura pour effet un paysage plus haut.
        /// </summary>
        public float HScale 
        { 
            get { return m_hscale; }
            set 
            { 
                m_hscale = value;
                foreach (DynamicMipmapedObject obj in m_objects)
                {
                    obj.Scale = new Vector3(value, value, obj.Scale.Z);
                }
            } 
        }
        /// <summary>
        /// Qualité max du modèle lorsque la qualité est rendu automatiquement.
        /// </summary>
        public Objects.ModelMipmap.ModelQuality MinModelAutoQuality { get; set; }
        public float[,] Heightmap { get { return m_heightmap; } set { m_heightmap = value; Regenerate(); } }
        public Vector3[,] NormalMap { get { return m_normalMap; } }
        public List<DynamicMipmapedObject> Object { get { return m_objects; } }
        #endregion

        #region Methods
        /// <summary>
        /// Crée une nouvelle instance de Landscape, vide.
        /// </summary>
        public Landscape()
        {
            m_objects = new List<DynamicMipmapedObject>();
        }
        /// <summary>
        /// Crée une nouvelle instance 
        /// </summary>
        public Landscape(float hscale, float vscale, float[,] heightmap, Vector3 position, Effect shader)
        {
            m_hscale = hscale;
            m_vscale = vscale;
            m_heightmap = heightmap;
            m_objects = new List<DynamicMipmapedObject>();
            Regenerate();

        }
        /// <summary>
        /// Retourne un Vector2 contenant le min et le max de la heightmap donnée.
        /// </summary>
        /// <param name="heightmap"></param>
        /// <returns></returns>
        Vector2 GetMinMax(float[,] heightmap)
        {
            float min = float.MaxValue;
            float max = float.MinValue;
            for (int x = 0; x < heightmap.GetLength(0); x++)
            {
                for (int y = 0; y < heightmap.GetLength(1); y++)
                {
                    min = Math.Min(min, heightmap[x, y]);
                    max = Math.Max(max, heightmap[x, y]);
                }
            }
            return new Vector2(min, max);
        }
        /// <summary>
        /// Re-génère le paysage.
        /// </summary>
        void Regenerate()
        {
            // TODO : supprimer correctement les objects.
            foreach (DynamicMipmapedObject obj in m_objects)
            {
                obj.Dispose();
            }
            m_objects.Clear();
            // Création d'un objet par cellule de grille.
            int cellWidth = Heightmap.GetLength(0) / LANDSCAPE_GRID_SIZE;
            int cellHeight = Heightmap.GetLength(1) / LANDSCAPE_GRID_SIZE;
            for (int x = 0; x < LANDSCAPE_GRID_SIZE; x++)
            {
                for (int y = 0; y < LANDSCAPE_GRID_SIZE; y++)
                {
                    DynamicMipmapedObject obj = new DynamicMipmapedObject();
                    Rectangle area = new Rectangle(x * cellWidth, y * cellWidth, cellWidth + 1, cellHeight + 1);
                    // On crée une heightmap qui représente une partie de la heightmap totale.
                    float sx = Math.Max(0, x * cellWidth-1);
                    float sy = Math.Max(0, y * cellHeight-1);
                    float[,] heightmap = CopyArea(m_heightmap, area);
                    Vector2 MinMax = GetMinMax(heightmap);
                    //obj.Model[ModelMipmap.ModelQuality.High] = Generation.ModelGenerator.GenerateModel(heightmap, 1, 1, false, area.X, area.Y, Heightmap.GetLength(0)-1, Heightmap.GetLength(1)-1, 1);
                    //obj.Model[ModelMipmap.ModelQuality.Medium] = Generation.ModelGenerator.GenerateModel(heightmap, 1, 1, false, area.X, area.Y, Heightmap.GetLength(0) - 1, Heightmap.GetLength(1) - 1, 4);
                    //obj.Model[ModelMipmap.ModelQuality.Low] = Generation.ModelGenerator.GenerateModel(heightmap, 1, 1, false, area.X, area.Y, Heightmap.GetLength(0) - 1, Heightmap.GetLength(1) - 1, 8);
                    List<ModelData> models = Generation.ModelGenerator.GenerateModel(3, heightmap, 1, 1, false, area.X, area.Y, Heightmap.GetLength(0) - 1, Heightmap.GetLength(1) - 1);
                    obj.Model[ModelMipmap.ModelQuality.High] = models[0];
                    obj.Model[ModelMipmap.ModelQuality.Medium] = models[1];
                    obj.Model[ModelMipmap.ModelQuality.Low] = models[2];
                    //models[1].Vertices.Dispose();
                    //models[1].Indices.Dispose();

                    // Création d'une bounding box
                    int bbsize = heightmap.GetLength(0);
                    float bbheight = (MinMax.Y - MinMax.X);
                    obj.ForceCullingBoundingBox(new BoundingBox(new Vector3(0, 0, MinMax.X), new Vector3(bbsize, bbsize, MinMax.Y)));
                    // Ajustement des paramètres.
                    obj.Scale = new Vector3(m_hscale, m_hscale, m_vscale);
                    obj.Position = Position + new Vector3(x * cellWidth * m_hscale, y * cellHeight * m_hscale, -20);
                    obj.DebugView = false;
                    obj.CullingEnabled = true;
                    obj.Shader = m_shader;
                    
                    m_objects.Add(obj);
                }
            }
            if (m_normalMap != null)
            {
                m_normalMap = null;
                GC.Collect();
            }
            m_normalMap = Modouv.Fractales.Generation.Mapping.NormalMapGenerator.GenerateArray(Heightmap);
        }

        /// <summary>
        /// Crée et retourne une copie de la partie définie par le Rectangle area de la heightmap donnée.
        /// </summary>
        /// <param name="heightmap"></param>
        /// <param name="area"></param>
        /// <returns></returns>
        float[,] CopyArea(float[,] heightmap, Rectangle area)
        {
            float[,] subMap = new float[area.Width, area.Height];
            for (int x = 0; x < area.Width; x++)
            {
                for (int y = 0; y < area.Height; y++)
                {
                    subMap[x, y] = heightmap[x + area.X, y + area.Y];
                }
            }
            return subMap;
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
            Vector2 verticePos2D =  new Vector2(verticeX, verticeY);
            Vector3 verticePos3D = new Vector3(verticePos2D, Heightmap[verticeX, verticeY]);
            Vector3 verticePos3DTransformed = Vector3.Transform(verticePos3D, m_objects.First().GetTransform());
            return verticePos3DTransformed;
        }
        /// <summary>
        /// Retourne la normale au vertex à la position donnée.
        /// </summary>
        /// <param name="verticeX"></param>
        /// <param name="verticeY"></param>
        /// <returns></returns>
        public Vector3 GetVerticeNormal(int verticeX, int verticeY)
        {
            return NormalMap[verticeX, verticeY];
        }
        /// <summary>
        /// Dessine le paysage.
        /// </summary>
        /// <param name="time"></param>
        public void Draw(GameWorld world, bool useLastDrawCulling=false, bool cullingEnabled=true)
        {
            foreach (DynamicMipmapedObject obj in m_objects)
            {
                obj.CullingEnabled = cullingEnabled;
                obj.MinModelAutoQuality = MinModelAutoQuality;
                obj.Draw(world, useLastDrawCulling);
            }
        }

        /// <summary>
        /// Dessine le paysage avec la qualité donnée.
        /// </summary>
        /// <param name="world"></param>
        /// <param name="quality"></param>
        public void Draw(GameWorld world, ModelMipmap.ModelQuality quality, bool useLastDrawCulling=false)
        {
            foreach (DynamicMipmapedObject obj in m_objects)
            {
                obj.Draw(world, quality, useLastDrawCulling);
            }
        }
        #endregion
    }
}
