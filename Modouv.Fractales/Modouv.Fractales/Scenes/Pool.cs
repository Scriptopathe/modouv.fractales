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

namespace Modouv.Fractales
{

    public class Pool<T>
    {
        /* --------------------------------------------------------------------------------
        * Variables
        * -------------------------------------------------------------------------------*/
        #region Variables
        public int MAX_COUNT = 5;
        private List<T> m_active;
        private T[] m_pool;
        private List<T> m_deactivationQueue;
        #endregion
        /* --------------------------------------------------------------------------------
         * Methods
         * -------------------------------------------------------------------------------*/
        #region Methods
        /// <summary>
        /// Constructeur.
        /// </summary>
        public Pool(T[] instances)
        {
            MAX_COUNT = instances.Length;
            m_active = new List<T>(MAX_COUNT);
            m_deactivationQueue = new List<T>(MAX_COUNT);
            m_pool = instances;
        }
        /// <summary>
        /// Pool update.
        /// </summary>
        public void Update()
        {
            // Deactivates objects in the deactivation queue.
            while (m_deactivationQueue.Count != 0)
            {
                Free(m_deactivationQueue[0]);
                m_deactivationQueue.RemoveAt(0);
            }
        }

        /// <summary>
        /// Returns a List containing every active object.
        /// Use it to iterate through events.
        /// </summary>
        /// <returns></returns>
        public List<T> GetActive()
        {
            return m_active;
        }
        /// <summary>
        /// Returns one instance of the subclass of event specified by GameObject,
        /// from the pool and set it up using <paramref name="data"/>.
        /// This item becomes active and goes as the first item of the list.
        /// pool -> active.
        /// </summary>
        /// <param name="data">The data to be used to initialize the event</param>
        /// <typeparam name="GameObject">The type of the object</typeparam>
        /// <returns></returns>
        public T GetFromPool()
        {
            for (int i = 0; i < MAX_COUNT; i++)
            {
                // Removes a reference from the pool and adds it into the active objects.
                if (m_pool[i] != null)
                {
                    T ev = (T)m_pool[i];
                    m_active.Add(m_pool[i]);
                    m_pool[i] = default(T);
                    return ev;
                }
            }
            throw new Exception("Not enough events in pool.");
        }
        /// <summary>
        /// Removes an event from the active ones and put in the pool.
        /// active -> pool
        /// </summary>
        /// <param name="ev"></param>
        void Free(T ev)
        {
            // Removes the event from the actives ones.
            m_active.Remove(ev);
            bool ok = false;
            // Adds the event in the pool.
            for (int i = 0; i < MAX_COUNT; i++)
            {
                if (m_pool[i] == null)
                {
                    m_pool[i] = ev;
                    ok = true;
                    break;
                }
            }
            if (!ok)
                throw new Exception("Problem");
        }


        /// <summary>
        /// Clears the pool, and marks all item as available.
        /// pool -> active.
        /// </summary>
        public void Clear()
        {
            int j = 0;
            foreach (T ev in m_active)
            {
                for (int i = j; i < MAX_COUNT; i++)
                {
                    if (m_pool[i] == null)
                    {
                        m_pool[i] = ev;
                        m_active.Remove(ev);
                        j = i;
                    }
                }
            }
        }
        /// <summary>
        /// Removes an event from the active ones and put in the pool.
        /// active -> pool
        /// </summary>
        /// <param name="ev"></param>
        public void Deactivate(T ev)
        {
            m_deactivationQueue.Add(ev);
        }
        /// <summary>
        /// Frees all the memory ressources held by the pool.
        /// </summary>
        public void Dispose()
        {
            m_pool = null;

            m_active.Clear();
            m_active = null;
        }
        #endregion
    }
}
