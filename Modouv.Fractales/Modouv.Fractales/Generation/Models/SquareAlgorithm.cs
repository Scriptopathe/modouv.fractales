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
    public class SquareAlgorithm
    {
        /// <summary>
        /// Générateur de nombres aléatoires.
        /// </summary>
        static Random rand = new Random();
        /// <summary>
        /// Valeur fixe de déplacement du point du milieu pour chaque itération.
        /// </summary>
        public static float middlePointOffset = -1000.0f;
        /// <summary>
        /// Valeur aléatoire maximum de déplacement du point du milieu pour chaque itération
        /// </summary>
        public static int middlePointMaxRandOffset = 7000;

        /// <summary>
        /// Regarde à la position spécifiée sur la heightmap.
        /// Prends en compte une répétition / symétrie de la heightmap.
        /// </summary>
        /// <param name="heightmap"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        static float Lookup(ref float[,] heightmap, int x, int y)
        {
            int sx = heightmap.GetLength(0);
            int sy = heightmap.GetLength(1);
            if (x < 0)
                x = sx -1;
            else if (x >= sx)
                x = 0;
            if (y < 0)
                y = sy - 1;
            else if (y >= sy)
                y = 0;
            return heightmap[x, y];
        }
        
        /// <summary>
        /// Ajoute deux points et retourne le résultat.
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        static Point add(Point p1, Point p2)
        {
            return new Point(p1.X + p2.X, p1.Y + p2.Y);
        }

        public static float[,] GenerateHeightMap(int width, int height, float c1, float c2, float c3, float c4)
        {
            float[,] heightmap = new float[width, height];
            // Remplis la heightmap avec des -1
            for (int x = 0; x < heightmap.GetLength(0); x++)
            {
                for (int y = 0; y < heightmap.GetLength(1); y++)
                {
                    heightmap[x, y] = -1;
                }
            }
            // Remplis les coins
            heightmap[0, 0] = c1;
            heightmap[0, height - 1] = c2;
            heightmap[width - 1, 0] = c3;
            heightmap[width - 1, height - 1] = c4;

            GenerateHeightmap(ref heightmap, new Rectangle(0, 0, width-1, height-1), 0);
            return heightmap;
        }
        /// <summary>
        /// Calcule les points contenus dans compute en utilisant les voisins contenus dans ker.
        /// </summary>
        /// <param name="heightmap"></param>
        /// <param name="compute"></param>
        /// <param name="ker"></param>
        /// <returns></returns>
        static List<Vector3> Compute(ref float[,] heightmap, Point[] compute, Point[] ker)
        {
            List<Vector3> operations = new List<Vector3>();
            foreach (Point pt in compute)
            {
                //if (Lookup(ref heightmap, pt.X, pt.Y) >= 0)
                //    continue;
                // On calcule la nouvelle valeur du point en fonction des voisins.
                float sum = 0;
                int divide = 0;
                foreach (Point kerPt in ker)
                {
                    Point neightboor = add(pt, kerPt);
                    float val = Lookup(ref heightmap, neightboor.X, neightboor.Y);
                    if (val >= 0)
                    {
                        sum += val;
                        divide++;
                    }
                }
                sum = sum / divide;
                operations.Add(new Vector3(pt.X, pt.Y, sum));
            }
            return operations;
        }

        /// <summary>
        /// Génère une heightmap basée sur l'algorithme Diamond Square.
        /// La heightmap doit être initialisée à -1 partout.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static void GenerateHeightmap(ref float[,] heightmap, Rectangle rect, int depth)
        {
            if (rect.Width <= 1 || rect.Height <= 1)
            {
                return;
            }

            // Calcule les valeurs des coins des sous rectangles.
            int mx = rect.X + rect.Width / 2;
            int my = rect.Y + rect.Height / 2;
            int sx = rect.X; int sy = rect.Y;
            int ex = rect.X + rect.Width; int ey = rect.Y + rect.Height;

            // Points à calculer.
            Point[] computeSquare = new Point[] {
                new Point(mx, sy), new Point(mx, ey),
                new Point(sx, my), new Point(ex, my)
            };

            Point[] computeMiddle = new Point[] { new Point(mx, my) };
            mx = rect.Width / 2;
            my = rect.Height / 2;

            // Voisins à utiliser pour le calcul des points
            Point[] ker = new Point[] { new Point(0, my), new Point(0, -my), new Point(-mx, 0), new Point(mx, 0) };
            
            // Calcul des points disposés en carrés
            List<Vector3> operations = Compute(ref heightmap, computeSquare, ker);
            foreach (Vector3 op in operations) { heightmap[(int)op.X, (int)op.Y] = op.Z; }

            // Calcule de manière différente le point du milieu.
            operations = Compute(ref heightmap, computeMiddle, ker);
            
            foreach (Vector3 op in operations)
            {
                // Cette fonction est bien pour avoir des terrains avec plat + 
                //float randValue = (middlePointOffset + rand.Next(middlePointMaxRandOffset))/((float)Math.Exp(depth/2.0f));

                // Cette fonction est pas mal pour avoir une montagne centrée.
                float dstCenter = (float)(Math.Sqrt(Math.Pow(op.X - heightmap.GetLength(0) / 2, 2) + Math.Pow(op.Y - heightmap.GetLength(1) / 2, 2)));
                //float randValue = (middlePointOffset + rand.Next(middlePointMaxRandOffset)) / ((float)Math.Exp(depth/2.0f + dstCenter / 100.0f));
                float randValue = (middlePointOffset + rand.Next(middlePointMaxRandOffset/(depth+1-rand.Next(depth)))) / ((float)Math.Exp(depth / 2.0f + dstCenter / 1000.0f));
                heightmap[(int)op.X, (int)op.Y] = op.Z + randValue;
                
            }


            // Fait de même récursivement sur les sous-rectangles
            Rectangle[] rects = new Rectangle[] { 
                new Rectangle(sx, sy, mx, my), new Rectangle(sx, sy+my, mx, my),
                new Rectangle(sx+mx, sy, mx, my), new Rectangle(sx+mx, sy+my, mx, my)
            };

            foreach (Rectangle r in rects)
            {
                GenerateHeightmap(ref heightmap, r, depth + 1);
            }
        }
    }
}
