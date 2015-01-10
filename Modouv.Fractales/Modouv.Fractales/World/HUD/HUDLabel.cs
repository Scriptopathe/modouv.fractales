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
    /// Représente une checkbox pouvant être cochée ou décochée.
    /// </summary>
    public class HUDLabel : HUDComponent
    {
        private string m_text;
        private string[] m_lines;
        /// <summary>
        /// Texte affiché sur le label.
        /// </summary>
        public string Text
        {
            get { return m_text; }
            set
            {
                m_text = value;
                m_lines = m_text.Split('\n');
            }
        }

        /// <summary>
        /// Position (en pixels du label).
        /// </summary>
        public Vector2 Position
        {
            get;
            set;
        }
        /// <summary>
        /// Initialise une nouvelle instance de HUDCheckbox.
        /// </summary>
        public HUDLabel(string text = "")
        {
            Text = text;
        }

        /// <summary>
        /// Dessine la checkbox.
        /// </summary>
        /// <param name="batch"></param>
        public override void Draw(SpriteBatch batch)
        {
            int y = (int)Position.Y;
            // Dessin.
            for (int i = 0; i < m_lines.Length; i++)
            {
                batch.DrawString(Game1.Instance.SmallFont, m_lines[i], new Vector2(Position.X, y), Color.White);
                y += 25;
            }
        }

        #region HUDComponent Implementation
        /// <summary>
        /// Affecte les champs Checked et Text de la checkbox.
        /// </summary>
        /// <param name="value"></param>
        public override void SetValue(HUDComponent.ComponentValue value)
        {
            if (value.Text != null)
                Text = value.Text;
        }
        /// <summary>
        /// Retourne un objet comprenant :
        ///     - Text : texte de la checkbox.
        ///     - Checked : vrai si la checkbox est checkée.
        /// </summary>
        /// <returns></returns>
        public override HUDComponent.ComponentValue GetValue()
        {
            ComponentValue value = new ComponentValue();
            value.Text = Text;
            return value;
        }
        #endregion
    }
}
