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
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
namespace Modouv.Fractales.World
{
    /// <summary>
    /// Contient les options graphiques.
    /// </summary>
    public class GraphicalParameters
    {
        #region Properties
        /// <summary>
        /// Obtient ou définit une valeur indiquant si les buffers sont préchargés.
        /// </summary>
        public bool PreloadInstanceBuffers
        {
            get;
            set;
        }
        /// <summary>
        /// Obtient ou définit une valeur qui indique si les ombres dynamiques doivent êtres dessinées.
        /// </summary>
        public bool DrawLandscapeShadows
        {
            get;
            set;
        }

        /// <summary>
        /// Qualité max du paysage.
        /// </summary>
        public Objects.ModelMipmap.ModelQuality LandscapeMaxQuality
        {
            get;
            set;
        }
        /// <summary>
        /// Obtient ou définit une valeur qui indique si on doit flouter le paysage à partir de LandscapeBlurDistance.
        /// </summary>
        public bool BlurLandscape
        {
            get;
            set;
        }
        /// <summary>
        /// Obtient ou définit une valeur qui indique si le paysage doit être dessiné.
        /// </summary>
        public bool DrawLandscape
        {
            get;
            set;
        }
        /// <summary>
        /// Obtient ou définit une valeur qui indique si les arbres doivent être dessinés.
        /// </summary>
        public bool DrawTrees
        {
            get;
            set;
        }
        /// <summary>
        /// Obtient ou définit une valeur qui indique si l'herbe doit être dessinée.
        /// </summary>
        public bool DrawGrass
        {
            get;
            set;
        }
        /// <summary>
        /// Obtient ou définit une valeur qui indique si les lumières doivent être dessinées.
        /// </summary>
        public bool DrawLights
        {
            get;
            set;
        }
        /// <summary>
        /// Distance de rendu des arbres.
        /// </summary>
        public float ReflectedTreesRenderDistance
        {
            get;
            set;
        }
        /// <summary>
        /// Obtient ou définit une valeur qui indique si les arbres doivent être réfléchis par l'eau.
        /// </summary>
        public bool ReflectTrees
        {
            get;
            set;
        }
        /// <summary>
        /// Obtient ou définit une valeur qui indique si l'herbe doit être réfléchie par l'eau.
        /// </summary>
        public bool ReflectGrass
        {
            get;
            set;
        }
        /// <summary>
        /// Distance de rendu des arbres.
        /// </summary>
        public float ReflectedGrassRenderDistance
        {
            get;
            set;
        }
        /// <summary>
        /// Obtient ou définit une valeur qui indique si le paysage doit être réfléchi par l'eau.
        /// </summary>
        public bool ReflectLandscape
        {
            get;
            set;
        }
        /// <summary>
        /// Obtient ou définit une valeur qui indique si les lumières doivent être réfléchies sur l'eau.
        /// </summary>
        public float ReflectedLightsRenderDistance
        {
            get;
            set;
        }
        /// <summary>
        /// Obtient ou définit une valeur qui indique si les lumières doivent être réfléchies sur l'eau.
        /// </summary>
        public bool ReflectLights
        {
            get;
            set;
        }


        /// <summary>
        /// Distance de rendu des arbres.
        /// </summary>
        public float TreesRenderDistance
        {
            get;
            set;
        }

        /// <summary>
        /// Distance de rendu des arbres.
        /// </summary>
        public float GrassRenderDistance
        {
            get;
            set;
        }

        /// <summary>
        /// Distance de rendu des lumières.
        /// </summary>
        public float LightRenderDistance
        {
            get;
            set;
        }

        /// <summary>
        /// Distance à partir de laquelle le paysage doit être flouté.
        /// </summary>
        public float LandscapeBlurDistance
        {
            get;
            set;
        }

        /// <summary>
        /// Position de la lumière.
        /// </summary>
        public Vector4 LightPosition
        {
            get;
            set;
        }
        /// <summary>
        /// Direction de la lumière.
        /// </summary>
        public Vector3 LightDirection
        {
            get;
            set;
        }

        /// <summary>
        /// Définit ou obtient la distance avec le Clipping Plane proche.
        /// Tout ce qui se situe après cette distance sera dessiné.
        /// 
        /// /!\ Ne pas modifier directement cette propriété.
        /// </summary>
        public float NearPlane
        {
            get;
            set;
        }
        /// <summary>
        /// Définit ou obtient la distance avec le Clipping Plane lointain.
        /// Tout ce qui se situe avant cette distance sera dessiné.
        /// 
        /// /!\ Ne pas modifier directement cette propriété.
        /// </summary>
        public float FarPlane
        {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit une valeur indiquant si l'effet de bloom est activé.
        /// </summary>
        public bool BloomEnabled
        {
            get;
            set;
        }
        #endregion
        /// <summary>
        /// Affecte des valeurs par défaut en fonction d'un profil.
        /// </summary>
        public Profile Profile
        {
            set
            {
                switch (value)
                {
                    case Profile.VeryHigh:
                        Resolution = new Point(1920, 1080);
                        MultiSamplingEnabled = true;
                        MultiSampleCount = 16;
                        IsFullScreen = true;
                        break;
                    case Profile.High:
                        Resolution = new Point(1600, 900);
                        MultiSamplingEnabled = false;
                        MultiSampleCount = 0;
                        IsFullScreen = true;
                        break;
                    case Profile.Medium:
                        Resolution = new Point(1200, 675);
                        MultiSamplingEnabled = false;
                        MultiSampleCount = 0;
                        IsFullScreen = true;
                        break;
                    case Profile.MediumDebug:
                        Resolution = new Point(1200, 675);
                        MultiSamplingEnabled = false;
                        MultiSampleCount = 0;
                        IsFullScreen = false;
                        break;
                    case Profile.Low:
                        Resolution = new Point(800, 450);
                        MultiSamplingEnabled = true;
                        MultiSampleCount = 16;
                        IsFullScreen = false;
                        break;
                    case Profile.VeryLow:
                        Resolution = new Point(400, 275);
                        MultiSamplingEnabled = true;
                        MultiSampleCount = 16;
                        IsFullScreen = false;
                        break;
                    case Profile.UltraLow:
                        Resolution = new Point(16, 9);
                        MultiSamplingEnabled = true;
                        MultiSampleCount = 16;
                        IsFullScreen = false;
                        break;

                }
            }
        }
        #region Paramètres d'initialisation
        int m_sampleCount;
        /// <summary>
        /// Valeur indiquant si le plein écran est activé.
        /// </summary>
        public bool IsFullScreen
        {
            get;
            set;
        }
        /// <summary>
        /// Résolution de dessin.
        /// </summary>
        public Point Resolution
        {
            get;
            set;
        }
        /// <summary>
        /// Valeur indiquant si le multi-sampling est activé.
        /// </summary>
        public bool MultiSamplingEnabled
        {
            get;
            set;
        }
        /// <summary>
        /// Nombre de samples si le multisampling est activé.
        /// </summary>
        public int MultiSampleCount
        {
            get { return MultiSamplingEnabled ? m_sampleCount : 0; }
            set { m_sampleCount = value; }
        }
        /// <summary>
        /// Taille des samples sur la map de réflection.
        /// 1 : qualité optimale
        /// >1 : qualité réduite
        /// 
        /// /!\ Modifier cette propriété n'a d'effet qu'avant la création de l'eau.
        /// </summary>
        public int ReflectionMapSampleSize
        {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit une valeur indiquant si le rendu HDR est activé.
        /// /!\ Cela n'a l'effet voulu qu'avant le démarrage de l'application.
        /// </summary>
        public bool UseHDR
        {
            get;
            set;
        }
        /// <summary>
        /// Taille des samples sur la Shadow Map.
        /// 1 : qualité optimale
        /// >1 : qualité réduite
        /// </summary>
        public int ShadowMapSampleSize
        {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit une valeur indiquant si le brouillard est activé.
        /// </summary>
        public bool FogEnabled
        {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit une valeur indiquant la couleur du brouillard.
        /// </summary>
        public Vector3 FogColor
        {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit une valeur comprise dans [0, 1] indiquant le début du brouillard.
        /// </summary>
        public float FogStart
        {
            get;
            set;
        }
        /// <summary>
        /// Obtient ou définit une valeur indiquant la quantité de brouillard.
        /// </summary>
        public float FogAmount
        {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit la valeur de la profondeur sur laquelle est faite le focus.
        /// -1 : focus auto.
        /// </summary>
        public float FocusDepth
        {
            get;
            set;
        }
        /// <summary>
        /// Obtient ou définit la valeur de puissance du focus.
        /// Plus le focus power est important, plus les endroits loin du point de focus seront floutés.
        /// </summary>
        public float FocusPower
        {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit la résolution du paysage.
        /// /// /!\ Cela n'a l'effet voulu qu'avant le démarrage de l'application.
        /// </summary>
        public int LandscapeResolution
        {
            get;
            set;
        }

        /// <summary>
        /// Nombre de base d'itérations pour les arbres.
        /// </summary>
        public int TreesIterationsBasis
        {
            get;
            set;
        }
        #endregion
        /// <summary>
        /// Crée une nouvelle instance de GraphicalParameters.
        /// </summary>
        public GraphicalParameters()
        {
            Resolution = new Point(1600, 900);
            MultiSamplingEnabled = false;
            MultiSampleCount = 16;
            ReflectionMapSampleSize = 1;
            ShadowMapSampleSize = 1;
            PreloadInstanceBuffers = false;

            DrawTrees = false;
            DrawGrass = false;
            DrawLandscape = true;
            DrawLights = true;

            ReflectTrees = false;
            ReflectLandscape = true;
            ReflectGrass = false;
            ReflectLights = true;

            LandscapeMaxQuality = Objects.ModelMipmap.ModelQuality.High;
            TreesRenderDistance = 250;//120;
            GrassRenderDistance = 250;//120;
            LightRenderDistance = 30;
            NearPlane = 0.1f;
            FarPlane = 250;
            ReflectedGrassRenderDistance = 50;
            ReflectedLightsRenderDistance = 30;
            ReflectedTreesRenderDistance = 120;

            LandscapeBlurDistance = 0.50f;
            BlurLandscape = true;
            BloomEnabled = true;
            FogEnabled = false;
            FogColor = new Vector3(0.7f, 0.786f, 0.9f);
            FogStart = 0.00f;
            FogAmount = 1.0f;
            FocusDepth = -1;
            FocusPower = 0.25f;
            
            LandscapeResolution = 1024;
            UseHDR = true;
            TreesIterationsBasis = 5;



            DrawLandscapeShadows = true;
            // Place la lumière à "l'infini";
            LightDirection = Vector3.Normalize(new Vector3(-0.6f, -0.4f, -0.1f));
        }
    }
    /// <summary>
    /// Contient les données du monde de jeu.
    /// </summary>
    public class GameWorld
    {
        public static GameWorld Instance;
        /* --------------------------------------------------------------------------
         * Constantes
         * ------------------------------------------------------------------------*/
        #region Constantes


        #endregion
        /* --------------------------------------------------------------------------
         * Variables
         * ------------------------------------------------------------------------*/
        #region Variables
        Matrix m_projection;
        Matrix m_customView;
        bool m_isCustomView = false;
        BoundingFrustum m_frustum;
        float m_renderDistance;
        RenderTarget2D m_shadowMapRenderTarget;
        RenderTarget2D m_backgroundRenderTarget;
        #endregion
        /* --------------------------------------------------------------------------
         * Graphics parameters
         * ------------------------------------------------------------------------*/
        #region Graphics Parameters
        /// <summary>
        /// Obtient ou définit les paramètres graphiques liés à ce monde.
        /// </summary>
        public GraphicalParameters GraphicalParameters { get; set;}
        /// <summary>
        /// Obtient ou définit une valeur disant si la shadow map a besoin d'être refresh.
        /// </summary>
        public bool NeedRefreshShadowMap { get; set; }
        /// <summary>
        /// Obtient le render target utilisé pour effectuer le rendu du background.
        /// </summary>
        public RenderTarget2D BackgroundRenderTarget { get { return m_backgroundRenderTarget; } }
        #endregion
        /* --------------------------------------------------------------------------
         * Properties
         * ------------------------------------------------------------------------*/
        #region Properties
        /// <summary>
        /// Obtient ou définit la direction de la lumière.
        /// 
        /// /!\ Obsolète. Utiliser GraphicalParameters.LightDirection.
        /// </summary>
        public Vector3 LightDirection { get { return GraphicalParameters.LightDirection; } set { GraphicalParameters.LightDirection = value; } }
        /// <summary>
        /// Obtient ou définit la position de la lumière.
        /// 
        /// /!\ Obsolète. Utiliser GraphicalParameters.LightPosition.
        /// </summary>
        public Vector4 LightPosition { get { return GraphicalParameters.LightPosition; } set { GraphicalParameters.LightPosition = value; } }

        /// <summary>
        /// Objet 3D dont les dimmensions font qu'il contient tout ce que voit la caméra.
        /// Utilisé pour déterminer si oui ou non des objets doivent être envoyés au GPU.
        /// </summary>
        public BoundingFrustum GetFrustrum()
        {
            return m_frustum;
        }

        /// <summary>
        /// Définit les paramètres communs à tous les shaders à partir du monde actuel et de la
        /// transformation passée en paramètre.
        /// </summary>
        /// <param name="worldTransform"></param>
        public void SetupShader(Effect Shader, Matrix worldTransform)
        {
            ComputePrecalculations();

            // Donne les near et far clipping planes au shader.
            Shader.Parameters["xFogNear"].SetValue(NearPlane);
            Shader.Parameters["xFogFar"].SetValue(m_renderDistance);
            Shader.Parameters["xMaxRenderDistance"].SetValue(FarPlane);
            Shader.Parameters["xFogEnabled"].SetValue(GraphicalParameters.FogEnabled);
            Shader.Parameters["xFogColor"].SetValue(GraphicalParameters.FogColor);
            Shader.Parameters["xFogAmount"].SetValue(GraphicalParameters.FogAmount);
            Shader.Parameters["xFogStart"].SetValue(GraphicalParameters.FogStart);
            Shader.Parameters["xBackgroundTexture"].SetValue(BackgroundRenderTarget);

            // Donne la position de la caméra au shader
            Shader.Parameters["xCameraPos"].SetValue(new Vector4(Camera.Position + Camera.Front, 1));
            Shader.Parameters["xCameraDirection"].SetValue(Camera.Front);
            Shader.Parameters["xLightPosition"].SetValue(GraphicalParameters.LightPosition);
            Shader.Parameters["xLightDirection"].SetValue(GraphicalParameters.LightDirection);
            Shader.Parameters["xGlobalIllumination"].SetValue(GetCurrentWorldLuminosity());

            // Donne la matrice World au shader.
            Matrix worldMatrix = World * worldTransform;
            Shader.Parameters["xWorld"].SetValue(worldMatrix);

            if (GraphicalParameters.DrawLandscapeShadows)
            {
                if (Shader.CurrentTechnique.Name != "ShadowMap" && Shader.CurrentTechnique.Name != "ShadowMapInstanced")
                    Shader.Parameters["xShadowMapTexture"].SetValue(m_shadowMapRenderTarget);
                Shader.Parameters["xLightWorldViewProjection"].SetValue(LightView * Projection);
            }

            // Donne la matrice WorldViewProjection précalculée au shader.
            Matrix worldViewProjection = worldMatrix * View * Projection;
            Shader.Parameters["xWorldViewProjection"].SetValue(worldViewProjection);

            // Matrice view
            Shader.Parameters["xView"].SetValue(View);
        }

        /// <summary>
        /// Crée un plan dont la direction est donnée par planeNormalDirection.
        /// </summary>
        public Plane CreatePlane(float size, Vector3 planeNormalDirection, Matrix view, bool clipSide)
        {
            planeNormalDirection.Normalize();
            Vector4 planeCoeffs = new Vector4(planeNormalDirection, size);
            if (clipSide)
                planeCoeffs *= -1;

            Matrix worldViewProjection = view * Projection;
            Matrix inverseWorldViewProjection = Matrix.Invert(worldViewProjection);
            inverseWorldViewProjection = Matrix.Transpose(inverseWorldViewProjection);

            planeCoeffs = Vector4.Transform(planeCoeffs, inverseWorldViewProjection);
            Plane finalPlane = new Plane(planeCoeffs);

            return finalPlane;
        }

        /// <summary>
        /// Définit ou obtient la distance avec le Clipping Plane proche.
        /// Tout ce qui se situe après cette distance sera dessiné.
        /// </summary>
        public float NearPlane
        {
            get { return GraphicalParameters.NearPlane; }
            set {
                GraphicalParameters.NearPlane = value;
                ComputePrecalculations();
            }
        }
        /// <summary>
        /// Définit ou obtient la distance avec le Clipping Plane lointain.
        /// Tout ce qui se situe avant cette distance sera dessiné.
        /// </summary>
        public float FarPlane
        {
            get { return GraphicalParameters.FarPlane; }
            set
            {
                GraphicalParameters.FarPlane = value;
                ComputePrecalculations();
            }
        }

        /// <summary>
        /// Matrice de projection utilisée pour le rendu.
        /// </summary>
        public Matrix Projection
        {
            get
            {
                return m_projection;
            }
        }
        /// <summary>
        /// Matrice World utilisée pour le rendu.
        /// </summary>
        public Matrix World
        {
            get;
            set;
        }

        /// <summary>
        /// Retourne la matrice "view" de la caméra.
        /// </summary>
        public Matrix View
        {
            get {
                if (m_isCustomView)
                    return m_customView;
                else
                    return Camera.View; 
            }
            set
            {
                m_isCustomView = true;
                m_customView = value;
                ComputePrecalculations();
            }
        }
        /// <summary>
        /// Obtient la matrice View de la lumière.
        /// Utilisée uniquement pour la shadow map.
        /// </summary>
        public Matrix LightView { get; set; }
        /// <summary>
        /// Restore la vue de la caméra si elle a été altérée par une view custom.
        /// </summary>
        public void RestoreCameraView()
        {
            m_isCustomView = false;
            ComputePrecalculations();
        }
        /// <summary>
        /// Obtient ou définit la caméra utilisée par ce monde.
        /// </summary>
        public Cameras.FirstPersonCameraV2 Camera
        {
            get;
            set;
        }
        /// <summary>
        /// Obtient le RenderTarget utilisé pour dessiner la shadow map.
        /// </summary>
        public RenderTarget2D ShadowMapRenderTarget
        {
            get { return m_shadowMapRenderTarget; }
        }
        /// <summary>
        /// Précalcule certaines matrices.
        /// </summary>
        void ComputePrecalculations()
        {
            // Projection
            float aspectRatio = (float)Game1.Instance.ResolutionWidth / (float)Game1.Instance.ResolutionHeight;
            float fov = MathHelper.PiOver4 * aspectRatio * 0.75f;//0.75f;//3 / 4;
            m_projection = Matrix.CreatePerspectiveFieldOfView(fov, aspectRatio, GraphicalParameters.NearPlane, GraphicalParameters.FarPlane);

            // Frustum
            // Matrice de projection prenant en compte la render distance.
            // Elle permet d'avoir un frustum différent, tout en conservant une matrice de projection pour le dessin
            // qui conserve la validité des données dans le depth buffer lorsqu'on change la render distance.
            Matrix frustumProjection = Matrix.CreatePerspectiveFieldOfView(fov, aspectRatio, GraphicalParameters.NearPlane, m_renderDistance);
            m_frustum.Matrix = (View * frustumProjection);
        }
        #endregion
        /* --------------------------------------------------------------------------
         * Constructeur privé
         * ------------------------------------------------------------------------*/
        #region Constructor (private)
        /// <summary>
        /// Constructeur privé.
        /// </summary>
        private void ctor()
        {
            // Monde = identité par défaut
            World = Matrix.Identity;

            // Caméra
            Camera = new Cameras.FirstPersonCameraV2();
            
            // Render distance par défaut : far plane
            m_renderDistance = GraphicalParameters.FarPlane;

            // Crée un frustum bidon.
            m_frustum = new BoundingFrustum(Matrix.Identity);

            // Crée le render target pour la shadow map
            int mult = 2;
            m_shadowMapRenderTarget = new RenderTarget2D(Game1.Instance.GraphicsDevice, Game1.Instance.ResolutionWidth / GraphicalParameters.ShadowMapSampleSize * mult,
                Game1.Instance.ResolutionHeight / GraphicalParameters.ShadowMapSampleSize * mult, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.DiscardContents);

            // Background render target avec le même surface format que le back buffer.
            SurfaceFormat format = (GraphicalParameters.UseHDR && Scenes.SceneFantasyWorld.USE_HDR_BLENDABLE_RENDER_TARGET) ? SurfaceFormat.HdrBlendable : SurfaceFormat.Color;
            m_backgroundRenderTarget = new RenderTarget2D(Game1.Instance.GraphicsDevice,
                Game1.Instance.ResolutionWidth,
                Game1.Instance.ResolutionHeight,
                false,
                format,
                DepthFormat.Depth24Stencil8);

            Hour = 19.2f;
            
            // Précalcule certaines matrices.
            ComputePrecalculations();
        }
        #endregion
        /* --------------------------------------------------------------------------
         * Public API
         * ------------------------------------------------------------------------*/
        #region Public API
        /// <summary>
        /// Crée une nouvelle instance du GameWorld avec des paramètres par défaut.
        /// </summary>
        public GameWorld()
        {
            Instance = this;
            GraphicalParameters = new GraphicalParameters();


            // Appelle le constructor privé.
            ctor();
        }
        /// <summary>
        /// Crée une nouvelle instance du GameWorld avec des paramètres donnés.
        /// </summary>
        public GameWorld(GraphicalParameters parameters)
        {
            Instance = this;
            GraphicalParameters = parameters;
            ctor();
        }
        /// <summary>
        /// Modifie la render distance pour la valeur donnée.
        /// </summary>
        /// <param name="renderDistance"></param>
        public void SetRenderDistance(float renderDistance)
        {
            m_renderDistance = renderDistance;
            ComputePrecalculations();
        }
        /// <summary>
        /// Restore la render distance par défaut.
        /// </summary>
        /// <param name="renderDistance"></param>
        public void RestoreRenderDistance()
        {
            m_renderDistance = GraphicalParameters.FarPlane;
            ComputePrecalculations();
        }

        #region Hour etc...
        float hour = 0;
        /// <summary>
        /// Obtient ou définit l'heure (comprise entre 0 et 24).
        /// </summary>
        public float Hour
        {
            get { return hour; }
            set { 
                hour = value;
                if (hour < 0)
                    hour = 24 - hour % 24;
                hour %= 24;
                NeedRefreshShadowMap = true;
            }
        }
        /// <summary>
        /// Retourne la lumiosité du monde.
        /// </summary>
        /// <returns></returns>
        public float GetCurrentWorldLuminosity()
        {
            return Math.Abs(Hour - 12) / 8.0f;
        }
        #endregion

        /// <summary>
        /// Mise à jour du GameWorld.
        /// </summary>
        public void Update(GameTime time)
        {
            ComputePrecalculations();
            float angle = (float)(hour * 2 * Math.PI / 24.0) + MathHelper.Pi;
            Vector3 direction = Vector3.Normalize(new Vector3(0.2f, (float)Math.Sin(angle), (float)Math.Cos(angle)));
            GraphicalParameters.LightPosition = new Vector4(direction*350, 1.0f);
            GraphicalParameters.LightDirection = direction;

        }
        #endregion
    }
}
