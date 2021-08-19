Option Explicit On

Imports System
Imports System.Text
Imports System.Collections.ObjectModel
Imports System.Threading
Imports System.ComponentModel
Imports System.Windows
Imports System.Collections.Specialized
Imports System.Windows.Media
Imports System.Windows.Data

Public Class TrippLiteViewModel
    Inherits DependencyObject

    Implements INotifyPropertyChanged
    Implements IDisposable

    Private _init As Boolean = False
    Private _waitInit As Thread

    Private WithEvents _TrippLite As TrippLiteUPS
    Private _Running As Boolean = False

    Public Property MaxTries As Integer = 100

    Public Property TimerInterval As Integer = 200
    Public Property DelayStart As Integer = 500

    Public Property Properties As New TrippLitePropertyBagViewModel(Me)
    Public Property ProminentProperties As New TrippLitePropertyBagViewModel(Me)

    Public Property LoadProperties As New TrippLitePropertyBagViewModel(Me)

    Private _WThread As Thread
    Private _lbProp As TrippLitePropertyViewModel

    Public Event ViewModelInitialized(sender As Object, e As EventArgs)
    Public Event PowerStateChanged(sender As Object, e As PowerStateChangedEventArgs)

#Region "LoadBar"

    Public Property LoadBarHandler As LoadBarPropertyHandler

    Public ReadOnly Property LoadBarValue As Double
        Get

            Return LoadBarHandler.HandleLoadBarValue(GetValue(TrippLiteViewModel.LoadBarValueProperty))
        End Get
    End Property

    Public Shared Function GetLoadBarValue(ByVal element As DependencyObject) As Double
        If element Is Nothing Then
            Throw New ArgumentNullException("element")
        End If

        Return CType(element.GetValue(LoadBarValueProperty), Double)
    End Function

    Private Shared ReadOnly LoadBarValuePropertyKey As DependencyPropertyKey = _
                            DependencyProperty.RegisterAttachedReadOnly("LoadBarValue", _
                            GetType(Double), GetType(TrippLiteViewModel), _
                            New PropertyMetadata(Nothing))

    Public Shared ReadOnly LoadBarValueProperty As DependencyProperty = _
                           LoadBarValuePropertyKey.DependencyProperty

    Public Sub MakeLoadBarProperty(prop As TrippLitePropertyViewModel, Optional handler As LoadBarPropertyHandler = Nothing)
        ClearLoadBarProperty()

        If handler Is Nothing Then
            LoadBarHandler = New LoadBarPropertyHandler(0, 100)
        Else
            LoadBarHandler = handler
        End If

        _lbProp = prop
        _lbProp.IsActiveProperty = True

        Dispatcher.Invoke(Sub()
                              LoadBarWatch(Me, New PropertyChangedEventArgs("LoadBarValue"))
                          End Sub)

        AddHandler _lbProp.PropertyChanged, AddressOf LoadBarWatch

    End Sub

    Public Sub ClearLoadBarProperty()

        If _lbProp IsNot Nothing Then
            RemoveHandler _lbProp.PropertyChanged, AddressOf LoadBarWatch
            _lbProp = Nothing
        End If

    End Sub

    Private Sub LoadBarWatch(sender As Object, e As PropertyChangedEventArgs)
        Dispatcher.BeginInvoke(Sub()
                                   SetValue(TrippLiteViewModel.LoadBarValuePropertyKey, CDbl(_lbProp.Value))
                               End Sub)

        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("LoadBarValue"))
    End Sub

#End Region

#Region "TrippLite Device"

    Public ReadOnly Property Title As String
        Get
            Return CStr(GetValue(TrippLiteViewModel.TitleProperty))
        End Get
    End Property

    Private Shared ReadOnly TitlePropertyKey As DependencyPropertyKey = _
                            DependencyProperty.RegisterReadOnly("Title", _
                            GetType(String), GetType(TrippLiteViewModel), _
                            New PropertyMetadata(Nothing))

    Public Shared ReadOnly TitleProperty As DependencyProperty = _
                           TitlePropertyKey.DependencyProperty

    Public ReadOnly Property SerialNumber As String
        Get
            Return CStr(GetValue(TrippLiteViewModel.SerialNumberProperty))
        End Get
    End Property

    Private Shared ReadOnly SerialNumberPropertyKey As DependencyPropertyKey = _
                            DependencyProperty.RegisterReadOnly("SerialNumber", _
                            GetType(String), GetType(TrippLiteViewModel), _
                            New PropertyMetadata(Nothing))

    Public Shared ReadOnly SerialNumberProperty As DependencyProperty = _
                           SerialNumberPropertyKey.DependencyProperty

    Public ReadOnly Property ProductString As String
        Get
            Return CStr(GetValue(TrippLiteViewModel.ProductStringProperty))
        End Get
    End Property

    Private Shared ReadOnly ProductStringPropertyKey As DependencyPropertyKey = _
                            DependencyProperty.RegisterReadOnly("ProductString", _
                            GetType(String), GetType(TrippLiteViewModel), _
                            New PropertyMetadata(Nothing))

    Public Shared ReadOnly ProductStringProperty As DependencyProperty = _
                           ProductStringPropertyKey.DependencyProperty

    Public ReadOnly Property ModelId As String
        Get
            Return CStr(GetValue(TrippLiteViewModel.ModelIdProperty))
        End Get
    End Property

    Private Shared ReadOnly ModelIdPropertyKey As DependencyPropertyKey = _
                            DependencyProperty.RegisterReadOnly("ModelId", _
                            GetType(String), GetType(TrippLiteViewModel), _
                            New PropertyMetadata(Nothing))

    Public Shared ReadOnly ModelIdProperty As DependencyProperty = _
                           ModelIdPropertyKey.DependencyProperty

    Public ReadOnly Property PowerState As PowerStates
        Get
            Return CType(GetValue(TrippLiteViewModel.PowerStateProperty), PowerStates)
        End Get
    End Property

    Private Shared ReadOnly PowerStatePropertyKey As DependencyPropertyKey = _
                            DependencyProperty.RegisterReadOnly("PowerState", _
                            GetType(PowerStates), GetType(TrippLiteViewModel), _
                            New PropertyMetadata(Nothing))

    Public Shared ReadOnly PowerStateProperty As DependencyProperty = _
                           PowerStatePropertyKey.DependencyProperty


    Public ReadOnly Property PowerStateDescription As String
        Get
            Return CType(GetValue(TrippLiteViewModel.PowerStateDescriptionProperty), String)
        End Get
    End Property

    Private Shared ReadOnly PowerStateDescriptionPropertyKey As DependencyPropertyKey = _
                            DependencyProperty.RegisterReadOnly("PowerStateDescription", _
                            GetType(String), GetType(TrippLiteViewModel), _
                            New PropertyMetadata(Nothing))

    Public Shared ReadOnly PowerStateDescriptionProperty As DependencyProperty = _
                           PowerStateDescriptionPropertyKey.DependencyProperty


    Public ReadOnly Property PowerStateDetail As String
        Get
            Return CType(GetValue(TrippLiteViewModel.PowerStateDetailProperty), String)
        End Get
    End Property

    Private Shared ReadOnly PowerStateDetailPropertyKey As DependencyPropertyKey = _
                            DependencyProperty.RegisterReadOnly("PowerStateDetail", _
                            GetType(String), GetType(TrippLiteViewModel), _
                            New PropertyMetadata(Nothing))

    Public Shared ReadOnly PowerStateDetailProperty As DependencyProperty = _
                           PowerStateDetailPropertyKey.DependencyProperty


    Public ReadOnly Property UtilityColor As Color
        Get
            Return CType(GetValue(TrippLiteViewModel.UtilityColorProperty), color)
        End Get
    End Property

    Private Shared ReadOnly UtilityColorPropertyKey As DependencyPropertyKey = _
                            DependencyProperty.RegisterReadOnly("UtilityColor", _
                            GetType(Color), GetType(TrippLiteViewModel), _
                            New PropertyMetadata(Nothing))

    Public Shared ReadOnly UtilityColorProperty As DependencyProperty = _
                           UtilityColorPropertyKey.DependencyProperty

    Public ReadOnly Property UtilityColorBackground As Color
        Get

            Dim v As Color = Color.FromArgb(255, 255, 255, 255)

            Try
                Select Case PowerState
                    Case PowerStates.Utility
                        Return Color.FromRgb(0, 128, 0)

                    Case PowerStates.Battery
                        Return Color.FromRgb(128, 128, 0)

                    Case Else
                        Return Color.FromRgb(128, 0, 0)

                End Select

            Catch ex As Exception
                Return Color.FromArgb(255, 255, 255, 255)
            End Try
        End Get
    End Property


#End Region

#Region "Constructors"

    Public Sub New()
        Me.New(False)
    End Sub

    Public Sub New(init As Boolean)
        If init Then Initialize()
    End Sub

    Public Function Initialize() As Boolean

        If _init Then Return False

        Try
            _TrippLite = New TrippLiteUPS(False)

            If _TrippLite.Connected = False Then
                _waitInit = New Thread( _
                    Sub()
                        Dim i As Integer

                        For i = 0 To MaxTries

                            Thread.Sleep(100)
                            _TrippLite.Connect()

                            If _TrippLite IsNot Nothing AndAlso _TrippLite.IsTrippLite Then

                                Dispatcher.Invoke( _
                                    Sub()
                                        _init = True
                                        _internalInit()
                                        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("TrippLite"))
                                        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Properties"))
                                        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("ProminentProperties"))
                                        RaiseEvent ViewModelInitialized(Me, New EventArgs)
                                    End Sub)

                                _waitInit = Nothing
                                Return
                            End If

                        Next

                    End Sub)

                _waitInit.SetApartmentState(ApartmentState.MTA)
                _waitInit.IsBackground = False

                _waitInit.Start()

                Return False
            End If

        Catch ex As Exception
            Return False
        End Try

        _init = True
        _internalInit()

        RaiseEvent ViewModelInitialized(Me, New EventArgs)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("TrippLite"))

        Return True

    End Function

    Private Sub _internalInit()

        '' build the view model property collection

        Dim pl As TrippLitePropertyViewModel

        For Each p In _TrippLite.PropertyBag
            If p.Code <> TrippLiteCodes.InputVoltage AndAlso p.Code <> TrippLiteCodes.OutputVoltage Then
                _Properties.Add(New TrippLitePropertyViewModel(p, Me))
            Else
                pl = New TrippLitePropertyViewModel(p, Me)
                pl.Prominent = True
                ProminentProperties.Add(pl)
                pl = Nothing
            End If
        Next

        PromoteToLoad(TrippLiteCodes.OutputPower)

        MakeLoadBarProperty(FindProperty(TrippLiteCodes.OutputLoad))


        SetValue(ProductStringPropertyKey, _TrippLite.Device.ProductString)
        SetValue(ModelIdPropertyKey, _TrippLite.ModelId)
        SetValue(SerialNumberPropertyKey, _TrippLite.Device.SerialNumber)
        SetValue(TitlePropertyKey, _TrippLite.Title)

        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Properties"))
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("ProminentProperties"))

        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("ProductString"))
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("ModelId"))
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("SerialNumber"))
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Title"))
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("PowerState"))
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("PowerStateDescription"))
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("PowerStateDetail"))

    End Sub

#End Region

#Region "Public Properties"

    Public ReadOnly Property Initialized As Boolean
        Get
            Return _init
        End Get
    End Property

    Public ReadOnly Property TrippLite As TrippLiteUPS
        Get
            TrippLite = _TrippLite
        End Get
    End Property

#End Region

#Region "Public Methods"

    Public Function FindProperty(c As TrippLiteCodes) As TrippLitePropertyViewModel
        For Each m In Me.Properties
            If m.Code = c Then Return m
        Next
        Return Nothing
    End Function

    Public Sub PromoteToLoad(code As TrippLiteCodes, Optional removeFromList As Boolean = False)
        Dim pm As TrippLitePropertyViewModel = Nothing

        For Each pm In _Properties
            If pm.Code = code Then Exit For
            pm = Nothing
        Next

        If pm Is Nothing Then Return

        If removeFromList Then _Properties.Remove(pm)
        _LoadProperties.Add(pm)
    End Sub

    Public Sub DemoteFromLoad(code As TrippLiteCodes)
        Dim pm As TrippLitePropertyViewModel = Nothing

        For Each pm In _ProminentProperties
            If pm.Code = code Then Exit For
            pm = Nothing
        Next

        If pm Is Nothing Then Return

        If _LoadProperties.Contains(pm) Then _
            _LoadProperties.Remove(pm)

        _Properties.Add(pm)
    End Sub

    Public Sub PromoteProperty(code As TrippLiteCodes)
        Dim pm As TrippLitePropertyViewModel = Nothing

        For Each pm In _Properties
            If pm.Code = code Then Exit For
            pm = Nothing
        Next

        If pm Is Nothing Then Return

        pm.Prominent = True

        _Properties.Remove(pm)
        _ProminentProperties.Add(pm)
    End Sub

    Public Sub DemoteProperty(code As TrippLiteCodes)
        Dim pm As TrippLitePropertyViewModel = Nothing

        For Each pm In _ProminentProperties
            If pm.Code = code Then Exit For
            pm = Nothing
        Next

        If pm Is Nothing Then Return

        pm.Prominent = False

        _ProminentProperties.Remove(pm)
        _Properties.Add(pm)
    End Sub

#End Region

#Region "Threading and Notification"

    Public Event PropertyChanged(sender As Object, e As PropertyChangedEventArgs) Implements INotifyPropertyChanged.PropertyChanged

    Public ReadOnly Property IsTimerRunning As Boolean
        Get
            IsTimerRunning = _Running
        End Get
    End Property

    Public Sub StopWatching()
        If _WThread Is Nothing Then Return

        _Running = False

        _WThread.Abort()
        _WThread = Nothing

    End Sub

    Public Sub StartWatching()

        If Not _init Then Return

        Dim ti As Long = TimerInterval

        If _WThread IsNot Nothing Then
            Return
        End If

        _Running = True
        _WThread = New Thread( _
            Sub()

                Dim tinc As Long = 0

                Thread.Sleep(DelayStart)
                Do

                    If Not _Running OrElse Thread.CurrentThread.ThreadState = ThreadState.AbortRequested Then Return

                    If Not _TrippLite.RefreshData(Me) Then
                        _Running = False
                    End If

                    If Not _Running Then
                        Return
                    End If

                    If tinc >= 10000 Then
                        GC.Collect(2)
                        Thread.Sleep(0)

                        Dispatcher.BeginInvoke( _
                              Sub()
                                  GC.Collect(2)
                                  Thread.Sleep(0)
                              End Sub)

                        tinc = 0
                    End If

                    tinc += ti

                    Thread.Sleep(ti)

                Loop


            End Sub)

        _WThread.IsBackground = True
        _WThread.SetApartmentState(ApartmentState.STA)
        _WThread.Start()

    End Sub

#End Region

#Region "IDisposable Support"
    Private disposedValue As Boolean ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not Me.disposedValue Then

            If _waitInit IsNot Nothing Then
                _waitInit.Abort()
                _waitInit = Nothing
            End If

            StopWatching()

            _TrippLite.Dispose()
            _TrippLite = Nothing

            If disposing Then

                ClearLoadBarProperty()

                Properties = Nothing
                ProminentProperties = Nothing

            End If

        End If

        GC.Collect(0)
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

    Private Sub _TrippLite_PowerStateChanged(sender As Object, e As PowerStateChangedEventArgs) Handles _TrippLite.PowerStateChanged
        Dispatcher.Invoke(Sub()
                              SetValue(PowerStatePropertyKey, _TrippLite.PowerState)
                              SetValue(PowerStateDescriptionPropertyKey, _TrippLite.PowerStateDescription)
                              SetValue(PowerStateDetailPropertyKey, _TrippLite.PowerStateDetail)
                              SetValue(UtilityColorPropertyKey, UtilityColorBackground)

                              RaiseEvent PowerStateChanged(Me, e)

                              RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("PowerState"))
                              RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("PowerStateDescription"))
                              RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("PowerStateDetail"))
                              RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("UtilityColor"))
                          End Sub)
    End Sub

End Class

''' <summary>
''' The object that represents the TrippLite property ViewModel
''' </summary>
''' <remarks></remarks>
Public Class TrippLitePropertyViewModel
    Implements INotifyPropertyChanged, IDisposable, IChild(Of TrippLiteViewModel)

    Public Event PropertyChanged(sender As Object, e As PropertyChangedEventArgs) Implements INotifyPropertyChanged.PropertyChanged

    Private WithEvents _Prop As TrippLiteProperty

    Private _Changed As Boolean
    Private _Prominent As Boolean
    Private _Owner As TrippLiteViewModel

    Private _DisplayValue As String = Nothing
    
    Private _DispEvent As New PropertyChangedEventArgs("DisplayValue")

    ''' <summary>
    ''' Creates a new object with the specified base object and owner.
    ''' </summary>
    ''' <param name="p"></param>
    ''' <param name="owner"></param>
    ''' <remarks></remarks>
    Friend Sub New(p As TrippLiteProperty, owner As TrippLiteViewModel)
        If p Is Nothing Then
            Throw New ArgumentException
        End If
        _Owner = owner
        _Prop = p

        If _Prop._Value = -1 Then
            _Prop._Value = 0
        End If

        _DisplayValue = _Prop.ToString(_Prop.NumberFormat, True)
    End Sub

    ''' <summary>
    ''' Gets the parent of this object (IChild)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Parent As TrippLiteViewModel Implements IChild(Of TrippLiteViewModel).Parent
        Get
            Return _Owner
        End Get
        Friend Set(value As TrippLiteViewModel)
            _Owner = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets a value indicating whether or not this property will actively trigger change events.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property IsActiveProperty As Boolean
        Get
            Return _prop.IsActiveProperty
        End Get
        Set(value As Boolean)
            _prop.IsActiveProperty = value
            If value Then SetDisplayValue()
        End Set
    End Property

    ''' <summary>
    ''' Indicates whether or not this property belongs to the prominent list.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Prominent As Boolean
        Get
            Return _Prominent
        End Get
        Friend Set(value As Boolean)
            _Prominent = value
        End Set
    End Property

    ''' <summary>
    ''' Returns the raw value of the HID property.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Value As Long
        Get
            Return _Prop.Value
        End Get
    End Property

    ''' <summary>
    ''' Returns the HID feature code that the value of this property represents.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Code As TrippLiteCodes
        Get
            Return _Prop.Code
        End Get
    End Property

    ''' <summary>
    ''' Returns the header or title of this property.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Header As String
        Get
            Return _Prop.Description
        End Get
    End Property

    ''' <summary>
    ''' Returns the formatted display value of this property.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property DisplayValue As String
        Get
            Return _DisplayValue
        End Get
    End Property

    ''' <summary>
    ''' Set the display value of the current value of the property.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SetDisplayValue()
        If _Prominent Then
            _DisplayValue = _Prop.ToString(_Prop.NumberFormat, True)
        Else
            _DisplayValue = _Prop.ToString()
        End If
    End Sub

    ''' <summary>
    ''' Returns the formatted display value of this property.
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overrides Function ToString() As String
        Return _DisplayValue
    End Function

#Region "Events"

    Private Sub _Prop_PropertyChanged(sender As Object, e As PropertyChangedEventArgs) Handles _Prop.PropertyChanged
        If Not _Prop.IsActiveProperty Then Return
        SetDisplayValue()
        RaiseEvent PropertyChanged(Me, _DispEvent)
    End Sub

#End Region

#Region "IDisposable Support"
    Private disposedValue As Boolean ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not Me.disposedValue Then
            _Prop.Dispose()
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

#Region "Operators"

    Public Shared Narrowing Operator CType(val As TrippLitePropertyViewModel) As ULong
        Return val._Prop._Value
    End Operator

    Public Shared Widening Operator CType(val As TrippLitePropertyViewModel) As Long
        Return val._Prop._Value
    End Operator

    Public Shared Narrowing Operator CType(val As TrippLitePropertyViewModel) As UInteger
        Return val._Prop._Value
    End Operator

    Public Shared Narrowing Operator CType(val As TrippLitePropertyViewModel) As Integer
        Return val._Prop._Value
    End Operator

#End Region

End Class

Public Class TrippLitePropertyBagViewModel
    Inherits DependencyCollectionBase(Of TrippLitePropertyViewModel, TrippLiteViewModel)

    Public Overrides ReadOnly Property IsReadOnly As Boolean
        Get
            Return False
        End Get
    End Property

    Public Function GetPropertyByCode(c As TrippLiteCodes) As TrippLitePropertyViewModel
        For Each v In Me
            If v.Code = c Then Return v
        Next
        Return Nothing
    End Function

    Public Sub New(owner As TrippLiteViewModel)
        MyBase.New(owner)
    End Sub

End Class

Public Class LoadBarPropertyHandler

    Private _Value As Double

    Private _min As Double, _
            _max As Double

    Public Delegate Function GetMinimumValue() As Double
    Public Delegate Function GetMaximumValue() As Double

    Public Delegate Sub SetMinimumValue(v As Double)
    Public Delegate Sub SetMaximumValue(v As Double)

    Private _getMin As New GetMinimumValue( _
        Function() As Double
            Return _min
        End Function)

    Private _getMax As New GetMaximumValue( _
        Function() As Double
            Return _max
        End Function)

    Private _setMin As New SetMinimumValue( _
        Sub(v As Double)
            _min = v
        End Sub)

    Private _setMax As New SetMaximumValue( _
        Sub(v As Double)
            _max = v
        End Sub)

    ''' <summary>
    ''' Creates a new object with the specified minimum and maximum values.
    ''' </summary>
    ''' <param name="minVal">Minimum value.</param>
    ''' <param name="maxVal">Maximum value.</param>
    ''' <remarks></remarks>
    Public Sub New(minVal As Double, maxVal As Double)
        Me.New(minVal, maxVal, minVal)
    End Sub

    ''' <summary>
    ''' Creates a new object with the specified minimum, maximum and initial values.
    ''' </summary>
    ''' <param name="minVal">Minimum value.</param>
    ''' <param name="maxVal">Maximum value.</param>
    ''' <param name="value">Default value.</param>
    ''' <remarks></remarks>
    Public Sub New(minVal As Double, maxVal As Double, value As Double)
        MinValue = minVal
        MaxValue = maxVal

        If value < minVal OrElse value > maxVal Then
            Throw New ArgumentOutOfRangeException("Value cannot be less than minVal or greater than maxVal")
        End If

        _Value = value
    End Sub


    ''' <summary>
    ''' Creates a new object with the specified get and set minimum and maximum value delegate functions and optional intial value.
    ''' </summary>
    ''' <param name="getMinFunc">The get minimum delegate.</param>
    ''' <param name="setMinFunc">The set minimum delegate.</param>
    ''' <param name="getMaxFunc">The get maximum delegate.</param>
    ''' <param name="setMaxFunc">The set maximum delegate.</param>
    ''' <param name="value">The optional initial value.</param>
    ''' <remarks></remarks>
    Public Sub New(getMinFunc As GetMinimumValue, _
                   setMinFunc As SetMinimumValue, _
                   getMaxFunc As GetMaximumValue, _
                   setMaxFunc As SetMaximumValue, Optional value As Double = 0)

        _setMax = setMaxFunc
        _getMax = getMaxFunc

        _setMin = setMinFunc
        _getMin = getMinFunc

        _Value = value

    End Sub

    ''' <summary>
    ''' Gets or sets the minimum load bar value.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property MinValue As Double
        Get
            Return _getMin()
        End Get
        Friend Set(value As Double)
            _setMin(value)
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the maximum load bar value.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property MaxValue As Double
        Get
            Return _getMax()
        End Get
        Friend Set(value As Double)
            _setMax(value)
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the current load bar value.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Value As Double
        Get
            Return _Value
        End Get
        Friend Set(value As Double)

            '' in other circumstances, we may want to throw an exception.
            '' however this is to report the load bar values, and there
            '' are some circumstances where the load bar value may
            '' be within an accepted physical threshold (for instance, voltage)
            '' but beyond the ability of the graphic to produce.
            If value < MinValue Then
                value = MinValue
            ElseIf value > MaxValue Then
                value = MaxValue
            End If

            _Value = value
        End Set
    End Property

    ''' <summary>
    ''' Handle the load bar value for the given value.
    ''' </summary>
    ''' <param name="v"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function HandleLoadBarValue(v As Double) As Double
        _Value = v
        Return HandleLoadBarValue()
    End Function

    ''' <summary>
    ''' Handle the load bar value for the current value.
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function HandleLoadBarValue() As Double

        Dim d As Double = MaxValue - MinValue
        Dim e As Double = Value - MinValue

        If d < 0 Then
            Throw New InvalidConstraintException("Maximum value is less than minimum value.")
        End If

        Return e / d

    End Function

End Class
