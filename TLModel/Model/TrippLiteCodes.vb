Option Strict On

Imports System.ComponentModel
Imports System.Collections.ObjectModel
Imports System.Reflection
Imports System.Text
Imports DataTools

#Region "Utility Functions"

Module TrippLiteCodeUtility

    ''' <summary>
    ''' Gets the specified property of the specified attribute type for the specified enumeration member.
    ''' </summary>
    ''' <typeparam name="T">The attribute type from which to retrieve the property.</typeparam>
    ''' <typeparam name="U">The type of the attribute property to retrieve.</typeparam>
    ''' <typeparam name="V">The enum type.</typeparam>
    ''' <param name="o">The enum value for which to retrieve the attribute.</param>
    ''' <param name="valueName">The name of the property inside the Attribute object to retrieve.</param>
    ''' <param name="b">Optional binding flags for member searching.</param>
    ''' <returns>A value of type U that represents the value of the property of the attribute of the member of the enumeration.</returns>
    ''' <remarks></remarks>
    Public Function GetEnumAttrVal(Of T As Attribute, U, V)(o As V, valueName As String, Optional b As BindingFlags = BindingFlags.Public Or BindingFlags.Static) As U

        Dim da As T
        Dim fi() As FieldInfo = o.GetType.GetFields(b)

        For Each fe In fi
            If o.ToString = fe.GetValue(o).ToString Then
                da = CType(fe.GetCustomAttribute(GetType(T)), T)
                If da IsNot Nothing Then
                    Dim f As PropertyInfo = da.GetType.GetProperty(valueName)
                    Return CType(f.GetValue(da), U)
                End If
            End If

        Next
        Return Nothing
    End Function

    Public Function GetAllDescriptions(Of T)(o As T, Optional b As BindingFlags = BindingFlags.Public Or BindingFlags.Static) As List(Of KeyValuePair(Of T, String))

        Dim da As DescriptionAttribute
        Dim fi() As FieldInfo = o.GetType.GetFields(b)
        Dim c As Integer = 0

        Dim l As New List(Of KeyValuePair(Of T, String))
        Dim kv As KeyValuePair(Of T, String)

        For Each fe In fi

            da = CType(fe.GetCustomAttribute(GetType(DescriptionAttribute)), DescriptionAttribute)
            If da IsNot Nothing Then

                kv = New KeyValuePair(Of T, String)(CType(fe.GetValue(o), T), da.Description)
                l.Add(kv)
            End If

        Next
        Return l
    End Function

    Public Function GetAllSymbols(Of T)(o As T, Optional b As BindingFlags = BindingFlags.Public Or BindingFlags.Static) As List(Of KeyValuePair(Of T, String))

        Dim da As UnitSymbolAttribute
        Dim fi() As FieldInfo = o.GetType.GetFields(b)
        Dim c As Integer = 0

        Dim l As New List(Of KeyValuePair(Of T, String))
        Dim kv As KeyValuePair(Of T, String)

        For Each fe In fi

            da = CType(fe.GetCustomAttribute(GetType(UnitSymbolAttribute)), UnitSymbolAttribute)
            If da IsNot Nothing Then

                kv = New KeyValuePair(Of T, String)(CType(fe.GetValue(o), T), da.Symbol)
                l.Add(kv)
            End If

        Next
        Return l
    End Function

    Public Function GetAllMeasureUnits(Of T)(o As T, Optional b As BindingFlags = BindingFlags.Public Or BindingFlags.Static) As List(Of KeyValuePair(Of T, Double))

        Dim da As MeasureUnitAttribute
        Dim fi() As FieldInfo = o.GetType.GetFields(b)
        Dim c As Integer = 0

        Dim l As New List(Of KeyValuePair(Of T, Double))
        Dim kv As KeyValuePair(Of T, Double)

        For Each fe In fi

            da = CType(fe.GetCustomAttribute(GetType(MeasureUnitAttribute)), MeasureUnitAttribute)
            If da IsNot Nothing Then

                kv = New KeyValuePair(Of T, Double)(CType(fe.GetValue(o), T), da.Unit)
                l.Add(kv)
            End If

        Next
        Return l
    End Function

    Public Function GetAllMultipliers(Of T)(o As T, Optional b As BindingFlags = BindingFlags.Public Or BindingFlags.Static) As List(Of KeyValuePair(Of T, Double))

        Dim da As MultiplierAttribute
        Dim fi() As FieldInfo = o.GetType.GetFields(b)
        Dim c As Integer = 0

        Dim l As New List(Of KeyValuePair(Of T, Double))
        Dim kv As KeyValuePair(Of T, Double)

        For Each fe In fi

            da = CType(fe.GetCustomAttribute(GetType(MultiplierAttribute)), MultiplierAttribute)
            If da IsNot Nothing Then

                kv = New KeyValuePair(Of T, Double)(CType(fe.GetValue(o), T), da.Value)
                l.Add(kv)
            End If

        Next
        Return l
    End Function

    Public Function GetAllEnumVals(Of T)() As T()
        Dim fi() As FieldInfo = GetType(T).GetFields(BindingFlags.Public Or BindingFlags.Static)
        Dim x() As T
        Dim i As Integer = 0
        ReDim x(fi.Length - 1)
        
        For Each fe In fi

            x(i) = CType(fe.GetValue(x(i)), T)
            i += 1
        Next

        Return x
    End Function

End Module

#End Region

#Region "Custom Attributes"

''' <summary>
''' Provides a detailed description.
''' </summary>
''' <remarks></remarks>
Public Class DetailAttribute
    Inherits Attribute

    Private _Detail As String

    Public ReadOnly Property Detail As String
        Get
            Return _Detail
        End Get
    End Property

    Public Sub New(detail As String)
        _Detail = detail
    End Sub

End Class

''' <summary>
''' Provides the byte length of the property.
''' </summary>
''' <remarks></remarks>
Public Class ByteLengthAttribute
    Inherits Attribute

    Private _Length As UShort = 4

    Public ReadOnly Property Length As UShort
        Get
            Return _Length
        End Get
    End Property

    Public Sub New()

    End Sub

    Public Sub New(value As UShort)
        _Length = value
    End Sub
End Class

''' <summary>
''' Provides the multiplier of the property.
''' </summary>
''' <remarks></remarks>
Public Class MultiplierAttribute
    Inherits Attribute

    Private _Value As Double = 1.0#

    Public ReadOnly Property Value As Double
        Get
            Value = _Value
        End Get
    End Property

    Public Sub New(Value As Double)
        _Value = Value
    End Sub

    Public Sub New()
        _Value = Value
    End Sub

End Class

''' <summary>
''' Provides the unit of measure of the property.
''' </summary>
''' <remarks></remarks>
Public Class MeasureUnitAttribute
    Inherits Attribute

    Private _Unit As MeasureUnitTypes

    Public ReadOnly Property Unit As MeasureUnitTypes
        Get
            Return _Unit
        End Get
    End Property

    Public Sub New(Unit As MeasureUnitTypes)
        _Unit = Unit
    End Sub

End Class

''' <summary>
''' Provides the unit symbol of the property.
''' </summary>
''' <remarks></remarks>
Public Class UnitSymbolAttribute
    Inherits Attribute

    Private _Symbol As String

    Public ReadOnly Property Symbol As String
        Get
            Return _Symbol
        End Get
    End Property

    Public Sub New(Symbol As String)
        _Symbol = Symbol
    End Sub
End Class

''' <summary>
''' Provides the number format for the property.
''' </summary>
''' <remarks></remarks>
Public Class NumberFormatAttribute
    Inherits Attribute

    Private _Format As String

    Public ReadOnly Property Format As String
        Get
            Return _Format
        End Get
    End Property

    Public Sub New(Format As String)
        _Format = Format
    End Sub

    Public Sub New()
        _Format = "0.0"
    End Sub
End Class

#End Region

#Region "Power Sources"

''' <summary>
''' Represents an enumeration of power source characteristics.
''' </summary>
''' <remarks></remarks>
<DefaultValue(-1)>
Public Enum PowerStates

    ''' <summary>
    ''' Communication with the device has not been established.
    ''' </summary>
    ''' <remarks></remarks>
    <Description("Uninitialized."), Detail("Communication with the device has not been established.")>
    Uninitialized = -1

    ''' <summary>
    ''' Operating on A/C power from the utility.
    ''' </summary>
    ''' <remarks></remarks>
    <Description("Utility Power"), Detail("Operating on A/C power from the utility.")>
    Utility

    ''' <summary>
    ''' Operating on battery power because there is no power coming from the utility.
    ''' </summary>
    ''' <remarks></remarks>
    <Description("Battery Power"), Detail("Operating on battery power because there is no power coming from the utility.")>
    Battery

    ''' <summary>
    ''' Operating on battery power because there is low voltage coming from the utility.
    ''' </summary>
    ''' <remarks></remarks>
    <Description("Battery Power Due To Low Voltage"), Detail("Operating on battery power because the voltage coming from the utility is too low.")>
    BatteryTransferLow

    ''' <summary>
    ''' Operating on battery power because there is high voltage coming from the utility.
    ''' </summary>
    ''' <remarks></remarks>
    <Description("Battery Power Due To High Voltage"), Detail("Operating on battery power because the voltage coming from the utility is too high.")>
    BatteryTransferHigh

End Enum

#End Region

#Region "TrippLiteCodes"

''' <summary>
''' Represents feature command codes that can be sent to a Tripp Lite Smart battery.
''' </summary>
''' <remarks></remarks>
Public Enum TrippLiteCodes As Byte

    ''' <summary>
    ''' VA RATING
    ''' </summary>
    <Description("VA RATING"), Multiplier(1), MeasureUnit(MeasureUnitTypes.Volt), NumberFormat("0"), ByteLength()>
    VARATING = &H3

    ''' <summary>
    ''' Nominal Battery Voltage
    ''' </summary>
    <Description("Nominal Battery Voltage"), Multiplier(1), MeasureUnit(MeasureUnitTypes.Volt), NumberFormat("0"), ByteLength()>
    NominalBatteryVoltage = &H4

    ''' <summary>
    ''' Low Voltage Transfer
    ''' </summary>
    <Description("Low Voltage Transfer"), Multiplier(1), MeasureUnit(MeasureUnitTypes.Volt), NumberFormat("0"), ByteLength()>
    LowVoltageTransfer = &H6

    ''' <summary>
    ''' High Voltage Transfer
    ''' </summary>
    <Description("High Voltage Transfer"), Multiplier(1), MeasureUnit(MeasureUnitTypes.Volt), NumberFormat("0"), ByteLength()>
    HighVoltageTransfer = &H9

    ''' <summary>
    ''' Input Frequency
    ''' </summary>
    <Description("Input Frequency"), Multiplier(0.1), MeasureUnit(MeasureUnitTypes.Hertz), NumberFormat(), ByteLength()>
    InputFrequency = &H19

    ''' <summary>
    ''' Output Frequency
    ''' </summary>
    <Description("Output Frequency"), Multiplier(0.1), MeasureUnit(MeasureUnitTypes.Hertz), NumberFormat(), ByteLength()>
    OutputFrequency = &H1C

    ''' <summary>
    ''' Input Voltage
    ''' </summary>
    <Description("Input Voltage"), Multiplier(0.1), MeasureUnit(MeasureUnitTypes.Volt), NumberFormat("000.0"), ByteLength()>
    InputVoltage = &H31

    ''' <summary>
    ''' Output Voltage
    ''' </summary>
    <Description("Output Voltage"), Multiplier(0.1), MeasureUnit(MeasureUnitTypes.Volt), NumberFormat("000.0"), ByteLength()>
    OutputVoltage = &H1B

    ''' <summary>
    ''' Output Current
    ''' </summary>
    <Description("Output Current"), Multiplier(0.1), MeasureUnit(MeasureUnitTypes.Amp), NumberFormat(), ByteLength()>
    OutputCurrent = &H46

    ''' <summary>
    ''' Output Power
    ''' </summary>
    <Description("Output Power"), Multiplier(1), MeasureUnit(MeasureUnitTypes.Watt), NumberFormat("0"), ByteLength()>
    OutputPower = &H47

    ''' <summary>
    ''' Output Load
    ''' </summary>
    <Description("Ouput Load"), Multiplier(1), MeasureUnit(MeasureUnitTypes.Percent), NumberFormat("0"), ByteLength()>
    OutputLoad = &H1E

    ''' <summary>
    ''' Seconds Remaining Power
    ''' </summary>
    <Description("Time Remaining"), Multiplier(1 / 60), MeasureUnit(MeasureUnitTypes.Time), NumberFormat(), ByteLength()>
    TimeRemaining = &H35

    ''' <summary>
    ''' Seconds Remaining Power
    ''' </summary>
    <Description("Battery Voltage"), Multiplier(0.1), MeasureUnit(MeasureUnitTypes.Volt), NumberFormat(), ByteLength()>
    BatteryVoltage = &H20

    ''' <summary>
    ''' Charge Remaining
    ''' </summary>
    <Description("Charge Remaining"), Multiplier(1), MeasureUnit(MeasureUnitTypes.Percent), NumberFormat("0"), ByteLength()>
    ChargeRemaining = &H34

End Enum

#End Region

#Region "MeasureUnitTypes"

''' <summary>
''' Indicates a measure unit type.
''' </summary>
''' <remarks></remarks>
Public Enum MeasureUnitTypes

    <Description("Volts"), UnitSymbol("V")>
    Volt

    <Description("Amps"), UnitSymbol("A")>
    Amp

    <Description("Watts"), UnitSymbol("W")>
    Watt

    <Description("Hertz"), UnitSymbol("Hz")>
    Hertz

    <Description("Percent"), UnitSymbol("%")>
    Percent

    <Description("Minutes")>
    Time

    <Description("Degrees Celsius"), UnitSymbol("°C")>
    Temperature

End Enum

#End Region

#Region "MeasureUnit"

''' <summary>
''' Represents a unit of measure.
''' </summary>
''' <remarks></remarks>
Public Class MeasureUnit
    Protected _UnitSymbol As String
    Protected _Name As String
    Protected _UnitType As MeasureUnitTypes
    Protected Shared _Units As Collection(Of MeasureUnit)

    Public Shared Function FindUnit(t As MeasureUnitTypes) As MeasureUnit
        For Each mu In _Units
            If mu.UnitType = t Then Return mu
        Next
        Return Nothing
    End Function

    Protected Sub New(unit As MeasureUnitTypes, name As String, symbol As String)
        _UnitSymbol = symbol
        _UnitType = unit
        _Name = name
    End Sub

    Shared Sub New()
        _Units = New Collection(Of MeasureUnit)

        Dim fi() As FieldInfo = GetType(MeasureUnitTypes).GetFields(BindingFlags.Public Or BindingFlags.Static)
        Dim mu As MeasureUnit
        Dim mt As MeasureUnitTypes = MeasureUnitTypes.Amp

        Dim kvc As List(Of KeyValuePair(Of MeasureUnitTypes, String)) = GetAllDescriptions(Of MeasureUnitTypes)(MeasureUnitTypes.Amp)
        Dim mvc As List(Of KeyValuePair(Of MeasureUnitTypes, String)) = GetAllSymbols(Of MeasureUnitTypes)(MeasureUnitTypes.Amp)
        Dim i As Integer = 0, _
            c As Integer = kvc.Count - 1

        Dim kv As KeyValuePair(Of MeasureUnitTypes, String)

        Dim d As Integer

        If mvc Is Nothing Then d = -1 Else d = 0

        Dim sym As String
        Dim desc As String

        For i = 0 To c
            kv = kvc(i)
            sym = ""

            If d <> -1 Then
                If mvc(d).Key = kvc(i).Key Then
                    sym = mvc(d).Value
                    d += 1
                    If d >= mvc.Count Then d = -1
                End If
            End If

            desc = kvc(i).Value
            mt = kv.Key

            mu = New MeasureUnit(mt, desc, sym)
            _Units.Add(mu)
        Next
    End Sub

    Public Shared ReadOnly Property Units As Collection(Of MeasureUnit)
        Get
            Return _Units
        End Get
    End Property

    Public ReadOnly Property UnitSymbol As String
        Get
            Return _UnitSymbol
        End Get
    End Property

    Public ReadOnly Property Name As String
        Get
            Return _Name
        End Get
    End Property

    Public ReadOnly Property UnitType As MeasureUnitTypes
        Get
            Return _UnitType
        End Get
    End Property

    Public Overrides Function ToString() As String
        ToString = Name
    End Function

End Class

#End Region



