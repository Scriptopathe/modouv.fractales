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
using Modouv.Fractales.World.Objects;
using Modouv.Fractales.World;
using Vert = Modouv.Fractales.Generation.ModelGenerator.VertexPositionColorNormalTexture;
namespace Modouv.Fractales.Generation.Populations.WorldFantasy
{
    public class FractalTreePopulator
    {
        /// <summary>
        /// Représente une espèce d'arbre.
        /// </summary>
        public enum TreeKind
        {
            Cerisier,
            Pommier,
            Pommier2,
            WTF, 
            Sapin,
            Demo,
            Demo2,
            Demo3,
            Demo4
        }
        /// <summary>
        /// Génère une population à partir des données fournies et de paramètres par défaut.
        /// </summary>
        /// <param name="landscape"></param>
        /// <param name="model"></param>
        /// <param name="shader"></param>
        /// <returns></returns>
        public static IObject3D Generate(Landscape landscape, Effect shader, TreeKind kind, bool leaves=true)
        {
            var rand = ObjectPopulator.rand;
            var data = new ObjectPopulator.PopulationData();
            data.shader = shader;
            data.model = CreateModel(kind, leaves);
            data.Landscape = landscape;
            data.ForcedBB = new BoundingBox(new Vector3(-1, -1, -1)*50, new Vector3(1, 1, 1)*50);
            data.InstanceDebugView = false;
            data.GroupDebugView = false;

            switch (kind)
            {
                // Génération des cerisiers.
                case TreeKind.Cerisier:
                    data.MaxDepth = 3;
                    data.Density = 8;
                    data.PopulateFunc = new ObjectPopulator.PopulateFunction((int depth, Rectangle region) =>
                    {
                        List<Transform> transforms = new List<Transform>(data.Density);
                        for (int i = 0; i < data.Density; i++)
                        {
                            int px = region.X + ObjectPopulator.rand.Next(region.Width);
                            int py = region.Y + rand.Next(region.Height);
                            Vector3 position = data.Landscape.GetVerticePosition(px, py);

                            // Saute un arbre sur deux
                            if (rand.Next(2) == 0)
                                continue;
                                                    
                        
                            if (position.Z < 0.5f && position.Z > -50f)
                            {
                                Vector3 normal = Vector3.Normalize(data.Landscape.GetVerticeNormal(px, py));
                                if (normal.Z > 0.825f) // ne fait pousser les arbres que sur des zones plates.
                                {
                                    Transform t = new Transform();
                                    t.Position = position - new Vector3(0, 0, -0.70f); //position;
                                    t.Position += new Vector3(rand.Next(20) / 200.0f, rand.Next(20) / 200.0f, 0);
                                    t.Scale = new Vector3(1, -1, 1) * (0.04f + rand.Next(100) / 2000.0f);
                                    t.Rotation = new Vector3(rand.Next(20) / 360.0f, rand.Next(20) / 360.0f, rand.Next(180) / 360.0f);
                                    t.AdditionalData1 = new Vector4(normal, rand.Next(100));
                                    int r = rand.Next(4);
                                    t.TextureOffset = new Vector2((r / 2) / 2.0f, (r % 2) / 2.0f);
                                    transforms.Add(t);
                                }
                            }


                        }
                        return transforms.ToArray();
                    });
                    break;
                // Génération des pommiers et WTF
                default:
                    data.MaxDepth = 3;
                    data.Density = 4;
                    data.PopulateFunc = new ObjectPopulator.PopulateFunction((int depth, Rectangle region) =>
                    {
                        List<Transform> transforms = new List<Transform>(data.Density);
                        for (int i = 0; i < data.Density; i++)
                        {
                            int px = region.X + ObjectPopulator.rand.Next(region.Width);
                            int py = region.Y + rand.Next(region.Height);
                            Vector3 position = data.Landscape.GetVerticePosition(px, py);


                            if (position.Z < 0.5f && position.Z > -22f)
                            {
                                Vector3 normal = Vector3.Normalize(data.Landscape.GetVerticeNormal(px, py));
                                Transform t = new Transform();
                                t.Position = position - new Vector3(0, 0, -0.70f); //position;
                                t.Position += new Vector3(rand.Next(20) / 200.0f, rand.Next(20) / 200.0f, 0);
                                t.Scale = new Vector3(1, -1, 1) * (0.04f + rand.Next(100) / 5000.0f) * (kind == TreeKind.WTF ? 1 : 2);
                                t.Rotation = new Vector3(rand.Next(20) / 360.0f, rand.Next(20) / 360.0f, rand.Next(180) / 360.0f);
                                t.AdditionalData1 = new Vector4(normal, rand.Next(100));
                                int r = rand.Next(4);
                                t.TextureOffset = new Vector2((r / 2) / 2.0f, (r % 2) / 2.0f);
                                transforms.Add(t);
                            }

                        }
                        return transforms.ToArray();
                    });
                    break;
            }
            return ObjectPopulator.Generate(data);
        }

        /// <summary>
        /// Cette fonction génère un arbre au milieu du terrain.
        /// Ne pas regarder le code il est vraiement très moche.
        /// <returns></returns>
        public static IObject3D GenerateUnique(Effect shader, TreeKind kind, bool leaves = true)
        {
            var rand = ObjectPopulator.rand;
            var data = new ObjectPopulator.PopulationData();
            data.shader = shader;
            data.model = CreateModel(kind, leaves);
            data.ForcedBB = new BoundingBox(new Vector3(-1, -1, -1) * 50, new Vector3(1, 1, 1) * 50);
            data.InstanceDebugView = false;
            data.GroupDebugView = false;
            data.MaxDepth = 1;
            data.Density = 1;
            Game1.Instance.GraphicalParameters.TreesIterationsBasis = 8;


            data.PopulateFunc = new ObjectPopulator.PopulateFunction((int depth, Rectangle region) =>
            {
                List<Transform> transforms = new List<Transform>(data.Density);
                int px = 0;
                int py = 0;
                Transform t = new Transform();
                t.Position = new Vector3(px, py, 0); //position;
                t.Scale = new Vector3(1, -1, 1) * (0.04f + rand.Next(100) / 5000.0f) * 4;
                t.Rotation = new Vector3(0, 0, 0);
                t.AdditionalData1 = new Vector4(Vector3.Zero, rand.Next(100));
                transforms.Add(t);
                return transforms.ToArray();
            });

            StaticInstancedObjects objects = new StaticInstancedObjects(data.model, data.PopulateFunc(0, new Rectangle(0, 0, 0, 0)), data.shader);
            objects.SetModelBoundingBox(data.ForcedBB);
            objects.InstanceCullingEnabled = false;
            objects.CullingEnabled = false;
            objects.DebugView = data.InstanceDebugView;


            return objects;
        }

        /// <summary>
        /// Crée le modèle 3D.
        /// </summary>
        /// <returns></returns>
        static ModelData CreateModel(TreeKind kind, bool leaves)
        {
            List<Modouv.Fractales.Generation.Models.FractalTreeModelGenerator.Branch> InitialCondition = new List<Models.FractalTreeModelGenerator.Branch>();
            float PI = (float)Math.PI;

            //Pommier
            switch (kind)
            {
                case TreeKind.Demo:
                    InitialCondition.Add(new Generation.Models.FractalTreeModelGenerator.Branch(0.58f, 0.70f, 7.41f * PI / 4.4f, 0.0f, 0.0f));
                    InitialCondition.Add(new Generation.Models.FractalTreeModelGenerator.Branch(0.56f, 0.70f, 7.42f * PI / 4.5f, 0.0f, 1.02f * PI / 2.0f));
                    InitialCondition.Add(new Generation.Models.FractalTreeModelGenerator.Branch(0.59f, 0.70f, 7.45f * PI / 4.3f, 0.0f, 2.06f * PI / 2.0f));
                    InitialCondition.Add(new Generation.Models.FractalTreeModelGenerator.Branch(0.57f, 0.70f, 7.43f * PI / 4.2f, 0.0f, 2.95f * PI / 2.0f));
                    return Generation.Models.FractalTreeModelGenerator.GenerateModel(InitialCondition, 60, 6, 3.0f, new Vector3(1.5f, 1.4f, 1.0f), Game1.Instance.GraphicalParameters.TreesIterationsBasis + 2, leaves);
                case TreeKind.Demo2:
                    InitialCondition.Add(new Generation.Models.FractalTreeModelGenerator.Branch(0.78f, 0.70f, 7.41f * PI / 4.4f, 0.0f, 0.0f));
                    InitialCondition.Add(new Generation.Models.FractalTreeModelGenerator.Branch(0.56f, 0.70f, 7.42f * PI / 4.5f, 0.0f, 1.02f * PI / 2.0f));
                    InitialCondition.Add(new Generation.Models.FractalTreeModelGenerator.Branch(0.59f, 0.70f, 7.45f * PI / 4.3f, 0.0f, 2.06f * PI / 2.0f));
                    InitialCondition.Add(new Generation.Models.FractalTreeModelGenerator.Branch(0.57f, 0.70f, 7.43f * PI / 4.2f, 0.0f, 2.95f * PI / 2.0f));
                    return Generation.Models.FractalTreeModelGenerator.GenerateModel(InitialCondition, 60, 6, 3.0f, new Vector3(1.5f, 1.4f, 1.0f), Game1.Instance.GraphicalParameters.TreesIterationsBasis + 2, leaves);
                case TreeKind.Demo3:
                    InitialCondition.Add(new Generation.Models.FractalTreeModelGenerator.Branch(0.67f, 0.75f, PI / 8.0f, -0.19f, 0.0f));
                    InitialCondition.Add(new Generation.Models.FractalTreeModelGenerator.Branch(0.59f, 0.75f, PI / 6.0f, -0.24f, 2 * PI / 3.0f));
                    InitialCondition.Add(new Generation.Models.FractalTreeModelGenerator.Branch(0.65f, 0.75f, PI / 8.0f, -0.32f, 4 * PI / 3.0f));
                    return Generation.Models.FractalTreeModelGenerator.GenerateModel(InitialCondition, 80, 5, 30.0f, new Vector3(1, 1, 1.1f), Game1.Instance.GraphicalParameters.TreesIterationsBasis, leaves);
                case TreeKind.Demo4:
                    InitialCondition.Add(new Generation.Models.FractalTreeModelGenerator.Branch(0.58f, 0.70f, 7.41f * PI / 4.4f, 0.0f, 0.0f));
                    InitialCondition.Add(new Generation.Models.FractalTreeModelGenerator.Branch(0.56f, 0.70f, 7.42f * PI / 4.5f, 0.0f, 1.02f * PI / 2.0f));
                    InitialCondition.Add(new Generation.Models.FractalTreeModelGenerator.Branch(0.59f, 0.70f, 7.45f * PI / 4.3f, 0.0f, 2.06f * PI / 2.0f));
                    InitialCondition.Add(new Generation.Models.FractalTreeModelGenerator.Branch(0.57f, 0.70f, 7.43f * PI / 4.2f, 0.0f, 2.95f * PI / 2.0f));
                    return Generation.Models.FractalTreeModelGenerator.GenerateModel(InitialCondition, 60, 6, 3.0f, new Vector3(1.5f, 1.4f, 1.0f), Game1.Instance.GraphicalParameters.TreesIterationsBasis + 2, leaves);
                case TreeKind.Cerisier:
                    InitialCondition.Add(new Generation.Models.FractalTreeModelGenerator.Branch(0.65f, 0.65f, PI / 4.0f, 0.0f, 0.0f));
                    InitialCondition.Add(new Generation.Models.FractalTreeModelGenerator.Branch(0.49f, 0.65f, PI / 5.8f, 0.0f, 1.32f * PI / 2.0f));
                    InitialCondition.Add(new Generation.Models.FractalTreeModelGenerator.Branch(0.66f, 0.65f, PI / 4.4f, 0.0f, 2.16f * PI / 2.0f));
                    InitialCondition.Add(new Generation.Models.FractalTreeModelGenerator.Branch(0.64f, 0.65f, PI / 3.7f, 0.0f, 2.95f * PI / 2.0f));
                    return Generation.Models.FractalTreeModelGenerator.GenerateModel(InitialCondition, 50, 3, 3.0f, new Vector3(1.5f, 1.5f, 1.5f), Game1.Instance.GraphicalParameters.TreesIterationsBasis-1, leaves);
                case TreeKind.Pommier:
                    InitialCondition.Add(new Generation.Models.FractalTreeModelGenerator.Branch(0.67f, 0.75f, PI / 8.0f, 0.0f, 0.0f));
                    InitialCondition.Add(new Generation.Models.FractalTreeModelGenerator.Branch(0.59f, 0.75f, PI / 8.0f, 0.0f, 2 * PI / 3.0f));
                    InitialCondition.Add(new Generation.Models.FractalTreeModelGenerator.Branch(0.65f, 0.75f, PI / 8.0f, 0.0f, 4 * PI / 3.0f));
                    return Generation.Models.FractalTreeModelGenerator.GenerateModel(InitialCondition, 50, 5, 20.0f, new Vector3(1, 1, 1), Game1.Instance.GraphicalParameters.TreesIterationsBasis, leaves);
                case TreeKind.Pommier2:
                    InitialCondition.Add(new Generation.Models.FractalTreeModelGenerator.Branch(0.67f, 0.75f, PI / 8.0f, -0.19f, 0.0f));
                    InitialCondition.Add(new Generation.Models.FractalTreeModelGenerator.Branch(0.59f, 0.75f, PI / 6.0f, -0.24f, 2 * PI / 3.0f));
                    InitialCondition.Add(new Generation.Models.FractalTreeModelGenerator.Branch(0.65f, 0.75f, PI / 8.0f, -0.32f, 4 * PI / 3.0f));
                    return Generation.Models.FractalTreeModelGenerator.GenerateModel(InitialCondition, 80, 5, 30.0f, new Vector3(1, 1, 1.1f), Game1.Instance.GraphicalParameters.TreesIterationsBasis, leaves);
                case TreeKind.WTF:
                    InitialCondition.Add(new Generation.Models.FractalTreeModelGenerator.Branch(0.65f, 0.65f, 9 * PI / 4.0f, 0.0f, 0.0f));
                    InitialCondition.Add(new Generation.Models.FractalTreeModelGenerator.Branch(0.49f, 0.65f, 9 * PI / 5.8f, 0.0f, 1.32f * PI / 2.0f));
                    InitialCondition.Add(new Generation.Models.FractalTreeModelGenerator.Branch(0.66f, 0.65f, 9 * PI / 4.4f, 0.0f, 2.16f * PI / 2.0f));
                    InitialCondition.Add(new Generation.Models.FractalTreeModelGenerator.Branch(0.64f, 0.65f, 9 * PI / 3.7f, 0.0f, 2.95f * PI / 2.0f));
                    return Generation.Models.FractalTreeModelGenerator.GenerateModel(InitialCondition, 50, 6, 3.0f, new Vector3(2.0f, 2.0f, 1.0f), Game1.Instance.GraphicalParameters.TreesIterationsBasis - 1, leaves);
                case TreeKind.Sapin:
                    InitialCondition.Add(new Generation.Models.FractalTreeModelGenerator.Branch(0.85f, 0.75f, 0.0f, 0.0f, 0.0f));
                    InitialCondition.Add(new Generation.Models.FractalTreeModelGenerator.Branch(0.45f, 0.75f, PI / 3.0f, 0.0f, 0.0f));
                    InitialCondition.Add(new Generation.Models.FractalTreeModelGenerator.Branch(0.42f, 0.75f, PI / 3.0f, 0.0f, 1.12f * PI / 2.0f));
                    InitialCondition.Add(new Generation.Models.FractalTreeModelGenerator.Branch(0.45f, 0.75f, PI / 3.0f, 0.0f, 2.16f * PI / 2.0f));
                    InitialCondition.Add(new Generation.Models.FractalTreeModelGenerator.Branch(0.45f, 0.75f, PI / 3.0f, 0.0f, 3.11f * PI / 2.0f));
                    return Generation.Models.FractalTreeModelGenerator.GenerateModel(InitialCondition, 50, 7, 0.0f, new Vector3(0.1f, 0.1f, 1.0f), Game1.Instance.GraphicalParameters.TreesIterationsBasis - 1, leaves);
                default:
                    throw new Exception();
            }

            /*
            //WTF :D
            InitialCondition.Add(new Generation.Models.FernModelGenerator.Branch(0.65f, 0.65f, 9 * PI / 4.0f, 0.0f, 0.0f));
            InitialCondition.Add(new Generation.Models.FernModelGenerator.Branch(0.49f, 0.65f, 9 * PI / 5.8f, 0.0f, 1.32f * PI / 2.0f));
            InitialCondition.Add(new Generation.Models.FernModelGenerator.Branch(0.66f, 0.65f, 9 * PI / 4.4f, 0.0f, 2.16f * PI / 2.0f));
            InitialCondition.Add(new Generation.Models.FernModelGenerator.Branch(0.64f, 0.65f, 9 * PI / 3.7f, 0.0f, 2.95f * PI / 2.0f));
            return Generation.Models.FernModelGenerator.GenerateModel(InitialCondition, 50, 6, 3.0f,new Vector3(2.0f, 2.0f, 1.0f), 7);

            //Sapin
            /*InitialCondition.Add(new Generation.Models.FernModelGenerator.Branch(0.75f, 0.75f, 0.0f, 0.0f, 0.0f));
            InitialCondition.Add(new Generation.Models.FernModelGenerator.Branch(0.35f, 0.75f, PI / 3.0f, 0.0f, 0.0f));
            InitialCondition.Add(new Generation.Models.FernModelGenerator.Branch(0.39f, 0.75f, PI / 3.0f, 0.0f, 1.12f * PI / 2.0f));
            InitialCondition.Add(new Generation.Models.FernModelGenerator.Branch(0.36f, 0.75f, PI / 3.0f, 0.0f, 2.16f * PI / 2.0f));
            InitialCondition.Add(new Generation.Models.FernModelGenerator.Branch(0.34f, 0.75f, PI / 3.0f, 0.0f, 3.11f * PI / 2.0f));
            return Generation.Models.FernModelGenerator.GenerateModel(InitialCondition, 50, 5, 0.0f, new Vector3(0.1f, 0.1f, 1.0f), 6);
             * */
        }
    }
}
