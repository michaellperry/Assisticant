using System;
using System.Collections.Generic;
using System.Linq;
using Assisticant;

namespace Assisticant.SourceGenerators.Demo
{
    public static class Program
    {
        // A minimal stand-in for a real UI dispatcher: it must queue rather than
        // run inline (see UpdateScheduler's documented contract), so scheduled
        // updates are only applied when we explicitly Pump() - simulating the
        // next tick of a real message loop.
        private static readonly Queue<Action> _pending = new Queue<Action>();

        private static void Pump()
        {
            while (_pending.Count > 0)
                _pending.Dequeue()();
        }

        public static void Main()
        {
            UpdateScheduler.Initialize(_pending.Enqueue);

            // Model: independent state. Person has no immutable fields, so no
            // constructor - initial mutable state is set with an object
            // initializer instead of a constructor parameter.
            var person = new Person { Name = "Alice", Age = 30 };

            // ViewModel: dependent state, derived from the injected Model - just
            // as a View is handed a ViewModel via DataContext/ForView.Wrap. Plus
            // its own IsSelected - a deliberate exception to the usual
            // separation (see PersonViewModel.cs). [NotifyPropertyChanged] makes
            // it directly bindable with MAUI's {Binding} - proven below via its
            // own PropertyChanged event, without any BindingManager or
            // Computed<T>.Subscribe involved.
            var viewModel = new PersonViewModel(person);

            Console.WriteLine($"Greeting={viewModel.Greeting}, IsAdult={viewModel.IsAdult}, SelectionLabel={viewModel.SelectionLabel}");

            var raised = new List<string>();
            viewModel.PropertyChanged += (_, e) => raised.Add(e.PropertyName!);

            person.Name = "Bob"; // Model change - affects Greeting only
            Pump();

            person.Age = 17; // Model change - affects IsAdult only
            Pump();

            viewModel.IsSelected = true; // ViewModel-local change - affects IsSelected itself and SelectionLabel, not the Model at all
            Pump();

            Console.WriteLine();
            Console.WriteLine("PropertyChanged raised for:");
            foreach (var name in raised)
                Console.WriteLine($"  {name}");

            Console.WriteLine();
            Console.WriteLine($"Greeting={viewModel.Greeting}, IsAdult={viewModel.IsAdult}, SelectionLabel={viewModel.SelectionLabel}");

            // SelectionLabel is raised BEFORE IsSelected, even though IsSelected is
            // the one that was set: IsSelected's setter invalidates SelectionLabel's
            // Computed<T> synchronously, as part of assigning __isSelectedField.Value
            // (invalidation cascades through the dependency graph before that
            // statement returns) - so SelectionLabel's deferred raise is scheduled
            // first, and IsSelected's own trailing "schedule my raise" statement runs
            // second. Both still land in the same Pump() either way.
            bool ok = raised.Count == 4
                && raised[0] == "Greeting"
                && raised[1] == "IsAdult"
                && raised[2] == "SelectionLabel"
                && raised[3] == "IsSelected"
                && viewModel.Greeting == "Hello, Bob!"
                && viewModel.IsAdult == false
                && viewModel.SelectionLabel == "Selected";

            Console.WriteLine();
            Console.WriteLine(ok
                ? "OK: Model changes and ViewModel-local changes both raise PropertyChanged correctly, and only for what actually depends on them."
                : "FAIL: something did not behave as expected.");

            // --- [Command] demo ---
            // ClearSelectionCommand's CanExecute (CanClearSelection => IsSelected) is
            // wrapped in a Computed<bool> - CanExecuteChanged fires automatically
            // whenever IsSelected changes, with no manual "RefreshCanExecutes()" call
            // anywhere, unlike .NET MAUI's own Command class.
            Console.WriteLine();
            Console.WriteLine("--- [Command] demo ---");

            var canExecuteValues = new List<bool>();
            viewModel.ClearSelectionCommand.CanExecuteChanged +=
                (_, __) => canExecuteValues.Add(viewModel.ClearSelectionCommand.CanExecute(null));

            Console.WriteLine($"IsSelected={viewModel.IsSelected}, CanExecute={viewModel.ClearSelectionCommand.CanExecute(null)}");

            viewModel.ClearSelectionCommand.Execute(null); // sets IsSelected = false
            Pump();
            Console.WriteLine($"After Execute(): IsSelected={viewModel.IsSelected}, CanExecute={viewModel.ClearSelectionCommand.CanExecute(null)}");

            viewModel.IsSelected = true; // flips CanExecute back, with no command-specific code involved
            Pump();
            Console.WriteLine($"After IsSelected=true: CanExecute={viewModel.ClearSelectionCommand.CanExecute(null)}");

            bool commandOk = canExecuteValues.Count == 2
                && canExecuteValues[0] == false // after Execute() set IsSelected = false
                && canExecuteValues[1] == true; // after IsSelected was set back to true

            Console.WriteLine();
            Console.WriteLine(commandOk
                ? "OK: CanExecuteChanged fired automatically both times IsSelected changed - no manual bookkeeping."
                : "FAIL: CanExecuteChanged did not fire as expected.");

            ok &= commandOk;

            // --- [Computed] collection-mode demo ---
            // People is a live ObservableCollection<PersonViewModel>, synchronized
            // (not replaced) on every recompute of the Model's ObservableList<Person>.
            Console.WriteLine();
            Console.WriteLine("--- [Computed] collection mode demo ---");

            var roster = new Roster();
            roster.People.Add(new Person { Name = "Carol", Age = 25 });
            roster.People.Add(new Person { Name = "Dave", Age = 40 });

            var rosterViewModel = new RosterViewModel(roster);

            var collectionActions = new List<string>();
            rosterViewModel.People.CollectionChanged += (_, e) => collectionActions.Add(e.Action.ToString());

            Console.WriteLine($"Initial: {string.Join(", ", rosterViewModel.People.Select(p => p.Greeting))}");

            // Capture a specific item's ViewModel and give it some ViewModel-local
            // state, to prove recycling preserves it across the recompute below.
            var carolViewModel = rosterViewModel.People.First(p => p.Greeting == "Hello, Carol!");
            carolViewModel.IsSelected = true;
            Pump();

            roster.People.Add(new Person { Name = "Eve", Age = 22 });
            roster.People.Remove(roster.People.First(p => p.Name == "Dave"));
            Pump();

            Console.WriteLine($"After add Eve/remove Dave: {string.Join(", ", rosterViewModel.People.Select(p => p.Greeting))}");
            Console.WriteLine($"CollectionChanged actions: {string.Join(", ", collectionActions)}");

            var carolViewModelAfter = rosterViewModel.People.First(p => p.Greeting == "Hello, Carol!");

            bool collectionOk = rosterViewModel.People.Count == 2
                && rosterViewModel.People.Any(p => p.Greeting == "Hello, Eve!")
                && !rosterViewModel.People.Any(p => p.Greeting == "Hello, Dave!")
                && ReferenceEquals(carolViewModel, carolViewModelAfter) // same instance - recycled, not rebuilt
                && carolViewModelAfter.IsSelected // ViewModel-local state survived the recompute
                && !collectionActions.Contains("Reset"); // incremental, not a wholesale rebuild

            Console.WriteLine();
            Console.WriteLine(collectionOk
                ? "OK: recompute added Eve and removed Dave incrementally, while Carol's ViewModel (and her IsSelected) was recycled, not rebuilt."
                : "FAIL: collection recompute did not behave as expected.");

            ok &= collectionOk;
            Environment.Exit(ok ? 0 : 1);
        }
    }
}
