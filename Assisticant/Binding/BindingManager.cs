using System;
using System.Collections.Generic;
using Assisticant.Fields;

namespace Assisticant.Binding
{
	/// <summary>
	/// Manages all data bindings for a view. Be sure to Initialize on load, Bind properties when
	/// the view is displayed, and Unbind when the view dissapears.
	/// </summary>
	public class BindingManager
	{
		struct SubscriptionPair
		{
			public ComputedSubscription Output;
			public IInputSubscription Input;
		}

		private List<SubscriptionPair> _subscriptions = new List<SubscriptionPair>();

		/// <summary>
		/// Initializes a new instance of the <see cref="Assisticant.Binding.BindingManager"/> class.
		/// </summary>
		public BindingManager()
		{
		}

		/// <summary>
		/// Bind the results of a function to an action.
		/// </summary>
		/// <param name="function">The function that computes a value to output.</param>
		/// <param name="action">The action to perform when new output is computed.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public void Bind<T>(Func<T> function, Action<T> action)
		{
			_subscriptions.Add (new SubscriptionPair
			{
				Output = new Computed<T> (function).Subscribe (action)
			});
		}

		/// <summary>
		/// Bind a custom input subscription.
		/// </summary>
		/// <param name="input">The custom input subscription.</param>
		public void Bind(IInputSubscription input)
		{
			input.Subscribe ();
			_subscriptions.Add (new SubscriptionPair
			{
				Input = input
			});
		}

		/// <summary>
		/// Bind the results of a function to an action, and a custom input subscription.
		/// </summary>
		/// <param name="function">The function that computes a value to output.</param>
		/// <param name="action">The action to perform when new output is computed.</param>
		/// <param name="input">The custom input subscription.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public void Bind<T>(Func<T> function, Action<T> action, IInputSubscription input)
		{
			input.Subscribe ();
			_subscriptions.Add (new SubscriptionPair
			{
				Output = new Computed<T> (function).Subscribe (action),
				Input = input
			});
		}

		/// <summary>
		/// Unbind all bindings. Call this method when the view disappers.
		/// </summary>
		public void Unbind()
		{
			foreach (var subscription in _subscriptions) {
				if (subscription.Output != null)
					subscription.Output.Unsubscribe ();
				if (subscription.Input != null)
					subscription.Input.Unsubscribe ();
			}
            _subscriptions.Clear();
		}
	}
}

