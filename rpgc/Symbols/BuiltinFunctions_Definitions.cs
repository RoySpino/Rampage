using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace rpgc.Symbols
{
    class BuiltinFunctions_Definitions
    {
        public static double BIF_ABS(object A)
        {
            double value;

            value = Convert.ToDouble(A);

            return Math.Abs(value);
        }

        // /////////////////////////////////////////////////////////////////
        public static double BIF_LOG(object A)
        {
            double value;

            value = Convert.ToDouble(A);

            return Math.Log(value);
        }

        // /////////////////////////////////////////////////////////////////
        public static double BIF_LOG10(object A)
        {
            double value;

            value = Convert.ToDouble(A);

            return Math.Log10(value);
        }

        // /////////////////////////////////////////////////////////////////
        public static int BIF_INT(object A)
        {
            int ret;

            if (int.TryParse(A.ToString(), out ret) == false)
                throw new Exception($"Input string ‘{A}’ was not in a correct format");

            return ret;
        }

        // /////////////////////////////////////////////////////////////////
        public static string BIF_CHAR(object val)
        {
            return val.ToString();
        }

        // /////////////////////////////////////////////////////////////////
        public static double BIF_SQRT(object A)
        {
            double value;

            value = Convert.ToDouble(A);

            return Math.Sqrt(value);
        }

        // /////////////////////////////////////////////////////////////////
        public static int BIF_REM(object A, object B)
        {
            double Val0, Val1;

            Val0 = Convert.ToDouble(A);
            Val1 = Convert.ToDouble(B);

            return (int)(Val0 % Val1);
        }

        // /////////////////////////////////////////////////////////////////
        public static int BIF_LEN(object A)
        {
            string val;

            val = A.ToString();

            return val.Length;
        }

        // /////////////////////////////////////////////////////////////////
        public static string BIF_LOWER(object A)
        {
            string val;

            val = A.ToString();

            return val.ToLower();
        }

        // /////////////////////////////////////////////////////////////////
        public static string BIF_UPPER(object A)
        {
            string val;

            val = A.ToString();

            return val.ToUpper();
        }

        // /////////////////////////////////////////////////////////////////
        public static string BIF_SUBST(object Val0, object Val1, object Val2)
        {
            string result, Tval;
            int sidx, len;

            Tval = Val0.ToString();
            sidx = Convert.ToInt32(Val1) - 1;
            len = Convert.ToInt32(Val2);
            result = Tval.Substring(sidx, len);

            return result;
        }

        // /////////////////////////////////////////////////////////////////
        public static int BIF_RAND(object Val0)
        {
            int max;
            Random randnum;

            randnum = new Random();

            max = Convert.ToInt32(Val0);
            return randnum.Next(max);
        }

        // /////////////////////////////////////////////////////////////////
        public static double BIF_DIV(object Val0, object Val1)
        {
            double A, B;

            A = Convert.ToDouble(Val0);
            B = Convert.ToDouble(Val1);

            return (A / B);
        }

        // /////////////////////////////////////////////////////////////////
        public static int BIF_SCAN(object A, object B)
        {
            string whatToFind, source;
            int idx;

            whatToFind = A.ToString();
            source = B.ToString();
            idx = source.IndexOf(whatToFind) + 1;

            return idx;
        }

        // /////////////////////////////////////////////////////////////////
        public static string BIF_REPLACE(object A, object B, object C, object D)
        {
            string wrdValue, source;
            int startIndex, length;
            string tmp;


            wrdValue = A.ToString();
            source = B.ToString();
            startIndex = Convert.ToInt32(C);
            length = Convert.ToInt32(D);
            tmp = wrdValue.Substring(0, length);

            if ((startIndex + length) >= source.Length)
                return (source.Substring(0, startIndex - 1) + tmp);

            tmp = source.Substring(0, startIndex - 1) + tmp + source.Substring((startIndex + length - 1));
            return tmp;
        }

        // /////////////////////////////////////////////////////////////////
        public static string BIF_TRIM(object A)
        {
            string value;

            value = A.ToString();

            value = value.Trim();
            return value;
        }

        // /////////////////////////////////////////////////////////////////
        public static string BIF_XLATE(object A, object B, object C)
        {
            string toStr, res, fromStr, source;
            int lim;

            fromStr = A.ToString();
            toStr = B.ToString();
            source = C.ToString();

            // get the smallest length as the limit
            lim = fromStr.Length;
            if (lim > toStr.Length)
                lim = toStr.Length;

            // perform translation
            res = source;
            for (int i = 0; i < lim; i++)
            {
                // dont only if symbol is in string
                if (res.Contains(fromStr[i]) == true)
                    res = res.Replace(fromStr[i], toStr[i]);
            }

            return res;
        }

        // /////////////////////////////////////////////////////////////////
        public static int BIF_CHECK(object A, object B)
        {
            string fromStr, source;
            int idx;
            Match mtch;

            fromStr = A.ToString();
            source = B.ToString();

            mtch = System.Text.RegularExpressions.Regex.Match(source, $"[^{fromStr}]");
            idx = mtch.Index;

            return idx + 1;
        }

        // /////////////////////////////////////////////////////////////////
        public static int BIF_CHECKR(object A, object B)
        {
            string fromStr, source;
            int idx;

            fromStr = A.ToString();
            source = B.ToString();

            source.Reverse();
            fromStr.Reverse();
            var mtch = System.Text.RegularExpressions.Regex.Match(source, $"[^{fromStr}]");
            idx = mtch.Index;
            idx = (source.Length - 1) - idx;

            return idx + 1;
        }

        // /////////////////////////////////////////////////////////////////
        public static string BIF_EDITW(object A, object B)
        {
            int idx, flim;
            char ch;
            string ret, fmt, inp;

            fmt = A.ToString();
            inp = B.ToString();

            ret = "";
            flim = fmt.Length - 1;
            idx = inp.Length - 1;

            //foreach (char ch in nfmt)
            for (int i = flim; i > -1; i--)
            {
                ch = fmt[i];

                if (ch != ' ')
                {
                    ret = ch + ret;
                    continue;
                }

                // apply input string to the returning string
                if (idx > -1)
                {
                    ret = inp[idx] + ret;
                    idx -= 1;
                }
                else
                {
                    // at the end of the input string 
                    // apply the format string
                    ret = ch + ret;
                }
            }

            // input string is longer than format
            if (idx > -1)
                while (idx > -1)
                {
                    ret = inp[idx] + ret;
                    idx -= 1;
                }

            return ret;
        }

        // /////////////////////////////////////////////////////////////////
        public static int BIF_SHIFTL(object A, object B)
        {
            int val1, val2;

            val1 = Convert.ToInt32(A);
            val2 = Convert.ToInt32(B);

            return (val1 << val2);
        }

        // /////////////////////////////////////////////////////////////////
        public static int BIF_SHIFTR(object A, object B)
        {
            int val1, val2;

            val1 = Convert.ToInt32(A);
            val2 = Convert.ToInt32(B);

            return (val1 >> val2);
        }

        // /////////////////////////////////////////////////////////////////
        public static string BIF_TRIML(Object A)
        {
            string val;

            val = A.ToString();

            return val.TrimEnd();
        }

        // /////////////////////////////////////////////////////////////////
        public static string BIF_TRIMR(Object A)
        {
            string val;

            val = A.ToString();

            return val.TrimStart();
        }

        // /////////////////////////////////////////////////////////////////
        public static double BIF_FLOAT(Object A)
        {
            double ret;

            if (double.TryParse(A.ToString(), out ret) == false)
                throw new Exception($"Input string ‘{A}’ was not in a correct format");

            return ret;
        }

        // /////////////////////////////////////////////////////////////////
        public static int BIF_BITAND(Object A, Object B)
        {
            int val1, val2;

            val1 = Convert.ToInt32(A);
            val2 = Convert.ToInt32(B);

            return (val1 & val2);
        }

        // /////////////////////////////////////////////////////////////////
        public static int BIF_BITOR(Object A, Object B)
        {
            int val1, val2;

            val1 = Convert.ToInt32(A);
            val2 = Convert.ToInt32(B);

            return (val1 | val2);
        }

        // /////////////////////////////////////////////////////////////////
        public static int BIF_BITXOR(Object A, Object B)
        {
            int val1, val2;

            val1 = Convert.ToInt32(A);
            val2 = Convert.ToInt32(B);

            return (val1 ^ val2);
        }

        // /////////////////////////////////////////////////////////////////
        public static int BIF_BITNOT(Object A)
        {
            int val1;

            val1 = Convert.ToInt32(A);

            return ~val1;
        }

        // /////////////////////////////////////////////////////////////////
        public static int BIF_BITANDNOT(Object A, Object B)
        {
            int val1, val2;

            val1 = Convert.ToInt32(A);
            val2 = Convert.ToInt32(B);

            return ~(val1 & val2);
        }

        // /////////////////////////////////////////////////////////////////
        public static string BIF_SCANRPL(Object A, Object B, Object C)
        {
            string val1, val2, val3;

            val1 = A.ToString();
            val2 = B.ToString();
            val3 = C.ToString();

            return val3.Replace(val1, val2);
        }
    }
}
