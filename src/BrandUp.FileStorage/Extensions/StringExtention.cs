using System.Globalization;
using System.Text.RegularExpressions;

namespace BrandUp.FileStorage
{
    /// <summary>
    /// 
    /// </summary>
    internal static class StringExtention
    {
        static readonly Regex r = new("[a-z]+");

        /// <summary>
        /// Convert string to pascal case.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ToPascalCase(this string str)
        {
            str = str.ToLower();
            var matchCollection = r.Matches(str);

            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            string returnString = string.Empty;
            foreach (Match match in matchCollection)
            {
                returnString += textInfo.ToTitleCase(match.Value);
            }

            return returnString;
        }
    }
}
