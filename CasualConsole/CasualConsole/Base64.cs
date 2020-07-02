using System;
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

            var safeLength = bytes.Length / 3 * 3;

            for (int i = 0; i < safeLength; i += 3)
            {
                var b1 = bytes[i];
                var b2 = bytes[i + 1];
                var b3 = bytes[i + 2];

                int nb1 = b1 >> 2;

                int nb2 = (b1 & 0b0011) << 4;
                nb2 |= b2 >> 4;

                int nb3 = (b2 & 0b001111) << 2;
                nb3 |= b3 >> 6;

                int nb4 = b3 & 0b00111111;

                newBytes[offset] = (byte)chars[nb1];
                newBytes[offset + 1] = (byte)chars[nb2];
                newBytes[offset + 2] = (byte)chars[nb3];
                newBytes[offset + 3] = (byte)chars[nb4];

                offset += 4;
            }

            if (bytes.Length > safeLength)
            {
                byte? by1 = safeLength + 0 < bytes.Length ? bytes[safeLength + 0] : (byte?)null;
                byte? by2 = safeLength + 1 < bytes.Length ? bytes[safeLength + 1] : (byte?)null;
                byte? by3 = safeLength + 2 < bytes.Length ? bytes[safeLength + 2] : (byte?)null;

                int nb1, nb2, nb3, nb4;
                nb1 = nb2 = nb3 = nb4 = 0;

                int validByteCount = 1;

                int b1 = by1.Value;
                nb1 = b1 >> 2;

                nb2 = (b1 & 0b0011) << 4;

                if (by2.HasValue)
                {
                    validByteCount++;

                    var b2 = by2.Value;
                    nb2 |= b2 >> 4;

                    nb3 = (b2 & 0b001111) << 2;

                    if (by3.HasValue)
                    {
                        validByteCount++;

                        var b3 = by3.Value;
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
            }

            return Encoding.UTF8.GetString(newBytes);
        }

        private static int PageSize(int length)
        {
            return (length - 1) / 3 + 1;
        }
    }
}
