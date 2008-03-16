using System;

namespace Assisticant.Test
{
	public interface ISpouseViewModel
	{
		Person Spouse { get; }
		string FullName { get; }
		bool Equals(object obj);
		int GetHashCode();
	}
}
