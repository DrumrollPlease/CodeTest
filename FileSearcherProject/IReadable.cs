/// <summary>
/// Interface for a readable object
/// </summary>

namespace FileSearcherProject
{
  public interface IReadable
  {
    /// <summary>
    /// returns whether or not file can be read
    /// </summary>
    bool CanRead { get; }

    /// <summary>
    /// returns capacity of file
    /// </summary>
    long Capacity { get; }

    /// <summary>
    /// reads a ushort from a file from the given offset
    /// </summary>
    /// <param name="offset">the offset into the file</param>
    /// <returns>resulting ushort integer</returns>
    ushort ReadUInt16(long offset);
  }
}
