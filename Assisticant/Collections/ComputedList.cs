/**********************************************************************
 * 
 * Update Controls .NET
 * Copyright 2011 Michael L Perry
 * MIT License
 * 
 * http://updatecontrols.net
 * http://www.codeplex.com/updatecontrols/
 * 
 **********************************************************************/

using System;
using System.Collections.Generic;

namespace Assisticant.Collections
{
    public class ComputedList<T> : IEnumerable<T>
    {
        private readonly Func<IEnumerable<T>> _computeCollection;

        private List<T> _list = new List<T>();
        private Computed _computedSentry;

        public ComputedList(Func<IEnumerable<T>> computeCollection)
        {
            _computeCollection = computeCollection;

            _computedSentry = new NamedComputed(MemoizedTypeName<ComputedList<T>>.GenericName(),
			delegate {
                using (var bin = new RecycleBin<T>(_list))
                {
                    _list.Clear();

                    var collection = computeCollection();
                    if (collection != null)
                        foreach (T item in collection)
                            _list.Add(bin.Extract(item));
                }
            });
        }

        public int IndexOf(T item)
        {
            _computedSentry.OnGet();
            return _list.IndexOf(item);
        }

        public T this[int index]
        {
            get
            {
                _computedSentry.OnGet();
                return _list[index];
            }
        }

        public bool Contains(T item)
        {
            _computedSentry.OnGet();
            return _list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _computedSentry.OnGet();
            _list.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { _computedSentry.OnGet(); return _list.Count; }
        }

        public IEnumerator<T> GetEnumerator()
        {
            _computedSentry.OnGet();
            return _list.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            _computedSentry.OnGet();
            return ((System.Collections.IEnumerable)_list).GetEnumerator();
        }

        public Computed ComputedSentry
        {
            get { return _computedSentry; }
        }
    }
}
