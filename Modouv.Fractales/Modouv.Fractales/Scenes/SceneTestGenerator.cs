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
using Modouv.Fractales.World;
using Modouv.Fractales.World.Objects;
using Modouv.Fractales.Generation;
using Modouv.Fractales.World.Objects.Specialized;
namespace Modouv.Fractales.Scenes
{
    /// <summary>
    /// Classe abstraite contenant toute une série de fonctions permettant l'affichage du monde 3D.
    /// </summary>
    public abstract class SceneFractalWorld : Scene
    {
        /* ----------------------------------------------------------------------------
         * Variables
         * --------------------------------------------------------------------------*/
        #region Variables
        /// <summary>
        /// Contient le nombre de frames ayant été dessinées jusqu'à présent.
        /// </summary>
        protected int m_frameCounter = 0;
        /// <summary>
        /// Permet de calculer le fps moyen.
        /// </summary>
        protected Debug.Profiling.FPSCounter m_fpsCounter = new Debug.Profiling.FPSCounter();
        /// <summary>
        /// Générateur de nombres aléatoires.
        /// </summary>
        protected Random m_rand = new Random();
        /// <summary>
        /// Référence vers le GameWorld.
        /// </summary>
        protected GameWorld m_gameWorld;
        /// <summary>
        /// Sauvegarde de l'état du rasterizer.
        /// </summary>
        protected RasterizerState _rasterizerState;
        /// <summary>
        /// Composant dessinant les rayons de soleil.
        /// </summary>
        protected World.LensFlareComponent m_lensFlare;
        /// <summary>
        /// Render Target de la reflection map.
        /// </summary>
        protected RenderTarget2D m_reflectionMapRenderTarget;

        #region DEBUG
        public GameWorld World
        {
            get { return m_gameWorld; }
        }
        /// <summary>
        /// Bounding frustum pouvant être affiché.
        /// </summary>
        protected BoundingFrustum m_debugFrustum;
        #endregion
        #endregion


        /* ----------------------------------------------------------------------------
         * Initialisation
         * --------------------------------------------------------------------------*/
        #region Initialisation
        /// <summary>
        /// Méthode à appeler au constructeur.
        /// </summary>
        protected void ctor()
        {
            
        }
        /// <summary>
        /// Initialise la scène.
        /// </summary>
        public override void Initialize()
        {
            m_gameWorld = new Modouv.Fractales.World.GameWorld();
            m_gameWorld.Camera.Position = new Vector3(0, 0, -25);//new Vector3(-100, -20, 20);

            // Etat du rasterizer
            _rasterizerState = new RasterizerState();
            _rasterizerState.CullMode = CullMode.CullCounterClockwiseFace;
            _rasterizerState.FillMode = FillMode.Solid;

            // Rayon de soleil
            m_lensFlare = new World.LensFlareComponent(Game1.Instance);
            m_lensFlare.Initialize();
            
            Game1.Instance.Components.Add(m_lensFlare);

            // Positionne la souris au centre de l'écran.
            int centerX = Game1.Instance.Window.ClientBounds.Width / 2;
            int centerY = Game1.Instance.Window.ClientBounds.Height / 2;
            Mouse.SetPosition(centerX, centerY);

            // Initialise la reflection map du render target.
            m_reflectionMapRenderTarget = new RenderTarget2D(Game1.Instance.GraphicsDevice,
                    Game1.Instance.ResolutionWidth / m_gameWorld.GraphicalParameters.ReflectionMapSampleSize,
                    Game1.Instance.ResolutionHeight / m_gameWorld.GraphicalParameters.ReflectionMapSampleSize, true, SurfaceFormat.Dxt5, DepthFormat.Depth24Stencil8);

        }

        /// <summary>
        /// Méthode effectuant la régénération du terrain.
        /// </summary>
        protected abstract void RegenerateTerrain();
        
        #endregion


        /* ----------------------------------------------------------------------------
         * Draw
         * --------------------------------------------------------------------------*/
        #region DRAW
        /* ----------------------------------------------------------------------------
         * -- Individual Elements
         * --------------------------------------------------------------------------*/
        #region Individual Elements
        /// <summary>
        /// Dessine les informations de debug
        /// </summary>
        /// <param name="gameTime"></param>
        protected virtual void DrawDebug(GameTime gameTime)
        {

        }
        /// <summary>
        /// Dessine l'arrière plan.
        /// </summary>
        /// <param name="time"></param>
        protected abstract void DrawBackground(GameTime time, Rectangle bounds, int alpha = 255, bool reflection=false);
        /// <summary>
        /// Dessine le terrain.
        /// </summary>
        protected abstract void DrawLandscape(GameTime time, string technique);
        /// <summary>
        /// Dessine l'eau
        /// </summary>
        protected abstract void DrawWater(GameTime time);
        #endregion

        /* ----------------------------------------------------------------------------
         * -- Scene Elements
         * --------------------------------------------------------------------------*/
        #region Scene
        /// <summary>
        /// Dessine les objets 3D par défaut de la scène.
        /// </summary>
        protected void DrawDefault3DObjects(GameTime time)
        {

        }
        /// <summary>
        /// Dessine les objets 3D de la scène.
        /// </summary>
        protected virtual void Draw3DObjects(GameTime time)
        {

        }
        /// <summary>
        /// Dessine la shadow map.
        /// /!\ Marche pas.
        /// </summary>
        /// <param name="time"></param>
        protected void DrawShadowMap(GameTime time)
        {

        }
        /// <summary>
        /// Dessine la map de réflexion dans une texture temporaire.
        /// </summary>
        protected void DrawReflectionMap_Wrapper(GameTime time)
        {

            // Crée la matrice "View" de la réflection. Typiquement la caméra à l'envers.
            Vector3 reflCameraPosition = m_gameWorld.Camera.Position;
            reflCameraPosition.Z = -m_gameWorld.Camera.Position.Z+0.05f;
            Vector3 reflTargetPos = m_gameWorld.Camera.Front + m_gameWorld.Camera.Position;
            reflTargetPos.Z = -(m_gameWorld.Camera.Front.Z + m_gameWorld.Camera.Position.Z)+0.05f;
            Vector3 invUpVector = -m_gameWorld.Camera.Up;
            var reflectionViewMatrix = Matrix.CreateLookAt(reflCameraPosition, reflTargetPos, invUpVector);
            GraphicsDevice device = Game1.Instance.GraphicsDevice;
            
            // Dessine la scène réflechie sur le RenderTarget temporaire.
            device.SetRenderTarget(m_reflectionMapRenderTarget);
            device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);
            m_gameWorld.View = reflectionViewMatrix;

            // Dessine l'arrière plan.
            Device.DepthStencilState = DepthStencilState.None;
            DrawBackground(time, m_reflectionMapRenderTarget.Bounds, 200, true); 

            // Restore le blend state pour qu'il gère la transparence.
            Device.DepthStencilState = DepthStencilState.Default;
            device.BlendState = BlendState.NonPremultiplied;
            RasterizerState state = new RasterizerState();
            state.CullMode = CullMode.None;
            state.FillMode = FillMode.Solid;
            device.RasterizerState = state;

            DepthStencilState dstate = new DepthStencilState();
            dstate.DepthBufferFunction = CompareFunction.LessEqual;
            device.DepthStencilState = dstate;

            // Dessine la map de réflection.
            DrawReflectionMap(time, reflectionViewMatrix * m_gameWorld.Projection);

            // Restore les paramètres précédents du monde
            m_gameWorld.RestoreCameraView();


            // Restore les paramètres du device.
            device.SetRenderTarget(null);
            device.RasterizerState = _rasterizerState;
            device.DepthStencilState = DepthStencilState.Default;
        }
        /// <summary>
        /// Effectue les opérations de dessin sur la map de réflection.
        /// </summary>
        protected abstract void DrawReflectionMap(GameTime time, Matrix reflectionViewProjection);


        /// <summary>
        /// Dessine la scène.
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime time)
        {
            m_frameCounter++;
            lock (Game1.GraphicsDeviceMutex)
            {
                Device.SetRenderTarget(null);
            }
        }
        #endregion
        #endregion


        /* ----------------------------------------------------------------------------
         * Update
         * --------------------------------------------------------------------------*/
        #region Update

        /* ----------------------------------------------------------------------------
         * Input
         * --------------------------------------------------------------------------*/
        #region Input
        /// <summary>
        /// Mets à jour tout ce qui est lié à l'appui de touches sur le clavier, et aux actions qui en résultent.
        /// </summary>
        protected virtual void UpdateInput(GameTime time)
        {
            float angleSpeed = 30;
            float speed = 5;
            if (Input.IsPressed(Keys.LeftShift))
                speed *= 20;
            if (Input.IsPressed(Keys.Space))
                speed *= 5;

            // Angle de la caméra.
            float delta = (float)time.ElapsedGameTime.TotalSeconds;
            KeyboardState keyboard = Keyboard.GetState();
            MouseState mouse = Mouse.GetState();
            int centerX = Game1.Instance.Window.ClientBounds.Width / 2;
            int centerY = Game1.Instance.Window.ClientBounds.Height / 2;
            Mouse.SetPosition(centerX, centerY);
            float cameraRotationX = -MathHelper.ToRadians((mouse.X - centerX) * angleSpeed * 0.01f);
            float cameraRotationY = MathHelper.ToRadians((mouse.Y - centerY) * angleSpeed * 0.01f);

            m_gameWorld.Camera.RotateSide(cameraRotationX * angleSpeed * delta);
            m_gameWorld.Camera.RotateUpDown(-cameraRotationY * angleSpeed * delta);



            // Déplacement de la caméra.
            Vector3 position = m_gameWorld.Camera.Position;
            if (Input.IsPressed(Keys.Z))
                m_gameWorld.Camera.MoveForward(delta * speed);
            else if (Input.IsPressed(Keys.S))
                m_gameWorld.Camera.MoveForward(-delta * speed);
            if (Input.IsPressed(Keys.Q))
                m_gameWorld.Camera.MoveSide(delta * speed);
            //m_gameWorld.Camera.MoveSide(delta * speed);
            else if (Input.IsPressed(Keys.D))
                m_gameWorld.Camera.MoveSide(-delta * speed);



            speed = 30;
            angleSpeed = 2;
            m_gameWorld.Camera.MoveForward(Input.GetLeftStickState().Y*delta*speed);
            m_gameWorld.Camera.RotateSide(-Input.GetLeftStickState().X * delta * speed / 100.0f);

            m_gameWorld.Camera.RotateRoll(-Input.GetRightStickState().X * angleSpeed * delta);
            m_gameWorld.Camera.RotateUpDown(-Input.GetRightStickState().Y * angleSpeed * delta);

            if (Input.IsPressed(Keys.NumPad5))
                m_gameWorld.Camera.Position = Vector3.Zero;
            if (Input.IsTrigger(Keys.G))
                RegenerateTerrain();
        }
        #endregion
        /// <summary>
        /// Mets à jour la scène.
        /// </summary>
        /// <param name="time"></param>
        public override void Update(GameTime time)
        {
            UpdateInput(time);
            m_gameWorld.Update(time);
        }

        #region DEBUG
        /// <summary>
        /// Mets à jour les paramètres 3D du rasterizer de la carte.
        /// </summary>
        protected void UpdateRasterizerState()
        {
            // Paramètre un depth buffer pour que les polygones dessinés en arrière plan soient cachés.
            Game1.Instance.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            Game1.Instance.GraphicsDevice.BlendState = BlendState.NonPremultiplied;


            // Permet d'avoir une vue de debug
            if (Input.IsTrigger(Keys.X))
            {
                // Vue normal
                _rasterizerState = new RasterizerState();
                _rasterizerState.CullMode = CullMode.CullCounterClockwiseFace;
                _rasterizerState.FillMode = FillMode.Solid;
            }
            else if (Input.IsTrigger(Keys.W))
            {
                // Vue en wireframe
                _rasterizerState = new RasterizerState();
                _rasterizerState.CullMode = CullMode.CullCounterClockwiseFace;
                _rasterizerState.FillMode = FillMode.WireFrame;
            }


            Game1.Instance.GraphicsDevice.RasterizerState = _rasterizerState;
        }
        #endregion
        #endregion

    }
}
