/**********************************************************************
 * 
 * Update Controls .NET
 * Copyright 2010 Michael L Perry
 * MIT License
 * 
 * http://updatecontrols.net
 * http://updatecontrolslight.codeplex.com/
 * 
 **********************************************************************/

using System;
using System.Windows;
using System.Windows.Threading;

namespace Assisticant.XAML.Wrapper
{
    public interface IObjectInstance : IDisposable
    {
        object WrappedObject { get; }
        Tree Tree { get; }
        void Defer(Computed computed);
        ObjectProperty LookupProperty(Assisticant.XAML.Wrapper.ClassProperty classProperty);
        void SetValue(DependencyProperty dependencyProperty, object value);
        void ClearValue(DependencyProperty dependencyProperty);
        void UpdateNodes();
    }
}
