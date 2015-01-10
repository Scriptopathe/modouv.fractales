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
    /// Classe permettant de calculer le plan sur lequel doit être fait le focus grâce à un depth buffer.
    /// </summary>
    public class FocusCalculationChain
    {
        #region Variable
        /// <summary>
        /// RenderTarget permettant le calcul du plan focal sur lequel doit être fait le focus.
        /// 
        /// Le dernier élément indique la focus actuel de la scène.
        /// </summary>
        RenderTarget2D[] m_focusChain;
        RenderTarget2D m_lastFrameAdaptedFocus;
        RenderTarget2D m_currentAdaptedFocus;
        RenderTarget2D m_currentFocus;

        Effect m_luminanceCalculationEffect;
        Effect m_adaptedLuminanceCalculationEffect;
        #endregion

        #region Properties
        /// <summary>
        /// Obtient une texture contenant le focus adapté de la scène.
        /// </summary>
        public RenderTarget2D AdaptedFocus
        {
            get { return m_currentAdaptedFocus; }
        }
        public RenderTarget2D[] MipChain
        {
            get { return m_focusChain; }
        }

        public Effect TEST
        { get { return m_luminanceCalculationEffect; } }
        #endregion
        /// <summary>
        /// Initialise une nouvelle instance de LuminanceCalculation.
        /// </summary>
        public FocusCalculationChain(Point resolution)
        {
            // Chargement des effets
            m_luminanceCalculationEffect = Game1.Instance.Content.Load<Effect>("Shaders\\postprocess\\LuminanceCalc");
            m_adaptedLuminanceCalculationEffect = Game1.Instance.Content.Load<Effect>("Shaders\\postprocess\\AdaptedLuminanceCalc");

            // Création de la mip chain
            Point currentResolution = new Point(resolution.X/16, resolution.Y/16);
            int chainSize = (int)Math.Log(Math.Max(currentResolution.X, currentResolution.Y), 2) + 1;

            // Chaine de focus.
            m_focusChain = new RenderTarget2D[chainSize];
            for(int i = 0; i < chainSize-1; i++)
            {
                // Crée le render target
                m_focusChain[i] = new RenderTarget2D(Game1.Instance.GraphicsDevice, currentResolution.X, currentResolution.Y, true, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);

                // Divise la résolution par deux.
                currentResolution = new Point(currentResolution.X / 2, currentResolution.Y / 2);
            }
            m_focusChain[chainSize - 1] = new RenderTarget2D(Game1.Instance.GraphicsDevice, 1, 1, true, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            
            // Le focus calculé sera celui de la dernière texture de la mipchain.
            m_currentFocus = m_focusChain[chainSize - 1];

            // Crée les deux render target qui correspondent à la luminosité adaptée de la frame actuelle ainsi que de la précédente.
            m_currentAdaptedFocus = new RenderTarget2D(Game1.Instance.GraphicsDevice, 1, 1, true, SurfaceFormat.Color, DepthFormat.None);
            m_lastFrameAdaptedFocus = new RenderTarget2D(Game1.Instance.GraphicsDevice, 1, 1, true, SurfaceFormat.Color, DepthFormat.None);

        }

        /// <summary>
        /// Impose le focus donné.
        /// </summary>
        /// <param name="focusValue"></param>
        public void SetFocus(float focusValue)
        {
            Color clearColor = new Color(focusValue, focusValue, focusValue, 1);
            Game1.Instance.GraphicsDevice.SetRenderTarget(m_currentAdaptedFocus);
            Game1.Instance.GraphicsDevice.Clear(clearColor);
            Game1.Instance.GraphicsDevice.SetRenderTarget(m_lastFrameAdaptedFocus);
            Game1.Instance.GraphicsDevice.Clear(clearColor);
        }

        /// <summary>
        /// Calcule la luminance de la scène actuelle.
        /// </summary>
        public void CalculateFocus(Texture2D depthBuffer)
        {
            // Swape les données de luminance des frames précédentes et actuelle.
            var tmp = m_lastFrameAdaptedFocus;
            m_lastFrameAdaptedFocus = m_currentAdaptedFocus;
            m_currentAdaptedFocus = tmp;

            // Calcul de la luminance de chaque pixel de la scène.
            Game1.Instance.GraphicsDevice.SetRenderTarget(m_focusChain[0]);
            m_luminanceCalculationEffect.CurrentTechnique = m_luminanceCalculationEffect.Techniques["ComputeLuminance"];
            m_luminanceCalculationEffect.Parameters["PreviousMipLevel"].SetValue(depthBuffer);

            // On dessine la partie la plus au centre des 1/16 du depth buffer.
            Point srcSize = new Point(depthBuffer.Width, depthBuffer.Height);
            Point dstSize = new Point(m_focusChain[0].Width, m_focusChain[0].Height);
            Game1.Instance.Batch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, m_luminanceCalculationEffect);
            Game1.Instance.Batch.Draw(depthBuffer, 
                new Rectangle(0, 0, dstSize.X, dstSize.Y),
                new Rectangle((srcSize.X - dstSize.X)/2, (srcSize.Y - dstSize.Y)/2, dstSize.X, dstSize.Y),
                Color.White);
            Game1.Instance.Batch.End();

            // Calcul de la luminance de la scène.
            int length = m_focusChain.Length;
            for (int i = 1; i < length; i++)
            {
                Game1.Instance.GraphicsDevice.SetRenderTarget(m_focusChain[i]);
                m_luminanceCalculationEffect.CurrentTechnique = m_luminanceCalculationEffect.Techniques["DownscaleBilinear"];
                m_luminanceCalculationEffect.Parameters["PreviousMipLevel"].SetValue(m_focusChain[i-1]);
                
                Game1.Instance.Batch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, m_luminanceCalculationEffect);
                Game1.Instance.Batch.Draw(Game1.Instance.DummyTexture, new Rectangle(0, 0, m_focusChain[i].Width, m_focusChain[i].Height), Color.White);
                Game1.Instance.Batch.End();
            }

            // Calcul de la luminance adaptée de la scène.
            Game1.Instance.GraphicsDevice.SetRenderTarget(m_currentAdaptedFocus);
            m_adaptedLuminanceCalculationEffect.Parameters["LastAdaptedLuminanceTexture"].SetValue(m_lastFrameAdaptedFocus);
            m_adaptedLuminanceCalculationEffect.Parameters["CurrentLuminanceTexture"].SetValue(m_currentFocus);
            m_adaptedLuminanceCalculationEffect.Parameters["Tau"].SetValue(0.5f);

            Game1.Instance.Batch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
            m_adaptedLuminanceCalculationEffect.CurrentTechnique.Passes[0].Apply();
            Game1.Instance.Batch.Draw(Game1.Instance.DummyTexture, new Rectangle(0, 0, m_currentAdaptedFocus.Width, m_currentAdaptedFocus.Height), Color.White);
            Game1.Instance.Batch.End();
        }
    }
}
