namespace apPatcherApp
{
    using System;
    using System.IO;
    using System.Security.Cryptography;

    public class Crc32 : HashAlgorithm
    {
        public const uint DefaultPolynomial = 0xedb88320;
        public const uint DefaultSeed = uint.MaxValue;
        private static uint[] defaultTable;
        private uint hash;
        private uint seed;
        private uint[] table;

        private long blockCount;
        private long blockIndex = 0;
        private Action<long, long> action = null;

        public Crc32()
        {
            this.table = InitializeTable(0xedb88320);
            this.seed = uint.MaxValue;
            this.Initialize();
        }

        public Crc32(uint polynomial, uint seed)
        {
            this.table = InitializeTable(polynomial);
            this.seed = seed;
            this.Initialize();
        }

        public byte[] ComputeHash(Stream inputStream, Action<long, long> action)
        {
            blockIndex = 0;
            blockCount = (inputStream.Length - 1) / 4096 + 1;
            this.action = action;
            return base.ComputeHash(inputStream);
        }

        [Obsolete("Do not use this function, use with action instead", true)]
        public new byte[] ComputeHash(Stream inputStream) { return null; }

        private uint CalculateHash(uint[] table, uint seed, byte[] buffer, int start, int size)
        {
            uint num = seed;
            for (int i = start; i < size; i++)
            {
                num = (num >> 8) ^ table[(int)((IntPtr)(buffer[i] ^ (num & 0xff)))];
            }

            action(blockIndex++, blockCount);

            return num;
        }

        public uint Compute(byte[] buffer) =>
            ~CalculateHash(InitializeTable(0xedb88320), uint.MaxValue, buffer, 0, buffer.Length);

        public uint Compute(uint seed, byte[] buffer) =>
            ~CalculateHash(InitializeTable(0xedb88320), seed, buffer, 0, buffer.Length);

        public uint Compute(uint polynomial, uint seed, byte[] buffer) =>
            ~CalculateHash(InitializeTable(polynomial), seed, buffer, 0, buffer.Length);

        protected override void HashCore(byte[] buffer, int start, int length)
        {
            this.hash = CalculateHash(this.table, this.hash, buffer, start, length);
        }

        protected override byte[] HashFinal()
        {
            byte[] buffer = this.UInt32ToBigEndianBytes(~this.hash);
            base.HashValue = buffer;
            return buffer;
        }

        public override void Initialize()
        {
            this.hash = this.seed;
        }

        private static uint[] InitializeTable(uint polynomial)
        {
            if ((polynomial == 0xedb88320) && (defaultTable != null))
            {
                return defaultTable;
            }
            uint[] numArray = new uint[0x100];
            for (int i = 0; i < 0x100; i++)
            {
                uint num2 = (uint)i;
                for (int j = 0; j < 8; j++)
                {
                    if ((num2 & 1) == 1)
                    {
                        num2 = (num2 >> 1) ^ polynomial;
                    }
                    else
                    {
                        num2 = num2 >> 1;
                    }
                }
                numArray[i] = num2;
            }
            if (polynomial == 0xedb88320)
            {
                defaultTable = numArray;
            }
            return numArray;
        }

        private byte[] UInt32ToBigEndianBytes(uint x) =>
            new byte[] { ((byte)((x >> 0x18) & 0xff)), ((byte)((x >> 0x10) & 0xff)), ((byte)((x >> 8) & 0xff)), ((byte)(x & 0xff)) };

        public override int HashSize =>
            0x20;
    }
}

