namespace Reliable
{
    public static class Util
    {
        public static string With(this string str, params object[] args)
            => string.Format(str, args);

        public static string Str<T>(this T[] array)
        {
            string s = "";
            foreach (T t in array)
                s += $"{t} ";
            return s;
        }

        public static string StrHex<T>(this T[] array)
        {
            string s = "";
            foreach (T t in array)
                s += "{0:X} ".With(t);
            return s;
        }
    }
}