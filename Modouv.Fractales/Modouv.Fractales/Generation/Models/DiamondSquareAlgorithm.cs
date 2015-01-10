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

namespace Modouv.Fractales.Generation.Models
{
    /// <summary>
    /// Classe statique permettant la génération de heightmap grâce au Diamond Square Algorithm.
    /// </summary>
    public static class DiamondSquareAlgorithm
    {
        /// <summary>
        /// Type de fonction représentant une fonction capable de générer un nombre pseudo-aléatoire ajusté
        /// en fonction de l'altitude courante, de la profondeur de calcul, et de la taille des samples.
        /// </summary>
        /// <returns></returns>
        public delegate float RandomFunctionDelegate(float currentZ, int depth, float sampleSize);
        /// <summary>
        /// Heightmap.
        /// </summary>
        static float[,] s_heightmap;
        /// <summary>
        /// Générateur de nombres aléatoires.
        /// </summary>
        static Random s_rand;
        /// <summary>
        /// Valeur utilisée par la fonction de génération de nombres aléatoires spécifiant le cap de la valeur
        /// aléatoire pouvant être générée.
        /// </summary>
        static int s_maxRandValue = 820;
        /// <summary>
        /// Valeur utilisée par la fonction de génération de nombres aléatoires spécifiant la valeur de base à laquelle
        /// s'ajoute une valeur aléatoire.
        /// </summary>
        static int s_baseRandValue = -380;

        /// <summary>
        /// Exemple de fonction pour la génération de nombres aléatoires.
        /// </summary>
        public static float BaseRandFunc(float currentZ, int depth, float sampleSize)
        {
            // Détermine si on est à la dernière passe.
            bool lastDepth = Math.Abs(sampleSize) < 1.01f;
            if(lastDepth) // dernière passe : on adoucit.
                return (s_baseRandValue + s_rand.Next(s_maxRandValue)) / (float)Math.Exp(depth / 1f);
            if (depth <= 1)
                return (s_baseRandValue + s_rand.Next(s_maxRandValue)) / 30;
            if (currentZ < -3)
                return (s_baseRandValue * 0.65f + s_rand.Next(s_maxRandValue)*0.80f) / (float)Math.Exp(depth / 1.7f);
            else if (currentZ > 30)
                return (s_baseRandValue*1.2f + s_rand.Next(s_maxRandValue)*1.7f) / (float)Math.Exp(depth / 1.4f);
            else if (currentZ > 52)
                return (s_baseRandValue * 2 + s_rand.Next(s_maxRandValue) * 2) / (float)Math.Exp(depth / 1.8f);
            else
                return (s_baseRandValue+s_rand.Next(s_maxRandValue))/(float)Math.Exp(depth/1.3f);
        }

        /// <summary>
        /// Deuxième fonction pour la génération aléatoire de paysage.
        /// Paysage avec + de relief.
        /// </summary>
        public static float BaseRandFunc2(float currentZ, int depth, float sampleSize)
        {
            // Détermine si on est à la dernière passe.
            bool lastDepth = Math.Abs(sampleSize) < 1.01f;
            if (lastDepth) // dernière passe : on adoucit.
                return (s_baseRandValue + s_rand.Next(s_maxRandValue)) / (float)Math.Exp(depth / 1f);
            if (depth <= 3)
                return (s_baseRandValue + s_rand.Next(s_maxRandValue)) / 30;
            if (currentZ < -3)
                return (s_baseRandValue * 0.65f + s_rand.Next(s_maxRandValue) * 0.80f) / (float)Math.Exp(depth / 2.4f);
            else if (currentZ > 12)
                return (s_baseRandValue * 2 + s_rand.Next(s_maxRandValue) * 1.7f) / (float)Math.Exp(depth / 1.9f);
            else if (currentZ > 30)
                return (s_baseRandValue * 2 + s_rand.Next(s_maxRandValue) * 1.7f) / (float)Math.Exp(depth / 1.7f);
            else if (currentZ > 52)
                return (s_baseRandValue * 2 + s_rand.Next(s_maxRandValue) * 2) / (float)Math.Exp(depth / 1.4f);
            else
                return (s_baseRandValue + s_rand.Next(s_maxRandValue)) / (float)Math.Exp(depth / 1.3f);
        }

        /// <summary>
        /// Troisième fonction pour la génération de paysage.
        /// Très basique.
        /// </summary>
        public static float BaseRandFunc3(float currentZ, int depth, float sampleSize)
        {
            return (s_baseRandValue + s_rand.Next(s_maxRandValue)) * sampleSize / 2000;
        }

        static RandomFunctionDelegate s_randomFunction;
        /// <summary>
        /// Génère une heightmap basée sur l'algorithme Diamond Square.
        /// </summary>
        /// <param name="width">Largeur de la heightmap</param>
        /// <param name="height">Hauteur de la heightmap.</param>
        /// <param name="seed">Graine du générateur de nombre aléatoire, -1 pour une graine aléatoire.</param>
        /// <returns></returns>
        public static float[,] GenerateHeightmap(int width, int height, float vscale, float c1, float c2, float c3, float c4, RandomFunctionDelegate randomFunction,
                                                 int maxRandValue=820, int baseRandValue=-380, int seed=-1)
        {
            // Valeurs max
            s_maxRandValue = maxRandValue;
            s_baseRandValue = baseRandValue;
            s_randomFunction = randomFunction;

            // Set up du générateur de nombres aléatoires.
            if (seed < 0)
                s_rand = new Random();
            else
                s_rand = new Random(seed);

            // Crée la heightmap
            float[,] heightmap = new float[width, height];
            // Initialise les coins de la heightmap.
            heightmap[0, 0] = c1;
            heightmap[width - 1, 0] = c2;
            heightmap[width - 1, height - 1] = c3;
            heightmap[0, height - 1] = c4;
            s_heightmap = heightmap;
            
            int sampleSize = (width-1);
            int depth = 1;
            while (sampleSize > 1)
            {
                // On effectue la passe sur tous les carrés.
                for (int x = sampleSize / 2; x < width - sampleSize / 2; x += sampleSize)
                {
                    for (int y = sampleSize / 2; y < height - sampleSize / 2; y += sampleSize)
                    {
                        SampleSquare(x, y, sampleSize, depth);
                    }
                }

                // Les diamants
                for (int x = 0; x < width - sampleSize; x += sampleSize)
                {
                    for (int y = 0; y < height - sampleSize; y += sampleSize)
                    {
                        SampleDiamond(x + sampleSize / 2, y, sampleSize, depth);
                        SampleDiamond(x, y + sampleSize / 2, sampleSize, depth);
                    }
                }
                depth += 1;
                sampleSize /= 2;
            }

            // Application de l'échelle
            for (int x = 0; x < s_heightmap.GetLength(0); x++)
            {
                for (int y = 0; y < s_heightmap.GetLength(1); y++)
                {
                    s_heightmap[x, y] = 0.5f+s_heightmap[x, y]*vscale;
                }
            }

            // On passe les bords à 0.
            for (int x = 0; x < s_heightmap.GetLength(0); x++)
            {
                s_heightmap[x, 0] = s_heightmap[x, 0] < 0 ? s_heightmap[x, 0] : 0;
                s_heightmap[x, s_heightmap.GetLength(1) - 1] = s_heightmap[x, s_heightmap.GetLength(1) - 1] < 0 ? s_heightmap[x, s_heightmap.GetLength(1) - 1] : 0;
            }
            for (int y = 0; y < s_heightmap.GetLength(1); y++)
            {
                s_heightmap[0, y] = s_heightmap[0, y] < 0 ?  s_heightmap[0, y] : 0;
                s_heightmap[s_heightmap.GetLength(0) - 1, y] = s_heightmap[s_heightmap.GetLength(0) - 1, y] < 0 ? s_heightmap[s_heightmap.GetLength(0) - 1, y]: 0;
            }


            // Retourne la heightmap crée
            return heightmap;
        }
        /// <summary>
        /// Effectue le traitement d'un 'carré'.
        /// </summary>
        static void SampleSquare(int x, int y, int sampleSize, int depth)
        {
            //  a     b
            //     x  
            //  c     d
            float a = Get(x - sampleSize / 2, y - sampleSize / 2);
            float b = Get(x + sampleSize / 2, y - sampleSize / 2);
            float c = Get(x - sampleSize / 2, y + sampleSize / 2);
            float d = Get(x + sampleSize / 2, y + sampleSize / 2);
            float center = (a + b + c + d) / 4;
            center += s_randomFunction(center, depth, sampleSize);
            Set(x, y, center);
        }

        /// <summary>
        /// Effectue le traitement d'un 'diamant' (=losange)
        /// </summary>
        static void SampleDiamond(int x, int y, int sampleSize, int depth)
        {
            //     a   
            //  d  x  b
            //     c
            float a = Get(x, y - sampleSize / 2);
            float b = Get(x + sampleSize / 2, y);
            float c = Get(x, y + sampleSize / 2);
            float d = Get(x - sampleSize / 2, y);
            float center = (a + b + c + d) / 4;
            center += s_randomFunction(center, depth, sampleSize);
            Set(x, y, center);
        }

        /// <summary>
        /// Retourne la valeur de la heightmap, aux coordonnées x et y.
        /// Si le couple de coordonnées (x, y) est en dehors des bords de la heightmap, 
        /// la valeur retournée est celle de l'extrémité.
        /// </summary>
        static float Get(int x, int y)
        {
            if (x < 0)
                x = 0;
            if (x > s_heightmap.GetLength(0))
                x = s_heightmap.GetLength(0) - 1;
            if (y < 0)
                y = 0;
            if (y > s_heightmap.GetLength(1))
                y = s_heightmap.GetLength(1);
            return s_heightmap[x, y];
        }

        /// <summary>
        /// Affecte à la heightmap, aux coordonnées x et y, la value val.
        /// Si le couple de coordonnées (x, y) est en dehors des bords de la heightmap, 
        /// la valeur affectée est celle de l'extrémité.
        /// </summary>
        static void Set(int x, int y, float val)
        {
            if (x < 0)
                x = 0;
            if (x > s_heightmap.GetLength(0))
                x = s_heightmap.GetLength(0) - 1;
            if (y < 0)
                y = 0;
            if (y > s_heightmap.GetLength(1))
                y = s_heightmap.GetLength(1);

            s_heightmap[x, y] = val;
        }
    }
}
