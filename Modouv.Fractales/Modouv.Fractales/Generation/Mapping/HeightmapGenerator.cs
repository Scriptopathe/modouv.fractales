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
    public static class HeightmapGenerator
    {
        /// <summary>
        /// Génère une texture de heightmap en prenant en compte la luminosité de la texture.
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static Texture2D GenerateTexture(Texture2D src)
        {
            Color[] data = new Color[src.Width * src.Height];
            src.GetData<Color>(data);
            // On calcule la luminosité de chaque pixel.
            for (int x = 0; x < src.Width; x++)
            {
                for (int y = 0; y < src.Height; y++)
                {
                    Color col = data[x + y * src.Width];
                    float lum = 0.2126f * col.R + 0.7152f * col.G + 0.0722f * col.B;
                    data[x + y * src.Width] = new Color(lum, lum, lum, 1.0f);
                }
            }
            // On crée la texture avec les couleurs calculées.
            Texture2D tex = new Texture2D(Game1.Instance.GraphicsDevice, src.Width, src.Height);
            tex.SetData<Color>(data);
            return tex;
        }
        /// <summary>
        /// Génère une heightmap sous forme de table à deux dimensions en prenant en compte la luminosité de la texture.
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static float[,] GenerateHeightmap(Texture2D src)
        {
            Color[] srcdata = new Color[src.Width * src.Height];
            src.GetData<Color>(srcdata);
            float[,] dstData = new float[src.Width, src.Height];
            // On calcule la luminosité de chaque pixel.
            for (int x = 0; x < src.Width; x++)
            {
                for (int y = 0; y < src.Height; y++)
                {
                    Color col = srcdata[x + y * src.Width];
                    float lum = 0.2126f * col.R + 0.7152f * col.G + 0.0722f * col.B;
                    dstData[x, y] = lum;
                }
            }
            return dstData;
        }
    }
}
