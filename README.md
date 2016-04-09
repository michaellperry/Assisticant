# Assisticant

Intelligent, thoughtful data binding.

## Overview

Assisticant uses dependency tracking to discover the data that your code depends upon. When that data changes, it takes action. Typically, that action is updating a view model. But it can be so much more.

Programming with Observables is a lot like using a spreadsheet. All you have to express are the equations that describe the desired behavior of your system. Assisticant will find the optimal execution of those formulae and invoke them in just the right order.

More information is available at http://assisticant.net.

## Get Started

Create an application using any of the XAML stacks: WPF, UWP, Windows Phone, etc.

```
Install-Package Assisticant
```

First, create a model. It uses Observables to store its data.

```c#
public class Model
{
    private Observable<int> _x = new Observable<int>(41);

    public PropertyX
    {
        get { return _x.Value; }
        set { _x.Value = value; }
    }
}
```

Then create a view model. It doesn't inherit from a base class or implement an interface!

```c#
public class MainViewModel
{
    private readonly Model _model;

    public MainViewModel(Model model)
    {
        _model = model;
    }

    public int PropertyY => _model.PropertyX + 1;
}
```

Then create a view model locator. (If you don't like the view model locator pattern, don't worry. You have options.)

```c#
public class ViewModelLocator : ViewModelLocatorBase
{
    private Model _model = new Model();

    public object Main => ViewModel(() => new MainViewModel(_model));
}
```

Put the view model locator in a resource dictionary.

```xaml
<Application>
    <Application.Resources>
        <local:ViewModelLocator x:Key="Locator" />
    </Application.Resources>
</Application>
```

Bind!

```xaml
<Window
    DataContext="{Binding Source={StaticResource Locator}, Path=Main}" >
    <TextBox Text="{Binding PropertyY}" />
</Window>
```

## Efficiency

Did you know that INotifyPropertyChanged is inefficient? When you raise PropertyChanged, all listeners are immediately updated on the same thread. Execution blocks while they update the views, recompute the layout, and rerender the pages. Then your code resumes ... only to modify another property and start the whole thing over again!

Let Assisticant queue those updates for you. Go ahead and modify all of the properties that you intend to modify. When you're done, Assisticant will find the optimal path to update just the parts of the application that were affected. Compared to the typical MVVM framework, it's much more efficient.

## Thread Safety

PropertyChanged events are synchronous. They aren't going to be return to the sender until all recipients have responded to them. Those recipients need to update the UI. That's why PropertyChanged events should only be raised on the UI thread.

You can change an Observable on any thread. Just be sure that you wrap it inside of a `lock` statement so that you don't get more than one thread accessing it at the same time. Assisticant will only raise PropertyChanged events on the UI thread, even though you changed the Observable in the background.

```c#
public class Clock
{
    private Observable<int> _time = new Observable<int>();

    public Clock()
    {
        var thread = new Thread(delegate
        {
            while (true)
            {
                Thread.Sleep(1000);
                lock (this)
                {
                    _time.Value = _time.Value + 1;
                }
            }
        };
        thread.Start();
    }

    public int Time
    {
        get
        {
            lock (this)
            {
                return _time.Value;
            }
        }
    }
}
```

## More than MVVM

View models are only the beginning. With Assisticant, you can create Computed properties that are calculated only when they need to be. These are great for caching results of expensive operations.

```c#
public class Calculator
{
    private Observable<int> _digitCount = new Observable<int>(4);
    private Computed<double> _pi;

    public Calculator()
    {
        _pi = new Computed<double>(() =>
            ComputePiToDigits(_digitCount.Value));
    }

    public int DigitCount
    {
        get { return _digitCount.Value; }
        set { _digitCount.Value = value; }
    }

    public double Pi => _pi.Value;

    private double ComputePiToDigits(int digitCount)
    {
        //...
    }
}
```

Here's a [great example](https://github.com/Assisticant/DecisionTree/blob/master/DecisionTree/Models/Nodes/ProbabilityNode.cs#L16) of building a machine out of computeds. See? Just like an object-oriented spreadsheet!

Subscribe to computeds to take additional action when it changes. This is a great way to update things that are not traditionally bindable. For example, live tiles.

```c#
public class LiveTileUpdater
{
    private readonly Inbox _inbox;
    private Computed<int> _messageCount;
    private ComputedSubscription _subscription;

    public LiveTileUpddater(Inbox inbox)
    {
        _inbox = inbox;
        _messageCount = new Computed<int>(() => _inbox.Messages
            .Where(m => !m.Read).Count());
        _subscription = _messageCount.Subscribe(UpdateLiveTile);
    }

    public void UpdateLiveTile(int messageCount)
    {
        //...
    }
}
```
