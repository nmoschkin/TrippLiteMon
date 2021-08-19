Imports System.ComponentModel
Imports DataTools.ExtendedMath
Imports DataTools.ExtendedMath.Polar

<Serializable>
Public Class LoadBar

    Private _refSize As Size = New Size(0, 0)
    Private _angle As Double = 0
    Private _maxSections As Integer = 28

    Private _polyCache As New List(Of Polygon)

    Public Shared ReadOnly BluntingProperty As DependencyProperty = DependencyProperty.Register("Blunting",
      GetType(Double), GetType(LoadBar),
      New PropertyMetadata(Nothing))

    Public Shared ReadOnly SectionSpacingProperty As DependencyProperty = DependencyProperty.Register("SectionSpacing",
      GetType(Double), GetType(LoadBar),
      New PropertyMetadata(Nothing))

    Public Shared ReadOnly SectionsProperty As DependencyProperty = DependencyProperty.Register("Sections",
      GetType(UInteger), GetType(LoadBar),
      New PropertyMetadata(Nothing))

    'Public Shared ReadOnly OutlinePointsProperty As DependencyProperty = DependencyProperty.Register("OutlinePoints",
    '  GetType(PointCollection), GetType(LoadBar),
    '  New PropertyMetadata(Nothing))

    Public Shared ReadOnly EffectLevelProperty As DependencyProperty = DependencyProperty.Register("EffectLevel",
      GetType(Double), GetType(LoadBar),
      New PropertyMetadata(Nothing))

    '' Load value

    <Browsable(True), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)>
    Public Property LoadValue As Double
        Get
            LoadValue = DirectCast(GetValue(LoadValueProperty), Double)
        End Get
        Set(value As Double)
            SetValue(LoadValueProperty, value)
        End Set
    End Property

    Public Shared Function GetLoadValue(ByVal element As DependencyObject) As Double
        If element Is Nothing Then
            Throw New ArgumentNullException("element")
        End If

        Return element.GetValue(LoadValueProperty)
    End Function

    Public Shared Sub SetLoadValue(ByVal element As DependencyObject, ByVal value As Double)
        If element Is Nothing Then
            Throw New ArgumentNullException("element")
        End If

        element.SetValue(LoadValueProperty, value)
    End Sub

    Public Shared ReadOnly LoadValueProperty As _
                           DependencyProperty = DependencyProperty.RegisterAttached("LoadValue",
                           GetType(Double), GetType(LoadBar),
                           New PropertyMetadata(New PropertyChangedCallback(AddressOf DepPropChanged)))


    Protected Shared Sub DepPropChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
        Dim lb As LoadBar = d
        lb.ChangeSectionsValues()
    End Sub

    <Browsable(True), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)>
    Public Property Blunting As Double
        Get
            Blunting = DirectCast(GetValue(BluntingProperty), Double)
        End Get
        Set(value As Double)
            SetValue(BluntingProperty, value)
            Recalculate()
        End Set
    End Property

    <Browsable(True), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)>
    Public Property Sections As UInteger
        Get
            Sections = DirectCast(GetValue(SectionsProperty), UInteger)
        End Get
        Set(value As UInteger)
            If value < 1 OrElse value > _maxSections Then
                Throw New ArgumentOutOfRangeException("Value must be a postive non-zero integer value less than or equal to " & _maxSections & ".")
            End If

            SetValue(SectionsProperty, value)
            Recalculate()
        End Set
    End Property

    <Browsable(True), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)>
    Public Property SectionSpacing As Double
        Get
            SectionSpacing = DirectCast(GetValue(SectionSpacingProperty), Double)
        End Get
        Set(value As Double)
            SetValue(SectionSpacingProperty, value)
            Recalculate()
        End Set
    End Property

    <Browsable(True), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)>
    Public Property EffectLevel As Double
        Get
            EffectLevel = DirectCast(GetValue(EffectLevelProperty), Double)
        End Get
        Set(value As Double)
            SetValue(EffectLevelProperty, value)
            ChangeSectionsValues(True)
        End Set
    End Property

    '<Browsable(True), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)>
    'Public Property OutlinePoints As PointCollection
    '    Get
    '        OutlinePoints = DirectCast(GetValue(OutlinePointsProperty), PointCollection)
    '    End Get
    '    Set(value As PointCollection)
    '        SetValue(OutlinePointsProperty, value)
    '    End Set
    'End Property

    ''' <summary>
    ''' Returns the maximum number of sections supported for this instance.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property MaxSections As Integer
        Get
            Return _maxSections
        End Get
    End Property

    ''' <summary>
    ''' Perform a complete recalculation of the control.
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub Recalculate()
        If DrawingArea Is Nothing Then Return

        _refSize = DrawingArea.RenderSize
        If _refSize.Height = 0 Then Exit Sub

        'UpdateOutline()

        '' this is the current angle calculated from the current box dimensions
        Dim pol As Polar.PolarCoordinates = Polar.ToPolarCoordinates(_refSize.Width, _refSize.Height)
        _angle = pol.Angle

        CalculateSections()
    End Sub

    '''' <summary>
    '''' Update the rectangular outline of the control.
    '''' </summary>
    '''' <remarks></remarks>
    'Private Sub UpdateOutline()
    '    Dim s As Size = _refSize
    '    Dim pc As New PointCollection

    '    pc.Add(New Point(0, s.Height))
    '    pc.Add(New Point(s.Width, s.Height))
    '    pc.Add(New Point(s.Width, 0))

    '    OutlinePoints = pc
    'End Sub

    ''' <summary>
    ''' Calculate only the lighted bars and backing effect.
    ''' </summary>
    ''' <param name="effectOnly"></param>
    ''' <remarks></remarks>
    Private Sub ChangeSectionsValues(Optional ByVal effectOnly As Boolean = False)

        If effectOnly Then
            For i = 0 To Sections - 1
                _polyCache(i).Effect = BackingBar.Effect
            Next
        Else

            Dim cblin As Integer = CalcBars(LoadValue, Sections) - 1
            Dim i As Integer
            Dim p As Polygon

            For i = 0 To Sections - 1

                p = _polyCache(i)

                p.Effect = BackingBar.Effect

                If cblin < i Then
                    If p.Opacity <> 0.33 Then p.Opacity = 0.33
                Else
                    If p.Opacity <> 1 Then p.Opacity = 1
                End If

            Next

        End If

    End Sub

    ''' <summary>
    ''' Calculate and position the polygon sections, based on geometry, for the current size, position, and number of bars.
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub CalculateSections()

        Dim s As Size = _refSize
        Dim pc As PointCollection
        Dim d As Integer = Sections - 1
        Dim p As Polygon

        Dim wHeight As Double = s.Height - Blunting
        Dim wWidth As Double = s.Width

        Dim bc As SolidColorBrush = Application.Current.Resources("TrippLiteLcdType")
        '' the width of each section will be calculated here, subtracting initial width by the spacing
        Dim pol As PolarCoordinates = ToPolarCoordinates(wWidth, wHeight)

        Dim w As Double = (wWidth - (SectionSpacing * (Sections - 1))) / Sections
        Dim sw As Double = SectionSpacing

        Dim pt As Point

        Dim i As Integer
        Dim x As Double = 0
        '' calculate from hypoteneuse side.
        Dim fr As Double = pol.Radius / wWidth
        Dim cblin As Integer = CalcBars(LoadValue, Sections) - 1

        BarsArea.Children.Clear()
        System.Threading.Thread.Sleep(0)

        For i = 0 To Sections - 1
            '' fresh point collection
            pc = New PointCollection

            p = _polyCache(i)

            pol.Radius = x + w
            pol.Radius *= fr
            pt = Polar.ToScreenCoordinates(pol)

            pc.Add(New Point(x, s.Height))
            pc.Add(New Point(pt.X, s.Height))
            pc.Add(New Point(pt.X, Math.Max(0, wHeight - pt.Y)))

            pol.Radius = x
            pol.Radius *= fr
            pt = Polar.ToScreenCoordinates(pol)

            pc.Add(New Point(pt.X, Math.Max(0, wHeight - pt.Y)))
            pc.Add(New Point(pt.X, s.Height))

            p.Points = pc
            p.Fill = bc
            p.Effect = BackingBar.Effect

            If cblin < i Then
                p.Opacity = 0.33
            Else
                p.Opacity = 1
            End If

            BarsArea.Children.Add(p)
            x += w
            x += sw
        Next

    End Sub

    ''' <summary>
    ''' Calculates the number of bars that should be lit for the given value.
    ''' </summary>
    ''' <param name="value"></param>
    ''' <param name="numBars"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function CalcBars(value As Double, numBars As Integer) As Integer
        Return Math.Ceiling(value / (100 / numBars))
    End Function

    Public Sub New(maxSections As Integer)
        InitializeComponent()

        _maxSections = maxSections
        instanceInit()
    End Sub

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()
        instanceInit()

    End Sub

    ''' <summary>
    ''' Initialize the instance of the load bar.
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub instanceInit()

        For i = 1 To _maxSections
            _polyCache.Add(New Polygon)
        Next

        EffectLevel = 0.0#
        LoadValue = 100
        Sections = 10
        SectionSpacing = 1
        Blunting = 3

    End Sub

    ''' <summary>
    ''' do the obvious.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub LoadBar_SizeChanged(sender As Object, e As SizeChangedEventArgs) Handles Me.SizeChanged
        Recalculate()
    End Sub

End Class
