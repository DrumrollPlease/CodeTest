
Imports System.IO.MemoryMappedFiles
Imports System.Threading

''' <summary>
'''     This is used to find JPEG files that ma be buried inside another file. The provided file should be less than 2GB.
'''     This can be used as part of a solution to recover JPEG files that have been deleted from an SD card.
''' </summary>
Public Class JpegFinder
    Private ReadOnly _readable As IReadable
    Private _offset As Long

    ''' <summary>
    '''     Creates a <c>JpegFinder</c> that can iterate through any JPEGs found in the provided <paramref name="readable" />.
    ''' </summary>
    ''' <param name="readable"></param>
    Public Sub New(readable As IReadable)
        _readable = readable
    End Sub


    Public Sub New(accessor As MemoryMappedViewAccessor)
        _readable = New AccessorAdapter(accessor)
    End Sub

    ''' <summary>
    '''     Return version for internal purposes.
    ''' </summary>
    Public Function GetVersion()
        Return "1.0 alpha"
    End Function

    ''' <summary>
    '''     Return date of last version for internal purposes.
    ''' </summary>
    Public Function GetDate() As Date
        Return #2019/2/2#
    End Function

    ''' <summary>
    '''     Returns an <c>IEnumerable</c> containing the offsets of every JPEG located in the <c>MemoryMappedViewAccessor</c>
    '''     passed into the constructor.
    ''' </summary>
    Public Iterator Function FindJpegs() As IEnumerable(Of Tuple(Of Long, Long))

        If Not _readable.CanRead Then Throw New Exception("Cannot read from source")

        While _offset < _readable.Capacity() - 2
            Dim startOfJpeg = FindSoi(_offset)
            If startOfJpeg = -1 Then Return
            Dim limits = WalkJpeg(startOfJpeg)
            If limits Is Nothing Then Continue While
            Yield limits
        End While
    End Function

    ''' <summary>
    '''     This loosely parses JPEG data so And returns a <code>Tuple&lt;long, long&gt;</code> containing the first And last
    '''     offsets of the JPEG bytes within the file. It does Not parse data within the markers; it simply determines their
    '''     length where possible And skips over them. This means that it Is possible that it may misidentify the bounds of
    '''     faulty JPEGs.
    ''' </summary>
    ''' <param name="startOfJpeg">The offset of the first byte of the JPEG data.</param>
    ''' <returns>
    '''     A Tuple&lt;long, long&gt; containing the offsets of the first And last byte of the JPEG, Or null if no complete
    '''     JPEG was found.
    ''' </returns>
    Private Function WalkJpeg(startOfJpeg As Long) As Tuple(Of Long, Long)
        Dim position = startOfJpeg
        If _readable.ReadUInt16(position) <> &HD8FF Then Return Nothing
        position += 2

        While position < _readable.Capacity() - 4
            Dim marker = FlipUShort(_readable.ReadUInt16(position))
            Dim length = FlipUShort(_readable.ReadUInt16(position + 2))

            If marker = &HFFFF Then
                ' Skip filler byte
                position += 1
            ElseIf (marker And &HFF) >= &HD0 AndAlso (marker And &HFF) <= &HD7 Then
                ' RST markers with no params
                position += 2
            ElseIf marker = &HFFDA Then
                ' SOS marker; skip and then continue until EOI
                position += length + 2
                marker = FlipUShort(_readable.ReadUInt16(position))
                While marker <> &HFFD9
                    position = position + 1
                    marker = _readable.ReadUInt16(position)
                End While
                _offset = position + 2
                Return New Tuple(Of Long, Long)(startOfJpeg, position + 1)
            ElseIf (marker And &HFF00) = &HFF00 Then
                ' All other markers have lengths or are reserved. Skip over.
                position += length + 2
            Else
                ' No marker found. Invalid JPEG.
                _offset += 1
                Exit While
            End If
        End While

        Return Nothing
    End Function

    ''' <summary>
    '''     Flip the bytes of a UShort to handle Big-Endian numbers read as Little-Endian.
    ''' </summary>
    Private Function FlipUShort(value As UShort) As UShort
        Return CType((((value And &HFF) << 8) Or ((value And &HFF00) >> 8)), UShort)
    End Function

    ''' <summary>
    '''     Returns the location of the first SOI marker found at Or after the provided <paramref name="offset" /> .
    ''' </summary>
    ''' <param name="offset">The memory location to start searching from.</param>
    ''' <returns>The offset of the SOI maker if found, Or -1 if none were found.</returns>
    Private Function FindSoi(offset As Long) As Long
        While offset < _readable.Capacity() - 2
            Dim marker = FlipUShort(_readable.ReadUInt16(offset))
            If marker = &HFFD8 Then Return offset
            offset += 1
        End While
        Return -1
    End Function
End Class