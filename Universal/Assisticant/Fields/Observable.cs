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
    public class Observable<T> : Observable
    {
		protected internal T _value;

		public Observable() { }
        public Observable(T value) { _value = value; }

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
			string s = String.Format("Observable<{0}>", typeof(T).Name);
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