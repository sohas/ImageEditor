using Docnet.Core;
using Docnet.Core.Models;
using ImageEditorControl.Models;
using ImageEditorControl.Tools;
using PDFiumCore;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageEditorControl.ImageReaders.Pdf
{
	internal class DocnetCoreRenderer
	{
		internal static BitmapSource? RenderPageFromBytes(PageModel pm, bool forThumbnail)
		{
			if (!(pm is not null && pm.ImageModelParent is ImageModel im && im.FileBytes is byte[] bytes && bytes.Length > 0))
			{
				return null;
			}

			var pdfBytes = bytes;

			try
			{
                                var scale = pm.DpiX / 72;

                                if (forThumbnail) 
				{
                                        var ths = ImageModel.ThumbnailSize.GetFitSize(pm.HeightPXLOrigin / pm.WidthPXLOrigin);
                                        scale *= ths.Width / pm.WidthPXLOrigin;
                                }

                                var dimensions = new PageDimensions(scale);
				using var docReader = DocLib.Instance.GetDocReader(pdfBytes, dimensions);
				using var pageReader = docReader.GetPageReader(pm.PageNumberInitial);
                                int width = pageReader.GetPageWidth();
                                int height = pageReader.GetPageHeight();

				byte[] rawPixels = forThumbnail ? pageReader.GetImage() : pageReader.GetImage(Docnet.Core.Models.RenderFlags.RenderAnnotations);
				var bitmapSource = new WriteableBitmap(width, height, pm.DpiX, pm.DpiX, PixelFormats.Bgra32, null);
				bitmapSource.Lock();

				try
				{
					Marshal.Copy(rawPixels, 0, bitmapSource.BackBuffer, rawPixels.Length);
					bitmapSource.AddDirtyRect(new Int32Rect(0, 0, width, height));
				}
				catch (Exception ex)
				{
					pm.ImageModelParent.Logger?.LogException(ex, "couldn't get bitmapSource");
				}
				finally
				{
					bitmapSource.Unlock();
				}

				bitmapSource.Freeze();
				return bitmapSource;
			}
			catch (Exception ex) 
			{
				pm.ImageModelParent.Logger?.LogException(ex, "couldn't get bitmapSource");
				return null;
			}
		}

		public static List<PageSize> GetPdfStructureFromBytes(byte[] fileBytes, ILogger logger)
		{
			var structureList = new List<PageSize>();

			try
			{
				using var docReader = DocLib.Instance.GetDocReader(fileBytes, new PageDimensions());
				int pageCount = docReader.GetPageCount();

				try
				{
					for (int i = 0; i < pageCount; i++)
					{
						using var pageReader = docReader.GetPageReader(i);
						int widthInPoints = pageReader.GetPageWidth();
						int heightInPoints = pageReader.GetPageHeight();

						structureList.Add(new PageSize() { PageIndex = i, PointsWidth = widthInPoints, PointsHeight = heightInPoints });
					}
				}
				catch (Exception ex)
				{
					logger?.LogException(new Exception("Couldn't get existing page"), "Couldn't get existing page");
					return structureList;
				}
			}
			catch (Exception ex)
			{
				logger?.LogException(new Exception("Couldn't load pdf"), "Couldn't load pdf");
				return structureList;
			}

			return structureList;
		}

		public static bool AddPages(ImageModel im, CancellationToken token)
		{
			if (im is null || im.FileBytes is not byte[] fileBytes || fileBytes.Length == 0)
			{
				return false;
			}

			try
			{
				using var docReader = DocLib.Instance.GetDocReader(fileBytes, new PageDimensions(1));
				int pageCount = docReader.GetPageCount();

				try
				{
					for (int i = 0; i < pageCount; i++)
					{
						if (token.IsCancellationRequested)
						{
							return false;
						}

						using var pageReader = docReader.GetPageReader(i);
						int pointsWidth = pageReader.GetPageWidth();
						int pointsHeight = pageReader.GetPageHeight();

                                                im.AddNewPage(new Size(pointsWidth, pointsHeight));
                                        }

                                        return true;
				}
				catch (Exception ex)
				{
					im.Logger?.LogException(new Exception("Couldn't get existing page"), "Couldn't get existing page");
					return false;
				}
			}
			catch (Exception ex)
			{
				im.Logger?.LogException(new Exception("Couldn't load pdf"), "Couldn't load pdf");
				return false;
			}
		}
	}
}