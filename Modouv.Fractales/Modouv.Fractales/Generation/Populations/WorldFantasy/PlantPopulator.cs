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
    public class PlantPopulation
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
            
            data.model = CreateModel();
            data.Landscape = landscape;
            data.MaxDepth = 5; // 6
            data.Density = 1000;
            data.ForcedBB = new BoundingBox(new Vector3(-1, -1, -1), new Vector3(1, 1, 1));
            data.InstanceDebugView = false;
            data.GroupDebugView = false;

            // On calcule la fractale nécessaires à la répartition des plantes.
            RenderTarget2D juliaHeightMap;
            Color[] juliaData;
            lock (Game1.GraphicsDeviceMutex)
            {
                juliaHeightMap = new RenderTarget2D(Game1.Instance.GraphicsDevice, landscape.Heightmap.GetLength(0), landscape.Heightmap.GetLength(1), false, SurfaceFormat.Color, DepthFormat.None);
                Fractals.Julia.GenerateTexture2DGPU(new MathHelpers.Complex(-0.857f, 0.231f),
                    new MathHelpers.Complex(-1, -1), 1.0f, juliaHeightMap, "Heightmap");
                juliaData = new Color[landscape.Heightmap.GetLength(0) * landscape.Heightmap.GetLength(1)];
                juliaHeightMap.GetData<Color>(juliaData);
            }

            data.PopulateFunc = new ObjectPopulator.PopulateFunction((int depth, Rectangle region) => 
            {
                List<Transform> transforms = new List<Transform>(data.Density);
                for (int i = 0; i < data.Density; i++)
                {
                    int px = region.X + rand.Next(region.Width);
                    int py = region.Y + rand.Next(region.Height);

                    // Récupère la valeur dans la heightmap de julia.
                    int posInTable = px + landscape.Heightmap.GetLength(0) * py;
                    int juliaHeight = 0;
                    if(posInTable >= 0 && posInTable < juliaData.Length)
                        juliaHeight = juliaData[posInTable].R;

                    Vector3 position = data.Landscape.GetVerticePosition(px, py);

                    if (position.Z < 0.5f) //&& position.Z > -20f)
                    {
                        Vector3 normal = Vector3.Normalize(data.Landscape.GetVerticeNormal(px, py));
                        if (normal.Z > 0.825f)
                        {
                            Transform t = new Transform();
                            t.Position = position + new Vector3(0, 0, 0.17f); //position;
                            t.Position += new Vector3(rand.Next(20) / 200.0f, rand.Next(20) / 200.0f, 0);
                            t.Scale = new Vector3(1, -1, 1) * (0.002f);
                            t.Rotation = new Vector3(-MathHelper.PiOver2, 0, 0);
                            t.AdditionalData1 = new Vector4(normal, rand.Next(100));
                            int r = rand.Next(4);
                            int r2 = rand.Next(64);

                            // Répartit les plantes en fonction de la fractale de julia.
                            if(juliaHeight < 50)
                                t.TextureOffset = new Vector2(r2 == 1 ? 0.0f : 0.5f, (r % 2) / 2.0f);
                            else if(juliaHeight < 80)
                                t.TextureOffset = new Vector2(0.0f, 0.5f);
                            else if (juliaHeight < 200)
                                t.TextureOffset = new Vector2((r / 2) / 2.0f, 0.0f);
                            else if (juliaHeight <= 255)
                                t.TextureOffset = new Vector2((r / 2) / 2.0f, (r % 2) / 2.0f);
                                
                            transforms.Add(t);
                        }
                    }
                    
                    
                }
                return transforms.ToArray();
            });
            var population =  ObjectPopulator.Generate(data);
            // On libère la mémoire vidéo utilisée par la heightmap.
            juliaHeightMap.Dispose();
            return population;
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
