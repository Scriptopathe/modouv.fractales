using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Test3D.World;
using Test3D.World.Objects;
using Test3D.Generation;
using Test3D.World.Objects.Specialized;
namespace Test3D.Scenes
{
    /// <summary>
    /// Scene de test du générateur de fractales.
    /// </summary>
    public class SceneTestGenerator : Scene
    {
        /* ----------------------------------------------------------------------------
         * Variables
         * --------------------------------------------------------------------------*/
        #region Variables
        /// <summary>
        /// Contient le nombre de frames ayant été dessinées jusqu'à présent.
        /// </summary>
        protected int frameCounter = 0;
        /// <summary>
        /// Paramètre "c" de la première fractale dessinée sur le terrain, 
        /// mappée sur les coordonnées de teXture.
        /// </summary>
        protected MathHelpers.Complex m_c1 = new MathHelpers.Complex(-0.820f, 0.3030f);
        /// <summary>
        /// Paramètre de "c" de la deuXième fractale dessinée sur le terrain, mappée sur
        /// les normales du mesh.
        /// </summary>
        protected MathHelpers.Complex m_c2 = new MathHelpers.Complex(-0.835f, 0.3045f);
        /// <summary>
        /// Générateur de nombres aléatoires.
        /// </summary>
        protected Random m_rand = new Random();
        /// <summary>
        /// Texture du ciel
        /// </summary>
        protected Texture2D m_skyTexture;
        /// <summary>
        /// Référence vers le GameWorld.
        /// </summary>
        protected GameWorld m_gameWorld;
        /// <summary>
        /// Paysage.
        /// </summary>
        protected Landscape m_landscape;
        /// <summary>
        /// Eau.
        /// </summary>
        protected WaterObject m_water;
        /// <summary>
        /// Population d'arbres.
        /// </summary>
        protected ObjectCullingGroup m_population;
        /// <summary>
        /// Sauvegarde de l'état du rasterizer.
        /// </summary>
        protected RasterizerState _rasterizerState;
        /// <summary>
        /// Shader pour le dessin de la montagne.
        /// </summary>
        protected Effect m_terrainShader;
        /// <summary>
        /// Shader utilisé pour le cube.
        /// </summary>
        protected Effect m_treesShader;
        /// <summary>
        /// Composant dessinant les rayons de soleil.
        /// </summary>
        protected World.LensFlareComponent m_lensFlare;

        #region DEBUG
        /// <summary>
        /// Bounding frustum pouvant être affiché.
        /// </summary>
        BoundingFrustum m_debugFrustum;
        #endregion
        #endregion


        /* ----------------------------------------------------------------------------
         * Initialisation
         * --------------------------------------------------------------------------*/
        #region Initialisation

        /// <summary>
        /// Crée une nouvelle instance de SceneTestJulia.
        /// </summary>
        public SceneTestGenerator()
        {
            // Rayon de soleil
            m_lensFlare = new World.LensFlareComponent(Game1.Instance);
            Game1.Instance.Components.Add(m_lensFlare);
        }

        /// <summary>
        /// Initialise la scène.
        /// </summary>
        public override void Initialize()
        {
            Initialize3DGraphics();

            // Positionne la souris au centre de l'écran.
            int centerX = Game1.Instance.Window.ClientBounds.Width / 2;
            int centerY = Game1.Instance.Window.ClientBounds.Height / 2;
            Mouse.SetPosition(centerX, centerY);
        }

        /// <summary>
        /// Initialise les éléments 3D.
        /// </summary>
        protected virtual void Initialize3DGraphics()
        {
            m_gameWorld = new Test3D.World.GameWorld();
            m_gameWorld.Camera.Position = new Vector3(0, 0, -25);//new Vector3(-100, -20, 20);
            
            // Etat du rasterizer
            _rasterizerState = new RasterizerState();
            _rasterizerState.CullMode = CullMode.CullCounterClockwiseFace;
            _rasterizerState.FillMode = FillMode.Solid;
            
            // Chargement de la texture du ciel.
            m_skyTexture = Game1.Instance.Content.Load<Texture2D>("textures\\sky");
            Texture2D treeTexture = Game1.Instance.Content.Load<Texture2D>("textures\\tree");
            // Chargement du modèle de cube.
            Model model = Content.Load<Model>("Models\\tree");
            
            World.ModelData cubeModel = new World.ModelData(model.Meshes.First().MeshParts.First().VertexBuffer,
                                                    model.Meshes.First().MeshParts.First().IndexBuffer);

            // Crée et paramètre le shader utilisé pour dessiner le terrain.
            m_terrainShader = Game1.Instance.Content.Load<Effect>("Shaders\\mountain");
            //m_terrainShader.Parameters["MountainTexture"].SetValue(Content.Load<Texture2D>("textures\\montagne-atlas"));

            // Crée et paramètre le shader utilisé pour dessiner le cube.
            m_treesShader = Content.Load<Effect>("Shaders\\basic");
            m_treesShader.Parameters["TreeTexture"].SetValue(treeTexture);

            // Création de l'eau
            m_water = new WaterObject(m_gameWorld.GraphicalParameters);
            
            // Test de l'instanciation statique.
            Transform[] transforms = new Transform[1];
            m_cubeStatic = new StaticInstancedObjects(cubeModel, transforms, m_treesShader);
            m_cubeStatic.CullingEnabled = true;
            m_cubeStatic.InstanceCullingEnabled = false;
            m_cubeStatic.DebugView = true;

            // Création d'un objet 3D à dessiner.
            m_landscape = new Landscape();
            m_landscape.Shader = m_terrainShader;
            m_landscape.Position = new Vector3(-128, -128, 20);
            m_landscape.HScale = 0.25f;
            m_landscape.VScale = -0.5f;
            RegenerateTerrain();

            
            // Crée le cube
            m_cubeTest = new DynamicInstancedObjects(cubeModel, m_treesShader, 1);
            m_cubeTest.DebugView = false;
            m_cubeTest.Shader = m_treesShader;
        }


        /// <summary>
        /// Régénère le terrain.
        /// </summary>
        protected virtual void RegenerateTerrain()
        {
            // Création d'un objet 3D à dessiner.
            float[,] machin = Generation.Models.DiamondSquareAlgorithm.GenerateHeightmap(2049, 2049,
                (m_rand.Next(100)-150)/10.0f,
                (m_rand.Next(100)-170)/10.0f, 
                (m_rand.Next(200)-250)/10.0f,
                (m_rand.Next(200)-180)/10.0f);
            m_landscape.Heightmap = machin;

            // -- Regénération des cubes statiques
            Transform[] transforms = new Transform[1000];
            int divide = 2;
            for (int i = 0; i < transforms.Count(); i++)
            {
                float piover2 = MathHelper.PiOver2;
                transforms[i] = new Transform();
                transforms[i].Position = m_landscape.GetVerticePosition(
                    m_rand.Next(m_landscape.Heightmap.GetLength(0)/divide),
                    m_rand.Next(m_landscape.Heightmap.GetLength(1)/divide));
                transforms[i].Position += new Vector3(m_rand.Next(100) / 100.0f);
                transforms[i].Scale = new Vector3(1, 1, 1) * (0.05f+m_rand.Next(500)/1000.0f);//m_rand.Next(500)/100.0f;
                transforms[i].Rotation = new Vector3(-piover2, 0, 0);
            }
            m_cubeStatic.Transforms = transforms;

            // Regénération du paysage de Populator
            m_population = (ObjectCullingGroup)Generation.Populations.PalmTreePopulation.Generate(m_landscape, m_cubeStatic.Model, m_treesShader);

            // Génération des fougères de FernPopulator
            m_fernPopulation = Generation.Populations.FernPopulation.Generate(m_landscape);
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
        void DrawDebug(GameTime gameTime)
        {

            // Dessine des infos de debug
            Batch.Begin();
            Batch.DrawString(Font, "Position = [" + m_gameWorld.Camera.Position.X.ToString().PadLeft(6) + " ; " +
                                    m_gameWorld.Camera.Position.Y.ToString().PadLeft(6) + " ; " +
                                    m_gameWorld.Camera.Position.Z.ToString().PadLeft(6) + "]",
                new Vector2(0, 0),
                Color.White);
            Batch.DrawString(Font, "Framerate : " + ((int)(1000 / gameTime.ElapsedGameTime.TotalMilliseconds)).ToString().PadLeft(4),
                new Vector2(0, 25), Color.White);
            Batch.DrawString(Font, "C1 = " + m_c1.Real.ToString().PadLeft(8, ' ') + " + " + m_c1.Imaginary.ToString().PadLeft(8, ' ') + "i", new Vector2(0, 50), Color.White);
            Batch.DrawString(Font, "C2 = " + m_c2.Real.ToString().PadLeft(8, ' ') + " + " + m_c2.Imaginary.ToString().PadLeft(8, ' ') + "i", new Vector2(0, 75), Color.White);
            Batch.End();
            /*
            Batch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
            Batch.Draw(m_water.ReflectionMapRenderTarget, new Rectangle(Game1.Instance.ScreenWidth - 320, 0, 320, 160), Color.White);
            Batch.Draw(m_gameWorld.ShadowMapRenderTarget, new Rectangle(Game1.Instance.ScreenWidth - 320, 160, 320, 160), Color.White);
            Batch.End();*/
        }
        /// <summary>
        /// Dessine le terrain.
        /// </summary>
        void DrawLandscape(GameTime time, string technique)
        {
            // Dessine le terrain.
            m_landscape.Shader.CurrentTechnique = m_landscape.Shader.Techniques[technique];
            m_landscape.Shader.Parameters["mC1"].SetValue(new Vector2(m_c1.Real, m_c1.Imaginary));
            m_landscape.Shader.Parameters["mC2"].SetValue(new Vector2(m_c2.Real, m_c2.Imaginary));

            if (technique != "Landscape" && technique != "Shadowed")
                // Pour la réflection, on dessine une version en basse qualité.
                m_landscape.Draw(m_gameWorld, ModelMipmap.ModelQuality.Low);
            else
                m_landscape.Draw(m_gameWorld);
        }
        /// <summary>
        /// Dessine l'eau
        /// </summary>
        void DrawWater(GameTime time)
        {
            m_water.Update(time);
            m_water.Draw(m_gameWorld);
        }

        /// <summary>
        /// Dessine les arbres.
        /// </summary>
        /// <param name="time"></param>
        void DrawTrees(GameTime time)
        {
            // Dessine de la flore créée par ObjectPopulator.
            m_gameWorld.SetRenderDistance(m_gameWorld.GraphicalParameters.TreesRenderDistance);
            m_population.Draw(m_gameWorld);
            m_gameWorld.RestoreRenderDistance();
        }
        #endregion

        /* ----------------------------------------------------------------------------
         * -- Scene Elements
         * --------------------------------------------------------------------------*/
        #region Scene
        /// <summary>
        /// Dessine le modèle 3D avec comme texture la fractale.
        /// </summary>
        protected void Draw3DObjects(GameTime time)
        {
            UpdateRasterizerState();

            // Dessin du terrain.
            if (m_gameWorld.GraphicalParameters.DrawLandscapeShadows)
                DrawLandscape(time, "Shadowed");
            else
                DrawLandscape(time, "Landscape");

            // Dessine l'eau
            DrawWater(time);

            // Dessine les arbres
            if(m_gameWorld.GraphicalParameters.DrawTrees)
                DrawTrees(time);

            // Debug
            if (Input.IsPressed(Microsoft.Xna.Framework.Input.Keys.F))
                m_debugFrustum = new BoundingFrustum(m_gameWorld.View * m_gameWorld.Projection);
            if (m_debugFrustum != null)
                Debug.Renderers.BoundingFrustumRenderer.Render(m_debugFrustum, Game1.Instance.GraphicsDevice, m_gameWorld.View, m_gameWorld.Projection, Color.Green);
        }

        /// <summary>
        /// Dessine l'arrière plan.
        /// </summary>
        /// <param name="time"></param>
        protected virtual void DrawBackground(GameTime time, Rectangle bounds, int alpha=255)
        {
            int lum = (int)((Math.Abs(m_gameWorld.Hour-12) / 8.0f)*255);
            Batch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
            Batch.Draw(m_skyTexture, bounds, new Color(lum, lum, lum, alpha));
            Batch.End();
        }
        /// <summary>
        /// Dessine la shadow map.
        /// </summary>
        /// <param name="time"></param>
        protected void DrawShadowMap(GameTime time)
        {
            // Crée la matrice View de la shadow map correspondant à la vue depuis le "soleil".
            var position = new Vector3(m_gameWorld.LightPosition.X, m_gameWorld.LightPosition.Y, m_gameWorld.LightPosition.Z);
            var lightViewMatrix = Matrix.CreateLookAt(position, Vector3.Zero, -Vector3.UnitY);

            // Prépare le render target
            Device.SetRenderTarget(m_gameWorld.ShadowMapRenderTarget);
            Device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);
            m_gameWorld.View = lightViewMatrix;
            m_gameWorld.LightView = lightViewMatrix;
            float oldNearPlane = m_gameWorld.NearPlane;
            m_gameWorld.NearPlane = 5;
            m_gameWorld.SetRenderDistance(2000);
            // Prépare le rasterizer.
            RasterizerState state = new RasterizerState();
            state.CullMode = CullMode.None;
            state.FillMode = FillMode.Solid;
            Device.RasterizerState = state;

            DepthStencilState dstate = new DepthStencilState();
            dstate.DepthBufferFunction = CompareFunction.LessEqual;
            Device.DepthStencilState = dstate;

            // Dessine la shadow map du paysage
            m_gameWorld.GraphicalParameters.DrawLandscapeShadows = false; // désactivation temporaire de l'utilisation des ombres.
            DrawLandscape(time, "ShadowMap");
            m_gameWorld.GraphicalParameters.DrawLandscapeShadows = true;

            // Restore les paramètres par défaut.
            m_gameWorld.RestoreCameraView();
            m_gameWorld.NearPlane = oldNearPlane;
            m_gameWorld.RestoreRenderDistance();
            Device.SetRenderTarget(null);
            Device.DepthStencilState = DepthStencilState.Default;
            Device.RasterizerState = _rasterizerState;
        }
        /// <summary>
        /// Dessine la map de réflexion dans une texture temporaire.
        /// </summary>
        protected void DrawReflectionMap(GameTime time)
        {

            // Crée la matrice "View" de la réflection. Typiquement la caméra à l'envers.
            Vector3 reflCameraPosition = m_gameWorld.Camera.Position;
            reflCameraPosition.Z = -m_gameWorld.Camera.Position.Z+0.05f;
            Vector3 reflTargetPos = m_gameWorld.Camera.Front + m_gameWorld.Camera.Position;
            reflTargetPos.Z = -(m_gameWorld.Camera.Front.Z + m_gameWorld.Camera.Position.Z)+0.05f;
            Vector3 invUpVector = -m_gameWorld.Camera.Up;
            var reflectionViewMatrix = Matrix.CreateLookAt(reflCameraPosition, reflTargetPos, invUpVector);
            m_water.Shader.Parameters["wWorldReflectionViewProjection"].SetValue(reflectionViewMatrix * m_gameWorld.Projection);
            GraphicsDevice device = Game1.Instance.GraphicsDevice;
            
            // Dessine la scène réflechie sur le RenderTarget temporaire.
            device.SetRenderTarget(m_water.ReflectionMapRenderTarget);
            device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.DarkCyan, 1.0f, 0);
            m_gameWorld.View = reflectionViewMatrix;

            // Dessine l'arrière plan.
            DrawBackground(time, m_water.ReflectionMapRenderTarget.Bounds, 200); 

            // Restore le blend state pour qu'il gère la transparence.
            device.BlendState = BlendState.NonPremultiplied;
            RasterizerState state = new RasterizerState();
            state.CullMode = CullMode.None;
            state.FillMode = FillMode.Solid;
            device.RasterizerState = state;

            DepthStencilState dstate = new DepthStencilState();
            dstate.DepthBufferFunction = CompareFunction.LessEqual;
            device.DepthStencilState = dstate;

            // Dessine les éléments à réfléchir.
            if(m_gameWorld.GraphicalParameters.ReflectLandscape)
                DrawLandscape(time, "Reflection");
            if(m_gameWorld.GraphicalParameters.ReflectTrees)
                DrawTrees(time);

            // Restore les paramètres précédents du monde
            m_gameWorld.RestoreCameraView();


            // Restore les paramètres du device.
            device.SetRenderTarget(null);
            device.RasterizerState = _rasterizerState;
            device.DepthStencilState = DepthStencilState.Default;
        }

        /// <summary>
        /// Dessinne tous les élements du jeu sans post process
        /// </summary>
        protected void DrawScene(GameTime gameTime)
        {
            // Dessine l'arrière plan.
            DrawBackground(gameTime, Device.ScissorRectangle, 255);

            // Dessine les modèles 3D
            Draw3DObjects(gameTime);

            // Mets à jour les matrices utilisées par le lens flare.
            m_lensFlare.View = m_gameWorld.View;
            m_lensFlare.Projection = m_gameWorld.Projection;
            m_lensFlare.LightDirection = Vector3.Normalize(-m_gameWorld.LightDirection);
            m_lensFlare.DrawGlow();
            m_lensFlare.DrawFlares();

            DrawDebug(gameTime);
        }
        /// <summary>
        /// Dessine la scène.
        /// </summary>
        /// <param name="gameTime"></param>
        public virtual void Draw(GameTime time)
        {
            frameCounter++;

            GraphicsDevice gfx = Game1.Instance.GraphicsDevice;
            gfx.SetRenderTarget(null);
            
            // Dessine la réflection map pour le dessin de l'eau.
            DrawReflectionMap(time);

            // Dessine la Shadow Map utilisée pour les ombres dynamiques
            if(m_gameWorld.GraphicalParameters.DrawLandscapeShadows)
                DrawShadowMap(time);
            
            // Dessine la scène entière.
            DrawScene(time);

            m_lensFlare.UpdateOcclusion();
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

            if (Input.IsPressed(Keys.NumPad5))
                m_gameWorld.Camera.Position = Vector3.Zero;
            if (Input.IsTrigger(Keys.G))
                RegenerateTerrain();


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
        #endregion
        /// <summary>
        /// Mets à jour la scène.
        /// </summary>
        /// <param name="time"></param>
        public virtual void Update(GameTime time)
        {
            UpdateInput(time);
            m_gameWorld.Update(time);
        }

        #region DEBUG
        /// <summary>
        /// Mets à jour les paramètres 3D du rasterizer de la carte.
        /// </summary>
        void UpdateRasterizerState()
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
