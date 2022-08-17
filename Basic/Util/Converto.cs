using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Basic.Util
{
    public class Converto
    {
        public static string toString(object obj, string def_str = "")
        {
            string str = "";
            if (obj != null)
            {
                try { str = obj.ToString(); }
                catch { str = def_str; }
            }
            return str;
        }
        public static int toInt(object obj, int def_val = 0)
        {
            try
            {
                def_val = (int)Round(toDecimal(obj, 0, def_val));
            }
            catch { }
            return def_val;
        }

        public static decimal toDecimal(object obj, int Bit = 2, decimal def_val = 0)
        {
            decimal val;
            try
            {
                val = Round(decimal.Parse(obj.ToString()), Bit);
            }
            catch { val = def_val; }
            return val;
        }

        public static decimal Round(decimal value, int Bit = 0)
        {
            return Math.Round(value, Bit, MidpointRounding.AwayFromZero);
        }
    }
}
