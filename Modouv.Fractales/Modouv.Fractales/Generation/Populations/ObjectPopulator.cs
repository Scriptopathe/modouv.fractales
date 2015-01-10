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
using Modouv.Fractales.World.Objects;
using Modouv.Fractales.World;
namespace Modouv.Fractales.Generation.Populations
{
    /// <summary>
    /// Classe permettant la création d'ObjectCullingGroup dont les objects sont répartis selon des fonctions données.
    /// </summary>
    public abstract class ObjectPopulator
    {
        /// <summary>
        /// Fonction retournant un tableau de transformations à partir de la profondeur et de la région
        /// en cours de traitement.
        /// </summary>
        /// <param name="depth"></param>
        /// <param name="region"></param>
        /// <returns></returns>
        public delegate Transform[] PopulateFunction(int depth, Rectangle region);
        /// <summary>
        /// Décrit un objet passé à la méthode Generate lui permettant de générer la population d'objets.
        /// </summary>
        public class PopulationData
        {
            public Landscape Landscape;
            public ModelData model;
            public Effect shader;
            public int MaxDepth = 5;
            public int Density = 10;
            public PopulateFunction PopulateFunc;
            public BoundingBox ForcedBB;
            public bool GroupDebugView;
            public bool InstanceDebugView;
        }

        public static Random rand = new Random();


        /// <summary>
        /// Génère une population à partir des données de population fournies en argument.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static IObject3D Generate(PopulationData data)
        {
            return Populate(data, new Rectangle(0, 0, data.Landscape.Heightmap.GetLength(0), data.Landscape.Heightmap.GetLength(1)), 0);
        }

        /// <summary>
        /// Fonction récursive créant des sous groupes à partir de groupes plus grands, pour finalement créer les objets à la profondeur
        /// s_data.MaxDepth.
        /// </summary>
        /// <param name="region"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        public static IObject3D Populate(PopulationData data, Rectangle region, int depth)
        {
            if (depth == data.MaxDepth)
            {
                StaticInstancedObjects objects = new StaticInstancedObjects(data.model, data.PopulateFunc(depth, region), data.shader);
                objects.SetModelBoundingBox(data.ForcedBB);
                objects.InstanceCullingEnabled = false;
                objects.CullingEnabled = false;
                objects.DebugView = data.InstanceDebugView;
                if (((Scenes.SceneFractalWorld)Game1.Instance.Scene).World.GraphicalParameters.PreloadInstanceBuffers && objects.Transforms.Length != 0)
                    objects.GenerateInstanceBuffer(null);
                /*
                if(objects.Transforms.Length != 0)
                    objects.GenerateInstanceBuffer(null);
                 * */
                return objects;
            }
            else
            {
                // On crée 4 object culling groups
                IObject3D[] objs = new IObject3D[4];
                objs[0] = Populate(data, new Rectangle(region.X                   , region.Y                      , region.Width / 2      , region.Height / 2), depth + 1);
                objs[1] = Populate(data, new Rectangle(region.X + region.Width / 2, region.Y                      , region.Width / 2      , region.Height / 2), depth + 1);
                objs[2] = Populate(data, new Rectangle(region.X                   , region.Y + region.Height / 2  , region.Width / 2      , region.Height / 2), depth + 1);
                objs[3] = Populate(data, new Rectangle(region.X + region.Width / 2, region.Y + region.Height / 2  , region.Width / 2      , region.Height / 2), depth + 1);
                ObjectCullingGroup group = new ObjectCullingGroup(objs);
                group.DebugView = data.GroupDebugView;
                return group;
            }
        }
    }
}
