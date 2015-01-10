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
    /// Gestionnaire de panels pour le HUD.
    /// </summary>
    public class HUDPanelManager : HUDComponent
    {
        /// <summary>
        /// Texture des boutons.
        /// </summary>
        static Texture2D s_texture;
        static Texture2D s_hoverTexture;
        static Texture2D s_pushedTexture;
        /// <summary>
        /// Dictionnaire contenant les pages à afficher.
        /// </summary>
        public Dictionary<string, Dictionary<string, HUDComponent>> Components
        {
            get;
            set;
        }
        /// <summary>
        /// Page actuellement affichée.
        /// </summary>
        public string CurrentPage
        {
            get;
            set;
        }
        /// <summary>
        /// Initialise une nouvelle instance de HUDPanelManager.
        /// </summary>
        public HUDPanelManager()
        {
            if (s_texture == null)
            {
                s_texture = Game1.Instance.Content.Load<Texture2D>("HUD\\button");
                s_pushedTexture = Game1.Instance.Content.Load<Texture2D>("HUD\\button-pushed");
                s_hoverTexture = Game1.Instance.Content.Load<Texture2D>("HUD\\button-hover");
            }
            Components = new Dictionary<string, Dictionary<string, HUDComponent>>();
        }

        /// <summary>
        /// Dessine le composant.
        /// </summary>
        public override void Draw(SpriteBatch batch)
        {
            int w = Game1.Instance.ResolutionWidth;
            int h = Game1.Instance.ResolutionHeight;
            int texW = 100;
            int texH = 25;
            int sx = 5;

            string pageHover = "";
            bool clicked = Input.IsLeftClickTrigger();
            // Affiche les boutons.
            foreach (var kvp in Components)
            {
                bool hover = false;
                // Vérification de la position de la souris :
                Rectangle rect = new Rectangle(sx, 0, texW, texH);
                if (rect.Contains(new Point(Input.GetMouseState().X, Input.GetMouseState().Y)))
                {
                    pageHover = kvp.Key;
                    hover = true;
                }
                Vector2 strLength = Game1.Instance.SmallFont.MeasureString(kvp.Key);

                // Dessin de la texture du bouton.
                Texture2D tex = hover ? s_hoverTexture : CurrentPage == kvp.Key ? s_pushedTexture : s_texture;
                int alpha = hover ? 58 : CurrentPage == kvp.Key ? 255 : 0;
                batch.Draw(tex, rect, new Color(255, 255, 255, alpha));

                // Dessin du texte du bouton.
                Color color = CurrentPage == kvp.Key ? Color.White : Color.Black;
                batch.DrawString(Game1.Instance.SmallFont, kvp.Key, new Vector2(sx + texW / 2 - strLength.X / 2, texH / 2 - strLength.Y / 2), color);
                sx += texW + 5;
            }

            // Si une page a été cliquée :
            if (pageHover != "" && clicked)
            {
                CurrentPage = pageHover;
            }

            
            // Dessine la liste de composants de la page active.
            foreach(var kvp  in Components[CurrentPage])
            {
                kvp.Value.Draw(batch);
            }
        }

        public override void SetValue(ComponentValue value) { throw new Exception(); }
        public override ComponentValue GetValue() { throw new Exception(); }
    }
}
