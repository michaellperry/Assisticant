using System;
using System.Windows;
using System.Windows.Data;

namespace Storyboard
{
	/// <summary>
	/// Useful for binding IsChecked property of a number of radio buttons to an enumeration.
	/// In this way, view model will be more readable, and we don't need to have a separate
	/// boolean for each radio button's IsChecked property.
	/// </summary>
	/// <seealso cref="System.Windows.Data.IValueConverter" />
	public class EnumBooleanConverter : IValueConverter
	{
		#region IValueConverter Members
		/// <summary>
		/// This method is called when bound property in the data context is changed, and UI needs to update its state.
		/// Returns true if value of the binding source (member of an enumeration)
		/// is equal to the used converter parameter specified in xaml (name of a member of an enumeration).
		/// </summary>
		/// <param name="value">The value produced by the binding source.</param>
		/// <param name="targetType">The type of the binding target property (which happens to be an enumeration).</param>
		/// <param name="parameter">The converter parameter to use.</param>
		/// <param name="culture">The culture to use in the converter.</param>
		/// <returns>
		/// A converted value. If the method returns <see langword="null" />, the valid null value is used.
		/// </returns>
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			string parameterString = parameter as string;
			if (parameterString == null)
				return DependencyProperty.UnsetValue;

			if (Enum.IsDefined(value.GetType(), value) == false)
				return DependencyProperty.UnsetValue;

			object parameterValue = Enum.Parse(value.GetType(), parameterString);

			return parameterValue.Equals(value);
		}

		/// <summary>
		/// This method is called when UI state is changed, and the bound property value needs to be updated.
		/// </summary>
		/// <param name="value">The value that is produced by the binding target.</param>
		/// <param name="targetType">The type to convert to.</param>
		/// <param name="parameter">The converter parameter to use.</param>
		/// <param name="culture">The culture to use in the converter.</param>
		/// <returns>
		/// A converted value. If the method returns <see langword="null" />, the valid null value is used.
		/// </returns>
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			string parameterString = parameter as string;
			if (parameterString == null)
				return DependencyProperty.UnsetValue;

			return Enum.Parse(targetType, parameterString);
		}
		#endregion
	}
}
