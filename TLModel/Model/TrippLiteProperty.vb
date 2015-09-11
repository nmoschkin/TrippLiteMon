Option Explicit On

Imports System.ComponentModel
Imports System.Collections.ObjectModel
Imports System.Reflection
Imports System.Text
Imports DataTools
Imports DataTools.Interop
Imports DataTools.Memory

#Region "TrippLiteProperty Class"

''' <summary>
''' Encapsulates a USB HID feature property for a Tripp Lite Smart Battery.
''' </summary>
''' <remarks></remarks>
Public NotInheritable Class TrippLiteProperty
    Implements INotifyPropertyChanged, IDisposable, IChild(Of TrippLiteUPS)

    Public Event PropertyChanged(sender As Object, e As PropertyChangedEventArgs) Implements INotifyPropertyChanged.PropertyChanged
    Private _propEvent As New PropertyChangedEventArgs("Value")

    Private _Code As TrippLiteCodes

    Friend _Value As Long = -1
    Private _IsSettable As Boolean

    Friend _Model As TrippLiteUPS
    Friend _Bag As TrippLitePropertyBag

    Private _byteLen As UShort = 4

    ''' <summary>
    ''' Initialize a new TrippLiteProperty
    ''' </summary>
    ''' <param name="owner">The TrippLiteUPS model object that will own this property.</param>
    ''' <param name="c">The property code.</param>
    ''' <remarks></remarks>
    Friend Sub New(owner As TrippLiteUPS, c As TrippLiteCodes)
        _Model = owner
        _Code = c
        _byteLen = ByteLength
    End Sub

    ''' <summary>
    ''' Specify whether or not the property object will call to the device, directly, to retrieve (or set) the value of its property.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property LiveInterface As Boolean = False

    ''' <summary>
    ''' Indicates whether this is a property that raises change events.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property IsActiveProperty As Boolean = True

    ''' <summary>
    ''' Gets the owner TrippLiteUPS object for this property.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Owner As TrippLiteUPS Implements IChild(Of TrippLite.TrippLiteUPS).Parent
        Get
            Return _Model
        End Get
        Friend Set(value As TrippLiteUPS)
            _Model = value
        End Set
    End Property

    ''' <summary>
    ''' Gets the hard-coded number format for this property type.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property NumberFormat As String
        Get
            Return GetEnumAttrVal(Of NumberFormatAttribute, String, TrippLiteCodes)(_Code, "Format")
        End Get
    End Property

    ''' <summary>
    ''' Gets the multiplier of the property.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Multiplier As Double
        Get
            Return GetEnumAttrVal(Of MultiplierAttribute, Double, TrippLiteCodes)(_Code, "Value")
        End Get
    End Property

    ''' <summary>
    ''' Gets the length of the USB HID property, in bytes, as defined in TrippLiteCodes.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property ByteLength As UShort
        Get
            ByteLength = GetEnumAttrVal(Of ByteLengthAttribute, UShort, TrippLiteCodes)(_Code, "Length")
            If ByteLength = 0 Then ByteLength = 4
        End Get
    End Property

    ''' <summary>
    ''' Gets or sets the value of the property.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Value As Long
        Get
            If _LiveInterface Then
                _Value = GetValue()
            End If

            Value = _Value
        End Get
        Set(value As Long)
            If LiveInterface AndAlso IsSettable Then
                Select Case _byteLen
                    Case Is <= 4
                        SetValue(CInt(value))

                    Case Is <= 8
                        SetValue(value)

                    Case Else
                        Throw New ArgumentException("This property cannot be set that way.")
                        Return
                End Select

                If _IsActiveProperty Then RaiseEvent PropertyChanged(Me, _propEvent)

            ElseIf (_Value <> value) OrElse (_Value = -1) Then
                _Value = value
                If _IsActiveProperty Then RaiseEvent PropertyChanged(Me, _propEvent)
            End If
        End Set
    End Property

    'Public ReadOnly Property RawValue As Byte()
    '    Get

    '    End Get
    'End Property

    ''' <summary>
    ''' Gets a value indicating whether or not this property supports setting on the device.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property IsSettable As Boolean
        Get
            If _Model Is Nothing Then Return False
            Return _IsSettable
        End Get
        Friend Set(value As Boolean)
            _IsSettable = value
        End Set
    End Property

    ''' <summary>
    ''' Gets the name of the property.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Name As String
        Get
            Return _Code.ToString
        End Get
    End Property

    ''' <summary>
    ''' Gets the description of the property.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Description As String
        Get
            Return GetEnumAttrVal(Of DescriptionAttribute, String, TrippLiteCodes)(_Code, "Description")
        End Get
    End Property

    ''' <summary>
    ''' Gets the unit of the property.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Unit As MeasureUnitTypes
        Get
            Return GetEnumAttrVal(Of MeasureUnitAttribute, MeasureUnitTypes, TrippLiteCodes)(_Code, "Unit")
        End Get
    End Property

    ''' <summary>
    ''' Gets the Tripp Lite property code.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Code As TrippLiteCodes
        Get
            Return _Code
        End Get
    End Property

    ''' <summary>
    ''' Gets detailed information about the property unit as a MeasureUnit object.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property UnitInfo As MeasureUnit
        Get
            Return MeasureUnit.FindUnit(Unit)
        End Get
    End Property

    ''' <summary>
    ''' Attempts to set a value or flag on the device using raw byte data.
    ''' The number of bytes in the byte array must exactly match the byte length of the property.
    ''' </summary>
    ''' <param name="value"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function SetValue(value As Byte()) As Boolean
        If IsSettable = False OrElse value Is Nothing OrElse value.Length <> _byteLen Then Return False

        Dim dev As IntPtr = OpenHid(_Model.Device)

        If dev = IntPtr.Zero Then Return False

        Dim mm As MemPtr
        mm.Alloc(1 + value.Length)

        mm.ByteAt(0) = _Code
        mm.SetBytes(1, value)

        SetValue = HidD_SetFeature(dev, mm.Handle, CInt(mm.Length))
        CloseHid(dev)

        If SetValue Then
            If value.Length >= 8 Then
                _Value = mm.LongAt(1)
            Else
                _Value = mm.IntegerAt(1)
            End If

            mm.Free()
            If _IsActiveProperty Then RaiseEvent PropertyChanged(Me, _propEvent)
        Else
            mm.Free()
        End If

    End Function

    ''' <summary>
    ''' Attempts to retrieve the value directly from the device.
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetValue() As Long

        GetValue = 0
        Dim dev As IntPtr = OpenHid(_Model.Device)
        If dev = IntPtr.Zero Then Return _Value

        Dim mm As MemPtr

        mm.AllocZero(_byteLen + 1)
        mm.ByteAt(0) = _Code

        If HidD_GetFeature(dev, mm.Handle, CInt(mm.Length)) Then
            If _byteLen = 8 Then
                GetValue = mm.LongAtAbsolute(1)
            Else
                GetValue = mm.IntegerAtAbsolute(1)
            End If
        End If

        CloseHid(dev)
        mm.Free()

    End Function

    ''' <summary>
    ''' Attempt to set a value or flag on the device.
    ''' </summary>
    ''' <param name="value"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function SetValue(value As Long) As Boolean
        If _byteLen < 8 Then Return SetValue(CInt(value))

        If IsSettable = False Then Return False
        If Me.Value = value Then Return False

        Dim dev As IntPtr = OpenHid(_Model.Device)

        If dev = IntPtr.Zero Then Return False

        Dim mm As MemPtr

        mm.Alloc(9)

        mm.ByteAt(0) = _Code
        mm.LongAtAbsolute(1) = value

        SetValue = HidD_SetFeature(dev, mm.Handle, CInt(mm.Length))

        mm.Free()
        CloseHid(dev)

        If SetValue Then
            _Value = value
            If _IsActiveProperty Then RaiseEvent PropertyChanged(Me, _propEvent)
        End If
    End Function

    ''' <summary>
    ''' Attempt to set a value or flag on the device.
    ''' </summary>
    ''' <param name="value"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function SetValue(value As Integer) As Boolean
        If _byteLen < 4 Then Return SetValue(CShort(value))

        If IsSettable = False Then Return False
        If Me.Value = value Then Return False

        Dim dev As IntPtr = OpenHid(_Model.Device)

        If dev = IntPtr.Zero Then Return False

        Dim mm As MemPtr
        mm.Alloc(5)

        mm.ByteAt(0) = _Code
        mm.IntegerAtAbsolute(1) = value

        SetValue = HidD_SetFeature(dev, mm.Handle, CInt(mm.Length))

        mm.Free()
        CloseHid(dev)

        If SetValue Then
            _Value = value
            If _IsActiveProperty Then RaiseEvent PropertyChanged(Me, _propEvent)
        End If
    End Function

    ''' <summary>
    ''' Attempt to set a value or flag on the device.
    ''' </summary>
    ''' <param name="value"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function SetValue(value As Short) As Boolean
        If _byteLen < 2 Then Return SetValue(CByte(value))

        If IsSettable = False Then Return False
        If Me.Value = value Then Return False

        Dim dev As IntPtr = OpenHid(_Model.Device)

        If dev = IntPtr.Zero Then Return False

        Dim mm As MemPtr
        mm.Alloc(3)

        mm.ByteAt(0) = _Code
        mm.ShortAtAbsolute(1) = value

        SetValue = HidD_SetFeature(dev, mm.Handle, CInt(mm.Length))

        mm.Free()
        CloseHid(dev)

        If SetValue Then
            _Value = value
            If _IsActiveProperty Then RaiseEvent PropertyChanged(Me, _propEvent)
        End If
    End Function

    ''' <summary>
    ''' Attempt to set a value or flag on the device.
    ''' </summary>
    ''' <param name="value"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function SetValue(value As Byte) As Boolean
        If IsSettable = False Then Return False
        If Me.Value = value Then Return False

        Dim dev As IntPtr = OpenHid(_Model.Device)

        If dev = IntPtr.Zero Then Return False

        Dim mm As MemPtr
        mm.Alloc(2)

        mm.ByteAt(0) = _Code
        mm.ByteAt(1) = value

        SetValue = HidD_SetFeature(dev, mm.Handle, CInt(mm.Length))

        mm.Free()
        CloseHid(dev)

        If SetValue Then
            _Value = value
            If _IsActiveProperty Then RaiseEvent PropertyChanged(Me, _propEvent)
        End If
    End Function

    ''' <summary>
    ''' Attempt to move this item to another property bag.
    ''' </summary>
    ''' <param name="bag">The destination property bag.</param>
    ''' <returns>True if successful.</returns>
    ''' <remarks></remarks>
    Public Function MoveTo(bag As TrippLitePropertyBag) As Boolean
        If _Model Is Nothing OrElse bag Is _Bag Then Return False

        If Owner.PropertyBag.Contains(Me) Then
            If Owner.PropertyBag.MoveTo(Me, bag) Then
                Return True
            End If
        End If
        Return False
    End Function

    ''' <summary>
    ''' Converts this property into its string representation
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overloads Overrides Function ToString() As String
        ToString = ToString(NumberFormat)
    End Function

    ''' <summary>
    ''' Converts this property into its string representation using the provided number format.
    ''' </summary>
    ''' <param name="numFmt">The standard format string used to format the numeric value of this property.</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overloads Function ToString(numFmt As String) As String
        ToString = ToString(numFmt, False)
    End Function

    ''' <summary>
    ''' Converts this property into its string representation using the provided number format.
    ''' </summary>
    ''' <param name="numFmt">The standard format string used to format the numeric value of this property.</param>
    ''' <param name="suppressUnit">Sets a value indicating whether or not to suppress appending the unit symbol to the string value of this property.</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overloads Function ToString(numFmt As String, suppressUnit As Boolean) As String
        ToString = CSng(Value * Multiplier).ToString(numFmt) & If(Not suppressUnit, " " & UnitInfo.UnitSymbol, "")
    End Function

    Public Shared Narrowing Operator CType(v As TrippLiteProperty) As Double
        Return v.Value
    End Operator

    Public Shared Narrowing Operator CType(v As TrippLiteProperty) As Integer
        Return CInt(v.Value)
    End Operator

    Public Shared Narrowing Operator CType(v As TrippLiteProperty) As String
        Return v.ToString
    End Operator

    Friend Sub SignalRefresh()
        If _IsActiveProperty Then RaiseEvent PropertyChanged(Me, _propEvent)
    End Sub

#Region "IDisposable Support"
    Private disposedValue As Boolean ' To detect redundant calls

    ' IDisposable
    Protected Sub Dispose(disposing As Boolean)
        If Not Me.disposedValue Then
            _Model = Nothing
            _Bag = Nothing
        End If
        Me.disposedValue = True
    End Sub

    Protected Overrides Sub Finalize()
        ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
        Dispose(False)
        MyBase.Finalize()
    End Sub

    ' This code added by Visual Basic to correctly implement the disposable pattern.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
#End Region

End Class

#End Region

#Region "TrippLitePropertyBag"

''' <summary>
''' Represents a collection of all USB HID feature properties for a Tripp Lite Smart Battery
''' </summary>
''' <remarks></remarks>
Public Class TrippLitePropertyBag
    Inherits ObservableCollection(Of TrippLiteProperty)
    Implements IDisposable, IChild(Of TrippLiteUPS)

    Private _Model As TrippLiteUPS

    ''' <summary>
    ''' Gets the owner TrippLiteUPS object for this property bag.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Owner As TrippLiteUPS Implements IChild(Of TrippLite.TrippLiteUPS).Parent
        Get
            Return _Model
        End Get
        Friend Set(value As TrippLiteUPS)
            _Model = value
        End Set
    End Property


    Public Function FindProperty(c As TrippLiteCodes) As TrippLiteProperty
        For Each fp In Me
            If fp.Code = c Then Return fp
        Next
        Return Nothing
    End Function

    ''' <summary>
    ''' Initialize a new TrippLitePropertyBag and automatically populate the property bag with known property values from the TrippLiteCodes enumeration.
    ''' </summary>
    ''' <param name="owner">The TrippLiteUPS object upon which to initialize.</param>
    ''' <remarks></remarks>
    Public Sub New(owner As TrippLiteUPS)
        Me.New(owner, True)
    End Sub

    ''' <summary>
    ''' Initialize a new TrippLitePropertyBag
    ''' </summary>
    ''' <param name="owner">The TrippLiteUPS object upon which to initialize.</param>
    ''' <param name="autoPopulate">Specify whether to automatically populate the property bag with known property values from the TrippLiteCodes enumeration.</param>
    ''' <remarks></remarks>
    Public Sub New(owner As TrippLiteUPS, autoPopulate As Boolean)
        MyBase.New()
        _Model = owner

        If autoPopulate Then
            Dim t() As TrippLiteCodes = GetAllEnumVals(Of TrippLiteCodes)()

            Dim i As Integer
            Dim c As Integer = t.Length - 1

            For i = 0 To c
                Me.Add(New TrippLiteProperty(owner, t(i)))
            Next
        End If

    End Sub

    Public Function MoveTo(item As TrippLiteProperty, newBag As TrippLitePropertyBag) As Boolean
        If Me.Contains(item) AndAlso Not newBag.Contains(item) Then
            Me.Remove(item)
            newBag.Add(item)

            item._Model = newBag._Model
            item._Bag = newBag

            Return True
        End If

        Return False
    End Function

    Friend Sub SignalRefresh()
        For Each p In Me
            p.SignalRefresh()
        Next
    End Sub

#Region "IDisposable Support"
    Private disposedValue As Boolean ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                ' TODO: dispose managed state (managed objects).
                For Each x In Me
                    x.Dispose()
                Next
                Me.Clear()
                GC.Collect(0)
            End If

            ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
            ' TODO: set large fields to null.
        End If
        Me.disposedValue = True
    End Sub

    ' TODO: override Finalize() only if Dispose(ByVal disposing As Boolean) above has code to free unmanaged resources.
    'Protected Overrides Sub Finalize()
    '    ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
    '    Dispose(False)
    '    MyBase.Finalize()
    'End Sub

    ' This code added by Visual Basic to correctly implement the disposable pattern.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
#End Region

End Class

#End Region

