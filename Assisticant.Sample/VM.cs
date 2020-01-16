using Assisticant.Fields;
using System.Collections.Generic;

namespace Assisticant.Sample
{
	public class VM
	{
		private readonly Model _model;
		public VM(Model model)
		{
			_model = model;
		}

		public int Num
		{
			get => _model.Num;
			set => _model.Num = value;
		}

		public string NumStr => $"Current Model Num : {_model.Num}";

		public IEnumerable<int> AllNumbers => _model.AllNumbers;
		public IList<int> SelectedNumbers => _model.SelectedNumbers;
		public string SelectedNumbersText => string.Join(", ", _model.SelectedNumbers);
	}
}