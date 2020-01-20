using System;
using System.Windows;

namespace Storyboard
{
	public class ImageFileInfo
	{
		public string Path { get; set; }
		public string Name { get; set; }
		public Size Size { get; set; }
		public int NumberOfBands { get; set; }
		public DateTime? CaptureDate { get; set; }
		public ImageDataFormat DataFormat { get; set; }
		public override string ToString()
		{
			return Name;
		}
	}

	public enum ImageDataFormat
	{
		/// <summary>Pixels are float format.</summary>
		FloatPixel,
		/// <summary>Pixels are byte format.</summary>
		BytePixel,
		/// <summary>
		/// Format is interlaced byte pixels. Eg. R-G-B-R-G-B-R-G-B-...
		/// </summary>
		ByteRGBPixel,
		/// <summary>Pixels are ushort format.</summary>
		Int16Pixel,
	}
}