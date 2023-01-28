using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Studio.Framework
{
    [System.Serializable]
    public class CountingLock
    {
        public int count;

        [System.Serializable]
        private class Handle : System.IDisposable
        {
            [System.NonSerialized]
            private CountingLock _lock;

            private bool _disposed = false;

            [SerializeField]
            private string stackTrace;

            public Handle(CountingLock l)
            {
#if UNITY_EDITOR
                stackTrace = System.Environment.StackTrace;
#endif
                _lock = l;
                if (_lock != null)
                    _lock.LockInternal();
            }

            public void Dispose()
            {
                if (_disposed == false && _lock != null)
                {
                    _lock.UnlockInternal(this);
                    _lock = null;
                    _disposed = true;
                }
            }
        }

        private class MockHandle : System.IDisposable
        {
            public void Dispose() {}
        }

        public static readonly System.IDisposable DefaultHandle = new MockHandle();

        [SerializeField]
        private List<Handle> _handles = new List<Handle>();

        public System.IDisposable Lock()
        {
            var h = new Handle(this);
            _handles.Add(h);
            return h;
        }

        public async void Lock(float timeout)
        {
            using var h = Lock();
            await System.Threading.Tasks.Task.Delay((int)(timeout * 1000));
        }

        public void Reset()
        {
            count = 0;
            oldCount = 0;
        }

        private void LockInternal()
        {
            oldCount = count;
            ++count;
            OnValidate();
            _subject.OnNext(this);
        }

        [System.NonSerialized]
        public int oldCount, id;

        private void UnlockInternal(Handle h)
        {
            oldCount = count;
            --count;
            OnValidate();
            _subject.OnNext(this);
            
            _handles.Remove(h);
        }

        private UniRx.Subject<CountingLock> _subject = new UniRx.Subject<CountingLock>();
        public System.IObservable<CountingLock> AsObservable() => _subject;

        public static implicit operator int(CountingLock l)
        {
            return l.count;
        }

        public void OnValidate()
        {
            count = Mathf.Max(0, count);
        }

        public override string ToString()
        {
            return $"CountingLock:{id},{count}";
        }
    }
}