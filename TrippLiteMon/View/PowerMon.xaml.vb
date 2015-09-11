
Public Class PowerMon

    Private WithEvents _ViewModel As TrippLiteViewModel

    Public Event OpenCoolWindow(sender As Object, e As EventArgs)

    Public ReadOnly Property ViewModel As TrippLiteViewModel
        Get
            ViewModel = Monitor.ViewModel
        End Get
    End Property

    Public ReadOnly Property TrippLite As TrippLiteUPS
        Get
            TrippLite = Monitor.TrippLite
        End Get
    End Property

    Sub New()

        InitializeComponent()
        _ViewModel = ViewModel

        Dim screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth
        Dim screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight
        Dim windowWidth = Me.Width
        Dim windowHeight = Me.Height

        Me.Left = (screenWidth / 2) - (windowWidth / 2)
        Me.Top = (screenHeight / 2) - (windowHeight / 2)

        If _ViewModel.Initialized Then
            initVars()
        End If

    End Sub

    Private Sub _ViewModel_PowerStateChanged(sender As Object, e As PowerStateChangedEventArgs) Handles _ViewModel.PowerStateChanged

        If e.NewState = PowerStates.Utility Then
            _ViewModel.MakeLoadBarProperty(_ViewModel.Properties.GetPropertyByCode(TrippLiteCodes.OutputLoad))
        Else
            _ViewModel.MakeLoadBarProperty(_ViewModel.Properties.GetPropertyByCode(TrippLiteCodes.ChargeRemaining))
        End If

    End Sub

    Private Sub _ViewModel_ViewModelInitialized(sender As Object, e As EventArgs) Handles _ViewModel.ViewModelInitialized
        initVars()
    End Sub

    Private Sub initVars()
        Me.DataContext = ViewModel
    End Sub

    Private Sub OpenPower_MouseUp(sender As Object, e As MouseButtonEventArgs) Handles OpenPower.MouseUp

        Dim h As New Interop.WindowInteropHelper(Me)
        TrippLiteUPS.OpenSystemPowerOptions(h.Handle)

    End Sub

    Private Sub OpenCool_MouseUp(sender As Object, e As Windows.Input.MouseButtonEventArgs) Handles OpenCool.MouseUp
        RaiseEvent OpenCoolWindow(Me, New EventArgs)
    End Sub

End Class
