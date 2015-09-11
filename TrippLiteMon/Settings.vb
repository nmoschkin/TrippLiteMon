Option Explicit On
Option Compare Binary

Imports System
Imports System.IO
Imports System.Text
Imports System.Drawing
Imports Microsoft.Win32
Imports DataTools.Memory

#Region "Registry Settings"

''' <summary>
''' Specifies the last default window type of the active display.
''' </summary>
''' <remarks></remarks>
Public Enum LastWindowType
    ''' <summary>
    ''' The main window was the last to be displayed.
    ''' </summary>
    ''' <remarks></remarks>
    Main

    ''' <summary>
    ''' The cool window was the last to be displayed.
    ''' </summary>
    ''' <remarks></remarks>
    Cool
End Enum

Public Class Settings

    Public Const TrippLiteMonKey As String = "Software\TrippLiteMon\TrippLiteMon"

    Public Shared Property LastWindow As LastWindowType
        Get
            Dim key As RegistryKey = Registry.CurrentUser.CreateSubKey(TrippLiteMonKey & "", RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryOptions.None)
            LastWindow = key.GetValue("LastWindow", 0)
            key.Close()

        End Get
        Set(value As LastWindowType)
            Dim key As RegistryKey = Registry.CurrentUser.CreateSubKey(TrippLiteMonKey & "", RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryOptions.None)

            key.SetValue("LastWindow", value, RegistryValueKind.DWord)
            key.Close()

        End Set
    End Property

    Public Shared Property PrimaryWindowBounds As RectangleF
        Get
            Dim key As RegistryKey = Registry.CurrentUser.CreateSubKey(TrippLiteMonKey & "", RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryOptions.None)

            Dim mm As New MemPtr(16)
            mm.SetBytes(0, key.GetValue("PrimaryWindowBounds", CType(mm, Byte())))
            key.Close()

            If mm.SingleAt(2) = 0 OrElse mm.SingleAt(3) = 0 Then
                mm.Free()
                Return New RectangleF(0, 0, 0, 0)
            End If

            PrimaryWindowBounds = New RectangleF(mm.SingleAt(0), mm.SingleAt(1), mm.SingleAt(2), mm.SingleAt(3))
            mm.Free()

        End Get
        Set(value As RectangleF)
            Dim key As RegistryKey = Registry.CurrentUser.CreateSubKey(TrippLiteMonKey & "", RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryOptions.None)

            Dim mm As New MemPtr(16)
            mm.FromStruct(Of RectangleF)(value)

            key.SetValue("PrimaryWindowBounds", CType(mm, Byte()), RegistryValueKind.Binary)
            key.Close()

            mm.Free()
        End Set
    End Property

    Public Shared Property CoolWindowBounds As RectangleF
        Get
            Dim key As RegistryKey = Registry.CurrentUser.CreateSubKey(TrippLiteMonKey & "", RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryOptions.None)

            Dim mm As New MemPtr(16)
            mm.SetBytes(0, key.GetValue("CoolWindowBounds", CType(mm, Byte())))
            key.Close()

            If mm.SingleAt(2) = 0 OrElse mm.SingleAt(3) = 0 Then
                mm.Free()
                Return New RectangleF(0, 0, 0, 0)
            End If

            CoolWindowBounds = New RectangleF(mm.SingleAt(0), mm.SingleAt(1), mm.SingleAt(2), mm.SingleAt(3))
            mm.Free()

        End Get
        Set(value As RectangleF)
            Dim key As RegistryKey = Registry.CurrentUser.CreateSubKey(TrippLiteMonKey & "", RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryOptions.None)

            Dim mm As New MemPtr(16)
            mm.FromStruct(Of RectangleF)(value)

            key.SetValue("CoolWindowBounds", CType(mm, Byte()), RegistryValueKind.Binary)
            key.Close()

            mm.Free()
        End Set
    End Property



End Class

#End Region
