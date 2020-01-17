using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Assisticant.Sample
{
    public class ModelPrism : BindableBase
	{
		public ModelPrism()
		{
			AllNumbers = new[] { 1, 2, 3 };
			_selectedNumbers = new ObservableCollection<int>(AllNumbers);
			_selectedNumbers.CollectionChanged += (s, e) => RaisePropertyChanged(nameof(SelectedNumbers));
		}

		public int[] AllNumbers { get; }

		private ObservableCollection<int> _selectedNumbers;
		public IEnumerable<int> SelectedNumbers => _selectedNumbers;
	}
}