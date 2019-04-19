''' <summary>
'''     Something which can be read.
''' </summary>
Public Interface IReadable
    Function CanRead() As Boolean
    Function Capacity() As Long
    Function ReadUInt16(offset As Long) As UShort
End Interface