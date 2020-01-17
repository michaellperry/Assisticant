namespace Assisticant.Sample
{
	public class VMLocator : ViewModelLocatorBase
	{
		private readonly Model _model;

		public VMLocator(Model model)
		{
			_model = model;
		}

		public object VM => ViewModel(() => new VM(_model));
	}
}
