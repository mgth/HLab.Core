using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace HLab.Base;




public class ConcurrentHashSet<T> : IDisposable, IReadOnlyCollection<T>
{
    readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
    readonly HashSet<T> _hashSet = [];

    public IEnumerator<T> GetEnumerator() => _hashSet.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #region Implementation of ICollection<T> ...ish
    public bool Add(T item)
    {
        _lock.EnterReadLock();
        try
        {
            if(_hashSet.Contains(item)) return false;
        }
        finally
        {
            if (_lock.IsReadLockHeld) _lock.ExitReadLock();
        }
        _lock.EnterWriteLock();
        try
        {
            return _hashSet.Add(item);
        }
        finally
        {
            if (_lock.IsWriteLockHeld) _lock.ExitWriteLock();
        }
    }

    public void Clear()
    {
        _lock.EnterWriteLock();
        try
        {
            _hashSet.Clear();
        }
        finally
        {
            if (_lock.IsWriteLockHeld) _lock.ExitWriteLock();
        }
    }

    public bool Contains(T item)
    {
        _lock.EnterReadLock();
        try
        {
            return _hashSet.Contains(item);
        }
        finally
        {
            if (_lock.IsReadLockHeld) _lock.ExitReadLock();
        }
    }

    public bool TryTake(out T item)
    {
        _lock.EnterWriteLock();
        try
        {
            if (_hashSet.Count == 0)
            {
                item = default;
                return false;
            }

            T result = default;
            foreach (var entry in _hashSet)
            {
                result = entry;
                break;
            }

            if (_hashSet.Remove(result))
            {
                item = result;
                return true;
            }

            item = default;
            return false;
        }
        finally
        {
            if (_lock.IsWriteLockHeld) _lock.ExitWriteLock();
        }
    }

    public bool Remove(T item)
    {
        _lock.EnterWriteLock();
        try
        {
            return _hashSet.Remove(item);
        }
        finally
        {
            if (_lock.IsWriteLockHeld) _lock.ExitWriteLock();
        }
    }

    public int Count
    {
        get
        {
            _lock.EnterReadLock();
            try
            {
                return _hashSet.Count;
            }
            finally
            {
                if (_lock.IsReadLockHeld) _lock.ExitReadLock();
            }
        }
    }
    #endregion

    #region Dispose
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _lock?.Dispose();
        }
    }


    ~ConcurrentHashSet()
    {
        Dispose(false);
    }
    #endregion
}