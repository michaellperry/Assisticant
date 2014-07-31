/**********************************************************************
 * 
 * Update Controls .NET
 * Copyright 2010 Michael L Perry
 * MIT License
 * 
 * http://updatecontrols.net
 * http://updatecontrols.codeplex.com/
 * 
 **********************************************************************/

using System;

namespace Assisticant.XAML.Wrapper
{
    internal class ObjectPropertyCollectionObject : ObjectPropertyCollection
	{
        public ObjectPropertyCollectionObject(IObjectInstance objectInstance, ClassProperty classProperty)
			: base(objectInstance, classProperty)
		{
		}

        public override object TranslateOutgoingValue(object value)
        {
            return value == null ? null : WrapObject(value);
        }
    }
}
