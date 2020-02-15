using System;
using System.Collections.Generic;

namespace Decryptor
{
    public class CoreEncryption
    {
        private readonly Lazy<byte[]> password;

        private static CoreEncryption _instance;
        public static CoreEncryption Instance
        {
            get => _instance;
        }

        public CoreEncryption(string passwordString)
        {
            password = new Lazy<byte[]>(() => ConvertStringToBytes(passwordString));
            _instance = this;
        }

        public void EncryptInPlace(byte[] content)
        {
            var pass = password.Value;

            for (int i = 0; i < content.Length; i++)
            {
                content[i] = (byte)(content[i] + pass[i % pass.Length]);
            }
        }

        public void DecryptInPlace(byte[] content)
        {
            var pass = password.Value;

            for (int i = 0; i < content.Length; i++)
            {
                content[i] = (byte)(content[i] - pass[i % pass.Length]);
            }
        }

        private byte[] ConvertStringToBytes(string s)
        {
            s = s.ToLowerInvariant();

            IEnumerable<(char c1, char c2)> GetBytes(string str)
            {
                for (int i = 0; i < str.Length / 2; i++)
                {
                    yield return (str[i], str[i + 1]);
                }
            }

            int ToInt(char c)
            {
                if (c >= 'a')
                    return c - 'a';
                else
                    return c - '0';
            }

            var resultArray = new byte[s.Length / 2];
            int j = 0;
            foreach (var charPair in GetBytes(s))
            {
                int b = (ToInt(charPair.c1) << 1) + ToInt(charPair.c2);
                resultArray[j] = (byte)b;
                j++;
            }

            return resultArray;
        }
    }
}
