/**********************************************************************
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
using Assisticant.Collections.Impl;

namespace Assisticant.Collections
{
	public class ObservableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IDictionary
	{
		private IDictionary<TKey, TValue> _dictionary = new Dictionary<TKey, TValue>();
		private Observable _indDictionary = new NamedObservable(MemoizedTypeName<ObservableDictionary<TKey, TValue>>.GenericName());

        public ObservableDictionary()
		{
			_dictionary = new Dictionary<TKey, TValue>();
		}
		public ObservableDictionary(IEqualityComparer<TKey> comp)
		{
			_dictionary = new Dictionary<TKey, TValue>(comp);
		}
		public ObservableDictionary(IDictionary<TKey,TValue> copy)
		{
			_dictionary = new Dictionary<TKey, TValue>(copy);
		}
		public ObservableDictionary(IDictionary<TKey, TValue> copy, IEqualityComparer<TKey> comp)
		{
			_dictionary = new Dictionary<TKey, TValue>(copy, comp);
		}

		public void Add(TKey key, TValue value)
		{
			_indDictionary.OnSet();
			_dictionary.Add(key, value);
		}

		public bool ContainsKey(TKey key)
		{
			_indDictionary.OnGet();
			return _dictionary.ContainsKey(key);
		}

		public UpdateCollectionHelper<TKey> Keys
		{
			get {
				return new UpdateCollectionHelper<TKey>(() =>
				{
					_indDictionary.OnGet();
					return _dictionary.Keys;
				});
			}
		}

        ICollection IDictionary.Values
        {
            get
            {
				_indDictionary.OnGet();
                return (ICollection)_dictionary.Values;
            }
        }

        ICollection<TKey> IDictionary<TKey, TValue>.Keys
        {
            get {
                return new UpdateCollectionHelper<TKey>(() =>
                {
                    _indDictionary.OnGet();
                    return _dictionary.Keys;
                });
            }
        }

		public bool Remove(TKey key)
		{
			_indDictionary.OnSet();
			return _dictionary.Remove(key);
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			_indDictionary.OnGet();
			return _dictionary.TryGetValue(key, out value);
		}

        ICollection IDictionary.Keys
        {
            get
            {
				_indDictionary.OnGet();
                return (ICollection) _dictionary.Keys;
            }
        }

        public UpdateCollectionHelper<TValue> Values
		{
			get {
				return new UpdateCollectionHelper<TValue>(() =>
				{
					_indDictionary.OnGet();
					return _dictionary.Values;
				});
			}
		}

        ICollection<TValue> IDictionary<TKey, TValue>.Values
        {
            get
            {
                return new UpdateCollectionHelper<TValue>(() =>
                {
                    _indDictionary.OnGet();
                    return _dictionary.Values;
                });
            }
        }

        public TValue this[TKey key]
		{
			get
			{
				_indDictionary.OnGet();
				return _dictionary[key];
			}
			set
			{
				_indDictionary.OnSet();
				_dictionary[key] = value;
			}
		}

		public void Add(KeyValuePair<TKey, TValue> item)
		{
			_indDictionary.OnSet();
			_dictionary.Add(item);
		}

        public void Add(object key, object value)
        {
            _indDictionary.OnSet();
            _dictionary.Add((TKey) key, (TValue) value);
        }

        public void Clear()
		{
			_indDictionary.OnSet();
			_dictionary.Clear();
		}

        public bool Contains(object key)
        {
            _indDictionary.OnGet();
            return _dictionary.ContainsKey((TKey) key);
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            _indDictionary.OnGet();
            return _dictionary.GetEnumerator() as IDictionaryEnumerator;
        }

        public void Remove(object key)
        {
            _indDictionary.OnSet();
            _dictionary.Remove((TKey) key);
        }

        public bool IsFixedSize => false;

        public bool Contains(KeyValuePair<TKey, TValue> item)
		{
			_indDictionary.OnGet();
			return _dictionary.Contains(item);
		}

		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			_indDictionary.OnGet();
			_dictionary.CopyTo(array, arrayIndex);
		}

        public void CopyTo(Array array, int index)
        {
            _indDictionary.OnGet();
            _dictionary.CopyTo((KeyValuePair<TKey, TValue>[]) array, index);
        }

        public int Count
		{
			get { _indDictionary.OnGet(); return _dictionary.Count; }
		}

        public bool IsSynchronized { get; }
        public object SyncRoot { get; }

        public bool IsReadOnly => false;

        public object this[object key]
        {
            get
            {
				_indDictionary.OnGet();
                return _dictionary[(TKey) key];
            }
            set
            {
				_indDictionary.OnSet();
                _dictionary[(TKey) key] = (TValue)value;
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
		{
			_indDictionary.OnSet();
			return _dictionary.Remove(item);
		}

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			_indDictionary.OnGet();
			return _dictionary.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			_indDictionary.OnGet();
			return ((System.Collections.IEnumerable)_dictionary).GetEnumerator();
		}
	}
}
