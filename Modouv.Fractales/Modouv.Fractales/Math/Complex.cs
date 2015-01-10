using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test3D.Math
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
