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
namespace Modouv.Fractales.World.Objects.Specialized
{
    /// <summary>
    /// Objet effectuant les traitements pour l'eau.
    /// </summary>
    public class WaterObject : DynamicObject
    {
        #region Variables
        float m_frame = 0.0f;
        Texture2D m_waterTexture;
        #endregion

        /// <summary>
        /// Crée une nouvelle instance de WaterObject.
        /// Ce constructeur charge les ressources nécessaires au fonctionnement de Water Object.
        /// </summary>
        public WaterObject(GraphicalParameters parameters)
        {
            // Crée le shader.
            Shader = Game1.Instance.Content.Load<Effect>("Shaders\\world_fantasy\\water");
            m_waterTexture = Game1.Instance.Content.Load<Texture2D>("textures\\world_fantasy\\mer");
            Shader.Parameters["WaterTexture"].SetValue(m_waterTexture);
            Shader.Parameters["Normals"].SetValue(Generation.Mapping.NormalMapGenerator.Generate(m_waterTexture));
            Regenerate();
        }

        /// <summary>
        /// Définit la reflection map utilisée pour le dessin.
        /// </summary>
        /// <param name="reflectionMap"></param>
        public void SetReflectionMap(Texture2D reflectionMap)
        {
            Shader.Parameters["ReflectionTexture"].SetValue(reflectionMap);
        }
        /// <summary>
        /// Définit la refraction map utilisée pour le dessin.
        /// </summary>
        /// <param name="refractionMap"></param>
        public void SetRefractionMap(Texture2D refractionMap)
        {
            Shader.Parameters["RefractionTexture"].SetValue(refractionMap);
        }
        /// <summary>
        /// Régénère le modèle utilisé pour le dessin de l'eau.
        /// </summary>
        public void Regenerate()
        {
            Position = new Vector3(0, 0, 0);
            Rotation = new Vector3(0, 0, 0);
            float[,] heightmap = new float[256, 256];//Generation.Mapping.HeightmapGenerator.GenerateHeightmap(waterTexture); // new float[192, 192]
            Shader.Parameters["wStartPos"].SetValue(Position);
            Shader.Parameters["wSize"].SetValue(heightmap.GetLength(0));
            Model = Generation.ModelGenerator.GenerateModel(heightmap, 10, -10f, true, 1);  //Generation.ModelGenerator.GenerateModel(heightmap, 1, -0.7f);
        }
        /// <summary>
        /// Mise à jour de la logique de l'eau.
        /// </summary>
        /// <param name="time"></param>
        public void Update(GameTime time)
        {
            m_frame += 0.0000005f * (float)time.ElapsedGameTime.TotalMilliseconds;
        }
        /// <summary>
        /// Dessine l'eau.
        /// </summary>
        /// <param name="world"></param>
        public void Draw(GameWorld world)
        {
            Game1.Instance.GraphicsDevice.BlendState = BlendState.NonPremultiplied;
            Shader.Parameters["wView"].SetValue(world.View);
            Shader.Parameters["WaterFrame"].SetValue(m_frame);
            base.Draw(world);
        }
    }
}
