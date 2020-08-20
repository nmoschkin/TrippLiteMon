Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports TrippLite

Public Class StatusDisplay

    Private WithEvents _ViewModel As TrippLiteViewModel

    Public ReadOnly Property ViewModel As TrippLiteViewModel
        Get
            Return GetValue(StatusDisplay.ViewModelProperty)
        End Get
    End Property

    Private Shared ReadOnly ViewModelPropertyKey As DependencyPropertyKey = _
                            DependencyProperty.RegisterReadOnly("ViewModel", _
                            GetType(TrippLiteViewModel), GetType(StatusDisplay), _
                            New PropertyMetadata(New PropertyChangedCallback( _
                                                 Sub(sender As Object, e As DependencyPropertyChangedEventArgs)

                                                     Return
                                                 End Sub)))

    Public Shared ReadOnly ViewModelProperty As DependencyProperty = _
                           ViewModelPropertyKey.DependencyProperty


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

    Public Shared ReadOnly ItemSpacingProperty As  _
                           DependencyProperty = DependencyProperty.RegisterAttached("ItemSpacing", _
                           GetType(Thickness), GetType(StatusDisplay), _
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

    Public Shared ReadOnly DisplayCodesProperty As  _
                           DependencyProperty = DependencyProperty.RegisterAttached("DisplayCodes", _
                           GetType(ObservableCollection(Of TrippLiteCodes)), GetType(StatusDisplay), _
                           New PropertyMetadata(CType(AddressOf PropertyChanged, PropertyChangedCallback)))


    Private Shared Sub PropertyChanged(d As DependencyObject, _
                                e As DependencyPropertyChangedEventArgs)

    End Sub




    Public ReadOnly Property TrippLite As TrippLiteUPS
        Get
            TrippLite = _ViewModel.TrippLite
        End Get
    End Property

    Public Property DisplayView As StatusDisplayViews
        Get
            Return GetValue(DisplayViewProperty)
        End Get

        Set(ByVal value As StatusDisplayViews)
            SetValue(DisplayViewProperty, value)
        End Set
    End Property

    Public Shared ReadOnly DisplayViewProperty As DependencyProperty = _
                           DependencyProperty.Register("DisplayView", _
                           GetType(StatusDisplayViews), GetType(StatusDisplay), _
                           New PropertyMetadata(Nothing))

    Sub New()

        DisplayView = StatusDisplayViews.Medium
        DisplayCodes = New ObservableCollection(Of TrippLiteCodes)

        _ViewModel = New TrippLiteViewModel
        SetValue(ViewModelPropertyKey, _ViewModel)

        _ViewModel.Initialize()

        ' This call is required by the designer.
        InitializeComponent()

    End Sub

    Dim _rc As Integer = 0
    Dim _cc As Integer = 0

    Private Function CreateItem(prop As TrippLitePropertyViewModel, t As DataTemplate) As UIElement

        Dim gr As StackPanel

        gr = New StackPanel
        gr.Children.Add(t.LoadContent)
        gr.HorizontalAlignment = HorizontalAlignment.Left
        gr.DataContext = prop

        gr.Margin = ItemSpacing
        Return gr
    End Function

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

        If DesignerProperties.GetIsInDesignMode(Me) = True Then
            _ViewModel.TrippLite.RefreshData()
        Else
            _ViewModel.StartWatching()
        End If

    End Sub

    Private Sub StatusDisplay_Unloaded(sender As Object, e As System.Windows.RoutedEventArgs) Handles Me.Unloaded

        If _ViewModel IsNot Nothing Then

            _ViewModel.Dispose()

            SetValue(ViewModelPropertyKey, Nothing)
            _ViewModel = Nothing

        End If

    End Sub
End Class

<Flags>
Public Enum StatusDisplayViews
    Small = 1
    Medium = 2
    Large = 4
    Desktop = &H80
End Enum
