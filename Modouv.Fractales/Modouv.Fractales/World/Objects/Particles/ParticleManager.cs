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
namespace Modouv.Fractales.World.Objects.Particles
{
    /// <summary>
    /// Permet de gérer des particules.
    /// </summary>
    public class ParticleManager
    {
        /// <summary>
        /// Liste de particules à afficher.
        /// </summary>
        List<IParticle> m_particles;
        /// <summary>
        /// Liste des particules à afficher.
        /// </summary>
        public List<IParticle> Particles
        {
            get { return m_particles; }
            protected set { m_particles = value; }
        }
        /// <summary>
        /// Initialise une nouvelle instance de ParticleManager.
        /// </summary>
        public ParticleManager()
        {
            m_particles = new List<IParticle>();
        }

        /// <summary>
        /// Dessine toutes les particules associées à ce ParticleManager.
        /// </summary>
        public void Draw(SpriteBatch batch, GameTime time)
        {
            batch.Begin(SpriteSortMode.BackToFront, BlendState.Additive);
            foreach (IParticle particle in m_particles)
            {
                particle.Draw(batch, time);
            }
            batch.End();
        }
    }
}
