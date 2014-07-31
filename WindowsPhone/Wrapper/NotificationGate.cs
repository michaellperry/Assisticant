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
    class NotificationGate : IDisposable
    {
        private static ThreadLocal<int> _outboundCount = new ThreadLocal<int>();

        private NotificationGate()
        {
        }

        public static bool IsInbound
        {
            get { return _outboundCount.Value == 0; }
        }

        public static IDisposable BeginOutbound()
        {
            int outboundCount = _outboundCount.Value;
            _outboundCount.Value = outboundCount + 1;
            return new NotificationGate();
        }

        public void Dispose()
        {
            int outboundCount = _outboundCount.Value;
            _outboundCount.Value = outboundCount - 1;
        }
    }
}
