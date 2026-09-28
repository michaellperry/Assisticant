using System;
using System.Collections.Generic;
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

            Environment.Exit(ok ? 0 : 1);
        }
    }
}
