using System;
using Foundation;
using UIKit;
using System.Collections.Generic;

namespace Assisticant.Binding
{
	/// <summary>
	/// Text binding extensions.
	/// </summary>
	public static class TextBindingExtensions
	{
		class TextBinding<TData> : IInputSubscription
		{
			private UITextField _control;
			private Action<TData> _input;
			private IDisplayDataConverter<string, TData> _converter;

			public TextBinding(UITextField control, Action<TData> input, IDisplayDataConverter<string, TData> converter)
			{
				_control = control;
				_input = input;
				_converter = converter;
			}

			public void Subscribe()
			{
				_control.EditingChanged += TextEditingChanged;
			}

			public void Unsubscribe()
			{
				_control.EditingChanged -= TextEditingChanged;
			}

			private void TextEditingChanged (object sender, EventArgs e)
			{
				_input(_converter.ConvertInput(_control.Text));
			}
		}

		class Identity : IDisplayDataConverter<string, string>
		{
			public static Identity Instance = new Identity();

			public string ConvertOutput (string data)
			{
				return data;
			}

			public string ConvertInput (string display)
			{
				return display;
			}
		}

		class ConvertInt : IDisplayDataConverter<string, int>
		{
			public static ConvertInt Instance = new ConvertInt();

			public string ConvertOutput (int data)
			{
				return data.ToString ();
			}

			public int ConvertInput (string display)
			{
				int data = 0;
				if (int.TryParse (display, out data))
					return data;
				else
					return 0;
			}
		}

		/// <summary>
		/// Bind the Text of a UITextField to a property using a value converter.
		/// </summary>
		/// <param name="bindings">The binding manager.</param>
		/// <param name="control">The text field.</param>
		/// <param name="output">A function that gets the property.</param>
		/// <param name="input">A function that sets the property.</param>
		/// <param name="converter">A custom value converter to string.</param>
		/// <typeparam name="TData">The type of the property.</typeparam>
		public static void BindText<TData>(this BindingManager bindings, UITextField control, Func<TData> output, Action<TData> input, IDisplayDataConverter<string, TData> converter)
		{
			bindings.Bind (output, s => control.Text = converter.ConvertOutput(s), new TextBinding<TData>(control, input, converter));
		}

		/// <summary>
		/// Bind the Text of a UILabel to a read-only property using a value converter.
		/// </summary>
		/// <param name="bindings">The binding manager.</param>
		/// <param name="control">The label.</param>
		/// <param name="output">A function that gets the property.</param>
		/// <param name="converter">A custom value converter to string.</param>
		/// <typeparam name="TData">The type of the property.</typeparam>
		public static void BindText<TData>(this BindingManager bindings, UILabel control, Func<TData> output, IDisplayDataConverter<string, TData> converter)
		{
			bindings.Bind (output, s => control.Text = converter.ConvertOutput(s));
		}

		/// <summary>
		/// Bind the Text of a UITextField to a string property.
		/// </summary>
		/// <param name="bindings">The binding manager.</param>
		/// <param name="control">The text field.</param>
		/// <param name="output">A function that gets the property.</param>
		/// <param name="input">A function that sets the property.</param>
		public static void BindText(this BindingManager bindings, UITextField control, Func<string> output, Action<string> input)
		{
			BindText (bindings, control, output, input, Identity.Instance);
		}

		/// <summary>
		/// Bind the Text of a UILabel to a read-only string property.
		/// </summary>
		/// <param name="bindings">The binding manager.</param>
		/// <param name="control">The label.</param>
		/// <param name="output">A function that gets the property.</param>
		public static void BindText(this BindingManager bindings, UILabel control, Func<string> output)
		{
			BindText (bindings, control, output, Identity.Instance);
		}

		/// <summary>
		/// Bind the Text of a UITextField to an int property.
		/// </summary>
		/// <param name="bindings">The binding manager.</param>
		/// <param name="control">The text field.</param>
		/// <param name="output">A function that gets the property.</param>
		/// <param name="input">A function that sets the property.</param>
		public static void BindText(this BindingManager bindings, UITextField control, Func<int> output, Action<int> input)
		{
			BindText (bindings, control, output, input, ConvertInt.Instance);
		}

		/// <summary>
		/// Bind the Text of a UILabel to a read-only int property.
		/// </summary>
		/// <param name="bindings">The binding manager.</param>
		/// <param name="control">The label.</param>
		/// <param name="output">A function that gets the property.</param>
		public static void BindText(this BindingManager bindings, UILabel control, Func<int> output)
		{
			BindText (bindings, control, output, ConvertInt.Instance);
		}
	}
}

