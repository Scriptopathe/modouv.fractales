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
using Vert = Modouv.Fractales.Generation.ModelGenerator.VertexPositionColorNormalTexture;
namespace Modouv.Fractales.Generation.Populations.WorldFantasy
{
    public class LightPopulation
    {
        /// <summary>
        /// Génère une population à partir des données fournies et de paramètres par défaut.
        /// </summary>
        /// <param name="landscape"></param>
        /// <param name="model"></param>
        /// <param name="shader"></param>
        /// <returns></returns>
        public static IObject3D Generate(Landscape landscape, Effect shader)
        {
            var rand = ObjectPopulator.rand;
            var data = new ObjectPopulator.PopulationData();
            data.shader = shader;
            data.shader.Parameters["TreeTexture"].SetValue(Game1.Instance.Content.Load<Texture2D>("textures\\world_fantasy\\light"));
            data.model = CreateModel();
            data.Landscape = landscape;
            data.MaxDepth = 1; // 5
            data.Density = 1;
            data.ForcedBB = new BoundingBox(new Vector3(-1, -1, -1), new Vector3(1, 1, 1));
            data.InstanceDebugView = false;
            data.GroupDebugView = false;


            data.PopulateFunc = new ObjectPopulator.PopulateFunction((int depth, Rectangle region) => 
            {
                List<Transform> transforms = new List<Transform>(data.Density);
                for (int i = 0; i < data.Density; i++)
                {
                    int px = region.X + ObjectPopulator.rand.Next(region.Width);
                    int py = region.Y + rand.Next(region.Height);
                    Vector3 position = data.Landscape.GetVerticePosition(px, py);

                    if (true || position.Z < 10f)
                    {
                        Vector3 normal = new Vector3(0, 0, 10);
                        Transform t = new Transform();
                        t.Position = position;
                        t.Position += new Vector3(rand.Next(20) / 200.0f, rand.Next(20) / 200.0f, -rand.Next(100) / 9.0f);
                        t.Scale = new Vector3(1, -1, 1) * (0.002f);
                        t.Rotation = new Vector3(-MathHelper.PiOver2, 0, 0);
                        t.AdditionalData1 = new Vector4(normal, rand.Next(100));
                        transforms.Add(t);
                    }
                    
                    
                }
                return transforms.ToArray();
            });
            return ObjectPopulator.Generate(data);
        }

        /// <summary>
        /// Crée le modèle 3D.
        /// </summary>
        /// <returns></returns>
        static ModelData CreateModel()
        {
            return Generation.ModelGenerator.GenerateModel(new float[2,2] { {0, 0}, {0, 0} }, 50, 1, true, 1);
        }
    }
}
