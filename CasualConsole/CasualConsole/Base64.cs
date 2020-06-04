using System;
using System.Collections.Generic;
using System.Text;

namespace CasualConsole
{
    public class Base64
    {
        private static readonly char[] chars = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '+', '/', };
        private const byte padding = (byte)'=';

        public static string EncodeBase64(byte[] bytes)
        {
            var newBytes = new byte[PageSize(bytes.Length) * 4];
            var offset = 0;

            foreach (var b in Get3Bytes(bytes))
            {
                int nb1, nb2, nb3, nb4;
                nb1 = nb2 = nb3 = nb4 = 0;

                int validByteCount = 1;

                int b1 = b.Item1.Value;
                nb1 = b1 >> 2;

                nb2 = (b1 & 0b0011) << 4;

                if (b.Item2.HasValue)
                {
                    validByteCount++;

                    var b2 = b.Item2.Value;
                    nb2 |= b2 >> 4;

                    nb3 = (b2 & 0b001111) << 2;

                    if (b.Item3.HasValue)
                    {
                        validByteCount++;

                        var b3 = b.Item3.Value;
                        nb3 |= b3 >> 6;

                        nb4 = b3 & 0b00111111;
                    }
                }

                newBytes[offset] = (byte)chars[nb1];
                newBytes[offset + 1] = (byte)chars[nb2];

                switch (validByteCount)
                {
                    case 1:
                        newBytes[offset + 2] = padding;
                        newBytes[offset + 3] = padding;
                        break;
                    case 2:
                        newBytes[offset + 2] = (byte)chars[nb3];
                        newBytes[offset + 3] = padding;
                        break;
                    case 3:
                        newBytes[offset + 2] = (byte)chars[nb3];
                        newBytes[offset + 3] = (byte)chars[nb4];
                        break;
                    default:
                        throw new Exception();
                }

                offset += 4;
            }

            return Encoding.UTF8.GetString(newBytes);
        }

        private static IEnumerable<(byte?, byte?, byte?)> Get3Bytes(byte[] bytes)
        {
            int safeLength = bytes.Length / 3 * 3;

            for (int i = 0; i < safeLength; i += 3)
            {
                yield return (bytes[i], bytes[i + 1], bytes[i + 2]);
            }

            switch (bytes.Length - safeLength)
            {
                case 2:
                    yield return (bytes[safeLength], bytes[safeLength + 1], null);
                    break;
                case 1:
                    yield return (bytes[safeLength], null, null);
                    break;
                default:
                    break;
            }
        }

        private static int PageSize(int length)
        {
            return (length - 1) / 3 + 1;
        }
    }
}
