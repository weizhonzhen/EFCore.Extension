using System;

namespace EFCore.Extension.Base
{
    internal static class BaseRegular
    {
        internal static DateTime? ToDate(this object strValue)
        {
            if (strValue.ToStr() == "")
                return null;
            return Convert.ToDateTime(strValue);
        }

        internal static int ToInt(this string str, int defValue)
        {
            int tmp = 0;
            if (Int32.TryParse(str, out tmp))
                return (int)tmp;
            else
                return defValue;
        }

        internal static float ToFloat(this string strValue, float defValue)
        {
            float tmp = 0;
            if (float.TryParse(strValue, out tmp))
                return (float)tmp;
            else
                return defValue;
        }

        internal static double ToDouble(this string strValue, double defValue)
        {
            double tmp = 0;
            if (double.TryParse(strValue, out tmp))
                return (double)tmp;
            else
                return defValue;
        }

        internal static long ToLong(this string strValue, long defValue)
        {
            long tmp = 0;
            if (Int64.TryParse(strValue, out tmp))
                return (long)tmp;
            else
                return defValue;
        }

        internal static Int16 ToInt16(this string strValue, Int16 defValue)
        {
            Int16 tmp = 0;
            if (Int16.TryParse(strValue, out tmp))
                return (Int16)tmp;
            else
                return defValue;
        }

        internal static string ToStr(this object strValue)
        {
            if (strValue == null)
                return "";
            else
                return strValue.ToString();
        }
    }
}