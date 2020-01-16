using Prism.Mvvm;
using System.Collections.Generic;

namespace Assisticant.Sample
{
	public class VMPrism : BindableBase
	{
		private readonly ModelPrism _model;
		public VMPrism(ModelPrism model)
		{
			_model = model;
			_model.PropertyChanged += (s, e) =>
			{
				switch (e.PropertyName)
				{
					case nameof(ModelPrism.SelectedNumbers):
						RaisePropertyChanged(nameof(SelectedNumbersText));
						break;
					default:
						break;
				}
			};
		}

		public IEnumerable<int> AllNumbers => _model.AllNumbers;
		public IEnumerable<int> SelectedNumbers => _model.SelectedNumbers;
		public string SelectedNumbersText => string.Join(", ", _model.SelectedNumbers);
	}
}