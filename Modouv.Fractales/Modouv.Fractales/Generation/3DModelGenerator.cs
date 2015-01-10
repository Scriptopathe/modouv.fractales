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
using Modouv.Fractales.MathHelpers;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
namespace Modouv.Fractales.Generation
{

    /// <summary>
    /// Générateur de modèle 3D.
    /// </summary>
    public class ModelGenerator
    {
        /// <summary>
        /// Type de vertex permettant de stocker une couleur, une position, une normal
        /// </summary>
        public struct VertexPositionColorNormalTexture
        {
            public Vector3 Position;
            public Color Color;
            public Vector3 Normal;
            public Vector3 TextureCoord;

            public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
            (
                new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                new VertexElement(sizeof(float) * 3, VertexElementFormat.Color, VertexElementUsage.Color, 0),
                new VertexElement(sizeof(float) * 3 + 4, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
                new VertexElement(sizeof(float) * 6 + 4, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 0)
            );

            public VertexPositionColorNormalTexture(Vector3 position, Color color, Vector3 textureCoord)
            {
                Position = position;
                Color = color;
                Normal = Vector3.Zero;
                TextureCoord = textureCoord;
            }
        }
        /// <summary>
        /// Génère un modèle 3D à partir d'une heightmap.
        /// </summary>
        /// <param name="heightmap"></param>
        /// <returns></returns>
        public static VertexPositionColorNormalTexture[] GenerateVertexBuffer(float[,] heightmap, float hscale, float vscale=1.0f)
        {

            // Taille du buffer.
            int size = heightmap.GetLength(0) * heightmap.GetLength(1);


            // Création du vertex buffer contenant tous les vertex à dessiner.
            VertexPositionColorNormalTexture[] vertexBuffer = new VertexPositionColorNormalTexture[size];
            for (int y = 0; y < heightmap.GetLength(1); y++)
            {
                for (int x = 0; x < heightmap.GetLength(0); x++)
                {
                    vertexBuffer[x + y * heightmap.GetLength(0)] = new VertexPositionColorNormalTexture(
                        new Vector3((x-heightmap.GetLength(0)/2)*hscale, 
                                    (y-heightmap.GetLength(1)/2)*hscale,
                                    heightmap[x, y]*vscale),
                        Color.White,
                        new Vector3((float)x/((float)heightmap.GetLength(0)-1), (float)y/((float)heightmap.GetLength(1)-1), 0)
                        );
                }
            }

            // Index buffer contenant l'ordre dans lequel dessiner les vertex. (sous forme de carrés).
            int[] indexBuffer = new int[(heightmap.GetLength(0) - 1)*(heightmap.GetLength(1) - 1)*6];
            int sizeX = heightmap.GetLength(0);
            int sizeY = heightmap.GetLength(1);
            int startIndex = 0;
            for (int x = 0; x < sizeX-1; x++)
            {
                for (int y = 0; y < sizeY-1; y++)
                {
                    int firstIndex = x + y * (sizeX);
                    int topLeft = firstIndex;
                    int topRight = firstIndex + 1;
                    int lowerLeft = topLeft + sizeX;
                    int lowerRight = lowerLeft + 1;
                    // Triangle 1 (up right)
                    indexBuffer[startIndex++] = topLeft;
                    indexBuffer[startIndex++] = lowerRight;
                    indexBuffer[startIndex++] = lowerLeft;
                    
                    // Triangle 2 (bottom left)
                    indexBuffer[startIndex++] = topLeft;
                    indexBuffer[startIndex++] = topRight;
                    indexBuffer[startIndex++] = lowerRight;
                }
            }


            // Calcule les normales aux surfaces.
            // Merci Riemer's XNA Tutorial :D
            for (int i = 0; i < vertexBuffer.Length; i++)
                vertexBuffer[i].Normal = new Vector3(0, 0, 0);

            for (int i = 0; i < indexBuffer.Length / 3; i++)
            {
                Vector3 firstvec = vertexBuffer[indexBuffer[i * 3 + 1]].Position - vertexBuffer[indexBuffer[i * 3]].Position;
                Vector3 secondvec = vertexBuffer[indexBuffer[i * 3]].Position - vertexBuffer[indexBuffer[i * 3 + 2]].Position;
                Vector3 normal = Vector3.Cross(firstvec, secondvec);
                normal.Normalize();
                vertexBuffer[indexBuffer[i * 3]].Normal += normal;
                vertexBuffer[indexBuffer[i * 3 + 1]].Normal += normal;
                vertexBuffer[indexBuffer[i * 3 + 2]].Normal += normal;
            }

            for (int i = 0; i < vertexBuffer.Length; i++)
            {
                vertexBuffer[i].Normal.Z = -vertexBuffer[i].Normal.Z;
                vertexBuffer[i].Normal.Normalize();
            }

            return vertexBuffer;
        }
        /// <summary>
        /// Génère un modèle 3D à partir d'une heightmap.
        /// </summary>
        /// <param name="heightmap">La heightmap</param>
        /// <param name="hscale">Ajustement de l'échelle horizontale.</param>
        /// <param name="vscale">Ajustement de l'échelle verticale.</param>
        /// <param name="center">Si vrai, les coordonnées des vertex du modèles seront centrées en (0, 0)</param>
        /// <returns></returns>
        public static World.ModelData GenerateModel(float[,] heightmap, float hscale, float vscale = 1.0f, bool center = true, int quality=1)
        {
            return GenerateModel(heightmap, hscale, vscale, center, 0, 0, heightmap.GetLength(0) - 1, heightmap.GetLength(1) - 1, quality);
        }

        /// <summary>
        /// Génère un modèle 3D à partir d'une heightmap.
        /// 
        /// </summary>
        /// <param name="heightmap">La heightmap</param>
        /// <param name="hscale">Ajustement de l'échelle horizontale.</param>
        /// <param name="vscale">Ajustement de l'échelle verticale.</param>
        /// <param name="center">Si vrai, les coordonnées des vertex du modèles seront centrées en (0, 0)</param>
        /// <param name="texOffsetX">Offset des coordonées de texture selon X.</param>
        /// <param name="texOffsetY">Offset des coordonées de texture selon Y</param>
        /// <param name="texScaleX">Diviseur d'échelle horitontale des coordonnées de la texture</param>
        /// <param name="quality">Qualité du modèle : 1 qualité max. Au dessus de 1 : qualité de plus en plus grossière.</param>
        /// <returns></returns>
        public static List<World.ModelData> GenerateModel(int maxQuality, float[,] heightmap, float hscale, float vscale, bool center,
            float texOffsetX = 0.0f, float texOffsetY = 0.0f, float texScaleX = 1.0f, float texScaleY = 1.0f)
        {

            // Taille du buffer.
            List<World.ModelData> models = new List<World.ModelData>();
            int size = heightmap.GetLength(0) * heightmap.GetLength(1);


            // Création du vertex buffer contenant tous les vertex à dessiner.
            VertexPositionColorNormalTexture[] vertexBuffer = new VertexPositionColorNormalTexture[size];
            for (int y = 0; y < heightmap.GetLength(1); y++)
            {
                for (int x = 0; x < heightmap.GetLength(0); x++)
                {
                    // Si center, on centre les vertices sur le point (0, 0).
                    Vector3 positionOffset = Vector3.Zero;
                    if (center)
                        positionOffset = new Vector3((x - heightmap.GetLength(0) / 2) * hscale,
                                    (y - heightmap.GetLength(1) / 2) * hscale,
                                    heightmap[x, y] * vscale);
                    else
                        positionOffset = new Vector3(x * hscale, y * hscale, heightmap[x, y] * vscale);

                    // Création du vertex.
                    vertexBuffer[x + y * heightmap.GetLength(0)] = new VertexPositionColorNormalTexture(
                        positionOffset,
                        Color.White,
                        new Vector3((x + texOffsetX) / texScaleX, (y + texOffsetY) / texScaleY, 0)
                        );
                }
            }

            // Index buffer contenant l'ordre dans lequel dessiner les vertex. (sous forme de carrés).
            // Il permet de calculer les normales de manière précise.
            int[] indexBuffer = new int[(heightmap.GetLength(0) - 1) * (heightmap.GetLength(1) - 1) * 6];
            int sizeX = heightmap.GetLength(0);
            int sizeY = heightmap.GetLength(1);
            int startIndex = 0;
            for (int x = 0; x < sizeX - 1; x++)
            {
                for (int y = 0; y < sizeY - 1; y++)
                {
                    int firstIndex = x + y * (sizeX);
                    int topLeft = firstIndex;
                    int topRight = firstIndex + 1;
                    int lowerLeft = topLeft + sizeX;
                    int lowerRight = lowerLeft + 1;
                    // Triangle 1 (up right)
                    indexBuffer[startIndex++] = topLeft;
                    indexBuffer[startIndex++] = lowerRight;
                    indexBuffer[startIndex++] = lowerLeft;

                    // Triangle 2 (bottom left)
                    indexBuffer[startIndex++] = topLeft;
                    indexBuffer[startIndex++] = topRight;
                    indexBuffer[startIndex++] = lowerRight;
                }
            }


            // Calcule les normales aux surfaces.

            for (int i = 0; i < vertexBuffer.Length; i++)
                vertexBuffer[i].Normal = new Vector3(0, 0, 0);

            for (int i = 0; i < indexBuffer.Length / 3; i++)
            {
                Vector3 firstvec = vertexBuffer[indexBuffer[i * 3 + 1]].Position - vertexBuffer[indexBuffer[i * 3]].Position;
                Vector3 secondvec = vertexBuffer[indexBuffer[i * 3]].Position - vertexBuffer[indexBuffer[i * 3 + 2]].Position;
                Vector3 normal = Vector3.Cross(firstvec, secondvec);
                normal.Normalize();
                vertexBuffer[indexBuffer[i * 3]].Normal += normal;
                vertexBuffer[indexBuffer[i * 3 + 1]].Normal += normal;
                vertexBuffer[indexBuffer[i * 3 + 2]].Normal += normal;
            }
            for (int i = 0; i < vertexBuffer.Length; i++)
            {
                vertexBuffer[i].Normal.Z = -vertexBuffer[i].Normal.Z;
                vertexBuffer[i].Normal.Normalize();
            }



            // Crée finalement les vertex et index buffers et les assigne à un modèle.
            var vertices = new VertexBuffer(Game1.Instance.GraphicsDevice, VertexPositionColorNormalTexture.VertexDeclaration, size, BufferUsage.None);
            vertices.SetData<VertexPositionColorNormalTexture>(vertexBuffer);
            var indices = new IndexBuffer(Game1.Instance.GraphicsDevice, IndexElementSize.ThirtyTwoBits, indexBuffer.Count(), BufferUsage.None);
            indices.SetData<int>(indexBuffer);

            // Si la qualité max est incluse, on l'ajoute aux modèles.
            models.Add(new World.ModelData(vertices, indices));

            // Crée les index buffer des qualités inférieures.
            int quality = 2;
            int[] lastBuffer = indexBuffer;
            while (quality <= maxQuality)
            {
                int div = (int)Math.Pow(2, quality-1);
                lastBuffer = GetSubdividedIndexBuffer(indexBuffer, heightmap.GetLength(0), heightmap.GetLength(1), div);
                // Crée et ajoute les indices subdivisés.
                IndexBuffer indicesMipmap = new IndexBuffer(Game1.Instance.GraphicsDevice, IndexElementSize.ThirtyTwoBits, lastBuffer.Count(), BufferUsage.None);
                indicesMipmap.SetData<int>(lastBuffer);
                models.Add(new World.ModelData(vertices, indicesMipmap));
                quality++;
            }

            return models;
        }

        /// <summary>
        /// Crée et retourne une version de qualité divisée par deux de l'index buffer.
        /// </summary>
        /// <param name="indexBuffer"></param>
        /// <returns></returns>
        static int[] GetSubdividedIndexBuffer(int[] indexBufferSrc, int width, int height, int div)
        {
            // Index buffer contenant l'ordre dans lequel dessiner les vertex. (sous forme de carrés).
            // Il permet de calculer les normales de manière précise.
            
            int sizeX = width/div;
            int sizeY = height/div;
            int[] indexBuffer = new int[sizeX * sizeY * 6];
            int startIndex = 0;
            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    int firstIndex = x*div + y * width * div;
                    int topLeft = firstIndex;
                    int topRight = firstIndex + div;
                    int lowerLeft = topLeft + div*width;
                    int lowerRight = lowerLeft + div;
                    // Triangle 1 (up right)
                    indexBuffer[startIndex++] = topLeft;
                    indexBuffer[startIndex++] = lowerRight;
                    indexBuffer[startIndex++] = lowerLeft;

                    // Triangle 2 (bottom left)
                    indexBuffer[startIndex++] = topLeft;
                    indexBuffer[startIndex++] = topRight;
                    indexBuffer[startIndex++] = lowerRight;
                }
            }
            return indexBuffer;
        }

        /// <summary>
        /// Génère un modèle 3D à partir d'une heightmap.
        /// </summary>
        /// <param name="heightmap">La heightmap</param>
        /// <param name="hscale">Ajustement de l'échelle horizontale.</param>
        /// <param name="vscale">Ajustement de l'échelle verticale.</param>
        /// <param name="center">Si vrai, les coordonnées des vertex du modèles seront centrées en (0, 0)</param>
        /// <param name="texOffsetX">Offset des coordonées de texture selon X.</param>
        /// <param name="texOffsetY">Offset des coordonées de texture selon Y</param>
        /// <param name="texScaleX">Diviseur d'échelle horitontale des coordonnées de la texture</param>
        /// <param name="quality">Qualité du modèle : 1 qualité max. Au dessus de 1 : qualité de plus en plus grossière.</param>
        /// <returns></returns>
        public static World.ModelData GenerateModel(float[,] heightmap, float hscale, float vscale, bool center,
            float texOffsetX = 0.0f, float texOffsetY = 0.0f, float texScaleX = 1.0f, float texScaleY = 1.0f, int quality = 1)
        {
            if (quality != 1)
            {
                hscale *= quality;
                texOffsetX /= quality;
                texOffsetY /= quality;
                texScaleX /= quality;
                texScaleY /= quality;
                heightmap = CopyArea(heightmap, quality);
            }


            // Taille du buffer.
            int size = heightmap.GetLength(0) * heightmap.GetLength(1);


            // Création du vertex buffer contenant tous les vertex à dessiner.
            VertexPositionColorNormalTexture[] vertexBuffer = new VertexPositionColorNormalTexture[size];
            for (int y = 0; y < heightmap.GetLength(1); y++)
            {
                for (int x = 0; x < heightmap.GetLength(0); x++)
                {
                    // Si center, on centre les vertices sur le point (0, 0).
                    Vector3 positionOffset = Vector3.Zero;
                    if (center)
                        positionOffset = new Vector3((x - heightmap.GetLength(0) / 2) * hscale,
                                    (y - heightmap.GetLength(1) / 2) * hscale,
                                    heightmap[x, y] * vscale);
                    else
                        positionOffset = new Vector3(x * hscale, y * hscale, heightmap[x, y] * vscale);

                    // Création du vertex.
                    vertexBuffer[x + y * heightmap.GetLength(0)] = new VertexPositionColorNormalTexture(
                        positionOffset,
                        Color.White,
                        new Vector3((x + texOffsetX) / texScaleX, (y + texOffsetY) / texScaleY, 0)
                        );
                }
            }

            // Index buffer contenant l'ordre dans lequel dessiner les vertex. (sous forme de carrés).
            int[] indexBuffer = new int[(heightmap.GetLength(0) - 1) * (heightmap.GetLength(1) - 1) * 6];
            int sizeX = heightmap.GetLength(0);
            int sizeY = heightmap.GetLength(1);
            int startIndex = 0;
            for (int x = 0; x < sizeX - 1; x++)
            {
                for (int y = 0; y < sizeY - 1; y++)
                {
                    int firstIndex = x + y * (sizeX);
                    int topLeft = firstIndex;
                    int topRight = firstIndex + 1;
                    int lowerLeft = topLeft + sizeX;
                    int lowerRight = lowerLeft + 1;
                    // Triangle 1 (up right)
                    indexBuffer[startIndex++] = topLeft;
                    indexBuffer[startIndex++] = lowerRight;
                    indexBuffer[startIndex++] = lowerLeft;

                    // Triangle 2 (bottom left)
                    indexBuffer[startIndex++] = topLeft;
                    indexBuffer[startIndex++] = topRight;
                    indexBuffer[startIndex++] = lowerRight;
                }
            }


            // Calcule les normals aux surfaces.
            // Merci Riemer's XNA Tutorial :D
            for (int i = 0; i < vertexBuffer.Length; i++)
                vertexBuffer[i].Normal = new Vector3(0, 0, 0);

            for (int i = 0; i < indexBuffer.Length / 3; i++)
            {
                Vector3 firstvec = vertexBuffer[indexBuffer[i * 3 + 1]].Position - vertexBuffer[indexBuffer[i * 3]].Position;
                Vector3 secondvec = vertexBuffer[indexBuffer[i * 3]].Position - vertexBuffer[indexBuffer[i * 3 + 2]].Position;
                Vector3 normal = Vector3.Cross(firstvec, secondvec);
                normal.Normalize();
                vertexBuffer[indexBuffer[i * 3]].Normal += normal;
                vertexBuffer[indexBuffer[i * 3 + 1]].Normal += normal;
                vertexBuffer[indexBuffer[i * 3 + 2]].Normal += normal;
            }
            for (int i = 0; i < vertexBuffer.Length; i++)
            {
                vertexBuffer[i].Normal.Z = -vertexBuffer[i].Normal.Z;
                vertexBuffer[i].Normal.Normalize();
            }


            // Crée finalement les vertex et index buffers et les assigne à un modèle.
            var vertices = new VertexBuffer(Game1.Instance.GraphicsDevice, VertexPositionColorNormalTexture.VertexDeclaration, size, BufferUsage.None);
            vertices.SetData<VertexPositionColorNormalTexture>(vertexBuffer);
            var indices = new IndexBuffer(Game1.Instance.GraphicsDevice, IndexElementSize.ThirtyTwoBits, indexBuffer.Count(), BufferUsage.None);
            indices.SetData<int>(indexBuffer);

            World.ModelData model = new World.ModelData(vertices, indices);
            return model;
        }

        #region Vertical
        /// <summary>
        /// Génère un modèle 3D à partir d'une heightmap.
        /// </summary>
        /// <param name="heightmap">La heightmap</param>
        /// <param name="hscale">Ajustement de l'échelle horizontale.</param>
        /// <param name="vscale">Ajustement de l'échelle verticale.</param>
        /// <param name="center">Si vrai, les coordonnées des vertex du modèles seront centrées en (0, 0)</param>
        /// <returns></returns>
        public static World.ModelData GenerateModelV(float[,] heightmap, float hscale, float vscale = 1.0f, bool center = true, int quality = 1)
        {
            return GenerateModelV(heightmap, hscale, vscale, center, 0, 0, heightmap.GetLength(0) - 1, heightmap.GetLength(1) - 1, quality);
        }

        /// <summary>
        /// Génère un modèle 3D à partir d'une heightmap.
        /// </summary>
        /// <param name="heightmap">La heightmap</param>
        /// <param name="hscale">Ajustement de l'échelle horizontale.</param>
        /// <param name="vscale">Ajustement de l'échelle verticale.</param>
        /// <param name="center">Si vrai, les coordonnées des vertex du modèles seront centrées en (0, 0)</param>
        /// <param name="texOffsetX">Offset des coordonées de texture selon X.</param>
        /// <param name="texOffsetY">Offset des coordonées de texture selon Y</param>
        /// <param name="texScaleX">Diviseur d'échelle horitontale des coordonnées de la texture</param>
        /// <param name="quality">Qualité du modèle : 1 qualité max. Au dessus de 1 : qualité de plus en plus grossière.</param>
        /// <returns></returns>
        public static World.ModelData GenerateModelV(float[,] heightmap, float hscale, float vscale, bool center,
            float texOffsetX = 0.0f, float texOffsetY = 0.0f, float texScaleX = 1.0f, float texScaleY = 1.0f, int quality = 1)
        {
            if (quality != 1)
            {
                hscale *= quality;
                texOffsetX /= quality;
                texOffsetY /= quality;
                texScaleX /= quality;
                texScaleY /= quality;
                heightmap = CopyArea(heightmap, quality);
            }


            // Taille du buffer.
            int size = heightmap.GetLength(0) * heightmap.GetLength(1);


            // Création du vertex buffer contenant tous les vertex à dessiner.
            VertexPositionColorNormalTexture[] vertexBuffer = new VertexPositionColorNormalTexture[size];
            for (int y = 0; y < heightmap.GetLength(1); y++)
            {
                for (int x = 0; x < heightmap.GetLength(0); x++)
                {
                    // Si center, on centre les vertices sur le point (0, 0).
                    Vector3 positionOffset = Vector3.Zero;
                    if (center)
                        positionOffset = new Vector3(
                                    x  * hscale,
                                    heightmap[x, y] * vscale,
                                    -y * hscale
                                    );
                    else
                        positionOffset = new Vector3(x * hscale, heightmap[x, y] * vscale, y * hscale);

                    // Création du vertex.
                    vertexBuffer[x + y * heightmap.GetLength(0)] = new VertexPositionColorNormalTexture(
                        positionOffset,
                        Color.White,
                        new Vector3((x + texOffsetX) / texScaleX, (y + texOffsetY) / texScaleY, 0)
                        );
                }
            }

            // Index buffer contenant l'ordre dans lequel dessiner les vertex. (sous forme de carrés).
            int[] indexBuffer = new int[(heightmap.GetLength(0) - 1) * (heightmap.GetLength(1) - 1) * 6];
            int sizeX = heightmap.GetLength(0);
            int sizeY = heightmap.GetLength(1);
            int startIndex = 0;
            for (int x = 0; x < sizeX - 1; x++)
            {
                for (int y = 0; y < sizeY - 1; y++)
                {
                    int firstIndex = x + y * (sizeX);
                    int topLeft = firstIndex;
                    int topRight = firstIndex + 1;
                    int lowerLeft = topLeft + sizeX;
                    int lowerRight = lowerLeft + 1;
                    // Triangle 1 (up right)
                    indexBuffer[startIndex++] = topLeft;
                    indexBuffer[startIndex++] = lowerRight;
                    indexBuffer[startIndex++] = lowerLeft;

                    // Triangle 2 (bottom left)
                    indexBuffer[startIndex++] = topLeft;
                    indexBuffer[startIndex++] = topRight;
                    indexBuffer[startIndex++] = lowerRight;
                }
            }


            // Calcule les normals aux surfaces.
            // Merci Riemer's XNA Tutorial :D
            for (int i = 0; i < vertexBuffer.Length; i++)
                vertexBuffer[i].Normal = new Vector3(0, 0, 0);

            for (int i = 0; i < indexBuffer.Length / 3; i++)
            {
                Vector3 firstvec = vertexBuffer[indexBuffer[i * 3 + 1]].Position - vertexBuffer[indexBuffer[i * 3]].Position;
                Vector3 secondvec = vertexBuffer[indexBuffer[i * 3]].Position - vertexBuffer[indexBuffer[i * 3 + 2]].Position;
                Vector3 normal = Vector3.Cross(firstvec, secondvec);
                normal.Normalize();
                vertexBuffer[indexBuffer[i * 3]].Normal += normal;
                vertexBuffer[indexBuffer[i * 3 + 1]].Normal += normal;
                vertexBuffer[indexBuffer[i * 3 + 2]].Normal += normal;
            }
            for (int i = 0; i < vertexBuffer.Length; i++)
            {
                vertexBuffer[i].Normal.Z = -vertexBuffer[i].Normal.Z;
                vertexBuffer[i].Normal.Normalize();
            }



            // Crée finalement les vertex et index buffers et les assigne à un modèle.
            var vertices = new VertexBuffer(Game1.Instance.GraphicsDevice, VertexPositionColorNormalTexture.VertexDeclaration, size, BufferUsage.None);
            vertices.SetData<VertexPositionColorNormalTexture>(vertexBuffer);
            var indices = new IndexBuffer(Game1.Instance.GraphicsDevice, IndexElementSize.ThirtyTwoBits, indexBuffer.Count(), BufferUsage.None);
            indices.SetData<int>(indexBuffer);

            World.ModelData model = new World.ModelData(vertices, indices);
            return model;
        }
        #endregion
        /// <summary>
        /// Crée et retourne une copie de la partie définie par le Rectangle area de la heightmap donnée.
        /// </summary>
        /// <param name="heightmap"></param>
        /// <param name="area"></param>
        /// <returns></returns>
        static float[,] CopyArea(float[,] heightmap, int quality)
        {
            int w = heightmap.GetLength(0) / quality;
            int h = heightmap.GetLength(1) / quality;
            float[,] subMap = new float[w+1, h+1];
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    subMap[x, y] = heightmap[x*quality, y*quality];
                }
            }

            // Pour que les frontières soient exactement les mêmes que la version en + grande qualité :
            for (int x = 0; x < w + 1; x++)
                subMap[x, h] = heightmap[x*quality, heightmap.GetLength(1) - 1];
            for (int y = 0; y < h + 1; y++)
                subMap[w, y] = heightmap[heightmap.GetLength(0) - 1, y*quality];

            return subMap;
        }

        /*/// <summary>
        /// Génère un modèle 3D à partir d'une heightmap.
        /// </summary>
        /// <param name="heightmap">La heightmap</param>
        /// <param name="hscale">Ajustement de l'échelle horizontale.</param>
        /// <param name="vscale">Ajustement de l'échelle verticale.</param>
        /// <param name="center">Si vrai, les coordonnées des vertex du modèles seront centrées en (0, 0)</param>
        /// <param name="texOffsetX">Offset des coordonées de texture selon X.</param>
        /// <param name="texOffsetY">Offset des coordonées de texture selon Y</param>
        /// <param name="texScaleX">Diviseur d'échelle horitontale des coordonnées de la texture</param>
        /// <param name="quality">Qualité du modèle : 1 qualité max. Au dessus de 1 : qualité de plus en plus grossière.</param>
        /// <returns></returns>
        public static World.ModelData GenerateModel(float[,] heightmap, float hscale, float vscale, bool center, 
            float texOffsetX=0.0f, float texOffsetY=0.0f, float texScaleX=1.0f, float texScaleY=1.0f, int quality=2)
        {

            // Taille du buffer.
            int size = heightmap.GetLength(0) * heightmap.GetLength(1) / quality;
            int ly = heightmap.GetLength(1) - heightmap.GetLength(1) % quality;
            int lx = heightmap.GetLength(0) - heightmap.GetLength(0) % quality;

            // Création du vertex buffer contenant tous les vertex à dessiner.
            VertexPositionColorNormalTexture[] vertexBuffer = new VertexPositionColorNormalTexture[size];
            int vbufferIndex = 0;
            for (int y = 0; y < ly; y+=quality)
            {
                for (int x = 0; x < lx; x+=quality)
                {
                    // Si center, on centre les vertices sur le point (0, 0).
                    Vector3 positionOffset = Vector3.Zero;
                    if (center)
                        positionOffset = new Vector3((x - heightmap.GetLength(0) / 2) * hscale,
                                    (y - heightmap.GetLength(1) / 2) * hscale,
                                    heightmap[x, y] * vscale);
                    else
                        positionOffset = new Vector3(x * hscale, y * hscale, heightmap[x, y] * vscale);

                    // Création du vertex.
                    vertexBuffer[vbufferIndex] = new VertexPositionColorNormalTexture(
                        positionOffset,
                        Color.White,
                        new Vector3((x+texOffsetX) / texScaleX, (y+texOffsetY) / texScaleY, 0)
                        );

                    vbufferIndex++;
                }
            }

            // Index buffer contenant l'ordre dans lequel dessiner les vertex. (sous forme de carrés).
            int qualitySqr = quality * quality;
            int[] indexBuffer = new int[(heightmap.GetLength(0) - 1) * (heightmap.GetLength(1) - 1) * 6 / qualitySqr];
            int sizeX = heightmap.GetLength(0);
            int sizeY = heightmap.GetLength(1);
            int startIndex = 0;
            for (int x = 0; x < (sizeX - 1)/quality; x++)
            {
                for (int y = 0; y < (sizeY - 1)/quality; y++)
                {
                    int firstIndex = x + y * (sizeX/quality);
                    int topLeft = firstIndex;
                    int topRight = firstIndex + 1;
                    int lowerLeft = topLeft + sizeX/quality;
                    int lowerRight = lowerLeft + 1;
                    // Triangle 1 (up right)
                    indexBuffer[startIndex++] = topLeft;
                    indexBuffer[startIndex++] = lowerRight;
                    indexBuffer[startIndex++] = lowerLeft;

                    // Triangle 2 (bottom left)
                    indexBuffer[startIndex++] = topLeft;
                    indexBuffer[startIndex++] = topRight;
                    indexBuffer[startIndex++] = lowerRight;
                }
            }


            // Calcule les normals aux surfaces.
            // Merci Riemer's XNA Tutorial :D
            for (int i = 0; i < vertexBuffer.Length; i++)
                vertexBuffer[i].Normal = new Vector3(0, 0, 0);

            for (int i = 0; i < indexBuffer.Length / 3; i++)
            {
                Vector3 firstvec = vertexBuffer[indexBuffer[i * 3 + 1]].Position - vertexBuffer[indexBuffer[i * 3]].Position;
                Vector3 secondvec = vertexBuffer[indexBuffer[i * 3]].Position - vertexBuffer[indexBuffer[i * 3 + 2]].Position;
                Vector3 normal = Vector3.Cross(firstvec, secondvec);
                normal.Normalize();
                vertexBuffer[indexBuffer[i * 3]].Normal += normal;
                vertexBuffer[indexBuffer[i * 3 + 1]].Normal += normal;
                vertexBuffer[indexBuffer[i * 3 + 2]].Normal += normal;
            }
            for (int i = 0; i < vertexBuffer.Length; i++)
                vertexBuffer[i].Normal.Normalize();



            // Crée finalement les vertex et index buffers et les assigne à un modèle.
            var vertices = new VertexBuffer(Game1.Instance.GraphicsDevice, VertexPositionColorNormalTexture.VertexDeclaration, size, BufferUsage.None);
            vertices.SetData<VertexPositionColorNormalTexture>(vertexBuffer);
            var indices = new IndexBuffer(Game1.Instance.GraphicsDevice, IndexElementSize.ThirtyTwoBits, indexBuffer.Count(), BufferUsage.None);
            indices.SetData<int>(indexBuffer);

            World.ModelData model = new World.ModelData(vertices, indices);
            return model;
        }
        */


    }
}
