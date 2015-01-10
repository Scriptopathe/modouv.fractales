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
using System.Threading.Tasks;
using Vert = Modouv.Fractales.Generation.ModelGenerator.VertexPositionColorNormalTexture;
namespace Modouv.Fractales.Scenes
{

    /// <summary>
    /// Représente une tâche asynchrone de calcul sur une frame.
    /// </summary>
    public class OceanTask
    {
        const int MAX_ADVANCE = 10;

        /// <summary>
        /// Mutex permettant de locker du code.
        /// </summary>
        object m_mutex;

        /// <summary>
        /// Indique si le thread doit être stoppé.
        /// </summary>
        bool m_threadStopped = true;
        /// <summary>
        /// Pool d'instances.
        /// </summary>
        Pool<Vert[]> m_instances;
        /// <summary>
        /// Contient les résultats de la tâche.
        /// </summary>
        Queue<Vert[]> m_results;
        /// <summary>
        /// Dernier résultat sortit de la pile.
        /// </summary>
        Vert[] lastPoped;
        /// <summary>
        /// Frame en cours.
        /// </summary>
        float m_waterFrame;

        #region Properties
        /// <summary>
        /// Obtient le dernier résultat du calcul.
        /// </summary>
        /// <returns></returns>
        public Vert[] PopLastResult()
        {
            lock (m_mutex)
            {
                lastPoped = m_results.Dequeue();
                m_instances.Deactivate(lastPoped);
            }
            return lastPoped;
        }

        /// <summary>
        /// Indique si un calcul au moins est terminé.
        /// </summary>
        public bool HasTerminated
        {
            get {
                lock (m_mutex)
                {
                    return m_results.Count != 0;
                }
            }
        }
        #endregion

        #region Methods
        
        /// <summary>
        /// Stoppe le thread de la tâche.
        /// </summary>
        public void Stop()
        {
            m_threadStopped = true;
        }
        /// <summary>
        /// Lance la tâche.
        /// </summary>
        public void Run()
        {
            while (!m_threadStopped)
            {
                Vert[] instance;
                lock (m_mutex)
                {
                    instance = m_instances.GetFromPool();
                }
                // Effectue les calculs

                lock (m_mutex)
                {
                    m_results.Enqueue(instance);
                }
            }
            m_threadStopped = false;
        }

        /// <summary>
        /// Effectue les calculs sur les vertex passés en paramètre.
        /// </summary>
        /// <param name="instance"></param>
        public void Calculate(Vert[] instance)
        {
            m_waterFrame += 0.002f;

        }

        void CalculateNormals()
        {

        }
        #endregion


    }
}
