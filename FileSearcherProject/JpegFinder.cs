using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;

namespace FileSearcherProject
{
  /// <summary>
  ///  This class is used to find JPEG files that may be buried inside another file. The provided file should be less than 2GB.
  ///  This can be used as part of a solution to recover JPEG files that have been deleted from an SD card.
  /// </summary>
  /// 
  public class JpegFinder
  {
    #region Private Members

    private readonly IReadable _readable;

    // TODO:  pull from standard project property file
    private const string _version = "1.0 alpha";
    private const string _versionDate = "2019/02/02";

    #endregion

    #region Public Properties

    public string Version
    {
      get { return _version; }
    }

    public string VersionDate
    {
      get { return _versionDate; }
    }

    #endregion

    #region Constructor(s)

    public JpegFinder(IReadable readable)
    {
      _readable = readable;
    }

    public JpegFinder(MemoryMappedViewAccessor accessor)
    {
      _readable = new AccessorAdapter(accessor);
    }

    #endregion

    #region Public Methods

    /// <summary>
    ///   Returns an <c>IEnumerable</c> containing the offsets of every JPEG located in the <c>MemoryMappedViewAccessor</c>
    ///   passed into the constructor.
    /// </summary>
    public IEnumerable<Tuple<long, long>> FindJpegs()
    {
      if (!_readable.CanRead)
      {
        throw new Exception("Cannot read from source");
      }

      long offset = 0;

      while (offset < _readable.Capacity - 2)
      {
        var startOfJpeg = FindSoi(offset);
        if (startOfJpeg == -1)
        {
          break;
        }
        var limits = WalkJpeg(startOfJpeg, ref offset);
        if (limits == null)
        {
          continue;
        }

        yield return limits;
      }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// This loosely parses JPEG data so And returns a <code>Tuple&lt;long, long&gt;</code> containing the first And last
    /// offsets of the JPEG bytes within the file. It does Not parse data within the markers; it simply determines their
    /// length where possible And skips over them. This means that it Is possible that it may misidentify the bounds of
    /// faulty JPEGs.
    /// </summary>
    /// <param name="startOfJpeg">The offset of the first byte of the JPEG data.</param>
    /// <param name="offset">Current file offset that will be updated.</param>
    /// <returns>
    /// A Tuple&lt;long, long&gt; containing the offsets of the first And last byte of the JPEG, Or null if no complete
    /// JPEG was found.
    /// </returns>
    /// </summary>
    private Tuple<long, long> WalkJpeg(long startOfJpeg, ref long offset)
    {
      var position = startOfJpeg;
      if (_readable.ReadUInt16(position) != 0xD8FF)
      {
        return null;
      }

      // TODO:  explain += 2
      position += 2;

      // TODO: again... - 4
      while (position < _readable.Capacity - 4)
      {
        var marker = FlipUShort(_readable.ReadUInt16(position));
        var length = FlipUShort(_readable.ReadUInt16(position + 2));

        if (marker == 0xFFFF)
        {
          // Skip filler byte
          position += 1;
        }
        else if ((marker & 0xFF) >= 0xD0 && (marker & 0xFF) <= 0xD7)
        {
          // RST markers with no params
          position += 2;
        }
        else if (marker == 0xFFDA)
        {
          // SOS marker; skip and then continue until EOI
          position += length + 2;
          marker = FlipUShort(_readable.ReadUInt16(position));

          while (marker != 0xFFD9)
          {
            position++;
            marker = _readable.ReadUInt16(position);
          }

          offset = position + 2;
          return new Tuple<long, long>(startOfJpeg, position + 1);
        }
        else if ((marker & 0xFF00) == 0xFF00)
        {
          // All other markers have lengths or are reserved. Skip over.
          position += length + 2;
        }
        else
        {
          // No marker found. Invalid JPEG.
          offset += 1;
          break;
        }
      }

      return null;
    }

    /// <summary>
    /// Flip the bytes of a ushort to handle Big-Endian numbers read as Little-Endian.
    /// </summary>
    /// <param name="value"></param>
    private ushort FlipUShort(ushort value)
    {
      return (ushort)(((value & 0xFF) << 8) | ((value & 0xFF00) >> 8));
    }

    /// <summary>
    ///     Returns the location of the first SOI marker found at Or after the provided <paramref name="offset" /> .
    /// </summary>
    /// <param name="offset">The memory location to start searching from.</param>
    /// <returns>The offset of the SOI maker if found, Or -1 if none were found.</returns>
    private long FindSoi(long offset)
    {
      long result = -1;

      while (offset < _readable.Capacity - 2)
      {
        var marker = FlipUShort(_readable.ReadUInt16(offset));
        if (marker == 0xFFD8)
        {
          result = offset;
          break;
        }

        offset++;
      }

      return result;
    }

    #endregion

    #region Private Tests

#if DEBUG

    public long TestPrivateFindSoi(long offset)
    {
      return FindSoi(offset);
    }

#endif

    #endregion

  }
}
