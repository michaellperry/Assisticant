/**********************************************************************
 * 
 * Update Controls .NET
 * Copyright 2011 Michael L Perry
 * MIT License
 * 
 * This class based on a contribution by David Piepgrass.
 * 
 * http://updatecontrols.net
 * http://www.codeplex.com/updatecontrols/
 * 
 **********************************************************************/
using System;
using System.ComponentModel;

namespace Assisticant.Fields
{
    public class Observable<T> : NamedObservable
    {
		protected internal T _value;

		public Observable() { }
		public Observable(T value) : this((string)null, value) { }
		
		// Oops, this constructor causes ambiguity in case of Observable<string>. 
		// In that case, C# compilers will reinterpret existing code that previously 
		// used the Observable(value) constructor to call Observable(name) instead.
		//public Observable(string name) : base(name) { }
		
		public Observable(string name, T value) : base(name) { _value = value; }
		public Observable(Type containerType, string name) : base(containerType, name) { }
		public Observable(Type containerType, string name, T value) : base(containerType, name) { _value = value; }

		public T Value
		{
			get { base.OnGet(); return _value; }
			set {
				if (_value == null ? value != null : !_value.Equals(value))
				{
					base.OnSet();
					_value = value;
				}
			}
		}
		public static implicit operator T(Observable<T> observable)
		{
			return observable.Value;
		}

		public override string VisualizerName(bool withValue)
		{
			string s = "[I] " + Computed<T>.VisualizerName(Name);
			if (withValue)
				s += " = " + (_value == null ? "null" : _value.ToString());
			return s;
		}

		[Obsolete, EditorBrowsable(EditorBrowsableState.Never)]
		public Observable ObservableSentry
		{
			get { return this; }
		}
    }
}