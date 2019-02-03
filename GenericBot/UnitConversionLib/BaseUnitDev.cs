using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnitConversionLib
{
    public class BaseUnitDev
    {
        public List<BaseUnitBaseExpr> soorate = new List<BaseUnitBaseExpr>();
        public List<BaseUnitBaseExpr> makhraj = new List<BaseUnitBaseExpr>();

        #region Methods
        public bool IsScalic()
        {
            this.Simplice();
            return this.IsEmpty();
        }

        public bool IsEmpty()
        {
            return this.soorate.Count == 0 && this.makhraj.Count == 0;
        }

        public static bool Equals(BaseUnitDev left, BaseUnitDev right)
        {
            left.Simplice();
            right.Simplice();

            var oLeft = left.soorate.Select(i => i).ToList();
            oLeft.AddRange(left.makhraj.Select(i => new BaseUnitBaseExpr(i._base, i.pow * -1)));

            var oRight = right.soorate.Select(i => i).ToList();
            oRight.AddRange(right.makhraj.Select(i => new BaseUnitBaseExpr(i._base, i.pow * -1)));

            if(oLeft.Count!=oRight.Count)
                return false;

            foreach (var l in oLeft)
            {
                var r = oRight.Where(i => i._base == l._base).ToList();

                if (r.Count > 1)
                    throw new UnitConversionLibException("Nabayad in ettefagh biofte");

                if (r.Count == 0)
                    return false;

                if (r.Count == 1)
                    if (l.pow != r.Single().pow)
                        return false;
            }

            foreach (var r in oRight)
            {
                var l = oLeft.Where(i => i._base == r._base).ToList();

                if (l.Count > 1)
                    throw new UnitConversionLibException("Nabayad in ettefagh biofte");

                if (l.Count == 0)
                    return false;

                if (l.Count == 1)
                    if (r.pow != l.Single().pow)
                        return false;
            }

            return true;
        }

        public void ApplyPower(double pow)
        {
            foreach (var s in this.soorate)
                s.pow *= pow;

            foreach (var m in this.makhraj)
                m.pow *= pow;
        }

        public void Simplice()
        {
            var all = this.soorate.Select(i => new Tuple<string, double>(i._base, i.pow)).ToList();
            all.AddRange(this.makhraj.Select(i => new Tuple<string, double>(i._base, -i.pow)));

            var dist = all.Select(i => i.Item1).Distinct().ToList();

            var newSoorate = new List<BaseUnitBaseExpr>();
            var newMakhraj = new List<BaseUnitBaseExpr>();

            foreach(var nm in dist)
            {
                double totPow = all.Where(i => i.Item1 == nm).Select(i => i.Item2).Sum();

                if (totPow == 0)
                    continue;

                if (totPow > 0)
                    newSoorate.Add(new BaseUnitBaseExpr(nm, totPow));
                
                if (totPow < 0)
                    newMakhraj.Add(new BaseUnitBaseExpr(nm, -totPow));
            }

            this.soorate.Clear();
            this.soorate.AddRange(newSoorate);
            this.makhraj.Clear();
            this.makhraj.AddRange(newMakhraj);
        }

        public override string ToString()
        {
            this.Simplice();
            var s = this.soorate.Select(i => i.pow == 1 ? i._base : string.Format("{0}^{1}", i._base, i.pow)).ToList();
            var m = this.makhraj.Select(i => i.pow == 1 ? i._base : string.Format("{0}^{1}", i._base, i.pow)).ToList();

            var stringS = string.Join(" . ", s.ToArray());
            var stringM = string.Join(" . ", m.ToArray());

            if (string.IsNullOrEmpty(stringS))
                stringS = "1";

            if (string.IsNullOrEmpty(stringM))
                stringM = "1";

            string format;

            if (!ContainsAnyCharacter(stringS, "*/^") && !ContainsAnyCharacter(stringM, "*/^"))
                format = stringM.Equals("1") ? "{0}" : "{0}/{1}";
            else
                format = stringM.Equals("1") ? "{0}" : "({0})/({1})";

            var buf = string.Format(format, stringS, stringM);
            return buf;

        }


        private static bool ContainsAnyCharacter(string taerget, string characters)
        {
            foreach (var chr in characters)
            {
                if (taerget.Contains(chr.ToString()))
                    return true;
            }

            return false;
        }


        #endregion
    }
}
