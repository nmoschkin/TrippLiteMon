Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports TrippLite

Public Class DesktopWindow


    Private WithEvents _ViewModel As TrippLiteViewModel

    Public Event OpenMainWindow(sender As Object, e As EventArgs)


    Public Property ViewModel As TrippLiteViewModel
        Get
            Return GetValue(ViewModelProperty)
        End Get
        Friend Set(ByVal value As TrippLiteViewModel)
            SetValue(ViewModelProperty, value)
            _ViewModel = value
        End Set
    End Property

    Public Shared ReadOnly ViewModelProperty As DependencyProperty =
                           DependencyProperty.Register("ViewModel",
                           GetType(TrippLiteViewModel), GetType(DesktopWindow),
                           New PropertyMetadata(Nothing))




    Public ReadOnly Property ModelId As String
        Get
            Return GetValue(DesktopWindow.ModelProperty)
        End Get
    End Property

    Private Shared ReadOnly ModelPropertyKey As DependencyPropertyKey =
                            DependencyProperty.RegisterReadOnly("ModelId",
                            GetType(String), GetType(DesktopWindow),
                            New PropertyMetadata(Nothing))

    Public Shared ReadOnly ModelProperty As DependencyProperty =
                           ModelPropertyKey.DependencyProperty



    <Browsable(True), Category("Layout"), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)>
    Public Property ItemSpacing As Thickness
        Get
            ItemSpacing = GetItemSpacing(Me)
        End Get
        Set(value As Thickness)
            SetItemSpacing(Me, value)
        End Set
    End Property

    Public Shared Function GetItemSpacing(ByVal element As DependencyObject) As Thickness
        If element Is Nothing Then
            Throw New ArgumentNullException("element")
        End If

        Return element.GetValue(ItemSpacingProperty)
    End Function

    Public Shared Sub SetItemSpacing(ByVal element As DependencyObject, ByVal value As Thickness)
        If element Is Nothing Then
            Throw New ArgumentNullException("element")
        End If

        element.SetValue(ItemSpacingProperty, value)
    End Sub

    Public Shared ReadOnly ItemSpacingProperty As _
                           DependencyProperty = DependencyProperty.RegisterAttached("ItemSpacing",
                           GetType(Thickness), GetType(DesktopWindow),
                           New PropertyMetadata(CType(AddressOf PropertyChanged, PropertyChangedCallback)))


    Public Property DisplayCodes As ObservableCollection(Of TrippLiteCodes)
        Get
            DisplayCodes = GetDisplayCodes(Me)
        End Get
        Set(value As ObservableCollection(Of TrippLiteCodes))
            SetDisplayCodes(Me, value)
        End Set
    End Property

    Public Shared Function GetDisplayCodes(ByVal element As DependencyObject) As ObservableCollection(Of TrippLiteCodes)
        If element Is Nothing Then
            Throw New ArgumentNullException("element")
        End If

        Return element.GetValue(DisplayCodesProperty)
    End Function

    Public Shared Sub SetDisplayCodes(ByVal element As DependencyObject, ByVal value As ObservableCollection(Of TrippLiteCodes))
        If element Is Nothing Then
            Throw New ArgumentNullException("element")
        End If

        element.SetValue(DisplayCodesProperty, value)
    End Sub

    Public Shared ReadOnly DisplayCodesProperty As _
                           DependencyProperty = DependencyProperty.RegisterAttached("DisplayCodes",
                           GetType(ObservableCollection(Of TrippLiteCodes)), GetType(DesktopWindow),
                           New PropertyMetadata(CType(AddressOf PropertyChanged, PropertyChangedCallback)))


    Private Shared Sub PropertyChanged(d As DependencyObject,
                                e As DependencyPropertyChangedEventArgs)

    End Sub

    Public ReadOnly Property TrippLite As TrippLiteUPS
        Get
            TrippLite = _ViewModel.TrippLite
        End Get
    End Property


    Sub New(vm As TrippLiteViewModel)

        DisplayCodes = New ObservableCollection(Of TrippLiteCodes)

        ViewModel = vm
        _ViewModel = vm

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

        If _ViewModel.Initialized = False Then
            _ViewModel.Initialize()
        Else
            Me.DataContext = ViewModel
        End If

        For Each pr In _ViewModel.Properties
            Select Case pr.Code

                Case TrippLiteCodes.InputVoltage, TrippLiteCodes.OutputVoltage, TrippLiteCodes.OutputLoad, TrippLiteCodes.OutputPower, TrippLiteCodes.OutputCurrent
                    Exit Select

                Case Else
                    pr.IsActiveProperty = False
            End Select
        Next

    End Sub

    Sub New()
        DisplayCodes = New ObservableCollection(Of TrippLiteCodes)
        ViewModel = New TrippLiteViewModel

        InitializeComponent()

        _ViewModel.Initialize()


        For Each pr In _ViewModel.Properties
            Select Case pr.Code

                Case TrippLiteCodes.InputVoltage, TrippLiteCodes.OutputVoltage, TrippLiteCodes.OutputLoad, TrippLiteCodes.OutputPower, TrippLiteCodes.OutputCurrent
                    Exit Select

                Case Else
                    pr.IsActiveProperty = False
            End Select
        Next

    End Sub


    Private Sub _ViewModel_ViewModelInitialized(sender As Object, e As EventArgs) Handles _ViewModel.ViewModelInitialized

        Me.DataContext = ViewModel

        For Each dc In _ViewModel.TrippLite.PropertyBag

            Select Case dc.Code
                Case TrippLiteCodes.InputVoltage, TrippLiteCodes.OutputVoltage
                    Continue For
                Case Else
                    DisplayCodes.Add(dc.Code)
            End Select
        Next

        _ViewModel.TrippLite.RefreshData()
        SetValue(ModelPropertyKey, _ViewModel.ModelId)

        If DesignerProperties.GetIsInDesignMode(Me) = False Then
            _ViewModel.StartWatching()
        End If

    End Sub

    Private Sub CloseButton_Click(sender As Object, e As RoutedEventArgs) Handles CloseButton.Click
        _ViewModel.StopWatching()
        Me.Close()
    End Sub

    Private _Moving As Boolean

    Private Sub MoveButton_PreviewMouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles MoveButton.PreviewMouseLeftButtonDown
        _Moving = True
        _offsetPoint = e.GetPosition(MoveButton)
    End Sub

    Private Sub MoveButton_PreviewMouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles MoveButton.PreviewMouseLeftButtonUp
        _Moving = False
    End Sub

    Private _offsetPoint As Point

    Private Sub MoveButton_PreviewMouseMove(sender As Object, e As MouseEventArgs) Handles MoveButton.PreviewMouseMove
        If _Moving Then
            Dim p As Point = e.GetPosition(MoveButton)

            Me.Left += p.X - _offsetPoint.X
            Me.Top += p.Y - _offsetPoint.Y

        End If
    End Sub

    Private Sub OptionsButton_Click(sender As Object, e As RoutedEventArgs) Handles OptionsButton.Click
        OptionsMenu.IsOpen = True
    End Sub

    Private Sub RevertToBig_Click(sender As Object, e As RoutedEventArgs) Handles RevertToBig.Click
        RaiseEvent OpenMainWindow(Me, e)
    End Sub

    Private Sub SysPower_Click(sender As Object, e As RoutedEventArgs) Handles SysPower.Click

        Dim h As New Interop.WindowInteropHelper(Me)
        TrippLiteUPS.OpenSystemPowerOptions(h.Handle)

    End Sub

    Private Sub DesktopWindow_Unloaded(sender As Object, e As RoutedEventArgs) Handles Me.Unloaded

        If _ViewModel IsNot Nothing Then
            _ViewModel.Dispose()
            SetValue(ViewModelProperty, Nothing)
            _ViewModel = Nothing
        End If

    End Sub

    Private Sub ChangePowerState(psu As Boolean, Optional reset As Boolean = False)

        psuA.first = False

        If reset Then
            psuA.psu = (Not psu)
        End If

        If psu = psuA.psu Then Return

        If psu = True Then
            _ViewModel.MakeLoadBarProperty(_ViewModel.Properties.GetPropertyByCode(TrippLiteCodes.ChargeRemaining))
            _ViewModel.Properties.GetPropertyByCode(TrippLiteCodes.OutputLoad).IsActiveProperty = False
            psuA.psu = True
        Else
            _ViewModel.MakeLoadBarProperty(_ViewModel.Properties.GetPropertyByCode(TrippLiteCodes.OutputLoad))
            _ViewModel.Properties.GetPropertyByCode(TrippLiteCodes.ChargeRemaining).IsActiveProperty = False
            psuA.psu = False
        End If

    End Sub

    Private Sub _ViewModel_PowerStateChanged(sender As Object, e As PowerStateChangedEventArgs) Handles _ViewModel.PowerStateChanged

        If e.NewState = PowerStates.Utility Then
            ChangePowerState(False, psuA.first)
        Else
            ChangePowerState(True, psuA.first)
        End If

    End Sub

End Class

Friend Module psuA

    <ThreadStatic>
    Public psu As Boolean = False

    <ThreadStatic>
    Public first As Boolean = True

End Module