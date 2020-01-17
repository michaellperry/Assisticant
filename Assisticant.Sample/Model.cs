using Assisticant.Collections;
using Assisticant.Fields;
using System.Collections.Generic;

namespace Assisticant.Sample
{
	public class Model
	{
		public Model()
		{
			AllNumbers = new[] { 1, 2, 3 };
			_selectedNumbers = new ObservableList<int>(AllNumbers);
		}

		private Observable<int> _num = new Observable<int>(0);
		public int Num
		{
			get => _num.Value;
			set => _num.Value = value;
		}

		public int[] AllNumbers { get; }

		private ObservableList<int> _selectedNumbers;
		public IList<int> SelectedNumbers => _selectedNumbers;
	}
}