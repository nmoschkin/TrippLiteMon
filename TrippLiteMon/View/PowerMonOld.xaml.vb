Imports DataTools.Interop

Public Class PowerMon

    Private WithEvents _TLStatus As TrippLiteUPS
    Private _props As List(Of TextBlock)

    Public Property TLStatus As TrippLiteUPS
        Get
            Return _TLStatus
        End Get
        Set(value As TrippLiteUPS)
            _TLStatus = value
        End Set
    End Property

    Sub New()
        InitializeComponent()

        Try
            TLStatus = New TrippLiteUPS(Me)
        Catch accex As AccessViolationException
            MsgBox("You don't have permission to run this program.")
            End
        Catch ex As Exception
            MsgBox(ex.Message)
            End
        End Try

        If TLStatus.IsTrippLite = False Then
            If MsgBox("A power device was found that was not a Tripp Lite device." & vbCrLf & "Do you want to run this program on that device, anyway?", MsgBoxStyle.YesNo) = MsgBoxResult.No Then
                End
            End If
        End If

        SerialNo.Text = TLStatus.Device.SerialNumber
        ProductId.Text = TLStatus.Device.ProductString
        ModelId.Text = "SMART" & TLStatus.PowerValue(TrippLiteCodes.VARATING) & "LCDT"
        Me.Title = TLStatus.Device.Manufacturer & " " & TLStatus.Device.ClassName

        Dim screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth
        Dim screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight
        Dim windowWidth = Me.Width
        Dim windowHeight = Me.Height
        Me.Left = (screenWidth / 2) - (windowWidth / 2)
        Me.Top = (screenHeight / 2) - (windowHeight / 2)

        Dim rows() As Integer = TLStatus.FieldValues

        Dim i As Integer, _
            c As Integer = rows.Count - 1

        Dim e As Integer = 0
        Dim f As Integer = 0

        Dim tb As TextBlock
        Dim rd As RowDefinition

        Dim s As String

        _props = New List(Of TextBlock)
        Dim grd As Grid = Settings1

        For i = 0 To c
            s = TLStatus.PowerString(rows(i))

            If s.IndexOf("Voltage") > 0 Then
                Select Case s
                    Case "Battery Voltage"
                        s = "Batt. Voltage"
                    Case "High Voltage Transfer"
                        s = "High Voltage Xfer"
                    Case "Low Voltage Transfer"
                        s = "Low Voltage Xfer"
                    Case "Input Voltage", "Output Voltage"
                        Continue For
                End Select
            End If
            If s.IndexOf("VA RATING") >= 0 Then Continue For

            If e = 8 Then
                If grd Is Settings1 Then
                    grd = Settings2
                ElseIf grd Is Settings2 Then
                    grd = Settings3
                ElseIf grd Is Settings3 Then
                    grd = Settings4
                Else
                    Exit For
                End If
                e = 0
            End If

            tb = New TextBlock

            tb.Text = If(s.IndexOf("Time") >= 0, "Battery Minutes", s)
            tb.Margin = New Thickness(2, 2, 2, 1)
            tb.FontFamily = MainsLabel.FontFamily
            tb.FontSize = MainsLabel.FontSize

            tb.Foreground = MainsLabel.Foreground

            tb.HorizontalAlignment = Windows.HorizontalAlignment.Left
            tb.VerticalAlignment = Windows.VerticalAlignment.Center

            rd = New RowDefinition With {.Height = New GridLength(12)}

            tb.SetValue(Grid.RowProperty, e)
            tb.SetValue(Grid.ColumnProperty, f)
            tb.SetValue(Grid.ColumnSpanProperty, 1)

            e += 1
            grd.RowDefinitions.Add(rd)
            grd.Children.Add(tb)

            tb = New TextBlock
            tb.Text = ""

            tb.Margin = New Thickness(2, 2, 2, 1)
            tb.FontFamily = LineVoltage.FontFamily
            tb.FontSize = 12
            tb.Foreground = LineVoltage.Foreground

            tb.HorizontalAlignment = Windows.HorizontalAlignment.Left
            tb.VerticalAlignment = Windows.VerticalAlignment.Center

            rd = New RowDefinition With {.Height = New GridLength(20)}

            tb.SetValue(Grid.RowProperty, e)
            tb.SetValue(Grid.ColumnProperty, f)
            tb.SetValue(Grid.ColumnSpanProperty, 1)
            e += 1

            grd.RowDefinitions.Add(rd)
            grd.Children.Add(tb)
            tb.Tag = rows(i)

            _props.Add(tb)
        Next

    End Sub

    Private Sub TLStatus_PropertyChanged(sender As Object, e As ComponentModel.PropertyChangedEventArgs) Handles _TLStatus.PropertyChanged

        LineVoltage.Text = TLStatus.PowerValue(TrippLiteCodes.InputVoltage).ToString("000.0")
        FeedVoltage.Text = TLStatus.PowerValue(TrippLiteCodes.OutputVoltage).ToString("000.0")

        Dim tc As TrippLiteCodes

        For Each pr In _props

            tc = CType(pr.Tag, TrippLiteCodes)

            If tc = TrippLiteCodes.OutputLoad Then
                LoadVisual.Value = TLStatus.PowerValue(tc)
            End If

            pr.Text = TLStatus.FormattedPowerValue(tc)
        Next

    End Sub

    Private Sub PowerMon_Closing(sender As Object, e As ComponentModel.CancelEventArgs) Handles Me.Closing
        _TLStatus.Dispose()
    End Sub

End Class

<Flags>
Public Enum PowerMonViews As Byte
    Small = 1
    Medium = 2
    Large = 4
    Desktop = &H80
End Enum

Public Class PowerMonView

    Private _View As PowerMonViews = PowerMonViews.Small

    Public Property View As PowerMonViews
        Get
            Return _View
        End Get
        Set(value As PowerMonViews)
            _View = value
        End Set
    End Property

End Class