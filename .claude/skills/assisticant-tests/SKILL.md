---
name: assisticant-tests
description: Write and run unit tests for the Assisticant library (Observable, Computed, ObservableList, ComputedList, UpdateScheduler, BindingManager, subscriptions, validation) in the style of the existing Assisticant.UnitTest suite, and run them on macOS/Linux/Windows with the bundled runner. Use this whenever you're asked to add, write, fix, or run tests in the Assisticant repo, to prove or reproduce a dependency-tracking or notification bug with a failing test, to cover an untested area before changing library code, or to check that a change didn't break anything, even if the request doesn't say "test" (e.g. "prove that Observable notifies before storing", "does BindingManager stop after Unbind?").
---

# Assisticant tests

Assisticant's tests are mostly a specification of the dependency graph. They say what should recompute, when, how often, and what should *not* be notified. New tests should read like more of that specification: a future maintainer should be able to delete the library code and rebuild it from the test names.

## Running tests

The checked-in test projects are old-style `.csproj` files that only build on Windows. Use the bundled runner instead. It works anywhere the .NET 8+ SDK is installed. Run it from inside the repo (or a worktree of it):

```bash
.claude/skills/assisticant-tests/scripts/run-tests.sh                                   # everything
.claude/skills/assisticant-tests/scripts/run-tests.sh "FullyQualifiedName~BindingManagerTest"  # one class
.claude/skills/assisticant-tests/scripts/run-tests.sh "Name=UnbindStopsUpdates"          # one test
```

The runner generates a throwaway SDK-style project outside the repo. It globs every `.cs` file in `Assisticant.UnitTest/` except `NotifyDataErrorInfoTests.cs` (that one needs the WPF-only proxy), references `Assisticant.Netstandard`, and runs `dotnet test` in Release with tiered compilation off. Release matters: the memory-leak tests null out a local and expect it to be collected, and unoptimized JIT code keeps locals alive until the method returns.

Baseline: 75 passed, 4 skipped (the `[Ignore]`d object-size tests). Every test file is compiled together, so one compile error breaks the whole run. Run the full suite once at the end, not only your filter. Failing tests are printed with their message and stack; passing tests aren't listed.

## Where files go

- Test classes: `Assisticant.UnitTest/<Subject>Test.cs`, namespace `Assisticant.UnitTest`.
- Model/fixture classes shared by several tests: a `<Domain>Data/` folder (like `ContactListData/`, `CollectionData/`), namespace `Assisticant.UnitTest.<Domain>Data`. Classes used by only one test class can be private nested classes (see `Counter` in `SubscriptionTest.cs`).
- Add every new file as a `<Compile Include="..." />` entry in `Assisticant.UnitTest/Assisticant.UnitTest.Portable.csproj`. The runner doesn't need this because it globs, but the Windows build does, since old-style projects don't glob.
- Tests that need the WPF proxy (`ForView.Wrap`, `INotifyDataErrorInfo`) go in `Assisticant.UnitTest.WPF.csproj` instead. They can't run off Windows, so say so when you write one.

## The house style, and why

**Test through small model classes, the way users write them.** Don't new up `Observable<int>` in every test and poke at it. Write a tiny domain class that hides its `Observable<T>`/`Computed<T>` fields behind ordinary properties (`SourceData`, `DirectComputed`, `ContactList`, `ContactListViewModel`), and test that class. This is how Assisticant is really used, and it catches bugs that only show up through property getters and setters. Direct field-level tests are fine for the field types themselves (`SubscriptionTest` does this).

**One behavior per test, named as a sentence.** Put shared setup in `[TestInitialize]`. Each test states one fact: `ComputedIsOutOfDateAgainAfterChange`, `ListDoesNotDependUponChildProperties`, `WhenSortOrderChangedShouldNotify`. Several asserts are fine when together they check that single fact. Accept duplication between similar test classes (compare `DirectComputedTest` and `IndirectComputedTest`) rather than building an abstraction the reader has to decode.

**Assert on laziness and on how much work was done, not just on values.** A computed that returns the right answer but recomputes on every read is a bug here. Where it's relevant, check:
- `IsUpToDate` before and after a read. Computeds start out of date and update only when read.
- How many times a source was read, or an update ran (`PrecedentIsOnlyAskedOnce`).
- Object identity when things should be reused (`Assert.AreSame` or `.Should().BeSameAs()`, as in `ComputedsAreRecycled`).

A read that exists only to force evaluation is written as `int fetch = model.Property;`. Follow that idiom so the intent stays visible.

**Pair each "it notifies" test with a "it doesn't notify" test.** Precise invalidation is the point of the library, so for most positive tests, write the negative neighbor: a change to something the dependent never read should not invalidate it. Also test dependencies that change over time. After a branch switches (sort order, `&&` short-circuit, a condition), the old dependency should be dropped. Hook `Computed.Invalidated` (or `ComputedList.ComputedSentry.Invalidated`) or count subscription callbacks, and assert on the count.

**Make concurrency deterministic.** Inject the "concurrent" change at an exact moment through a hook rather than racing threads. `SourceData.AfterGet` runs a callback on a thread-pool thread in the middle of a read while the reader waits. Brute-force thread tests (`MultithreadedTest`) are the exception: keep them few, lock on the caller side as the README tells users to, and assert only on final totals.

**Memory is a feature.** Weak links from sources to dependents are tested with `WeakReference`: hold a strong reference and assert alive, null it, `GC.Collect()`, assert dead, then show the source still works. Creating the object inside a `[MethodImpl(MethodImplOptions.NoInlining)]` helper makes the test independent of JIT settings.

## The update scheduler is global. Share one test harness.

`ComputedSubscription` (used by `Computed<T>.Subscribe` and `BindingManager.Bind`) never calls you back directly. It hands the update to `UpdateScheduler.ScheduleUpdate`. `UpdateScheduler.Initialize` is process-wide and **only the first call wins**. All test classes run in one process, so if two classes each call `Initialize` with their own private queue, only one queue ever receives work. The other class's tests then pass or fail depending on test order.

`SubscriptionTest` was written with its own private static queue. That only works while it's the sole class that initializes the scheduler. Don't copy that pattern. If `Assisticant.UnitTest/TestScheduler.cs` doesn't exist yet, create it (and add it to the Portable csproj), and use it in every test class that subscribes:

```csharp
using System;
using System.Collections.Generic;

namespace Assisticant.UnitTest
{
    // UpdateScheduler.Initialize is first-call-wins and process-wide, so every
    // test class must go through this one harness.
    public static class TestScheduler
    {
        private static readonly Queue<Action> _queue = new Queue<Action>();
        private static bool _initialized;

        // When true, updates run the moment they're scheduled, like a UI
        // dispatcher that executes synchronously on the UI thread.
        public static bool RunInline { get; set; }

        public static void Reset()
        {
            if (!_initialized)
            {
                UpdateScheduler.Initialize(a => { if (RunInline) a(); else _queue.Enqueue(a); });
                _initialized = true;
            }
            RunInline = false;
            _queue.Clear();
        }

        public static void Process()
        {
            while (_queue.Count > 0)
                _queue.Dequeue()();
        }
    }
}
```

Call `TestScheduler.Reset()` in `[TestInitialize]`. Call `TestScheduler.Process()` wherever the old tests call their private `Process()`, which means after subscribing too, because subscribing schedules the first update.

**When you create `TestScheduler`, move the existing private-queue classes in the same project onto it in the same change.** Otherwise whichever class initializes first wins, and the other class's tests fail depending on the run order. Today that's `SubscriptionTest`: point its `[TestInitialize]` at `TestScheduler.Reset()` and its `Process()` at `TestScheduler.Process()`, and delete its private queue. This is a test-only change that keeps its behavior, so do it without asking, and mention it in your summary. `NotifyDataErrorInfoTests` is the exception. It's compiled only into the WPF project, which doesn't include `TestScheduler`, so leave it alone. If the full suite shows failures in existing subscription tests after your change, suspect this collision first.

Use `RunInline = true` to test what happens under a dispatcher that runs synchronously (MAUI's `BeginInvokeOnMainThread` does this on the UI thread). That's where ordering bugs show up, because the update re-reads the model before the setter has finished. A queued scheduler hides them.

## Tests that document a bug

When a test exists to prove a suspected bug:
- Write the assertion for the **correct** behavior and name the test for it (`SubscriberSeesNewValueWithInlineScheduler`, not `SubscriberSeesOldValue`).
- Run it and let it fail. Report the failure message as the evidence.
- Don't weaken the assertion, mark it `[Ignore]`, or change library code to make it pass, unless you were asked to fix the bug. A red test with a clear name is the deliverable.
- Add a one-line comment saying what's wrong today and why (e.g. "`Observable<T>.Value` calls `OnSet()` before storing `_value`").

Before calling a behavior a bug, read the library code. Some surprising behavior is intended. For example, `a && b` in a computed doesn't depend on `b` while `a` is false; that's dynamic dependency discovery working correctly.

## Assertions: stay compatible with both runners

The Windows project uses FluentAssertions 4.19 and the old MSTest (`Microsoft.VisualStudio.QualityTools.UnitTestFramework`). The runner uses FluentAssertions 6 and MSTest 3. Stick to APIs that exist in both:
- MSTest: `Assert.AreEqual`, `IsTrue`, `IsFalse`, `AreSame`, `IsNull`, `IsNotNull`, `Fail`. There's no `Assert.ThrowsException` in old MSTest. For exceptions, use try/catch with `Assert.Fail` in the try.
- FluentAssertions: `.Should().Be`, `NotBe`, `BeTrue`, `BeFalse`, `BeNull`, `NotBeNull`, `BeSameAs`, and on collections `Equal(...)`, `HaveCount`, `BeEmpty`. Avoid `Should().Throw` (it's `ShouldThrow` in 4.x) and `BeEquivalentTo` (its semantics changed).
- `using Microsoft.VisualStudio.TestTools.UnitTesting;` without the old `#if NETFX_CORE` block, as in `SubscriptionTest.cs`.

Newer tests (`SubscriptionTest`, `LargeListTest`) use FluentAssertions. Older ones use `Assert` with a message describing the failure ("The dependent did not go out of date"). Either is fine. Stay consistent within a file.

## When you finish

Report:
- which files you added or changed (including the csproj entries),
- the runner command and its summary line for your tests and for the full suite. Every failure in the full suite should be one of your intentional bug tests. A failure in a test you didn't write means your change broke something, so fix it before reporting.
- any test that fails on purpose (bug documentation) and what it shows,
- anything you couldn't run (WPF-only tests).

## Where coverage is thin

If asked to "improve coverage" without a target, these areas have no tests today, and the MAUI integration depends on them:
- `BindingManager`: `Bind` pushes values and `Unbind` stops them.
- `ObservableList<T>` / `ObservableDictionary` mutators invalidating dependents (and *not* invalidating unrelated dependents).
- `UpdateScheduler.Begin()`/`End()` batching.
- Store-before-notify ordering in `Observable<T>` and the collection types under an inline scheduler.
- `ViewProxy`/`MemberSlot` (WPF-only, via `ForView.Wrap`).
