using System;
using Foundation;
using UIKit;

namespace Assisticant.Binding
{
	/// <summary>
	/// Stepper binding extensions.
	/// </summary>
	public static class StepperBindingExtensions
	{
		class ValueBinding<TData> : IInputSubscription
		{
			private UIStepper _control;
			private Action<TData> _input;
			private IDisplayDataConverter<double, TData> _converter;

			public ValueBinding(UIStepper control, Action<TData> input, IDisplayDataConverter<double, TData> converter)
			{
				_control = control;
				_input = input;
				_converter = converter;
			}

			public void Subscribe()
			{
				_control.ValueChanged += StepperValueChanged;
			}

			public void Unsubscribe()
			{
				_control.ValueChanged -= StepperValueChanged;
			}

			private void StepperValueChanged (object sender, EventArgs e)
			{
				_input(_converter.ConvertInput(_control.Value));
			}
		}

		class Identity : IDisplayDataConverter<double, double>
		{
			public static Identity Instance = new Identity();

			public double ConvertOutput (double data)
			{
				return data;
			}

			public double ConvertInput (double display)
			{
				return display;
			}
		}

		class ConvertInt : IDisplayDataConverter<double, int>
		{
			public static ConvertInt Instance = new ConvertInt();

			public double ConvertOutput (int data)
			{
				return data;
			}

			public int ConvertInput (double display)
			{
				return (int)display;
			}
		}

		/// <summary>
		/// Bind the Value property of a UIStepper to a property using a value converter.
		/// </summary>
		/// <param name="bindings">The binding manager.</param>
		/// <param name="control">The stepper.</param>
		/// <param name="output">A function that gets the property.</param>
		/// <param name="input">An action sets the property.</param>
		/// <param name="converter">A custom value converter to type double.</param>
		/// <typeparam name="TData">The type of property to which the value is bound.</typeparam>
		public static void BindValue<TData>(this BindingManager bindings, UIStepper control, Func<TData> output, Action<TData> input, IDisplayDataConverter<double, TData> converter)
		{
			bindings.Bind (output, s => control.Value = converter.ConvertOutput(s), new ValueBinding<TData>(control, input, converter));
		}

		/// <summary>
		/// Bind the Value property of a UIStepper to a double property.
		/// </summary>
		/// <param name="bindings">The binding manager.</param>
		/// <param name="control">The stepper.</param>
		/// <param name="output">A function that gets the property.</param>
		/// <param name="input">An action sets the property.</param>
		public static void BindValue(this BindingManager bindings, UIStepper control, Func<double> output, Action<double> input)
		{
			BindValue (bindings, control, output, input, Identity.Instance);
		}

		/// <summary>
		/// Bind the Value property of a UIStepper to an integer property.
		/// </summary>
		/// <param name="bindings">The binding manager.</param>
		/// <param name="control">The stepper.</param>
		/// <param name="output">A function that gets the property.</param>
		/// <param name="input">An action sets the property.</param>
		public static void BindValue(this BindingManager bindings, UIStepper control, Func<int> output, Action<int> input)
		{
			BindValue (bindings, control, output, input, ConvertInt.Instance);
		}
	}
}

