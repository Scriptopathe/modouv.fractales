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
namespace Modouv.Fractales.World.Objects.Particles
{
    /// <summary>
    /// Particule de neige.
    /// </summary>
    public class SnowParticle : IParticle
    {
        /// <summary>
        /// Générateur de nombres aléatoires utilisé pour l'apparition de la neige.
        /// </summary>
        static Random s_rand = new Random();
        static Texture2D s_tex;
        /// <summary>
        /// Position de la particule.
        /// </summary>
        Vector3 m_position;
        /// <summary>
        /// Vitesse de la particle en px/seconde.
        /// </summary>
        Vector3 m_velocity;
        /// <summary>
        /// Taille en pixels de la particule.
        /// </summary>
        Point m_sizePx;
        /// <summary>
        /// Temps de vie de la particule en secondes.
        /// </summary>
        float m_totalLifeTime;
        /// <summary>
        /// Temps de vie écoulé de la particule.
        /// </summary>
        float m_elapsedLifeTime;
        /// <summary>
        /// Durée en secondes des animations de fade in / fade out.
        /// </summary>
        float m_fadeDuration;
        /// <summary>
        /// Taille initiale de la particule.
        /// </summary>
        Point m_initialSize;
        /// <summary>
        /// Couleur de la particule.
        /// </summary>
        Color m_color;
        /// <summary>
        /// Initialise une nouvelle instance de SnowParticle.
        /// </summary>
        public SnowParticle()
        {
            Reset(Vector3.Zero);
            if (s_tex == null)
            {
                s_tex = Game1.Instance.Content.Load<Texture2D>("textures\\world_fantasy\\flocon");
            }
        }

        /// <summary>
        /// Effectue une mise à zéro de la particule.
        /// </summary>
        void Reset(Vector3 cameraPosition)
        {
            int resolution = 1900;
            float posMax = 24f;
            int signSide = s_rand.Next(2) == 0 ? 1 : -1;
            int signUp = s_rand.Next(2) == 0 ? 1 : -1;
            int maxLength = 96; // 96;

            // Positionne les particules autour du joueur.
            m_position = cameraPosition + GameWorld.Instance.Camera.Front*(-maxLength/2 + s_rand.Next(maxLength*100)/100.0f)
                - GameWorld.Instance.Camera.Right * s_rand.Next(resolution) * (posMax / resolution) * signSide
                + GameWorld.Instance.Camera.Up * s_rand.Next(resolution) * (posMax / resolution) * signUp;  

            // Vitesse des particules.
            m_velocity = new Vector3(0, 0, 1) * (4+s_rand.Next(200)/100.0f) 
                + new Vector3(s_rand.Next(100), s_rand.Next(100), s_rand.Next(100))/50;

            // Durée de vie : max 1sec
            m_totalLifeTime = (50 + s_rand.Next(50)) / 100.0f;
            m_fadeDuration = m_totalLifeTime / 2;
            
            m_elapsedLifeTime = 0;
            int size = s_rand.Next(16) + 24;
            m_sizePx = new Point(size, size);
            //m_sizePx = new Point(s_rand.Next(512), s_rand.Next(512));
            m_initialSize = m_sizePx;

            m_color = Color.White;

            if (Game1.Instance.Scene is Scenes.SceneFantasyWorld)
            {
                Scenes.SceneFantasyWorld scene = (Scenes.SceneFantasyWorld)Game1.Instance.Scene;
                if (scene.WorldSeason == Scenes.SceneFantasyWorld.Season.Summer)
                {
                    // Particules de couleur rouge / orange.
                    int r = s_rand.Next(255);
                    int g = s_rand.Next(200);
                    int b = s_rand.Next(68);
                    m_color = new Color(r, g, b, 255);
                }
            }
        }

        /// <summary>
        /// Dessine la particule de neige.
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="time"></param>
        public void Draw(SpriteBatch batch, GameTime time)
        {
            // Temps de vie écoulé.
            m_elapsedLifeTime += (float)(time.ElapsedGameTime.TotalSeconds);

            // Update de la vélocité
            if (Game1.Instance.Scene is Scenes.SceneFantasyWorld)
            {
                Scenes.SceneFantasyWorld scene = (Scenes.SceneFantasyWorld)Game1.Instance.Scene;
                Vector3 normalizedWindDirection = Vector3.Normalize(scene.WindAttractor.CurrentPosition);
                normalizedWindDirection.Z = 0;
                m_velocity += normalizedWindDirection;
            }
            // Mise à jour de la position.
            m_position += (m_velocity * (float)time.ElapsedGameTime.TotalSeconds);
            
            // Calcul de l'alpha.
            float alpha = lerp(0, 1, saturate(m_elapsedLifeTime / m_fadeDuration));
            if (Math.Abs(alpha - 1) <= 0.0001f)
            {
                // Si on est après le fade in, on regarde si on est dans le fade out.
                alpha = lerp(0, 1, saturate((m_totalLifeTime - m_elapsedLifeTime) / m_fadeDuration));
            }
            Color color = new Color(m_color.R, m_color.G, m_color.B, (int)(alpha*120));

            // Opérations matricielles pour avoir le truc en 3D.
            var proj = GameWorld.Instance.Projection;
            var view = GameWorld.Instance.View;
            var world = Matrix.CreateTranslation(m_position);
            var worldViewProj = world * view * proj;

            // Calcul de la position en 2D de la particule.
            Vector4 pos2D = Vector4.Transform(new Vector4(0, 0, 0, 1), worldViewProj);
            pos2D.X = pos2D.X * Game1.Instance.ResolutionWidth / pos2D.W;
            pos2D.Y = Game1.Instance.ResolutionHeight - pos2D.Y * Game1.Instance.ResolutionHeight / pos2D.W;
            pos2D.X /= 2;
            pos2D.Y /= 2;
            pos2D.Z /= 16;
            m_sizePx.X = Math.Min(m_initialSize.X, (int)(m_initialSize.X / pos2D.Z));//(int)lerp(m_initialSize.X, 0, m_elapsedLifeTime / m_totalLifeTime);
            m_sizePx.Y = Math.Min(m_initialSize.Y, (int)(m_initialSize.Y / pos2D.Z));//(int)lerp(m_initialSize.Y, 0, m_elapsedLifeTime / m_totalLifeTime);
            
            if(pos2D.Z > 0)
                batch.Draw(s_tex, new Rectangle((int)pos2D.X+Game1.Instance.ResolutionWidth/2,
                    (int)pos2D.Y, m_sizePx.X, m_sizePx.Y), color);

            

            // Reset de la particule si sa durée de vie est expirée.
            if (m_elapsedLifeTime >= m_totalLifeTime)
            {
                Reset(GameWorld.Instance.Camera.Position);
            }
        }


        /// <summary>
        /// Borne la valeur val entre 0 et 1.
        /// </summary>
        float saturate(float val)
        {
            return clamp(val, 0, 1);
        }
        /// <summary>
        /// Borne la valeur val entre les valeurs min et max fournies.
        /// </summary>
        float clamp(float val, float min, float max)
        {
            return Math.Max(min, Math.Min(max, val));
        }
        /// <summary>
        /// Interpolation linéaire entre val1 et val2.
        /// </summary>
        float lerp(float val1, float val2, float x)
        {
            return val1 * (1 - x) + val2 * x;
        }
    }


}
