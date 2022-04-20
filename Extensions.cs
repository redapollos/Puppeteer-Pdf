using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace RainstormTech.Puppeteer_Pdf
{
    internal static class Extensions
    {
        internal static string Clean(this string str, string exclusions = "") // i.e. br,p,style
        {
            if (string.IsNullOrEmpty(str))
                return string.Empty;

            // if no exclusions, just remove all tags
            if (string.IsNullOrEmpty(exclusions))
                return Regex.Replace(str, "<(.|\n)+?>", ""); // strip all tags

            // exclusions will be csv of tag names
            var list = new List<string>();
            foreach (var e in exclusions.Split(','))
                list.Add($"/?{e}");

            string re = "<(?!(" + string.Join('|', list) + ")[\x20/>])[^<>]+>";

            return Regex.Replace(str, re, ""); // strip tags with exceptions
        }

        internal static int ToInt(this string s, int defValue)
        {
            if (string.IsNullOrEmpty(s))
                return defValue;

            if (Int32.TryParse(s, out int i))
                return i;
            return defValue;
        }

        internal static bool ToBool(this string s, bool defValue)
        {
            if (string.IsNullOrEmpty(s))
                return defValue;

            if (bool.TryParse(s, out bool i))
                return i;
            return defValue;
        }

        internal static string ToWebsite(this string url)
        {
            if (string.IsNullOrEmpty(url))
                return string.Empty;

            // check both full and relative links
            if (!url.StartsWith("http"))
                url = $"https://{url.Replace("//", "")}";

            return HttpUtility.UrlDecode(url);
        }
    }
}
