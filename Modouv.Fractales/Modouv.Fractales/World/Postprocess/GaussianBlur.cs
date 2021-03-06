﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Modouv.Fractales.World.Postprocess
{
    /// <summary>
    /// A Gaussian blur filter kernel class. A Gaussian blur filter kernel is
    /// perfectly symmetrical and linearly separable. This means we can split
    /// the full 2D filter kernel matrix into two smaller horizontal and
    /// vertical 1D filter kernel matrices and then perform the Gaussian blur
    /// in two passes. Contrary to what you might think performing the Gaussian
    /// blur in this way is actually faster than performing the Gaussian blur
    /// in a single pass using the full 2D filter kernel matrix.
    /// <para>
    /// The GaussianBlur class is intended to be used in conjunction with an
    /// HLSL Gaussian blur shader. The following code snippet shows a typical
    /// Effect file implementation of a Gaussian blur.
    /// <code>
    /// #define RADIUS  7
    /// #define KERNEL_SIZE (RADIUS * 2 + 1)
    ///
    /// float weights[KERNEL_SIZE];
    /// float2 offsets[KERNEL_SIZE];
    ///
    /// texture colorMapTexture;
    ///
    /// sampler2D colorMap = sampler_state
    /// {
    ///     Texture = <![CDATA[<colorMapTexture>;]]>
    ///     MipFilter = Linear;
    ///     MinFilter = Linear;
    ///     MagFilter = Linear;
    /// };
    ///
    /// float4 PS_GaussianBlur(float2 texCoord : TEXCOORD) : COLOR0
    /// {
    ///     float4 color = float4(0.0f, 0.0f, 0.0f, 0.0f);
    ///
    ///     <![CDATA[for (int i = 0; i < KERNEL_SIZE; ++i)]]>
    ///         color += tex2D(colorMap, texCoord + offsets[i]) * weights[i];
    /// 
    ///     return color;
    /// }
    /// 
    /// technique GaussianBlur
    /// {
    ///     pass
    ///     {
    ///         PixelShader = compile ps_2_0 PS_GaussianBlur();
    ///     }
    /// }
    /// </code>
    /// The RADIUS constant in the effect file must match the radius value in
    /// the GaussianBlur class. The effect file's weights global variable
    /// corresponds to the GaussianBlur class' kernel field. The effect file's
    /// offsets global variable corresponds to the GaussianBlur class'
    /// offsetsHoriz and offsetsVert fields.
    /// </para>
    /// </summary>
    public class GaussianBlur
    {
        private Game game;
        private Effect effect;
        private int radius;
        private float amount;
        private float sigma;
        private float[] kernel;
        private Vector2[] offsetsHoriz;
        private Vector2[] offsetsVert;

        /// <summary>
        /// Returns the radius of the Gaussian blur filter kernel in pixels.
        /// </summary>
        public int Radius
        {
            get { return radius; }
        }

        /// <summary>
        /// Returns the blur amount. This value is used to calculate the
        /// Gaussian blur filter kernel's sigma value. Good values for this
        /// property are 2 and 3. 2 will give a more blurred result whilst 3
        /// will give a less blurred result with sharper details.
        /// </summary>
        public float Amount
        {
            get { return amount; }
        }

        /// <summary>
        /// Returns the Gaussian blur filter's standard deviation.
        /// </summary>
        public float Sigma
        {
            get { return sigma; }
        }

        /// <summary>
        /// Returns the Gaussian blur filter kernel matrix. Note that the
        /// kernel returned is for a 1D Gaussian blur filter kernel matrix
        /// intended to be used in a two pass Gaussian blur operation.
        /// </summary>
        public float[] Kernel
        {
            get { return kernel; }
        }

        /// <summary>
        /// Returns the texture offsets used for the horizontal Gaussian blur
        /// pass.
        /// </summary>
        public Vector2[] TextureOffsetsX
        {
            get { return offsetsHoriz; }
        }

        /// <summary>
        /// Returns the texture offsets used for the vertical Gaussian blur
        /// pass.
        /// </summary>
        public Vector2[] TextureOffsetsY
        {
            get { return offsetsVert; }
        }

        /// <summary>
        /// Default constructor for the GaussianBlur class. This constructor
        /// should be called if you don't want the GaussianBlur class to use
        /// its GaussianBlur.fx effect file to perform the two pass Gaussian
        /// blur operation.
        /// </summary>
        public GaussianBlur()
        {
        }

        /// <summary>
        /// This overloaded constructor instructs the GaussianBlur class to
        /// load and use its GaussianBlur.fx effect file that implements the
        /// two pass Gaussian blur operation on the GPU. The effect file must
        /// be already bound to the asset name: 'Effects\GaussianBlur' or
        /// 'GaussianBlur'.
        /// </summary>
        public GaussianBlur(Game game)
        {
            this.game = game;

            effect = game.Content.Load<Effect>(@"Shaders\postprocess\RadialBlur").Clone();

            effect.Parameters["MatrixTransform"].SetValue(Game1.Instance.PlaneTransform2D);

        }

        /// <summary>
        /// Calculates the Gaussian blur filter kernel. This implementation is
        /// ported from the original Java code appearing in chapter 16 of
        /// "Filthy Rich Clients: Developing Animated and Graphical Effects for
        /// Desktop Java".
        /// </summary>
        /// <param name="blurRadius">The blur radius in pixels.</param>
        /// <param name="blurAmount">Used to calculate sigma.</param>
        public void ComputeKernel(int blurRadius, float blurAmount)
        {
            radius = blurRadius;
            amount = blurAmount;

            kernel = null;
            kernel = new float[radius * 2 + 1];
            sigma = radius / amount;

            float twoSigmaSquare = 2.0f * sigma * sigma;
            float sigmaRoot = (float)Math.Sqrt(twoSigmaSquare * Math.PI);
            float total = 0.0f;
            float distance = 0.0f;
            int index = 0;

            for (int i = -radius; i <= radius; ++i)
            {
                distance = i * i;
                index = i + radius;
                kernel[index] = (float)Math.Exp(-distance / twoSigmaSquare) / sigmaRoot;
                total += kernel[index];
            }

            for (int i = 0; i < kernel.Length; ++i)
                kernel[i] /= total;
        }

        /// <summary>
        /// Calculates the texture coordinate offsets corresponding to the
        /// calculated Gaussian blur filter kernel. Each of these offset values
        /// are added to the current pixel's texture coordinates in order to
        /// obtain the neighboring texture coordinates that are affected by the
        /// Gaussian blur filter kernel. This implementation has been adapted
        /// from chapter 17 of "Filthy Rich Clients: Developing Animated and
        /// Graphical Effects for Desktop Java".
        /// </summary>
        /// <param name="textureWidth">The texture width in pixels.</param>
        /// <param name="textureHeight">The texture height in pixels.</param>
        public void ComputeOffsets(float textureWidth, float textureHeight)
        {
            offsetsHoriz = null;
            offsetsHoriz = new Vector2[radius * 2 + 1];

            offsetsVert = null;
            offsetsVert = new Vector2[radius * 2 + 1];

            int index = 0;
            float xOffset = 1.0f / textureWidth;
            float yOffset = 1.0f / textureHeight;

            for (int i = -radius; i <= radius; ++i)
            {
                index = i + radius;
                offsetsHoriz[index] = new Vector2(i * xOffset, 0.0f);
                offsetsVert[index] = new Vector2(0.0f, i * yOffset);
            }
        }

        /// <summary>
        /// Performs the Gaussian blur operation on the source texture image.
        /// The Gaussian blur is performed in two passes: a horizontal blur
        /// pass followed by a vertical blur pass. The output from the first
        /// pass is rendered to renderTarget1. The output from the second pass
        /// is rendered to renderTarget2. The dimensions of the blurred texture
        /// is therefore equal to the dimensions of renderTarget2.
        /// </summary>
        /// <param name="srcTexture">The source image to blur.</param>
        /// <param name="renderTargetTmp">Stores the output from the horizontal blur pass.</param>
        /// <param name="renderTargetDst">Stores the output from the vertical blur pass.</param>
        /// <param name="spriteBatch">Used to draw quads for the blur passes.</param>
        /// <returns>The resulting Gaussian blurred image.</returns>
        public void PerformGaussianBlur(Texture2D srcTexture,
                                             RenderTarget2D renderTargetTmp,
                                             RenderTarget2D renderTargetDst,
                                             SpriteBatch spriteBatch)
        {
            if (effect == null)
                throw new InvalidOperationException("GaussianBlur.fx effect not loaded.");

            Texture2D outputTexture = null;
            Rectangle srcRect = new Rectangle(0, 0, srcTexture.Width, srcTexture.Height);
            Rectangle destRect1 = new Rectangle(0, 0, renderTargetTmp.Width, renderTargetTmp.Height);
            Rectangle destRect2 = new Rectangle(0, 0, renderTargetDst.Width, renderTargetDst.Height);

            // Horizontal
  
            game.GraphicsDevice.SetRenderTarget(renderTargetTmp);
            effect.CurrentTechnique = effect.Techniques["Blur"];
            effect.Parameters["weights"].SetValue(kernel);
            effect.Parameters["offsets"].SetValue(offsetsHoriz);
            effect.Parameters["Texture"].SetValue(srcTexture);
            effect.Parameters["KernelSize"].SetValue(2 * radius + 1);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
            effect.CurrentTechnique.Passes[0].Apply();
            spriteBatch.Draw(srcTexture, destRect1, Color.White);
            spriteBatch.End();

            // Vertical

            game.GraphicsDevice.SetRenderTarget(renderTargetDst);
            outputTexture = (Texture2D)renderTargetTmp;

            effect.Parameters["Texture"].SetValue(renderTargetTmp);
            effect.Parameters["offsets"].SetValue(offsetsVert);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
            effect.CurrentTechnique.Passes[0].Apply();
            spriteBatch.Draw(renderTargetTmp, destRect2, Color.White);
            spriteBatch.End();
        }

    }
}