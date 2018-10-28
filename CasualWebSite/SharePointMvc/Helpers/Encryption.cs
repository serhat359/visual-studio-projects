using System.Security.Cryptography;

namespace SharePointMvc.Helpers
{
    public class Encryption
    {
        private static byte[] key = new byte[8] { 1, 2, 3, 4, 5, 6, 7, 8 };
        private static byte[] iv = new byte[8] { 1, 2, 3, 4, 5, 6, 7, 8 };
        private const byte xorKey = 58;

        public static byte[] Crypt(byte[] text)
        {
            SymmetricAlgorithm algorithm = DES.Create();
            ICryptoTransform transform = algorithm.CreateEncryptor(key, iv);
            byte[] inputbuffer = text;
            byte[] outputBuffer = transform.TransformFinalBlock(inputbuffer, 0, inputbuffer.Length);
            return outputBuffer;
        }

        public static byte[] Decrypt(byte[] text)
        {
            SymmetricAlgorithm algorithm = DES.Create();
            ICryptoTransform transform = algorithm.CreateDecryptor(key, iv);
            byte[] inputbuffer = text;
            byte[] outputBuffer = transform.TransformFinalBlock(inputbuffer, 0, inputbuffer.Length);
            return outputBuffer;
        }

        public static void XorInplaceCrypt(byte[] text)
        {
            for (int i = 0; i < text.Length; i++)
            {
                text[i] = (byte)(text[i] ^ xorKey);
            }
        }
    }
}