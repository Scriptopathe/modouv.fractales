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
namespace Modouv.Fractales.World.Postprocess
{
    /// <summary>
    /// Classe permettant de calculer la luminosité adaptée de la scène.
    /// </summary>
    public class LuminanceCalculationChain
    {
        #region Variable
        /// <summary>
        /// RenderTarget permettant le calcul de la luminance de la scène.
        /// 
        /// Le dernier élément indique la luminance actuelle de la scène.
        /// </summary>
        RenderTarget2D[] m_luminanceChain;
        RenderTarget2D m_lastFrameAdaptedLuminance;
        RenderTarget2D m_currentAdaptedLuminance;
        RenderTarget2D m_currentLuminance;

        Effect m_luminanceCalculationEffect;
        Effect m_adaptedLuminanceCalculationEffect;
        #endregion

        #region Properties
        /// <summary>
        /// Obtient une texture contenant la luminance adaptée de la scène.
        /// </summary>
        public RenderTarget2D AdaptedLuminance
        {
            get { return m_currentAdaptedLuminance; }
        }
        public RenderTarget2D[] MipChain
        {
            get { return m_luminanceChain; }
        }

        public Effect TEST
        { get { return m_luminanceCalculationEffect; } }
        #endregion
        /// <summary>
        /// Initialise une nouvelle instance de LuminanceCalculation.
        /// </summary>
        public LuminanceCalculationChain(Point resolution)
        {
            // Chargement des effets
            m_luminanceCalculationEffect = Game1.Instance.Content.Load<Effect>("Shaders\\postprocess\\LuminanceCalc");
            m_adaptedLuminanceCalculationEffect = Game1.Instance.Content.Load<Effect>("Shaders\\postprocess\\AdaptedLuminanceCalc");

            // Création de la mip chain
            Point currentResolution = resolution;
            int chainSize = (int)Math.Log(Math.Max(resolution.X, resolution.Y), 2) + 1;

            // Chaine de luminance.
            m_luminanceChain = new RenderTarget2D[chainSize];
            for(int i = 0; i < chainSize-1; i++)
            {
                // Crée le render target
                m_luminanceChain[i] = new RenderTarget2D(Game1.Instance.GraphicsDevice, currentResolution.X, currentResolution.Y, true, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);

                // Divise la résolution par deux.
                currentResolution = new Point(currentResolution.X / 2, currentResolution.Y / 2);
            }
            m_luminanceChain[chainSize - 1] = new RenderTarget2D(Game1.Instance.GraphicsDevice, 1, 1, true, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            // La luminance calculée sera celle de la dernière texture de la mipchain.
            m_currentLuminance = m_luminanceChain[chainSize - 1];

            // Crée les deux render target qui correspondent à la luminosité adaptée de la frame actuelle ainsi que de la précédente.
            m_currentAdaptedLuminance = new RenderTarget2D(Game1.Instance.GraphicsDevice, 1, 1, true, SurfaceFormat.Color, DepthFormat.None);
            m_lastFrameAdaptedLuminance = new RenderTarget2D(Game1.Instance.GraphicsDevice, 1, 1, true, SurfaceFormat.Color, DepthFormat.None);

        }

        /// <summary>
        /// Calcule la luminance de la scène actuelle.
        /// </summary>
        public void CalculateLuminance(RenderTarget2D srcTexture)
        {
            // Swape les données de luminance des frames précédentes et actuelle.
            var tmp = m_lastFrameAdaptedLuminance;
            m_lastFrameAdaptedLuminance = m_currentAdaptedLuminance;
            m_currentAdaptedLuminance = tmp;

            // Calcul de la luminance de chaque pixel de la scène.
            Game1.Instance.GraphicsDevice.SetRenderTarget(m_luminanceChain[0]);
            m_luminanceCalculationEffect.CurrentTechnique = m_luminanceCalculationEffect.Techniques["ComputeLuminance"];
            m_luminanceCalculationEffect.Parameters["PreviousMipLevel"].SetValue(srcTexture);

            Game1.Instance.Batch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, m_luminanceCalculationEffect);
            Game1.Instance.Batch.Draw(srcTexture, new Rectangle(0, 0, m_luminanceChain[0].Width, m_luminanceChain[0].Height), Color.White);
            Game1.Instance.Batch.End();

            // Calcul de la luminance de la scène.
            int length = m_luminanceChain.Length;
            for (int i = 1; i < length; i++)
            {
                Game1.Instance.GraphicsDevice.SetRenderTarget(m_luminanceChain[i]);
                m_luminanceCalculationEffect.CurrentTechnique = m_luminanceCalculationEffect.Techniques["DownscaleBilinear"];
                m_luminanceCalculationEffect.Parameters["PreviousMipLevel"].SetValue(m_luminanceChain[i-1]);
                Game1.Instance.Batch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, m_luminanceCalculationEffect);
                Game1.Instance.Batch.Draw(Game1.Instance.DummyTexture, new Rectangle(0, 0, m_luminanceChain[i].Width, m_luminanceChain[i].Height), Color.White);
                Game1.Instance.Batch.End();
            }

            // Calcul de la luminance adaptée de la scène.
            Game1.Instance.GraphicsDevice.SetRenderTarget(m_currentAdaptedLuminance);
            m_adaptedLuminanceCalculationEffect.Parameters["LastAdaptedLuminanceTexture"].SetValue(m_lastFrameAdaptedLuminance);
            m_adaptedLuminanceCalculationEffect.Parameters["CurrentLuminanceTexture"].SetValue(m_currentLuminance);
            m_adaptedLuminanceCalculationEffect.Parameters["Tau"].SetValue(0.5f);
            Game1.Instance.Batch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
            m_adaptedLuminanceCalculationEffect.CurrentTechnique.Passes[0].Apply();
            Game1.Instance.Batch.Draw(Game1.Instance.DummyTexture, new Rectangle(0, 0, m_currentAdaptedLuminance.Width, m_currentAdaptedLuminance.Height), Color.White);
            Game1.Instance.Batch.End();
        }
    }
}
