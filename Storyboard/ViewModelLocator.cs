using System.Collections.Generic;
using Assisticant;

namespace Storyboard
{
	public class ViewModelLocator : ViewModelLocatorBase
	{
		private readonly StoryboardParametersModel _storyboardParametersModel;

		public ViewModelLocator(IEnumerable<ImageFileInfo> imagesInfo)
		{
			_storyboardParametersModel = new StoryboardParametersModel(imagesInfo);
		}

		public object ViewModel => ViewModel(() => new StoryboardParametersViewModel(_storyboardParametersModel));
	}
}
