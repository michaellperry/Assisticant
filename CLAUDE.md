# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

Assisticant is a .NET MVVM/data-binding library that uses automatic dependency tracking instead of `INotifyPropertyChanged`. Models store state in `Observable<T>`; derived values are `Computed<T>`; view models are plain classes (no base class) that get wrapped in a proxy which raises `PropertyChanged` on the UI thread. Distributed as the `Assisticant` NuGet package (version lives in `NuGet/Core/assisticant.nuspec`).

## Building and testing

This is a legacy Windows-centric solution (`Assisticant.sln`): old-style `.csproj` files targeting .NET Framework 4.5, a `netstandard1.4` SDK project, and Xamarin.Android/iOS projects. A full build needs Visual Studio/MSBuild on Windows with the Xamarin workloads; it will not fully build with `dotnet build` on macOS/Linux. CI (`.github/workflows`) just triggers an Azure DevOps pipeline ("Build from GitHub"), so the real build definition is not in this repo.

- Build: `msbuild Assisticant.sln /p:Configuration=Release` (restore NuGet packages first; test projects use `packages.config`)
- Tests use MSTest (`[TestClass]`/`[TestMethod]`) with FluentAssertions 4.19. On any OS with the .NET 8+ SDK, run them with `.claude/skills/assisticant-tests/scripts/run-tests.sh [filter]` (e.g. `"FullyQualifiedName~SubscriptionTest"`). It builds a throwaway SDK-style project against `Assisticant.Netstandard` and runs everything except the WPF-only `NotifyDataErrorInfoTests`. Baseline: 75 passed, 4 skipped. On Windows you can also use `vstest.console.exe` against the built test DLL.
- When writing tests, use the `assisticant-tests` skill (`.claude/skills/assisticant-tests/SKILL.md`). It covers the house style and the shared `UpdateScheduler` harness.
- `Assisticant.Netstandard` can be built alone with `dotnet build Assisticant.Netstandard/Assisticant.Netstandard.csproj`.
- Packaging: `nuget pack NuGet/Core/assisticant.nuspec` after a Release build. The nuspec pulls DLLs from fixed `bin\...\Release` paths, so output paths in the csproj files must not change.

## Project layout — one source folder, many projects

All core source lives in `Assisticant/`, but it is compiled by several projects that each select a subset of files and set a platform define:

| Project | Assembly | Define | Contents |
|---|---|---|---|
| `Assisticant/Assisticant.Framework.csproj` | `Assisticant` (net45) | `NETFRAMEWORK` | Platform-neutral core |
| `Assisticant/Assisticant.WPF.csproj` | `Assisticant.XAML` (net45) | `WPF` | WPF layer; references Framework |
| `Assisticant.Netstandard/…csproj` | `Assisticant` (netstandard1.4) | `NETSTANDARD1_4` | Core files **linked** from `../Assisticant/` |
| `Android/`, `iOS/` | `Assisticant.Android` / `.iOS` | — | Imperative binding extensions; reference Netstandard |
| `Assisticant/Assisticant.Universal.csproj` | — | `UNIVERSAL` | UWP/PCL build; **not in the solution** (stale) |

Consequences when editing:
- **Adding a core file** means adding a `<Compile>` entry to `Assisticant.Framework.csproj` *and* a linked `<Compile Include="..\Assisticant\..." Link="..."/>` in `Assisticant.Netstandard.csproj`. Old-style csproj files do not glob.
- Platform differences are handled with `#if WPF` / `#if NETFRAMEWORK` / `#if NETSTANDARD1_4` inside shared files.
- The .NET Framework assemblies are strong-named with `Common/Mallardsoft.snk`; `Common/CommonProperties.cs` is linked into them for shared assembly attributes.
- Two test projects live in `Assisticant.UnitTest/`. `Assisticant.UnitTest.Portable.csproj` (references Netstandard) compiles nearly all the tests. `Assisticant.UnitTest.WPF.csproj` (references Framework + WPF) compiles only `NotifyDataErrorInfoTests.cs`, because it needs the WPF proxy. Put new core tests in the Portable project.

## Core architecture

**Dependency tracking** (`Precedent.cs`, `Observable.cs`, `Computed.cs`, `Fields/`):
- `Precedent` is the base for anything that can be depended on. `Observable` (a sentry for mutable state) and `Computed` (a cached derivation) both derive from it; `Computed` is also a dependent.
- While a `Computed` runs its update delegate, it is the thread's "current update". Every `Precedent.OnGet()` read during that time records itself as a precedent of the current computed (`RecordDependent`/`AddPrecedent`). There is no explicit dependency declaration.
- `Observable.OnSet()` calls `MakeDependentsOutOfDate()`, which invalidates dependents transitively. Computeds are lazy: they recompute on the next read. The `Invalidated` event is the hook used by subscriptions and view proxies.
- `Fields/Observable<T>`, `Fields/Computed<T>` and `ComputedSubscription` are the typed wrappers users see; `Collections/` provides `ObservableList`/`ObservableDictionary` and `ComputedList`/`ComputedDictionary`. `ComputedList` uses a `RecycleBin` to reuse child objects across recomputations.
- Weak references (`WeakArray`, `WeakHashSet`) keep precedent→dependent links from causing leaks (see `MemoryLeakTest`).
- `NamedPrecedents.cs` and the visualizer classes support debugging (`Precedent.DebugMode`).

**Update batching / threading** (`UpdateScheduler.cs`): invalidations schedule UI updates rather than firing them synchronously. `UpdateScheduler.Initialize(runOnUIThread)` (called from `ForView.Initialize`) sets how updates get marshalled to the UI thread. Updates scheduled before initialization are queued. `Begin()`/`End()` capture a batch. Observables can be changed from any thread (callers lock), and `PropertyChanged` is still raised only on the UI thread.

**View-model proxying** (`Metas/`, `Descriptors/`, `XamlTypes/`, `ForView.cs`, `ViewModelLocatorBase.cs`):
- `ForView.Wrap(obj)` creates a `PlatformProxy<T>` around any plain object. `ViewModelLocatorBase.ViewModel(() => ...)` does this lazily per property (keyed by `[CallerMemberName]`) and returns the unwrapped instance in design mode.
- `TypeMeta` reflects the view-model type into `MemberMeta`s (properties, `Observable`/`Computed` fields, and methods exposed as commands via `CommandMeta`/`MethodCommand`). `ViewProxy` builds a `MemberSlot` per member. Each slot wraps the getter in a `Computed`, so when its dependencies change the slot fires `PropertyChanged` for that member. Child objects and collections are recursively wrapped (`AtomSlot`, `CollectionSlot`, `ListSlot`, `PassThroughSlot`).
- The platform layer exposes the proxy's members to the binding engine: `Descriptors/` uses `ICustomTypeDescriptor`/`TypeDescriptionProvider` for WPF (plus `INotifyDataErrorInfo` via `Validation/`), and `XamlTypes/` uses `IXamlMetadataProvider` for UWP.
- `ForView.Unwrap<T>(dataContext)` gets the original object back from a proxy (e.g. in code-behind event handlers).

**Mobile** (`Android/`, `iOS/`): there is no XAML, so binding is imperative. `Binding/BindingManager` (in core) plus per-control extension methods (`TextBindingExtensions`, `ButtonBindingExtensions`, list/table views, etc.) subscribe computeds to native controls. The manager is initialized from the Activity/ViewController.

**Timers** (`Timers/`): time-dependent observables (`FloatingDateTime`, `RisingTimeSpan`, `DroppingTimeSpan`, …) that invalidate dependents when a time threshold is crossed. They are driven by `FloatingTimeZone`, which is also initialized with the UI-thread dispatcher.

## NuGet packages

- `NuGet/Core`: the main `Assisticant` package (lib assemblies for net45, MonoAndroid, Xamarin.iOS).
- `NuGet/App`: `Assisticant.App`, which drops starter `Models/` and `ViewModels/` source (`.cs.pp` transforms) into a consuming project.
- `NuGet/Snippets`: Visual Studio code snippets.
