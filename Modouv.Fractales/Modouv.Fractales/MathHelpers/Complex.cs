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

namespace Modouv.Fractales.MathHelpers
{
    /// <summary>
    /// Représente un nombre complexe.
    /// </summary>
    public struct Complex
    {
        public float Real;
        public float Imaginary;

        /// <summary>
        /// Obtient la valeur du module du nombre complexe représenté par cette instance.
        /// </summary>
        public float Module
        {
            get
            {
                return (float)System.Math.Sqrt(System.Math.Pow(Real, 2) + System.Math.Pow(Imaginary, 2));
            }
        }
        /// <summary>
        /// Obtient la valeur du carré du module du nombre complexe représenté par cette instance.
        /// Plus rapide que Module.
        /// </summary>
        public float SquaredModule
        {
            get
            {
                return (float)(System.Math.Pow(Real, 2) + System.Math.Pow(Imaginary, 2));
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="real"></param>
        /// <param name="imaginary"></param>
        public Complex(float real, float imaginary)
        {
            Real = real;
            Imaginary = imaginary;
        }
        /// <summary>
        /// Ajoute les complexes x et y et retourne le résulat. 
        /// </summary>
        public static Complex operator + (Complex x, Complex y)
        {
            return new Complex(x.Real + y.Real, x.Imaginary + y.Imaginary);
        }
        /// <summary>
        /// Ajoute les complexes x et y et retourne le résulat. 
        /// </summary>
        public static Complex operator *(Complex x, Complex y)
        {
            return new Complex(x.Real * y.Real - x.Imaginary*y.Imaginary, x.Real*y.Imaginary + x.Imaginary*y.Real);
        }
        /// <summary>
        /// Ajoute les complexes x et y et retourne le résulat. 
        /// </summary>
        public static Complex operator *(float f, Complex y)
        {
            return new Complex(f * y.Real, f * y.Imaginary);
        }
        /// <summary>
        /// Ajoute les complexes x et y et retourne le résulat. 
        /// </summary>
        public static Complex operator -(Complex x)
        {
            return new Complex(-x.Real, -x.Imaginary);
        }
    }
}
