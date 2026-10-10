using ImageEditorControl.ImageReaders;
using ImageEditorControl.Models;
using ImageEditorControl.Tools;

using System.Windows.Media.Imaging;

namespace ImageEditorControl.Readers
{
	public interface IImageReader
	{
		List<PageSize> GetRawPages(byte[] fileBytes, ILogger logger);
		BitmapSource? GetBitmapSource(PageModel pm, bool forThumbnail);
		bool AddPages(ImageModel im, CancellationToken tocken);
	}
}