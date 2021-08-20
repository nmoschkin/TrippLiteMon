using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;

namespace TrippLite
{
    public interface IChild<T>
    {
        T Parent { get; set; }
    }

    public abstract class DependencyCollectionBase<T, U> : DependencyObject, INotifyCollectionChanged, INotifyPropertyChanged, ICollection<T>, IChild<U> where T : IChild<U>
    {
        protected Collection<T> _col = new Collection<T>();
        protected U _Parent;

        public event PropertyChangedEventHandler PropertyChanged;
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public DependencyCollectionBase(U parent)
        {
            if (parent is null)
            {
                throw new ArgumentNullException();
                return;
            }

            _Parent = parent;
        }

        public T this[int index]
        {
            get
            {
                return _col[index];
            }

            set
            {
                _col[index] = value;
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, _col[index], index));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item"));
            }
        }

        public U Parent
        {
            get
            {
                return _Parent;
            }

            internal set
            {
                _Parent = value;
            }
        }

        U IChild<U>.Parent
        {
            get => _Parent;
            set => _Parent = value;
        }

        public bool TransferItem(T item, DependencyCollectionBase<T, U> newCol)
        {
            if (Contains(item))
            {
                if (!newCol.Contains(item))
                {
                    Remove(item);
                    newCol.Add(item);
                    item.Parent = newCol.Parent;
                    return true;
                }
            }

            return false;
        }

        public void Add(T item)
        {
            _col.Add(item);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Count"));
        }

        public void Clear()
        {
            _col.Clear();
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Count"));
        }

        public bool Contains(T item)
        {
            return _col.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _col.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get
            {
                int CountRet = default;
                CountRet = _col.Count;
                return CountRet;
            }
        }

        public abstract bool IsReadOnly { get; }

        public bool Remove(T item)
        {
            return _col.Remove(item);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Count"));
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new DependencyCollectionBaseEnumerator<T, U>(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new DependencyCollectionBaseEnumerator<T, U>(this);
        }

        public IEnumerator GetEnumerator1() => GetEnumerator();
    }

    public sealed class DependencyCollectionBaseEnumerator<T, U> : IEnumerator<T> where T : IChild<U>
    {
        private DependencyCollectionBase<T, U> subj;
        private int pos = -1;

        internal DependencyCollectionBaseEnumerator(DependencyCollectionBase<T, U> subject)
        {
            subj = subject;
        }

        public T Current
        {
            get
            {
                return subj[pos];
            }
        }

        object IEnumerator.Current
        {
            get
            {
                return subj[pos];
            }
        }

        public bool MoveNext()
        {
            pos += 1;
            if (pos >= subj.Count)
                return false;
            return true;
        }

        public void Reset()
        {
            pos = -1;
        }

        #region IDisposable Support
        private bool disposedValue; // To detect redundant calls

        // IDisposable
        protected void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    subj = null;
                    pos = -1;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                // TODO: set large fields to null.
            }

            disposedValue = true;
        }

        // TODO: override Finalize() only if Dispose(ByVal disposing As Boolean) above has code to free unmanaged resources.
        // Protected Overrides Sub Finalize()
        // ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
        // Dispose(False)
        // MyBase.Finalize()
        // End Sub

        // This code added by Visual Basic to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

    }
}