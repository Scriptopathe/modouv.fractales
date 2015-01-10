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
using Modouv.Fractales.World.Postprocess;
namespace Modouv.Fractales.Scenes
{
    /// <summary>
    /// Scene de test du générateur de fractales.
    /// </summary>
    public class SceneTestLandscape : SceneFractalWorld
    {
        /* ----------------------------------------------------------------------------
         * Variables
         * --------------------------------------------------------------------------*/
        #region Variables
        #region Paramètres de rendu / fractales etc...
        const float LANDSCAPE_UNIT_SIZE = 0.128f;
        const int LANDSCAPE_UNITS_COUNT = 128;
        /// <summary>
        /// Paramètre "c" de la première fractale dessinée sur le terrain, 
        /// mappée sur les coordonnées de teXture.
        /// </summary>
        MathHelpers.Complex m_c1 = new MathHelpers.Complex(-0.820f, 0.9030f);//new MathHelpers.Complex(-0.820f, 0.3030f);
        /// <summary>
        /// Paramètre de "c" de la deuXième fractale dessinée sur le terrain, mappée sur
        /// les normales du mesh.
        /// </summary>
        MathHelpers.Complex m_c2 = new MathHelpers.Complex(-0.835f, 0.9045f);//new MathHelpers.Complex(-0.835f, 0.3045f);
        #endregion

        #region Objets de la scène
        /// <summary>
        /// Shader pour le dessin de la montagne.
        /// </summary>
        Effect m_terrainShader;
        /// <summary>
        /// Texture du ciel
        /// </summary>
        Texture2D m_skyTexture;
        /// <summary>
        /// Paysage.
        /// </summary>
        LandscapeSingle m_landscape;
        /// <summary>
        /// Eau.
        /// </summary>
        WaterObject m_water;
        /// <summary>
        /// Skybox utilisée en arrière plan.
        /// </summary>
        Skybox m_skybox;
        /// <summary>
        /// Interface utilisateur.
        /// </summary>
        World.HUD.HUDManager m_hud;
        World.Objects.Specialized.NoiseMap m_noiseMap;
        #endregion

        #region Objets de post process
        /// <summary>
        /// Texture sur laquelle va être faite le dessin.
        /// </summary>
        RenderTarget2D m_backBuffer;
        /// <summary>
        /// Texture sur laquelle vont être effectuées des opérations de post process.
        /// </summary>
        RenderTarget2D m_postProcessRenderTarget;
        /// <summary>
        /// Texture sur laquelle vont être effectuées des opérations de post process liées au blur.
        /// </summary>
        RenderTarget2D m_blurRenderTarget;
        /// <summary>
        /// Effet de flou gaussien utilisé pour l'effet "Depth of field" du paysage.
        /// </summary>
        GaussianBlur m_blurEffect;
        /// <summary>
        /// Flou rapide non gaussien.
        /// </summary>
        Effect m_landscapeBlur;
        /// <summary>
        /// Effet de bloom utilisé si BloomEnabled vaut vrai.
        /// </summary>
        Bloom m_bloomEffect;
        /// <summary>
        /// Attracteur chaotique effectuant la simulation à la fin.
        /// </summary>
        Generation.Attractors.HenonAttractor m_windAttractor;
        #endregion

        #region Loading
        /// <summary>
        /// Indique si le chargement est terminé.
        /// </summary>
        bool m_isLoadingComplete = false;
        /// <summary>
        /// Thread de chargement de la scène.
        /// </summary>
        System.Threading.Thread m_loadingThread;
        /// <summary>
        /// Contient la phase de chargement en cours d'exécution lors du chargement.
        /// </summary>
        string m_loadingStringStatus;
        /// <summary>
        /// Contient la texture affichée pendant le chargement.
        /// </summary>
        RenderTarget2D m_loadingTexture;
        /// <summary>
        /// Effet permettant la génération de la texture de chargement.
        /// </summary>
        Effect m_loadingEffect;
        #endregion


        /// <summary>
        /// Valeur indiquant si le contrôle de la camera par la souris est activé.
        /// </summary>
        bool m_mouseControlEnabled = true;
        #endregion
        /* ----------------------------------------------------------------------------
         * Properties
         * --------------------------------------------------------------------------*/
        #region Properties
        /// <summary>
        /// Change les paramètres graphiques.
        /// </summary>
        /// <param name="parameters"></param>
        public void SetGraphicalParameters(GraphicalParameters parameters)
        {
            m_gameWorld.GraphicalParameters = parameters;
        }
        #endregion
        /* ----------------------------------------------------------------------------
         * Initialisation
         * --------------------------------------------------------------------------*/
        #region Initialisation

        /// <summary>
        /// Crée une nouvelle instance de SceneTestJulia.
        /// </summary>
        public SceneTestLandscape() : base()
        {

        }

        /// <summary>
        /// Initialise la scène.
        /// </summary>
        public override void Initialize()
        {
            /*Debug.Tools.TextureAtlasCreator.CreateTilableTextures("grass.jpg", "grasst.jpg", size / 2, size / 2);
            Debug.Tools.TextureAtlasCreator.CreateTilableTextures("sand.jpg", "sandt.jpg", size / 2, size / 2);
            Debug.Tools.TextureAtlasCreator.CreateTilableTextures("mountain.jpg", "mountaint.jpg", size / 2, size / 2);
            Debug.Tools.TextureAtlasCreator.CreateTilableTextures("snow.jpg", "snowt.jpg", size / 2, size / 2);
            Debug.Tools.TextureAtlasCreator.CreateFromTextures(new string[] { "grasst.jpg", "mountaint.jpg", "snowt.jpg", "sandt.jpg" }, "montagne-atlas.jpg", size, size);
            // 
            throw new Exception();*/
            //Debug.Tools.TextureAtlasCreator.CreateFromTextures(new string[] { "grass1.png", "grass2.png", "grass3.png", "grass4.png" }, "grass.png", 512, 512);

            base.Initialize();


            // Map de bruit
            m_noiseMap = new NoiseMap(LANDSCAPE_UNITS_COUNT);

            // Création du render target de chargement
            m_loadingTexture = new RenderTarget2D(Game1.Instance.GraphicsDevice, Game1.Instance.ResolutionWidth, Game1.Instance.ResolutionHeight, false, SurfaceFormat.Color, DepthFormat.None);
            
            // Chargement de la texture du ciel.
            m_skyTexture = Game1.Instance.Content.Load<Texture2D>("textures\\world_fantasy\\sky");

            // Crée et paramètre le shader utilisé pour dessiner le terrain.
            m_terrainShader = Game1.Instance.Content.Load<Effect>("Shaders\\mountain-fractal");
            m_terrainShader.Parameters["HeightmapTexture"].SetValue(m_noiseMap.HeightmapAndNormalMap);
            m_terrainShader.Parameters["MountainTexture"].SetValue(Content.Load<Texture2D>("textures\\world_fantasy\\landscape-winter"));

            // Création de l'eau
            m_water = new WaterObject(m_gameWorld.GraphicalParameters);
            m_water.SetReflectionMap(m_reflectionMapRenderTarget);

            // Création d'un objet 3D à dessiner.
            m_landscapeBlur = Content.Load<Effect>("Shaders\\postprocess\\Landscape-blur");
            m_landscape = new LandscapeSingle();
            m_landscape.Object.Shader = m_terrainShader;
            m_landscape.Object.Position = new Vector3(0, -0, 0);
            m_landscape.HScale = LANDSCAPE_UNIT_SIZE;
            m_landscape.VScale = -LANDSCAPE_UNIT_SIZE;
            

            // Création de la skybox
            m_skybox = new Skybox();

            // Création des principaux render targets
            m_backBuffer = new RenderTarget2D(Game1.Instance.GraphicsDevice, Game1.Instance.ResolutionWidth, Game1.Instance.ResolutionHeight, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.PreserveContents);
            m_postProcessRenderTarget = new RenderTarget2D(Game1.Instance.GraphicsDevice, Game1.Instance.ResolutionWidth, Game1.Instance.ResolutionHeight, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.PreserveContents);
            m_blurRenderTarget = new RenderTarget2D(Game1.Instance.GraphicsDevice, Game1.Instance.ResolutionWidth, Game1.Instance.ResolutionHeight, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.PreserveContents);
            
            // Paramétrage de l'effet de flou
            GaussianBlur blur = new GaussianBlur(Game1.Instance);
            blur.ComputeKernel(3, 5.61f);
            blur.ComputeOffsets(Game1.Instance.ResolutionWidth, Game1.Instance.ResolutionHeight);
            m_blurEffect = blur;

            // Paramétrage du bloom
            m_bloomEffect = new Bloom();

            // Création du HUD.
            m_hud = new World.HUD.HUDManager();
            InitializeHUD();

            // Attracteur chaotique
            m_windAttractor = new Generation.Attractors.HenonAttractor();

            // Création du thread de chargement
            m_loadingThread = new System.Threading.Thread(new System.Threading.ThreadStart(delegate()
            {
                RegenerateTerrain();
            }));

            m_loadingStringStatus = "Initialisation";
            m_loadingThread.Start();
            m_isLoadingComplete = false;

            

        }


        /// <summary>
        /// Régénère le terrain.
        /// </summary>
        protected override void RegenerateTerrain()
        {
            // Création d'un objet 3D à dessiner.
            m_loadingStringStatus = "Génération du paysage";
            float[,] machin = new float[LANDSCAPE_UNITS_COUNT, LANDSCAPE_UNITS_COUNT];

            Game1.Recorder.Clear();

            m_loadingStringStatus = "Création des modèles du paysage";
            Game1.Recorder.StartRecord("Geomipmap generation");
            m_landscape.Heightmap = machin;
            m_noiseMap.LandscapeIndexBuffer = m_landscape.Indices;
            Game1.Recorder.EndRecord("Geomipmap generation");
        }
        


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
        protected override void DrawDebug(GameTime gameTime)
        {
            // Mets à jour le compteur fps
            m_fpsCounter.AddFrame((float)gameTime.ElapsedGameTime.TotalSeconds);

            // Dessine des infos de debug
            Batch.Begin();
            Batch.DrawString(Font, "Framerate : " + ((int)(m_fpsCounter.GetAverageFps())).ToString().PadLeft(4),
                new Vector2(Game1.Instance.GraphicsDevice.ScissorRectangle.Width-200, 25), Color.White);
            Batch.End();
        }
        /// <summary>
        /// Dessine le hud.
        /// </summary>
        /// <param name="time"></param>
        protected void DrawHUD(GameTime time)
        {
            if(!m_mouseControlEnabled)
                m_hud.Draw(time);
        }
        /// <summary>
        /// Dessine le terrain.
        /// </summary>
        protected override void DrawLandscape(GameTime time, string technique)
        {

            m_landscape.Object.Shader.Parameters["HeightmapTexture"].SetValue(m_noiseMap.HeightmapAndNormalMap);
            m_landscape.Object.Shader.CurrentTechnique = m_landscape.Object.Shader.Techniques[technique];
            m_landscape.Object.Shader.Parameters["mC1"].SetValue(new Vector2(m_c1.Real, m_c1.Imaginary));
            m_landscape.Object.Shader.Parameters["mC2"].SetValue(new Vector2(m_c2.Real, m_c2.Imaginary));
            m_landscape.Object.Shader.Parameters["ClipPlaneNear"].SetValue(0);
            m_landscape.Object.Shader.Parameters["ClipPlaneFar"].SetValue(1);

            // Change la luminosité du brouillard.
            float lum = m_gameWorld.GetCurrentWorldLuminosity();
            Vector4 color = new Vector4(new Vector3(lum), 1);
            m_landscape.Object.Shader.Parameters["xFogColor"].SetValue(color);

            if (technique != "Landscape" && technique != "Shadowed")
                // Pour la réflection, on dessine une version en basse qualité.
                m_landscape.Draw(m_gameWorld);
            else
            {
                if (m_gameWorld.GraphicalParameters.BlurLandscape)
                {
                    // On dessine une première fois le paysage lointain seul.
                    // Cela sert uniquement à écrire dans le depth buffer.
                    Device.SetRenderTarget(m_backBuffer);
                    Device.BlendState = BlendState.Additive;
                    m_landscape.Object.Shader.CurrentTechnique = m_landscape.Object.Shader.Techniques["NoColor"];
                    m_landscape.Object.Shader.Parameters["ClipPlaneNear"].SetValue(m_gameWorld.GraphicalParameters.LandscapeBlurDistance - 0.05f);
                    m_landscapeBlur.Parameters["MatrixTransform"].SetValue(Game1.Instance.PlaneTransform2D);
                    m_landscape.Object.Draw(m_gameWorld); // dessin + cull

                    // On réutilise la technique demandée.
                    m_landscape.Object.Shader.CurrentTechnique = m_landscape.Object.Shader.Techniques[technique];
                    Device.BlendState = BlendState.NonPremultiplied;

                    // On redessine le paysage sur le render target de post process
                    Device.SetRenderTarget(m_postProcessRenderTarget);
                    Device.Clear(Color.Transparent);
                    m_landscape.Draw(m_gameWorld); // dessin sans recalcul du culling
                    
                    // Calcul de la luminance adaptée :


                    // Dessin (sans info de profondeur) du paysage avec application du flou.
                    Device.SetRenderTarget(m_backBuffer);

                    if (true)
                    {
                        // Effectue un flou gaussien en 2 passes
                        m_blurEffect.PerformGaussianBlur(m_postProcessRenderTarget, m_blurRenderTarget, m_backBuffer, Batch);
                    }
                    else
                    {
                        // Utilise un flou moisi
                        Batch.Begin(0, BlendState.NonPremultiplied, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, m_landscapeBlur);
                        Batch.Draw(m_postProcessRenderTarget, new Rectangle(0, 0, Game1.Instance.ResolutionWidth, Game1.Instance.ResolutionHeight), Color.White);
                        Batch.End();
                    }
                    
                    // Dessin de la partie "proche" du paysage.
                    Device.SetRenderTarget(m_backBuffer);
                    Device.DepthStencilState = DepthStencilState.Default;
                    Device.BlendState = BlendState.NonPremultiplied;
                    m_landscape.Object.Shader.Parameters["ClipPlaneNear"].SetValue(0);
                    m_landscape.Object.Shader.Parameters["ClipPlaneFar"].SetValue(m_gameWorld.GraphicalParameters.LandscapeBlurDistance);
                    m_landscape.Draw(m_gameWorld);
                }
                else
                {
                    m_landscape.Draw(m_gameWorld);
                }
            }
        }
        /// <summary>
        /// Dessine l'eau
        /// </summary>
        protected override void DrawWater(GameTime time)
        {
            m_water.Update(time);
            m_water.Draw(m_gameWorld);
        }

        float cacahouette;
        /// <summary>
        /// Dessine l'arrière plan.
        /// </summary>
        /// <param name="time"></param>
        protected override void DrawBackground(GameTime time, Rectangle bounds, int alpha = 255, bool reflection=false)
        {     
            m_skybox.Draw(m_gameWorld, reflection);
        }
        /// <summary>
        /// Dessine le rayon de soleil.
        /// </summary>
        /// <param name="time"></param>
        protected void DrawLensFlare(GameTime time)
        {
            // Mets à jour les matrices utilisées par le lens flare.
            m_lensFlare.View = m_gameWorld.View;
            m_lensFlare.Projection = m_gameWorld.Projection;
            m_lensFlare.LightDirection = Vector3.Normalize(-m_gameWorld.LightDirection);
            m_lensFlare.DrawGlow();
            m_lensFlare.DrawFlares();
            
        }
        #endregion

        /* ----------------------------------------------------------------------------
         * -- Scene Elements
         * --------------------------------------------------------------------------*/
        #region Scene
        /// <summary>
        /// Dessine la map de réflexion dans une texture temporaire.
        /// </summary>
        protected override void DrawReflectionMap(GameTime time, Matrix reflectionViewMatrix)
        {
            m_water.Shader.Parameters["wWorldReflectionViewProjection"].SetValue(reflectionViewMatrix);

            // Dessine les éléments à réfléchir.
            if (m_gameWorld.GraphicalParameters.ReflectLandscape)
                DrawLandscape(time, "Reflection");

            // Mets à jour les matrices utilisées par le lens flare.
            m_lensFlare.View = m_gameWorld.View;
            m_lensFlare.Projection = m_gameWorld.Projection;
            m_lensFlare.LightDirection = Vector3.Normalize(-m_gameWorld.LightDirection);
            m_lensFlare.DrawGlow();
            m_lensFlare.DrawFlares();
        }
        
        /// <summary>
        /// Dessine les objets 3D de la scène.
        /// </summary>
        /// <param name="time"></param>
        protected override void Draw3DObjects(GameTime time)
        {
            UpdateRasterizerState();
            // Dessin du terrain.
            if (m_gameWorld.GraphicalParameters.DrawLandscapeShadows)
                DrawLandscape(time, "Shadowed");
            else
                if(m_gameWorld.GraphicalParameters.DrawLandscape)
                    DrawLandscape(time, "Landscape");

            // Dessine l'eau
            DrawWater(time);

            
            // Debug
            if (Input.IsPressed(Microsoft.Xna.Framework.Input.Keys.F))
                m_debugFrustum = new BoundingFrustum(m_gameWorld.View * m_gameWorld.Projection);
            if (m_debugFrustum != null)
                Debug.Renderers.BoundingFrustumRenderer.Render(m_debugFrustum, Game1.Instance.GraphicsDevice, m_gameWorld.View, m_gameWorld.Projection, Color.Green);
            
            DrawLensFlare(time);
            m_lensFlare.UpdateOcclusion();
        }

        /// <summary>
        /// Dessinne tous les élements du jeu sans post process
        /// </summary>
        protected void DrawScene(GameTime gameTime)
        {
            // Dessine l'arrière plan.
            DrawBackground(gameTime, Device.ScissorRectangle);

            // Dessine les modèles 3D
            Draw3DObjects(gameTime);
        }

        /// <summary>
        /// Dessine la scène.
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime time)
        {
            base.Draw(time);

            // Si on a fini de charger, on effectue le rendu.
            if (m_isLoadingComplete)
            {
                // Dessine la réflection map pour le dessin de l'eau.
                DrawReflectionMap_Wrapper(time);

                // Efface le buffer de l'effet de blur
                Device.SetRenderTarget(m_blurRenderTarget);
                Device.BlendState = BlendState.NonPremultiplied;
                Device.Clear(Color.Transparent);

                // Dessine la scène entière.
                Device.SetRenderTarget(m_backBuffer);
                Device.Clear(ClearOptions.Target | ClearOptions.Stencil | ClearOptions.DepthBuffer, Color.Transparent, 1.0f, 0);
                DrawScene(time);
                
                // Dessine la scène qui a été rendue sur le render target de post process
                Device.SetRenderTarget(null);
                Device.BlendState = BlendState.NonPremultiplied;
                Batch.Begin();
                Batch.Draw(m_backBuffer, new Rectangle(0, 0, Device.ScissorRectangle.Width, Device.ScissorRectangle.Height), Color.White);
                Batch.End();
                

                // Dessine les derniers élements de la scène non soumis au post process comme le HUD et le lens flare.
                Device.SetRenderTarget(null);
   

                // DrawLensFlare(time);
                DrawDebug(time);
                // Dessine le HUD.
                DrawHUD(time);
            }
            else
            {
                // Dessin d'une texture pour faire patienter :D
                MathHelpers.Complex c = new MathHelpers.Complex(0.384f , -0.125f)
                    + new MathHelpers.Complex((float)time.TotalGameTime.TotalSeconds / 500.0f, -(float)time.TotalGameTime.TotalSeconds / 500.0f); ;// new MathHelpers.Complex(0.384f + (float)time.TotalGameTime.TotalSeconds / 200.0f, -0.125f + (float)time.TotalGameTime.TotalSeconds / 200.0f);
                
                // On locke l'utilisation du sprite batch sur ce thread car les opérations de chargement peuvent inclure du dessin utilisant
                // le Batch.
                lock (Game1.GraphicsDeviceMutex)
                {
                    int w = Game1.Instance.GraphicsDevice.ScissorRectangle.Width;
                    int h = Game1.Instance.GraphicsDevice.ScissorRectangle.Height;
                    // Si on a pas fini de charger, on affiche l'écran de chargement.
                    Generation.Fractals.Julia.GenerateTexture2DGPU(c, new MathHelpers.Complex(-1, -1), 1.0f / (float)time.TotalGameTime.TotalSeconds * 10, m_loadingTexture);
                    Device.Clear(Color.Black);
                    Batch.Begin();
                    Batch.Draw(m_loadingTexture, new Rectangle(0, 0, w, h), Color.White);
                    Vector2 position = new Vector2(w / 2 - 100, h / 2 - 25);
                    Batch.DrawString(Font, "Chargement en cours" + "".PadLeft((m_frameCounter / 60) % 4, '.'),
                       position, Color.White);
                    position.Y += 25;
                    Batch.DrawString(Font, m_loadingStringStatus, position, Color.White);
                    Batch.End();
                }
                
                if (!m_loadingThread.IsAlive)
                    m_isLoadingComplete = true;
            }

        }
        #endregion
        #endregion


        /* ----------------------------------------------------------------------------
         * Update
         * --------------------------------------------------------------------------*/
        #region Update
        /// <summary>
        /// Vérifie l'appui de touches permettant de modifier les paramètres des fractales.
        /// </summary>
        /// <param name="time"></param>
        void UpdateFractalParametersInput(GameTime time)
        {
            // Paramètres de test
            float cFactor = 0.0005f;
            if (Input.IsPressed(Keys.NumPad8))
            {
                m_c1.Imaginary -= cFactor;
            }
            else if (Input.IsPressed(Keys.NumPad2))
            {
                m_c1.Imaginary += cFactor;
            }

            if (Input.IsPressed(Keys.NumPad4))
            {
                m_c1.Real -= cFactor;
            }
            else if (Input.IsPressed(Keys.NumPad6))
            {
                m_c1.Real += cFactor;
            }


            // Paramétrage du deuXième paramètre.
            if (Input.IsPressed(Keys.NumPad7))
            {
                m_c2.Imaginary -= cFactor;
            }
            else if (Input.IsPressed(Keys.NumPad3))
            {
                m_c2.Imaginary += cFactor;
            }

            if (Input.IsPressed(Keys.NumPad9))
            {
                m_c2.Real -= cFactor;
            }
            else if (Input.IsPressed(Keys.NumPad1))
            {
                m_c2.Real += cFactor;
            }
        }
        /// <summary>
        /// Vérifie l'appui des touches de la manette XBOX.
        /// </summary>
        /// <param name="time"></param>
        void UpdateXBOXInput(GameTime time)
        {
            float delta = Math.Min((float)time.ElapsedGameTime.TotalSeconds, 0.1f);
            float angleSpeed = 2;
            float speed = 10*4;
            m_gameWorld.Camera.MoveForward(Input.GetLeftStickState().Y * delta * speed);
            m_gameWorld.Camera.RotateSide(-Input.GetLeftStickState().X * delta * speed / 20.0f);

            m_gameWorld.Camera.RotateRoll(-Input.GetRightStickState().X * angleSpeed * delta);
            m_gameWorld.Camera.RotateUpDown(-Input.GetRightStickState().Y * angleSpeed * delta);
        }

        /// <summary>
        /// Vérifie l'appui des touches permettant de se déplacer au clavier.
        /// </summary>
        /// <param name="time"></param>
        void UpdateKeyboardMovementInput(GameTime time)
        {
            float delta = Math.Min((float)time.ElapsedGameTime.TotalSeconds, 0.1f);
            float speed = LANDSCAPE_UNIT_SIZE;
            
            if (Input.IsPressed(Keys.LeftShift))
                speed *= 20;
            if (Input.IsPressed(Keys.Space))
                speed *= 5;

            float value = speed;

            if (Input.IsPressed(Keys.Z))
                m_gameWorld.Camera.MoveForward(value);
            else if (Input.IsPressed(Keys.S))
                m_gameWorld.Camera.MoveForward(-value);
            if (Input.IsPressed(Keys.Q))
                m_gameWorld.Camera.MoveSide(value);
            else if (Input.IsPressed(Keys.D))
                m_gameWorld.Camera.MoveSide(-value);


        }
        /// <summary>
        /// Mets à jour la caméra contrôlée par la souris.
        /// </summary>
        /// <param name="time"></param>
        void UpdateMouseCamera(GameTime time)
        {
            float angleSpeed = 30;
            // Angle de la caméra.
            float delta = Math.Min((float)time.ElapsedGameTime.TotalSeconds, 0.1f);
            MouseState mouse = Input.GetMouseState();
            int centerX = Game1.Instance.Window.ClientBounds.Width / 2;
            int centerY = Game1.Instance.Window.ClientBounds.Height / 2;
            Mouse.SetPosition(centerX, centerY);
            float cameraRotationX = -MathHelper.ToRadians((mouse.X - centerX) * angleSpeed * 0.01f);
            float cameraRotationY = MathHelper.ToRadians((mouse.Y - centerY) * angleSpeed * 0.01f);

            m_gameWorld.Camera.RotateSide(cameraRotationX * angleSpeed * delta);
            m_gameWorld.Camera.RotateUpDown(-cameraRotationY * angleSpeed * delta);
        }

        /// <summary>
        /// Mets à jour tous les appuis de touches concernant la modification des paramètres graphiques.
        /// </summary>
        /// <param name="time"></param>
        void UpdateGraphicalParametersInput(GameTime time)
        {
            // Modification de l'effet de bloom.
            if (Input.IsGamepadPressed(Microsoft.Xna.Framework.Input.Buttons.LeftShoulder) || Input.IsPressed(Microsoft.Xna.Framework.Input.Keys.J))
                m_bloomEffect.BloomEffectThreshold -= 0.01f;
            if (Input.IsGamepadPressed(Microsoft.Xna.Framework.Input.Buttons.RightShoulder) || Input.IsPressed(Microsoft.Xna.Framework.Input.Keys.K))
                m_bloomEffect.BloomEffectThreshold += 0.01f;
            if (Input.IsGamepadPressed(Microsoft.Xna.Framework.Input.Buttons.X) || Input.IsPressed(Microsoft.Xna.Framework.Input.Keys.P))
                m_bloomEffect.BloomRadius++;
            if (Input.IsGamepadPressed(Microsoft.Xna.Framework.Input.Buttons.Y) || Input.IsPressed(Microsoft.Xna.Framework.Input.Keys.O))
                m_bloomEffect.BloomRadius--;
            if (Input.IsGamepadPressed(Microsoft.Xna.Framework.Input.Buttons.Start) || Input.IsPressed(Microsoft.Xna.Framework.Input.Keys.M))
                m_bloomEffect.BloomAmount += 0.1f;
            if (Input.IsGamepadPressed(Microsoft.Xna.Framework.Input.Buttons.Back) || Input.IsPressed(Microsoft.Xna.Framework.Input.Keys.L))
                m_bloomEffect.BloomAmount -= 0.1f;
            // Heure
            if (Input.IsGamepadPressed(Buttons.A) || Input.IsPressed(Keys.I))
                m_gameWorld.Hour += 0.1f;
            if (Input.IsGamepadPressed(Buttons.B) || Input.IsPressed(Keys.U))
                m_gameWorld.Hour -= 0.1f;
        }
        /// <summary>
        /// Initialise le HUD.
        /// </summary>
        void InitializeHUD()
        {
            // Bloom
            World.HUD.HUDCheckbox bloomCb = new World.HUD.HUDCheckbox();
            bloomCb.Text = "BloomEnabled";
            bloomCb.Position = new Vector2(10, 10);
            bloomCb.Checked = m_gameWorld.GraphicalParameters.BloomEnabled;
            bloomCb.ValueChanged += delegate(bool newValue)
            {
                m_gameWorld.GraphicalParameters.BloomEnabled = newValue;
            };

            // Bloom radius
            World.HUD.HUDTrackbar bloomRadiusCb = new World.HUD.HUDTrackbar();
            bloomRadiusCb.Text = "BloomRadius";
            bloomRadiusCb.Value = m_bloomEffect.BloomRadius;
            bloomRadiusCb.Position = bloomCb.Position + new Vector2(25, 25);
            bloomRadiusCb.MinValue = 1;
            bloomRadiusCb.MaxValue = 20;
            bloomRadiusCb.ValueChanged += delegate(float newValue)
            {
                m_bloomEffect.BloomRadius = (int)newValue;
            };

            // Bloom threshold
            World.HUD.HUDTrackbar bloomThreshTb = new World.HUD.HUDTrackbar();
            bloomThreshTb.Text = "BloomThreshold";
            bloomThreshTb.Value = m_bloomEffect.BloomEffectThreshold;
            bloomThreshTb.Position = bloomCb.Position + new Vector2(25, 50);
            bloomThreshTb.MinValue = 0;
            bloomThreshTb.MaxValue = 1;
            bloomThreshTb.ValueChanged += delegate(float newValue)
            {
                m_bloomEffect.BloomEffectThreshold = newValue;
            };


            // -- LANDSCAPE BLUR
            // Activation / désactivation
            World.HUD.HUDCheckbox landscapeBlur = new World.HUD.HUDCheckbox();
            landscapeBlur.Text = "DepthOfField";
            landscapeBlur.Position = new Vector2(bloomCb.Position.X, bloomThreshTb.Position.Y+25);
            landscapeBlur.Checked = m_gameWorld.GraphicalParameters.BlurLandscape;
            landscapeBlur.ValueChanged += delegate(bool newValue)
            {
                m_gameWorld.GraphicalParameters.BlurLandscape = newValue;
            };
            // Distance
            World.HUD.HUDTrackbar landscapeBlurDistance = new World.HUD.HUDTrackbar();
            landscapeBlurDistance.Text = "BlurDistance";
            landscapeBlurDistance.Value = m_gameWorld.GraphicalParameters.LandscapeBlurDistance;
            landscapeBlurDistance.Position = landscapeBlur.Position + new Vector2(25, 25);
            landscapeBlurDistance.MinValue = 0;
            landscapeBlurDistance.MaxValue = 1;
            landscapeBlurDistance.ValueChanged += delegate(float newValue)
            {
                m_gameWorld.GraphicalParameters.LandscapeBlurDistance = newValue;
            };
            // Radius
            World.HUD.HUDTrackbar landscapeBlurRadius = new World.HUD.HUDTrackbar();
            landscapeBlurRadius.Text = "BlurRadius";
            landscapeBlurRadius.Value = m_blurEffect.Radius;
            landscapeBlurRadius.Position = landscapeBlurDistance.Position + new Vector2(0, 25);
            landscapeBlurRadius.MinValue = 1;
            landscapeBlurRadius.MaxValue = 20;
            landscapeBlurRadius.ValueChanged += delegate(float newValue)
            {
                m_blurEffect.ComputeKernel((int)newValue, m_blurEffect.Amount);
            };
            // Amount
            World.HUD.HUDTrackbar landscapeBlurAmount = new World.HUD.HUDTrackbar();
            landscapeBlurAmount.Text = "BlurAmount";
            landscapeBlurAmount.Value = m_blurEffect.Amount;
            landscapeBlurAmount.Position = landscapeBlurRadius.Position + new Vector2(0, 25);
            landscapeBlurAmount.MinValue = 1;
            landscapeBlurAmount.MaxValue = 10;
            landscapeBlurAmount.ValueChanged += delegate(float newValue)
            {
                m_blurEffect.ComputeKernel(m_blurEffect.Radius, newValue);
            };

            // -- HOUR
            World.HUD.HUDTrackbar hourTrackbar = new World.HUD.HUDTrackbar();
            hourTrackbar.Text = "Hour";
            hourTrackbar.Value = m_blurEffect.Amount;
            hourTrackbar.Position = new Vector2(bloomCb.Position.X, landscapeBlurAmount.Position.Y + 25);
            hourTrackbar.MinValue = 0;
            hourTrackbar.MaxValue = 24;
            hourTrackbar.ValueChanged += delegate(float newValue)
            {
                m_gameWorld.Hour = newValue;
            };

            // -- COMPLEX
            // Partie réelle
            World.HUD.HUDTrackbar textureComplexReal = new World.HUD.HUDTrackbar();
            textureComplexReal.Text = "C (real)";
            textureComplexReal.Value = m_c2.Real;
            textureComplexReal.Position = new Vector2(bloomCb.Position.X, hourTrackbar.Position.Y + 25);
            textureComplexReal.MinValue = -1;
            textureComplexReal.MaxValue = 1;
            textureComplexReal.ValueChanged += delegate(float newValue)
            {
                m_c2.Real = newValue;
            };

            // Partie imaginaire
            World.HUD.HUDTrackbar textureComplexImaginary = new World.HUD.HUDTrackbar();
            textureComplexImaginary.Text = "C (imaginary)";
            textureComplexImaginary.Value = m_c2.Imaginary;
            textureComplexImaginary.Position = new Vector2(bloomCb.Position.X, textureComplexReal.Position.Y + 25);
            textureComplexImaginary.MinValue = -1;
            textureComplexImaginary.MaxValue = 1;
            textureComplexImaginary.ValueChanged += delegate(float newValue)
            {
                m_c2.Imaginary = newValue;
            };

            // -- DRAWING
            // Grass
            World.HUD.HUDCheckbox drawGrass = new World.HUD.HUDCheckbox();
            drawGrass.Text = "Grass";
            drawGrass.Position = new Vector2(bloomCb.Position.X, textureComplexImaginary.Position.Y + 25);
            drawGrass.Checked = m_gameWorld.GraphicalParameters.DrawGrass;
            drawGrass.ValueChanged += delegate(bool newValue)
            {
                m_gameWorld.GraphicalParameters.DrawGrass = newValue;
                m_gameWorld.GraphicalParameters.ReflectGrass = m_gameWorld.GraphicalParameters.DrawGrass & m_gameWorld.GraphicalParameters.ReflectGrass;
            };
            World.HUD.HUDCheckbox reflectGrass = new World.HUD.HUDCheckbox();
            reflectGrass.Text = "Reflect";
            reflectGrass.Position = new Vector2(bloomCb.Position.X+200, textureComplexImaginary.Position.Y + 25);
            reflectGrass.Checked = m_gameWorld.GraphicalParameters.ReflectGrass;
            reflectGrass.ValueChanged += delegate(bool newValue)
            {
                m_gameWorld.GraphicalParameters.ReflectGrass = newValue;
                
            };
            // Arbres
            World.HUD.HUDCheckbox drawTrees = new World.HUD.HUDCheckbox();
            drawTrees.Text = "Trees";
            drawTrees.Position = new Vector2(bloomCb.Position.X, drawGrass.Position.Y + 25);
            drawTrees.Checked = m_gameWorld.GraphicalParameters.DrawTrees;
            drawTrees.ValueChanged += delegate(bool newValue)
            {
                m_gameWorld.GraphicalParameters.DrawTrees = newValue;
            };
            World.HUD.HUDCheckbox reflectTrees = new World.HUD.HUDCheckbox();
            reflectTrees.Text = "Reflect";
            reflectTrees.Position = new Vector2(bloomCb.Position.X+200, drawGrass.Position.Y + 25);
            reflectTrees.Checked = m_gameWorld.GraphicalParameters.ReflectTrees;
            reflectTrees.ValueChanged += delegate(bool newValue)
            {
                m_gameWorld.GraphicalParameters.ReflectTrees = newValue;
                m_gameWorld.GraphicalParameters.ReflectTrees = m_gameWorld.GraphicalParameters.DrawTrees & m_gameWorld.GraphicalParameters.ReflectTrees;
            };
            // -- LANDSCAPE QUALITY
            World.HUD.HUDCheckbox hqLandscape = new World.HUD.HUDCheckbox();
            hqLandscape.Text = "HQ Landscape";
            hqLandscape.Position = new Vector2(bloomCb.Position.X, reflectTrees.Position.Y + 25);
            hqLandscape.Checked = m_gameWorld.GraphicalParameters.LandscapeMaxQuality == ModelMipmap.ModelQuality.High;
            hqLandscape.ValueChanged += delegate(bool newValue)
            {
                m_gameWorld.GraphicalParameters.LandscapeMaxQuality = newValue ? ModelMipmap.ModelQuality.High : ModelMipmap.ModelQuality.Medium;
            };
            // Ajout des composants
            m_hud.Components.Add("Bloom", bloomCb);
            m_hud.Components.Add("BloomRadius", bloomRadiusCb);
            m_hud.Components.Add("BloomThreshold", bloomThreshTb);
            m_hud.Components.Add("DepthOfField", landscapeBlur);
            m_hud.Components.Add("BlurDistance", landscapeBlurDistance);
            m_hud.Components.Add("BlurRadius", landscapeBlurRadius);
            m_hud.Components.Add("BlurAmount", landscapeBlurAmount);
            m_hud.Components.Add("Hour", hourTrackbar);
            m_hud.Components.Add("TextureComplexReal", textureComplexReal);
            m_hud.Components.Add("TextureComplexImaginary", textureComplexImaginary);
            m_hud.Components.Add("DrawGrass", drawGrass);
            m_hud.Components.Add("ReflectGrass", reflectGrass);
            m_hud.Components.Add("DrawTrees", drawTrees);
            m_hud.Components.Add("ReflectTrees", reflectTrees);
            m_hud.Components.Add("HQLandscape", hqLandscape);
        }
        /// <summary>
        /// Mise à jour du HUD.
        /// </summary>
        /// <param name="time"></param>
        void UpdateHUD(GameTime time)
        {
            // World.HUD.HUDCheckbox bloomCb = (World.HUD.HUDCheckbox)m_hud.Components["Bloom"];

        }
        /// <summary>
        /// Mets à jour tout ce qui est lié à l'appui de touches sur le clavier, et aux actions qui en résultent.
        /// </summary>
        protected override void UpdateInput(GameTime time)
        {
            // Attracteur du vent.
            m_windAttractor.NextStep(0.5f*(float)time.ElapsedGameTime.TotalSeconds, 100);
            
            // Caméra souris
            if(m_mouseControlEnabled)
                UpdateMouseCamera(time);

            // Mouvement 
            UpdateKeyboardMovementInput(time);
            UpdateXBOXInput(time);

            // Paramètres graphiques
            UpdateGraphicalParametersInput(time);
            UpdateHUD(time);


            // Affichage du HUD.
            if (Input.IsTrigger(Keys.LeftAlt))
            {
                m_mouseControlEnabled = !m_mouseControlEnabled;
                int centerX = Game1.Instance.Window.ClientBounds.Width / 2;
                int centerY = Game1.Instance.Window.ClientBounds.Height / 2;
                Mouse.SetPosition(centerX, centerY);
            }

            // Mise à jour du HUD si la capture de la souris est désactivée.
            if (!m_mouseControlEnabled)
                UpdateHUD(time);

            if (Input.IsPressed(Keys.NumPad5))
                m_gameWorld.Camera.Position = Vector3.Zero;
            if (Input.IsTrigger(Keys.G) && m_isLoadingComplete)
                RegenerateTerrain();

            UpdateFractalParametersInput(time);
            float offsetX = m_gameWorld.Camera.Position.X;
            float offsetY = m_gameWorld.Camera.Position.Y;
            offsetX = offsetX - offsetX % LANDSCAPE_UNIT_SIZE;
            offsetY = offsetY - offsetY % LANDSCAPE_UNIT_SIZE;

            m_landscape.Object.Position = new Vector3(offsetX,
                offsetY,
                m_landscape.Object.Position.Z);

            m_landscape.Object.Shader.Parameters["Offset"].SetValue(
                new Vector2(offsetX/m_landscape.HScale,
                    offsetY/m_landscape.HScale));
            m_landscape.Object.Shader.Parameters["LandscapeSampleSize"].SetValue(LANDSCAPE_UNIT_SIZE);

            // Effectue un arrondi de la position.
            float xAdjust = m_gameWorld.Camera.Position.X % LANDSCAPE_UNIT_SIZE;
            xAdjust = xAdjust < LANDSCAPE_UNIT_SIZE / 2 ? -xAdjust : LANDSCAPE_UNIT_SIZE - xAdjust;
            float yAdjust = m_gameWorld.Camera.Position.Y % LANDSCAPE_UNIT_SIZE;
            yAdjust = yAdjust < LANDSCAPE_UNIT_SIZE / 2 ? -yAdjust : LANDSCAPE_UNIT_SIZE - yAdjust;
            m_gameWorld.Camera.Position = new Vector3(m_gameWorld.Camera.Position.X,
                                        m_gameWorld.Camera.Position.Y,
                                        m_gameWorld.Camera.Position.Z);
            // Dessine le terrain.
            m_noiseMap.Update(new Vector2(offsetX / m_landscape.HScale,
                    offsetY / m_landscape.HScale));
        }
        /// <summary>
        /// Mets à jour la scène.
        /// </summary>
        /// <param name="time"></param>
        public override void Update(GameTime time)
        {
            if (!m_isLoadingComplete)
                return;
            base.Update(time);
        }
        #endregion

    }
}
