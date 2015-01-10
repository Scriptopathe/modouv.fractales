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
using Modouv.Fractales.MathHelpers;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
namespace Modouv.Fractales.World
{
    /// <summary>
    /// Structure contenant un VertexBuffer et un IndexBuffer.
    /// </summary>
    public class ModelData
    {
        VertexBuffer m_vertices;
        IndexBuffer m_indices;
        public VertexBuffer Vertices
        {
            get { return m_vertices; }
            private set { m_vertices = value; }
        }

        public IndexBuffer Indices
        {
            get { return m_indices; }
            private set { m_indices = value; }
        }

        /// <summary>
        /// Crée un ModelData avec les VertexBuffer et IndexBuffer spécifiés.
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="indices"></param>
        public ModelData(VertexBuffer vertices, IndexBuffer indices)
        {
            Vertices = vertices;
            Indices = indices;
        }
    }

    /// <summary>
    /// Structure contenant un Index Buffer et un Vertex Buffer sous forme directement accessible.
    /// </summary>
    public class PrimitiveData
    {
        Generation.ModelGenerator.VertexPositionColorNormalTexture[] m_vertices;
        int[] m_indices;
        public Generation.ModelGenerator.VertexPositionColorNormalTexture[] Vertices
        {
            get { return m_vertices; }
            private set { m_vertices = value; }
        }

        public int[]  Indices
        {
            get { return m_indices; }
            private set { m_indices = value; }
        }

        /// <summary>
        /// Crée un ModelData avec les VertexBuffer et IndexBuffer spécifiés.
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="indices"></param>
        public PrimitiveData(Generation.ModelGenerator.VertexPositionColorNormalTexture[] vertices, int[]  indices)
        {
            Vertices = vertices;
            Indices = indices;
        }
    }
}
