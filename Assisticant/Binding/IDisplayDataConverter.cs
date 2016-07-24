using System;

namespace Assisticant.Binding
{
	public interface IDisplayDataConverter<TDisplay, TData>
	{
		TDisplay ConvertOutput(TData data);
		TData ConvertInput(TDisplay display);
	}
}

