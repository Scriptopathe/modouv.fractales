// Copyright (C) 2013, 2014 Jacques Lucas, 
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

// The developer's email is jacquesATetudDOOOTinsa-toulouseDOOOTfr (for valid email, replace 
// capital letters by the corresponding character)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Modouv.Fractales.World;
using Modouv.Fractales.Generation;
using VertexPositionColorNormalTexture = Modouv.Fractales.Generation.ModelGenerator.VertexPositionColorNormalTexture;
namespace Modouv.Fractales.Generation.Models
{
    /// <summary>
    /// Générateur de modèle 3D de fougère.
    /// </summary>
    public class FractalTreeModelGenerator
    {
        //Param par default
        const int VERTICES_PER_SECTION = 8;
        const int MIN_VERTICES_PER_SECTION = 4;
        const int SECTIONS = 8;

        /// <summary>
        /// Repère de l'espace
        /// </summary>
        struct Frame
        {
            //Constructeurs
            public Frame(Vector3 Origin, Vector3 OX_Vector, Vector3 OY_Vector, Vector3 OZ_Vector) : this()
            {
                O = Origin;
                OX = OX_Vector;
                OY = OY_Vector;
                OZ = OZ_Vector;
            }
            public Frame(Frame frame) : this()
            {
                O = frame.O;
                OX = frame.OX;
                OY = frame.OY;
                OZ = frame.OZ;
            }

            //Rotation autour de X
            public void Yaw(float angle)
            {
                Matrix Mat = Matrix.CreateFromAxisAngle(OX, angle);
                OY = Vector3.Transform(OY, Mat);
                OZ = Vector3.Transform(OZ, Mat);
            }
            //Rotation autour de Y
            public void Pitch(float angle)
            {
                Matrix Mat = Matrix.CreateFromAxisAngle(OY, angle);
                OX = Vector3.Transform(OX, Mat);
                OZ = Vector3.Transform(OZ, Mat);
            }
            //Rotation autour de Z
            public void Roll(float angle)
            {
                Matrix Mat = Matrix.CreateFromAxisAngle(OZ, angle);
                OX = Vector3.Transform(OX, Mat);
                OY = Vector3.Transform(OY, Mat);
            }
            //Rotation à l'aide d'une matrice
            public void Rotate(Matrix Mat)
            {
                OX = Vector3.Transform(OX, Mat);
                OY = Vector3.Transform(OY, Mat);
                OZ = Vector3.Transform(OZ, Mat);
            }

            //Position du repère
            public Vector3 O{ get; set; }
            //Vecteurs
            public Vector3 OX{ get; set; }
            public Vector3 OY{ get; set; }
            public Vector3 OZ{ get; set; }
        }

        /// <summary>
        /// Contient les paramètres d'une branche
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        ///
        public class Branch
        {
            //Constructeurs
            public Branch(float relativeLength, float relativeFinalRadius, float yaw, float pitch, float roll)
            {
                m_relativeLength = relativeLength;
                m_relativeFinalRadius = relativeFinalRadius;
                m_yaw = yaw;
                m_pitch = pitch;
                m_roll = roll;
            }

            //Attributs de la branche
            public float m_relativeLength { get; set; }
            public float m_relativeFinalRadius { get; set; }
            //Angles définissant la position relative de la branche
            public float m_yaw { get; set; }
            public float m_pitch { get; set; }
            public float m_roll { get; set; }
        }


        static Random s_leaveRandom = new Random();
        /// <summary>
        /// Génère une leave
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        ///
        private static void AddLeave(Frame frame,
                                      float scale,
                                      VertexPositionColorNormalTexture[] vertexBuffer,
                                      int startVB,
                                      int[] indexBuffer,
                                      int startIB)
        {
            
            float x;
            float z;

            Vector3 fPosTopLeft;
            Vector3 fPosTopRigth;
            Vector3 fPosBottomLeft;
            Vector3 fPosBottomRigth;

            Vector2 texOffset;
            Vector2 texSize;
            if (s_leaveRandom.Next(20) == 0)
            {
                // Fruit
                texOffset = new Vector2(0.25f, 0);
                texSize = new Vector2(0.25f, 0.50f);

                //Coordonnées du rectangle
                x = scale * -0.5f;
                z = 0;
                fPosBottomLeft = Vector3.Transform(new Vector3(x, 0, 0), Matrix.CreateWorld(frame.O, -1 * frame.OZ, -1 * frame.OY));
                fPosBottomLeft.Z += -z + scale;
                
                x = scale * 0.5f;
                fPosBottomRigth = Vector3.Transform(new Vector3(x, 0, 0), Matrix.CreateWorld(frame.O, -1 * frame.OZ, -1 * frame.OY));
                fPosBottomRigth.Z += -z + scale;

                x = scale * -0.5f;
                z = scale * 1;
                fPosTopLeft = Vector3.Transform(new Vector3(x, 0, 0), Matrix.CreateWorld(frame.O, -1 * frame.OZ, -1 * frame.OY));
                fPosTopLeft.Z += -z + scale;

                x = scale * 0.5f;
                fPosTopRigth = Vector3.Transform(new Vector3(x, 0, 0), Matrix.CreateWorld(frame.O, -1 * frame.OZ, -1 * frame.OY));
                fPosTopRigth.Z += -z + scale;
            }

            else
            {
                // Feuille
                texOffset = Vector2.Zero;
                texSize = new Vector2(0.25f, 0.50f);

                //Coordonnées du rectangle
                x = scale * -0.5f;
                z = 0;
                fPosTopLeft = Vector3.Transform(new Vector3(x, 0, z), Matrix.CreateWorld(frame.O, -1 * frame.OZ, -1 * frame.OY));
                x = scale * 0.5f;
                fPosTopRigth = Vector3.Transform(new Vector3(x, 0, z), Matrix.CreateWorld(frame.O, -1 * frame.OZ, -1 * frame.OY));
                x = scale * -0.5f;
                z = scale * 1;
                fPosBottomLeft = Vector3.Transform(new Vector3(x, 0, z), Matrix.CreateWorld(frame.O, -1 * frame.OZ, -1 * frame.OY));
                x = scale * 0.5f;
                fPosBottomRigth = Vector3.Transform(new Vector3(x, 0, z), Matrix.CreateWorld(frame.O, -1 * frame.OZ, -1 * frame.OY));
            }

            //Coordonnées des textures
            vertexBuffer[startVB] = new VertexPositionColorNormalTexture(fPosTopRigth, new Color(0.1f, 0.0f, 0.0f), Vector3.UnitZ);
            vertexBuffer[startVB].TextureCoord = new Vector3(texOffset.X + texSize.X, texOffset.Y + texSize.Y, 0.0f);

            vertexBuffer[++startVB] = new VertexPositionColorNormalTexture(fPosBottomRigth, new Color(0.1f, 0.0f, 0.0f), Vector3.UnitZ);
            vertexBuffer[startVB].TextureCoord = new Vector3(texOffset.X + texSize.X, texOffset.Y, 1.0f);

            vertexBuffer[++startVB] = new VertexPositionColorNormalTexture(fPosBottomLeft, new Color(0.1f, 0.0f, 0.0f), Vector3.UnitZ);
            vertexBuffer[startVB].TextureCoord = new Vector3(texOffset.X, texOffset.Y, 1.0f);

            vertexBuffer[++startVB] = new VertexPositionColorNormalTexture(fPosTopLeft, new Color(0.1f, 0.0f, 0.0f), Vector3.UnitZ);
            vertexBuffer[startVB].TextureCoord = new Vector3(texOffset.X, texOffset.Y + texSize.Y, 0.0f);

            //Génération de l'index buffer
            indexBuffer[startIB] = startVB - 3;
            indexBuffer[++startIB] = startVB - 2;
            indexBuffer[++startIB] = startVB - 1;

            indexBuffer[++startIB] = startVB - 1;
            indexBuffer[++startIB] = startVB;
            indexBuffer[++startIB] = startVB - 3;

            return;
        }

        /// <summary>
        /// Génère une branche
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        ///
        private static Frame AddBranch(Frame frame,
                                      int verticesPerSection,
                                      int sections,
                                      float startRadius,
                                      float finalRadius,
                                      float length,
                                      float scale,
                                      Vector3 torsion,
                                      VertexPositionColorNormalTexture[] vertexBuffer,
                                      int startVB,
                                      int[] indexBuffer,
                                      int startIB)
        {
            float radius = startRadius;
            //Calcul de la position des points
            for (int j = 0; j < sections + 1; j++)
            {
                for (int i = 0; i < verticesPerSection; i++)
                {
                    float x = scale * radius * (float)System.Math.Cos(2 * i * (System.Math.PI / verticesPerSection));
                    float y = scale * radius * (float)System.Math.Sin(2 * i * (System.Math.PI / verticesPerSection));
                    Vector3 fPos = new Vector3(x, y, 0);

                    //Calcul dans le référentiel world
                    fPos = Vector3.Transform(fPos, Matrix.CreateWorld(frame.O, -1*frame.OZ, -1*frame.OY));

                    vertexBuffer[startVB + i + j * verticesPerSection] = new VertexPositionColorNormalTexture(fPos, new Color(radius * scale, 0.0f, 0.0f), Vector3.UnitZ);
                }

                //Calcul du nouveau repère de coordonné après transformation
                if (j < sections)
                {
                    frame.Yaw(torsion.X * (2.0f/15) * (float)System.Math.Sin(1.0f * (System.Math.PI / (float)sections)));
                    frame.Pitch(torsion.Y * (2.0f / 15) * (float)System.Math.Sin(1.0f * (System.Math.PI / (float)sections)));
                    frame.Roll(torsion.Z * (8.0f / (float)sections) * 1.74f * 0.314159f);
                    frame.O = Vector3.Transform(frame.O, Matrix.CreateTranslation(scale * (length / sections) * frame.OZ));

                    radius += (finalRadius - startRadius) / (float)(sections);
                }
            }

            //IndexBuffer & texture
            for (int level = 0; level < sections; level++)
            {
                for (int vertex = 0; vertex < verticesPerSection - 1; vertex++)
                {
                    int firstVertex = (vertex + level * verticesPerSection);
                    int firstIndex = 6 * firstVertex;
                    firstVertex += startVB;
                    firstIndex += startIB;

                    int lowerLeft = firstVertex;
                    int lowerRight = firstVertex + 1;
                    int topLeft = lowerLeft + verticesPerSection;
                    int topRight = topLeft + 1;

                    //Maping texture
                    if (2*(vertex) < verticesPerSection)
                    {
                        vertexBuffer[lowerLeft].TextureCoord = new Vector3(0.5f + (float)vertex / (float)(verticesPerSection), (float)level / (float)sections, -1.0f);
                        vertexBuffer[lowerRight].TextureCoord = new Vector3(0.5f + (float)(vertex + 1) / (float)(verticesPerSection), (float)level / (float)sections, -1.0f);
                        vertexBuffer[topLeft].TextureCoord = new Vector3(0.5f + (float)vertex / (float)(verticesPerSection), (float)(level + 1) / (float)sections, -1.0f);
                        vertexBuffer[topRight].TextureCoord = new Vector3(0.5f + (float)(vertex + 1) / (float)(verticesPerSection), (float)(level + 1) / (float)sections, -1.0f);
                    }
                    else
                    {
                        int v = (vertex - verticesPerSection / 2);
                        vertexBuffer[lowerLeft].TextureCoord = new Vector3(1 - (float)v / (float)(verticesPerSection), (float)level / (float)sections, -1.0f);
                        vertexBuffer[lowerRight].TextureCoord = new Vector3(1 - (float)(v + 1) / (float)(verticesPerSection), (float)level / (float)sections, -1.0f);
                        vertexBuffer[topLeft].TextureCoord = new Vector3(1 - (float)v / (float)(verticesPerSection), (float)(level + 1) / (float)sections, -1.0f);
                        vertexBuffer[topRight].TextureCoord = new Vector3(1 - (float)(v + 1) / (float)(verticesPerSection), (float)(level + 1) / (float)sections, -1.0f);
                    }

                    // Triangle 1
                    indexBuffer[firstIndex++] = topLeft;
                    indexBuffer[firstIndex++] = lowerRight;
                    indexBuffer[firstIndex++] = lowerLeft;

                    // Triangle 2
                    indexBuffer[firstIndex++] = topLeft;
                    indexBuffer[firstIndex++] = topRight;
                    indexBuffer[firstIndex++] = lowerRight;
                }

                // Par cyclicité, le dernier vertex doit se relier à un vertex positionné avant lui dans la liste. On doit donc le calculer séparément
                int firstVertex2 = (verticesPerSection - 1 + level * verticesPerSection);
                int firstIndex2 = 6 * firstVertex2;
                firstVertex2 += startVB;
                firstIndex2 += startIB;
                int lowerLeft2 = firstVertex2;
                int lowerRight2 = firstVertex2 - verticesPerSection + 1;
                int topLeft2 = lowerLeft2 + verticesPerSection;
                int topRight2 = lowerLeft2 + 1;

                // Triangle 1
                indexBuffer[firstIndex2++] = topLeft2;
                indexBuffer[firstIndex2++] = lowerRight2;
                indexBuffer[firstIndex2++] = lowerLeft2;


                // Triangle 2
                indexBuffer[firstIndex2++] = topLeft2;
                indexBuffer[firstIndex2++] = topRight2;
                indexBuffer[firstIndex2++] = lowerRight2;
            }

            return frame;
        }

        /// <summary>
        /// Génère l'arbre par récurrence.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        ///
        private static void GenerateTree(List<Branch> InitialCondition,
                                         int currentIteration,
                                         int maxIteration,
                                         float scale,
                                         int height,
                                         int weight,
                                         float fruitScale,
                                         Vector3 torsion,
                                         Frame frame,
                                         VertexPositionColorNormalTexture[] vertexBuffer,
                                         ref int startVB,
                                         int[] indexBuffer,
                                         ref int startIB,
                                         bool addLeaves)
        {
            if (currentIteration <= maxIteration)
            {
                //Diminution de la définition de la branche en fonction de l'itération
                int verticesPerSection = VERTICES_PER_SECTION + 2 * (1 - currentIteration);
                if (verticesPerSection < MIN_VERTICES_PER_SECTION)
                    verticesPerSection = MIN_VERTICES_PER_SECTION;
                int sections = SECTIONS + 3 * (1 - currentIteration);
                if (sections < 3)
                    sections = 3;

                for (int i = 0; i < InitialCondition.Count; i++)
                {

                    //Repère qui correspondra au "bout" de la nouvelle branche
                    Frame newFrame = frame;

                    //Orientation de la nouvelle branche
                    Matrix yaw = Matrix.CreateFromAxisAngle(frame.OX, InitialCondition[i].m_yaw);
                    Matrix pitch = Matrix.CreateFromAxisAngle(frame.OY, InitialCondition[i].m_pitch);
                    Matrix roll = Matrix.CreateFromAxisAngle(frame.OZ, InitialCondition[i].m_roll);
                    newFrame.Rotate(yaw);
                    newFrame.Rotate(pitch);
                    newFrame.Rotate(roll);

                    //Génération de la nouvelle branche
                    newFrame = AddBranch(newFrame, verticesPerSection, sections, weight, weight * InitialCondition[i].m_relativeFinalRadius, height, scale * InitialCondition[i].m_relativeLength, torsion, vertexBuffer, startVB, indexBuffer, startIB);
                    startVB += verticesPerSection * (sections + 1);
                    startIB += verticesPerSection * sections * 6;

                    //Récurrence
                    GenerateTree(InitialCondition, currentIteration + 1, maxIteration, InitialCondition[i].m_relativeLength * scale, height, weight, fruitScale, torsion, newFrame, vertexBuffer, ref startVB, indexBuffer, ref startIB, addLeaves);
                }
            }

            else if (addLeaves)
            {
                AddLeave(frame, fruitScale, vertexBuffer, startVB, indexBuffer, startIB);
                startIB += 6;
                startVB += 4;
            }
        }

        /// <summary>
        /// Génère un modèle 3D.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        ///
        public static ModelData GenerateModel(List<Branch> InitialCondition, int height, int weight, float fruitScale, Vector3 torsion, int iterations, bool addLeaves=true)
        {
            int scale = 1;

            //Définition de l'arbre
            int verticesPerSection = VERTICES_PER_SECTION; //Nombre de points dans une section de branche
            int sections = SECTIONS; //Nombre de sections dans une branche

            //Calcul de la taille du vertex et index buffer
            int verticesNbr = VERTICES_PER_SECTION * (SECTIONS +1);
            int indexLength = VERTICES_PER_SECTION * SECTIONS * 6 ;
            for (int i = 1; i <= iterations; i++)
            {
                verticesNbr += verticesPerSection * (sections + 1) * (int)System.Math.Pow(InitialCondition.Count, i);
                indexLength += verticesPerSection * sections * 6 * (int)System.Math.Pow(InitialCondition.Count, i);
                verticesPerSection = VERTICES_PER_SECTION - 2 * i;
                sections = SECTIONS - 3 * i;
                if (verticesPerSection < MIN_VERTICES_PER_SECTION)
                    verticesPerSection = MIN_VERTICES_PER_SECTION;
                if (sections < 3)
                    sections = 3;
            }

            //Feuillage
            verticesNbr += 4 * (int)System.Math.Pow(InitialCondition.Count, iterations);
            indexLength += 6 * (int)System.Math.Pow(InitialCondition.Count, iterations);

            //Création du vertex / index buffer
            var vertexBuffer = new VertexPositionColorNormalTexture[verticesNbr];
            int[] indexBuffer = new int[indexLength];
            int startVB=0;
            int startIB=0;

            //Repère initial local de l'arbre
            Frame frame = new Frame(new Vector3(0, 0, 0),
                        new Vector3(-1, 0, 0),
                        new Vector3(0, -1, 0),
                        new Vector3(0, 0, -1));

            //Tronc : à modifier ...
            frame = AddBranch(frame, VERTICES_PER_SECTION, SECTIONS, weight, weight * InitialCondition[0].m_relativeFinalRadius, height, scale, torsion, vertexBuffer, startVB, indexBuffer, startIB);
            startVB += VERTICES_PER_SECTION * (SECTIONS + 1);
            startIB += VERTICES_PER_SECTION * SECTIONS * 6;

            //Génération du modèle par récurrence
            GenerateTree(InitialCondition, 1, iterations, scale, height, weight, fruitScale, torsion, frame, vertexBuffer, ref startVB, indexBuffer, ref startIB, addLeaves);

            // Calcule les normals aux surfaces.
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
            
            // Assignation du vertex et index buffer au model data

            var Vertices = new VertexBuffer(Game1.Instance.GraphicsDevice, VertexPositionColorNormalTexture.VertexDeclaration, vertexBuffer.Count(), BufferUsage.None);
            Vertices.SetData<VertexPositionColorNormalTexture>(vertexBuffer);
            var Indices = new IndexBuffer(Game1.Instance.GraphicsDevice, IndexElementSize.ThirtyTwoBits, indexBuffer.Count(), BufferUsage.None);
            Indices.SetData<int>(indexBuffer);
            ModelData model = new ModelData(Vertices, Indices);
            return model;
        }
    }
}