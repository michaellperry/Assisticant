# [Observable] / [Computed] / [NotifyPropertyChanged] / [Command] generators (prototype)

Four Roslyn incremental source generators that remove the boilerplate around
Assisticant's dependency-tracking primitives, without ever exposing
`Observable<T>` or `Computed<T>` in user code - while defaulting toward
Assisticant's separation of independent and dependent variables, without
hard-enforcing it.

## The design these generators default toward

Assisticant's fundamental principle is separating independent variables from
dependent variables across three tiers, each injected into the next:

```
View  <--DataContext/ForView.Wrap--  ViewModel  <--constructor injection--  Model
```

- **Model**: holds `Observable<T>` state - the independent variables. Use
  `[Observable]` here.
- **ViewModel**: holds `Computed<T>` state - derived from one or more injected
  Models. Use `[Computed]` here.
- **View**: binds to the ViewModel, same as always.

This is a strong default, not an axiom the generators enforce. Putting
`[Observable]` properties on a class named `...ViewModel` collapses the Model
into the ViewModel and mixes independent and dependent variables - usually
worth avoiding - but it can be broken with little consequence when there's a
good reason to: pure UI/interaction state that no Model would ever care about
(e.g. "is this row selected") reasonably lives directly on the ViewModel.
Dependency tracking doesn't care which class or layer an `Observable<T>` or
`Computed<T>` lives in - a `[Computed]` property can depend on an
`[Observable]` property on its *own* class exactly as it depends on an
injected Model's property, with no generator changes needed for that to work.
The demo project shows both: `Person.cs` (a Model) and `PersonViewModel.cs`
(a ViewModel, constructor-injected with `Person`, that *also* carries its own
`IsSelected` and a `SelectionLabel` computed from it).

**Constructors set immutable fields only.** A constructor exists solely to
establish a type's *immutable* (`readonly`) fields - not to push initial
values into mutable, `[Observable]`-backed properties. Since the set of
immutable fields a type has is fixed, a type needs exactly one constructor:

- `Person` (a Model) has no immutable fields, so it has **no constructor at
  all**. Initial mutable state is set with an object initializer at the call
  site: `new Person { Name = "Alice", Age = 30 }`.
- `PersonViewModel` (a ViewModel) has exactly one immutable field - the
  injected `Person` - so it has exactly one constructor, taking exactly that:
  `public PersonViewModel(Person person)`. This is the sanctioned use of a
  constructor parameter: wiring an immutable dependency, never seeding a
  mutable property.

## [Observable] - Model layer

```csharp
public partial class Person
{
    [Observable] public partial string Name { get; set; }
    [Observable] public partial int Age { get; set; }
}
```

generates a hidden backing field plus the implementing declaration:

```csharp
partial class Person
{
    private readonly Observable<string> __nameField = new Observable<string>();
    public partial string Name
    {
        get => __nameField.Value;
        set => __nameField.Value = value;
    }
    // ...same for Age
}
```

used as:

```csharp
var person = new Person { Name = "Alice", Age = 30 };
```

This is C# 13's partial-properties feature (the same mechanism
`CommunityToolkit.Mvvm` 8.4 moved to for the same reason: nobody has to see
the backing store). `public partial string Name { get; set; }` - with no
accessor bodies and no initializer - is a *declaration*, not an
implementation; the generator supplies the other half. An initializer isn't
legal there either way (C# only allows one on an auto-implemented property),
which lines up with the constructor doctrine above: initial state is always
set from outside, via an object initializer or a plain property assignment,
never baked into the declaration or threaded through a constructor parameter.

`[Observable]` only supports this one style - an earlier prototype also
supported wrapping a hand-declared `Observable<T>` field, but that was removed
as unnecessary complexity once the partial-property style covers every case
without ever naming `Observable<T>`.

## [Computed] - ViewModel layer

```csharp
public partial class PersonViewModel
{
    private readonly Person _person; // the one immutable field

    public PersonViewModel(Person person) => _person = person;

    [Computed] private string ComputeGreeting() => $"Hello, {_person.Name}!";
    [Computed] private bool GetIsAdult() => _person.Age >= 18;
}
```

generates:

```csharp
partial class PersonViewModel
{
    private Computed<string>? __greetingField;
    public string Greeting => (__greetingField ??= new Computed<string>(ComputeGreeting)).Value;

    private Computed<bool>? __isAdultField;
    public bool IsAdult => (__isAdultField ??= new Computed<bool>(GetIsAdult)).Value;
}
```

`[Computed]` goes on a parameterless, non-void method - it becomes the
`Func<T>` passed to `Computed<T>`'s constructor. Because a property can't
share its name with the method that computes it, the property name is derived
by stripping a leading `Compute` or `Get` prefix (or given explicitly via
`[Computed("PropertyName")]`). The backing `Computed<T>` is constructed
*lazily*, on first property access, rather than via a field initializer or a
generated constructor: C# field initializers can't reference instance methods
(`this` isn't available yet - CS0236), and a generated constructor would
conflict with "exactly one constructor per type."

Only the containing type needs to be `partial` for `[Computed]` - the method
itself is an ordinary private method, not split across declarations the way
`[Observable]`'s partial property is.

### [Computed] collection mode - a live, recycled `ObservableCollection<T>`

When the method returns `IEnumerable<T>` (or anything implementing it for
exactly one `T` - excluding `string`, mirroring the reflection-based
`Metas/MemberMeta.cs`'s existing exclusion), `[Computed]` generates a
different shape entirely:

```csharp
[Computed] private IEnumerable<PersonViewModel> ComputePeople() =>
    _roster.People.Select(p => new PersonViewModel(p));
```

generates:

```csharp
private ObservableCollection<PersonViewModel>? __peopleField;
private Computed? __peopleSentryField;
public ObservableCollection<PersonViewModel> People
{
    get
    {
        if (__peopleField == null)
        {
            __peopleField = new ObservableCollection<PersonViewModel>();
            __peopleSentryField = new Computed(() =>
                CollectionSynchronizer<PersonViewModel>.Synchronize(__peopleField, ComputePeople()));
            __peopleSentryField.Invalidated += () =>
                UpdateScheduler.ScheduleUpdate(() => __peopleSentryField!.OnGet());
        }
        __peopleSentryField!.OnGet();
        return __peopleField;
    }
}
```

`People`'s identity never changes across recomputes - only its *contents* do,
via `CollectionSynchronizer<T>` (emitted the same way `GeneratedCommand` is),
which applies the newly computed sequence with minimal Add/Remove/Move
mutations instead of a full Clear+rebuild, using a `RecycleBin<T>` to match
old-vs-new items by identity - exactly the algorithm Assisticant's WPF proxy
already uses (`Metas/ListSlot.cs`/`Metas/CollectionItem.cs`), generalized
from an untyped `ObservableCollection<object>` to a strongly-typed one.

**Why the target is a real `ObservableCollection<T>`, not a custom
`INotifyCollectionChanged` implementation, and not `ComputedList<T>`
itself** (which implements neither): confirmed independent of anything in
this repo, [dotnet/maui#29284](https://github.com/dotnet/maui/issues/29284)
(open as of this writing) reports that MAUI's `CollectionView` does *not*
reliably subscribe to `INotifyCollectionChanged` on an arbitrary custom
collection class - only when it's actually an `ObservableCollection<T>` (or
derives from one). Implementing the interface ourselves on our own type
wouldn't reliably work as something bound to `ItemsSource`.

**Why identity preservation matters, concretely:** `RecycleBin<T>.Extract`
matches by `Equals`/`GetHashCode`, and a fresh `PersonViewModel` is
constructed on *every* recompute (`.Select(p => new PersonViewModel(p))`).
Without `PersonViewModel` delegating `Equals`/`GetHashCode` to its wrapped
`Person` (see `PersonViewModel.cs`), recycling would silently never match
anything - every recompute would produce all-new `PersonViewModel` instances,
discarding each one's own `IsSelected` (or any other ViewModel-local state)
every time. This is exactly the footgun `RecycleBin.cs`'s own doc comment
warns about ("It is imperative that you properly implement `GetHashCode` and
`Equals`"), and a natural candidate for a follow-up generator: since a
ViewModel's one constructor (per the constructor discipline above) already
names its injected Model, that's enough information to generate this
override automatically. Not implemented here - hand-written in the demo.

**Why `[NotifyPropertyChanged]` doesn't apply to collection-mode properties:**
there's nothing for it to do. `People`'s reference never changes, so a MAUI
`{Binding}` only needs to read it once; every subsequent update flows through
`CollectionChanged`, which `ObservableCollection<T>` already raises itself.

### [NotifyPropertyChanged] - binding a ViewModel (or Model) directly (e.g. on MAUI)

```csharp
[NotifyPropertyChanged]
public partial class PersonViewModel
{
    private readonly Person _person;
    public PersonViewModel(Person person) => _person = person;

    [Observable] public partial bool IsSelected { get; set; }

    [Computed] private string ComputeGreeting() => $"Hello, {_person.Name}!";
    [Computed] private string ComputeSelectionLabel() => IsSelected ? "Selected" : "Not selected";
}
```

adds an `INotifyPropertyChanged` implementation, in its own generated file:

```csharp
partial class PersonViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private void __RaisePropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
```

with `[Observable]`'s and `[Computed]`'s own generated files each just
*calling* that helper - `[Observable]`'s setter, when the value actually
changes:

```csharp
public partial bool IsSelected
{
    get => __isSelectedField.Value;
    set
    {
        if (!EqualityComparer<bool>.Default.Equals(__isSelectedField.Value, value))
        {
            __isSelectedField.Value = value;
            UpdateScheduler.ScheduleUpdate(() => __RaisePropertyChanged("IsSelected"));
        }
    }
}
```

and `[Computed]`'s backing field, when it invalidates - whether that's because
the injected Model changed or because `IsSelected` (on this same class) did:

```csharp
private Computed<string>? __selectionLabelField;
public string SelectionLabel => (__selectionLabelField ??= __CreateSelectionLabelField()).Value;
private Computed<string> __CreateSelectionLabelField()
{
    var computed = new Computed<string>(ComputeSelectionLabel);
    computed.Invalidated += () => UpdateScheduler.ScheduleUpdate(() => __RaisePropertyChanged("SelectionLabel"));
    return computed;
}
```

**Why a third, separate generator owns the event and helper.** A class can
combine `[Observable]` properties and `[Computed]` methods (see the design
note above) - if `ObservableGenerator` and `ComputedGenerator` each
independently emitted the `PropertyChanged` event and `__RaisePropertyChanged`
helper whenever they saw `[NotifyPropertyChanged]`, a class using both would
get two conflicting definitions of the same members. `NotifyPropertyChangedGenerator`
alone owns that scaffolding; the other two only ever *call*
`__RaisePropertyChanged`, never declare it.

**Why this exists at all.** MAUI's `{Binding}` engine - confirmed against
Microsoft's own docs, independent of anything in this repo - has no
`ICustomTypeDescriptor`-style proxy layer the way WPF's `ForView.Wrap` does. A
MAUI ViewModel must implement `INotifyPropertyChanged` *itself* to get live
(`OneWay`/`TwoWay`) `{Binding}` updates; without it, a binding silently
behaves as `OneTime` no matter what mode you asked for. `[NotifyPropertyChanged]`
is what makes an `[Observable]`/`[Computed]`-based class usable that way,
without hand-writing the `PropertyChanged` plumbing.

**Why it's opt-in rather than automatic.** A `[Computed]`-based ViewModel
isn't MAUI-exclusive under this design - on WPF it's wrapped by
`ForView.Wrap`'s proxy (which already raises its own `PropertyChanged`
externally), and it's often read directly in unit tests - so every class
would otherwise pay for an event field and a subscription per member whether
or not anything ever binds to it directly.

**Why every raise is deferred through `UpdateScheduler`, never inline.**
`[Computed]`'s case genuinely needs it: `Observable<T>.Value`'s setter raises
invalidation (which cascades into a dependent `Computed<T>`'s `Invalidated`)
*before* storing the new value, and `UpdateScheduler.Initialize`'s contract
warns that some UI dispatchers run queued work inline when already on the UI
thread - the common case for a MAUI app (a button tap already runs on the UI
thread). Deferring, the same way `ComputedSubscription` and the WPF proxy's
`MemberSlot` already do, guarantees the eventual re-read sees the new value
regardless of what the host binding infrastructure's own dispatch does.
`[Observable]`'s own raise isn't actually at risk of that specific race (it's
the next statement after the value is already stored, not a reaction to
`Invalidated`), but it defers anyway, for one uniform rule rather than two
subtly different ones to reason about. One visible consequence: setting
`IsSelected` enqueues `SelectionLabel`'s raise *before* its own, because
invalidation cascades synchronously into the dependent `Computed<T>` partway
through the `IsSelected` setter, before that setter's own trailing
"schedule my raise" statement runs - see `Program.cs` for this traced through
a live assertion.

### [Command] - ICommand for a view's Command/CommandParameter binding

```csharp
[Command] private void ClearSelection() => IsSelected = false;
private bool CanClearSelection() => IsSelected;
```

generates:

```csharp
private GeneratedCommand? __clearSelectionCommandField;
public ICommand ClearSelectionCommand =>
    __clearSelectionCommandField ??= new GeneratedCommand(ClearSelection, CanClearSelection);
```

`[Command]` goes on a parameterless, void-returning method - it becomes
`GeneratedCommand`'s `Action` (its `Execute`). The property name is the
method's name plus `"Command"` (or given explicitly via
`[Command("PropertyName")]`) - unlike `[Computed]`, there's no name-collision
problem to work around here, since the method and the generated property
never share a name.

`CanClearSelection` is found purely by naming convention - a parameterless,
bool-returning method named `"Can"` + the command method's name, on the same
class - matching the convention Assisticant's existing reflection-based
`Metas/CommandMeta.cs` already uses. It isn't itself attributed; if no such
method exists, the command is always executable (`GeneratedCommand` is
constructed with `null` for `canExecute`).

**This is where Assisticant's dependency tracking beats MAUI's own commanding
model, not just matches it.** MAUI's own docs are explicit: *"Unlike some UI
frameworks (such as WPF), .NET MAUI does not automatically detect when the
return value of `CanExecute` might change. You must manually raise the
`CanExecuteChanged` event (or call `ChangeCanExecute()` on the `Command`
class)"* - their own sample hand-writes a `RefreshCanExecutes()` method called
after every mutation that could affect any command's enabled state.
`GeneratedCommand` wraps the `CanExecute` predicate in a `Computed<bool>` and
raises `CanExecuteChanged` (deferred through `UpdateScheduler`, same
reasoning as `[Computed]`'s `PropertyChanged` raise) automatically whenever
anything that predicate reads changes - the dependency is *discovered*, not
declared, so there's no `RefreshCanExecutes()`-style bookkeeping to maintain
at every call site that might affect it, and no `[NotifyCanExecuteChangedFor]`-style
attribute listing the way `CommunityToolkit.Mvvm` needs either.

**Scope of this prototype:** only parameterless commands are supported - not
MAUI's `Command<T>`/`CommandParameter` pattern. A parameter supplied by the
platform at `Execute`/`CanExecute` call time isn't a value Assisticant's
dependency graph can track the way `Computed<T>` tracks a captured
no-argument delegate's reads (there's no single cached value to invalidate
when the caller can pass a different parameter on every call), so it doesn't
fit this generator's model without more design work. `ASSISTICANT302` is
intentional, not a missing case to route around.

## Layout

- `Assisticant.SourceGenerators/`
  - `ObservableGenerator.cs` - the Model-layer generator (`[Observable]`).
  - `ComputedGenerator.cs` - the ViewModel-layer generator (`[Computed]`),
    including collection mode; also emits the `CollectionSynchronizer<T>`
    runtime type collection-mode properties are generated in terms of.
  - `NotifyPropertyChangedGenerator.cs` - the cross-cutting generator
    (`[NotifyPropertyChanged]`) that `ObservableGenerator`/`ComputedGenerator`
    call into; see its doc comment for why it's a separate generator rather
    than something either of them emits directly.
  - `CommandGenerator.cs` - the ICommand generator (`[Command]`); also emits
    the `GeneratedCommand` runtime type it generates properties in terms of.
  - `GeneratorSupport.cs` - helpers shared by all four (partial-type-chain
    validation - including the class-itself case `[NotifyPropertyChanged]`
    needs - namespace/indentation rendering, and the shared attribute-name
    constant used to recognize `[NotifyPropertyChanged]`).
  - All four emit their own attribute (and, for `[Command]`/`[Computed]`
    collection mode, the `GeneratedCommand`/`CollectionSynchronizer<T>`
    runtime types) via `RegisterPostInitializationOutput`, so trying this
    needs no changes to the main `Assisticant`/`Assisticant.Netstandard`
    projects.
- `Assisticant.SourceGenerators.Demo/` - a `net8.0` console app (needs
  `LangVersion` 13+ for partial properties; the project already sets
  `LangVersion=latest`) that references all four generators as analyzers and
  `Assisticant.Netstandard` as a library:
  - `Person.cs` - the Model.
  - `PersonViewModel.cs` - the ViewModel, constructor-injected with `Person`,
    marked `[NotifyPropertyChanged]`, carrying its own `IsSelected`
    (`[Observable]`) and `SelectionLabel` (`[Computed]`, depending on
    `IsSelected` rather than the injected Model) as the deliberate exception
    to the usual separation, plus `ClearSelectionCommand` (`[Command]`,
    guarded by `CanClearSelection`) and an `Equals`/`GetHashCode` override
    delegating to the wrapped `Person` (required for collection-mode
    recycling - see below).
  - `Roster.cs` - a Model holding a plain `ObservableList<Person>`.
  - `RosterViewModel.cs` - a ViewModel whose `People` is `[Computed]` in
    collection mode: `IEnumerable<PersonViewModel>` projected from the
    injected `Roster`'s `ObservableList<Person>`.
  - `Program.cs` - proves `PropertyChanged` fires exactly when something a
    property actually depends on changes - whether that dependency is the
    injected Model (`Greeting`, `IsAdult`) or the ViewModel's own state
    (`SelectionLabel` depending on `IsSelected`) - and not otherwise, with the
    raise order across a single mutation traced and asserted explicitly; that
    `ClearSelectionCommand.CanExecuteChanged` fires automatically both when
    `IsSelected` is set directly and when executing the command itself
    changes it, with no bookkeeping code anywhere; and that adding/removing
    `Person`s in the Model produces incremental `CollectionChanged` events
    (never a `Reset`) on `RosterViewModel.People`, while a captured item's
    `PersonViewModel` instance - and its own `IsSelected` - survives the
    recompute unchanged (recycled, not rebuilt).
- `NuGet.Config` - scoped to this subtree only; it resets package sources to
  nuget.org because the repo's machine-wide NuGet config points at an internal,
  VPN-only feed that isn't reachable in every environment. It doesn't affect
  restoring the rest of the solution.

## Try it

```
dotnet run --project SourceGenerators/Assisticant.SourceGenerators.Demo
```

Diagnostics to try breaking on purpose:

In `Person.cs` (Model / `[Observable]`):
- Remove `partial` from the type or a property → `ASSISTICANT001` /
  `ASSISTICANT002` instead of a confusing "duplicate member" error.
- Give a partial property an accessor body or an initializer →
  `ASSISTICANT003` (plus the compiler's own CS9249, since the generator
  correctly declines to emit a second, conflicting implementation).
- Remove the `set` accessor from a partial property → `ASSISTICANT004`.

In `PersonViewModel.cs` (ViewModel / `[Computed]`):
- Remove `partial` from the type → `ASSISTICANT101`.
- Give a `[Computed]` method a parameter → `ASSISTICANT102`.
- Make a `[Computed]` method return `void` → `ASSISTICANT103`.
- Name a `[Computed]` method without a `Compute`/`Get` prefix and no explicit
  name → `ASSISTICANT104`.

In `RosterViewModel.cs` (`[Computed]` collection mode) and `PersonViewModel.cs`:
- Remove the `Equals`/`GetHashCode` override from `PersonViewModel` → verified
  live: the demo still runs and still ends up with the right two people, but
  `CollectionChanged` reports `Remove, Remove, Add, Add` instead of `Remove,
  Add` - both old items were discarded and both new ones rebuilt from
  scratch, including Carol, who didn't even change. Recycling silently
  stopped matching *anything* (default reference equality never matches a
  freshly-constructed prototype), so `carolViewModel` and
  `carolViewModelAfter` are no longer the same instance and `IsSelected` is
  lost. No diagnostic catches this; it's the exact footgun `RecycleBin.cs`'s
  doc comment warns about (see the rough edges below).

`[NotifyPropertyChanged]`:
- Remove `[NotifyPropertyChanged]` from `PersonViewModel` and see `Program.cs`
  fail to compile (`viewModel.PropertyChanged` no longer exists) - the
  attribute is what adds that member.
- Remove `partial` from `PersonViewModel` (with `[NotifyPropertyChanged]`
  still on it) → all relevant generators report their own diagnostic for the
  same mistake: `ASSISTICANT001`/`ASSISTICANT101`/`ASSISTICANT201`/`ASSISTICANT301`.

`[Command]`:
- Give `ClearSelection` a parameter → `ASSISTICANT302` (this prototype's
  scope boundary, not a bug).
- Make `ClearSelection` return a value instead of `void` → `ASSISTICANT303`.
- Rename `CanClearSelection` to anything else → `ClearSelectionCommand` is
  still generated, just always executable (`CanExecute` returns `true`
  unconditionally) - no diagnostic, since an unguarded command is valid.

Set `<EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>` (already on
in the demo project) and look under `SourceGenerators/Generated/` after a build
to read exactly what got generated.

## What this does and doesn't prove

`[Observable]`/`[Computed]` alone only remove the boilerplate around
declaring the storage for a tracked property; they do **not** remove the
runtime reflection in `Metas/TypeMeta.cs` and friends that WPF's
`ICustomTypeDescriptor` bridge uses to discover a view model's members - a
separate, larger follow-on. Because the generated members are ordinary CLR
properties, the existing reflection-based `TypeMeta` scan picks them up
unmodified, so WPF bindings work against `[Observable]`/`[Computed]` Models
and ViewModels with no other changes.

`[NotifyPropertyChanged]`, `[Command]`, and `[Computed]` collection mode are
the three pieces of this prototype that change what's possible on MAUI rather
than just reducing boilerplate: `[NotifyPropertyChanged]` gives a
`[Computed]`-based ViewModel a real `INotifyPropertyChanged` implementation,
which MAUI's `{Binding}` requires for live updates and had no path to before
(there being no MAUI equivalent of `ForView.Wrap`); `[Command]` gives it a
`Button.Command`-typed member whose `CanExecuteChanged` fires itself, which -
per MAUI's own docs - its built-in `Command` class does not do on its own;
collection mode gives it a real `ObservableCollection<T>` that a
`CollectionView.ItemsSource` binding can actually rely on for incremental
updates, which `ComputedList<T>` alone cannot provide (see its section above).
All three are still additive, though - `BindingManager`'s imperative pattern
keeps working unchanged for ViewModels that don't opt in.

## Known rough edges (prototype, not production-ready)

- The incremental-generator pipeline models don't implement Roslyn's
  recommended value-equality for optimal caching; this only costs rebuild
  performance, not correctness.
- Only `class`/`struct` nesting is handled; generics on the containing type
  are passed through untouched but not exercised by a test.
- `[Computed]`'s "strip a Compute/Get prefix" name derivation is a simple
  convention, not configurable beyond the explicit-name attribute argument.
- The partial-property style requires C# 13 (ships with recent .NET SDKs).
- `[Command]` doesn't support MAUI's `Command<T>`/`CommandParameter` pattern
  (see its section above) or batching multiple property changes from one
  `Execute` into a single UI update pass the way WPF's proxy does via
  `UpdateScheduler.Begin()`/`End()`.
- `[Computed]` collection mode has no diagnostic for the single biggest
  footgun in using it: a recycled item type (like `PersonViewModel`) that
  doesn't override `Equals`/`GetHashCode` to delegate to its wrapped Model
  silently loses all recycling, with no compiler error - verified above. A
  Roslyn analyzer could plausibly catch "this type is only ever constructed
  inside a `[Computed]` collection projection and never overrides `Equals`,"
  but that's meaningfully more work than this prototype does today; more
  realistically, a follow-up generator that auto-generates the override from
  the ViewModel's one constructor (as suggested above) removes the footgun
  entirely instead of just detecting it.
- `[Computed]` collection mode only recognizes `Model`/`ViewModel` shape by
  return type (`IEnumerable<T>`); it doesn't validate that `T` is sensible to
  recycle (e.g. it would "work," uselessly, for `IEnumerable<int>`, just
  without any benefit over a plain list, since primitives have no
  ViewModel-local state to preserve and default `Equals` is already
  value-based for them).
- Only `List`/`IEnumerable`-shaped collections are covered - no
  `ComputedDictionary<TKey,TValue>` equivalent, and no generator support yet
  for a Model's own collection properties (`ObservableList<T>` is still
  hand-written in `Roster.cs`, per suggestion #1 from the design discussion
  that preceded this prototype).
