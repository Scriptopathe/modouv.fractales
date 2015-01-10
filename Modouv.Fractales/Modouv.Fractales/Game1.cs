using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Modouv.Fractales
{
    public enum Profile
    {
        VeryHigh,
        High,
        Medium,
        MediumDebug,
        Low,
        VeryLow,
        UltraLow,
        Custom,
    }

    public enum Mode
    {
        TreeDemo,
        WorldDemo,
        Custom,
    }
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {

        /// <summary>
        /// Instance Singleton de cette classe.
        /// </summary>
        public static Game1 Instance;
        public static Mode Mode;
        public static Debug.Profiling.Recorder Recorder = new Debug.Profiling.Recorder();
        /// <summary>
        /// Mutex à utiliser lorsque la carte graphique est susceptible d'être utilisé par deux threads.
        /// </summary>
        public static object GraphicsDeviceMutex = new object();
        
        #region Variables
        GraphicsDeviceManager m_graphics;
        SpriteBatch m_spriteBatch;
        Effect m_effect;
        SpriteFont m_font;
        SpriteFont m_smallFont;
        Scenes.Scene m_scene;
        #endregion

        #region Properties
        /// <summary>
        /// Retourne la largeur en pixels de l'écran.
        /// </summary>
        public int ResolutionWidth
        {
            get;
            set;
        }
        /// <summary>
        /// Retourne la hauteur en pixels de l'écran.
        /// </summary>
        public int ResolutionHeight
        {
            get;
            set;
        }
        /// <summary>
        /// Obtient le sprite batch utilisé pour le rendu de textures 2D.
        /// </summary>
        public SpriteBatch Batch
        {
            get { return m_spriteBatch; }
        }
        /// <summary>
        /// Obtient une reférence vers le GraphicsDeviceManager.
        /// </summary>
        public GraphicsDeviceManager Graphics
        {
            get { return m_graphics; }
        }
        /// <summary>
        /// Obtient une référence vers la police utilisée pour dessiner des chaines de caractère.
        /// </summary>
        public SpriteFont Font
        {
            get { return m_font; }
        }
        /// <summary>
        /// Obtient une référence vers la police utilisée pour dessiner des chaines de caractère.
        /// Version + petite.
        /// </summary>
        public SpriteFont SmallFont
        {
            get { return m_smallFont; }
        }
        /// <summary>
        /// Matrice de transformation pour les dessins 2D.
        /// </summary>
        public Matrix PlaneTransform2D
        {
            get;
            set;
        }
        /// <summary>
        /// Texture servant à être passée en paramètres dans les appels à SpriteBatch.Draw() quand le pixel shader
        /// n'utilise pas la texture pour dessiner.
        /// </summary>
        public Texture2D DummyTexture
        {
            get;
            protected set;
        }
        /// <summary>
        /// Retourne la scene actuellement en cours d'exécution.
        /// </summary>
        public Scenes.Scene Scene
        {
            get { return m_scene; }
        }
        World.GraphicalParameters _parameters;
        public World.GraphicalParameters GraphicalParameters
        {
            get { return _parameters; }
            set { _parameters = value; }
        }
        /// <summary>
        /// Obtient une référence vers la WinForm de l'application.
        /// </summary>
        public System.Windows.Forms.Form Form
        {
            get;
            protected set;
        }
        #endregion

        /// <summary>
        /// Initialise le jeu.
        /// </summary>
        public Game1()
        {
            Instance = this;
            
            // Sélection du mode.
            System.Windows.Forms.Application.EnableVisualStyles();
            Forms.StartForm form = new Forms.StartForm();
            if (form.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                Exit();
                return;
            }
            Mode = form.StartMode;

            // Windows forms
            Form = (System.Windows.Forms.Form)System.Windows.Forms.Form.FromHandle(Game1.Instance.Window.Handle);

            // Paramètres graphiques
            World.GraphicalParameters parameters = new World.GraphicalParameters();
            parameters.Profile = Profile.Medium;
            parameters.IsFullScreen = form.FullScreen;
            parameters.Resolution = new Point((int)form.Resolution.X, (int)form.Resolution.Y);
            parameters.LandscapeResolution = 1024;
            parameters.FarPlane = 1000;

            // Application des paramètres graphiques
            ResolutionWidth = parameters.Resolution.X;
            ResolutionHeight = parameters.Resolution.Y;

            // Préférences diverses
            m_graphics = new GraphicsDeviceManager(this);
            m_graphics.PreferredBackBufferWidth = parameters.Resolution.X;
            m_graphics.PreferredBackBufferHeight = parameters.Resolution.Y;
            m_graphics.SynchronizeWithVerticalRetrace = false;
            m_graphics.PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8;
            m_graphics.PreferMultiSampling = parameters.MultiSamplingEnabled;
            m_graphics.IsFullScreen = parameters.IsFullScreen;
            this.IsFixedTimeStep = false;
            
            Content.RootDirectory = "Content";
            switch (Mode)
            {
                case Fractales.Mode.TreeDemo:
                    m_scene = new Scenes.SceneTree(form.TreeKind);
                    parameters.DrawTrees = true;
                    parameters.BlurLandscape = false;
                    parameters.BloomEnabled = false;
                     
                    break;
                case Fractales.Mode.WorldDemo:
                    m_scene = new Scenes.SceneFantasyWorld();
                    parameters.PreloadInstanceBuffers = true;
                    parameters.DrawTrees = true;
                    parameters.DrawGrass = true;
                    break;
                case Fractales.Mode.Custom:
                    m_scene = new Scenes.SceneFantasyWorld();
                    break;
            }

            _parameters = parameters;
            Input.ModuleInit();
        }

        /// <summary>
        /// Initialise le jeu.
        /// </summary>
        protected override void Initialize()
        {
            m_graphics.GraphicsDevice.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PreserveContents;
            base.Initialize();
        }

        /// <summary>
        /// Charle les ressources que le jeu doit utiliser.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            m_spriteBatch = new SpriteBatch(GraphicsDevice);
            m_effect = Content.Load<Effect>("Shaders\\Test");
            m_font = Content.Load<SpriteFont>("segoe_ui");
            m_smallFont = Content.Load<SpriteFont>("segoe_ui_16");
            // Création de la texture inutile :D
            DummyTexture = new Texture2D(GraphicsDevice, 8, 8);
            Color[] data = new Color[64];
            for (int i = 0; i < 64; i++) { data[i] = new Color(255, 0, 255); }
            DummyTexture.SetData<Color>(data);

            // Chargement des fractales
            Generation.Fractals.Julia.Initialize();
            Generation.Fractals.Mandelbrot.Initialize();
            Generation.Fractals.Repulsor.Initialize();

            // Création de la matrice de vertex shader 2D.
            CreateBasicEffectMatrix();

            m_scene.Initialize();

            // Application des paramètres aux scènes.
            if (m_scene is Scenes.SceneFantasyWorld)
                ((Scenes.SceneFantasyWorld)m_scene).SetGraphicalParameters(_parameters);
            else if (m_scene is Scenes.SceneTestLandscape)
                ((Scenes.SceneTestLandscape)m_scene).SetGraphicalParameters(_parameters);
            else if (m_scene is Scenes.SceneTree)
                ((Scenes.SceneTree)m_scene).SetGraphicalParameters(_parameters);


            base.LoadContent();
        }
        /// <summary>
        /// Crée la matrice utilisée pour compiler les vertex shader des objets 2D en version 3.0.
        /// </summary>
        void CreateBasicEffectMatrix()
        {
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Graphics.PreferredBackBufferWidth, Graphics.PreferredBackBufferHeight, 0, 0, 1);
            Matrix halfPixelOffset = Matrix.CreateTranslation(-0.5f, -0.5f, 0);
            PlaneTransform2D = halfPixelOffset * projection;
        }
        /// <summary>
        /// Décharge toutes les ressources allouées.
        /// </summary>
        protected override void UnloadContent()
        {
            
        }

        /// <summary>
        /// Mise à jour du jeu.
        /// </summary>
        /// <param name="gameTime"></param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (Input.IsPressed(Keys.Escape))
                Exit();
            Input.Update();
            m_scene.Update(gameTime);
            base.Update(gameTime);
        }

        /// <summary>
        /// Draws the scene.
        /// </summary>
        /// <param name="gameTime"></param>
        protected override void Draw(GameTime gameTime)
        {
            m_scene.Draw(gameTime);
            //base.Draw(gameTime);
        }
    }
    
}
