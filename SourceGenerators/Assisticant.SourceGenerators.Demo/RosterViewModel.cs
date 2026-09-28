using System.Collections.Generic;
using System.Linq;
using Assisticant.Fields;

namespace Assisticant.SourceGenerators.Demo
{
    // VIEWMODEL: a [Computed] method returning IEnumerable<PersonViewModel> is
    // backed by a live, incrementally-synchronized ObservableCollection<PersonViewModel>
    // instead of a scalar Computed<T> - see ComputedGenerator's "Collection mode"
    // doc comment. PersonViewModel items are recycled by identity (its Equals/
    // GetHashCode delegate to the wrapped Person - see PersonViewModel.cs), so an
    // item's own [Observable] IsSelected survives a recompute triggered by adding
    // or removing an unrelated Person from the Model.
    public partial class RosterViewModel
    {
        private readonly Roster _roster;

        public RosterViewModel(Roster roster)
        {
            _roster = roster;
        }

        [Computed] private IEnumerable<PersonViewModel> ComputePeople() =>
            _roster.People.Select(p => new PersonViewModel(p));
    }
}
