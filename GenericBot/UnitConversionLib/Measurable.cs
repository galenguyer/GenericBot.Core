using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnitConversionLib
{
    public struct Measurable : IFormattable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public Measurable(Unit unit, double amount)
        {
            _unit = unit;
            _amount = amount;
        }

        #region Field-Props

        private Unit _unit;
        private double _amount;

        public Unit Unit
        {
            get { return _unit; }
        }

        public double Amount
        {
            get { return _amount; }
        }

        #endregion

        #region Operators

        public static Measurable operator *(Measurable left, Measurable right)
        {
            if (left.Unit.Scale != 1)
                if (left.Unit.OverridedLocalName == null)
                    throw UnitConversionLibException.UnregedScalic;

            if (right.Unit.Scale != 1)
                if (right.Unit.OverridedLocalName == null)
                    throw UnitConversionLibException.UnregedScalic;

            Measurable buf = new Measurable(left._unit*right.Unit, left.Amount*right.Amount);

            return buf;
        }

        public static Measurable operator *(Measurable meas, double coeff)
        {
            if (meas.Unit.Scale != 1)
                if (meas.Unit.OverridedLocalName == null)
                    throw UnitConversionLibException.UnregedScalic;

            Measurable buf = new Measurable(meas._unit, meas.Amount*coeff);

            return buf;
        }

        public static Measurable operator ^(Measurable meas, double pow)
        {
            if (meas.Unit.Scale != 1)
                if (meas.Unit.OverridedLocalName == null)
                    throw UnitConversionLibException.UnregedScalic;

            Measurable buf = new Measurable(meas._unit ^ pow, Math.Pow(meas.Amount, pow));

            return buf;
        }

        public static Measurable operator *(double coeff, Measurable meas)
        {
            return meas*coeff;
        }

        public static Measurable operator /(Measurable left, Measurable right)
        {
            if (left.Unit.Scale != 1)
                if (left.Unit.OverridedLocalName == null)
                    throw UnitConversionLibException.UnregedScalic;

            if (right.Unit.Scale != 1)
                if (right.Unit.OverridedLocalName == null)
                    throw UnitConversionLibException.UnregedScalic;

            Measurable buf = new Measurable(left._unit/right.Unit, left.Amount/right.Amount);

            return buf;
        }

        public static Measurable operator /(Measurable meas, double coeff)
        {
            if (meas.Unit.Scale != 1)
                if (meas.Unit.OverridedLocalName == null)
                    throw UnitConversionLibException.UnregedScalic;

            Measurable buf = new Measurable(meas._unit, meas.Amount/coeff);

            return buf;
        }

        public static Measurable operator /(double coeff, Measurable meas)
        {
            Measurable buf = new Measurable(1/meas._unit, coeff/meas.Amount);

            return buf;
        }

        public static Measurable operator +(Measurable left, Measurable right)
        {
            if (left.Unit.Scale != 1)
                if (left.Unit.OverridedLocalName == null)
                    throw UnitConversionLibException.UnregedScalic;

            if (right.Unit.Scale != 1)
                if (right.Unit.OverridedLocalName == null)
                    throw UnitConversionLibException.UnregedScalic;

            double coef;

            if (!left.Unit.IsConvertibleTo(right._unit, out coef))
                throw UnitConversionLibException.InconsistentUnits;

            var leftAmou = left._amount;

            var rightAmount = right._amount*coef;

            Measurable buf = new Measurable(left._unit, leftAmou + rightAmount);

            return buf;
        }

        public static bool operator ==(Measurable left, Measurable right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Measurable left, Measurable right)
        {
            return !left.Equals(right);
        }


        #endregion

        #region Public Methods

        public bool IsConvertibleTo(Unit unt)
        {
            double coef;
            return this._unit.IsConvertibleTo(unt, out coef);
        }

        public Measurable ConvertTo(Unit unt)
        {
            double coef;

            if (!this._unit.IsConvertibleTo(unt, out coef))
                throw new InvalidOperationException(string.Format("Unit '{0}' is not convertible to '{1}'",
                                                                  this._unit.LocalCaption, unt.LocalCaption));

            return new Measurable(unt, this._amount/coef);
        }

        #endregion

        #region Static

        public static Measurable Parse(string meaus)
        {
            var pat = @"^((\d+\.?\d*)([Ee][+-]?(\d+))?)\s+(.*?)$";

            if (!System.Text.RegularExpressions.Regex.IsMatch(meaus, pat))
                throw new UnitConversionLibException(string.Format("Fomat mismatch:\r\n{0}", meaus));

            var mt = System.Text.RegularExpressions.Regex.Match(meaus, pat);

            var allVals = mt.Groups.Cast<System.Text.RegularExpressions.Group>().Select(i => i.Value).ToList();

            var amount = mt.Groups[1].Value;

            var unit = mt.Groups[5].Value;
            
            var amountDbl = double.Parse(amount);

            var untU = Unit.Parse(unit);

            return new Measurable(untU, amountDbl);
        }

        public static bool TryParse(string meaus,out Measurable val)
        {
            try
            {
                val = Parse(meaus);
                return true;
            }
            catch (Exception)
            {
                val = new Measurable();
                return false;
            }
        }

        public static implicit operator Measurable(string val)
        {
            var temp = Measurable.Parse(val);
            // code to convert from int to SampleClass...

            return temp;
        }

        public static implicit operator double(Measurable val)
        {
            if (!val._unit.NotIsScalic) //then is scalic
                return val._amount*val._unit.GetTotalScales();

            throw new Exception("Measurable cannot convert directly to double type.");
            // code to convert from int to SampleClass...
        }

        #endregion

        
        public override string ToString()
        {
            return this.ToString(null,null);
        }


        public string ToString(string format, IFormatProvider formatProvider)
        {

            if(string.IsNullOrEmpty(format))
                return string.Format(formatProvider, "{0} {1}", this.Amount, this.Unit);
            else
                return string.Format(formatProvider, "{0} {1}", this.Amount.ToString(format), this.Unit);
            
        }
    }
}