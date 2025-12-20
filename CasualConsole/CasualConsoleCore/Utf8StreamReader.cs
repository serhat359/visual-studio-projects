using System;
using System.IO;

namespace CasualConsoleCore;

public class Utf8StreamReader
{
    private readonly Stream stream;
    private readonly byte[] buffer;
    private int start = -1;
    private int end;
    private int lastByteReadCount = -1;

    const byte ln = (byte)'\n';

    public Utf8StreamReader(Stream stream, int bufferSize = 30000)
    {
        this.stream = stream;
        this.buffer = new byte[bufferSize];
    }

    public bool TryReadLine(out ReadOnlySpan<byte> ret)
    {
        if (lastByteReadCount == 0 && start == end)
        {
            ret = default;
            return false;
        }

        if (start == -1)
        {
            // first run
            start = 0;
            lastByteReadCount = end = stream.Read(buffer);

            // Check for UTF8 BOM
            if (end >= 3 && buffer[0] == 239 && buffer[1] == 187 && buffer[2] == 191)
                start = 3;
        }

        while (true)
        {
            // start should always be after new line
            var wholeBufferSpan = buffer.AsSpan();
            var bufferSpan = wholeBufferSpan[start..end];
            int i = bufferSpan.IndexOf(ln);
            if (i >= 0)
            {
                // If new line was found
                ret = bufferSpan[..i];
                if (ret.Length > 0 && ret[^1] == '\r')
                    ret = ret[..^1];
                start += i + 1;
                return true;
            }

            // New line was not found
            int remainingLength = end - start;
            bufferSpan.CopyTo(buffer);
            var target = wholeBufferSpan[remainingLength..];
            lastByteReadCount = stream.Read(target);
            if (lastByteReadCount == 0)
            {
                if (remainingLength == 0)
                {
                    ret = default;
                    return false;
                }
                else
                {
                    // Last line
                    ret = bufferSpan[..remainingLength];
                    start = end = remainingLength;
                    return true;
                }
            }
            this.end = remainingLength + lastByteReadCount;
            this.start = 0;
        }
    }
}