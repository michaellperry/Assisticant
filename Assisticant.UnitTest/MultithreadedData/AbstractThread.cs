using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
#if NETFX_CORE
using Windows.Foundation;
using Windows.System.Threading;
#endif

namespace Assisticant.UnitTest.MultithreadedData
{
    public abstract class AbstractThread
    {
#if NETFX_CORE
        private IAsyncAction _asyncAction;
#else
        private Thread _thread;
#endif

        protected abstract void ThreadProc();

        public AbstractThread()
        {
#if !NETFX_CORE
            _thread = new Thread(() => ThreadProc());
#endif
        }

        public void Start()
        {
#if NETFX_CORE
            _asyncAction = ThreadPool.RunAsync(wi => ThreadProc());
#else
            _thread.Start();
#endif
        }

        public void Join()
        {
#if NETFX_CORE
            _asyncAction.AsTask().Wait();
#else
            _thread.Join();
#endif
        }
    }
}
