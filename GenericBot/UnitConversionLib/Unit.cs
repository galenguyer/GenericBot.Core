using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using UnitConversionLib.UnitParse;

namespace UnitConversionLib
{
    public struct Unit : IEquatable<Unit>
    {
        #region Constructors

        static Unit()
        {
            TimerHelper.CreateNew("as");

            
            #region Base SI Units

            var m = CreateNew("m"); //meter - length
            abbreviationDic["M"] = "m";

            var sec = CreateNew("s"); //second - time
            abbreviationDic["sec"] = "s";
            abbreviationDic["Sec"] = "s";

            var kg = CreateNew("kg"); //kilograms - mass
            abbreviationDic["Kg"] = "kg";

            var a = CreateNew("A"); //Ampere - electric current
            abbreviationDic["ampere"] = "A";
            abbreviationDic["Ampere"] = "A";

            var c = CreateNew("°C"); //centigrad degree - temprature
            abbreviationDic["Celsius"] = "°C";
            abbreviationDic["celsius"] = "°C";

            var mol = CreateNew("mol"); //mol - amount of subtance
            abbreviationDic["Mol"] = "mol";
            abbreviationDic["MOL"] = "mol";

            var cd = CreateNew("cd"); //candella - luminous intensity
            abbreviationDic["Candela"] = "cd";
            abbreviationDic["candela"] = "cd";
            #endregion

            #region Length

            var angstrom = 1e-10 * m; //angstrom
            Register(ref angstrom, "Å", false);
            abbreviationDic["angstrom"] = "Å";
            abbreviationDic["Angstrom"] = "Å";

            var ft = 0.304800610 * m; //foot
            Register(ref ft, "foot", false);
            //Register(ref ft, "");

            var inch = 0.0254 * m; //inch
            Register(ref inch, "in", false);
            abbreviationDic["inch"] = "in";
            abbreviationDic["Inch"] = "in";

            var micron = 1e-6 * m; //micron
            Register(ref micron, "µ", false);
            abbreviationDic["micron"] = "µ";

            var mi = 1609.344 * m; //international mile 
            Register(ref mi, "mi", false);
            abbreviationDic["Mile"] = "mi";
            abbreviationDic["mile"] = "mi";

            var yard = 0.9144 * m; //yard
            Register(ref yard, "yd", false);
            abbreviationDic["Yard"] = "yd";
            abbreviationDic["yard"] = "yd";

            var centimeter = 0.01 * m; //centimeter
            Register(ref centimeter, "cm", false);
            abbreviationDic["Centimeter"] = "cm";
            abbreviationDic["centimeter"] = "cm";

            var milimeter = 0.001 * m; //milimeter
            Register(ref milimeter, "mm", false);
            abbreviationDic["Milimeter"] = "cm";
            abbreviationDic["milimeter"] = "cm";

            var km = 1000 * m; //kilometer
            Register(ref km, "km", false);
            abbreviationDic["kilometer"] = "km";
            abbreviationDic["Kilometer"] = "km";

            #endregion



            #region Time

            var microsecond = 1e-6*sec; //microsecond
            Register(ref microsecond, "µs", false);
            abbreviationDic["microsecond"] = "µs";
            abbreviationDic["Microsecond"] = "µs";


            var milisecond = 1e-3*sec; //millisecond
            Register(ref milisecond, "ms", false);
            abbreviationDic["millisecond"] = "ms";
            abbreviationDic["Millisecond"] = "ms";

            var hour = 3600*sec; //hour
            Register(ref hour, "h", false);
            abbreviationDic["hour"] = "h";
            abbreviationDic["Hour"] = "h";

            var minute = 60*sec; //minute
            Register(ref minute, "min", false);
            abbreviationDic["Minute"] = "min";
            abbreviationDic["minute"] = "min";

            var day = 86400*sec; //day
            Register(ref day, "d", false);
            abbreviationDic["Day"] = "d";
            abbreviationDic["day"] = "d";

            var week = 7*86400*sec; //Week
            Register(ref week, "week", false);
            abbreviationDic["Week"] = "week";

            var year = 365.24219878125*86400*sec; //year
            Register(ref year, "yr", false);
            abbreviationDic["year"] = "yr";
            abbreviationDic["Year"] = "yr";

            #endregion

            #region Mass

            var gr = 1e-3*kg; //grams
            Register(ref gr, "gm", false);
            abbreviationDic["gram"] = "gm";
            abbreviationDic["Gram"] = "gm";

            var mgr = 1e-6*kg; //mili grams
            Register(ref mgr, "mg", false);
            abbreviationDic["milligram"] = "mg";
            abbreviationDic["Milligram"] = "mg";

            var amu = 1.660538921e-27*kg; //atomic mass unit
            Register(ref amu, "u", false);
            abbreviationDic["Atomic mass unit"] = "u";
            abbreviationDic["atomic mass unit"] = "u";

            var slug = 14.5939*kg; //slug
            Register(ref slug, "slug", false);
            abbreviationDic["Slug"] = "slug";
            abbreviationDic["pond"] = "slug";

            var ton = 1e3 * kg; //ton
            Register(ref ton, "ton", false);
            abbreviationDic["Ton"] = "ton";

#endregion

            #region Force

            var N = kg*m/(sec ^ 2); //newton
            Register(ref N, "N",false);

            var dyne = 1e-5*N; //Dyne
            Register(ref dyne, "dyn", false);
            abbreviationDic["Dyne"] = "dyn";
            abbreviationDic["dyne"] = "dyn";

            var lbf = 4.4482216152605*N; //pound FOrce
            Register(ref lbf, "lbf", false);

            var kgf = 1e-1*N; //killogram-force
            Register(ref kgf, "kgf", false);
            abbreviationDic["Kgf"] = "kgf";

            var tonForce = 1e2*N; //Tone FOrce
            Register(ref tonForce, "tonf", false);
            abbreviationDic["Tonf"] = "tonf";

            #endregion
            
            #region Pressure

            var pas = N / (m ^ 2); //Pascal
            Register(ref pas, "Pa",false);
            abbreviationDic["pas"] = "Pa";
            abbreviationDic["Pas"] = "Pa";

            var kpas = 1e3 * pas; //Killo Pascal
            Register(ref kpas, "KPa", false);
            abbreviationDic["kpa"] = "KPa";
            abbreviationDic["kpas"] = "KPa";
            abbreviationDic["KPas"] = "KPa";

            var mpas = 1e6 * pas; //mega Pascal
            Register(ref mpas, "MPa", false);
            abbreviationDic["mpa"] = "MPa";
            abbreviationDic["MPas"] = "MPa";

            var gpas = 1e9 * pas; //giga Pascal
            Register(ref gpas, "GPa", false);
            abbreviationDic["gpa"] = "GPa";
            abbreviationDic["GPas"] = "GPa";

            var bar = 1e5*pas; //Bar
            Register(ref bar, "bar", false);
            abbreviationDic["Bar"] = "bar";

            var tAtm = 0.980665e5*pas; //Technical atmosphere
            Register(ref tAtm, "at", false);
            abbreviationDic["At"] = "at";

            var atm = 1.01325e5*pas; //Atmosphere
            Register(ref atm, "atm", false);
            abbreviationDic["Atm"] = "atm";

            var psi = 6.895*1e+3*pas; //Pound-force per square inch
            Register(ref psi, "psi", false);
            abbreviationDic["Psi"] = "psi";

            var torr = 133.322*pas; //Pound-force per square inch
            Register(ref torr, "torr", false);
            abbreviationDic["Torr"] = "torr";

            var mmH2O = 9.80665*pas; //milimeter water
            Register(ref mmH2O, "mmH2O", false);
            abbreviationDic["mmH2o"] = "mmH2O";

            var mmHg = 133.322368421*pas; //milimeter Hg
            Register(ref mmHg, "mmHg", false);

            #endregion

            var durr = TimerHelper.GetDuration("as");
            
        }

        public Unit(string name)
            : this()
        {
            IsBase = true;
            Name = name;
            OverridedLocalName = name;
            Scale = 1;
            this.dev = new UnitDev();
            this.NotIsScalic = true;
        }

        public Unit(UnitDev dev, double scale = 1)
            : this()
        {
            this.dev = dev;

            if (scale < 0)
                throw new UnitConversionLibException("Scale Cannot Be Lower Than Zero");
            Scale = scale;

            this.NotIsScalic = !dev.GetOrigins().IsScalic();
        }

        #endregion

        #region Methods

        #region NonStatic

        public BaseUnitDev GetLocalCaption()
        {
            if (Scale != 1)
            {
                if (OverridedLocalName == null)
                    throw new UnitConversionLibException("Unregistered Scalic Unit");
                else
                {
                    var tbuf = new BaseUnitDev();
                    tbuf.soorate.Add(new BaseUnitBaseExpr(OverridedLocalName));
                    return tbuf;
                }
            }

            if (OverridedLocalName != null)
            {
                var tbuf = new BaseUnitDev();
                tbuf.soorate.Add(new BaseUnitBaseExpr(OverridedLocalName));
                return tbuf;
            }

            var s = this.dev.soorate.Select(i =>
                                                {
                                                    var t = i._base.GetLocalCaption();
                                                    t.ApplyPower(i.pow);
                                                    return t;
                                                });

            var m = this.dev.makhraj.Select(i =>
                                                {
                                                    var t = i._base.GetLocalCaption();
                                                    t.ApplyPower(i.pow);
                                                    return t;
                                                });

            var ss = s.SelectMany(i => i.soorate).ToList();
            ss.AddRange(m.SelectMany(i => i.makhraj));

            var mm = s.SelectMany(i => i.makhraj).ToList();
            mm.AddRange(m.SelectMany(i => i.soorate));

            var buf = new BaseUnitDev();

            buf.soorate.AddRange(ss);
            buf.makhraj.AddRange(mm);

            buf.Simplice();

            return buf;
        }

        internal BaseUnitDev GetOrigins()
        {
            if (this.IsBase)
            {
                var buf = new BaseUnitDev();
                buf.soorate.Add(new BaseUnitBaseExpr(Name));
                return buf;
            }

            if (!NotIsScalic)
                return new BaseUnitDev();

            return dev.GetOrigins();
        }

        internal double GetTotalScales()
        {
            if (this.IsBase)
                return 1;

            if (this.Scale == 0)
                throw new UnitConversionLibException("Direct Created Unit");

            var buf = this.Scale;

            foreach (var r in this.dev.soorate)
                buf *= Math.Pow(r._base.GetTotalScales(), r.pow);

            foreach (var r in this.dev.makhraj)
                buf /= Math.Pow(r._base.GetTotalScales(), r.pow);

            return buf;
        }

        public Unit Copy()
        {
            var buf = new Unit();

            buf.dev = this.dev.Copy();
            buf.Scale = this.Scale;
            buf.Name = this.Name;
            buf.OverridedLocalName = this.OverridedLocalName;
            buf.IsBase = this.IsBase;
            buf.NotIsScalic = this.NotIsScalic;
            return buf;
        }

        public bool IsConvertibleTo(Unit destUnit, out double scale)
        {
            scale = 0;

            var thisOrg = this.GetOrigins();
            var thatOrg = destUnit.GetOrigins();
            if (BaseUnitDev.Equals(thisOrg, thatOrg))
            {
                var t = this.GetTotalScales();
                var th = destUnit.GetTotalScales();
                scale = th/t;
                return true;
            }

            return false;
        }

        public override string ToString()
        {
            return string.Format("{0}", LocalCaption);
        }

        #region IEquatable

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(Unit other)
        {
            double c;
            if (this.IsConvertibleTo(other, out c))
                if (c == 1)
                    return true;
            return false;
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <returns>
        /// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false.
        /// </returns>
        /// <param name="obj">Another object to compare to. </param><filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (obj.GetType() != typeof (Unit)) return false;
            return Equals((Unit) obj);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            return 0;
        }

        #endregion

        #endregion

        #region Static

        

        public static string GetGlobalName(Unit unt)
        {
            if (ArealyRegistered(unt))
                return registrationDic.Where(i => i.Value.Equals(unt)).Single().Key;
            throw new UnitConversionLibException(
                string.Format("No unit with local caption '{0}' registered yet.", unt.LocalCaption));
        }

        private static void Register(ref Unit unt, string nm ,bool checkNotRegitered = true)
        {
            if (checkNotRegitered)
            {

                if (ArealyRegistered(nm))
                    throw new UnitConversionLibException(string.Format("unit with name '{0}' arealy registered.", nm));

                if (ArealyRegistered(unt))
                    throw new UnitConversionLibException(
                        string.Format("unit arealy registered."));

            }

            unt.OverridedLocalName = nm;
            registrationDic[nm] = unt.Copy();
        }

        public static void Register(ref Unit unt, string nm)
        {
            Register(ref unt, nm, true);
        }

        public static Unit CreateNew(string nm)
        {
            if (ArealyRegistered(nm))
                throw new UnitConversionLibException(string.Format("unit with name '{0}' arealy registered.", nm));

            var buf = new Unit(nm);
            Register(ref buf, nm);
            return buf;
        }

        public static Unit GetRegisteredUnit(string nm)
        {
            if (ArealyRegistered(nm))
                if (abbreviationDic.Keys.Contains(nm))
                    return registrationDic[abbreviationDic[nm]];
                else
                    return registrationDic[nm];
            else
                throw new UnitConversionLibException(string.Format("No unit with name '{0}' registered yet", nm));
        }

        /// <summary>
        /// Gets the appropriate unit tags with nm.
        /// </summary>
        /// <param name="nm">The nm.</param>
        /// <returns></returns>
        public static List<string> GetAppropriate(string nm)
        {
            var buf = new List<string>();
            
            
            buf.AddRange(abbreviationDic.Values);

            buf = buf.Distinct().ToList();

            var wanted = buf.Where(i => i.ToLower() == nm.ToLower()).ToList();


            if (wanted.Count == 0)
                wanted = abbreviationDic.Keys.Where(i => i.ToLower() == nm.ToLower()).ToList();

            return wanted;
        }

        public static bool ArealyRegistered(string nm)
        {
            return registrationDic.Keys.Contains(nm) || abbreviationDic.Keys.Contains(nm);
        }

        public static bool ArealyRegistered(Unit unt)
        {
            return registrationDic.Values.Where(i => i.Equals(unt)).Count() == 1;
        }

        internal static Unit ParseDirect(string unt)
        {
            //TODO: handle more complex units
            var pat = @"(\S+)";
            if (!System.Text.RegularExpressions.Regex.IsMatch(unt, pat))
                throw new UnitConversionLibException(string.Format("Fomat mismatch:\r\n{0}", unt));
            
            var mt = System.Text.RegularExpressions.Regex.Match(unt, pat);
            var unit = mt.Groups[1].Value;
            if (ArealyRegistered(unit))
                return GetRegisteredUnit(unit);

            throw new UnitConversionLibException(string.Format("Fomat mismatch:\r\n{0}", unt));
        }

        public static Unit Parse(string unt)
        {
            var prsr = new ReversePolishNotation();
            
            prsr.Parse(unt);
            
            
            var res= prsr.Evaluate();
            return res;
            //TODO: handle more complex units
            var pat = @"(\S+)";
            if (!System.Text.RegularExpressions.Regex.IsMatch(unt, pat))
                throw new UnitConversionLibException(string.Format("Fomat mismatch:\r\n{0}", unt));

            var mt = System.Text.RegularExpressions.Regex.Match(unt, pat);
            var unit = mt.Groups[1].Value;
            if (ArealyRegistered(unit))
                return GetRegisteredUnit(unit);

            throw new UnitConversionLibException(string.Format("Fomat mismatch:\r\n{0}", unt));
        }

        #region Operators

        public static implicit operator Unit(string val)
        {
            var temp = Unit.Parse(val);
            // code to convert from int to SampleClass...
            return temp;
        }

        public static Unit operator *(Unit left, Unit right)
        {
            if (left.Scale != 1)
                if (left.OverridedLocalName == null)
                    throw UnitConversionLibException.UnregedScalic;

            if (right.Scale != 1)
                if (right.OverridedLocalName == null)
                    throw UnitConversionLibException.UnregedScalic;

            var newDev = new UnitDev();

            newDev.soorate.Add(new UnitBaseExpr(left.Copy()));
            newDev.soorate.Add(new UnitBaseExpr(right.Copy()));

            var buf = new Unit(newDev);

            return buf;
        }

        public static bool operator ==(Unit left, Unit right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Unit left, Unit right)
        {
            return !left.Equals(right);
        }

        public static Unit operator /(Unit left, Unit right)
        {
            if (left.Scale != 1)
                if (left.OverridedLocalName == null)
                    throw UnitConversionLibException.UnregedScalic;

            if (right.Scale != 1)
                if (right.OverridedLocalName == null)
                    throw UnitConversionLibException.UnregedScalic;

            var newDev = new UnitDev();

            newDev.soorate.Add(new UnitBaseExpr(left.Copy()));
            newDev.makhraj.Add(new UnitBaseExpr(right.Copy()));

            var buf = new Unit(newDev);
            return buf;
        }

        public static Unit operator *(double sc, Unit unt)
        {
            if (unt.Scale != 1)
                if (unt.OverridedLocalName == null)
                    throw UnitConversionLibException.UnregedScalic;

            if (sc <= 0)
                throw UnitConversionLibException.ScaleNonPositive;


            if (unt.Scale == 0)
                throw UnitConversionLibException.DirectUnitCreated;

            var newDev = new UnitDev();

            newDev.soorate.Add(new UnitBaseExpr(unt.Copy()));

            var buf = new Unit(newDev, sc);

            return buf;
        }

        public static Unit operator *(Unit unt, double sc)
        {
            return sc*unt;
        }

        public static Unit operator /(double sc, Unit unt)
        {
            if (unt.Scale != 1)
                if (unt.OverridedLocalName == null)
                    throw UnitConversionLibException.UnregedScalic;

            if (sc <= 0)
                throw UnitConversionLibException.ScaleNonPositive;


            if (unt.Scale == 0)
                throw UnitConversionLibException.DirectUnitCreated;

            var newDev = new UnitDev();

            newDev.makhraj.Add(new UnitBaseExpr(unt.Copy()));

            var buf = new Unit(newDev);

            return sc*unt;
        }

        public static Unit operator /(Unit unt, double sc)
        {
            return (1/sc)*unt;
        }

        public static Unit operator ^(Unit unt, double pow)
        {
            if (unt.Scale != 1)
                if (unt.OverridedLocalName == null)
                    throw UnitConversionLibException.UnregedScalic;

            if (unt.Scale == 0)
                throw UnitConversionLibException.DirectUnitCreated;

            var newDev = new UnitDev();

            newDev.soorate.Add(new UnitBaseExpr(unt.Copy(), pow));

            var buf = new Unit(newDev);

            return buf;
        }

        #endregion

        #endregion

        #endregion

        #region Fields

        internal static Dictionary<string, Unit> registrationDic = new Dictionary<string, Unit>();
        internal static Dictionary<string, string> abbreviationDic = new Dictionary<string, string>();

        public bool IsBase;
        public string Name;
        public string OverridedLocalName;
        public double Scale;

        public UnitDev dev;

        public bool NotIsScalic;
        /*{
            get
            {
                if (this.dev == null)
                    throw UnitConversionLibException.DirectUnitCreated;

                return !this.GetOrigins().IsScalic();
            }
        }*/

        public string LocalCaption
        {
            get
            {
                //return this.LocalCaption;
                return this.OverridedLocalName ?? GetLocalCaption().ToString();
            }
        }

        public string GlobalCaption
        {
            get { return Unit.GetGlobalName(this); }
        }

        #endregion
    }
}