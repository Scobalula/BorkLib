using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Borks
{
    public static class BytePattern
    {
        public static Pattern<byte> ParseString(string hexString)
        {
            // 2 characters per byte
            Span<char> buffer = stackalloc char[2];
            // We're going to need at least the size of this input string
            var pattern = new List<byte>(hexString.Length);
            var mask = new List<byte>(hexString.Length);

            for (int i = 0, j = 0; i < 2 && j < hexString.Length; j++)
            {
                // Skip whitespace
                if (char.IsWhiteSpace(hexString[j]))
                    continue;
                buffer[i++] = hexString[j];

                if (i == 2)
                {
                    i = 0;

                    // Check if unknown vs hex
                    if (buffer[0] == '?' || buffer[1] == '?')
                    {
                        pattern.Add(0);
                        mask.Add(0xFF);
                    }
                    else if (byte.TryParse(buffer, NumberStyles.HexNumber, null, out byte b))
                    {
                        pattern.Add(b);
                        mask.Add(0);
                    }
                }
            }

            return new(pattern.ToArray(), mask.ToArray());
        }
    }
}
