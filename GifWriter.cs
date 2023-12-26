using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

public class GifWriter : IDisposable
{
    private const long SourceGlobalColorInfoPosition = 10;
    private const long SourceImageBlockPosition = 789;

    private readonly BinaryWriter _writer;
    private bool _firstFrame = true;
    private readonly object _syncLock = new object();

    public GifWriter(Stream outStream, int defaultFrameDelay = 500, int repeat = -1)
    {
        if (outStream == null)
            throw new ArgumentNullException(nameof(outStream));

        if (defaultFrameDelay <= 0)
            throw new ArgumentOutOfRangeException(nameof(defaultFrameDelay));

        if (repeat < -1)
            throw new ArgumentOutOfRangeException(nameof(repeat));

        _writer = new BinaryWriter(outStream);
        DefaultFrameDelay = defaultFrameDelay;
        Repeat = repeat;
    }

    public GifWriter(string fileName, int defaultFrameDelay = 500, int repeat = -1)
        : this(new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read), defaultFrameDelay, repeat) { }

    public int DefaultWidth { get; set; }
    public int DefaultHeight { get; set; }
    public int DefaultFrameDelay { get; set; }
    public int Repeat { get; }

    public void WriteFrame(Image image, int delay = 0)
    {
        lock (_syncLock)
        {
            using (var gifStream = new MemoryStream())
            {
                image.Save(gifStream, ImageFormat.Gif);

                if (_firstFrame)
                    InitHeader(gifStream, _writer, image.Width, image.Height);

                WriteGraphicControlBlock(gifStream, _writer, delay == 0 ? DefaultFrameDelay : delay);
                WriteImageBlock(gifStream, _writer, !_firstFrame, 0, 0, image.Width, image.Height);
            }

            if (_firstFrame)
                _firstFrame = false;

            _writer.Flush();
        }
    }

    private void InitHeader(Stream sourceGif, BinaryWriter writer, int width, int height)
    {
        writer.Write("GIF".ToCharArray());
        writer.Write("89a".ToCharArray());
        writer.Write((short)(DefaultWidth == 0 ? width : DefaultWidth));
        writer.Write((short)(DefaultHeight == 0 ? height : DefaultHeight));

        sourceGif.Position = SourceGlobalColorInfoPosition;
        writer.Write((byte)sourceGif.ReadByte());
        writer.Write((byte)0);
        writer.Write((byte)0);
        WriteColorTable(sourceGif, writer);

        if (Repeat == -1)
            return;

        writer.Write(unchecked((short)0xff21));
        writer.Write((byte)0x0b);
        writer.Write("NETSCAPE2.0".ToCharArray());
        writer.Write((byte)3);
        writer.Write((byte)1);
        writer.Write((short)Repeat);
        writer.Write((byte)0);
    }

    private static void WriteColorTable(Stream sourceGif, BinaryWriter writer)
    {
        sourceGif.Position = 13;
        var colorTable = new byte[768];
        sourceGif.Read(colorTable, 0, colorTable.Length);
        writer.Write(colorTable, 0, colorTable.Length);
    }

    private static void WriteGraphicControlBlock(Stream sourceGif, BinaryWriter writer, int frameDelay)
    {
        sourceGif.Position = 781;
        var blockhead = new byte[8];
        sourceGif.Read(blockhead, 0, blockhead.Length);

        writer.Write(unchecked((short)0xf921));
        writer.Write((byte)0x04);
        writer.Write((byte)(blockhead[3] & 0xf7 | 0x08));
        writer.Write((short)(frameDelay / 10));
        writer.Write(blockhead[6]);
        writer.Write((byte)0);
    }

    private static void WriteImageBlock(Stream sourceGif, BinaryWriter writer, bool includeColorTable, int x, int y, int width, int height)
    {
        sourceGif.Position = SourceImageBlockPosition;
        var header = new byte[11];
        sourceGif.Read(header, 0, header.Length);
        writer.Write(header[0]);
        writer.Write((short)x);
        writer.Write((short)y);
        writer.Write((short)width);
        writer.Write((short)height);

        if (includeColorTable)
        {
            sourceGif.Position = SourceGlobalColorInfoPosition;
            writer.Write((byte)(sourceGif.ReadByte() & 0x3f | 0x80));
            WriteColorTable(sourceGif, writer);
        }
        else writer.Write((byte)(header[9] & 0x07 | 0x07));

        writer.Write(header[10]);

        sourceGif.Position = SourceImageBlockPosition + header.Length;

        var dataLength = sourceGif.ReadByte();
        while (dataLength > 0)
        {
            var imgData = new byte[dataLength];
            sourceGif.Read(imgData, 0, dataLength);

            writer.Write((byte)dataLength);
            writer.Write(imgData, 0, dataLength);
            dataLength = sourceGif.ReadByte();
        }

        writer.Write((byte)0);
    }

    public void Dispose()
    {
        _writer.Write((byte)0x3b);
        _writer.BaseStream.Dispose();
        _writer.Dispose();
    }

    public void Flush()
    {
        _writer.Flush();
    }
}