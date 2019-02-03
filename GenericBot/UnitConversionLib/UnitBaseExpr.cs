using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnitConversionLib
{
    public class UnitBaseExpr
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public UnitBaseExpr()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public UnitBaseExpr(Unit @base, double pow = 1)
        {
            this._base = @base;
            this.pow = pow;
        }

        #endregion
        public Unit _base;
        public double pow;

        public UnitBaseExpr Copy()
        {
            return new UnitBaseExpr(_base, pow);
        }
    }
}
