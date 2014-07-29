using System.Collections.Generic;
using System.Threading;

namespace Assisticant
{
    public class ThreadLocal<T>
    {
        private Dictionary<int, T> _valueByThread = new Dictionary<int, T>();

        public T Value
        {
            get
            {
                lock (this)
                {
                    T value;
                    if (!_valueByThread.TryGetValue(Thread.CurrentThread.ManagedThreadId, out value))
                        return default(T);
                    return value;
                }
            }
            set
            {
                lock (this)
                {
                    _valueByThread[Thread.CurrentThread.ManagedThreadId] = value;
                }
            }
        }
    }
}
