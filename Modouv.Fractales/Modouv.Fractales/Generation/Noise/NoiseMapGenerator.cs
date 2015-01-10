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
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace Modouv.Fractales.Generation.Noise
{
    /// <summary>
    /// Permet la génération d'une carte de bruit dans une heightmap.
    /// </summary>
    public class NoiseMapGenerator
    {
        /// <summary>
        /// Représente une collection de paramètres pouvant être passés à un bruit.
        /// </summary>
        public class NoiseParameters
        {
            /// <summary>
            /// Contient une liste de bruits pouvant être désignés par leur ID.
            /// </summary>
            public static List<Type> Noises = new List<Type>() {
                typeof(PerlinNoise),                    // 0
                typeof(RidgedMultifractalNoise),        // 1
                typeof(VoronoiNoise),                   // 2
                typeof(WhiteNoise),                     // 3
                typeof(GradientNoise),                  // 4
                typeof(GradientCoherentNoise),          // 5
                typeof(ValueCoherentNoise)};            // 6
            static Random s_seedRandom = new Random();
            public const int PERLIN_ID = 0;
            public const int RIDGED_ID = 1;
            public const int VORONOI_ID = 2;
            public const int WHITE_ID = 3;
            /// <summary>
            /// Type de bruit utilisé.
            /// </summary>
            public int NoiseType { get; set; }
            /// <summary>
            /// Nombre d'octaves du bruit.
            /// Plus va valeur est grande, plus le bruit aura du détail.
            /// </summary>
            public int OctaveCount { get; set; }
            /// <summary>
            /// Graine du bruit.
            /// Chaque graine produit un bruit radicalement différent.
            /// 
            /// Si la graine est inférieure à 0, une graine aléatoire sera générée.
            /// </summary>
            public int Seed { get; set; }
            /// <summary>
            /// Coefficient de réduction du bruit entre deux octaves successives.
            /// Plus cette valeur est grande, moins les octaves de bas niveau auront d'importance.
            /// </summary>
            public float Lacunarity { get; set; }
            /// <summary>
            /// Détermine la fréquence initiale du bruit.
            /// 
            /// Plus cette valeur est grande, plus il y aura de variations dans le bruit.
            /// </summary>
            public float Frequency { get; set; }
            /// <summary>
            /// Obtient ou définit la persistance du bruit.
            /// 
            /// Il s'agit du multiplicateur d'amplitude entre deux octaves de bruit successives.
            /// </summary>
            public float Persistence { get; set; }
            /// <summary>
            /// Initialise une nouvelle instance de NoiseParameters avec des valeurs par défaut.
            /// </summary>
            public NoiseParameters()
            {
                OctaveCount = 8;
                Seed = -1;
                Frequency = 1.0f;
                Lacunarity = 5.0f;
                Persistence = 1;
                NoiseType = 0;
            }

            /// <summary>
            /// Crée une instance de bruit à partir des paramètres donnés.
            /// </summary>
            /// <returns></returns>
            public NoiseBase CreateNoise()
            {
                NoiseBase noise = (NoiseBase)Activator.CreateInstance(Noises[NoiseType]);
                noise.Seed = Seed < 0 ? s_seedRandom.Next() : Seed;
                noise.Persistence = Persistence;
                noise.OctaveCount = OctaveCount;
                noise.Lacunarity = Lacunarity;
                noise.Quality = NoiseBase.NoiseQuality.QUALITY_BEST;
                noise.Frequency = Frequency;
                return noise;
            }
        }
        
        const int GENERATION_THREADS = 4;
        /// <summary>
        /// Génère une heighmap à partir d'un bruit Ridged Multifratcal.
        /// Version parrallélisée.
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public static unsafe float[,] GeneratePerlinParallel(int size, NoiseParameters parameters, float vscale=120.0f)
        {
            var noise = parameters.CreateNoise();
            Random rand = new Random();
            float[,] heightmap = new float[size,size];

            Thread[] threads = new Thread[GENERATION_THREADS];
            for (int core = 0; core < GENERATION_THREADS; core++)
            {
                 // Donne une liste de tâches à effectuer pour chaque core.
                int taskSize = size / GENERATION_THREADS;
                int taskStart = taskSize * core;
                int taskEnd = taskStart + taskSize;
                threads[core] = new Thread(new ThreadStart( () =>
                {
                    for (int x = taskStart; x < taskEnd; x++)
                    {
                        for (int y = 0; y < size; y++)
                        {
                            float value = (float)noise.GetValue(x / 512.0f, y / 512.0f, 0);
                            value += 0.75f;
                            fixed (float* pValue = &heightmap[x, y])
                            {
                                *pValue = value * vscale;
                            }
                        }
                    }
                }));

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


            // On passe les bords à 0.
            for (int x = 0; x < heightmap.GetLength(0); x++)
            {
                heightmap[x, 0] = heightmap[x, 0] < 0 ? heightmap[x, 0] : 0;
                heightmap[x, heightmap.GetLength(1) - 1] = heightmap[x, heightmap.GetLength(1) - 1] < 0 ? heightmap[x, heightmap.GetLength(1) - 1] : 0;
            }
            for (int y = 0; y < heightmap.GetLength(1); y++)
            {
                heightmap[0, y] = heightmap[0, y] < 0 ? heightmap[0, y] : 0;
                heightmap[heightmap.GetLength(0) - 1, y] = heightmap[heightmap.GetLength(0) - 1, y] < 0 ? heightmap[heightmap.GetLength(0) - 1, y] : 0;
            }

            return heightmap;
        }
        /// <summary>
        /// Génère une heighmap à partir d'un bruit Ridged Multifratcal.
        /// Version parrallélisée.
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public static unsafe float[,] GenerateMultiNoiseParallel(int size, NoiseParameters repartitionNoiseParams,
            NoiseParameters noiseHighParams,
            NoiseParameters noiseLowParams,
            float vscale = 120.0f)
        {
            Random rand = new Random();
            float[,] heightmap = new float[size, size];

            // Création des bruits.
            NoiseBase repartitionNoise = repartitionNoiseParams.CreateNoise();
            NoiseBase noiseHigh = noiseHighParams.CreateNoise();
            NoiseBase noiseLow = noiseLowParams.CreateNoise();
            

            Thread[] threads = new Thread[GENERATION_THREADS]; 
            for (int core = 0; core < GENERATION_THREADS; core++)
            {
                // Donne une liste de tâches à effectuer pour chaque core.
                int taskSize = size / GENERATION_THREADS;
                int taskStart = taskSize * core;
                int taskEnd = taskStart + taskSize;
                threads[core] = new Thread(new ThreadStart(() =>
                {
                    for (int x = taskStart; x < taskEnd; x++)
                    {
                        for (int y = 0; y < size; y++)
                        {
                            float sx = x/512.0f;
                            float sy = y/512.0f;
                            
                            // Obtention des valeurs des bruits.
                            float repartition = Math.Min(Math.Abs(repartitionNoise.GetValue(sx, sy, 0)), 1);
                            float valueHigh = noiseHigh.GetValue(sx, sy, 0);
                            float valueLow = noiseLow.GetValue(sx, sy, 0);
                            
                            // Interpolation linéaire entre value low et value high de coefficient donné par le bruit de
                            // répartition.
                            float value = (float)(valueLow * (1 - repartition) + valueHigh * (repartition));
                            value += 0.75f;
                            fixed (float* pValue = &heightmap[x, y])
                            {
                                *pValue = value * vscale;
                            }
                        }
                    }
                }));

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


            // On passe les bords à 0.
            for (int x = 0; x < heightmap.GetLength(0); x++)
            {
                heightmap[x, 0] = heightmap[x, 0] < 0 ? heightmap[x, 0] : 0;
                heightmap[x, heightmap.GetLength(1) - 1] = heightmap[x, heightmap.GetLength(1) - 1] < 0 ? heightmap[x, heightmap.GetLength(1) - 1] : 0;
            }
            for (int y = 0; y < heightmap.GetLength(1); y++)
            {
                heightmap[0, y] = heightmap[0, y] < 0 ? heightmap[0, y] : 0;
                heightmap[heightmap.GetLength(0) - 1, y] = heightmap[heightmap.GetLength(0) - 1, y] < 0 ? heightmap[heightmap.GetLength(0) - 1, y] : 0;
            }

            return heightmap;
        }

        /// <summary>
        /// Génère une heighmap à partir d'un bruit Ridged Multifratcal.
        /// Version parrallélisée.
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public static unsafe float[,] GenerateMultiNoiseParallelWithTextures(
            int size,
            ref Texture2D outRepartitionTex, // format color, size = size*size
            ref Texture2D outHighNoiseTex,
            ref Texture2D outLowNoiseTex,
            ref Texture2D outOutputTex,
            NoiseParameters repartitionNoiseParams,
            NoiseParameters noiseHighParams,
            NoiseParameters noiseLowParams,
            float vscale = 120.0f)
        {
            Random rand = new Random();
            float[,] heightmap = new float[size, size];
            // Création des bruits.
            NoiseBase repartitionNoise = repartitionNoiseParams.CreateNoise();
            NoiseBase noiseHigh = noiseHighParams.CreateNoise();
            NoiseBase noiseLow = noiseLowParams.CreateNoise();

            // Création des buffers de couleur.
            Color[] repartitionTexColor = new Color[size * size];
            Color[] highTexColor = new Color[size * size];
            Color[] lowTexColor = new Color[size * size];
            Color[] outputTexColor = new Color[size * size];

            Thread[] threads = new Thread[GENERATION_THREADS];
            for (int core = 0; core < GENERATION_THREADS; core++)
            {
                // Donne une liste de tâches à effectuer pour chaque core.
                int taskSize = size / GENERATION_THREADS;
                int taskStart = taskSize * core;
                int taskEnd = taskStart + taskSize;
                threads[core] = new Thread(new ThreadStart(() =>
                {
                    for (int x = taskStart; x < taskEnd; x++)
                    {
                        for (int y = 0; y < size; y++)
                        {
                            float sx = x / 512.0f;
                            float sy = y / 512.0f;

                            // Obtention des valeurs des bruits.
                            float repartition = Math.Min((repartitionNoise.GetValue(sx, sy, 0)+1)/2, 1);
                            float valueHigh = noiseHigh.GetValue(sx, sy, 0)+1f;
                            float valueLow = noiseLow.GetValue(sx, sy, 0)/2;

                            // Remplissage des textures.
                            int index = x + y * size;
                            fixed (Color* pColor = &repartitionTexColor[index])
                            {
                                float repartitionLum = (float)repartition;
                                *pColor = new Color(repartitionLum, repartitionLum, repartitionLum); // range [0, 1]
                            }

                            fixed (Color* pColor = &highTexColor[index])
                            {
                                float valueHighLum = (float)valueHigh/2;
                                *pColor = new Color(valueHighLum, valueHighLum, valueHighLum); // range [0, 1]
                            }

                            fixed (Color* pColor = &lowTexColor[index])
                            {
                                float valueLowLum = (float)valueLow+0.5f;
                                *pColor = new Color(valueLowLum, valueLowLum, valueLowLum); // range [0, 1]
                            }

                            // Interpolation linéaire entre value low et value high de coefficient donné par le bruit de
                            // répartition.
                            float value = (float)(valueLow * (1 - repartition) + valueHigh * (repartition));
                            fixed (Color* pColor = &outputTexColor[index])
                            {
                                float valueLum = (float)value / 2;
                                *pColor = new Color(valueLum, valueLum, valueLum);
                            }

                            value -= 0.25f;
                            fixed (float* pValue = &heightmap[x, y])
                            {
                                *pValue = value * vscale;

                            }
                        }
                    }
                }));

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


            // On passe les bords à 0.
            for (int x = 0; x < heightmap.GetLength(0); x++)
            {
                heightmap[x, 0] = heightmap[x, 0] < 0 ? heightmap[x, 0] : 0;
                heightmap[x, heightmap.GetLength(1) - 1] = heightmap[x, heightmap.GetLength(1) - 1] < 0 ? heightmap[x, heightmap.GetLength(1) - 1] : 0;
            }
            for (int y = 0; y < heightmap.GetLength(1); y++)
            {
                heightmap[0, y] = heightmap[0, y] < 0 ? heightmap[0, y] : 0;
                heightmap[heightmap.GetLength(0) - 1, y] = heightmap[heightmap.GetLength(0) - 1, y] < 0 ? heightmap[heightmap.GetLength(0) - 1, y] : 0;
            }

            // Update des textures.
            outRepartitionTex.SetData<Color>(repartitionTexColor);
            outHighNoiseTex.SetData<Color>(highTexColor);
            outLowNoiseTex.SetData<Color>(lowTexColor);
            outOutputTex.SetData<Color>(outputTexColor);

            return heightmap;
        }

        /// <summary>
        /// Génère une heighmap à partir d'un bruit Ridged Multifratcal.
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public static float[,] Generate(int size, NoiseParameters parameters)
        {
            
            RidgedMultifractalNoise noise = new RidgedMultifractalNoise();
            Random rand = new Random();
            float z = rand.Next(2000000)/1000.0f;
            float[,] heightmap = new float[size,size];

            // Paramétrage du bruit.
            noise.OctaveCount = parameters.OctaveCount;
            noise.Seed = parameters.Seed >= 0 ? parameters.Seed : rand.Next();
            noise.Lacunarity = parameters.Lacunarity;
            noise.Frequency = parameters.Frequency;
            
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    float value = (float)noise.GetValue(x / 512.0f, y / 512.0f, 0);
                    value += 0.75f;
                    heightmap[x, y] = value*120;
                }
            }
            
            return heightmap;
        }
    }
}
