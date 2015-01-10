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
    public class HUDCheckbox : HUDComponent
    {
        public delegate void ValueChangedDelegate(bool newValue);
        /// <summary>
        /// Event appelé lorsque la valeur de la checkbox change.
        /// La nouvelle valeur est alors passée en argument.
        /// </summary>
        public event ValueChangedDelegate ValueChanged;

        public static Texture2D s_checkedTexture;
        public static Texture2D s_uncheckedTexture;
        /// <summary>
        /// Texte affiché sur la checkbox.
        /// </summary>
        public string Text
        {
            get;
            set;
        }
        /// <summary>
        /// Position (en pixels de la checkbox).
        /// </summary>
        public Vector2 Position
        {
            get;
            set;
        }
        /// <summary>
        /// Obtient ou définit une valeur indiquant si la checkbox est activée ou non.
        /// </summary>
        public bool Checked
        {
            get;
            set;
        }

        /// <summary>
        /// Initialise une nouvelle instance de HUDCheckbox.
        /// </summary>
        public HUDCheckbox()
        {
            
        }

        /// <summary>
        /// Dessine la checkbox.
        /// </summary>
        /// <param name="batch"></param>
        public override void Draw(SpriteBatch batch)
        {
            // Vérification de l'input.
            if (Input.IsLeftClickTrigger())
            {
                Rectangle hitBox = new Rectangle((int)Position.X, (int)Position.Y, 100, 16);
                
                if (hitBox.Contains(new Point(Input.GetMouseState().X, Input.GetMouseState().Y)))
                {
                    Checked = !Checked;
                    if (ValueChanged != null)
                        ValueChanged(Checked);
                }
            }

            // Chargement paraisseux de la texture.
            if (s_checkedTexture == null)
            {
                s_checkedTexture = Game1.Instance.Content.Load<Texture2D>("HUD\\check_box_checked_3");
                s_uncheckedTexture = Game1.Instance.Content.Load<Texture2D>("HUD\\check_box_3");
            }

            // Dessin.
            batch.Draw(Checked ? s_checkedTexture : s_uncheckedTexture,
                        new Rectangle((int)Position.X, (int)Position.Y, 16, 16),
                        Color.White);
            batch.DrawString(Game1.Instance.SmallFont, Text, new Vector2(Position.X + 20, Position.Y-3), Color.White);
        }

        #region HUDComponent Implementation
        /// <summary>
        /// Affecte les champs Checked et Text de la checkbox.
        /// </summary>
        /// <param name="value"></param>
        public override void SetValue(HUDComponent.ComponentValue value)
        {
            Checked = value.Checked;
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
            value.Checked = Checked;
            return value;
        }
        #endregion
    }
}
