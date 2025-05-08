using System;
using System.IO;

namespace CasualConsoleCore;

public class Utf8StreamReader
{
    private readonly Stream stream;
    private readonly byte[] buffer;
    private int start = -1;
    private int end;

    const byte ln = (byte)'\n';

    public Utf8StreamReader(Stream stream, int bufferSize = 30000)
    {
        this.stream = stream;
        this.buffer = new byte[bufferSize];
    }

    public bool TryReadLine(out ReadOnlySpan<byte> ret)
    {
        if (start == -1)
        {
            // first run
            start = 0;
            end = stream.Read(buffer);
        }

        while (true)
        {
            // start should always be after new line
            var wholeBufferSpan = buffer.AsSpan();
            var bufferSpan = wholeBufferSpan[start..end];
            int i = bufferSpan.IndexOf(ln);
            if (i >= 0)
            {
                ret = bufferSpan[..i];
                start += i + 1;
                return true;
            }

            int remainingLength = end - start;
            bufferSpan.CopyTo(buffer);
            var target = wholeBufferSpan[remainingLength..];
            int count = stream.Read(target);
            if (count == 0)
            {
                if (remainingLength == 0)
                {
                    ret = default;
                    return false;
                }
                throw new Exception();
            }
            this.end = remainingLength + count;
            this.start = 0;
        }
    }
}