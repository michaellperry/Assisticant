# [Observable] / [Computed] / [NotifyPropertyChanged] generators (prototype)

Three Roslyn incremental source generators that remove the boilerplate around
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

## Layout

- `Assisticant.SourceGenerators/`
  - `ObservableGenerator.cs` - the Model-layer generator (`[Observable]`).
  - `ComputedGenerator.cs` - the ViewModel-layer generator (`[Computed]`).
  - `NotifyPropertyChangedGenerator.cs` - the cross-cutting generator
    (`[NotifyPropertyChanged]`) that the other two call into; see its doc
    comment for why it's a separate generator rather than something either of
    them emits directly.
  - `GeneratorSupport.cs` - helpers shared by all three (partial-type-chain
    validation - including the class-itself case `[NotifyPropertyChanged]`
    needs - namespace/indentation rendering, and the shared attribute-name
    constant the three generators use to recognize each other's attribute).
  - All three emit their own attribute via `RegisterPostInitializationOutput`,
    so trying this needs no changes to the main
    `Assisticant`/`Assisticant.Netstandard` projects.
- `Assisticant.SourceGenerators.Demo/` - a `net8.0` console app (needs
  `LangVersion` 13+ for partial properties; the project already sets
  `LangVersion=latest`) that references all three generators as analyzers and
  `Assisticant.Netstandard` as a library:
  - `Person.cs` - the Model.
  - `PersonViewModel.cs` - the ViewModel, constructor-injected with `Person`,
    marked `[NotifyPropertyChanged]`, and also carrying its own `IsSelected`
    (`[Observable]`) and `SelectionLabel` (`[Computed]`, depending on
    `IsSelected` rather than the injected Model) as the deliberate exception
    to the usual separation.
  - `Program.cs` - proves `PropertyChanged` fires exactly when something a
    property actually depends on changes - whether that dependency is the
    injected Model (`Greeting`, `IsAdult`) or the ViewModel's own state
    (`SelectionLabel` depending on `IsSelected`) - and not otherwise, with the
    raise order across a single mutation traced and asserted explicitly.
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

`[NotifyPropertyChanged]`:
- Remove `[NotifyPropertyChanged]` from `PersonViewModel` and see `Program.cs`
  fail to compile (`viewModel.PropertyChanged` no longer exists) - the
  attribute is what adds that member.
- Remove `partial` from `PersonViewModel` (with `[NotifyPropertyChanged]`
  still on it) → all three generators report their own diagnostic for the
  same mistake: `ASSISTICANT001`/`ASSISTICANT101`/`ASSISTICANT201`.

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

`[NotifyPropertyChanged]` is the one piece of this prototype that changes
what's possible on MAUI rather than just reducing boilerplate: it gives a
`[Computed]`-based ViewModel a real `INotifyPropertyChanged` implementation,
which MAUI's `{Binding}` requires for live updates and had no path to before
(there being no MAUI equivalent of `ForView.Wrap`). It's still additive,
though - `BindingManager`'s imperative pattern keeps working unchanged for
ViewModels that don't opt in.

## Known rough edges (prototype, not production-ready)

- The incremental-generator pipeline models don't implement Roslyn's
  recommended value-equality for optimal caching; this only costs rebuild
  performance, not correctness.
- Only `class`/`struct` nesting is handled; generics on the containing type
  are passed through untouched but not exercised by a test.
- `[Computed]`'s "strip a Compute/Get prefix" name derivation is a simple
  convention, not configurable beyond the explicit-name attribute argument.
- The partial-property style requires C# 13 (ships with recent .NET SDKs).
