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
    public class Bloom
    {
        #region Variables
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
        public int BloomRadius
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
        public float BloomAmount
        {
            get { return m_amount; }
            set { 
                m_amount = value;
                m_needComputeKernel = true;
                m_amount = Math.Min(10, Math.Max(1, m_amount));
            }
        }
        /// <summary>
        /// Seuil de déclenchement du bloom.
        /// </summary>
        public float BloomEffectThreshold
        {
            get { return m_bloomEffectThreshold; }
            set { 
                m_bloomEffectThreshold = value;
                m_bloomEffectThreshold = Math.Min(1, Math.Max(0, m_bloomEffectThreshold));
            }
        }
        /// <summary>
        /// Définit la valeur max de luminosité pour le tone mapping.
        /// </summary>
        public float MaxLuminance
        {
            get;
            set;
        }

        
        /// <summary>
        /// Détermine le coefficient de mélange de la couleur du bloom lors de l'effet de bloom.
        /// </summary>
        public float BloomPower
        {
            get;
            set;
        }
        /// <summary>
        /// Détermine le coefficient de mélange de la couleur de la map lors de l'effet de bloom.
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
        public Bloom()
        {
            m_bloomExtractEffect = Game1.Instance.Content.Load<Effect>("Shaders\\postprocess\\BloomExtract");
            m_combineEffect = Game1.Instance.Content.Load<Effect>("Shaders\\postprocess\\Combine");
            m_tmpRenderTarget3 = new RenderTarget2D(Game1.Instance.GraphicsDevice, Game1.Instance.ResolutionWidth, Game1.Instance.ResolutionHeight,
                true, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.PreserveContents);
            m_blurEffect = new GaussianBlur(Game1.Instance);
            BloomEffectThreshold = 0.100f;
            BloomRadius = 1;
            BloomAmount = 10;
            MaxLuminance = 6.254f;
            MapPower = 1;
            BloomPower = 1.045f;
            // Ceci est magique, mais ne sert à rien.
            m_blurEffect.ComputeKernel(0, m_amount);
            m_blurEffect.ComputeKernel(m_radius, m_amount);
        }


        /// <summary>
        /// Effectue le rendu du bloom.
        /// Les render target utilisés doivent être en mode PreserveContent.
        /// <param name="bloomThreshold">Seuil de déclenchement du bloom.</param>
        /// <param name="srcRenderTarget">Render target source.</param>
        /// <param name="tempRenderTarget">Render target temporaire utilisé pour l'application de l'effet.</param>
        /// <param name="tempRenderTarget2">Deuxième render target temporaire.</param>
        /// </summary>
        public void ProcessBloom(RenderTarget2D srcRenderTarget, RenderTarget2D tempRenderTarget, RenderTarget2D tempRenderTarget2, RenderTarget2D dstBuffer, 
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
                m_blurEffect.ComputeOffsets(m_tmpRenderTarget3.Bounds.Width, m_tmpRenderTarget3.Bounds.Height);
                m_needComputeKernel = false;
            }

            // Nettoie le render target temporaire.
            Game1.Instance.GraphicsDevice.SetRenderTarget(tempRenderTarget);
            Game1.Instance.GraphicsDevice.BlendState = BlendState.Opaque;
            Game1.Instance.GraphicsDevice.Clear(Color.White);

            // Extraction des points lumineux de la source -> dstRenderTarget
            Game1.Instance.GraphicsDevice.SetRenderTarget(tempRenderTarget2);
            Game1.Instance.GraphicsDevice.BlendState = BlendState.Opaque;
            Game1.Instance.GraphicsDevice.Clear(Color.White);
            m_bloomExtractEffect.Parameters["threshold"].SetValue(m_bloomEffectThreshold);
            batch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, m_bloomExtractEffect);
            batch.Draw(srcRenderTarget, new Rectangle(0, 0, tempRenderTarget2.Width, tempRenderTarget2.Height), Color.White);
            batch.End();

            // Application du flou -> m_tmpRenderTarget3.
            Game1.Instance.GraphicsDevice.SetRenderTarget(m_tmpRenderTarget3);
            Game1.Instance.GraphicsDevice.BlendState = BlendState.Opaque;
            Game1.Instance.GraphicsDevice.Clear(Color.White);
            m_blurEffect.PerformGaussianBlur(tempRenderTarget2, tempRenderTarget, m_tmpRenderTarget3, batch);

            Game1.Instance.GraphicsDevice.SetRenderTarget(dstBuffer);
            m_combineEffect.Parameters["bloomTexture"].SetValue(m_tmpRenderTarget3);
            m_combineEffect.Parameters["mapTexture"].SetValue(srcRenderTarget);
            m_combineEffect.Parameters["BloomPower"].SetValue(BloomPower);
            m_combineEffect.Parameters["MapPower"].SetValue(MapPower);
            m_combineEffect.Parameters["AdaptedLuminanceTexture"].SetValue(adaptedLuminance);
            m_combineEffect.Parameters["GlobalIllumination"].SetValue(globalIllumination);
            m_combineEffect.Parameters["UseHDR"].SetValue(useHDR);
            m_combineEffect.Parameters["MaxLuminance"].SetValue(MaxLuminance);
            // Si le depth of field est activé, on effectue le paramétrage nécessaire.
            m_combineEffect.CurrentTechnique = m_combineEffect.Techniques["Basic"];


            // Combinaison de dstRenderTarget et srcRenderTarget.
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
