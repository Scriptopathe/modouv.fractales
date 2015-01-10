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
using Microsoft.Xna.Framework.Input;
using Modouv.Fractales.World.Objects;
namespace Modouv.Fractales.Scenes
{
    /// <summary>
    /// Scene de test du générateur de fractales.
    /// </summary>
    public class SceneTestChaoticAttractor : Scene
    {
        #region Variables / Julia
        /// <summary>
        /// Texture de la fractale de julia, redessinée si besoin (et réallouée lors du changement de résolution).
        /// </summary>
        RenderTarget2D m_juliaTexture;
        /// <summary>
        /// Effet permettant d'adoucir les contours de la fractale.
        /// </summary>
        Effect m_multisampling;
        /// <summary>
        /// Echelle de la fractale.
        /// </summary>
        float m_scale = 1.2f;
        /// <summary>
        /// Position sur la fractale.
        /// </summary>
        MathHelpers.Complex m_offset = new MathHelpers.Complex(0, 0);
        /// <summary>
        /// Paramètre "c" de la fractale.
        /// Pour julia : new Math.Complex(0.285f, 0.01f);
        /// Pour mandelbrot : new Math.Complex(0.58f, 0.44f);
        /// Pour repulsor : new Math.Complex(0.50f, 0.50f);
        /// </summary>
        MathHelpers.Complex m_c = new MathHelpers.Complex(-0.857f, 0.231f);
        /// <summary>
        /// Résolution de dessin de la fractale.
        /// </summary>
        Point m_resolution;
        #endregion

        #region Variables / 3D
        World.GameWorld m_gameWorld;
        List<DynamicObject> m_objects;
        Generation.Attractors.RosslerAttractor m_attractor;
        /// <summary>
        /// Shader 3D.
        /// </summary>
        Effect m_3DShader;
        /// <summary>
        /// Position de la particule de l'attracteur chaotique.
        /// </summary>
        Vector3 m_position;
        #endregion

        #region Methods 3D
        /// <summary>
        /// Initialise les positions 3D des éléments.
        /// </summary>
        void Initialize3DPositions()
        {
            m_gameWorld = new Modouv.Fractales.World.GameWorld();
            m_gameWorld.Camera.Position = new Vector3(-50, -50, -50);//new Vector3(-100, -20, 20);
            m_position = new Vector3();
            
            // Etat du rasterizer
            _rasterizerState = new RasterizerState();
            _rasterizerState.CullMode = CullMode.None;
            _rasterizerState.FillMode = FillMode.Solid;
            m_attractor = new Generation.Attractors.RosslerAttractor();

            // Crée et paramètre le shader utilisé pour dessiner les modèles.
            m_3DShader = Game1.Instance.Content.Load<Effect>("Shaders\\attractor");
            var renderTargetJulia = new RenderTarget2D(Game1.Instance.GraphicsDevice, 2048, 2048);
            Generation.Fractals.Julia.GenerateTexture2DGPU(m_c, new MathHelpers.Complex(0, 0), 1f, renderTargetJulia);
            m_3DShader.Parameters["TexParam"].SetValue(renderTargetJulia);
            
            // Crée une heightmap de test.
            float[,] machin = new float[500, 500];

            // Création d'un objet 3D à dessiner.
            var obj = new DynamicObject();
            obj.Model = Generation.ModelGenerator.GenerateModel(machin, 20);
            obj.Shader = m_3DShader;
            obj.Position = new Vector3(0, 0, 0);
            obj.Rotation = new Vector3(0, 0, 3.14f);
            
            // Création d'une liste d'objets et ajout de l'objet à dessiner dedans.
            m_objects = new List<DynamicObject>();
            m_objects.Add(obj);

            obj = new DynamicObject();
            obj.Model = Generation.ModelGenerator.GenerateModel(machin, 20);
            obj.Shader = m_3DShader;
            obj.Position = new Vector3(0, 0.0f, 0);
            obj.Rotation = new Vector3(0, -(float)System.Math.PI / 2-0.08f, 0);
            //m_objects.Add(obj);
        }
        /// <summary>
        /// Mets à jour l'attracteur.
        /// </summary>
        void UpdateAttractor()
        {
            m_attractor.NextStep(0.00001f, 10000);
            m_position = m_attractor.CurrentPosition;
            m_position.Z *= 10000;
            m_position.X *= 400;
            m_position.Y *= 400;
            m_position.X %= 500*20;
            m_position.Y %= 500*20;

            /*Params pour Lorentz 
            m_attractor.NextStep(0.00001f, 100);
            m_position = m_attractor.CurrentPosition;
            m_position.Z *= 100;
            m_position.X *= 10;
            m_position.Y *= 10;
            m_position.X %= 500*20;
            m_position.Y %= 500*20; */
        }

        /// <summary>
        /// Sauvegarde de l'état du rasterizer.
        /// </summary>
        RasterizerState _rasterizerState;

        /// <summary>
        /// Dessine le modèle 3D avec comme texture la fractale.
        /// </summary>
        void Draw3DModel()
        {
            
            // Paramètre un depth buffer pour que les polygones dessinés en arrière plan soient cachés.
            Game1.Instance.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            Game1.Instance.GraphicsDevice.BlendState = BlendState.Opaque;

            // Permet d'avoir une vue de debug
            if (Input.IsTrigger(Keys.X))
            {
                // Vue normal
                _rasterizerState = new RasterizerState();
                _rasterizerState.CullMode = CullMode.None;
                _rasterizerState.FillMode = FillMode.Solid;
            }
            else if (Input.IsTrigger(Keys.W))
            {
                // Vue en wireframe
                _rasterizerState = new RasterizerState();
                _rasterizerState.CullMode = CullMode.None;
                _rasterizerState.FillMode = FillMode.WireFrame;
            }

            Game1.Instance.GraphicsDevice.RasterizerState = _rasterizerState;
            m_3DShader.Parameters["ParticlePosition"].SetValue(m_position);
            m_3DShader.Parameters["DiffuseDirection"].SetValue(m_gameWorld.Camera.Front);
            Game1.Instance.GraphicsDevice.Textures[10] = m_juliaTexture;

            foreach(var obj in m_objects)
                obj.Draw(m_gameWorld);
        }
        #endregion

        #region Core Methods
        /// <summary>
        /// Crée une nouvelle instance de SceneTestJulia.
        /// </summary>
        public SceneTestChaoticAttractor()
        {

        }

        /// <summary>
        /// Initialise la scène.
        /// </summary>
        public override void Initialize()
        {
            m_multisampling = Content.Load<Effect>("Shaders\\blur");
            m_resolution = new Point(Game1.Instance.Window.ClientBounds.Width, Game1.Instance.Window.ClientBounds.Height);
            Initialize3DPositions();

            // Positionne la souris au centre de l'écran.
            int centerX = Game1.Instance.Window.ClientBounds.Width / 2;
            int centerY = Game1.Instance.Window.ClientBounds.Height / 2;
            Mouse.SetPosition(centerX, centerY);
        }


        Vector3 m_cameraFront = Vector3.UnitX;
        Vector3 m_cameraUp = Vector3.UnitZ;
        
        /// <summary>
        /// Mets à jour tout ce qui est lié à l'appui de touches sur le clavier, et aux actions qui en résultent.
        /// </summary>
        void UpdateInput(GameTime time)
        {
            float angleSpeed = 30;
            float speed = 2000;

            // Angle de la caméra.
            float delta = (float)time.ElapsedGameTime.TotalSeconds;
            KeyboardState keyboard = Keyboard.GetState();
            MouseState mouse = Mouse.GetState();
            int centerX = Game1.Instance.Window.ClientBounds.Width / 2;
            int centerY = Game1.Instance.Window.ClientBounds.Height / 2;
            Mouse.SetPosition(centerX, centerY);
            float cameraRotationX = -MathHelper.ToRadians((mouse.X - centerX) * angleSpeed * 0.01f);
            float cameraRotationY = MathHelper.ToRadians((mouse.Y - centerY) * angleSpeed * 0.01f);

            m_gameWorld.Camera.RotateRoll(cameraRotationX * angleSpeed * delta);
            m_gameWorld.Camera.RotateUpDown(cameraRotationY * angleSpeed * delta);

            // Déplacement de la caméra.
            Vector3 position = m_gameWorld.Camera.Position;
            if (Input.IsPressed(Keys.Z))
                m_gameWorld.Camera.MoveForward(delta * speed);
            else if (Input.IsPressed(Keys.S))
                m_gameWorld.Camera.MoveForward(-delta * speed);
            if (Input.IsPressed(Keys.Q))
                m_gameWorld.Camera.MoveSide(delta * speed);
            else if (Input.IsPressed(Keys.D))
                m_gameWorld.Camera.MoveSide(-delta * speed);


        }
        /// <summary>
        /// Mets à jour la scène.
        /// </summary>
        /// <param name="time"></param>
        public override void Update(GameTime time)
        {
            UpdateInput(time);
            UpdateAttractor();

        }

        /// <summary>
        /// Dessine la scène.
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            Draw3DModel();
            Batch.Begin();

            Batch.DrawString(Font, "Rotation = [" + m_attractor.CurrentPosition.X.ToString() + " ; " +
                                    m_attractor.CurrentPosition.Y.ToString() + " ; " +
                                    m_attractor.CurrentPosition.Z.ToString() + "]",
                new Vector2(0, 0),
                Color.White);

            Batch.End();
        }
        #endregion

    }
}
