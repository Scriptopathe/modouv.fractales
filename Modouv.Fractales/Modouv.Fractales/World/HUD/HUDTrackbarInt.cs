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
    public class HUDTrackbarInt : HUDComponent
    {
        public delegate void ValueChangedDelegate(int newValue);
        /// <summary>
        /// Event appelé lorsque la valeur de la trackbar change.
        /// La nouvelle valeur est alors passée en argument.
        /// </summary>
        public event ValueChangedDelegate ValueChanged;

        public static Texture2D s_trackbarTexture;
        public static Texture2D s_backgroundTexture;
        bool m_mouseCaptured = false;
        /// <summary>
        /// Texte affiché sur le côté gauche de la trackbar.
        /// </summary>
        public string Text
        {
            get;
            set;
        }
        /// <summary>
        /// Position (en pixels) de la trackbar.
        /// </summary>
        public Vector2 Position
        {
            get;
            set;
        }
        /// <summary>
        /// Obtient ou définit une valeur indiquant le niveau de la trackbar.
        /// </summary>
        public int Value
        {
            get;
            set;
        }
        
        /// <summary>
        /// Valeur minimum de la trackbar.
        /// </summary>
        public int MinValue
        {
            get;
            set;
        }

        /// <summary>
        /// Valeur max de la trackbar.
        /// </summary>
        public int MaxValue
        {
            get;
            set;
        }

        /// <summary>
        /// Largeur de la trackbar.
        /// </summary>
        public float Width
        {
            get;
            set;
        }

        /// <summary>
        /// Offset de la trackbar par rapport au label situé à gauche.
        /// </summary>
        public float Offset
        {
            get;
            set;
        }
        /// <summary>
        /// Initialise une nouvelle instance de HUDCheckbox.
        /// </summary>
        public HUDTrackbarInt()
        {
            Width = 80;
            MinValue = 0;
            MaxValue = 100;
            Offset = 120;
        }

        /// <summary>
        /// Dessine la checkbox.
        /// </summary>
        /// <param name="batch"></param>
        public override void Draw(SpriteBatch batch)
        {
            // Vérification de l'input.
            if (Input.GetMouseState().LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
            {
                Rectangle hitBox = new Rectangle((int)(Position.X + Offset), (int)Position.Y, (int)Width, 10);
                if (m_mouseCaptured || hitBox.Contains(new Point(Input.GetMouseState().X, Input.GetMouseState().Y)))
                {
                    float percent = Math.Min(1, Math.Max(0, (Input.GetMouseState().X - (Offset + Position.X)) / Width));
                    float value = percent * (MaxValue - MinValue) + MinValue;
                    Value = (int)value;
                    ValueChanged(Value);
                    m_mouseCaptured = true;
                }
            }
            else
                m_mouseCaptured = false;

            // Chargement paraisseux de la texture.
            if (s_trackbarTexture == null)
            {
                s_trackbarTexture = Game1.Instance.Content.Load<Texture2D>("HUD\\trackbar");
                s_backgroundTexture = Game1.Instance.Content.Load<Texture2D>("HUD\\trackbar_back");
            }

            // Dessin.
            // Label
            batch.DrawString(Game1.Instance.SmallFont, Text, new Vector2(Position.X, Position.Y-3), Color.White);

            // Trackbar
            batch.Draw(s_backgroundTexture, new Rectangle((int)(Position.X + Offset), (int)Position.Y, (int)Width, 16), Color.White);

            float pos = Offset + Width * (Value - MinValue) / (MaxValue - MinValue);
            batch.Draw(s_trackbarTexture, new Rectangle((int)(Position.X + pos), (int)Position.Y, s_trackbarTexture.Width, s_trackbarTexture.Height), Color.White);

            // Valeur 
            string val = Value.ToString();
            batch.DrawString(Game1.Instance.SmallFont, val, new Vector2(Position.X + Offset + Width + 5, Position.Y-3), Color.White);
        }

        #region HUDComponent Implementation
        /// <summary>
        /// Affecte les champs Checked et FloatValue de la trackbar.
        /// </summary>
        /// <param name="value"></param>
        public override void SetValue(HUDComponent.ComponentValue value)
        {
            Value = value.IntValue;
            if (value.Text != null)
                Text = value.Text;
        }
        /// <summary>
        /// Retourne un objet comprenant :
        ///     - Text : texte de la trackbar
        ///     - FloatValue : valeur de la trackbar
        /// </summary>
        /// <returns></returns>
        public override HUDComponent.ComponentValue GetValue()
        {
            ComponentValue value = new ComponentValue();
            value.Text = Text;
            value.IntValue = Value;
            return value;
        }
        #endregion
    }
}
