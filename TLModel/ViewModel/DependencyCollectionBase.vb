Option Strict On

Imports System
Imports System.Text
Imports System.Collections.ObjectModel
Imports System.Threading
Imports System.ComponentModel
Imports System.Windows
Imports System.Collections.Specialized

Public Interface IChild(Of T)
    Property Parent As T
End Interface

Public MustInherit Class DependencyCollectionBase(Of T As IChild(Of U), U)
    Inherits DependencyObject
    Implements INotifyCollectionChanged, INotifyPropertyChanged, ICollection(Of T), IChild(Of U)

    Protected _col As New Collection(Of T)
    Protected _Parent As U

    Public Event PropertyChanged(sender As Object, e As PropertyChangedEventArgs) Implements INotifyPropertyChanged.PropertyChanged

    Public Event CollectionChanged(sender As Object, e As NotifyCollectionChangedEventArgs) Implements INotifyCollectionChanged.CollectionChanged

    Public Sub New(parent As U)

        If parent Is Nothing Then
            Throw New ArgumentNullException()
            Return
        End If

        _Parent = parent
    End Sub

    Default Public Property Item(index As Integer) As T
        Get
            Return _col(index)
        End Get
        Set(value As T)
            _col(index) = value
            RaiseEvent CollectionChanged(Me, New NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, _col(index), index))
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Item"))
        End Set
    End Property

    Public Property Parent As U Implements IChild(Of U).Parent
        Get
            Return _Parent
        End Get
        Friend Set(value As U)
            _Parent = value
        End Set
    End Property

    Public Function TransferItem(item As T, newCol As DependencyCollectionBase(Of T, U)) As Boolean

        If Me.Contains(item) Then
            If Not newCol.Contains(item) Then
                Me.Remove(item)
                newCol.Add(item)
                item.Parent = newCol.Parent
                Return True
            End If
        End If

        Return False
    End Function

    Public Sub Add(item As T) Implements ICollection(Of T).Add
        _col.Add(item)
        RaiseEvent CollectionChanged(Me, New NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item))
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Count"))
    End Sub

    Public Sub Clear() Implements ICollection(Of T).Clear
        _col.Clear()
        RaiseEvent CollectionChanged(Me, New NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset))
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Count"))
    End Sub

    Public Function Contains(item As T) As Boolean Implements ICollection(Of T).Contains
        Return _col.Contains(item)
    End Function

    Public Sub CopyTo(array() As T, arrayIndex As Integer) Implements ICollection(Of T).CopyTo
        _col.CopyTo(array, arrayIndex)
    End Sub

    Public ReadOnly Property Count As Integer Implements ICollection(Of T).Count
        Get
            Count = _col.Count
        End Get
    End Property

    Public MustOverride ReadOnly Property IsReadOnly As Boolean Implements ICollection(Of T).IsReadOnly

    Public Function Remove(item As T) As Boolean Implements ICollection(Of T).Remove
        Return _col.Remove(item)
        RaiseEvent CollectionChanged(Me, New NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item))
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Count"))
    End Function

    Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
        Return New DependencyCollectionBaseEnumerator(Of T, U)(Me)
    End Function

    Function GetEnumerator1() As IEnumerator Implements IEnumerable.GetEnumerator
        Return New DependencyCollectionBaseEnumerator(Of T, U)(Me)
    End Function

End Class

Public NotInheritable Class DependencyCollectionBaseEnumerator(Of T As IChild(Of U), U)
    Implements IEnumerator(Of T)

    Private subj As DependencyCollectionBase(Of T, U)
    Private pos As Integer = -1

    Friend Sub New(subject As DependencyCollectionBase(Of T, U))
        subj = subject
    End Sub

    Public ReadOnly Property Current As T Implements IEnumerator(Of T).Current
        Get
            Return subj(pos)
        End Get
    End Property

    Public ReadOnly Property Current1 As Object Implements IEnumerator.Current
        Get
            Return subj(pos)
        End Get
    End Property

    Public Function MoveNext() As Boolean Implements IEnumerator.MoveNext
        pos += 1
        If pos >= subj.Count Then Return False
        Return True
    End Function

    Public Sub Reset() Implements IEnumerator.Reset
        pos = -1
    End Sub

#Region "IDisposable Support"
    Private disposedValue As Boolean ' To detect redundant calls

    ' IDisposable
    Protected Sub Dispose(disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                ' TODO: dispose managed state (managed objects).
                subj = Nothing
                pos = -1
            End If

            ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
            ' TODO: set large fields to null.
        End If
        Me.disposedValue = True
    End Sub

    ' TODO: override Finalize() only if Dispose(ByVal disposing As Boolean) above has code to free unmanaged resources.
    'Protected Overrides Sub Finalize()
    '    ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
    '    Dispose(False)
    '    MyBase.Finalize()
    'End Sub

    ' This code added by Visual Basic to correctly implement the disposable pattern.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
#End Region

End Class