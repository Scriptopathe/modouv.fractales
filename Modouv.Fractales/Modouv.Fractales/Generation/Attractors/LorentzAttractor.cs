﻿// Copyright (C) 2013, 2014 Alvarez Josué
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
    /// Attracteur de Lorentz.
    /// </summary>
    public class LorentzAttractor
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

        float sigma = 10;
        float ro = 2.666667f;
        float beta = 28f;

        /// <summary>
        /// Crée une nouvelle instance de l'attracteur de Rossler.
        /// </summary>
        public LorentzAttractor()
        {
            m_currentPosition = new Vector3(400, 400, 400);
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
                float dx = sigma * (m_currentPosition.Y - m_currentPosition.X);
                float dy = ro * m_currentPosition.X - m_currentPosition.Y - m_currentPosition.X * m_currentPosition.Z;
                float dz = m_currentPosition.X * m_currentPosition.Y - beta * m_currentPosition.Z;

                m_currentPosition += new Vector3(dx * delta, dy * delta, dz * delta);
            }
        }

        
    }
}
