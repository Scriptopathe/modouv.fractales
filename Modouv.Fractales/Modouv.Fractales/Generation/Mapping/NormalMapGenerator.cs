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
namespace Modouv.Fractales.Generation.Mapping
{
    /// <summary>
    /// Permet de générer une texture de heightmap à partir d'une texture.
    /// </summary>
    public static class NormalMapGenerator
    {
        static Effect s_normalMapGenerationEffect;
        /// <summary>
        /// Génère une texture de heightmap en prenant en compte la luminosité de la texture.
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static Texture2D Generate(float[,] heightmap)
        {
            Color[] data = new Color[heightmap.GetLength(0) * heightmap.GetLength(1)];

            // Récupère les vertices avec les normales précalculées depuis le générateur de modèles
            var vertices = Generation.ModelGenerator.GenerateVertexBuffer(heightmap, 1, 1);
            // Crée la texture à partir des normales
            for (int i = 0; i < vertices.Length; i++)
            {
                data[i] = new Color(vertices[i].Normal);
            }
            Texture2D tex = new Texture2D(Game1.Instance.GraphicsDevice, heightmap.GetLength(0), heightmap.GetLength(1));
            tex.SetData<Color>(data);
            return tex;
        }
        /// <summary>
        /// Génère une texture de heightmap en prenant en compte la luminosité de la texture.
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static Vector3[,] GenerateArray(float[,] heightmap)
        {
            Vector3[,] data = new Vector3[heightmap.GetLength(0), heightmap.GetLength(1)];

            // Récupère les vertices avec les normales précalculées depuis le générateur de modèles
            var vertices = Generation.ModelGenerator.GenerateVertexBuffer(heightmap, 1, 1);
            // Crée la texture à partir des normales
            for (int i = 0; i < vertices.Length; i++)
            {
                int x = i % heightmap.GetLength(0);
                int y = i / heightmap.GetLength(0);
                data[x, y] = vertices[i].Normal;
            }
            return data;
        }
        /// <summary>
        /// Génère une normal map à partir d'une texture (dont on extrait la heightmap puis calcule
        /// la normal map).
        /// </summary>
        /// <param name="texture"></param>
        /// <returns></returns>
        public static Texture2D Generate(Texture2D texture)
        {
            float[,] heightmap = HeightmapGenerator.GenerateHeightmap(texture);
            return Generate(heightmap);
        }

        public static Texture2D GenerateGPU(Texture2D texture)
        {
            RenderTarget2D dstTexture = new RenderTarget2D(Game1.Instance.GraphicsDevice, texture.Width, texture.Height, true, SurfaceFormat.Color, DepthFormat.None);
            if (s_normalMapGenerationEffect == null)
            {
                s_normalMapGenerationEffect = Game1.Instance.Content.Load<Effect>("Shaders\\preprocess\\GenerateNormals");
            }
            s_normalMapGenerationEffect.Parameters["pWidth"].SetValue(1.0f / texture.Width);
            s_normalMapGenerationEffect.Parameters["pHeight"].SetValue(1.0f / texture.Height);

            lock(Game1.GraphicsDeviceMutex)
            {
                Game1.Instance.GraphicsDevice.SetRenderTarget(dstTexture);
                Game1.Instance.Batch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
                s_normalMapGenerationEffect.CurrentTechnique.Passes[0].Apply();
                Game1.Instance.Batch.Draw(texture, dstTexture.Bounds, Color.White);
                Game1.Instance.Batch.End();
            }

            return dstTexture;
        }
    }
}
