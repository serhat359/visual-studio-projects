namespace DotNetCoreWebsite
{
    public static class Extensions
    {
        public static long sizeBase = 1024;
        public static double sizeBaseD = (double)sizeBase;
        
        public static string ToFileSizeString(this long? length)
        {
            if (length == null)
                return "";

            if (length < sizeBase)
                return $"{length} bytes";
            if (length < sizeBase * sizeBase)
                return string.Format("{0:N2} KiB", (length / sizeBaseD));
            if (length < sizeBase * sizeBase * sizeBase)
                return string.Format("{0:N2} MiB", (length / sizeBaseD / sizeBaseD));
            if (length < sizeBase * sizeBase * sizeBase * sizeBase)
                return string.Format("{0:N2} GiB", (length / sizeBaseD / sizeBaseD / sizeBaseD));

            return "";
        }

        public static string NullIfEmpty(this string s)
        {
            if (s == "")
                return null;
            else
                return s;
        }
    }
}
