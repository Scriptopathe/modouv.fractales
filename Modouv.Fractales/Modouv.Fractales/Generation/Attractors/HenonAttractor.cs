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
    /// Attracteur de Henon.
    /// </summary>
    public class HenonAttractor
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
        Vector3 m_currentDirection;
        float m_currentTime;
        float m_currentWindForce;

        float a = 0.2f;
        float b = 0.2f;
        float c = 5.7f;

        float sigma = 10;
        float ro = 2.666667f;
        float beta = 28f;

        /// <summary>
        /// Crée une nouvelle instance de l'attracteur de Henon.
        /// </summary>
        public HenonAttractor()
        {
            m_currentPosition = Vector3.Zero;
        }

        /// <summary>
        /// Effectue une étape de l'attracteur.
        /// </summary>
        public void NextStep(float delta, int numberOfSteps)
        {
            m_currentTime += delta;
            delta /= numberOfSteps;
            for (int i = 0; i < numberOfSteps; i++)
            {
                float dx = -m_currentDirection.Y - m_currentDirection.Z;
                float dy = m_currentDirection.X + a * m_currentDirection.Y;
                float dz = b + m_currentDirection.Z * (m_currentDirection.X - c);

                /*float dx = sigma * (m_currentPosition.Y - m_currentPosition.X);
                float dy = ro * m_currentPosition.X - m_currentPosition.Y - m_currentPosition.X * m_currentPosition.Z;
                float dz = m_currentPosition.X * m_currentPosition.Y - beta * m_currentPosition.Z;*/
                m_currentDirection += new Vector3(dx * delta, dy * delta, dz * delta);

            }
            // Force du vent
            m_currentWindForce = (float)Math.Cos(m_currentTime) * 6;
            m_currentPosition = Vector3.Normalize(m_currentDirection) * m_currentWindForce;
        }

        
    }
}
