using System;
using System.IO;

namespace CasualConsoleCore;

public class Utf8StreamReader
{
    private readonly Stream stream;
    private byte[] buffer;
    private int start = -1;
    private int end;
    private int lastByteReadCount = -1;

    const byte ln = (byte)'\n';

    public Utf8StreamReader(Stream stream, int bufferSize = 30000)
    {
        if (bufferSize <= 0) throw new ArgumentException($"{nameof(bufferSize)} must be positive", nameof(bufferSize));

        this.stream = stream;
        this.buffer = new byte[bufferSize];
    }

    public bool TryReadLine(out ReadOnlySpan<byte> ret)
    {
    begin:
        if (lastByteReadCount == 0 && start == end)
        {
            ret = default;
            return false;
        }

        if (start == -1)
        {
            // First run
            start = 0;
            lastByteReadCount = end = stream.Read(buffer);

            // Check for UTF8 BOM
            if (end >= 3 && buffer[0] == 239 && buffer[1] == 187 && buffer[2] == 191)
                start = 3;
        }

        while (true)
        {
            // Start should always be after new line
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
            if (remainingLength == buffer.Length)
            {
                // Buffer must be too small
                var newBuffer = new byte[buffer.Length * 2];
                buffer.CopyTo(newBuffer);
                this.buffer = newBuffer;
                start = 0;
                int read = stream.Read(buffer.AsSpan()[remainingLength..]);
                lastByteReadCount = end = read + remainingLength;
                goto begin;
            }

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