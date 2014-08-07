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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Assisticant
{
    /// <summary>
    /// Base class for <see cref="Observable"/> and <see cref="Computed"/> sentries.
    /// </summary>
    /// <threadsafety static="true" instance="true"/>
    /// <remarks>
    /// This class is for internal use only.
    /// </remarks>
    public abstract class Precedent
    {
        const int _maxArraySize = 16;

        WeakReference[] _computedArray;
        WeakHashSet<Computed> _computedHash;

        /// <summary>
        /// Method called when the first dependent references this field. This event only
        /// fires when HasDependents goes from false to true. If the field already
        /// has dependents, then this event does not fire.
        /// </summary>
        protected virtual void GainDependent()
        {
        }

        /// <summary>
        /// Method called when the last dependent goes out-of-date. This event
        /// only fires when HasDependents goes from true to false. If the field has
        /// other dependents, then this event does not fire. If the dependent is
        /// currently updating and it still depends upon this field, then the
        /// GainComputed event will be fired immediately.
        /// </summary>
        protected virtual void LoseDependent()
        {
        }

        /// <summary>
        /// Establishes a relationship between this precedent and the currently
        /// updating dependent.
        /// </summary>
        internal void RecordDependent()
        {
            // Get the current dependent.
            Computed update = Computed.GetCurrentUpdate();
            if (update != null && !Contains(update) && update.AddPrecedent(this))
            {
                if (Insert(update))
                    GainDependent();
            }
            else if (!Any())
            {
                // Though there is no lasting dependency, someone
                // has shown interest.
                GainDependent();
                LoseDependent();
            }
        }

        /// <summary>
        /// Makes all direct and indirect dependents out of date.
        /// </summary>
        internal void MakeDependentsOutOfDate()
        {
            if (_computedArray != null)
            {
                foreach (var computed in WeakArray.Enumerate<Computed>(_computedArray).ToList())
                    computed.MakeOutOfDate();
            }
            else if (_computedHash != null)
            {
                foreach (var computed in _computedHash.ToList())
                    computed.MakeOutOfDate();
            }
        }

        internal void RemoveDependent(Computed dependent)
        {
            if (Delete(dependent))
                LoseDependent();
        }

        /// <summary>
        /// True if any other fields depend upon this one.
        /// </summary>
        /// <remarks>
        /// If any dependent field has used this observable field while updating,
        /// then HasDependents is true. When that dependent becomes out-of-date,
        /// however, it no longer depends upon this field.
        /// <para/>
        /// This property is useful for caching. When all dependents are up-to-date,
        /// check this property for cached fields. If it is false, then nothing
        /// depends upon the field, and it can be unloaded. Be careful not to
        /// unload the cache while dependents are still out-of-date, since
        /// those dependents may in fact need the field when they update.
        /// </remarks>
        public bool HasDependents
		{
            get { return Any(); }
		}

        private bool Insert(Computed update)
        {
            lock (this)
            {
                if (_computedHash != null)
                {
                    _computedHash.Add(update);
                    return false;
                }
                if (WeakArray.Contains(ref _computedArray, update))
                    return false;
                bool first = _computedArray == null;
                if (WeakArray.GetCount(ref _computedArray) >= _maxArraySize)
                {
                    _computedHash = new WeakHashSet<Computed>();
                    foreach (var item in WeakArray.Enumerate<Computed>(_computedArray))
                        _computedHash.Add(item);
                    _computedArray = null;
                    _computedHash.Add(update);
                    return false;
                }
                WeakArray.Add(ref _computedArray, update);
                return first;
            }
        }

        private bool Delete(Computed dependent)
        {
            lock (this)
            {
                if (_computedArray != null)
                {
                    WeakArray.Remove(ref _computedArray, dependent);
                    return _computedArray == null;
                }
                else if (_computedHash != null)
                {
                    _computedHash.Remove(dependent);
                    if (_computedHash.Count == 0)
                    {
                        _computedHash = null;
                        return true;
                    }
                    return false;
                }
                else
                    return false;
            }
        }

        private bool Contains(Computed update)
        {
            lock (this)
            {
                if (_computedArray != null)
                    return WeakArray.Contains(ref _computedArray, update);
                else if (_computedHash != null)
                    return _computedHash.Contains(update);
                else
                    return false;
            }
        }

        private bool Any()
        {
            lock (this)
            {
                return _computedArray != null || _computedHash != null;
            }
        }

		public override string ToString()
		{
			return VisualizerName(true);
		}

		#region Debugger Visualization

		/// <summary>Gets or sets a flag that allows extra debug features.</summary>
		/// <remarks>
		/// This flag currently just controls automatic name detection for untitled
		/// NamedObservables, and other precedents that were created without a name 
		/// by calling <see cref="Observable.New"/>() or <see cref="Computed.New"/>(),
		/// including dependents created implicitly by <see cref="GuiUpdateHelper"/>.
		/// <para/>
		/// DebugMode should be enabled before creating any Assisticant sentries,
		/// otherwise some of them may never get a name. For example, if 
		/// Indepedent.New() is called (without arguments) when DebugMode is false, 
		/// a "regular" <see cref="Observable"/> is created that is incapable of 
		/// having a name.
		/// <para/>
		/// DebugMode may slow down your program. In particular, if you use named 
		/// observables (or <see cref="Observable{T}"/>) but do not explicitly 
		/// specify a name, DebugMode will cause them to compute their names based 
		/// on a stack trace the first time OnGet() is called; this process is
		/// expensive if it is repeated for a large number of Observables.
		/// </remarks>
		public static bool DebugMode { get; set; }
		
		public virtual string VisualizerName(bool withValue)
		{
			return VisNameWithOptionalHash(GetType().Name, withValue);
		}
		protected string VisNameWithOptionalHash(string name, bool withHash)
		{
			if (withHash) {
				// Unless VisualizerName has been overridden, we have no idea what 
				// value is associated with the Precedent. Include an ID code so 
				// that the user has a chance to detect duplicates (that is, when
				// he sees two Observables with the same code, they are probably 
				// the same Observable.)
				return string.Format("{0} #{1:X5}", name, GetHashCode() & 0xFFFFF);
			} else
				return name;
		}

		protected class DependentVisualizer
		{
			Precedent _self;
			public DependentVisualizer(Precedent self) { _self = self; }
			public override string ToString() { return _self.VisualizerName(true); }

			[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
			public DependentVisualizer[] Items
			{
				get {
					var list = new List<DependentVisualizer>();
					lock (_self)
					{
                        if (_self._computedArray != null)
                        {
                            foreach (var item in WeakArray.Enumerate<Computed>(_self._computedArray))
                                list.Add(new DependentVisualizer(item));
                        }
                        else if (_self._computedHash != null)
                        {
                            foreach (var item in _self._computedHash)
                                list.Add(new DependentVisualizer(item));
                        }

						list.Sort((a, b) => a.ToString().CompareTo(b.ToString()));

						// Return as array so that the debugger doesn't offer a useless "Raw View"
						return list.ToArray();
					}
				}
			}
		}

		#endregion
	}
}
