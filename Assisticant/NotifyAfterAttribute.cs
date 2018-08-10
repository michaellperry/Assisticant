using System;

namespace Assisticant
{
    [AttributeUsage(AttributeTargets.Property)]
    public class NotifyAfterAttribute : Attribute
    {
        public string OtherProperty { get; }

        public NotifyAfterAttribute(string otherProperty)
        {
            OtherProperty = otherProperty;
        }
    }
}
