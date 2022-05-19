using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Borks
{
    /// <summary>
    /// A class for handling Wildcards
    /// </summary>
    public class Wildcard
    {
        /// <summary>
        /// Translates the Wildcard string to a <see cref="Regex"/> object 
        /// </summary>
        /// <param name="input">Wildcard string</param>
        /// <returns>Resulting <see cref="Regex"/> object</returns>
        public static Regex TranslateToRegex(string input)
        {
            var bobTheBuilder = new StringBuilder(input.Length);

            for (int i = 0; i < input.Length; i++)
            {
                if(input[i] == '*')
                {
                    bobTheBuilder.Append(".*");
                }
                else if(input[i] == '?')
                {
                    bobTheBuilder.Append('.');
                }
                else
                {
                    bobTheBuilder.Append(input[i]);
                }
            }

            bobTheBuilder.Append("\\Z(?ms)");

            return new Regex(bobTheBuilder.ToString());
        }
    }
}
