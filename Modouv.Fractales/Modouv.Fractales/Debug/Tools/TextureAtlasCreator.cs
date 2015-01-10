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
namespace Modouv.Fractales.Debug.Tools
{
    /// <summary>
    /// Crée et sauvegarde un atlas de textures à partir de textures existantes.
    /// </summary>
    public class TextureAtlasCreator
    {
        public static string ContentTexturesPath = "../../../../Modouv.Fractales.Content/textures/";
        static Texture2D LoadTexture(string texName)
        {
            string fullName = ContentTexturesPath + texName;
            System.IO.Stream stream = System.IO.File.Open(fullName, System.IO.FileMode.Open);
            Texture2D tex = Texture2D.FromStream(Game1.Instance.GraphicsDevice, stream);
            stream.Close();
            return tex;
        }
        /// <summary>
        /// Crée un atlas de 4 textures.
        /// </summary>
        /// <param name="src"></param>
        /// <param name="output"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        public static Texture2D CreateFromTextures(string[] src, string output, int w, int h)
        {
            GraphicsDevice device = Game1.Instance.GraphicsDevice;
            if (src.Length != 4)
                throw new Exception();
            // Charge les textures
            Texture2D[] textures = new Texture2D[4];
            for (int i = 0; i < 4; i++)
            {
                textures[i] = LoadTexture(src[i]);
            }
            RenderTarget2D target = new RenderTarget2D(Game1.Instance.GraphicsDevice, w, h);
            
            SpriteBatch batch = new SpriteBatch(Game1.Instance.GraphicsDevice);
            device.SetRenderTarget(target);
            device.BlendState = BlendState.NonPremultiplied;
            device.Clear(new Color(255, 255, 255, 0));
            batch.Begin();
            for (int i = 0; i < 4; i++)
            {
                int mx = w/2;
                int my = h/2;
                batch.Draw(textures[i],
                    new Microsoft.Xna.Framework.Rectangle((i % 2) * mx, (i / 2) * my, mx, my),
                    Color.White);
            }
            batch.End();
            device.SetRenderTarget(null);
            System.IO.Stream s = System.IO.File.Open(ContentTexturesPath + output, System.IO.FileMode.Create);
            target.SaveAsPng(s, w, h);
            s.Close();

            return target;
        }

        /// <summary>
        /// Crée une texture tilable à partir de la texture donnée.
        /// </summary>
        /// <param name="src"></param>
        /// <param name="output"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        public static Texture2D CreateTilableTextures(string src, string output, int w, int h, bool mirror=true)
        {
            GraphicsDevice device = Game1.Instance.GraphicsDevice;
            // Charge les textures
            Texture2D texture = LoadTexture(src);
            RenderTarget2D target = new RenderTarget2D(Game1.Instance.GraphicsDevice, w, h);

            SpriteBatch batch = new SpriteBatch(Game1.Instance.GraphicsDevice);
            device.SetRenderTarget(target);
            device.BlendState = BlendState.NonPremultiplied;
            device.Clear(new Color(255, 255, 255, 0));
            batch.Begin();
            SpriteEffects[] effects;
            if (mirror)
                effects = new SpriteEffects[4] { SpriteEffects.None, SpriteEffects.FlipHorizontally, 
                            SpriteEffects.FlipVertically, SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically };
            else
                effects = new SpriteEffects[4] { SpriteEffects.None, SpriteEffects.None, SpriteEffects.None, SpriteEffects.None };

            for (int i = 0; i < 4; i++)
            {
                int mx = w / 2;
                int my = h / 2;

                batch.Draw(texture,
                    new Microsoft.Xna.Framework.Rectangle((i % 2) * mx, (i / 2) * my, mx, my),
                    null,
                    Color.White,
                    0.0f,
                    Vector2.Zero,
                    effects[i],
                    0.0f
                    );
            }
            
            batch.End();
            device.SetRenderTarget(null);
            System.IO.Stream s = System.IO.File.Open(ContentTexturesPath + output, System.IO.FileMode.Create);
            target.SaveAsPng(s, w, h);
            s.Close();

            return target;
        }
    }
}
