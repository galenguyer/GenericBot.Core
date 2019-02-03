using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnitConversionLib
{
    public class UnitDev
    {
        public List<UnitBaseExpr> soorate = new List<UnitBaseExpr>();
        public List<UnitBaseExpr> makhraj = new List<UnitBaseExpr>();

        #region Methods
        public UnitDev Copy()
        {
            var buf = new UnitDev();
            
            buf.soorate.AddRange(this.soorate.Select(i => i.Copy()));
            buf.makhraj.AddRange(this.makhraj.Select(i => i.Copy()));

            return buf;
        }

        public BaseUnitDev GetOrigins()
        {
            var so = this.soorate.Select(i =>
            {
                var t = i._base.GetOrigins();
                t.ApplyPower(i.pow);
                return t;
            });

            var mo = this.makhraj.Select(i =>
            {
                var t = i._base.GetOrigins();
                t.ApplyPower(i.pow);
                return t;
            });

            var st = so.SelectMany(i => i.soorate).ToList();
            st.AddRange(mo.SelectMany(i => i.makhraj));

            var mt = so.SelectMany(i => i.makhraj).ToList();
            mt.AddRange(mo.SelectMany(i => i.soorate));

            var buf = new BaseUnitDev();
            buf.soorate.AddRange(st);
            buf.makhraj.AddRange(mt);

            buf.Simplice();

            return buf;
        }
        #endregion
    }
}
