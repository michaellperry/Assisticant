using System;

namespace Assisticant.StoreApp.ViewModels
{
	public interface ISpouseViewModel
	{
		Person Spouse { get; }
		string FullName { get; }
		bool Equals(object obj);
		int GetHashCode();
	}
}
