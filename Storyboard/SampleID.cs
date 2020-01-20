using System.Collections.Generic;
using System.Linq;

namespace Storyboard
{
	public class SampleID
	{
		public SampleID(string name, IEnumerable<ImageFileInfo> imageFilesInfo)
		{
			Name = name;
			ImageFilesInfo = imageFilesInfo;
		}

		public string Name { get; }
		public int ImageFilesCount => ImageFilesInfo.Count();

		public IEnumerable<ImageFileInfo> ImageFilesInfo { get; }

		public override string ToString() => Name;
	}
}