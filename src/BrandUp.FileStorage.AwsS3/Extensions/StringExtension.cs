using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace BrandUp.FileStorage.AwsS3
{
    public static class StringExtension
    {
        public static string ToTrainCase(this string value)
        {
            if (value is null) return null;

            if (value.Length == 0) return string.Empty;

            StringBuilder builder = new();

            for (var i = 0; i < value.Length; i++)
            {
                if (i == 0) // if current char is the first char 
                {
                    builder.Append(char.ToUpper(value[i]));
                }
                else if (value[i] == '-')
                    continue;
                else if (char.IsLower(value[i])) // if current char is already lowercase
                {
                    builder.Append(value[i]);
                }
                else if (char.IsDigit(value[i]) && !char.IsDigit(value[i - 1])) // if current char is a number and the previous is not
                {
                    builder.Append('-');
                    builder.Append(value[i]);
                }
                else if (char.IsDigit(value[i])) // if current char is a number and previous is
                {
                    builder.Append(value[i]);
                }
                else if (char.IsLower(value[i - 1])) // if current char is upper and previous char is lower
                {
                    builder.Append('-');
                    builder.Append(value[i]);
                }
                else if (i + 1 == value.Length || char.IsUpper(value[i + 1])) // if current char is upper and next char doesn't exist or is upper
                {
                    builder.Append(char.ToLower(value[i]));
                }
                else // if current char is upper and next char is lower
                {
                    builder.Append('-');
                    builder.Append(value[i]);
                }
            }
            return builder.ToString();
        }


        static readonly Regex r = new("[a-zA-Z0-9]+");


        public static string ToPascalCase(this string value)
        {
            var matchCollection = r.Matches(value);

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
