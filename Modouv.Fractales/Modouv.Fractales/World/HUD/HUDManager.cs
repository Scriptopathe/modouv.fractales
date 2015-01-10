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
namespace Modouv.Fractales.World.HUD
{
    /// <summary>
    /// Contient, mets à jour et dessine les éléments du HUD.
    /// </summary>
    public class HUDManager
    {
        Texture2D m_sidebar;
        Texture2D m_mouseCursor;
        /// <summary>
        /// Contient les composants du HUD.
        /// </summary>
        public Dictionary<string, HUDComponent> Components
        {
            get;
            set;
        }

        /// <summary>
        /// Crée une nouvelle instance de HUD manager.
        /// </summary>
        public HUDManager()
        {
            Components = new Dictionary<string, HUDComponent>();
            m_sidebar = Game1.Instance.Content.Load<Texture2D>("HUD\\sidebar");
            m_mouseCursor = Game1.Instance.Content.Load<Texture2D>("HUD\\cursor");
        }

        /// <summary>
        /// Dessine le HUD.
        /// </summary>
        /// <param name="time"></param>
        public void Draw(GameTime time)
        {
            Game1.Instance.Batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            Game1.Instance.Batch.Draw(m_sidebar,
                new Rectangle(0, 25, Game1.Instance.GraphicsDevice.ScissorRectangle.Width / 3, Game1.Instance.GraphicsDevice.ScissorRectangle.Height),
                Color.White);

            // Dessin de la souris.
            Rectangle r = new Rectangle(Input.GetMouseState().X, Input.GetMouseState().Y, m_mouseCursor.Width, m_mouseCursor.Height);
            Game1.Instance.Batch.Draw(m_mouseCursor, r, Color.White);

            // Dessin des composants.
            foreach (KeyValuePair<string, HUDComponent> kvp in Components)
            {
                kvp.Value.Draw(Game1.Instance.Batch);
            }
            Game1.Instance.Batch.End();
        }
    }
}
