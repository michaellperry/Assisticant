using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Assisticant;
using Xceed.Wpf.Toolkit.Primitives;

namespace Storyboard
{
	/// <summary>
	/// Interaction logic for StoryboardOutputParametersControl.xaml
	/// </summary>
	public partial class StoryboardParametersControl : UserControl
	{
		#region InputPath

		public string InputPath
		{
			get { return (string)GetValue(InputPathProperty); }
			set { SetValue(InputPathProperty, value); }
		}

		// Using a DependencyProperty as the backing store for InputPath.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty InputPathProperty =
			DependencyProperty.Register("InputPath", typeof(string), typeof(StoryboardParametersControl), new PropertyMetadata(null));

		#endregion

		#region IncludeSubfolders

		public bool IncludeSubfolders
		{
			get { return (bool)GetValue(IncludeSubfoldersProperty); }
			set { SetValue(IncludeSubfoldersProperty, value); }
		}

		// Using a DependencyProperty as the backing store for IncludeSubfolders.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty IncludeSubfoldersProperty =
			DependencyProperty.Register("IncludeSubfolders", typeof(bool), typeof(StoryboardParametersControl), new PropertyMetadata(false));

		#endregion

		private ViewModelLocator _viewModelLocator;
		private object _viewModel;

		public StoryboardParametersControl()
		{
			InitializeComponent();
			_viewModelLocator = new ViewModelLocator(GetImagesInfo());
			_viewModel = _viewModelLocator.ViewModel;
			DataContext = _viewModel;
		}

		private IEnumerable<ImageFileInfo> GetImagesInfo()
		{
			var result = new List<ImageFileInfo>();

			var size = new Size(1,1);
			var numBands = 1;

			result.Add(new ImageFileInfo
			{
				Name = "A shaked_0.hips",
				NumberOfBands = numBands,
				Path = "A shaked_0.hips",
				Size = size,
				DataFormat = ImageDataFormat.BytePixel
			});
			result.Add(new ImageFileInfo
			{
				Name = "A shaken_0.hips",
				NumberOfBands = numBands,
				Path = "A shaken_0.hips",
				Size = size,
				DataFormat = ImageDataFormat.BytePixel
			});
			result.Add(new ImageFileInfo
			{
				Name = "A shaking_1.hips",
				NumberOfBands = numBands,
				Path = "A shaking_1.hips",
				Size = size,
				DataFormat = ImageDataFormat.BytePixel
			});
			result.Add(new ImageFileInfo
			{
				Name = "ProductA_0.hips",
				NumberOfBands = numBands,
				Path = "ProductA_0.hips",
				Size = size,
				DataFormat = ImageDataFormat.BytePixel
			});
			result.Add(new ImageFileInfo
			{
				Name = "ProductA_1.hips",
				NumberOfBands = numBands,
				Path = "ProductA_1.hips",
				Size = size,
				DataFormat = ImageDataFormat.BytePixel
			});
			result.Add(new ImageFileInfo
			{
				Name = "ProductA_2.hips",
				NumberOfBands = numBands,
				Path = "ProductA_2.hips",
				Size = size,
				DataFormat = ImageDataFormat.BytePixel
			});
			result.Add(new ImageFileInfo
			{
				Name = "ProductB_0.hips",
				NumberOfBands = numBands,
				Path = "ProductB_0.hips",
				Size = size,
				DataFormat = ImageDataFormat.BytePixel
			});
			result.Add(new ImageFileInfo
			{
				Name = "ProductB_1.hips",
				NumberOfBands = numBands,
				Path = "ProductB_1.hips",
				Size = size,
				DataFormat = ImageDataFormat.BytePixel
			});
			result.Add(new ImageFileInfo
			{
				Name = "ProductB_2.hips",
				NumberOfBands = numBands,
				Path = "ProductB_2.hips",
				Size = size,
				DataFormat = ImageDataFormat.BytePixel
			});

			return result;
		}

	}
}
