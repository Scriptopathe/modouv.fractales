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
using System.Threading;
namespace Modouv.Fractales.World.Objects.Specialized
{
    /// <summary>
    /// Permet la génération d'une map de bruit et de normales.
    /// </summary>
    public class NoiseMap
    {
        #region Variables
        /// <summary>
        /// Texture contenant à la fois la heightmap (x) et la normal map (y, z, w).
        /// </summary>
        Texture2D m_heightmapAndNormalMap;
        Texture2D m_heightmapAndNormalMapBuffer;
        Vector4[] m_data;
        int m_size;
        int[] m_landscapeIndexBuffer;
        Random m_rand = new Random();
        #endregion

        #region Properties
        public Texture2D HeightmapAndNormalMap
        {
            get { return m_heightmapAndNormalMap; }
        }
        public Vector4[] HeightmapAndNormalMapAsData
        {
            get { return m_data; }
        }
        public int[] LandscapeIndexBuffer
        {
            set { m_landscapeIndexBuffer = value; }
        }
        public Generation.Noise.NoiseMapGenerator.NoiseParameters NoiseParameters
        {
            get;
            set;
        }
        Generation.Noise.RidgedMultifractalNoise m_noise;
        #endregion
        /// <summary>
        /// Crée une nouvelle instance de NoiseMap.
        /// </summary>
        public NoiseMap(int size)
        {
            m_heightmapAndNormalMap = new RenderTarget2D(Game1.Instance.GraphicsDevice, size, size, true, SurfaceFormat.Vector4, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
            m_heightmapAndNormalMapBuffer = new RenderTarget2D(Game1.Instance.GraphicsDevice, size, size, true, SurfaceFormat.Vector4, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
            m_data = new Vector4[size * size];
            m_size = size;
            NoiseParameters = new Generation.Noise.NoiseMapGenerator.NoiseParameters();
            m_noise = new Generation.Noise.RidgedMultifractalNoise();
            m_noise.Seed = m_rand.Next();
        }

        /// <summary>
        /// Retourne un vecteur correspondant à la normale contenue dans le Vector4 donné.
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        Vector3 GetNormal(Vector4 vector)
        {
            return new Vector3(vector.Y, vector.Z, vector.W);
        }
        void SetNormal(ref Vector4 vector, Vector3 normal)
        {
            vector.Y = normal.X;
            vector.Z = normal.Y;
            vector.W = normal.Z;
        }
        /// <summary>
        /// Retourne un vecteur correspondant à la position contenue dans le vecteur donné en fonction de la position sur la grille.
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="gridXY"></param>
        /// <returns></returns>
        Vector3 GetPosition(Vector4 vector, Vector2 gridXY)
        {
            return new Vector3(gridXY.X, gridXY.Y, vector.X);
        }
        /// <summary>
        /// Prions pour qu'il n'explose pas :D
        /// </summary>
        Dictionary<Vector2, float> m_cache = new Dictionary<Vector2,float>(10000000);
        object mutex = new object();
        const int GENERATION_THREADS = 4;

        /// <summary>
        /// blabla.
        /// </summary>
        unsafe void DoTheJob(Vector2 offset, int taskStart, int taskEnd)
        {
            for (int x = taskStart; x < taskEnd; x++)
            {
                float sx = (x+offset.X);
                for (int y = 0; y < m_size; y++)
                {
                    float sy = (y+offset.Y);
                    Vector2 vect = new Vector2(sx, sy);
                            

                    fixed (Vector4* pValue = &m_data[x + m_size * y])
                    {
                        bool cacheContains = m_cache.ContainsKey(vect);
                        float value = 0.0f;
                        if (cacheContains)
                        {
                            lock(mutex)
                                value = m_cache[vect];
                        }
                        else
                        {
                            value = (float)m_noise.GetValue(sx / 100.0f, sy / 100.0f, 0);
                            value = (value + 0.85f) * 50;
                            lock(mutex)
                                m_cache.Add(vect, value);
                        }
                                

                        *pValue = new Vector4(value, 0, 0, 0);
                                
                    }
                }
                            


            }
        }
        /// <summary>
        /// Mets à jour la normal map.
        /// </summary>
        public unsafe void Update(Vector2 offset)
        {
            m_noise.Frequency = (NoiseParameters.Frequency);
            m_noise.Lacunarity = (NoiseParameters.Lacunarity);
            m_noise.Quality = (Generation.Noise.NoiseBase.NoiseQuality.QUALITY_FAST);
            m_noise.OctaveCount = 4;
            
            // Version parallèle :
            Thread[] threads = new Thread[GENERATION_THREADS];
            for (int core = 0; core < GENERATION_THREADS; core++)
            {
                // Donne une liste de tâches à effectuer pour chaque core.
                int taskSize = m_size / GENERATION_THREADS;
                int taskStart = taskSize * core;
                int taskEnd = taskStart + taskSize;
                threads[core] = new Thread(new ThreadStart( () => DoTheJob(offset, taskStart, taskEnd)));

                // Démarre le thread
                threads[core].Start();
            }


            // Attend la fin de l'exécution de tous les threads.
            bool ended = false;
            while (!ended)
            {
                ended = true;
                foreach (Thread t in threads)
                {
                    if (t.IsAlive)
                        ended = false;
                }
                Thread.Sleep(1);
            }
            
            for (int i = 0; i < m_landscapeIndexBuffer.Length / 3; i++)
            {
                int i1 = i * 3;
                int i2 = i * 3 + 1;
                int i3 = i * 3 + 2;
                Vector2 gridPos1 = new Vector2((i2) % m_size, (i2) / m_size);
                Vector2 gridPos2 = new Vector2((i1) % m_size, (i1) / m_size);
                Vector2 gridPos3 = new Vector2((i3) % m_size, (i3) / m_size);
                Vector4 v1 = m_data[m_landscapeIndexBuffer[i1]];
                Vector4 v2 = m_data[m_landscapeIndexBuffer[i2]];
                Vector4 v3 = m_data[m_landscapeIndexBuffer[i3]];
                Vector3 firstvec = GetPosition(v2, gridPos1) - GetPosition(v1, gridPos2);
                Vector3 secondvec = GetPosition(v1, gridPos2) - GetPosition(v3, gridPos3);
                Vector3 normal = Vector3.Cross(firstvec, secondvec);
                normal.Normalize();
                SetNormal(ref m_data[m_landscapeIndexBuffer[i1]], normal + GetNormal(v1));
                SetNormal(ref m_data[m_landscapeIndexBuffer[i2]], normal + GetNormal(v2));
                SetNormal(ref m_data[m_landscapeIndexBuffer[i3]], normal + GetNormal(v3));
            } // 

            /*for (int i = 0; i < m_data.Length; i++)
            {
                // Normalise les normales et les retourne par rapport à Z.
                Vector3 normal = GetNormal(m_data[i]);
                normal.Normalize();
                normal.Z = -normal.Z;
                SetNormal(ref m_data[i], normal);
            }*/

            m_heightmapAndNormalMapBuffer.SetData<Vector4>(m_data);
            var temp = m_heightmapAndNormalMap;
            m_heightmapAndNormalMap = m_heightmapAndNormalMapBuffer;
            m_heightmapAndNormalMapBuffer = temp;
        }
    }
}
