using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assisticant
{
    class WeakHashSet<T> : IEnumerable<T>
        where T : class
    {
        readonly HashSet<HashedWeakReference> _set = new HashSet<HashedWeakReference>();

        public int Count { get { return _set.Count; } }

        public void Add(T item)
        {
            _set.Add(new HashedWeakReference(item));
        }

        public void Remove(T item)
        {
            _set.Remove(new HashedWeakReference(item));
        }

        public bool Contains(T item)
        {
            return _set.Contains(new HashedWeakReference(item));
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() { return GetItems().GetEnumerator(); }
        IEnumerator IEnumerable.GetEnumerator() { return GetItems().GetEnumerator(); }

        IEnumerable<T> GetItems()
        {
            foreach (var reference in _set)
            {
                var target = reference.Target as T;
                if (target != null)
                    yield return target;
            }
        }

        class HashedWeakReference : WeakReference
        {
            int _hashCode;

            public HashedWeakReference(object target)
                : base(target)
            {
                _hashCode = target.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                var reference = obj as HashedWeakReference;
                if (reference == null)
                    return false;
                var target1 = Target;
                var target2 = reference.Target;
                if (target1 == null || target2 == null)
                    return false;
                return Equals(target1, target2);
            }

            public override int GetHashCode() { return _hashCode; }
        }
    }
}
