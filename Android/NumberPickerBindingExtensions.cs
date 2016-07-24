using System;
using Android.Widget;

namespace Assisticant.Binding
{
	/// <summary>
	/// Number picker binding extensions.
	/// </summary>
	public static class NumberPickerBindingExtensions
	{
		class ValueBinding<TData> : IInputSubscription
		{
			private NumberPicker _control;
			private Action<TData> _input;
			private IDisplayDataConverter<int, TData> _converter;

			public ValueBinding(NumberPicker control, Action<TData> input, IDisplayDataConverter<int, TData> converter)
			{
				_control = control;
				_input = input;
				_converter = converter;
			}

			public void Subscribe()
			{
				_control.ValueChanged += NumberPickerValueChanged;
			}

			public void Unsubscribe()
			{
				_control.ValueChanged -= NumberPickerValueChanged;
			}

			private void NumberPickerValueChanged (object sender, EventArgs e)
			{
				_input(_converter.ConvertInput(_control.Value));
			}
		}

		class Identity : IDisplayDataConverter<int, int>
		{
			public static Identity Instance = new Identity();

			public int ConvertOutput (int data)
			{
				return data;
			}

			public int ConvertInput (int display)
			{
				return display;
			}
		}

		/// <summary>
        /// Bind the value of a number picker to a property using a value converter.
		/// </summary>
		/// <param name="bindings">The binding manager.</param>
        /// <param name="control">The number picker.</param>
		/// <param name="output">A function that gets the property.</param>
		/// <param name="input">An action sets the property.</param>
		/// <param name="converter">A custom value converter to type double.</param>
		/// <typeparam name="TData">The type of property to which the value is bound.</typeparam>
		public static void BindValue<TData>(this BindingManager bindings, NumberPicker control, Func<TData> output, Action<TData> input, IDisplayDataConverter<int, TData> converter)
		{
			bindings.Bind (output, s => control.Value = converter.ConvertOutput(s), new ValueBinding<TData>(control, input, converter));
		}

		/// <summary>
        /// Bind the value of a number picker to an integer property.
		/// </summary>
		/// <param name="bindings">The binding manager.</param>
		/// <param name="control">The number picker.</param>
		/// <param name="output">A function that gets the property.</param>
		/// <param name="input">An action sets the property.</param>
		public static void BindValue(this BindingManager bindings, NumberPicker control, Func<int> output, Action<int> input)
		{
			BindValue (bindings, control, output, input, Identity.Instance);
		}
	}
}

