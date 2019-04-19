Imports System.IO.MemoryMappedFiles

''' <summary>
'''     A class which creates an IReadable out of a MemoryMappedViewAccessor
''' </summary>
Public Class AccessorAdapter
    Implements IReadable

    ReadOnly _accessor As MemoryMappedViewAccessor

    Public Sub New(accessor As MemoryMappedViewAccessor)
        _accessor = accessor
    End Sub


    Public Function CanRead() As Boolean Implements IReadable.CanRead
        Return _accessor.CanRead
    End Function


    Public Function Capacity() As Long Implements IReadable.Capacity
        Return _accessor.Capacity
    End Function

    Public Function ReadUInt16(offset As Long) As UShort Implements IReadable.ReadUInt16
        Return _accessor.ReadUInt16(offset)
    End Function
End Class