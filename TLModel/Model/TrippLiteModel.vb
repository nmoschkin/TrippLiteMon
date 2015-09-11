Option Explicit On

Imports System.Threading
Imports System.Runtime.InteropServices
Imports System.ComponentModel
Imports DataTools.Memory
Imports DataTools.SystemInfo

Imports DataTools.Interop.Native
Imports DataTools.Interop

#Region "TrippLite"

Public Class TrippLiteUPS
    Inherits System.Runtime.ConstrainedExecution.CriticalFinalizerObject

    Implements INotifyPropertyChanged, IDisposable

    Protected mm As MemPtr
    Protected _hid As IntPtr

    Protected _Power As HidDeviceInfo
    Protected _Values(255) As Long
    Protected _conn As Boolean = False
    Protected _isTL As Boolean = False
    Protected _PowerState As PowerStates = PowerStates.Uninitialized
    Protected _Bag As New TrippLitePropertyBag(Me)

    Protected _buffLen As Long = 65

    Public Shared Property DefaultRetries As Integer = 2
    Public Shared Property DefaultRetryDelay As Integer = 1000

    Public Event PropertyChanged(sender As Object, e As PropertyChangedEventArgs) Implements INotifyPropertyChanged.PropertyChanged
    Public Event PowerStateChanged(sender As Object, e As PowerStateChangedEventArgs)

#Region "Shared"

    ''' <summary>
    ''' Returns a list of all TrippLite devices.
    ''' </summary>
    ''' <param name="forceRefreshCache">Whether or not to refresh the internal HID power device cache.</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function FindAllTrippLiteDevices(Optional forceRefreshCache As Boolean = False) As HidDeviceInfo()
        Dim lOut As New List(Of HidDeviceInfo)
        Dim devs() As HidDeviceInfo

        devs = HidDevicesByUsage(HidUsagePage.PowerDevice1)

        If devs IsNot Nothing Then
            For Each dev In devs
                If dev.HidManufacturer = "Tripp Lite" Then
                    lOut.Add(dev)
                End If
            Next
        End If

        Return lOut.ToArray
    End Function

    ''' <summary>
    ''' List all TrippLite devices by serial number, only.
    ''' </summary>
    ''' <param name="forceRefreshCache">Whether or not to refresh the internal HID power device cache.</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function ListTrippLiteDevicesBySerialNumber(Optional forceRefreshCache As Boolean = False) As String()
        Dim devs() As HidDeviceInfo = FindAllTrippLiteDevices(forceRefreshCache)

        Dim l As New List(Of String)

        For Each d In devs
            l.Add(d.SerialNumber)
        Next

        Return l.ToArray
    End Function


    ''' <summary>
    ''' Opens the system power options.
    ''' Opens either the control panel or the Windows 10 settings panel.
    ''' </summary>
    ''' <param name="hwndOwner">Optional pointer to the parent window (default is null).</param>
    ''' <param name="win10">Open the Windows 10 settings panel, if available (default is True).</param>
    Public Shared Sub OpenSystemPowerOptions(Optional hwndOwner As IntPtr = Nothing, Optional win10 As Boolean = True)

        Dim do10 As Boolean = (win10 And OSInfo.IsWindows10)

        Dim shex As New SHELLEXECUTEINFO

        shex.cbSize = Marshal.SizeOf(shex)
        shex.fMask = SEE_MASK_UNICODE Or SEE_MASK_ASYNCOK Or SEE_MASK_FLAG_DDEWAIT
        shex.hWnd = hwndOwner
        shex.hInstApp = Process.GetCurrentProcess.Handle
        shex.nShow = SW_SHOW


        If do10 Then

            shex.lpFile = "ms-settings:powersleep"
        Else
            shex.lpDirectory = "::{26EE0668-A00A-44D7-9371-BEB064C98683}\0"
            shex.lpFile = "::{26EE0668-A00A-44D7-9371-BEB064C98683}\0\::{025A5937-A6BE-4686-A844-36FE4BEC8B6D}"

        End If


        shex.lpVerb = ""

        ShellExecuteEx(shex)

    End Sub

#End Region

#Region "Constructor"

    ''' <summary>
    ''' Initialize a new TrippLiteUPS object.
    ''' </summary>
    ''' <param name="connect"></param>
    ''' <remarks></remarks>
    Public Sub New(Optional connect As Boolean = True)
        If connect Then Me.Connect()
    End Sub

    ''' <summary>
    ''' Connect to a TrippLite battery.
    ''' </summary>
    ''' <param name="device">Optional manually-selected device.</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function Connect(Optional device As HidDeviceInfo = Nothing) As Boolean

        '' we won't be connecting if it's already disposed.
        If disposedValue Then Return False

        Dim devs() As HidDeviceInfo
        Dim i As Integer = 0

        If device Is Nothing Then

            Do
                devs = HidDevicesByUsage(HidUsagePage.PowerDevice1)

                If devs IsNot Nothing Then
                    For Each dev In devs
                        If dev.Vid = &H09AE Then
                            _Power = dev
                            _isTL = True
                            Exit For
                        End If
                    Next

                    If _Power Is Nothing Then _
                            _Power = devs(0)

                    _conn = True
                    Exit Do
                End If

                i += 1
                If i >= DefaultRetries Then Exit Do
                Threading.Thread.Sleep(DefaultRetryDelay)
            Loop
        Else
            If device.Vid = &H09AE Then
                _isTL = True
            Else
                _isTL = False
            End If

            _Power = device
            _conn = True
        End If

        i = 0

        If _conn Then
            _hid = OpenHid(_Power)
            mm.AllocZero(_buffLen)
        End If

        Return _conn And (_hid.ToInt64 > 0)
    End Function

    ''' <summary>
    ''' Disconnect the device and free all resources.
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function Disconnect() As Boolean
        Try
            mm.Free()
            CloseHid(_hid)

            _hid = -1
            _conn = False
            _Power = Nothing

        Catch ex As Exception
            Return False
        End Try

        Return True
    End Function

#End Region

#Region "Public Properties"

    ''' <summary>
    ''' Gets a value indicating whether or not the current object is connected to a battery.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Connected As Boolean
        Get
            Return _conn
        End Get
    End Property

    ''' <summary>
    ''' Returns the current power state of the Tripp Lite device.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property PowerState As PowerStates
        Get
            Return _PowerState
        End Get
        Protected Set(value As PowerStates)
            If _PowerState <> value Then
                Dim os As PowerStates = _PowerState
                _PowerState = value
                RaiseEvent PowerStateChanged(Me, New PowerStateChangedEventArgs(os, _PowerState))
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("PowerState"))
            End If
        End Set
    End Property

    ''' <summary>
    ''' Gets the description of the current power state.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property PowerStateDescription As String
        Get

            '' retrieve the 'Description' property (as string) of the DescriptionAttribute
            '' associated with the specified field of the PowerStates enumeration.
            Return GetEnumAttrVal(Of DescriptionAttribute, String, PowerStates)(_PowerState, "Description")
        End Get
    End Property

    ''' <summary>
    ''' Gets the detailed description of the current power state.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property PowerStateDetail As String
        Get
            Return GetEnumAttrVal(Of DetailAttribute, String, PowerStates)(_PowerState, "Detail")
        End Get
    End Property

    ''' <summary>
    ''' Gets the title of the device.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Title As String
        Get
            Return Device.HidManufacturer & " " & Device.ClassName
        End Get
    End Property

    ''' <summary>
    ''' Gets the model of the SMART Tripp Lite UPS
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property ModelId As String
        Get
            Return "SMART" & _Bag.FindProperty(TrippLiteCodes.VARATING).GetValue & "LCDx"
        End Get
    End Property

    ''' <summary>
    ''' Contains all properties exposed by the Tripp Lite HID Power Interface
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property PropertyBag As TrippLitePropertyBag
        Get
            Return _Bag
        End Get
    End Property

    ''' <summary>
    ''' Indicates whether this power interface is attached to a Tripp Lite power device.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property IsTrippLite As Boolean
        Get
            Return _isTL
        End Get
    End Property

    ''' <summary>
    ''' Contains detailed Operating System-Reported information about the device.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Device As HidDeviceInfo
        Get
            Return _Power
        End Get
    End Property

#End Region

#Region "Public Methods"

    ''' <summary>
    ''' Refresh the data from the device, using the default number of tries and the default delay.
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function RefreshData(Optional dep As System.Windows.DependencyObject = Nothing) As Boolean
        RefreshData = RefreshData(DefaultRetries, DefaultRetryDelay, dep)
    End Function

    ''' <summary>
    ''' Refresh the data from the device.
    ''' </summary>
    ''' <param name="tries">The number of times to attempt to collect data.</param>
    ''' <param name="delay">The interval between each try, in milliseconds.</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function RefreshData(tries As Integer, delay As Integer, Optional dep As System.Windows.DependencyObject = Nothing) As Boolean
        Dim b As Boolean
        Dim i As Integer = 0

        Do
            b = _internalRefresh(dep)
            If b = True Then Return True

            i += 1
            If i = tries Then Return False

            Threading.Thread.Sleep(delay)
        Loop

    End Function

    ''' <summary>
    ''' Signals the system to signal refresh events for all properties regardless of their changed state.
    ''' Property events will always signal if a property changes.
    ''' To refresh the status of the device, use the RefreshData() method.
    ''' (This method does not trigger the PowerStateChanged event)
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SignalRefresh()

        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("PowerState"))
        _Bag.SignalRefresh()

    End Sub

#End Region

#Region "Protected Methods"

    Protected Function _internalRefresh(Optional dep As System.Windows.DependencyObject = Nothing) As Boolean

        If Not _conn Then Return False

        Dim v As Integer

        Dim ret As Integer = 0

        Dim max As Double, _
            min As Double, _
            volt As Double

        Dim involtRet As Boolean = False

        Dim cex As New List(Of TrippLiteProperty)

        Try
            For Each prop In _Bag

                mm.LongAtAbsolute(1) = 0

                mm.ByteAt(0) = prop.Code

                If HidD_GetFeature(_hid, mm, _buffLen) Then
                    v = mm.IntegerAtAbsolute(1)

                    Select Case prop.Code
                        Case TrippLiteCodes.InputVoltage
                            volt = v * prop.Multiplier
                            involtRet = True

                        Case TrippLiteCodes.OutputLoad
                            If v > 100 OrElse v < 0 Then v = prop.Value

                    End Select

                    If prop._Value <> v OrElse prop._Value = -1 Then
                        prop._Value = v
                        cex.Add(prop)
                    End If
                End If

            Next

        Catch thx As ThreadAbortException

            Return False

        Catch ex As Exception

            Return False
        End Try
        ''
        '' Check for power failure
        Static lps As Integer = 0
        Dim lpsMax As Integer = 0

        If involtRet = True Then
            min = _Bag.FindProperty(TrippLiteCodes.LowVoltageTransfer).Value
            max = _Bag.FindProperty(TrippLiteCodes.HighVoltageTransfer).Value

            Select Case volt

                Case 0
                    If PowerState <> PowerStates.Battery Then
                        If lps >= lpsMax Then
                            If dep IsNot Nothing Then
                                dep.Dispatcher.BeginInvoke(
                                    Sub()
                                        PowerState = PowerStates.Battery
                                    End Sub)
                            Else
                                PowerState = PowerStates.Battery
                            End If

                            lps = 0
                        End If
                    Else
                        lps += 1
                    End If

                Case Is <= min
                    If PowerState <> PowerStates.BatteryTransferLow Then
                        If lps >= lpsMax Then
                            If dep IsNot Nothing Then
                                dep.Dispatcher.BeginInvoke( _
                                    Sub()
                                        PowerState = PowerStates.BatteryTransferLow
                                    End Sub)
                            Else
                                PowerState = PowerStates.BatteryTransferLow
                            End If
                            lps = 0
                        End If
                    Else
                        lps += 1
                    End If

                Case Is >= max
                    If PowerState <> PowerStates.BatteryTransferHigh Then
                        If lps >= lpsMax Then
                            If dep IsNot Nothing Then
                                dep.Dispatcher.BeginInvoke( _
                                    Sub()
                                        PowerState = PowerStates.BatteryTransferHigh
                                    End Sub)
                            Else
                                PowerState = PowerStates.BatteryTransferHigh
                            End If
                            lps = 0
                        End If
                    Else
                        lps += 1
                    End If

                Case Else
                    If PowerState <> PowerStates.Utility Then
                        If lps >= lpsMax Then
                            If dep IsNot Nothing Then
                                dep.Dispatcher.BeginInvoke( _
                                    Sub()
                                    PowerState = PowerStates.Utility
                                End Sub)
                            Else
                                PowerState = PowerStates.Utility
                            End If

                            lps = 0
                        End If
                    Else
                        lps += 1
                    End If

            End Select
        End If

        If dep IsNot Nothing Then

            For Each prop In cex
                dep.Dispatcher.BeginInvoke( _
                    Sub()
                        prop.SignalRefresh()
                    End Sub)
            Next
        Else

            For Each prop In cex
                prop.SignalRefresh()
            Next
        End If

        Return True
    End Function

#End Region

#Region "IDisposable Support"
    Private disposedValue As Boolean ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not Me.disposedValue Then
            Disconnect()
        End If

        Me.disposedValue = True
    End Sub

    Protected Overrides Sub Finalize()
        Dispose(False)
        MyBase.Finalize()
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
#End Region

End Class

#End Region

#Region "PowerStateChangedEventArgs"

Public Class PowerStateChangedEventArgs
    Inherits EventArgs

    Private _oldState As PowerStates
    Private _newState As PowerStates

    Public ReadOnly Property OldState As PowerStates
        Get
            Return _oldState
        End Get
    End Property

    Public ReadOnly Property NewState As PowerStates
        Get
            Return _newState
        End Get
    End Property

    Public Sub New(o As PowerStates, n As PowerStates)
        _oldState = o
        _newState = n
    End Sub

End Class
#End Region