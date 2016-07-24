using System;
using UIKit;
using Foundation;

namespace Assisticant.Binding
{
	/// <summary>
	/// Binding manager extensions.
	/// </summary>
	public static class BindingManagerExtensions
	{
		/// <summary>
		/// Initialize the binding manager for a view controller.
		/// </summary>
		/// <param name="bindings">The binding manager for this view.</param>
		/// <param name="controller">The view controller for this view.</param>
		public static void Initialize (this BindingManager bindings, UIViewController controller)
		{
			UpdateScheduler.Initialize (a =>
				controller.BeginInvokeOnMainThread (new Action(a)));
		}
	}
}

