using Assisticant.Fields;

namespace Storyboard
{
	/// <summary>
	/// Holds information about how to define a sample ID.
	/// </summary>
	internal class SampleIDDefinitionModel
	{
		private Observable<SampleIDDefinitionMode> _sampleIDDefinitionMode = new Observable<SampleIDDefinitionMode>(StoryboardSpecDefaults.SampleIDDefinitionMode);
		public SampleIDDefinitionMode DefinitionMode
		{
			get => _sampleIDDefinitionMode.Value;
			set => _sampleIDDefinitionMode.Value = value;
		}

		private Observable<int> _sampleIDNumCharacters = new Observable<int>(StoryboardSpecDefaults.SampleIDNumCharacters);
		public int SampleIDNumCharacters
		{
			get => _sampleIDNumCharacters.Value;
			set => _sampleIDNumCharacters.Value = value;
		}
	}
}