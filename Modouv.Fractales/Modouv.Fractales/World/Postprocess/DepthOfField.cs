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
namespace Modouv.Fractales.World.Postprocess
{
    /// <summary>
    /// Effet de de post traitement de type bloom.
    /// </summary>
    public class DepthOfField
    {
        #region Variables
        /// <summary>
        /// Chaine de calcul de l'auto focus.
        /// </summary>
        FocusCalculationChain m_autoFocusCalculationChain;
        /// <summary>
        /// Effet permettant d'extraire les zones lumineuses d'une image.
        /// </summary>
        Effect m_bloomExtractEffect;
        /// <summary>
        /// Effet permettant de combiner la source originale et le bloom.
        /// </summary>
        Effect m_combineEffect;
        /// <summary>
        /// Effet de flou appliqué aux zones brillantes.
        /// </summary>
        GaussianBlur m_blurEffect;

        RenderTarget2D m_tmpRenderTarget3;

        /// <summary>
        /// Seuil de luminosité à partir duquel on va extraire une couleur.
        /// </summary>
        float m_bloomEffectThreshold;
        /// <summary>
        /// Rayon de l'effet de bloom.
        /// </summary>
        int m_radius = 1;
        /// <summary>
        /// Quantité de l'effet de bloom.
        /// </summary>
        float m_amount = 2.7f;
        /// <summary>
        /// Si vrai, le kernel du bloom doit être recalculé.
        /// </summary>
        bool m_needComputeKernel;
        #endregion

        #region Properties
        /// <summary>
        /// Radius du bloom.
        /// </summary>
        public int BlurRadius
        {
            get { return m_radius; }
            set { 
                m_radius = value; 
                m_needComputeKernel = true;
                m_radius = Math.Min(20, Math.Max(1, m_radius));
            }
        }
        /// <summary>
        /// Quantité de bloom à appliquer.
        /// </summary>
        public float BlurAmount
        {
            get { return m_amount; }
            set { 
                m_amount = value;
                m_needComputeKernel = true;
                m_amount = Math.Min(10, Math.Max(1, m_amount));
            }
        }

        /// <summary>
        /// Détermine le coefficient de mélange de la couleur du bloom lors de l'effet de blur.
        /// </summary>
        public float BlurPower
        {
            get;
            set;
        }
        /// <summary>
        /// Détermine le coefficient de mélange de la couleur de la map lors de l'effet de blur.
        /// </summary>
        public float MapPower
        {
            get;
            set;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Initialise une nouvelle instance de Bloom.
        /// </summary>
        public DepthOfField()
        {
            m_combineEffect = Game1.Instance.Content.Load<Effect>("Shaders\\postprocess\\Combine");
            m_autoFocusCalculationChain = new FocusCalculationChain(new Point(Game1.Instance.ResolutionWidth, Game1.Instance.ResolutionHeight));
            m_blurEffect = new GaussianBlur(Game1.Instance);
            BlurRadius = 20;
            BlurAmount = 3.0f;
            MapPower = 1;
            BlurPower = 50;//5.0f;
            // Ceci est magique, mais ne sert à rien.
            m_blurEffect.ComputeKernel(0, m_amount);
            m_blurEffect.ComputeKernel(m_radius, m_amount);
        }

        /// <summary>
        /// Effectue le rendu de l'effet depth of field.
        /// Les render target utilisés doivent être en mode PreserveContent.
        /// <param name="bloomThreshold">Seuil de déclenchement du bloom.</param>
        /// <param name="srcRenderTarget">Render target source.</param>
        /// <param name="tempRenderTarget">Render target temporaire utilisé pour l'application de l'effet.</param>
        /// <param name="tempRenderTarget2">Deuxième render target temporaire.</param>
        /// </summary>
        public void ProcessDepthOfField(RenderTarget2D srcRenderTarget, RenderTarget2D tempRenderTarget, RenderTarget2D tempRenderTarget2, RenderTarget2D dstBuffer, 
            GameWorld gameWorld,
            Texture2D adaptedLuminance,
            Texture2D depthBuffer,
            SpriteBatch batch)
        {
            bool useHDR = gameWorld.GraphicalParameters.UseHDR;
            float globalIllumination = gameWorld.GetCurrentWorldLuminosity();
            // Précalcule le kernel pour le flou.
            if (m_needComputeKernel)
            {
                m_blurEffect.ComputeKernel(m_radius, m_amount);
                m_blurEffect.ComputeOffsets(tempRenderTarget2.Bounds.Width, tempRenderTarget2.Bounds.Height);
                m_needComputeKernel = false;
            }


            // Application du flou -> m_tmpRenderTarget3.
            Game1.Instance.GraphicsDevice.SetRenderTarget(tempRenderTarget2);
            Game1.Instance.GraphicsDevice.BlendState = BlendState.Opaque;
            Game1.Instance.GraphicsDevice.Clear(Color.White);
            Game1.Instance.GraphicsDevice.SetRenderTarget(tempRenderTarget);
            Game1.Instance.GraphicsDevice.BlendState = BlendState.Opaque;
            Game1.Instance.GraphicsDevice.Clear(Color.White);
            m_blurEffect.PerformGaussianBlur(srcRenderTarget, tempRenderTarget, tempRenderTarget2, batch);

  
            Game1.Instance.GraphicsDevice.SetRenderTarget(dstBuffer);
            m_combineEffect.Parameters["bloomTexture"].SetValue(tempRenderTarget2);
            m_combineEffect.Parameters["mapTexture"].SetValue(srcRenderTarget);
            m_combineEffect.Parameters["BloomPower"].SetValue(BlurPower);
            m_combineEffect.Parameters["MapPower"].SetValue(MapPower);
            m_combineEffect.Parameters["AdaptedLuminanceTexture"].SetValue(adaptedLuminance);
            m_combineEffect.Parameters["GlobalIllumination"].SetValue(globalIllumination);
            m_combineEffect.Parameters["UseHDR"].SetValue(useHDR);
            
            // Si le depth of field est activé, on effectue le paramétrage nécessaire.
            m_combineEffect.CurrentTechnique = m_combineEffect.Techniques["UseDepth"];

            // Détermination (auto ou pas) du point de focus.
            if (gameWorld.GraphicalParameters.FocusDepth < 0)
                m_autoFocusCalculationChain.CalculateFocus(depthBuffer);
            else
                m_autoFocusCalculationChain.SetFocus(gameWorld.GraphicalParameters.FocusDepth);

            Game1.Instance.GraphicsDevice.SetRenderTarget(dstBuffer);
            // Effectue le paramétrage du shader.
            m_combineEffect.Parameters["DepthBuffer"].SetValue(depthBuffer);
            m_combineEffect.Parameters["FocusPower"].SetValue(gameWorld.GraphicalParameters.FocusPower);
            m_combineEffect.Parameters["FocusDepth"].SetValue(m_autoFocusCalculationChain.AdaptedFocus);

            // Effectue la combinaison de tous les effets.
            Game1.Instance.GraphicsDevice.SetRenderTarget(dstBuffer);
            Game1.Instance.GraphicsDevice.BlendState = BlendState.Opaque;
            Game1.Instance.GraphicsDevice.Clear(Color.White);
            batch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, m_combineEffect);
            batch.Draw(srcRenderTarget, new Rectangle(0, 0,
                Game1.Instance.GraphicsDevice.ScissorRectangle.Width, Game1.Instance.GraphicsDevice.ScissorRectangle.Height), Color.White);
            batch.End();
            
        }
        #endregion


    }
}
