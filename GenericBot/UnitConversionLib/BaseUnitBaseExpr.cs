using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnitConversionLib
{
    public class BaseUnitBaseExpr
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public BaseUnitBaseExpr()
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public BaseUnitBaseExpr(string @base, double pow = 1)
        {
            _base = @base;
            this.pow = pow;
        }

        #endregion

        public string _base;
        public double pow;

        public override string ToString()
        {
            return string.Format("{0}^{1}", _base, pow);
        }
    }
}