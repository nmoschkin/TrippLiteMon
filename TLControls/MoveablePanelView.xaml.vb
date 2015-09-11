Public Class MoveablePanelView

    Public Property Items As Object
        Get
            Return GetValue(ItemsProperty)
        End Get

        Set(ByVal value As Object)
            SetValue(ItemsProperty, value)
        End Set
    End Property

    Public Shared ReadOnly ItemsProperty As DependencyProperty = _
                           DependencyProperty.Register("Items", _
                           GetType(Object), GetType(MoveablePanelView), _
                           New PropertyMetadata(Nothing))

End Class
