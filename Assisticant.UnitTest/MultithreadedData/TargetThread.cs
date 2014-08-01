using System;
using System.Linq;
using System.Threading;
using Assisticant.Fields;

namespace Assisticant.UnitTest.MultithreadedData
{
    public class TargetThread : AbstractThread
    {
        private SourceThread[] _sources;
        private Computed<int> _total;

        public TargetThread(SourceThread[] sources)
        {
            _sources = sources;
            _total = new Computed<int>(() => _sources.Sum(source => source.Value));
        }

        public int Total
        {
            get
            {
                lock (this)
                    return _total;
            }
        }

        protected override void ThreadProc()
        {
            for (int i = 0; i < SourceThread.MaxValue; i++)
            {
                int total = Total;
                if (total < 0) // This will never happen, but we need to ensure that the property get is not optimized out.
                    throw new InvalidOperationException();
            }
        }
    }
}
