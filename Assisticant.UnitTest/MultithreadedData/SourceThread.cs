using System.Threading;
using Assisticant.Fields;

namespace Assisticant.UnitTest.MultithreadedData
{
    public class SourceThread : AbstractThread
    {
        public const int MaxValue = 10000;

        private Observable<int> _value = new Observable<int>();

        protected override void ThreadProc()
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
