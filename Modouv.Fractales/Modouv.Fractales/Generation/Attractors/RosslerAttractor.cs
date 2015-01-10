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
namespace Modouv.Fractales.Generation.Attractors
{
    /// <summary>
    /// Attracteur de Rossler.
    /// </summary>
    public class RosslerAttractor
    {
        /// <summary>
        /// Position à la dernière étape.
        /// </summary>
        Vector3 m_currentPosition;
        /// <summary>
        /// Position de l'attracteur à la dernière étape.
        /// </summary>
        public Vector3 CurrentPosition
        {
            get { return m_currentPosition; }
        }

        float a = 0.2f;
        float b = 0.2f;
        float c = 5.7f;

        /// <summary>
        /// Crée une nouvelle instance de l'attracteur de Rossler.
        /// </summary>
        public RosslerAttractor()
        {
            m_currentPosition = Vector3.Zero;
        }

        /// <summary>
        /// Effectue une étape de l'attracteur.
        /// </summary>
        public void NextStep(float delta, int numberOfSteps)
        {
            // x. = -y -z
            // y. = x + ay
            // z. = b + z(x-c)
            for (int i = 0; i < numberOfSteps; i++)
            {
                float dx = -m_currentPosition.Y - m_currentPosition.Z;
                float dy = m_currentPosition.X + a * m_currentPosition.Y;
                float dz = b + m_currentPosition.Z * (m_currentPosition.X - c);

                m_currentPosition += new Vector3(dx * delta, dy * delta, dz * delta);
            }
        }

        
    }
}
