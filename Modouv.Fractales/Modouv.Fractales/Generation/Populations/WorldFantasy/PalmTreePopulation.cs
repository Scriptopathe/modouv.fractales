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
namespace Modouv.Fractales.Generation.Populations.WorldFantasy
{
    public class PalmTreePopulation
    {
        /// <summary>
        /// Génère une population à partir des données fournies et de paramètres par défaut.
        /// </summary>
        /// <param name="landscape"></param>
        /// <param name="model"></param>
        /// <param name="shader"></param>
        /// <returns></returns>
        public static IObject3D Generate(Landscape landscape, ModelData model, Effect shader)
        {
            var rand = ObjectPopulator.rand;
            var data = new ObjectPopulator.PopulationData();
            data.shader = shader;
            data.model = model;
            data.Landscape = landscape;
            data.MaxDepth = 1;
            data.Density = 1;
            data.ForcedBB = new BoundingBox(new Vector3(-1, -1, -1), new Vector3(1, 1, 1));
            data.PopulateFunc = new ObjectPopulator.PopulateFunction((int depth, Rectangle region) => 
            {
                List<Transform> transforms = new List<Transform>(data.Density);
                for (int i = 0; i < data.Density; i++)
                {
                    Vector3 position = data.Landscape.GetVerticePosition(
                        region.X + ObjectPopulator.rand.Next(region.Width),
                        region.Y + rand.Next(region.Height));
                    if (position.Z < 0 && (Math.Abs(position.Z) < 10 || rand.Next(50) == 0))
                    {
                        float piover2 = MathHelper.PiOver2;
                        Transform t = new Transform();
                        t.Position = position;
                        t.Position += new Vector3(rand.Next(10) / 10.0f, rand.Next(10) / 10.0f, 0.4f);
                        t.Scale = new Vector3(1, 1, 1) * (0.02f + rand.Next(200) / 10000.0f);
                        t.Rotation = new Vector3(-piover2, rand.Next(20) / 100.0f, rand.Next(314) / 100.0f);
                        transforms.Add(t);
                    }
                }
                return transforms.ToArray();
            });
            return ObjectPopulator.Generate(data);
        }
        
    }
}
