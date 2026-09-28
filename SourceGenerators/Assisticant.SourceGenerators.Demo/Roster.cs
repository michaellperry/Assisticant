using Assisticant.Collections;

namespace Assisticant.SourceGenerators.Demo
{
    // MODEL: a plain ObservableList<Person> - independent state. No [Observable]
    // needed here: ObservableList<T> already tracks itself (every read/write goes
    // through its own Observable sentry - see Assisticant/Collections/ObservableList.cs),
    // so there's no scalar to hide the way [Observable] hides Observable<T> for a
    // plain property. A follow-up generator could still recognize this shape and
    // generate the field/get-only-property pair, but that's not what's prototyped
    // here - see RosterViewModel.cs for the [Computed] collection-mode piece.
    public class Roster
    {
        public ObservableList<Person> People { get; } = new ObservableList<Person>();
    }
}
