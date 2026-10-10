using ImageEditorControl.Models;
using ImageEditorControl.Readers;
using ImageEditorControl.Tools;
using System.Windows.Media.Imaging;

namespace ImageEditorControl.ImageReaders.Pdf
{
	public class ImageReaderPdf : IImageReader
	{
		static readonly PdfReaderType type = PdfReaderType.PdfiumCore;

		public bool AddPages(ImageModel im, CancellationToken token) => type switch
		{
			PdfReaderType.PdfiumCore => PdfiumCoreRenderer.AddPages(im, token),
			PdfReaderType.DocnetCore => DocnetCoreRenderer.AddPages(im, token),
			_ => false,
		};

		public BitmapSource? GetBitmapSource(PageModel pm, bool forThumbnail) => type switch
		{
			PdfReaderType.PdfiumCore => PdfiumCoreRenderer.RenderPageFromBytes(pm, forThumbnail),
			PdfReaderType.DocnetCore => DocnetCoreRenderer.RenderPageFromBytes(pm, forThumbnail),
			_ => null,
		};

		public List<PageSize> GetRawPages(byte[] fileBytes, ILogger logger) => type switch
		{
			PdfReaderType.PdfiumCore => PdfiumCoreRenderer.GetPdfStructureFromBytes(fileBytes, logger),
			PdfReaderType.DocnetCore => DocnetCoreRenderer.GetPdfStructureFromBytes(fileBytes, logger),
			_ => new List<PageSize>(),
		};
	}

	enum PdfReaderType
	{
		PdfiumCore,
		DocnetCore,
		Uncknown,
	}
}