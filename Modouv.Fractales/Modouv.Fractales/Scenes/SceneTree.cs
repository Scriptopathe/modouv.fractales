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
using Modouv.Fractales.World.Objects.Particles;
namespace Modouv.Fractales.Scenes
{
    /// <summary>
    /// Scene de test du générateur de fractales.
    /// </summary>
    public class SceneTree : SceneFractalWorld
    {
        protected override void DrawLandscape(GameTime time, string technique)
        {
            throw new NotImplementedException();
        }
        /* ----------------------------------------------------------------------------
        * Constants
        * --------------------------------------------------------------------------*/
        #region Constants
        public const bool USE_HDR_BLENDABLE_RENDER_TARGET = true;
        #endregion
        /* ----------------------------------------------------------------------------
        * Enums
        * --------------------------------------------------------------------------*/
        #region Enums
        public enum Season
        {
            Winter,
            Summer
        }
        #endregion
        /* ----------------------------------------------------------------------------
         * Variables
         * --------------------------------------------------------------------------*/
        #region Variables
        #region Paramètres de rendu / fractales etc...

        /// <summary>
        /// Paramètre "c" de la première fractale dessinée sur le terrain, 
        /// mappée sur les coordonnées de teXture.
        /// </summary>
        MathHelpers.Complex m_c1 = new MathHelpers.Complex(-0.820f, 0.9030f);//new MathHelpers.Complex(-0.820f, 0.3030f);
        /// <summary>
        /// Paramètre de "c" de la deuXième fractale dessinée sur le terrain, mappée sur
        /// les normales du mesh.
        /// </summary>
        MathHelpers.Complex m_c2 = new MathHelpers.Complex(0.500f, 0.9045f);//new MathHelpers.Complex(-0.835f, 0.3045f);
        #endregion

        #region Objets de la scène
        /// <summary>
        /// Chaine de calcul de la luminance adaptée de la scène.
        /// </summary>
        LuminanceCalculationChain m_luminanceCalculationChain;
        /// <summary>
        /// Populations d'arbres.
        /// </summary>
        IObject3D m_tree;
        /// <summary>
        /// Population de light sprites.
        /// </summary>
        ObjectCullingGroup m_lightSprites;
        /// <summary>
        /// Texture du ciel
        /// </summary>
        Texture2D m_skyTexture;
        /// <summary>
        /// Eau.
        /// </summary>
        WaterObject m_water;
        /// <summary>
        /// Map de refraction de l'eau.
        /// </summary>
        RenderTarget2D m_refractionMapRenderTarget;
        /// <summary>
        /// Skybox utilisée en arrière plan.
        /// </summary>
        Skybox m_skybox;
        /// <summary>
        /// Shader utilisé pour le dessin des arbres fractals.
        /// </summary>
        Effect m_fractalTreesShader;
        /// <summary>
        /// Interface utilisateur.
        /// </summary>
        World.HUD.HUDManager m_hud;
        #endregion

        #region Objets de post process
        /// <summary>
        /// Contient les informations de profondeur de la scène.
        /// </summary>
        RenderTarget2D m_depthBuffer;
        /// <summary>
        /// HDR back buffer.
        /// </summary>
        RenderTarget2D m_hdrBuffer;
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
        /// Effet de bloom utilisé si BloomEnabled vaut vrai.
        /// </summary>
        Bloom m_bloomEffect;
        /// <summary>
        /// Effet de profondeur de champ.
        /// </summary>
        DepthOfField m_depthOfFieldEffect;
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
        /// <summary>
        /// Obtient l'attracteur chaotique en charge de la répartition du vent.
        /// </summary>
        public Generation.Attractors.HenonAttractor WindAttractor
        {
            get { return m_windAttractor; }
        }
        /// <summary>
        /// Obtient ou définit le viewport sur lequel est effectué l'affichage du rendu final.
        /// </summary>
        public Rectangle Viewport
        {
            get;
            set;
        }

        public Generation.Populations.WorldFantasy.FractalTreePopulator.TreeKind TreeKind
        {
            get;
            set;
        }
        #endregion
        /* ----------------------------------------------------------------------------
         * Initialisation
         * --------------------------------------------------------------------------*/
        #region Initialisation

        /// <summary>
        /// Crée une nouvelle instance de SceneTestJulia.
        /// </summary>
        public SceneTree(Generation.Populations.WorldFantasy.FractalTreePopulator.TreeKind kind) : base()
        {
            TreeKind = kind;
        }

        /// <summary>
        /// Initialise la scène.
        /// </summary>
        public override void Initialize()
        {
            /*int size = 2048;
            Debug.Tools.TextureAtlasCreator.CreateTilableTextures("tex_1.jpg", "tex1t.jpg", size / 2, size / 2, false);
            Debug.Tools.TextureAtlasCreator.CreateTilableTextures("tex_2.jpg", "tex2t.jpg", size / 2, size / 2, false);
            Debug.Tools.TextureAtlasCreator.CreateTilableTextures("tex_3.jpg", "tex3t.jpg", size / 2, size / 2, false);
            Debug.Tools.TextureAtlasCreator.CreateTilableTextures("tex_4.jpg", "tex4t.jpg", size / 2, size / 2);
            Debug.Tools.TextureAtlasCreator.CreateFromTextures(new string[] { "tex1t.jpg", "tex2t.jpg", "tex4t.jpg", "tex3t.jpg" }, 
                "world_fantasy\\landscape-summer.jpg", size, size);
            throw new Exception(); // */
            //Debug.Tools.TextureAtlasCreator.CreateFromTextures(new string[] { "grass1.png", "grass2.png", "grass3.png", "grass4.png" }, "grass.png", 512, 512);

            base.Initialize();

            // Initialise le viewport.
            Viewport = new Rectangle(0, 0, Device.ScissorRectangle.Width, Device.ScissorRectangle.Height);

            // Initialise la refraction map du render target.
            m_refractionMapRenderTarget = new RenderTarget2D(Game1.Instance.GraphicsDevice,
                    Game1.Instance.ResolutionWidth / m_gameWorld.GraphicalParameters.ReflectionMapSampleSize,
                    Game1.Instance.ResolutionHeight / m_gameWorld.GraphicalParameters.ReflectionMapSampleSize, true, SurfaceFormat.Dxt5, DepthFormat.Depth24Stencil8);

            // Chaine de calcul de la luminosité adaptée de la scène.
            m_luminanceCalculationChain = new LuminanceCalculationChain(new Point(Game1.Instance.ResolutionWidth, Game1.Instance.ResolutionHeight));

            // Création du render target de chargement
            m_loadingTexture = new RenderTarget2D(Game1.Instance.GraphicsDevice, Game1.Instance.ResolutionWidth, Game1.Instance.ResolutionHeight, false, SurfaceFormat.Color, DepthFormat.None);
            
            // Chargement de la texture du ciel.
            m_skyTexture = Game1.Instance.Content.Load<Texture2D>("textures\\world_fantasy\\sky");
            Texture2D cerisierTexture = Game1.Instance.Content.Load<Texture2D>("textures\\world_fantasy\\pommier");
            Texture2D pommierTexture = Game1.Instance.Content.Load<Texture2D>("textures\\world_fantasy\\pommier");

            // Crée et paramètre le shader utilisé pour les arbres fractals
            m_fractalTreesShader = Content.Load<Effect>("Shaders\\world_fantasy\\fractal-tree");
            m_fractalTreesShader.Parameters["TreeTexture"].SetValue(cerisierTexture);

            // Populations d'arbres en fonction des saisons.
            m_tree = Generation.Populations.WorldFantasy.FractalTreePopulator.GenerateUnique(m_fractalTreesShader, TreeKind);

            // Création de l'eau
            m_water = new WaterObject(m_gameWorld.GraphicalParameters);
            m_water.SetReflectionMap(m_reflectionMapRenderTarget);
            m_water.SetRefractionMap(m_refractionMapRenderTarget);

            // Création de la skybox
            m_skybox = new Skybox();

            // Création des principaux render targets
            if (m_gameWorld.GraphicalParameters.UseHDR && USE_HDR_BLENDABLE_RENDER_TARGET)
            {
                m_backBuffer = new RenderTarget2D(Game1.Instance.GraphicsDevice, Game1.Instance.ResolutionWidth, Game1.Instance.ResolutionHeight, false, SurfaceFormat.HdrBlendable, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.PreserveContents);
                m_depthBuffer = new RenderTarget2D(Game1.Instance.GraphicsDevice, Game1.Instance.ResolutionWidth, Game1.Instance.ResolutionHeight, false, SurfaceFormat.HdrBlendable, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.PreserveContents);
            }
            else
            {
                m_backBuffer = new RenderTarget2D(Game1.Instance.GraphicsDevice, Game1.Instance.ResolutionWidth, Game1.Instance.ResolutionHeight, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.PreserveContents);
                m_depthBuffer = new RenderTarget2D(Game1.Instance.GraphicsDevice, Game1.Instance.ResolutionWidth, Game1.Instance.ResolutionHeight, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.PreserveContents);
            }
            
            m_postProcessRenderTarget = new RenderTarget2D(Game1.Instance.GraphicsDevice, Game1.Instance.ResolutionWidth, Game1.Instance.ResolutionHeight, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.PreserveContents);
            m_blurRenderTarget = new RenderTarget2D(Game1.Instance.GraphicsDevice, Game1.Instance.ResolutionWidth, Game1.Instance.ResolutionHeight, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.PreserveContents);
            m_hdrBuffer = new RenderTarget2D(Game1.Instance.GraphicsDevice, Game1.Instance.ResolutionWidth, Game1.Instance.ResolutionHeight, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.PreserveContents);
            
            // Paramétrage de l'effet de flou
            GaussianBlur blur = new GaussianBlur(Game1.Instance);
            blur.ComputeKernel(3, 5.61f);
            blur.ComputeOffsets(Game1.Instance.ResolutionWidth, Game1.Instance.ResolutionHeight);
            m_blurEffect = blur;

            // Paramétrage du bloom
            m_bloomEffect = new Bloom();

            // Paramétrage du depth of field
            m_depthOfFieldEffect = new DepthOfField();

            // Création du HUD.
            m_hud = new World.HUD.HUDManager();
            InitializeHUD();

            // Attracteur chaotique
            m_windAttractor = new Generation.Attractors.HenonAttractor();

            // Création du thread de chargement
            m_loadingThread = new System.Threading.Thread(new System.Threading.ThreadStart(delegate()
            {
                if (Program.LOG_EXCEPTIONS)
                {
                    try
                    {
                        RegenerateTerrain();
                        
                    }
                    catch (Exception e)
                    {

                        var stream = System.IO.File.Open("log.txt", System.IO.FileMode.OpenOrCreate);
                        System.IO.StreamWriter w = new System.IO.StreamWriter(stream);
                        w.WriteLine(e.Message);
                        w.WriteLine(e.StackTrace);
                        w.WriteLine(e.InnerException);
                        w.Close();
                        stream.Close();
                        Game1.Instance.Exit();
                    }
                }
                else
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

            // Regénération des lumières.
            Game1.Recorder.StartRecord("Fractal trees generation");
            m_loadingStringStatus = "Génération des arbres";


            Game1.Recorder.EndRecord("Fractal trees generation");
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
            // Dessine le HUD
            if(!m_mouseControlEnabled)
                m_hud.Draw(time);
        }
        /// <summary>
        /// Dessine l'eau
        /// </summary>
        protected override void DrawWater(GameTime time)
        {
            m_water.Shader.CurrentTechnique = m_water.Shader.Techniques["ReflectedWater"];
            m_water.Update(time);
            m_water.Draw(m_gameWorld);
        }

        float cacahouette;
        /// <summary>
        /// Dessine les arbres fractals.
        /// </summary>
        /// <param name="reflection">True si les arbres sont dessinés à l'aide de réflection.</param>
        protected void DrawFractalTrees(GameTime time, bool reflection=false)
        {
            Device.BlendState = BlendState.NonPremultiplied;
            Device.DepthStencilState = DepthStencilState.Default;
            Device.RasterizerState = _rasterizerState;

            // Dessine de la flore créée par ObjectPopulator.
            m_fractalTreesShader.CurrentTechnique = m_fractalTreesShader.Techniques["Ambient"];
            m_fractalTreesShader.Parameters["WindDirection"].SetValue(m_windAttractor.CurrentPosition / 2.0f);

            // Le paramètre réel de la fractale permet d'approximer la "froideur" du paysage en hiver.
            m_fractalTreesShader.Parameters["Coldness"].SetValue(0);
            m_fractalTreesShader.Parameters["SnowPow"].SetValue(1);
            m_fractalTreesShader.Parameters["SnowThreshold"].SetValue(1);
            m_fractalTreesShader.Parameters["SnowNormal"].SetValue(Vector3.Zero);


            m_gameWorld.SetRenderDistance(reflection ?
                m_gameWorld.GraphicalParameters.ReflectedTreesRenderDistance : m_gameWorld.GraphicalParameters.TreesRenderDistance);

            m_tree.Draw(m_gameWorld);

            m_gameWorld.RestoreRenderDistance();
        }
        /// <summary>
        /// Dessine l'arrière plan.
        /// </summary>
        /// <param name="time"></param>
        protected override void DrawBackground(GameTime time, Rectangle bounds, int alpha = 255, bool reflection=false)
        {     
            m_skybox.Draw(m_gameWorld, reflection);
        }

        float windTime = 0.0f;
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

            if (m_gameWorld.GraphicalParameters.ReflectTrees)
                DrawFractalTrees(time, true);
        }
        /// <summary>
        /// Dessine la map de réfraction dans une texture temporaire.
        /// </summary>
        protected void DrawRefractionMap(GameTime time)
        {
            Game1.Instance.GraphicsDevice.SetRenderTarget(m_refractionMapRenderTarget);
            Game1.Instance.GraphicsDevice.BlendState = BlendState.NonPremultiplied;

            m_water.Shader.Parameters["wWorldReflectionViewProjection"].SetValue(m_gameWorld.View);

            // Dessine l'arrière plan.
            DrawBackground(time, m_refractionMapRenderTarget.Bounds, 255);

        }
        /// <summary>
        /// Dessine les objets 3D de la scène.
        /// </summary>
        /// <param name="time"></param>
        protected override void Draw3DObjects(GameTime time)
        {
            UpdateRasterizerState();

            // On dessine sur le backbuffer et mon met les infos de profondeurs dans un depth buffer séparé.
            Device.SetRenderTargets(new RenderTargetBinding(m_backBuffer), new RenderTargetBinding(m_depthBuffer));

            // Dessine l'eau
            DrawWater(time);

            // Dessine les arbres
            if (m_gameWorld.GraphicalParameters.DrawTrees)
                DrawFractalTrees(time);

            
            // Debug
            if (Input.IsPressed(Microsoft.Xna.Framework.Input.Keys.F))
                m_debugFrustum = new BoundingFrustum(m_gameWorld.View * m_gameWorld.Projection);
            if (m_debugFrustum != null)
                Debug.Renderers.BoundingFrustumRenderer.Render(m_debugFrustum, Game1.Instance.GraphicsDevice, m_gameWorld.View, m_gameWorld.Projection, Color.Green);
            
            DrawLensFlare(time);
            Device.BlendState = BlendState.NonPremultiplied;
            m_lensFlare.UpdateOcclusion();
        }

        /// <summary>
        /// Dessinne tous les élements du jeu sans post process
        /// </summary>
        protected void DrawScene(GameTime gameTime)
        {
            // Dessine l'arrière plan.
            Game1.Instance.GraphicsDevice.SetRenderTargets(
                new RenderTargetBinding(m_backBuffer), 
                new RenderTargetBinding(m_gameWorld.BackgroundRenderTarget));
            DrawBackground(gameTime, Device.ScissorRectangle);

            // Dessine les modèles 3D
            Draw3DObjects(gameTime);
        }

        /// <summary>
        /// Dessine la map de réflexion dans une texture temporaire.
        /// </summary>
        protected void DrawShadowMap(GameTime time)
        {

            // Crée la matrice "View" de la réflection. Typiquement la caméra à l'envers.
            Vector4 oldLightPos = m_gameWorld.LightPosition;

            Vector3 lightPos = new Vector3(m_gameWorld.LightPosition.X, m_gameWorld.LightPosition.Y, m_gameWorld.LightPosition.Z);
            lightPos *= 1.2f;
            var lightViewMatrix = Matrix.CreateLookAt(lightPos, Vector3.Zero, -Vector3.UnitZ);

            m_gameWorld.LightView = lightViewMatrix;
            m_gameWorld.LightPosition = new Vector4(lightPos.X, lightPos.Y, lightPos.Z, 1);
            GraphicsDevice device = Game1.Instance.GraphicsDevice;

            // Dessine la scène réflechie sur le RenderTarget temporaire.
            device.SetRenderTarget(m_gameWorld.ShadowMapRenderTarget);
            device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.White, 1.0f, 0);
            m_gameWorld.View = lightViewMatrix;

            // Restore le blend state pour qu'il gère la transparence.
            device.BlendState = BlendState.Opaque;
            device.DepthStencilState = DepthStencilState.Default;
            RasterizerState state = new RasterizerState();
            state.CullMode = CullMode.None;
            state.FillMode = FillMode.Solid;
            device.RasterizerState = state;

            // Dessine la map de réflection.
            DrawLandscape(time, "ShadowMap");
            m_fractalTreesShader.CurrentTechnique = m_fractalTreesShader.Techniques["ShadowMapInstanced"];

            m_tree.Draw(m_gameWorld);
            // Restore les paramètres précédents du monde
            m_gameWorld.RestoreCameraView();


            // Restore les paramètres du device.
            device.SetRenderTarget(null);
            device.RasterizerState = _rasterizerState;
            device.DepthStencilState = DepthStencilState.Default;
            m_gameWorld.LightPosition = oldLightPos;
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
                // Efface le "depth buffer"
                Device.SetRenderTarget(m_depthBuffer);
                Device.Clear(Color.White);
                

                // Dessine la Shadow Map utilisée pour les ombres dynamiques
                if (m_gameWorld.GraphicalParameters.DrawLandscapeShadows)
                {
                    if (m_gameWorld.NeedRefreshShadowMap)
                    {
                        DrawShadowMap(time);
                        m_gameWorld.NeedRefreshShadowMap = false;
                    }
                }

                // Dessine la map de réfraction
                DrawRefractionMap(time);

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

                // Calcule la luminosité de la scène.
                m_luminanceCalculationChain.CalculateLuminance(m_backBuffer);


                // Dessine la scène qui a été rendue sur le render target de post process
                var dstBuffer = m_hdrBuffer; // buffer quoi doit être dessiné à la fin.
                var srcBuffer = m_backBuffer;

                // Applique le blur
                if (m_gameWorld.GraphicalParameters.BlurLandscape)
                {
                    // Dessine le back buffer avec bloom
                    m_depthOfFieldEffect.ProcessDepthOfField(srcBuffer, m_blurRenderTarget, m_postProcessRenderTarget, dstBuffer,
                        m_gameWorld,
                        m_luminanceCalculationChain.AdaptedLuminance,
                        m_depthBuffer,
                        Batch);

                    dstBuffer = m_backBuffer;
                    srcBuffer = m_hdrBuffer;
                }

                // Applique l'effet de bloom
                if (m_gameWorld.GraphicalParameters.BloomEnabled)
                {
                    // Dessine le back buffer avec bloom
                    m_bloomEffect.ProcessBloom(srcBuffer, m_blurRenderTarget, m_postProcessRenderTarget, dstBuffer,
                        m_gameWorld,
                        m_luminanceCalculationChain.AdaptedLuminance,
                        m_depthBuffer,
                        Batch);

                    // Echange les buffers
                    var tmp = dstBuffer;
                    dstBuffer = srcBuffer;
                    srcBuffer = tmp;
                }


                // Dessine le back buffer
                Device.SetRenderTarget(null);
                Device.Clear(new Color(2, 22, 47, 0));
                Device.BlendState = BlendState.NonPremultiplied;
                Batch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone);
                Batch.Draw(srcBuffer, Viewport, Color.White);
                Batch.End();
                // ---- TEST
                /*Device.SetRenderTarget(null);
                Batch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone);
                Batch.Draw(m_luminanceCalculationChain.AdaptedLuminance, new Rectangle(60, 60, 100, 100), Color.White);
                for (int i = 0; i < m_luminanceCalculationChain.MipChain.Length; i++)
                {
                    Batch.Draw(m_luminanceCalculationChain.MipChain[i], new Rectangle(200, 0 + (i * 50), 50, 50), Color.White); 
                }
                Batch.End();*/
                /*Device.SetRenderTarget(null);
                Batch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone);
                Batch.Draw(m_depthBuffer, new Rectangle(400, 0, 100, 100), Color.White);
                Batch.End();*/
                // Dessine les derniers élements de la scène non soumis au post process comme le HUD et le lens flare.
                //DrawLensFlare(time);

                // Dessine les informations de debug
                DrawDebug(time);
                // Dessine le HUD.
                DrawHUD(time);

                /*Batch.Begin();
                Batch.Draw(m_terrainNormalMaps[m_season], new Rectangle(300, 300, 100, 100), Color.White);
                Batch.End();*/

                Point s = new Point(320, 240);
                //Batch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
                //Batch.Draw(m_gameWorld.ShadowMapRenderTarget, new Rectangle(Device.ScissorRectangle.Width - s.X, Device.ScissorRectangle.Height - s.Y, s.X, s.Y), Color.White);
                //Batch.Draw(m_reflectionMapRenderTarget, new Rectangle(Device.ScissorRectangle.Width - s.X*2, Device.ScissorRectangle.Height - s.Y, s.X, s.Y), Color.White);
                //Batch.End();
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

                lock (Game1.GraphicsDeviceMutex)
                {
                    System.Threading.Thread.Sleep(10);
                }
                System.Threading.Thread.Sleep(10);
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
            float speed = 10 * 4;
            m_gameWorld.Camera.MoveForward(Input.GetLeftStickState().Y * delta * speed);
            //m_gameWorld.Camera.RotateSide(-Input.GetLeftStickState().X * delta * speed / 20.0f);

            m_gameWorld.Camera.RotateRoll(-Input.GetRightStickState().X * 0.25f);
            m_gameWorld.Camera.RotateSide(-Input.GetRightStickState().X * delta * angleSpeed);
            m_gameWorld.Camera.RotateUpDown(-Input.GetRightStickState().Y * angleSpeed * delta);
        }

        /// <summary>
        /// Vérifie l'appui des touches permettant de se déplacer au clavier.
        /// </summary>
        /// <param name="time"></param>
        void UpdateKeyboardMovementInput(GameTime time)
        {
            float delta = Math.Min((float)time.ElapsedGameTime.TotalSeconds, 0.1f);
            float speed = 5;

            if (Input.IsPressed(Keys.LeftShift))
                speed *= 20;
            if (Input.IsPressed(Keys.Space))
                speed *= 5;
            if (Input.IsPressed(Keys.Z))
                m_gameWorld.Camera.MoveForward(delta * speed);
            else if (Input.IsPressed(Keys.S))
                m_gameWorld.Camera.MoveForward(-delta * speed);
            if (Input.IsPressed(Keys.Q))
                m_gameWorld.Camera.MoveSide(delta * speed);
            //m_gameWorld.Camera.MoveSide(delta * speed);
            else if (Input.IsPressed(Keys.D))
                m_gameWorld.Camera.MoveSide(-delta * speed);
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
        /// Initialise les composants du HUD concernant les paramètres graphiques.
        /// </summary>
        /// <returns></returns>
        Dictionary<string, World.HUD.HUDComponent> InitializeHUDGraphicsComponents()
        {
            // Bloom
            World.HUD.HUDCheckbox bloomCb = new World.HUD.HUDCheckbox();
            bloomCb.Text = "BloomEnabled";
            bloomCb.Position = new Vector2(10, 30);
            bloomCb.Checked = m_gameWorld.GraphicalParameters.BloomEnabled;
            bloomCb.ValueChanged += delegate(bool newValue)
            {
                m_gameWorld.GraphicalParameters.BloomEnabled = newValue;
            };

            // Bloom power
            World.HUD.HUDTrackbar bloomPowerCb = new World.HUD.HUDTrackbar();
            bloomPowerCb.Text = "BloomPower";
            bloomPowerCb.Value = m_bloomEffect.BloomPower;
            bloomPowerCb.Position = bloomCb.Position + new Vector2(25, 25);
            bloomPowerCb.MinValue = 0;
            bloomPowerCb.MaxValue = 3;
            bloomPowerCb.ValueChanged += delegate(float newValue)
            {
                m_bloomEffect.BloomPower = newValue;
            };

            // Bloom radius
            World.HUD.HUDTrackbarInt bloomRadiusCb = new World.HUD.HUDTrackbarInt();
            bloomRadiusCb.Text = "BloomRadius";
            bloomRadiusCb.Value = m_bloomEffect.BloomRadius;
            bloomRadiusCb.Position = bloomPowerCb.Position + new Vector2(0, 25);
            bloomRadiusCb.MinValue = 1;
            bloomRadiusCb.MaxValue = 30;
            bloomRadiusCb.ValueChanged += delegate(int newValue)
            {
                m_bloomEffect.BloomRadius = newValue;
            };
            // Bloom amount
            World.HUD.HUDTrackbar bloomAmount = new World.HUD.HUDTrackbar();
            bloomAmount.Text = "BloomAmount";
            bloomAmount.Value = m_bloomEffect.BloomAmount;
            bloomAmount.Position = bloomPowerCb.Position + new Vector2(0, 50);
            bloomAmount.MinValue = 1;
            bloomAmount.MaxValue = 10;
            bloomAmount.ValueChanged += delegate(float newValue)
            {
                m_bloomEffect.BloomAmount = newValue;
            };

            // Bloom threshold
            World.HUD.HUDTrackbar bloomThreshTb = new World.HUD.HUDTrackbar();
            bloomThreshTb.Text = "BloomThreshold";
            bloomThreshTb.Value = m_bloomEffect.BloomEffectThreshold;
            bloomThreshTb.Position = bloomPowerCb.Position + new Vector2(0, 75);
            bloomThreshTb.MinValue = 0;
            bloomThreshTb.MaxValue = 1;
            bloomThreshTb.ValueChanged += delegate(float newValue)
            {
                m_bloomEffect.BloomEffectThreshold = newValue;
            };

            // Bloom threshold
            World.HUD.HUDTrackbar bloomMaxLuminanceTb = new World.HUD.HUDTrackbar();
            bloomMaxLuminanceTb.Text = "Max Luminance";
            bloomMaxLuminanceTb.Value = m_bloomEffect.MaxLuminance;
            bloomMaxLuminanceTb.Position = bloomThreshTb.Position + new Vector2(0, 25);
            bloomMaxLuminanceTb.MinValue = 0.50f;
            bloomMaxLuminanceTb.MaxValue = 256;
            bloomMaxLuminanceTb.ValueChanged += delegate(float newValue)
            {
                m_bloomEffect.MaxLuminance = newValue;
            };
            // -- LANDSCAPE BLUR
            // Activation / désactivation
            World.HUD.HUDCheckbox landscapeBlur = new World.HUD.HUDCheckbox();
            landscapeBlur.Text = "DepthOfField";
            landscapeBlur.Position = new Vector2(bloomCb.Position.X, bloomMaxLuminanceTb.Position.Y + 25);
            landscapeBlur.Checked = m_gameWorld.GraphicalParameters.BlurLandscape;
            landscapeBlur.ValueChanged += delegate(bool newValue)
            {
                m_gameWorld.GraphicalParameters.BlurLandscape = newValue;
            };

            // Power
            World.HUD.HUDTrackbar focusPowerTb = new World.HUD.HUDTrackbar();
            focusPowerTb.Text = "Focus Power";
            focusPowerTb.Value = m_gameWorld.GraphicalParameters.FocusPower;
            focusPowerTb.Position = landscapeBlur.Position + new Vector2(25, 25);
            focusPowerTb.MinValue = 0;
            focusPowerTb.MaxValue = 4;
            focusPowerTb.ValueChanged += delegate(float newValue)
            {
                m_gameWorld.GraphicalParameters.FocusPower = newValue;
            };

            // Depth
            World.HUD.HUDTrackbar focusDepthTb = new World.HUD.HUDTrackbar();
            focusDepthTb.Text = "Focus Depth";
            focusDepthTb.Value = m_gameWorld.GraphicalParameters.FocusDepth;
            focusDepthTb.Position = focusPowerTb.Position + new Vector2(0, 25);
            focusDepthTb.MinValue = -1;
            focusDepthTb.MaxValue = 1;
            focusDepthTb.ValueChanged += delegate(float newValue)
            {
                m_gameWorld.GraphicalParameters.FocusDepth = newValue;
            };

            // Radius
            World.HUD.HUDTrackbarInt focusBlurRadiusTb = new World.HUD.HUDTrackbarInt();
            focusBlurRadiusTb.Text = "BlurRadius";
            focusBlurRadiusTb.Value = m_depthOfFieldEffect.BlurRadius;
            focusBlurRadiusTb.Position = focusDepthTb.Position + new Vector2(0, 25);
            focusBlurRadiusTb.MinValue = 1;
            focusBlurRadiusTb.MaxValue = 30;
            focusBlurRadiusTb.ValueChanged += delegate(int newValue)
            {
                m_depthOfFieldEffect.BlurRadius = newValue;
            };
            // Power
            World.HUD.HUDTrackbar focusBlurPowerTb = new World.HUD.HUDTrackbar();
            focusBlurPowerTb.Text = "BlurPower";
            focusBlurPowerTb.Value = m_depthOfFieldEffect.BlurPower;
            focusBlurPowerTb.Position = focusBlurRadiusTb.Position + new Vector2(0, 25);
            focusBlurPowerTb.MinValue = 0;
            focusBlurPowerTb.MaxValue = 50;
            focusBlurPowerTb.ValueChanged += delegate(float newValue)
            {
                m_depthOfFieldEffect.BlurPower = newValue;
            };

            // Amount
            World.HUD.HUDTrackbar focusBlurAmountTb = new World.HUD.HUDTrackbar();
            focusBlurAmountTb.Text = "BlurAmount";
            focusBlurAmountTb.Value = m_depthOfFieldEffect.BlurAmount;
            focusBlurAmountTb.Position = focusBlurPowerTb.Position + new Vector2(0, 25);
            focusBlurAmountTb.MinValue = 1;
            focusBlurAmountTb.MaxValue = 10;
            focusBlurAmountTb.ValueChanged += delegate(float newValue)
            {
                m_depthOfFieldEffect.BlurAmount = newValue;
            };

            // -- DRAWING
            // Grass
            World.HUD.HUDCheckbox drawGrass = new World.HUD.HUDCheckbox();
            drawGrass.Text = "Grass";
            drawGrass.Position = new Vector2(bloomCb.Position.X, focusBlurAmountTb.Position.Y + 25);
            drawGrass.Checked = m_gameWorld.GraphicalParameters.DrawGrass;
            drawGrass.ValueChanged += delegate(bool newValue)
            {
                m_gameWorld.GraphicalParameters.DrawGrass = newValue;
                m_gameWorld.GraphicalParameters.ReflectGrass = m_gameWorld.GraphicalParameters.DrawGrass & m_gameWorld.GraphicalParameters.ReflectGrass;
            };
            World.HUD.HUDCheckbox reflectGrass = new World.HUD.HUDCheckbox();
            reflectGrass.Text = "Reflect";
            reflectGrass.Position = new Vector2(bloomCb.Position.X + 200, drawGrass.Position.Y);
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
            reflectTrees.Position = new Vector2(bloomCb.Position.X + 200, drawTrees.Position.Y);
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


            Dictionary<string, World.HUD.HUDComponent> graphicalComponents = new Dictionary<string, World.HUD.HUDComponent>();
            graphicalComponents.Add("Bloom", bloomCb);
            graphicalComponents.Add("BloomAmount", bloomAmount);
            graphicalComponents.Add("BloomPower", bloomPowerCb);
            graphicalComponents.Add("BloomRadius", bloomRadiusCb);
            graphicalComponents.Add("BloomThreshold", bloomThreshTb);
            graphicalComponents.Add("BloomMaxLuminance", bloomMaxLuminanceTb);
            graphicalComponents.Add("DepthOfField", landscapeBlur);
            graphicalComponents.Add("BlurRadius", focusBlurRadiusTb);
            graphicalComponents.Add("BlurAmount", focusBlurAmountTb);
            graphicalComponents.Add("BlurPower", focusBlurPowerTb);
            graphicalComponents.Add("FocusDepth", focusDepthTb);
            graphicalComponents.Add("FocusPower", focusPowerTb);
            graphicalComponents.Add("DrawGrass", drawGrass);
            graphicalComponents.Add("ReflectGrass", reflectGrass);
            graphicalComponents.Add("DrawTrees", drawTrees);
            graphicalComponents.Add("ReflectTrees", reflectTrees);
            graphicalComponents.Add("HQLandscape", hqLandscape);
            return graphicalComponents;
        }

        /// <summary>
        /// Initialise le HUD.
        /// </summary>
        void InitializeHUD()
        {
            // Ajout des composants
            World.HUD.HUDPanelManager mgr = new World.HUD.HUDPanelManager();
            m_hud.Components.Add("TabManager", mgr);
            
            
            // Composants graphiques
            var graphicalComponents = InitializeHUDGraphicsComponents();

            mgr.Components.Add("Graphics", graphicalComponents);
            mgr.CurrentPage = "Graphics";
        }
        /// <summary>
        /// Méthode appelé lorsque la gui est montrée / cachée.
        /// </summary>
        /// <param name="time"></param>
        void OnToggleGui(GameTime time)
        {
            // Recentre la souris.
            int centerX = Game1.Instance.Window.ClientBounds.Width / 2;
            int centerY = Game1.Instance.Window.ClientBounds.Height / 2;
            Mouse.SetPosition(centerX, centerY);

            // Recalcule le viewport
            if (!m_mouseControlEnabled)
                //Viewport = new Rectangle(Game1.Instance.ResolutionWidth / 3, 25, Game1.Instance.ResolutionWidth * 2 / 3, Game1.Instance.ResolutionHeight - 25);
                Viewport = new Rectangle(0, 0, Game1.Instance.ResolutionWidth, Game1.Instance.ResolutionHeight);
            else
                Viewport = new Rectangle(0, 0, Game1.Instance.ResolutionWidth, Game1.Instance.ResolutionHeight);
        }
        /// <summary>
        /// Mets à jour tout ce qui est lié à l'appui de touches sur le clavier, et aux actions qui en résultent.
        /// </summary>
        protected override void UpdateInput(GameTime time)
        {
            // Attracteur du vent.
            m_windAttractor.NextStep(0.8f*(float)time.ElapsedGameTime.TotalSeconds, 1000);

            // Caméra souris
            if(m_mouseControlEnabled)
                UpdateMouseCamera(time);

            // Mouvement 
            UpdateKeyboardMovementInput(time);
            UpdateXBOXInput(time);

            // Paramètres graphiques
            UpdateGraphicalParametersInput(time);

            // Affichage du HUD.
            if (Input.IsTrigger(Keys.LeftAlt))
            {
                m_mouseControlEnabled = !m_mouseControlEnabled;
                OnToggleGui(time);
            }

            if (Input.IsPressed(Keys.NumPad5))
                m_gameWorld.Camera.Position = Vector3.Zero;

            if (Input.IsTrigger(Keys.G) && m_isLoadingComplete)
                RegenerateTerrain();

            UpdateFractalParametersInput(time);
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