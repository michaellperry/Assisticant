---
name: assisticant-source-generators
description: Captures the opinions, feature set, and platform support of Assisticant's [Observable]/[Computed]/[NotifyPropertyChanged]/[Command] source-generator prototype (SourceGenerators/), so an agent can decide which attribute or pattern to reach for on a given platform and desired binding behavior. Use when writing or reviewing an Assisticant Model or ViewModel class; deciding whether a property, collection, or command needs [Observable], [Computed] (scalar or collection mode), [NotifyPropertyChanged], or [Command]; choosing between ObservableList<T>/ComputedList<T> and a generator-based collection; asking whether a ViewModel needs INotifyPropertyChanged or ICommand for WPF, UWP, Android, iOS, or MAUI specifically; or reviewing a recycled ViewModel's Equals/GetHashCode correctness.
---

# Assisticant source generators

**Status:** prototype, not yet merged or packaged. Lives in `SourceGenerators/`
alongside the main solution (a separate `netstandard2.0` analyzer project plus
a runnable demo), on branch `feature/source-generators-prototype`
([PR #49](https://github.com/michaellperry/Assisticant/pull/49)). Trying it
needs no changes to `Assisticant`/`Assisticant.Netstandard` - every attribute,
and the two runtime helper types (`GeneratedCommand`, `CollectionSynchronizer<T>`),
are emitted by the generators themselves. `SourceGenerators/README.md` is the
exhaustive, always-current reference with full generated-code listings for
every mode; this skill is the distilled decision layer on top of it - read the
README when you need the generated code itself, not just the choice of attribute.

## The feature set, in one table

| Attribute | Applies to | Layer | Backed by | Generates |
|---|---|---|---|---|
| `[Observable]` | a partial property, no body, no initializer | Model | `Observable<T>` | a tracked scalar property |
| `[Computed]` (scalar) | a parameterless method returning a scalar | ViewModel | `Computed<T>` | a read-only tracked property |
| `[Computed]` (collection mode) | a parameterless method returning `IEnumerable<T>` (not `string`) | ViewModel | `Computed` + `RecycleBin<T>` + `CollectionSynchronizer<T>` | a live `ObservableCollection<T>`, synchronized (not replaced) each recompute |
| `[NotifyPropertyChanged]` | a class | either | (cross-cutting; owns nothing else does) | an `INotifyPropertyChanged` implementation that `[Observable]`/scalar `[Computed]` call into |
| `[Command]` | a parameterless, void-returning method | ViewModel | `Computed<bool>` for `CanExecute` | an `ICommand`-typed property (`GeneratedCommand`) |

All four are independent generators that only agree on shared attribute-name
strings (see `GeneratorSupport.cs`); a class can combine any subset of them.

## Opinions this design holds, and how strongly to hold them

1. **Model holds `Observable<T>`; ViewModel holds `Computed<T>`, derived from
   one or more Models injected through the constructor** - the same three-tier
   chain (View ← ViewModel ← Model) as View ← DataContext ← ViewModel. This is
   a **strong default, not an axiom** - the user who owns this design said so
   explicitly: it "does not rise to the level of an axiom... it can be broken
   with little consequence." A ViewModel legitimately carries its own
   `[Observable]` state for things no Model would ever represent (e.g. "is
   this row selected" - see `PersonViewModel.IsSelected` in the demo).
   Dependency tracking doesn't care which class or layer an `Observable`/
   `Computed` lives in; don't flag a ViewModel-local `[Observable]` property
   as a mistake by default - check whether it's genuinely UI/interaction
   state first.
2. **Constructors set immutable fields only, never seed a mutable property.**
   A class's constructor exists solely to wire up its `readonly` fields (most
   often an injected Model). Since that set is fixed, a class needs **at most
   one constructor**. A Model with no immutable fields (the common case) gets
   **no constructor at all** - initial state is set with an object initializer
   at the call site (`new Person { Name = "Alice" }`), never a constructor
   parameter. Don't add a constructor overload "for convenience," and don't
   thread an initial value through a constructor parameter into an
   `[Observable]` property.
3. **`[NotifyPropertyChanged]` is opt-in per class, never automatic.** A
   `[Computed]`-based ViewModel isn't MAUI-exclusive: on WPF/UWP it's wrapped
   by `ForView.Wrap`'s reflection proxy (which raises its own
   `PropertyChanged` externally, making the class's own implementation dead
   weight), and it's often read directly in unit tests. Add it only when the
   class is actually bound directly (typically: MAUI).
4. **Every generated notification defers through `UpdateScheduler`, never
   raises inline** - `PropertyChanged`, `CanExecuteChanged`, and collection
   `Add`/`Remove`/`Move` alike. `[Computed]`'s case is a genuine correctness
   requirement (`Observable<T>.Value`'s setter invalidates dependents *before*
   storing the new value, and some UI dispatchers run queued work inline when
   already on the UI thread); `[Observable]`'s own raise isn't strictly at
   risk of that race, but defers anyway so there is exactly one rule to reason
   about, not two subtly different ones depending on which generator you're
   reading.
5. **Collection-mode `[Computed]` targets a real `ObservableCollection<T>`,
   never a custom `INotifyCollectionChanged` implementation and never
   `ComputedList<T>` directly.** Confirmed independent of anything in this
   repo: [dotnet/maui#29284](https://github.com/dotnet/maui/issues/29284)
   (open) reports that MAUI's `CollectionView` does not reliably subscribe to
   `INotifyCollectionChanged` on an arbitrary custom collection class - only
   on `ObservableCollection<T>` itself. Don't propose a custom
   `INotifyCollectionChanged` wrapper as "cleaner" - it plausibly won't work
   on the platform this feature exists for.
6. **A recycled ViewModel type must override `Equals`/`GetHashCode`,
   delegating to its wrapped Model.** This is not automated yet - it is the
   user's responsibility today, and getting it wrong doesn't error, it
   silently breaks (see the footgun section below). Always check for this
   override when reviewing a type used inside a `[Computed]` collection
   projection.
7. **Naming conventions are load-bearing, not cosmetic.** `[Computed]`
   derives its property name by stripping a leading `Compute`/`Get` prefix
   from the method name (there's no other way to avoid a member-name
   collision); `[Command]` appends `"Command"` to the method name and finds
   its `CanExecute` predicate by looking for a method literally named
   `"Can" + methodName` on the same class - unattributed, matching
   Assisticant's existing reflection-based `Metas/CommandMeta.cs` convention.
   Both accept an explicit name via the attribute's constructor argument
   (`[Computed("PropertyName")]`, `[Command("PropertyName")]`) when the
   convention doesn't fit.

## Deciding what to use

| You want... | Reach for... |
|---|---|
| A Model property holding real, independent state | `[Observable] public partial T Name { get; set; }` |
| A ViewModel property derived from an injected Model (or from another property on the same class) | `[Computed] private T ComputeName() => ...;` |
| A ViewModel property that's a filtered/projected list of per-Model ViewModels, with per-item ViewModel state (e.g. `IsSelected`) preserved across recomputes | `[Computed] private IEnumerable<TViewModel> ComputeName() => models.Select(m => new TViewModel(m));` (collection mode) - and don't forget `Equals`/`GetHashCode` on `TViewModel` (see the footgun below) |
| A class bindable via MAUI (or any) XAML `{Binding}` with live updates | Add `[NotifyPropertyChanged]` to the class |
| A `Button.Command`/similar binding target, with automatic enable/disable | `[Command] private void DoThing() => ...;` plus an optional `private bool CanDoThing() => ...;` |
| A command whose enabled state depends on a `CommandParameter` value supplied per-call | **Not supported** by this prototype (`ASSISTICANT302` is intentional) - a per-call parameter isn't a value `Computed<T>` can cache and invalidate the way a captured no-argument delegate's reads are. Use a hand-written `ICommand`/MAUI's own `Command<T>` for this case. |
| A Model's own collection of children | Still hand-written `ObservableList<T>`/`ObservableDictionary<K,V>` (see `Roster.cs` in the demo) - no generator support for this side yet |

### Per-platform: do you actually need `[NotifyPropertyChanged]`/`[Command]`/collection mode?

These three exist specifically to close gaps that only exist on some
platforms. Adding them where they aren't needed isn't wrong, just wasted
weight (an event field, a subscription, or a `Computed<bool>` nobody reads).

- **WPF / UWP**: `ForView.Wrap`'s reflection proxy (`Metas/TypeMeta.cs` and
  friends, `#if WPF`/UWP's `XamlTypes/`) already reflects over *any* plain
  object and gives you `INotifyPropertyChanged` (from the proxy, not your
  class), `ICommand` for any public parameterless method plus its `Can<X>`
  companion (`Metas/CommandMeta.cs`), and incremental `ObservableCollection<object>`
  diffing for any `IList`-returning member (`Metas/ListSlot.cs`). **Skip
  `[NotifyPropertyChanged]`, `[Command]`, and collection mode here** unless
  the same class is *also* bound unwrapped somewhere else (a MAUI app sharing
  ViewModels with a WPF app, or a unit test reading the raw object directly).
  `[Observable]`/scalar `[Computed]` still help - the proxy discovers
  generated properties exactly like hand-written ones.
- **Android / iOS (Xamarin, imperative `BindingManager`)**: binding is
  `BindingManager.Bind(() => vm.Prop, value => control.Text = value)`, driven
  by `Computed<T>.Subscribe` - it needs no `INotifyPropertyChanged` and no
  `ICommand` at all. `[Observable]`/scalar `[Computed]` reduce boilerplate the
  same as everywhere else; `[NotifyPropertyChanged]`/`[Command]`/collection
  mode are typically unnecessary here.
- **MAUI**: has no proxy layer at all (no `ForView.Wrap` equivalent), and its
  `{Binding}`/`CollectionView`/`Button.Command` genuinely require
  `INotifyPropertyChanged`/`INotifyCollectionChanged`(via `ObservableCollection<T>`)/`ICommand`
  directly on the bound object. **This is the platform all three exist for.**
  Use `[NotifyPropertyChanged]` on any ViewModel bound this way, `[Command]`
  for any `Button.Command` target, and collection mode for any
  `CollectionView.ItemsSource`.

## Language and runtime prerequisites

- `[Observable]`'s partial-property style needs **C# 13** in the *consuming*
  project. This is a compiler switch (`LangVersion`), independent of target
  framework - a `net45` or `netstandard1.4` project can use it as long as
  it's actually built with a C# 13-capable compiler (bundled with .NET SDK
  8.0.4xx+/9.0+, or Visual Studio 2022 17.12+). The target framework itself
  doesn't need to change. This repo's legacy, non-SDK-style `.csproj` files
  (`Assisticant.Framework.csproj` etc.) build via classic MSBuild/VS on
  Windows - whether that toolchain is new enough depends on the installed VS
  version, not anything in the project file.
- `[Computed]` (scalar or collection mode), `[NotifyPropertyChanged]`, and
  `[Command]` need no particular C# version - ordinary attributes on ordinary
  methods/classes. The BCL types they generate against
  (`System.Windows.Input.ICommand`, `System.ComponentModel.INotifyPropertyChanged`,
  `System.Collections.ObjectModel.ObservableCollection<T>`) are all available
  in `netstandard1.0`+/`net45`+ - i.e. everywhere Assisticant already targets.

## The one footgun to always check for in review

Any ViewModel type constructed inside a `[Computed]` collection-mode
projection **must** override `Equals`/`GetHashCode`, delegating to its wrapped
Model reference (see `PersonViewModel.cs` in the demo). `RecycleBin<T>.Extract`
matches old-vs-new items by `Equals`/`GetHashCode`, and a fresh instance is
constructed on *every* recompute (`.Select(m => new TViewModel(m))`) - without
that override, default reference equality never matches anything, and
recycling silently stops working with no compiler error. Verified directly by
removing the override in the demo: a 2-item collection with one item added
and one removed reported `CollectionChanged` actions `Remove, Remove, Add,
Add` instead of `Remove, Add` - *both* old items were discarded and rebuilt,
including the one that hadn't changed, and any ViewModel-local state on it
(e.g. `IsSelected`) was lost. This is exactly what `RecycleBin.cs`'s own doc
comment warns about ("It is imperative that you properly implement
`GetHashCode` and `Equals`"). No generator or analyzer catches this today.

## Diagnostics reference

| ID | Generator | Meaning |
|---|---|---|
| `ASSISTICANT001` | `[Observable]` | Containing type isn't `partial` |
| `ASSISTICANT002` | `[Observable]` | The property itself isn't `partial` |
| `ASSISTICANT003` | `[Observable]` | Property has an accessor body or an initializer (must be a bare `{ get; set; }`) |
| `ASSISTICANT004` | `[Observable]` | Property is missing `get` or `set` |
| `ASSISTICANT101` | `[Computed]` | Containing type isn't `partial` |
| `ASSISTICANT102` | `[Computed]` | Method declares parameters |
| `ASSISTICANT103` | `[Computed]` | Method returns `void` |
| `ASSISTICANT104` | `[Computed]` | Can't derive a property name (no `Compute`/`Get` prefix, no explicit name given) |
| `ASSISTICANT201` | `[NotifyPropertyChanged]` | Containing type isn't `partial` |
| `ASSISTICANT301` | `[Command]` | Containing type isn't `partial` |
| `ASSISTICANT302` | `[Command]` | Method declares parameters (scope boundary - `Command<T>`/`CommandParameter` isn't supported, not a bug) |
| `ASSISTICANT303` | `[Command]` | Method doesn't return `void` |

## What's explicitly not implemented (don't claim otherwise)

- No generator for a Model's own collection properties (`ObservableList<T>`/
  `ObservableDictionary<K,V>`) - still hand-written.
- No auto-generated `Equals`/`GetHashCode` for recycled ViewModel types (see
  the footgun above) - a natural follow-up, since a ViewModel's one
  constructor already names its injected Model, but not built.
- `[Command]` doesn't support MAUI's `Command<T>`/`CommandParameter` pattern,
  and doesn't batch multiple property changes from one `Execute` into a
  single UI update pass the way the WPF proxy does via
  `UpdateScheduler.Begin()`/`End()`.
- No `ComputedDictionary<TKey,TValue>` equivalent for collection mode.
- Not yet packaged as a NuGet analyzer - it's a prototype project referenced
  by `ProjectReference`/`OutputItemType="Analyzer"`, not something you add via
  a package.

## Where to look for more

- `SourceGenerators/README.md` - exhaustive design rationale and full
  generated-code listings for every mode, kept current as the prototype
  changes.
- `SourceGenerators/Assisticant.SourceGenerators.Demo/` - a runnable demo
  (`dotnet run --project SourceGenerators/Assisticant.SourceGenerators.Demo`)
  exercising every generator together in one compilation, with live
  assertions (not just printed output) for every behavior claimed above.
