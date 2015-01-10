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
    public static class Julia
    {
        /// <summary>
        /// Effet contenant le pixel shader permettant de dessiner la fractale sur la carte graphique.
        /// </summary>
        static Effect m_juliaEffect;
        /// <summary>
        /// Charge les composants nécessaires à l'utilisation ultérieure de la classe.
        /// </summary>
        public static void Initialize()
        {
            m_juliaEffect = Game1.Instance.Content.Load<Effect>("Shaders\\julia");
        }

        /// <summary>
        /// Génère la fractale de Julia à l'aide du GPU.
        /// </summary>
        /// <param name="c"></param>
        /// <param name="origin"></param>
        /// <param name="scale"></param>
        public static void GenerateTexture2DGPU(Complex c, Complex origin, float scale, RenderTarget2D renderTexture)
        {
            int width = renderTexture.Width;
            int height = renderTexture.Height;

            // Passe les paramètres de la fractale au pixel shader.
            m_juliaEffect.Parameters["width"].SetValue(width);
            m_juliaEffect.Parameters["height"].SetValue(height);
            m_juliaEffect.Parameters["scale"].SetValue(scale);
            m_juliaEffect.Parameters["c"].SetValue(new float[] { c.Real, c.Imaginary });
            m_juliaEffect.Parameters["origin"].SetValue(new float[] { origin.Real, origin.Imaginary });
            m_juliaEffect.Parameters["MatrixTransform"].SetValue(Game1.Instance.PlaneTransform2D);

            // Crée un render target sur lequel dessiner la fractale, et dessine dessus en utilisant l'effet.
            Game1.Instance.GraphicsDevice.SetRenderTarget(renderTexture);
            SpriteBatch batch = new SpriteBatch(Game1.Instance.GraphicsDevice);
            batch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            m_juliaEffect.CurrentTechnique.Passes[0].Apply();
            batch.Draw(Game1.Instance.DummyTexture, new Rectangle(0, 0, width, height), Color.White);
            batch.End();
            Game1.Instance.GraphicsDevice.SetRenderTarget(null);
        }

        #region Versions CPU
        /// <summary>
        /// [Obsolète]
        /// Représente une fonction qui retourne une couleur en fonction d'une valeur de profondeur retournée par 
        /// la fractale.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public delegate Color IntValueToColor(int value);
        const int CORES = 4;
        public const int JULIA_MAX_DEPTH = 256;
        /// <summary>
        /// Remplit une texture contenant une fractale de Julia.
        /// Version optimisée et multithread.
        /// </summary>
        /// <param name="c">Condition initiale de génération de la fractale.</param>
        /// <param name="width">Largeur de la texture.</param>
        /// <param name="height">Hauteur de la texture.</param>
        /// <param name="origin">Origine (supérieure gauche) de l'intervalle de dessin de la texture.</param>
        /// <param name="scale">Echelle de dessin de la fractale.</param>
        /// <param name="func">Fonction retournant une couleur en fonction d'une valeur de profondeur de calcul de la fractale.</param>
        /// <param name="tex">La texture à remplir de la fractale.</param>
        /// <returns></returns>
        public static void GenerateJuliaTexture2DParallel(Complex c, int width, int height, Complex origin, float scale, IntValueToColor func, ref Texture2D tex)
        {
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            // On crée une texture de la taille demandée.
            if (tex == null || tex.Width != width || tex.Height != height)
                throw new Exception("La texture passée est nulle ou de taille incorrecte.");
            Color[] data = new Color[width * height];

            // On précalcule les couleurs possibles :
            Color[] colors = new Color[JULIA_MAX_DEPTH];
            for (int i = 0; i < JULIA_MAX_DEPTH; i++)
            {
                colors[i] = func(i);
            }

            // On remplit le tableau de couleur.
            object mutex = new object();
            ParameterizedThreadStart start = new ParameterizedThreadStart((object rectangle) => 
            {
                Complex z;
                int depth = 0;
                Rectangle rect = (Rectangle)rectangle;
                float scalex = scale / (float)width;
                float scaley = scale / (float)height;
                for (int x = rect.X; x < rect.Right; x++)
                {
                    float zx = scalex * (x + origin.Real);
                    for (int y = rect.Y; y < rect.Bottom; y++)
                    {
                        // Depth nous permet d'obtenir la couleur à l'aide de l'algorithme de génération de Julia.
                        z = new Complex(zx, scaley * (y + origin.Imaginary));
                        depth = 0;
                        while (z.SquaredModule <= 4 && depth < JULIA_MAX_DEPTH - 1)
                        {
                            z = z * z + c;
                            depth++;
                        }
                        // On appelle la fonction qui nous donne la couleur en fonction de la valeur depth qu'à donné le calcul de ce point
                        // de la fractale.
                        Color col = colors[depth];
                        lock (mutex)
                        {
                            data[y * width + x] = col;
                        }
                    }
                }
            });

            Thread[] threads = new Thread[CORES];
            for (int x = 0; x < CORES; x++)
            {
                threads[x] = new Thread(start);
                threads[x].Start(new Rectangle(x * (width / CORES), 0, width / CORES, height));
            }

            // On attend que tous les threads aient fini.
            bool allFinished = false;
            while (!allFinished)
            {
                allFinished = true;
                for (int x = 0; x < CORES; x++)
                {
                    if (threads[x].IsAlive)
                    {
                        allFinished = false;
                        break;
                    }
                }
                Thread.Sleep(5);
            }
            
            tex.SetData<Color>(data);
            watch.Stop();
        }
        /// <summary>
        /// Génère une texture contenant une fractale de Julia.
        /// </summary>
        /// <param name="c">Condition initiale de génération de la fractale.</param>
        /// <param name="width">Largeur de la texture.</param>
        /// <param name="height">Hauteur de la texture.</param>
        /// <param name="origin">Origine (supérieure gauche) de l'intervalle de dessin de la texture.</param>
        /// <param name="scale">Echelle de dessin de la fractale.</param>
        /// <param name="func">Fonction retournant une couleur en fonction d'une valeur de profondeur de calcul de la fractale.</param>
        /// <returns></returns>
        public static Texture2D GenerateJuliaTexture2D(Complex c, int width, int height, Complex origin, float scale, IntValueToColor func)
        {
            // On crée une texture de la taille demandée.
            Texture2D tex = new Texture2D(Game1.Instance.GraphicsDevice, width, height, true, SurfaceFormat.Color);
            Color[] data = new Color[width * height];

            // On remplit le tableau de couleur.
            Complex z;
            int depth = 0;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // Depth nous permet d'obtenir la couleur à l'aide de l'algorithme de génération de Julia.
                    z = new Complex(scale * (x + origin.Real) / (float)width, scale * (y + origin.Imaginary) / (float)height);
                    depth = 0;
                    while (z.Module <= 2)
                    {
                        z = z * z + c;
                        depth++;
                    }
                    // On appelle la fonction qui nous donne la couleur en fonction de la valeur depth qu'à donné le calcul de ce point
                    // de la fractale.
                    data[y * width + x] = func(depth);
                }
            }

            tex.SetData<Color>(data);
            return tex;
        }
        /// <summary>
        /// Génère une représentation 2D de la fractale de julia.
        /// </summary>
        /// <param name="real"></param>
        /// <param name="imaginary"></param>
        /// <returns></returns>
        public static int[,] GenerateJulia(Complex c, int width, int height, Complex origin, float scale)
        {
            Complex z;
            int depth = 0;
            int[,] result = new int[(int)width, (int)height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    z = new Complex(scale*(x+origin.Real)/(float)width, scale*(y+origin.Imaginary)/(float)height);
                    depth = 0;
                    while (z.Module <= 2)
                    {
                        z = z * z + c;
                        depth++;
                    }
                    result[x, y] = depth;
                }
            }
            return result;
        }

        public static int[,] GenerateJulia(Complex c, int width, int height)
        {
            return GenerateJulia(c, width, height, new Complex(-width / 2, -height / 2), 2.5f);
        }
        //public static Texture2D GenerateJuliaTexture(GraphicsDevice device, Point sizePx, Complex c, 
        #endregion
    }
}
