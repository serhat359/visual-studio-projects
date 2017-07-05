namespace CSEncryptDecrypt
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using System.Text;

    public class encryptRoutineType
    {
        public CryptoStream createDecryptionReadStream(string sKey, FileStream fs)
        {
            ICryptoTransform transform = new DESCryptoServiceProvider { 
                Key = Encoding.ASCII.GetBytes(sKey),
                IV = Encoding.ASCII.GetBytes(sKey)
            }.CreateDecryptor();
            return new CryptoStream(fs, transform, CryptoStreamMode.Read);
        }

        public void DecryptFile(string sInputFilename, string sOutputFilename, string sKey, GCHandle gch)
        {
            DESCryptoServiceProvider provider = new DESCryptoServiceProvider {
                Key = Encoding.ASCII.GetBytes(sKey),
                IV = Encoding.ASCII.GetBytes(sKey)
            };
            FileStream stream = new FileStream(sInputFilename, FileMode.Open, FileAccess.Read);
            ICryptoTransform transform = provider.CreateDecryptor();
            CryptoStream stream2 = new CryptoStream(stream, transform, CryptoStreamMode.Read);
            StreamWriter writer = new StreamWriter(sOutputFilename);
            writer.Write(new StreamReader(stream2).ReadToEnd());
            writer.Flush();
            writer.Close();
            ZeroMemory(gch.AddrOfPinnedObject(), sKey.Length * 2);
            gch.Free();
        }

        public void EncryptFile(string sInputFilename, string sOutputFilename, string sKey, GCHandle gch)
        {
            FileStream stream = new FileStream(sInputFilename, FileMode.Open, FileAccess.Read);
            FileStream stream2 = new FileStream(sOutputFilename, FileMode.Create, FileAccess.Write);
            ICryptoTransform transform = new DESCryptoServiceProvider { 
                Key = Encoding.ASCII.GetBytes(sKey),
                IV = Encoding.ASCII.GetBytes(sKey)
            }.CreateEncryptor();
            CryptoStream stream3 = new CryptoStream(stream2, transform, CryptoStreamMode.Write);
            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);
            stream3.Write(buffer, 0, buffer.Length);
            stream3.Close();
            stream.Close();
            stream2.Close();
            ZeroMemory(gch.AddrOfPinnedObject(), sKey.Length * 2);
            gch.Free();
        }

        public string GenerateKey()
        {
            DESCryptoServiceProvider provider = (DESCryptoServiceProvider) DES.Create();
            return Encoding.ASCII.GetString(provider.Key);
        }

        [DllImport("KERNEL32.DLL", EntryPoint="RtlZeroMemory")]
        public static extern bool ZeroMemory(IntPtr Destination, int Length);
    }
}

