using System;
using System.Collections.Generic;
using System.Numerics;

namespace DotNetCoreWebsite
{
    public class CoreEncryption
    {
        private readonly Lazy<byte[]> password;
        private readonly Dictionary<int, byte[]> alignedKeys = new();

        public CoreEncryption(string passwordString)
        {
            password = new Lazy<byte[]>(() => ConvertStringToBytes(passwordString));
        }

        public void EncryptInPlace(byte[] content, long misalignment)
        {
            var pass = password.Value;
            pass = GetAlignedKey(pass, (int)(misalignment % pass.Length));
            pass = ExpandToSize(pass, Vector<byte>.Count);

            if (pass.Length != Vector<byte>.Count) throw new Exception();

            EncryptFast(content, Vector<byte>.Count, new Vector<byte>(pass));
        }

        private void EncryptFast(Span<byte> originalData, int keySize, Vector<byte> keyVector)
        {
            int remainder = originalData.Length % keySize;
            int safeLength = originalData.Length - remainder;

            int processed = 0;
            while (processed < safeLength)
            {
                var smallSpan = originalData.Slice(processed, keySize);
                Vector<byte> result = Vector.Add(new Vector<byte>(smallSpan), keyVector);
                result.CopyTo(smallSpan);
                processed += keySize;
            }
            if (remainder > 0)
            {
                var smallSpan = originalData[processed..originalData.Length];
                var newBuffer = new byte[keySize];
                smallSpan.CopyTo(newBuffer);
                Vector<byte> result = Vector.Add(new Vector<byte>(smallSpan), keyVector);

                for (int i = 0; i < smallSpan.Length; i++)
                {
                    smallSpan[i] = result[i];
                }
            }
        }

        private byte[] ExpandToSize(byte[] key, int size)
        {
            if (size % key.Length != 0)
                throw new Exception("incompatible size");

            var newKey = new byte[size];
            int times = size / key.Length;
            for (int i = 0; i < times; i++)
            {
                Array.Copy(key, 0, newKey, i * key.Length, key.Length);
            }
            return newKey;
        }

        private byte[] GetAlignedKey(byte[] key, int misalignment)
        {
            if (alignedKeys.TryGetValue(misalignment, out var value))
            {
                return value;
            }
            value = AlignKey(key, misalignment);
            alignedKeys[misalignment] = value;
            return value;
        }

        private byte[] AlignKey(byte[] key, int misalignment)
        {
            if (misalignment == 0)
                return key;
            var newKey = new byte[key.Length];
            for (int i = 0; i < key.Length; i++)
            {
                newKey[i] = key[(i + misalignment) % key.Length];
            }
            return newKey;
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
