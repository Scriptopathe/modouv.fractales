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
namespace Modouv.Fractales.Scenes
{
    /// <summary>
    /// Scene de test de la fractale Julia.
    /// </summary>
    public class SceneTestJulia : Scene
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
        /// <summary>
        /// Option permettant de faire bouger la fractale toute seule
        /// 0 : pas de mvt
        /// 1 ou -1 : mvt de c.
        /// </summary>
        int m_moveFractal;
        #endregion

        #region Variables / 3D
        /// <summary>
        /// Matrice World.
        /// </summary>
        Matrix m_world;
        /// <summary>
        /// Matrice View.
        /// </summary>
        Matrix m_view;
        /// <summary>
        /// Matrice Projection.
        /// </summary>
        Matrix m_projection;

        /// <summary>
        /// Position de la caméra.
        /// </summary>
        Vector3 m_cameraPosition;
        /// <summary>
        /// Position de la cible de la caméra (l'endroit où la caméra pointe du nez).
        /// </summary>
        Vector3 m_cameraTarget;

        /// <summary>
        /// Position du modèle 3D.
        /// </summary>
        Vector3 m_modelPosition;

        /// <summary>
        /// Modèle 3D.
        /// </summary>
        //Model m_model;

        /// <summary>
        /// Shader 3D.
        /// </summary>
        Effect m_3DShader;
        #endregion

        #region Methods 3D
        /// <summary>
        /// Initialise les positions 3D des éléments.
        /// </summary>
        void Initialize3DPositions()
        {
            m_cameraPosition = Vector3.Zero;
            m_cameraTarget = new Vector3(1, 0, 0);
            m_modelPosition = new Vector3(5000, 0, 0);
            //m_model = Game1.Instance.Content.Load<Model>("Models\\p1_wedge");
            m_3DShader = Game1.Instance.Content.Load<Effect>("Shaders\\Test");
        }
        /// <summary>
        /// Initialise les matrices.
        /// </summary>
        void UpdateMatrix()
        {
            _rotation += 1f;
            // Matrice world
            m_modelPosition = new Vector3(5000 + (1 / m_scale)*1000, 0, 0);
            m_world = Matrix.Identity;//Matrix.CreateWorld(Vector3.Zero, Vector3.Forward, Vector3.Up);
            // Matrice View : représente la caméra.
            m_view = Matrix.CreateLookAt(m_cameraPosition, m_cameraTarget, Vector3.Up);
            // Calcule la matrice de projection.
            float aspectRatio = (float)Game1.Instance.ResolutionWidth / (float)Game1.Instance.ResolutionHeight;
            float fov = MathHelper.PiOver4 * aspectRatio * 3 / 4;
            
            m_projection = Matrix.CreatePerspectiveFieldOfView(fov, aspectRatio, 0.1f, 1000000000.0f);
        }

        float _rotation = 0.0f;
        /// <summary>
        /// Dessine le modèle 3D avec comme texture la fractale.
        /// </summary>
        void Draw3DModel()
        {
            // Paramètre un depth buffer pour que les polygones dessinés en arrière plan soient cachés.
            Game1.Instance.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            Game1.Instance.GraphicsDevice.BlendState = BlendState.Opaque;
            Game1.Instance.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            // Dessine le mesh 3D.
            /*Matrix[] transforms = new Matrix[m_model.Bones.Count];
            m_model.CopyAbsoluteBoneTransformsTo(transforms);

            Matrix modelWorld = m_world*Matrix.CreateRotationZ(MathHelper.ToRadians(45f)) *
                Matrix.CreateRotationY(MathHelper.ToRadians(45 + _rotation)) *
                Matrix.CreateTranslation(m_modelPosition);

            // Draw the model. A model can have multiple meshes, so loop.
            foreach (ModelMesh mesh in m_model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    Game1.Instance.GraphicsDevice.SetVertexBuffer(part.VertexBuffer, part.VertexOffset);
                    Game1.Instance.GraphicsDevice.Indices = part.IndexBuffer;
                    m_3DShader.Parameters["World"].SetValue(transforms[mesh.ParentBone.Index] * modelWorld);
                    m_3DShader.Parameters["View"].SetValue(m_view);
                    m_3DShader.Parameters["Projection"].SetValue(m_projection);
                    Game1.Instance.GraphicsDevice.Textures[10] = m_juliaTexture;
                    m_3DShader.CurrentTechnique.Passes[0].Apply();
                    Game1.Instance.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 
                            0, 0,
                            part.NumVertices, part.StartIndex, part.PrimitiveCount);

                }
            }*/
        }
        #endregion

        #region Core Methods
        /// <summary>
        /// Crée une nouvelle instance de SceneTestJulia.
        /// </summary>
        public SceneTestJulia()
        {

        }

        /// <summary>
        /// Initialise la scène.
        /// </summary>
        public override void Initialize()
        {
            //m_multisampling = Content.Load<Effect>("Shaders\\blur");
            m_resolution = new Point(Game1.Instance.Window.ClientBounds.Width, Game1.Instance.Window.ClientBounds.Height);
            LoadJulia(m_c, m_resolution);
            Initialize3DPositions();
        }
        /// <summary>
        /// Mets à jour tout ce qui est lié à l'appui de touches sur le clavier, et aux actions qui en résultent.
        /// </summary>
        void UpdateInput()
        {
            bool reload = false;
            float scaleFactor = 1.005f;
            float mvSpeed = 25;
            float cFactor = 0.001f;

            if (m_moveFractal != 0)
            {
                reload = true;
                m_c.Real += 0.0001f*m_moveFractal;
            }

            if (Input.IsTrigger(Keys.E))
            {
                m_moveFractal = m_moveFractal == 1 ? 0 : 1;
                reload = true;
            }
            else if (Input.IsTrigger(Keys.A))
            {
                m_moveFractal = m_moveFractal == -1 ? 0 : -1;
                reload = true;
            }

            // Traite l'appui de touches.
            if (Input.IsPressed(Keys.NumPad9))
            {
                m_scale *= scaleFactor;
                reload = true;
            }
            else if (Input.IsPressed(Keys.NumPad7))
            {
                m_scale /= scaleFactor;
                reload = true;
            }
            if (Input.IsPressed(Keys.NumPad3))
            {
                m_resolution = new Point(Game1.Instance.Window.ClientBounds.Width, Game1.Instance.Window.ClientBounds.Height);
                reload = true;
            }
            else if (Input.IsPressed(Keys.NumPad1))
            {
                m_resolution = new Point(Game1.Instance.Window.ClientBounds.Width/4, Game1.Instance.Window.ClientBounds.Height/4);
                reload = true;
            }
            if (Input.IsPressed(Keys.NumPad8))
            {
                m_c.Imaginary -= cFactor;
                reload = true;
            }
            else if (Input.IsPressed(Keys.NumPad2))
            {
                m_c.Imaginary += cFactor;
                reload = true;
            }

            if (Input.IsPressed(Keys.NumPad4))
            {
                m_c.Real -= cFactor;
                reload = true;
            }
            else if (Input.IsPressed(Keys.NumPad6))
            {
                m_c.Real += cFactor;
                reload = true;
            }

            if (Input.IsPressed(Keys.Z))
            {
                m_offset.Imaginary -= mvSpeed/800.0f*m_scale;
                reload = true;
            }
            else if (Input.IsPressed(Keys.S))
            {
                m_offset.Imaginary += mvSpeed/800.0f*m_scale;
                reload = true;
            }

            if (Input.IsPressed(Keys.Q))
            {
                m_offset.Real -= mvSpeed/1200.0f*m_scale;
                reload = true;
            }
            else if (Input.IsPressed(Keys.D))
            {
                m_offset.Real += mvSpeed/1200.0f*m_scale;
                reload = true;
            }

            if (reload)
                LoadJulia(m_c, m_resolution);
        }
        /// <summary>
        /// Mets à jour la scène.
        /// </summary>
        /// <param name="time"></param>
        public override void Update(GameTime time)
        {
            UpdateInput();
            UpdateMatrix();
        }

        /// <summary>
        /// Dessine la fractale en 2D sur l'écran.
        /// </summary>
        public void Draw2DFractal()
        {
            // Dessine la fractale de julia
            Batch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
           /* m_multisampling.Parameters["pixelWidth"].SetValue(1.0f / Device.ScissorRectangle.Width);
            m_multisampling.Parameters["pixelHeight"].SetValue(1.0f / Device.ScissorRectangle.Height);
            m_multisampling.CurrentTechnique.Passes[0].Apply();*/
            Batch.Draw(m_juliaTexture, Device.ScissorRectangle, Color.White);
            Batch.End();

            // Dessine les informations sur le dessin.
            Batch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            Batch.DrawString(Font, "c = " + m_c.Real.ToString() + "+" + m_c.Imaginary.ToString() + "i" +
                " - Zoom = " + m_scale.ToString(),
                new Vector2(0, 0), Color.White);
            Batch.DrawString(Font, "+",
                new Vector2(Device.ScissorRectangle.Center.X - 4, Device.ScissorRectangle.Center.Y - 4), Color.White);

            // Dessine l'échelle
            float scalex = m_scale / (float)m_resolution.X;
            float scaley = m_scale / (float)m_resolution.Y;
            Vector2 LateralBounds = new Vector2(scalex * (m_offset.Real - m_resolution.X / 2), scalex * (m_offset.Real + m_resolution.X - m_resolution.X / 2));
            Vector2 UpDownBounds = new Vector2(scaley * (m_offset.Imaginary - m_resolution.Y / 2), scaley * (m_offset.Imaginary + m_resolution.Y - m_resolution.Y / 2));
            Batch.DrawString(Font, "X : [" + LateralBounds.X.ToString() + ", " + LateralBounds.Y.ToString() + "]", new Vector2(0, 25), Color.White);
            Batch.DrawString(Font, "Y : [" + UpDownBounds.X.ToString() + ", " + UpDownBounds.Y.ToString() + "]", new Vector2(0, 50), Color.White);
            Batch.End();
        }
        /// <summary>
        /// Dessine la scène.
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            Draw2DFractal();
            Draw3DModel();
        }
        #endregion

        /// <summary>
        /// Charge une texture de julia dans la carte graphique.
        /// </summary>
        /// <param name="c"></param>
        void LoadJulia(MathHelpers.Complex c, Point size)
        {
            // Crée la texture si elle n'existe pas ou n'est pas de la bonne taille.
            if (m_juliaTexture != null)
            {
                if (m_juliaTexture.Width != size.X || m_juliaTexture.Height != size.Y)
                {
                    m_juliaTexture.Dispose();
                    m_juliaTexture = new RenderTarget2D(Device, size.X, size.Y);
                }
            }
            else
                m_juliaTexture = new RenderTarget2D(Device, size.X, size.Y);

            // Dessine la texture de la fractale de julia à l'aide du GPU sur m_juliaTexture.
            Generation.Fractals.Julia.GenerateTexture2DGPU(c,
                new MathHelpers.Complex(-size.X / 2 + m_offset.Real * size.X / m_scale,
                                 -size.Y / 2 + m_offset.Imaginary * size.Y / m_scale),
                m_scale,
                m_juliaTexture);
        }
    }
}
