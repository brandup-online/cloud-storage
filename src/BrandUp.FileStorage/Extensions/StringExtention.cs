using System.Globalization;
using System.Text.RegularExpressions;

namespace BrandUp.FileStorage
{
    /// <summary>
    /// 
    /// </summary>
    internal static class StringExtention
    {
        static readonly Regex r = new("[a-zA-Z0-9]+");

        /// <summary>
        /// Convert string to pascal case.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ToPascalCase(this string str)
        {
            var matchCollection = r.Matches(str);

            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            string returnString = string.Empty;
            if (matchCollection.Count == 1)
                return matchCollection[0].Value;
            foreach (Match match in matchCollection)
            {
                returnString += textInfo.ToTitleCase(match.Value);
            }

            return returnString;
        }
    }
}
