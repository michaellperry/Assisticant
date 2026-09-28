using Assisticant.Fields; // for [Computed] - Assisticant.Fields.ComputedAttribute; no Observable<T> is used here

namespace Assisticant.SourceGenerators.Demo
{
    // VIEWMODEL: mostly dependent variables derived from an injected Model - but
    // Model/ViewModel separation is a strong default here, not a hard rule.
    // IsSelected below is pure UI/interaction state (no Model would ever care
    // whether this row is selected), so it's declared directly as [Observable]
    // on the ViewModel. Dependency tracking doesn't care which class or layer
    // an Observable/Computed lives in - SelectionLabel (a [Computed] on this
    // same class) depends on IsSelected exactly the way Greeting depends on
    // the injected Model's Name.
    //
    // Both the type and each [Computed] method's containing type must be
    // 'partial' - remove it and the generator reports ASSISTICANT101 instead
    // of a confusing compiler error.
    //
    // [NotifyPropertyChanged] makes this ViewModel directly bindable with MAUI
    // XAML {Binding} - MAUI's binding engine (unlike WPF's ForView.Wrap proxy)
    // requires the bound object to implement INotifyPropertyChanged itself for
    // live updates. Only opt in on ViewModels actually bound this way. Both
    // [Observable] and [Computed] members participate: IsSelected raises
    // PropertyChanged directly when set; Greeting/IsAdult/SelectionLabel raise
    // it when their backing Computed<T> invalidates - regardless of whether
    // that's because the injected Model changed or because IsSelected did.
    [NotifyPropertyChanged]
    public partial class PersonViewModel
    {
        // The one constructor a class needs: it sets this type's only immutable
        // field, the injected Model. Nothing mutable is ever set through a
        // constructor parameter. IsSelected, being mutable, starts at its
        // default (false) and is set afterward, the same as any other
        // [Observable] property - see ObservableGenerator's doc comment.
        private readonly Person _person;

        public PersonViewModel(Person person)
        {
            _person = person;
        }

        [Observable] public partial bool IsSelected { get; set; }

        // Property name is derived by stripping the "Compute" prefix: "Greeting".
        [Computed] private string ComputeGreeting() => $"Hello, {_person.Name}!";

        // Property name is derived by stripping the "Get" prefix: "IsAdult".
        [Computed] private bool GetIsAdult() => _person.Age >= 18;

        // Depends on IsSelected - a [Computed] property on this ViewModel
        // depending on an [Observable] property on the same ViewModel, not on
        // the injected Model at all.
        [Computed] private string ComputeSelectionLabel() => IsSelected ? "Selected" : "Not selected";

        // ClearSelectionCommand's CanExecute is CanClearSelection below, found by
        // naming convention (no attribute on it). It's wrapped in a Computed<bool>,
        // so CanExecuteChanged fires automatically whenever IsSelected changes -
        // whether that's from code (Program.cs sets it directly) or from executing
        // this very command - with no manual "RefreshCanExecutes()" anywhere.
        [Command] private void ClearSelection() => IsSelected = false;
        private bool CanClearSelection() => IsSelected;

        // Required for RosterViewModel's [Computed] collection property
        // (see ComputedGenerator's "Collection mode" doc comment): RecycleBin<T>
        // matches old-vs-new instances by Equals/GetHashCode, and a fresh
        // PersonViewModel is constructed on every recompute (`.Select(p => new
        // PersonViewModel(p))`), so without delegating identity to the wrapped
        // Model, recycling would silently never match anything - every recompute
        // would produce a brand-new PersonViewModel per Person, discarding
        // IsSelected each time. This is exactly the footgun RecycleBin.cs's own
        // doc comment warns about, and exactly what a follow-up generator could
        // auto-generate from this class's one constructor parameter.
        public override bool Equals(object? obj) => obj is PersonViewModel other && ReferenceEquals(_person, other._person);
        public override int GetHashCode() => _person.GetHashCode();
    }
}
