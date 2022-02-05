﻿/**********************************************************************
 * 
 * Update Controls .NET
 * Copyright 2010 Michael L Perry
 * MIT License
 * 
 * http://updatecontrols.net
 * http://www.codeplex.com/updatecontrols/
 * 
 **********************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;

namespace Assisticant.Collections
{
	public class ObservableList<T> : IList<T>, IList, IReadOnlyList<T>
	{
        private List<T> _list;
		private Observable _indList = new NamedObservable(MemoizedTypeName<ObservableList<T>>.GenericName());

        public ObservableList()
        {
            _list = new List<T>();
        }

        public ObservableList(IEnumerable<T> collection)
        {
            _list = new List<T>(collection);
        }

        public int IndexOf(T item)
		{
			_indList.OnGet();
			return _list.IndexOf(item);
		}

		public void Insert(int index, T item)
		{
			_indList.OnSet();
			_list.Insert(index, item);
		}

		public void RemoveAt(int index)
		{
			_indList.OnSet();
			_list.RemoveAt(index);
		}

		public T this[int index]
		{
			get
			{
				_indList.OnGet();
				return _list[index];
			}
			set
			{
				_indList.OnSet();
				_list[index] = value;
			}
		}

		public void Add(T item)
		{
			_indList.OnSet();
			_list.Add(item);
		}

        public void AddRange(IEnumerable<T> items)
        {
            _indList.OnSet();
            _list.AddRange(items);
        }

		public void Clear()
		{
			_indList.OnSet();
			_list.Clear();
		}

		public bool Contains(T item)
		{
			_indList.OnGet();
			return _list.Contains(item);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			_indList.OnGet();
			_list.CopyTo(array, arrayIndex);
		}

		public int Count
		{
			get { _indList.OnGet(); return _list.Count; }
		}

		public bool IsReadOnly
		{
			get { return false; }
		}

		public bool Remove(T item)
		{
			_indList.OnSet();
			return _list.Remove(item);
		}

		public IEnumerator<T> GetEnumerator()
		{
			_indList.OnGet();
			return _list.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			_indList.OnGet();
			return ((System.Collections.IEnumerable)_list).GetEnumerator();
		}
    
        bool ICollection.IsSynchronized { get { return false; } }
        object ICollection.SyncRoot { get { return this; } }
        bool IList.IsFixedSize { get { return false; } }
        object IList.this[int index] { get { return this[index]; } set { this[index] = (T)value; } }

        int IList.Add(object value) { Add((T)value); return Count - 1; }
        bool IList.Contains(object value) { return Contains((T)value); }
        int IList.IndexOf(object value) { return IndexOf((T)value); }
        void IList.Insert(int index, object value) { Insert(index, (T)value); }
        void IList.Remove(object value) { Remove((T)value); }
        void ICollection.CopyTo(Array array, int index) { CopyTo((T[])array, index); }
    }
}
