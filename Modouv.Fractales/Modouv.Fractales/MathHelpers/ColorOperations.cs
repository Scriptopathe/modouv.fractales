using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
namespace Modouv.Fractales.MathHelpers
{
    public class ColorOperations
    {
        /// <summary>
        /// Représente une couleur sous forme HSL.
        /// Avec H, S, L € [0, 1]
        /// </summary>
        public class HSL
        {
            #region Variables
            double _h;
            double _s;
            double _l;
            #endregion
            
            #region Properties
            /// <summary>
            /// Teinte de la couleur € [0, 1]
            /// </summary>
            public double H
            {
                get { return _h; }
                set
                {
                    _h = value;
                    _h = _h > 1 ? 1 : _h < 0 ? 0 : _h;
                }
            }
            /// <summary>
            /// Saturation de la couleur € [0, 1]
            /// </summary>
            public double S
            {
                get { return _s; }
                set
                {
                    _s = value;
                    _s = _s > 1 ? 1 : _s < 0 ? 0 : _s;
                }
            }
            /// <summary>
            /// Brillance de la couleur € [0, 1]
            /// </summary>
            public double L
            {
                get { return _l; }
                set
                {
                    _l = value;
                    _l = _l > 1 ? 1 : _l < 0 ? 0 : _l;
                }
            }
            #endregion

            public HSL()
            {
                _h = 0;
                _s = 0;
                _l = 0;
            }

            public HSL(float h, float s, float l)
            {
                H = h;
                S = s;
                L = l;
            }
            /// <summary>
            /// Convertit cette couleur HSL en RGB
            /// </summary>
            /// <returns></returns>
            public Color ToRGB()
            {
                return HSL_to_RGB(this);
            }
        }

        /// <summary>
        /// Convertit une couleur HSL en RGB.
        /// </summary>
        public static Color HSL_to_RGB(HSL hsl)
        {

            double r = 0, g = 0, b = 0;
            double temp1, temp2;
            if (hsl.L == 0)
            {
                r = g = b = 0;
            }
            else
            {
                if (hsl.S == 0)
                {
                    r = g = b = hsl.L;
                }
                else
                {
                    temp2 = ((hsl.L <= 0.5) ? hsl.L * (1.0 + hsl.S) : hsl.L + hsl.S - (hsl.L * hsl.S));
                    temp1 = 2.0 * hsl.L - temp2;
                    double[] t3 = new double[] { hsl.H + 1.0 / 3.0, hsl.H, hsl.H - 1.0 / 3.0 };
                    double[] clr = new double[] { 0, 0, 0 };
                    for (int i = 0; i < 3; i++)
                    {
                        if (t3[i] < 0)
                            t3[i] += 1.0;
                        if (t3[i] > 1)
                            t3[i] -= 1.0;

                        if (6.0 * t3[i] < 1.0)
                            clr[i] = temp1 + (temp2 - temp1) * t3[i] * 6.0;

                        else if (2.0 * t3[i] < 1.0)
                            clr[i] = temp2;

                        else if (3.0 * t3[i] < 2.0)
                            clr[i] = (temp1 + (temp2 - temp1) * ((2.0 / 3.0) - t3[i]) * 6.0);

                        else
                            clr[i] = temp1;

                    }
                    r = clr[0];
                    g = clr[1];
                    b = clr[2];
                }
            }
            return new Color((int)(255 * r), (int)(255 * g), (int)(255 * b));
        }
    }
}
