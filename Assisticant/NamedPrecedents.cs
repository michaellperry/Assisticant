using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Reflection;

namespace Assisticant
{
	public class NamedComputed : Computed
	{
		public NamedComputed(Action update) : this(null, update) { }
		public NamedComputed(string name, Action update) : base(update) { _name = name; }

		protected string _name;
		public string Name
		{
			get {
				if (_name == null)
					_name = ComputeName();
				return _name;
			}
		}

		public override string VisualizerName(bool withValue)
		{
			return VisNameWithOptionalHash(Name, withValue);
		}
		public static string GetClassAndMethodName(Delegate d)
		{
            var method = GetMethodInfo(d);
            return MemoizedTypeName.GenericName(method.DeclaringType) + "." + method.Name;
		}
        static MethodInfo GetMethodInfo(Delegate d)
        {
#if UNIVERSAL
            return d.GetMethodInfo();
#else
            return d.Method;
#endif
        }
		protected virtual string ComputeName()
		{
			return GetClassAndMethodName(_update) + "()";
		}
	}

	public class NamedObservable : Observable
	{
		public NamedObservable() : base() { }
		public NamedObservable(string name) : base() { _name = name; }
		public NamedObservable(Type valueType) : this(valueType.NameWithGenericParams()) { }
		public NamedObservable(Type containerType, string name) :
			this(string.Format("{0}.{1}", containerType.NameWithGenericParams(), name)) { }

		public override void OnGet()
		{
            // TODO: Figure out _name
			base.OnGet();
		}

        protected string _name;
		public string Name
		{
			get { return _name ?? "NamedObservable"; }
			set { _name = value; }
		}

		public override string VisualizerName(bool withValue)
		{
			return VisNameWithOptionalHash("[I] " + Name, withValue);
		}
	}

	[Obsolete]
	public class NamedObservable<T> : Assisticant.Fields.Observable<T>
	{
		public NamedObservable() : base() { }
		public NamedObservable(T value) : base(value) { }
		public NamedObservable(string name, T value) : base(name, value) { }
		public NamedObservable(Type containerType, string name) : base(containerType, name) { }
		public NamedObservable(Type containerType, string name, T value) : base(containerType, name, value) { }
	}

	[Obsolete]
	public class NamedComputed<T> : Assisticant.Fields.Computed<T>
	{
		public NamedComputed(Func<T> compute) : base(compute) { }
		public NamedComputed(string name, Func<T> compute) : base(name, compute) { }
	}
}
