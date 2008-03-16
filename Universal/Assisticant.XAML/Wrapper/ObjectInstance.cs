using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Assisticant.XAML.Wrapper
{
    internal interface IObjectInstance
    {
        object WrappedObject { get; }
        bool Equals(object obj);
        int GetHashCode();
        ObjectProperty LookupProperty(CustomMemberProvider member);
        string ToString();
        void FirePropertyChanged(string propertyName);
    }
    [DebuggerDisplay("ForView.Wrap({_wrappedObject})")]
    class ObjectInstance<T> : IObjectInstance, INotifyPropertyChanged, INotifyDataErrorInfo, IEditableObject
    {
        private readonly T _wrappedObject;

        private Dictionary<CustomMemberProvider, ObjectProperty> _propertyByName = new Dictionary<CustomMemberProvider, ObjectProperty>();

        public ObjectInstance(T wrappedObject)
        {
            _wrappedObject = wrappedObject;
        }

        public override bool Equals(object obj)
        {
            if (Object.ReferenceEquals(this, obj))
                return true;
            ObjectInstance<T> that = obj as ObjectInstance<T>;
            if (that == null)
                return false;
            return Object.Equals(this._wrappedObject, that._wrappedObject);
        }

        public override int GetHashCode()
        {
            return _wrappedObject.GetHashCode();
        }

        public override string ToString()
        {
            return String.Format("{0}", _wrappedObject);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void FirePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public object WrappedObject
        {
            get { return _wrappedObject; }
        }

        public ObjectProperty LookupProperty(CustomMemberProvider provider)
        {
            ObjectProperty computedProperty;
            if (!_propertyByName.TryGetValue(provider, out computedProperty))
            {
                PropertyInfo propertyInfo = _wrappedObject.GetType().GetRuntimeProperty(provider.Name);
                if (propertyInfo == null)
                    return null;

                computedProperty = new ObjectProperty(this, _wrappedObject, propertyInfo, provider);
                _propertyByName.Add(provider, computedProperty);
            }
            return computedProperty;
        }

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public System.Collections.IEnumerable GetErrors(string propertyName)
        {
            var errors = _wrappedObject as INotifyDataErrorInfo;
            if (errors != null)
                return errors.GetErrors(propertyName);
            else
                return Enumerable.Empty<object>();
        }

        public bool HasErrors
        {
            get
            {
                var errors = _wrappedObject as INotifyDataErrorInfo;
                if (errors != null)
                    return errors.HasErrors;
                else
                    return false;
            }
        }

        public void BeginEdit()
        {
            IEditableObject wrappedObject = _wrappedObject as IEditableObject;
            if (wrappedObject != null)
                wrappedObject.BeginEdit();
        }

        public void CancelEdit()
        {
            IEditableObject wrappedObject = _wrappedObject as IEditableObject;
            if (wrappedObject != null)
                wrappedObject.CancelEdit();
        }

        public void EndEdit()
        {
            IEditableObject wrappedObject = _wrappedObject as IEditableObject;
            if (wrappedObject != null)
                wrappedObject.EndEdit();
        }
    }
}
