using System.IO.MemoryMappedFiles;

namespace FileSearcherProject
{
  public class AccessorAdapter : IReadable
  {
    #region Private Members

    private readonly MemoryMappedViewAccessor _accessor;

    #endregion

    #region Constructor(s)

    public AccessorAdapter(MemoryMappedViewAccessor accessor)
    {
      _accessor = accessor;
    }

    #endregion

    #region Public Properties

    /// <summary>
    /// returns whether or not file can be read
    /// </summary>
    public bool CanRead { get { return _accessor.CanRead; } }

    /// <summary>
    /// returns capacity of file
    /// </summary>
    public long Capacity { get { return _accessor.Capacity; } }

    #endregion

    #region Public Methods

    public ushort ReadUInt16(long offset)
    {
      return _accessor.ReadUInt16(offset);
    }

    #endregion

  }
}
