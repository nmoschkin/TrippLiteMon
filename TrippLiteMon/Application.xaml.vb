
Class Application

    Private WithEvents _Main As PowerMon
    Private WithEvents _Cool As DesktopWindow

    ' Application-level events, such as Startup, Exit, and DispatcherUnhandledException
    ' can be handled in this file.

    Private Sub Application_SessionEnding(sender As Object, e As SessionEndingCancelEventArgs) Handles Me.SessionEnding

    End Sub

    Private Sub Application_Startup(sender As Object, e As StartupEventArgs) Handles Me.Startup
        System.Windows.Forms.Application.EnableVisualStyles()

        If Settings.LastWindow = LastWindowType.Cool Then
            SwitchToCool()
        Else
            SwitchToMain()
        End If

    End Sub

    Private Sub SwitchToMain()

        Dim rcM As System.Drawing.RectangleF = Settings.PrimaryWindowBounds

        _Main = New PowerMon
        _Main.Show()

        Settings.LastWindow = LastWindowType.Main

        If _Cool IsNot Nothing Then
            _Cool.ViewModel.StopWatching()
            _Cool.Close()
            _Cool.ViewModel.Dispose()
            _Cool = Nothing
        End If

        If rcM.Width <> 0 AndAlso rcM.Height <> 0 Then
            _Main.Left = rcM.Left
            _Main.Top = rcM.Top
            '_Main.Width = rcM.Width
            '_Main.Height = rcM.Height
        End If

        System.Threading.Thread.Sleep(100)
        GC.Collect(2)

    End Sub

    Private Sub SwitchToCool()

        Dim rcC As System.Drawing.RectangleF = Settings.CoolWindowBounds

        _Cool = New DesktopWindow
        _Cool.Show()

        Settings.LastWindow = LastWindowType.Cool

        If _Main IsNot Nothing Then
            _Main.ViewModel.StopWatching()
            _Main.Close()
            _Main.ViewModel.Dispose()
            _Main = Nothing
        End If

        If rcC.Width <> 0 AndAlso rcC.Height <> 0 Then
            _Cool.Left = rcC.Left
            _Cool.Top = rcC.Top
            '_Cool.Width = rcC.Width
            '_Cool.Height = rcC.Height
        End If

        System.Threading.Thread.Sleep(100)
        GC.Collect(2)

    End Sub

    Private Sub _Cool_OpenMainWindow(sender As Object, e As EventArgs) Handles _Cool.OpenMainWindow
        SwitchToMain()
    End Sub

    Private Sub _Main_OpenCoolWindow(sender As Object, e As EventArgs) Handles _Main.OpenCoolWindow
        SwitchToCool()
    End Sub

    Private Sub _Cool_LocationChanged(sender As Object, e As EventArgs) Handles _Cool.LocationChanged
        Settings.CoolWindowBounds = New System.Drawing.RectangleF(_Cool.Left, _Cool.Top, _Cool.Width, _Cool.Height)
    End Sub

    Private Sub _Cool_SizeChanged(sender As Object, e As SizeChangedEventArgs) Handles _Cool.SizeChanged
        Settings.CoolWindowBounds = New System.Drawing.RectangleF(_Cool.Left, _Cool.Top, _Cool.Width, _Cool.Height)
    End Sub

    Private Sub _Main_LocationChanged(sender As Object, e As EventArgs) Handles _Main.LocationChanged
        Settings.PrimaryWindowBounds = New System.Drawing.RectangleF(_Main.Left, _Main.Top, _Main.Width, _Main.Height)
    End Sub

    Private Sub _Main_SizeChanged(sender As Object, e As SizeChangedEventArgs) Handles _Main.SizeChanged
        Settings.PrimaryWindowBounds = New System.Drawing.RectangleF(_Main.Left, _Main.Top, _Main.Width, _Main.Height)
    End Sub

End Class