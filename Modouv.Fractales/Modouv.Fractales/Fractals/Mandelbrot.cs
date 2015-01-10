using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Test3D.Math;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Threading;
namespace Test3D.Fractals
{
    /// <summary>
    /// Classe permettant 
    /// </summary>
    public static class Mandelbrot
    {
        static Effect m_mandelbrotEffect;

        /// <summary>
        /// Charge les composants nécessaires à l'utilisation ultérieure de la classe.
        /// </summary>
        public static void Initialize()
        {
            m_mandelbrotEffect = Game1.Instance.Content.Load<Effect>("Shaders\\mandelbrot");
        }
        /// <summary>
        /// Génère la fractale de Mandelbrot à l'aide du GPU.
        /// </summary>
        /// <param name="c"></param>
        /// <param name="origin"></param>
        /// <param name="scale"></param>
        public static void GenerateTexture2DGPU(Complex c, Complex origin, float scale, RenderTarget2D renderTexture)
        {
            int width = renderTexture.Width;
            int height = renderTexture.Height;

            // Passe les paramètres de la fractale au pixel shader.
            m_mandelbrotEffect.Parameters["width"].SetValue(width);
            m_mandelbrotEffect.Parameters["height"].SetValue(height);
            m_mandelbrotEffect.Parameters["scale"].SetValue(scale);
            m_mandelbrotEffect.Parameters["origin"].SetValue(new float[] { origin.Real, origin.Imaginary });
            m_mandelbrotEffect.Parameters["MatrixTransform"].SetValue(Game1.Instance.PlaneTransform2D);
            m_mandelbrotEffect.Parameters["colorParam"].SetValue(new float[] { c.Real, c.Imaginary });
            // Crée un render target sur lequel dessiner la fractale, et dessine dessus en utilisant l'effet.
            Game1.Instance.GraphicsDevice.SetRenderTarget(renderTexture);
            SpriteBatch batch = new SpriteBatch(Game1.Instance.GraphicsDevice);
            batch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            m_mandelbrotEffect.CurrentTechnique.Passes[0].Apply();
            batch.Draw(Game1.Instance.DummyTexture, new Rectangle(0, 0, width, height), Color.White);
            batch.End();
            Game1.Instance.GraphicsDevice.SetRenderTarget(null);
        }
    }
}
