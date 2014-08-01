using System;
using System.Threading;
#if NETFX_CORE
using Windows.System.Threading;
#endif
using Assisticant.Fields;

namespace Assisticant.UnitTest
{
	public class SourceData
	{
		private Observable<int> _sourceProperty = new Observable<int>();
		private AutoResetEvent _continue = new AutoResetEvent(false);

		public int SourceProperty
		{
			get
			{
				int result = _sourceProperty;
#if NETFX_CORE
                var ignored = ThreadPool.RunAsync(wi =>
#else
				ThreadPool.QueueUserWorkItem(o =>
#endif
				{
					if (AfterGet != null)
						AfterGet();
					_continue.Set();
				});
				_continue.WaitOne();
				return result;
			}
			set { _sourceProperty.Value = value; }
		}

		public event Action AfterGet;
	}
}
