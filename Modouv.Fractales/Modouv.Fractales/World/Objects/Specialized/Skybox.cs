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
namespace Modouv.Fractales.World.Objects.Specialized
{
    /// <summary>
    /// Permet la création et le dessin d'une skybox.
    /// </summary>
    public class Skybox
    {
        private VertexBuffer m_buffer;
        private TextureCube m_skyBoxTextureNight;
        private TextureCube m_skyBoxTextureDay;
        private Effect m_shader;
        private float m_size = 2000;
        /// <summary>
        /// Crée et retourne les vertices permettant de créer le cube.
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        VertexBuffer CreateCube(Vector3 size)
        {
            Vector3 position = Vector3.Zero;
            VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[36];

            Vector3 topLeftFront = position + new Vector3(-1.0f, 1.0f, -1.0f) * size;
            Vector3 topLeftBack = position + new Vector3(-1.0f, 1.0f, 1.0f) * size;
            Vector3 topRightFront = position + new Vector3(1.0f, 1.0f, -1.0f) * size;
            Vector3 topRightBack = position + new Vector3(1.0f, 1.0f, 1.0f) * size;

            Vector3 btmLeftFront = position + new Vector3(-1.0f, -1.0f, -1.0f) * size;
            Vector3 btmLeftBack = position + new Vector3(-1.0f, -1.0f, 1.0f) * size;
            Vector3 btmRightFront = position + new Vector3(1.0f, -1.0f, -1.0f) * size;
            Vector3 btmRightBack = position + new Vector3(1.0f, -1.0f, 1.0f) * size;

            Vector3 normalFront = new Vector3(0.0f, 0.0f, 1.0f) * size;
            Vector3 normalBack = new Vector3(0.0f, 0.0f, -1.0f) * size;
            Vector3 normalTop = new Vector3(0.0f, 1.0f, 0.0f) * size;
            Vector3 normalBottom = new Vector3(0.0f, -1.0f, 0.0f) * size;
            Vector3 normalLeft = new Vector3(-1.0f, 0.0f, 0.0f) * size;
            Vector3 normalRight = new Vector3(1.0f, 0.0f, 0.0f) * size;


            Vector2 textureTopLeft = new Vector2(1.0f * size.X, 0.0f * size.Y);
            Vector2 textureTopRight = new Vector2(0.0f * size.X, 0.0f * size.Y);
            Vector2 textureBottomLeft = new Vector2(1.0f * size.X, 1.0f * size.Y);
            Vector2 textureBottomRight = new Vector2(0.0f * size.X, 1.0f * size.Y);

            vertices[0] = new VertexPositionNormalTexture(topLeftFront, normalFront, textureTopLeft);
            vertices[1] = new VertexPositionNormalTexture(btmLeftFront, normalFront, textureBottomLeft);
            vertices[2] = new VertexPositionNormalTexture(topRightFront, normalFront, textureTopRight);
            vertices[3] = new VertexPositionNormalTexture(btmLeftFront, normalFront, textureBottomLeft);
            vertices[4] = new VertexPositionNormalTexture(btmRightFront, normalFront, textureBottomRight);
            vertices[5] = new VertexPositionNormalTexture(topRightFront, normalFront, textureTopRight);

            vertices[6] = new VertexPositionNormalTexture(topLeftBack, normalBack, textureTopRight);
            vertices[7] = new VertexPositionNormalTexture(topRightBack, normalBack, textureTopLeft);
            vertices[8] = new VertexPositionNormalTexture(btmLeftBack, normalBack, textureBottomRight);
            vertices[9] = new VertexPositionNormalTexture(btmLeftBack, normalBack, textureBottomRight);
            vertices[10] = new VertexPositionNormalTexture(topRightBack, normalBack, textureTopLeft);
            vertices[11] = new VertexPositionNormalTexture(btmRightBack, normalBack, textureBottomLeft);

            vertices[12] = new VertexPositionNormalTexture(topLeftFront, normalTop, textureBottomLeft);
            vertices[13] = new VertexPositionNormalTexture(topRightBack, normalTop, textureTopRight);
            vertices[14] = new VertexPositionNormalTexture(topLeftBack, normalTop, textureTopLeft);
            vertices[15] = new VertexPositionNormalTexture(topLeftFront, normalTop, textureBottomLeft);
            vertices[16] = new VertexPositionNormalTexture(topRightFront, normalTop, textureBottomRight);
            vertices[17] = new VertexPositionNormalTexture(topRightBack, normalTop, textureTopRight);

            vertices[18] = new VertexPositionNormalTexture(btmLeftFront, normalBottom, textureTopLeft);
            vertices[19] = new VertexPositionNormalTexture(btmLeftBack, normalBottom, textureBottomLeft);
            vertices[20] = new VertexPositionNormalTexture(btmRightBack, normalBottom, textureBottomRight);
            vertices[21] = new VertexPositionNormalTexture(btmLeftFront, normalBottom, textureTopLeft);
            vertices[22] = new VertexPositionNormalTexture(btmRightBack, normalBottom, textureBottomRight);
            vertices[23] = new VertexPositionNormalTexture(btmRightFront, normalBottom, textureTopRight);

            vertices[24] = new VertexPositionNormalTexture(topLeftFront, normalLeft, textureTopRight);
            vertices[25] = new VertexPositionNormalTexture(btmLeftBack, normalLeft, textureBottomLeft);
            vertices[26] = new VertexPositionNormalTexture(btmLeftFront, normalLeft, textureBottomRight);
            vertices[27] = new VertexPositionNormalTexture(topLeftBack, normalLeft, textureTopLeft);
            vertices[28] = new VertexPositionNormalTexture(btmLeftBack, normalLeft, textureBottomLeft);
            vertices[29] = new VertexPositionNormalTexture(topLeftFront, normalLeft, textureTopRight);

            vertices[30] = new VertexPositionNormalTexture(topRightFront, normalRight, textureTopLeft);
            vertices[31] = new VertexPositionNormalTexture(btmRightFront, normalRight, textureBottomLeft);
            vertices[32] = new VertexPositionNormalTexture(btmRightBack, normalRight, textureBottomRight);
            vertices[33] = new VertexPositionNormalTexture(topRightBack, normalRight, textureTopRight);
            vertices[34] = new VertexPositionNormalTexture(topRightFront, normalRight, textureTopLeft);
            vertices[35] = new VertexPositionNormalTexture(btmRightBack, normalRight, textureBottomRight);

            VertexBuffer buffer = new VertexBuffer(Game1.Instance.GraphicsDevice, VertexPositionNormalTexture.VertexDeclaration, 36, BufferUsage.None);
            buffer.SetData<VertexPositionNormalTexture>(vertices);
            return buffer;
        }
        /// <summary>
        /// Crée une nouvelle instance de Skybox.
        /// </summary>
        public Skybox()
        {
            var Content = Game1.Instance.Content;
            m_buffer = CreateCube(new Vector3(1, 1, 1) * 1);
            m_skyBoxTextureNight = Content.Load<TextureCube>("textures\\world_fantasy\\skybox\\SkyBoxNight");
            m_skyBoxTextureDay = Content.Load<TextureCube>("textures\\world_fantasy\\skybox\\SkyBox");
            m_shader = Content.Load<Effect>("Shaders\\world_fantasy\\skybox");
        }

        float yaw = 0.0f;
        float pitch = 0.0f;
        float roll = 0.0f;
        /// <summary>
        /// Dessine la skybox.
        /// </summary>
        public void Draw(GameWorld world, bool reflection)
        {
            if (Input.IsGamepadPressed(Microsoft.Xna.Framework.Input.Buttons.Y))
                yaw += 0.01f;
            if (Input.IsGamepadPressed(Microsoft.Xna.Framework.Input.Buttons.X))
                pitch += 0.01f;
            if (Input.IsGamepadPressed(Microsoft.Xna.Framework.Input.Buttons.RightTrigger))
                roll += 0.01f;
            Game1.Instance.GraphicsDevice.DepthStencilState = DepthStencilState.None;
            Game1.Instance.GraphicsDevice.BlendState = BlendState.Opaque;
            Game1.Instance.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            m_shader.CurrentTechnique = m_shader.Techniques["Skybox" + (reflection ? "" : "Double")];
            m_shader.Parameters["xWorld"].SetValue(Matrix.CreateFromYawPitchRoll(yaw, pitch, roll) 
                * Matrix.CreateScale(m_size) * Matrix.CreateTranslation(world.Camera.Position));
            m_shader.Parameters["xView"].SetValue(world.View);
            m_shader.Parameters["xProjection"].SetValue(world.Projection);
            m_shader.Parameters["SkyBoxTextureDay"].SetValue(m_skyBoxTextureDay);
            m_shader.Parameters["SkyBoxTextureNight"].SetValue(m_skyBoxTextureNight);
            m_shader.Parameters["xCameraPosition"].SetValue(world.Camera.Position);
            m_shader.Parameters["xGlobalIllumination"].SetValue(world.GetCurrentWorldLuminosity());
            m_shader.Parameters["xFogEnabled"].SetValue(world.GraphicalParameters.FogEnabled);
            m_shader.Parameters["xFogColor"].SetValue(world.GraphicalParameters.FogColor);
            m_shader.CurrentTechnique.Passes[0].Apply();
            Game1.Instance.GraphicsDevice.SetVertexBuffer(m_buffer);
            Game1.Instance.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, m_buffer.VertexCount);
            Game1.Instance.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        }
    }
}
