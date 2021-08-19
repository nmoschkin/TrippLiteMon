Option Explicit On

Imports System
Imports System.Windows
Imports System.Collections.ObjectModel
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.IO
Imports System.Text

Imports TrippLite

Public Class DisplayConfig


    Private WithEvents _ViewModel As TrippLiteViewModel

    Public Property ViewModel As TrippLiteViewModel
        Get
            Return GetValue(ViewModelProperty)
        End Get

        Set(ByVal value As TrippLiteViewModel)
            SetValue(ViewModelProperty, value)
            _ViewModel = value
        End Set
    End Property

    Public Shared ReadOnly ViewModelProperty As DependencyProperty = _
                           DependencyProperty.Register("ViewModel", _
                           GetType(TrippLiteViewModel), GetType(DisplayConfig), _
                           New PropertyMetadata(Nothing))

    Public Sub New(m As TrippLiteViewModel)

        InitializeComponent()
        ViewModel = m

    End Sub


End Class
