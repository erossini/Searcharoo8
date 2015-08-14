using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Searcharoo.Common.Extensions
{
    public static class SupportCode
    {
        public static string StripInvalidUnicodeCharacters(this string str)
        {
            var invalidCharactersRegex = new Regex("([\ud800-\udbff](?![\udc00-\udfff]))|((?<![\ud800-\udbff])[\udc00-\udfff])");
            return invalidCharactersRegex.Replace(str, "");
        }

        public static string UnicodeToCharacter(this string inStr)
        {
            string result = inStr;

            Match m = Regex.Match(inStr, @".*\\u.*");
            while (m.Success)
            {
                Regex rx = new Regex(@"\\[uU]([0-9A-F]{4})");
                result = rx.Replace(result, delegate (Match match) { return ((char)Int32.Parse(match.Value.Substring(2), NumberStyles.HexNumber)).ToString(); });

                m = m.NextMatch();
            }
            return result;
        }
    }
}