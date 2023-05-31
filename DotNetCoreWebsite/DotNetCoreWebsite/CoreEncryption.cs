using System;

namespace DotNetCoreWebsite
{
    public class CoreEncryption
    {
        private readonly Lazy<byte[]> password;

        public CoreEncryption(string passwordString)
        {
            password = new Lazy<byte[]>(() => ConvertStringToBytes(passwordString));
        }

        public void EncryptInPlace(byte[] content, long misalignment)
        {
            var pass = password.Value;

            for (int i = 0; i < content.Length; i++)
            {
                content[i] = (byte)(content[i] + pass[(i + misalignment) % pass.Length]);
            }
        }

        private byte[] ConvertStringToBytes(string s)
        {
            s = s.ToLowerInvariant();

            int ToInt(char c)
            {
                if (c >= 'a')
                    return c - 'a' + 10;
                else
                    return c - '0';
            }

            var resultArray = new byte[s.Length / 2];

            for (int i = 0; i < s.Length; i += 2)
            {
                int b = (ToInt(s[i]) << 4) + ToInt(s[i + 1]);
                resultArray[i / 2] = (byte)b;
            }

            return resultArray;
        }
    }
}
