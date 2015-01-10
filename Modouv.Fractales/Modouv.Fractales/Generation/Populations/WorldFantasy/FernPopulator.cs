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
    public class FernPopulation
    {
        /// <summary>
        /// Génère une population à partir des données fournies et de paramètres par défaut.
        /// </summary>
        /// <param name="landscape"></param>
        /// <param name="model"></param>
        /// <param name="shader"></param>
        /// <returns></returns>
        public static IObject3D Generate(Landscape landscape)
        {
            var rand = ObjectPopulator.rand;
            var data = new ObjectPopulator.PopulationData();
            data.shader = Game1.Instance.Content.Load<Effect>("Shaders\\world_fantasy\\fern");
            data.shader.Parameters["Tex"].SetValue(Game1.Instance.Content.Load<Texture2D>("textures\\world_fantasy\\fern"));
            data.model = CreateModel();
            data.Landscape = landscape;
            data.MaxDepth = 1;
            data.Density = 1;
            data.PopulateFunc = new ObjectPopulator.PopulateFunction((int depth, Rectangle region) => 
            {
                Transform[] transforms = new Transform[data.Density];
                for (int i = 0; i < transforms.Length; i++)
                {
                    Vector3 position = data.Landscape.GetVerticePosition(
                        region.X + ObjectPopulator.rand.Next(region.Width),
                        region.Y + rand.Next(region.Height));
                    transforms[i] = new Transform();
                    transforms[i].Position = position - new Vector3(0, 0, 2.5f);//position;
                    transforms[i].Position += new Vector3(rand.Next(20) / 20.0f, rand.Next(20) / 20.0f, 0);
                    transforms[i].Scale = new Vector3(1, 1, 1) * (0.004f);
                    transforms[i].Rotation = new Vector3(-MathHelper.PiOver2, 0, rand.Next(314)/100.0f);
                    
                }
                return transforms;
            });
            return ObjectPopulator.Generate(data);
        }

        /// <summary>
        /// Crée le modèle 3D.
        /// </summary>
        /// <returns></returns>
        static ModelData CreateModel()
        {
            return Generation.ModelGenerator.GenerateModel(new float[2,2] { {0, 0}, {0, 0} }, 100, 1, true, 1);
            VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[4];
            int s = 20;
            vertices[0] = new VertexPositionNormalTexture(new Vector3(-s, -s, 1), new Vector3(0, 0, 1), new Vector2(0, 0));
            vertices[1] = new VertexPositionNormalTexture(new Vector3(s, -s, 1), new Vector3(0, 0, 1), new Vector2(1, 0));
            vertices[2] = new VertexPositionNormalTexture(new Vector3(s, s, 1), new Vector3(0, 0, 1), new Vector2(1, 1));
            vertices[3] = new VertexPositionNormalTexture(new Vector3(-s, s, 1), new Vector3(0, 0, 1), new Vector2(0, 1));

            short[] indices = new short[] { 0, 1, 2, 0, 2, 3 };

            VertexBuffer buffer = new VertexBuffer(Game1.Instance.GraphicsDevice, VertexPositionNormalTexture.VertexDeclaration, vertices.Length, BufferUsage.None);
            IndexBuffer ibuffer = new IndexBuffer(Game1.Instance.GraphicsDevice, IndexElementSize.SixteenBits, indices.Length, BufferUsage.None);

            ModelData model = new ModelData(buffer, ibuffer);
            return model;
        }
    }
}
