﻿using System.Threading;
using Assisticant.Fields;

namespace Assisticant.UnitTest.MultithreadedData
{
    public class SourceThread
    {
        public const int MaxValue = 10000;

        private Thread _thread;
        private Observable<int> _value = new Observable<int>();

        public SourceThread()
        {
            _thread = new Thread(ThreadProc);
        }

        public void Start()
        {
            _thread.Start();
        }

        public void Join()
        {
            _thread.Join();
        }

        private void ThreadProc()
        {
            for (int i = 0; i <= MaxValue; i++)
            {
                Value = i;
            }
        }

        public int Value
        {
            get
            {
                lock (this)
                    return _value;
            }
            set
            {
                lock (this)
                	_value.Value = value;
            }
        }
    }
}
