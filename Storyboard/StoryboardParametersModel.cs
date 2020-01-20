using System;
using System.Collections.Generic;
using System.Linq;
using Assisticant.Collections;
using Assisticant.Fields;

namespace Storyboard
{
	internal class StoryboardParametersModel
	{
		private readonly IEnumerable<ImageFileInfo> _imagesInfo;

		private readonly SampleIDDefinitionModel _sampleIDDefinitionModel;

		public StoryboardParametersModel(IEnumerable<ImageFileInfo> imagesInfo)
		{
			_imagesInfo = imagesInfo;
			_sampleIDDefinitionModel = new SampleIDDefinitionModel();

			_allSampleIDs = new Computed<IEnumerable<SampleID>>(() =>
				ComputeAllSampleIDs(_sampleIDDefinitionModel));
			_selectedSampleIDs = new ObservableList<SampleID>(_allSampleIDs.Value);
			// when the list of all sample IDs changes, change the _selectedSampleIDs, so that by default all the items
			// in the CheckedComboBox get selected in UI. This is a user requirement specification for our case.
			_allSampleIDs.Subscribe(s =>
			{
				_selectedSampleIDs.Clear();
				_selectedSampleIDs.AddRange(_allSampleIDs.Value);
			});
		}

		public SampleIDDefinitionModel SampleIDDefinitionModel => _sampleIDDefinitionModel;

		private Computed<IEnumerable<SampleID>> _allSampleIDs;
		public IEnumerable<SampleID> AllSampleIDs => _allSampleIDs.Value;

		private ObservableList<SampleID> _selectedSampleIDs;
		public IList<SampleID> SelectedSampleIDs => _selectedSampleIDs;

		private IEnumerable<SampleID> ComputeAllSampleIDs(SampleIDDefinitionModel model)
		{
			var result = new List<SampleID>();
			foreach (var item in _imagesInfo)
			{
				var sampleIDName = string.Empty;
				switch (model.DefinitionMode)
				{
					case SampleIDDefinitionMode.FirstXNumberOfCharacters:
						sampleIDName = item.Name.Substring(0, model.SampleIDNumCharacters);
						break;
					case SampleIDDefinitionMode.CharactersBeforeUnderline:
						sampleIDName = item.Name
							.Substring(0,
								item.Name.IndexOf("_") == -1
									? item.Name.Length
									: item.Name.IndexOf("_")
							);
						break;
					default:
						throw new NotImplementedException();
				}
				if (result.Any(x => x.Name == sampleIDName))
				{
					continue;
				}
				var imageFilesInfo = _imagesInfo
					.Where(x => FilenameMatchesThisSampleID(model, x.Path, sampleIDName))
					.ToArray();
				result.Add(new SampleID(sampleIDName, imageFilesInfo));
			}
			return result;
		}

		private bool FilenameMatchesThisSampleID(SampleIDDefinitionModel definitionModel, string filename, string sampleID)
		{
			switch (definitionModel.DefinitionMode)
			{
				case SampleIDDefinitionMode.FirstXNumberOfCharacters:
					return filename.Substring(0, definitionModel.SampleIDNumCharacters).Equals(sampleID);
				case SampleIDDefinitionMode.CharactersBeforeUnderline:
					var firstUnderlineCharIndex = filename.IndexOf("_");
					return firstUnderlineCharIndex != -1
						? filename.Substring(0, firstUnderlineCharIndex).Equals(sampleID)
						: false;
				default:
					throw new NotImplementedException();
			}
		}
	}
}