using ImageEditorControl.Models;
using ImageEditorControl.Readers;
using ImageEditorControl.Tools;
using System.Windows.Media.Imaging;

namespace ImageEditorControl.ImageReaders.Tiff
{
	public class ImageReaderTiff : IImageReader
	{
		static readonly TiffReaderType type = TiffReaderType.BitMiracle;

		public bool AddPages(ImageModel im, CancellationToken token) => type switch
		{
                        TiffReaderType.WindowsMedia => WindowsMediaRenderer.AddPages(im, token),
                        TiffReaderType.BitMiracle => BitMiracleRenderer.AddPages(im, token),
			_ => false,
		};

		public BitmapSource? GetBitmapSource(PageModel pm, bool forThumbnail) => type switch
		{
                        TiffReaderType.WindowsMedia => WindowsMediaRenderer.RenderPageFromBytes(pm, forThumbnail),
                        TiffReaderType.BitMiracle => BitMiracleRenderer.RenderPageFromBytes(pm, forThumbnail),
			_ => null,
		};

		public List<PageSize> GetRawPages(byte[] fileBytes, ILogger logger) => type switch
		{
                        TiffReaderType.WindowsMedia => WindowsMediaRenderer.GetTiffStructureFromBytes(fileBytes, logger),
                        TiffReaderType.BitMiracle => BitMiracleRenderer.GetTiffStructureFromBytes(fileBytes, logger),
			_ => new List<PageSize>(),
		};
	}

	enum TiffReaderType
	{
		WindowsMedia,
		BitMiracle,
		Uncknown,
	}
}