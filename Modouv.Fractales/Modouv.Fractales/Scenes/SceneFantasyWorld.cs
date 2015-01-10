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
    public class SceneFantasyWorld : SceneFractalWorld
    {
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

        /// <summary>
        /// Normale selon laquelle la neige s'étale.
        /// </summary>
        Vector3 m_snowNormal = new Vector3(1, 0.357f, -0.89f);
        /// <summary>
        /// Seuil à partir duquel il va y avoir de la neige.
        /// Plus la valeur est grande, moins il y aura de neige.
        /// </summary>
        float m_snowThreshold = 0.83f;
        /// <summary>
        /// Plus cette valeur est grande, moins la neige sera diffuse.
        /// </summary>
        float m_snowPow = 2.24f;
        /// <summary>
        /// Permet de modifier les paramètres variant en fonction de la saison.
        /// </summary>
        Season m_season;
        
        #endregion

        #region Objets de la scène
        /// <summary>
        /// Chaine de calcul de la luminance adaptée de la scène.
        /// </summary>
        LuminanceCalculationChain m_luminanceCalculationChain;
        /// <summary>
        /// Gestionnaire de particules.
        /// </summary>
        ParticleManager m_particuleManager;
        /// <summary>
        /// Populations d'arbres.
        /// </summary>
        Dictionary<Season, List<ObjectCullingGroup>> m_treePopulations;
        /// <summary>
        /// Population de billboard sprites.
        /// </summary>
        ObjectCullingGroup m_billboardSprites;
        /// <summary>
        /// Population de light sprites.
        /// </summary>
        ObjectCullingGroup m_lightSprites;
        /// <summary>
        /// Shader utilisé pour le dessin des billboard sprites.
        /// </summary>
        Effect m_billboardSpritesShader;
        /// <summary>
        /// Shader pour le dessin de la montagne.
        /// </summary>
        Effect m_terrainShader;
        /// <summary>
        /// Contient les textures à assigner au terrain en fonction de la saison.
        /// </summary>
        Dictionary<Season, Texture2D> m_terrainTextures;
        /// <summary>
        /// Contient les textures des plantes à fonction de la saison.
        /// </summary>
        Dictionary<Season, Texture2D> m_plantsTextures;
        /// <summary>
        /// Normal maps des terrains..
        /// </summary>
        Dictionary<Season, Texture2D> m_terrainNormalMaps;
        /// <summary>
        /// Shader utilisé pour les particules de lumière.
        /// </summary>
        Effect m_lightParticlesShader;
        /// <summary>
        /// Texture du ciel
        /// </summary>
        Texture2D m_skyTexture;
        /// <summary>
        /// Paysage.
        /// </summary>
        Landscape m_landscape;
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
        /// Shader utilisé pour le dessin de la deuxième population d'arbres fractals.
        /// </summary>
        Effect m_fractalTreesShader2;
        /// <summary>
        /// Interface utilisateur.
        /// </summary>
        World.HUD.HUDManager m_hud;
        /// <summary>
        /// Multiplicateur de fréquence de texture pour le terrain.
        /// </summary>
        float m_lanscapeTextureFactor = 25;
        #region Bruits de génération de terrain
        /// <summary>
        /// Bruit servant à la répartition des coefficient affectés aux deux bruits de 
        /// génération de terrain
        /// Une valeur 1 sortie par ce bruit indique que la valeur du bruit "High" est entièrement
        /// utilisée pour la génération d'un point de la heightmap.
        /// Une valeur 0, indique que le bruit "Low" sera complètement utilisé.
        /// </summary>
        Modouv.Fractales.Generation.Noise.NoiseMapGenerator.NoiseParameters m_repartitionNoise;
        /// <summary>
        /// Bruit de génération de terrain "High".
        /// </summary>
        Modouv.Fractales.Generation.Noise.NoiseMapGenerator.NoiseParameters m_noiseHigh;
        /// <summary>
        /// Bruit de génération de terrain "Low".
        /// </summary>
        Modouv.Fractales.Generation.Noise.NoiseMapGenerator.NoiseParameters m_noiseLow;

        Texture2D m_repartitionNoiseTex;
        Texture2D m_noiseHighTex;
        Texture2D m_noiseLowTex;
        Texture2D m_noiseMergedTex;

        /// <summary>
        /// Indique si les textures des bruits doivent être dessinées.
        /// </summary>
        bool m_drawNoiseTextures = false;
        /// <summary>
        /// Indique que la taille du paysage a changé.
        /// </summary>
        bool m_landscapeSizeChanged = true;
        #endregion
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
        bool m_displayFramerate;
        #endregion
        /* ----------------------------------------------------------------------------
         * Properties
         * --------------------------------------------------------------------------*/
        #region Properties
        /// <summary>
        /// Obtient la saison associée au monde.
        /// </summary>
        public Season WorldSeason
        {
            get { return m_season; }
        }
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
        #endregion
        /* ----------------------------------------------------------------------------
         * Initialisation
         * --------------------------------------------------------------------------*/
        #region Initialisation

        /// <summary>
        /// Crée une nouvelle instance de SceneTestJulia.
        /// </summary>
        public SceneFantasyWorld() : base()
        {

        }

        /// <summary>
        /// Permet la création de ressources relatives au paysage.
        /// Cette méthode doit être appelée si l'on souhaite modifier la taille
        /// du paysage.
        /// </summary>
        void OnLandscapeSizeChanged()
        {
            
            int size = m_gameWorld.GraphicalParameters.LandscapeResolution+1;

            // Création / refresh des textures de bruit
            if (m_repartitionNoiseTex != null)
            {
                m_repartitionNoiseTex.Dispose();
                m_noiseHighTex.Dispose();
                m_noiseLowTex.Dispose();
                m_noiseMergedTex.Dispose();
            }
            m_repartitionNoiseTex = new Texture2D(Game1.Instance.GraphicsDevice, size, size);
            m_noiseHighTex = new Texture2D(Game1.Instance.GraphicsDevice, size, size);
            m_noiseLowTex = new Texture2D(Game1.Instance.GraphicsDevice, size, size);
            m_noiseMergedTex = new Texture2D(Game1.Instance.GraphicsDevice, size, size);

            // Repositionnement du paysage.
            m_landscape.Position = new Vector3(-m_gameWorld.GraphicalParameters.LandscapeResolution / 4, -m_gameWorld.GraphicalParameters.LandscapeResolution / 4, 20);
            m_landscape.HScale = 0.5f; // 0.25f
            m_landscape.VScale = -0.5f; // -0.5f
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
                "world_fantasy\\landscape-winter.jpg", size, size);
            throw new Exception(); // */
            //Debug.Tools.TextureAtlasCreator.CreateFromTextures(new string[] { "grass1.png", "grass2.png", "grass3.png", "grass4.png" }, "grass.png", 512, 512);

            base.Initialize();

            // Initialise le viewport.
            Viewport = new Rectangle(0, 0, Device.ScissorRectangle.Width, Device.ScissorRectangle.Height);

            // Initialise la refraction map du render target.
            m_refractionMapRenderTarget = new RenderTarget2D(Game1.Instance.GraphicsDevice,
                    Game1.Instance.ResolutionWidth / m_gameWorld.GraphicalParameters.ReflectionMapSampleSize,
                    Game1.Instance.ResolutionHeight / m_gameWorld.GraphicalParameters.ReflectionMapSampleSize, true, SurfaceFormat.Dxt5, DepthFormat.Depth24Stencil8);

            // Populations d'arbres en fonction des saisons.
            m_treePopulations = new Dictionary<Season, List<ObjectCullingGroup>>();

            // Gestionnaire de particules
            m_particuleManager = new ParticleManager();
            for (int i = 0; i < 3000; i++)
            {
                m_particuleManager.Particles.Add(new SnowParticle());
            }

            // Chaine de calcul de la luminosité adaptée de la scène.
            m_luminanceCalculationChain = new LuminanceCalculationChain(new Point(Game1.Instance.ResolutionWidth, Game1.Instance.ResolutionHeight));

            // Initialisation des paramètres du bruit
            m_repartitionNoise = new Generation.Noise.NoiseMapGenerator.NoiseParameters();
            m_noiseHigh = new Generation.Noise.NoiseMapGenerator.NoiseParameters();
            m_noiseLow = new Generation.Noise.NoiseMapGenerator.NoiseParameters();

            m_noiseHigh.NoiseType = Generation.Noise.NoiseMapGenerator.NoiseParameters.RIDGED_ID;
            m_noiseHigh.OctaveCount = 6;
            m_noiseHigh.Persistence = 1;
            m_noiseHigh.Lacunarity = 3.75f;
            m_noiseHigh.Frequency = 1.0f;
            m_noiseHigh.Seed = 1073741824;

            m_repartitionNoise.NoiseType = Generation.Noise.NoiseMapGenerator.NoiseParameters.RIDGED_ID;
            m_repartitionNoise.OctaveCount = 4;
            m_repartitionNoise.Persistence = 1;
            m_repartitionNoise.Frequency = 1;
            m_repartitionNoise.Lacunarity = 4.5f;
            m_repartitionNoise.Seed = 1254546457;

            m_noiseLow.NoiseType = Generation.Noise.NoiseMapGenerator.NoiseParameters.PERLIN_ID;
            m_noiseLow.Frequency = 1.125f;
            m_noiseLow.Lacunarity = 4.5f;
            m_noiseLow.Persistence = 0.237f;
            m_noiseLow.OctaveCount = 2;
            m_noiseLow.Seed = 1073741824;

            // Saison de base : hiver
            m_season = Season.Summer;

            // Création du render target de chargement
            m_loadingTexture = new RenderTarget2D(Game1.Instance.GraphicsDevice, Game1.Instance.ResolutionWidth, Game1.Instance.ResolutionHeight, false, SurfaceFormat.Color, DepthFormat.None);
            
            // Chargement de la texture du ciel.
            m_skyTexture = Game1.Instance.Content.Load<Texture2D>("textures\\world_fantasy\\sky");
            Texture2D cerisierTexture = Game1.Instance.Content.Load<Texture2D>("textures\\world_fantasy\\pommier");
            Texture2D pommierTexture = Game1.Instance.Content.Load<Texture2D>("textures\\world_fantasy\\pommier");

            // Textures du paysage.
            m_terrainTextures = new Dictionary<Season, Texture2D>();
            m_terrainTextures[Season.Winter] = Content.Load<Texture2D>("textures\\world_fantasy\\landscape-winter");
            m_terrainTextures[Season.Summer] = Content.Load<Texture2D>("textures\\world_fantasy\\landscape-summer");

            // Normales des textures
            m_terrainNormalMaps = new Dictionary<Season, Texture2D>();
            foreach (var kvp in m_terrainTextures)
            {
                m_terrainNormalMaps.Add(kvp.Key, Generation.Mapping.NormalMapGenerator.GenerateGPU(kvp.Value));
            }

            // Crée et paramètre le shader utilisé pour dessiner le terrain.
            m_terrainShader = Game1.Instance.Content.Load<Effect>("Shaders\\world_fantasy\\mountain");
            m_terrainShader.Parameters["MountainTexture"].SetValue(m_terrainTextures[m_season]);


            // Shader des plantes et des lumières.
            m_billboardSpritesShader = Game1.Instance.Content.Load<Effect>("Shaders\\world_fantasy\\basic-billboard");
            m_lightParticlesShader = Game1.Instance.Content.Load<Effect>("Shaders\\world_fantasy\\light-billboard");

            // Crée et paramètre le shader utilisé pour les arbres fractals
            m_fractalTreesShader = Content.Load<Effect>("Shaders\\world_fantasy\\fractal-tree");
            m_fractalTreesShader.Parameters["TreeTexture"].SetValue(cerisierTexture);
            m_fractalTreesShader2 = m_fractalTreesShader.Clone();
            m_fractalTreesShader2.Parameters["TreeTexture"].SetValue(pommierTexture);

            // Charge les textures de l'herbe
            m_plantsTextures = new Dictionary<Season, Texture2D>();
            m_plantsTextures[Season.Summer] = Game1.Instance.Content.Load<Texture2D>("textures\\world_fantasy\\grass-summer");
            m_plantsTextures[Season.Winter] = Game1.Instance.Content.Load<Texture2D>("textures\\world_fantasy\\grass-summer");

            // Création de l'eau
            m_water = new WaterObject(m_gameWorld.GraphicalParameters);
            m_water.SetReflectionMap(m_reflectionMapRenderTarget);
            m_water.SetRefractionMap(m_refractionMapRenderTarget);

            // Création d'un objet 3D à dessiner.
            m_landscape = new Landscape();
            m_landscape.Shader = m_terrainShader;
            m_landscape.Position = new Vector3(-m_gameWorld.GraphicalParameters.LandscapeResolution / 4, -m_gameWorld.GraphicalParameters.LandscapeResolution / 4, 20);
            m_landscape.HScale = 0.5f; // 0.25f
            m_landscape.VScale = -0.5f; // -0.5f

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

            // Crée les ressources relatives au paysage.
            OnLandscapeSizeChanged();

            
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
            // Création d'un objet 3D à dessiner.
            m_loadingStringStatus = "Génération du paysage";
            /*float[,] machin = Generation.Models.DiamondSquareAlgorithm.GenerateHeightmap(m_gameWorld.GraphicalParameters.LandscapeResolution+1, 
                m_gameWorld.GraphicalParameters.LandscapeResolution + 1,
                2.0f, //2049, 2049,
                (m_rand.Next(100)-150)/10.0f,
                (m_rand.Next(100)-170)/10.0f, 
                (m_rand.Next(200)-250)/10.0f,
                (m_rand.Next(200)-180)/10.0f,
                Generation.Models.DiamondSquareAlgorithm.BaseRandFunc3); // */

            // Regénère certains éléments si la taille du paysage a été modifiée.
            if (m_landscapeSizeChanged)
            {
                m_landscapeSizeChanged = false;
                OnLandscapeSizeChanged();
            }

            float[,] machin = Generation.Noise.NoiseMapGenerator.GenerateMultiNoiseParallelWithTextures(
                m_gameWorld.GraphicalParameters.LandscapeResolution+1,
                ref m_repartitionNoiseTex,
                ref m_noiseHighTex,
                ref m_noiseLowTex,
                ref m_noiseMergedTex,
                m_repartitionNoise, 
                m_noiseHigh,
                m_noiseLow); // */

            Game1.Recorder.Clear();

            m_loadingStringStatus = "Création des modèles du paysage";
            Game1.Recorder.StartRecord("Geomipmap generation");
            m_landscape.Heightmap = machin;
            Game1.Recorder.EndRecord("Geomipmap generation");

            // Regénération des plantes.
            Game1.Recorder.StartRecord("Plant generation");
            if (m_billboardSprites != null)
                m_billboardSprites.Dispose();
            m_loadingStringStatus = "Génération des plantes";
            m_billboardSprites = (ObjectCullingGroup)Generation.Populations.WorldFantasy.PlantPopulation.Generate(m_landscape, m_billboardSpritesShader);
            Game1.Recorder.EndRecord("Plant generation");

            // Regénération des lumières.
            Game1.Recorder.StartRecord("Light generation");
            if (m_lightSprites != null)
                m_lightSprites.Dispose();
            m_loadingStringStatus = "Génération des lumières";
            m_lightSprites = (ObjectCullingGroup)Generation.Populations.WorldFantasy.LightPopulation.Generate(m_landscape, m_lightParticlesShader);
            Game1.Recorder.EndRecord("Light generation");

            // Regénération des lumières.
            Game1.Recorder.StartRecord("Fractal trees generation");
            foreach (var kvp in m_treePopulations)
            {
                foreach (ObjectCullingGroup group in kvp.Value)
                    if (group != null)
                        group.Dispose();
            }
            m_treePopulations.Clear();
            m_loadingStringStatus = "Génération des arbres";

            // Arbres d'été
            List<ObjectCullingGroup> summerTrees = new List<ObjectCullingGroup>();
            /*summerTrees.Add((ObjectCullingGroup)Generation.Populations.WorldFantasy.FractalTreePopulator.Generate(m_landscape, 
                m_fractalTreesShader, Generation.Populations.WorldFantasy.FractalTreePopulator.TreeKind.Cerisier));*/
            summerTrees.Add((ObjectCullingGroup)Generation.Populations.WorldFantasy.FractalTreePopulator.Generate(m_landscape,
    m_fractalTreesShader2, Generation.Populations.WorldFantasy.FractalTreePopulator.TreeKind.Pommier));
            summerTrees.Add((ObjectCullingGroup)Generation.Populations.WorldFantasy.FractalTreePopulator.Generate(m_landscape,
    m_fractalTreesShader, Generation.Populations.WorldFantasy.FractalTreePopulator.TreeKind.Pommier2));

            // Arbres d'hiver
            List<ObjectCullingGroup> winterTrees = new List<ObjectCullingGroup>();
            winterTrees.Add((ObjectCullingGroup)Generation.Populations.WorldFantasy.FractalTreePopulator.Generate(m_landscape,
m_fractalTreesShader2, Generation.Populations.WorldFantasy.FractalTreePopulator.TreeKind.Pommier, true));
            winterTrees.Add((ObjectCullingGroup)Generation.Populations.WorldFantasy.FractalTreePopulator.Generate(m_landscape,
    m_fractalTreesShader, Generation.Populations.WorldFantasy.FractalTreePopulator.TreeKind.Pommier2, true));
            //winterTrees.Add((ObjectCullingGroup)Generation.Populations.WorldFantasy.FractalTreePopulator.Generate(m_landscape,
            //    m_fractalTreesShader2, Generation.Populations.WorldFantasy.FractalTreePopulator.TreeKind.Sapin));


            m_treePopulations.Add(Season.Summer, summerTrees);
            m_treePopulations.Add(Season.Winter, winterTrees);

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
            if (m_displayFramerate)
            {
                Batch.Begin();
                Batch.DrawString(Font, "Framerate : " + ((int)(m_fpsCounter.GetAverageFps())).ToString().PadLeft(4),
                    new Vector2(Game1.Instance.GraphicsDevice.ScissorRectangle.Width - 200, 25), Color.White);
                Batch.End();
            }
        }
        /// <summary>
        /// Dessine les textures du bruit.
        /// </summary>
        /// <param name="time"></param>
        protected void DrawNoise(GameTime time)
        {
            // Dessine les preview des textures de bruit
            if (m_drawNoiseTextures)
            {
                int w = Game1.Instance.ResolutionWidth;
                int h = Game1.Instance.ResolutionHeight;
                int size = Game1.Instance.ResolutionHeight / 2;
                Batch.Begin();
                Batch.Draw(m_noiseLowTex, new Rectangle(w - size * 2, h - size * 2, size, size), Color.White);
                Batch.Draw(m_noiseHighTex, new Rectangle(w - size, h - size * 2, size, size), Color.White);
                Batch.Draw(m_repartitionNoiseTex, new Rectangle(w - size * 2, h - size, size, size), Color.White);
                Batch.Draw(m_noiseMergedTex, new Rectangle(w - size, h - size, size, size), Color.White);
                Batch.End();
            }
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
        /// Dessine le terrain.
        /// </summary>
        protected override void DrawLandscape(GameTime time, string technique)
        {
            // Dessine le terrain.
            m_landscape.Shader.CurrentTechnique = m_landscape.Shader.Techniques[technique];

            // Si on doit dessiner seulement la shadow map.
            if (technique == "ShadowMap")
            {
                m_landscape.Draw(m_gameWorld, false, false);
                return;
            }

            m_landscape.Shader.Parameters["TextureFactor"].SetValue(m_lanscapeTextureFactor);
            m_landscape.Shader.Parameters["mC1"].SetValue(new Vector2(m_c1.Real, m_c1.Imaginary));
            m_landscape.Shader.Parameters["mC2"].SetValue(new Vector2(m_c2.Real, m_c2.Imaginary));
            m_landscape.Shader.Parameters["ClipPlaneNear"].SetValue(0);
            m_landscape.Shader.Parameters["ClipPlaneFar"].SetValue(1);
            m_landscape.Shader.Parameters["SnowThreshold"].SetValue(m_snowThreshold);
            m_landscape.Shader.Parameters["SnowPow"].SetValue(m_snowPow);
            m_landscape.Shader.Parameters["SnowNormal"].SetValue(m_snowNormal);
            m_landscape.Shader.Parameters["NormalMapTexture"].SetValue(m_terrainNormalMaps[m_season]);

            // Change la luminosité du brouillard.
            float lum = m_gameWorld.GetCurrentWorldLuminosity();
            Vector4 color = new Vector4(new Vector3(lum), 1);

            if (!(technique.Contains("Landscape") | technique.Contains("Shadowed")))
            // Pour la réflection, on dessine une version en basse qualité.
            {
                
                m_landscape.Draw(m_gameWorld, ModelMipmap.ModelQuality.Low);
            }
            else
            {
                if (m_gameWorld.GraphicalParameters.BlurLandscape)
                {
                    // Dessin du paysage + infos de profondeur.
                    Device.SetRenderTargets(new RenderTargetBinding[] { new RenderTargetBinding(m_backBuffer), new RenderTargetBinding(m_depthBuffer) });
                    Device.BlendState = BlendState.NonPremultiplied;
                    m_landscape.Shader.CurrentTechnique = m_landscape.Shader.Techniques[technique];
                    m_landscape.Draw(m_gameWorld, false); // dessin + cull
                }
                else
                {
                    Device.SetRenderTarget(m_backBuffer);
                    m_landscape.Draw(m_gameWorld);
                }
            }
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

            // Dessine de la flore créée par ObjectPopulator.
            m_fractalTreesShader.CurrentTechnique = m_fractalTreesShader.Techniques["Ambient"];
            m_fractalTreesShader2.CurrentTechnique = m_fractalTreesShader2.Techniques["Ambient"];
            m_fractalTreesShader.Parameters["WindDirection"].SetValue(m_windAttractor.CurrentPosition / 2.0f);
            m_fractalTreesShader2.Parameters["WindDirection"].SetValue(m_windAttractor.CurrentPosition / 2.0f);

            // Le paramètre réel de la fractale permet d'approximer la "froideur" du paysage en hiver.
            float coldness = m_season == Season.Winter ? m_c2.Real*2 : 0;
            m_fractalTreesShader.Parameters["Coldness"].SetValue(coldness);
            m_fractalTreesShader2.Parameters["Coldness"].SetValue(coldness);
            m_fractalTreesShader.Parameters["SnowPow"].SetValue(m_snowPow);
            m_fractalTreesShader2.Parameters["SnowPow"].SetValue(m_snowPow);
            m_fractalTreesShader.Parameters["SnowThreshold"].SetValue(m_snowThreshold);
            m_fractalTreesShader2.Parameters["SnowThreshold"].SetValue(m_snowThreshold);
            m_fractalTreesShader.Parameters["SnowNormal"].SetValue(m_snowNormal);
            m_fractalTreesShader2.Parameters["SnowNormal"].SetValue(m_snowNormal);


            m_gameWorld.SetRenderDistance(reflection ?
                m_gameWorld.GraphicalParameters.ReflectedTreesRenderDistance : m_gameWorld.GraphicalParameters.TreesRenderDistance);

            /*ObjectCullingGroup group1 = m_treePopulations[m_season][0];
            group1.Draw(m_gameWorld);*/
            foreach (ObjectCullingGroup group in m_treePopulations[m_season])
            {
                group.Draw(m_gameWorld);
            }

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
        /// Dessine les herbes.
        /// </summary>
        /// <param name="time"></param>
        /// <param name="reflection"></param>
        protected void DrawGrass(GameTime time, bool reflection=false)
        {
            windTime += (float)time.ElapsedGameTime.TotalMilliseconds * 0.002f;
            GraphicsDevice device = Device;

            m_gameWorld.SetRenderDistance(reflection ? 
                m_gameWorld.GraphicalParameters.ReflectedGrassRenderDistance : m_gameWorld.GraphicalParameters.GrassRenderDistance);

            // Dessine les sprites une première fois (pixels opaques).
            device.BlendState = BlendState.NonPremultiplied;
            device.DepthStencilState = DepthStencilState.Default;
            device.RasterizerState = RasterizerState.CullNone;

            m_billboardSpritesShader.Parameters["SnowThreshold"].SetValue(m_snowThreshold);
            m_billboardSpritesShader.Parameters["SnowPow"].SetValue(m_snowPow);
            m_billboardSpritesShader.Parameters["SnowNormal"].SetValue(m_snowNormal);
            m_billboardSpritesShader.Parameters["xProjection"].SetValue(m_gameWorld.Projection);
            //m_billboardSpritesShader.Parameters["WindTime"].SetValue(windTime);
            m_billboardSpritesShader.Parameters["WindDirection"].SetValue(m_windAttractor.CurrentPosition*10);
            m_billboardSpritesShader.Parameters["AlphaTestDirection"].SetValue(1);
            m_billboardSpritesShader.Parameters["TreeTexture"].SetValue(m_plantsTextures[m_season]);
            m_billboardSprites.Draw(m_gameWorld);

            // Dessine les pixels transparents
            /*device.BlendState = BlendState.NonPremultiplied;
            device.DepthStencilState = DepthStencilState.DepthRead;
            m_billboardSpritesShader.Parameters["AlphaTestDirection"].SetValue(-1);
            m_billboardSprites.Draw(m_gameWorld);*/
            
            m_gameWorld.RestoreRenderDistance();
        }
        /// <summary>
        /// Dessine des particules de lumière.
        /// </summary>
        /// <param name="time"></param>
        protected void DrawLightParticles(GameTime time, bool reflection=false)
        {
            windTime += (float)time.ElapsedGameTime.TotalMilliseconds * 0.0000005f;
            GraphicsDevice device = Device;
            m_gameWorld.SetRenderDistance(reflection ?
                m_gameWorld.GraphicalParameters.ReflectedLightsRenderDistance : m_gameWorld.GraphicalParameters.LightRenderDistance);

            // Dessine les sprites une première fois (pixels opaques).
            device.BlendState = BlendState.Additive;
            device.DepthStencilState = DepthStencilState.DepthRead;
            device.RasterizerState = RasterizerState.CullNone;

            m_lightParticlesShader.Parameters["xView"].SetValue(m_gameWorld.View);
            m_lightParticlesShader.Parameters["xProjection"].SetValue(m_gameWorld.Projection);
            m_lightParticlesShader.Parameters["WindTime"].SetValue(windTime);
            m_lightParticlesShader.Parameters["AlphaTestDirection"].SetValue(1);
            m_lightSprites.Draw(m_gameWorld);

            m_gameWorld.RestoreRenderDistance();
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
                DrawLandscape(time, "Reflection" + m_season.ToString());
            if (m_gameWorld.GraphicalParameters.ReflectTrees)
                DrawFractalTrees(time, true);
            if (m_gameWorld.GraphicalParameters.ReflectGrass)
                DrawGrass(time, true);
            if (m_gameWorld.GraphicalParameters.ReflectLights)
                DrawLightParticles(time, true);
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

            // Dessine le paysage.
            if (m_gameWorld.GraphicalParameters.ReflectLandscape)
                DrawLandscape(time, "Refraction" + m_season.ToString());
        }
        /// <summary>
        /// Dessine les objets 3D de la scène.
        /// </summary>
        /// <param name="time"></param>
        protected override void Draw3DObjects(GameTime time)
        {
            UpdateRasterizerState();
            m_landscape.MinModelAutoQuality = m_gameWorld.GraphicalParameters.LandscapeMaxQuality;
            // Dessin du terrain.
            if(m_gameWorld.GraphicalParameters.DrawLandscape)
                DrawLandscape(time, "Landscape" + m_season.ToString());

            // On dessine sur le backbuffer et mon met les infos de profondeurs dans un depth buffer séparé.
            Device.SetRenderTargets(new RenderTargetBinding(m_backBuffer), new RenderTargetBinding(m_depthBuffer));

            // Dessine l'eau
            DrawWater(time);

            if(m_gameWorld.GraphicalParameters.DrawGrass)
                DrawGrass(time);

            // Dessines les lumières.
            if(m_gameWorld.GraphicalParameters.DrawLights)
                DrawLightParticles(time);

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

            // Dessine les particules
            m_particuleManager.Draw(Batch, gameTime);
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
            m_fractalTreesShader2.CurrentTechnique = m_fractalTreesShader2.Techniques["ShadowMapInstanced"];
            foreach(ObjectCullingGroup group in  m_treePopulations[m_season])
            {
                group.DrawWithoutCull(m_gameWorld);
            }

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

                // Prends un screenshot.
                // Effectue la combinaison de tous les effets.
                bool takeScreenshot = Input.IsTrigger(Microsoft.Xna.Framework.Input.Keys.B);
                if (takeScreenshot)
                {
                    Game1.Instance.GraphicsDevice.SetRenderTarget(null);
                    DateTime t = DateTime.Now;
                    string filename = "Screen-" + t.Day.ToString() + "-" + t.Month.ToString() + "-" + t.Year.ToString() + "- (" +
                        t.Hour.ToString() + "h " + t.Minute.ToString() + "m " + t.Second.ToString() + "s).png";
                    System.IO.Stream f = System.IO.File.Open(filename, System.IO.FileMode.OpenOrCreate);
                    srcBuffer.SaveAsPng(f, srcBuffer.Width, srcBuffer.Height);
                    f.Close();
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

                // Dessine les textures représentant les bruits.
                DrawNoise(time);
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

            // Landscape texture factor
            World.HUD.HUDTrackbar texFactorCb = new World.HUD.HUDTrackbar();
            texFactorCb.Text = "Tex. Factor";
            texFactorCb.Value = m_lanscapeTextureFactor;
            texFactorCb.Position = hqLandscape.Position + new Vector2(0, 25);
            texFactorCb.MinValue = 0;
            texFactorCb.MaxValue = 200;
            texFactorCb.ValueChanged += delegate(float newValue)
            {
                m_lanscapeTextureFactor = newValue;
            };
            
            // Framerate
            World.HUD.HUDCheckbox dispFramerateCb = new Fractales.World.HUD.HUDCheckbox();
            dispFramerateCb.Text = "Display Framerate";
            dispFramerateCb.Position = new Vector2(texFactorCb.Position.X, texFactorCb.Position.Y + 25);
            dispFramerateCb.Checked = m_displayFramerate;
            dispFramerateCb.ValueChanged += delegate(bool newValue)
            {
                m_displayFramerate = newValue;
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
            graphicalComponents.Add("TexFactor", texFactorCb);
            graphicalComponents.Add("DisplayFramerate", dispFramerateCb);
            return graphicalComponents;
        }

        /// <summary>
        /// Initialise les composants du HUD concernant l'environnement.
        /// </summary>
        Dictionary<string, World.HUD.HUDComponent> InitializeHUDEnvironmentComponents()
        {

            // -- HOUR
            World.HUD.HUDTrackbar hourTrackbar = new World.HUD.HUDTrackbar();
            hourTrackbar.Text = "Hour";
            hourTrackbar.Value = m_blurEffect.Amount;
            hourTrackbar.Position = new Vector2(10, 30);
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
            textureComplexReal.Position = new Vector2(hourTrackbar.Position.X, hourTrackbar.Position.Y + 25);
            textureComplexReal.MinValue = 0;
            textureComplexReal.MaxValue = 0.5f;
            textureComplexReal.ValueChanged += delegate(float newValue)
            {
                m_c2.Real = newValue;
            };

            // Partie imaginaire
            World.HUD.HUDTrackbar textureComplexImaginary = new World.HUD.HUDTrackbar();
            textureComplexImaginary.Text = "C (imaginary)";
            textureComplexImaginary.Value = m_c2.Imaginary;
            textureComplexImaginary.Position = new Vector2(hourTrackbar.Position.X, textureComplexReal.Position.Y + 25);
            textureComplexImaginary.MinValue = -1;
            textureComplexImaginary.MaxValue = 1f;
            textureComplexImaginary.ValueChanged += delegate(float newValue)
            {
                m_c2.Imaginary = newValue;
            };


            // -- SEASON
            World.HUD.HUDCheckbox summerCb = new World.HUD.HUDCheckbox();
            World.HUD.HUDCheckbox winterCb = new World.HUD.HUDCheckbox();

            // Hiver
            winterCb.Text = "Hiver";
            winterCb.Position = new Vector2(hourTrackbar.Position.X, textureComplexImaginary.Position.Y + 25);
            winterCb.Checked = m_season == Season.Winter;
            winterCb.ValueChanged += delegate(bool newValue)
            {
                m_season = Season.Winter;
                m_terrainShader.Parameters["MountainTexture"].SetValue(m_terrainTextures[m_season]);
                summerCb.Checked = false;
            };
            // Eté
            summerCb.Text = "Eté";
            summerCb.Position = new Vector2(winterCb.Position.X + 200, winterCb.Position.Y);
            summerCb.Checked = m_season == Season.Summer;
            summerCb.ValueChanged += delegate(bool newValue)
            {
                m_season = Season.Summer;
                m_terrainShader.Parameters["MountainTexture"].SetValue(m_terrainTextures[m_season]);
                winterCb.Checked = false;
            };

            // -- Neige
            // Snow pow
            World.HUD.HUDTrackbar powTb = new World.HUD.HUDTrackbar();
            powTb.Text = "Snow Pow.";
            powTb.Value = m_snowPow;
            powTb.Position = new Vector2(winterCb.Position.X, summerCb.Position.Y + 25);
            powTb.MinValue = 0;
            powTb.MaxValue = 4;
            powTb.Width = 150;
            powTb.Offset = 80;
            powTb.ValueChanged += delegate(float newValue)
            {
                m_snowPow = newValue;
            };
            World.HUD.HUDTrackbar snowThresholdTb = new World.HUD.HUDTrackbar();
            snowThresholdTb.Text = "Snow Thresh.";
            snowThresholdTb.Value = m_snowThreshold;
            snowThresholdTb.Position = new Vector2(winterCb.Position.X, summerCb.Position.Y + 50);
            snowThresholdTb.MinValue = 0;
            snowThresholdTb.MaxValue = 1f;
            snowThresholdTb.Width = 150;
            snowThresholdTb.Offset = 80;
            snowThresholdTb.ValueChanged += delegate(float newValue)
            {
                m_snowThreshold = newValue;
            };
            // Direction
            World.HUD.HUDTrackbar snowNormalX = new World.HUD.HUDTrackbar();
            snowNormalX.Text = "Normal X";
            snowNormalX.Value = m_snowNormal.X;
            snowNormalX.Position = new Vector2(winterCb.Position.X, snowThresholdTb.Position.Y + 25);
            snowNormalX.MinValue = -1;
            snowNormalX.MaxValue = 1f;
            snowNormalX.Width = 150;
            snowNormalX.Offset = 80;
            snowNormalX.ValueChanged += delegate(float newValue)
            {
                m_snowNormal.X = newValue;
            };
            World.HUD.HUDTrackbar snowNormalY = new World.HUD.HUDTrackbar();
            snowNormalY.Text = "Y";
            snowNormalY.Value = m_snowNormal.Y;
            snowNormalY.Position = new Vector2(winterCb.Position.X + 50, snowThresholdTb.Position.Y + 50);
            snowNormalY.MinValue = -1;
            snowNormalY.MaxValue = 1f;
            snowNormalY.Width = 150;
            snowNormalY.Offset = 30;
            snowNormalY.ValueChanged += delegate(float newValue)
            {
                m_snowNormal.Y = newValue;
            };
            World.HUD.HUDTrackbar snowNormalZ = new World.HUD.HUDTrackbar();
            snowNormalZ.Text = "Z";
            snowNormalZ.Value = m_snowNormal.Z;
            snowNormalZ.Position = new Vector2(winterCb.Position.X + 50, snowThresholdTb.Position.Y + 75);
            snowNormalZ.MinValue = -1;
            snowNormalZ.MaxValue = 1f;
            snowNormalZ.Width = 150;
            snowNormalZ.Offset = 30;
            snowNormalZ.ValueChanged += delegate(float newValue)
            {
                m_snowNormal.Z = newValue;
            };

            // --- Brouillard

            // Activé ?
            World.HUD.HUDCheckbox fogCb = new World.HUD.HUDCheckbox();
            fogCb.Text = "Fog";
            fogCb.Position = new Vector2(hourTrackbar.Position.X, snowNormalZ.Position.Y + 25);
            fogCb.Checked = m_gameWorld.GraphicalParameters.FogEnabled;
            fogCb.ValueChanged += delegate(bool newValue)
            {
                m_gameWorld.GraphicalParameters.FogEnabled = newValue;
            };
            // Start
            World.HUD.HUDTrackbar fogStart = new World.HUD.HUDTrackbar();
            fogStart.Text = "Start";
            fogStart.Value = m_gameWorld.GraphicalParameters.FogStart;
            fogStart.Position = new Vector2(hourTrackbar.Position.X + 50, fogCb.Position.Y + 25);
            fogStart.MinValue = 0;
            fogStart.MaxValue = 1f;
            fogStart.Width = 150;
            fogStart.Offset = 50;
            fogStart.ValueChanged += delegate(float newValue)
            {
                m_gameWorld.GraphicalParameters.FogStart = newValue;
            };
            // Amount
            World.HUD.HUDTrackbar fogAmount = new World.HUD.HUDTrackbar();
            fogAmount.Text = "Amount";
            fogAmount.Value = m_gameWorld.GraphicalParameters.FogAmount;
            fogAmount.Position = new Vector2(hourTrackbar.Position.X + 50, fogStart.Position.Y + 25);
            fogAmount.MinValue = 0;
            fogAmount.MaxValue = 8f;
            fogAmount.Width = 150;
            fogAmount.Offset = 50;
            fogAmount.ValueChanged += delegate(float newValue)
            {
                m_gameWorld.GraphicalParameters.FogAmount = newValue;
            };
            // Color
            // Red
            World.HUD.HUDTrackbar fogRed = new World.HUD.HUDTrackbar();
            fogRed.Text = "Red";
            fogRed.Value = m_gameWorld.GraphicalParameters.FogColor.X;
            fogRed.Position = new Vector2(hourTrackbar.Position.X + 50, fogAmount.Position.Y + 25);
            fogRed.MinValue = 0;
            fogRed.MaxValue = 1f;
            fogRed.Width = 150;
            fogRed.Offset = 50;
            fogRed.ValueChanged += delegate(float newValue)
            {
                Vector3 newColor = m_gameWorld.GraphicalParameters.FogColor;
                newColor.X = newValue;
                m_gameWorld.GraphicalParameters.FogColor = newColor;
            };
            // Green
            World.HUD.HUDTrackbar fogGreen = new World.HUD.HUDTrackbar();
            fogGreen.Text = "Green";
            fogGreen.Value = m_gameWorld.GraphicalParameters.FogColor.Y;
            fogGreen.Position = new Vector2(hourTrackbar.Position.X + 50, fogAmount.Position.Y + 50);
            fogGreen.MinValue = 0;
            fogGreen.MaxValue = 1f;
            fogGreen.Width = 150;
            fogGreen.Offset = 50;
            fogGreen.ValueChanged += delegate(float newValue)
            {
                Vector3 newColor = m_gameWorld.GraphicalParameters.FogColor;
                newColor.Y = newValue;
                m_gameWorld.GraphicalParameters.FogColor = newColor;
            };
            // Red
            World.HUD.HUDTrackbar fogBlue = new World.HUD.HUDTrackbar();
            fogBlue.Text = "Red";
            fogBlue.Value = m_gameWorld.GraphicalParameters.FogColor.Z;
            fogBlue.Position = new Vector2(hourTrackbar.Position.X + 50, fogAmount.Position.Y + 75);
            fogBlue.MinValue = 0;
            fogBlue.MaxValue = 1f;
            fogBlue.Width = 150;
            fogBlue.Offset = 50;
            fogBlue.ValueChanged += delegate(float newValue)
            {
                Vector3 newColor = m_gameWorld.GraphicalParameters.FogColor;
                newColor.Z = newValue;
                m_gameWorld.GraphicalParameters.FogColor = newColor;
            };
            Dictionary<string, World.HUD.HUDComponent> environmentComponents = new Dictionary<string, World.HUD.HUDComponent>();
            environmentComponents.Add("Hour", hourTrackbar);
            environmentComponents.Add("TextureComplexReal", textureComplexReal);
            environmentComponents.Add("TextureComplexImaginary", textureComplexImaginary);
            environmentComponents.Add("SeasonWinter", winterCb);
            environmentComponents.Add("SeasonSummer", summerCb);
            environmentComponents.Add("SnowPow", powTb);
            environmentComponents.Add("SnowThreshold", snowThresholdTb);
            environmentComponents.Add("SnowNormalX", snowNormalX);
            environmentComponents.Add("SnowNormalY", snowNormalY);
            environmentComponents.Add("SnowNormalZ", snowNormalZ);
            environmentComponents.Add("FogEnabled", fogCb);
            environmentComponents.Add("FogStart", fogStart);
            environmentComponents.Add("FogAmount", fogAmount);
            environmentComponents.Add("FogRed", fogRed);
            environmentComponents.Add("FogGreen", fogGreen);
            environmentComponents.Add("FogBlue", fogBlue);

            return environmentComponents;
        }
        /// <summary>
        /// Initialise les composants de génération de terrain.
        /// </summary>
        Dictionary<string, World.HUD.HUDComponent> InitializeHUDGenerationComponents()
        {
            int y = 30;
            Dictionary<string, World.HUD.HUDComponent> generationComponents = new Dictionary<string, World.HUD.HUDComponent>();

            // Suffixes permettant d'identifier les bruits.
            string[] appendix = new string[] { "Rep.", "High", "Low" };
            Modouv.Fractales.Generation.Noise.NoiseMapGenerator.NoiseParameters[] paramsRef = new Modouv.Fractales.Generation.Noise.NoiseMapGenerator.NoiseParameters[] {
                m_repartitionNoise,
                m_noiseHigh,
                m_noiseLow };

            // -- Noise
            for(int i = 0; i < appendix.Length; i++)
            {
                // Obtient une référence locale vers le paramètre visé.
                var noiseParamsRef = paramsRef[i];
                // Paramétrage du type de bruit
                World.HUD.HUDTrackbarInt noiseType = new World.HUD.HUDTrackbarInt();
                noiseType.Text = "Type " + appendix[i];
                noiseType.Value = noiseParamsRef.NoiseType;
                noiseType.Position = new Vector2(10, y);
                noiseType.MaxValue = Modouv.Fractales.Generation.Noise.NoiseMapGenerator.NoiseParameters.Noises.Count;
                noiseType.ValueChanged += delegate(int newValue)
                {
                    noiseParamsRef.NoiseType = newValue;
                };
                y += 25;

                // Octaves
                World.HUD.HUDTrackbarInt noiseOctaves = new World.HUD.HUDTrackbarInt();
                noiseOctaves.Text = "Octaves " + appendix[i];
                noiseOctaves.Value = noiseParamsRef.OctaveCount;
                noiseOctaves.Position = new Vector2(10, y);
                noiseOctaves.MaxValue = 30;
                noiseOctaves.ValueChanged += delegate(int newValue)
                {
                    noiseParamsRef.OctaveCount = newValue;
                };
                y+=25;

                // Octaves
                World.HUD.HUDTrackbar noisePersistence = new World.HUD.HUDTrackbar();
                noisePersistence.Text = "Persistence " + appendix[i];
                noisePersistence.Value = (float)noiseParamsRef.Persistence;
                noisePersistence.Position = new Vector2(10, y);
                noisePersistence.MaxValue = 1;
                noisePersistence.ValueChanged += delegate(float newValue)
                {
                    noiseParamsRef.Persistence = newValue;
                };
                y += 25;

                // Lacunarité
                World.HUD.HUDTrackbar noiseLacunarity = new World.HUD.HUDTrackbar();
                noiseLacunarity.Text = "Lacunarity " + appendix[i];
                noiseLacunarity.Value = (float)noiseParamsRef.Lacunarity;
                noiseLacunarity.Position = new Vector2(noiseOctaves.Position.X, y);
                noiseLacunarity.MinValue = 0;
                noiseLacunarity.MaxValue = 20;
                noiseLacunarity.ValueChanged += delegate(float newValue)
                {
                    noiseParamsRef.Lacunarity = newValue;
                };
                y+=25;
                // Fréquence
                World.HUD.HUDTrackbar noiseFrequency = new World.HUD.HUDTrackbar();
                noiseFrequency.Text = "Frequency " + appendix[i];
                noiseFrequency.Value = (float)noiseParamsRef.Frequency;
                noiseFrequency.Position = new Vector2(noiseOctaves.Position.X, y);
                noiseFrequency.MinValue = 0;
                noiseFrequency.MaxValue = 10;
                noiseFrequency.ValueChanged += delegate(float newValue)
                {
                    noiseParamsRef.Frequency = newValue;
                };
                y+=25;
                // Graine
                World.HUD.HUDTrackbarInt noiseSeed = new World.HUD.HUDTrackbarInt();
                noiseSeed.Text = "Seed " + appendix[i];
                noiseSeed.Value = noiseParamsRef.Seed;
                noiseSeed.Position = new Vector2(noiseOctaves.Position.X, y);
                noiseSeed.MinValue = -1;
                noiseSeed.MaxValue = int.MaxValue/2;
                noiseSeed.ValueChanged += delegate(int newValue)
                {
                    noiseParamsRef.Seed = newValue;
                };
                y += 40;

                generationComponents.Add("Type" + appendix[i], noiseType);
                generationComponents.Add("Persistence" + appendix[i], noisePersistence);
                generationComponents.Add("Octaves"+appendix[i], noiseOctaves);
                generationComponents.Add("Frequency" + appendix[i], noiseFrequency);
                generationComponents.Add("Lacunarity" + appendix[i], noiseLacunarity);
                generationComponents.Add("Seed" + appendix[i], noiseSeed);
            }
            
            // Landscape resolution.
            World.HUD.HUDTrackbarInt landscapeResolution = new World.HUD.HUDTrackbarInt();
            landscapeResolution.Text = "Resolution : ";
            landscapeResolution.Value = (int)Math.Log(m_gameWorld.GraphicalParameters.LandscapeResolution/512, 2);
            landscapeResolution.Position = new Vector2(10, y);
            landscapeResolution.MinValue = 0;
            landscapeResolution.MaxValue = 2;
            landscapeResolution.ValueChanged += delegate(int newValue)
            {
                m_gameWorld.GraphicalParameters.LandscapeResolution = 512 * (int)Math.Pow(2, newValue);
                m_landscapeSizeChanged = true;
            };
            y += 25;

            // Prévisualisation 
            World.HUD.HUDCheckbox previewCb = new World.HUD.HUDCheckbox();
            previewCb.Text = "Noise Preview";
            previewCb.Position = new Vector2(10, y);
            previewCb.Checked = m_drawNoiseTextures;
            previewCb.ValueChanged += delegate(bool newValue)
            {
                m_drawNoiseTextures = newValue;
            };
            y += 25;

            // Texte explicatif :
            StringBuilder text = new StringBuilder();
            text.AppendLine("Types de bruits");
            for (int i = 0; i < Generation.Noise.NoiseMapGenerator.NoiseParameters.Noises.Count; i++)
            {
                Type noiseType = Generation.Noise.NoiseMapGenerator.NoiseParameters.Noises[i];
                text.AppendLine(i.ToString() + " : " + noiseType.Name);
            }
            World.HUD.HUDLabel label = new World.HUD.HUDLabel(text.ToString());
            label.Position = new Vector2(10, y);

            generationComponents.Add("NoisePreview", previewCb);
            generationComponents.Add("Resolution", landscapeResolution);
            generationComponents.Add("Label", label);
            return generationComponents;
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
            // Environment
            var environmentComponents = InitializeHUDEnvironmentComponents();
            // Bruit / génération
            var noiseComponents = InitializeHUDGenerationComponents();

            mgr.Components.Add("Graphics", graphicalComponents);
            mgr.Components.Add("Environment", environmentComponents);
            mgr.Components.Add("Noise", noiseComponents);
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