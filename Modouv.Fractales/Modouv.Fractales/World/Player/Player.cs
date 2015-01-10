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
namespace Modouv.Fractales.World.Player
{
    /// <summary>
    /// Représente le joueur.
    /// </summary>
    public class Player
    {
        #region Variables
        Vector3 m_velocity;
        Vector3 m_acceleration;
        Vector3 m_inertia;
        Vector3 m_position;
        #endregion

        #region Properties
        public Vector3 Position
        {
            get { return m_position; }
            set { m_position = value; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Player
        /// </summary>
        public Player()
        {
            m_velocity = Vector3.Zero;
            m_position = Vector3.Zero;
            m_acceleration = Vector3.Zero;
            m_inertia = new Vector3(100, 100, 100);
        }


        /// <summary>
        /// Accélère.
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="amount"></param>
        public void Accelerate(Vector3 direction, float amount)
        {
            m_acceleration += direction * amount;
        }

        /// <summary>
        /// Mets à jour la vélocité du héros.
        /// </summary>
        /// <param name="time"></param>
        public void UpdateVelocity(GameTime time)
        {
            m_velocity = Vector3.Max(Vector3.Zero, m_velocity - m_inertia * (float)time.ElapsedGameTime.TotalMilliseconds/1000.0f);
            m_velocity += m_acceleration * (float)time.ElapsedGameTime.TotalMilliseconds/1000.0f;

            m_velocity = Vector3.Min(m_velocity, new Vector3(50, 50, 50));
            m_position += m_velocity;
        }
        #endregion
    }
}
