using Docnet.Core.Readers;
using ImageEditorControl.Models;
using ImageEditorControl.Tools;
using PDFiumCore;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageEditorControl.ImageReaders.Pdf
{
	internal static class PdfiumCoreRenderer
	{
		private static readonly object renderLocker = new object();

		internal static BitmapSource? RenderPageFromBytes(PageModel pm, bool forThumbnail)
		{
			if (!(pm is not null && pm.ImageModelParent is ImageModel im && im.FileBytes is byte[] bytes && bytes.Length > 0))
			{
				return null;
			}

			var pdfBytes = bytes;

			if (!PdfiumCoreLifecycleManager.IsInitialized)
			{
				PdfiumCoreLifecycleManager.Initialize();
			}

			lock (renderLocker)
			{
				// 1. Закрепляем (Pin) массив байт в управляемой памяти для C++
				GCHandle pinnedArray = GCHandle.Alloc(pdfBytes, GCHandleType.Pinned);
				nint bufferAddr = pinnedArray.AddrOfPinnedObject();

				try
				{
					// 2. ИСПОЛЬЗУЕМ VAR: Загружаем PDF напрямую из закрепленного участка памяти
					var docHandle = fpdfview.FPDF_LoadMemDocument(bufferAddr, pdfBytes.Length, null);
					// Проверяем нативный указатель. Если документ не загрузился, PDFium вернет пустой хэндл
					if (docHandle == null || docHandle.Equals(default) || docHandle.Equals(nint.Zero))
					{
						pm.ImageModelParent.Logger?.LogException(new Exception("Couldn't download PDF from memory"), "Couldn't download PDF from memory");
						return null;
					}

					// Загружаем хэндл страницы
					var pageHandle = fpdfview.FPDF_LoadPage(docHandle, pm.PageNumberInitial);
					
					if (pageHandle == null || pageHandle.Equals(default) || pageHandle.Equals(nint.Zero))
					{
						fpdfview.FPDF_CloseDocument(docHandle);
						pm.ImageModelParent.Logger?.LogException(new Exception("Couldn't download PDF page"), "Couldn't download PDF page");
						return null;
					}

					try
					{
						double widthInInch = fpdfview.FPDF_GetPageWidth(pageHandle)/72;
						double heightInInch = fpdfview.FPDF_GetPageHeight(pageHandle)/72;
                                                var ths = ImageModel.ThumbnailSize.GetFitSize(heightInInch / widthInInch);
                                                int width = (int)(forThumbnail ? ths.Width : widthInInch * pm.DpiX);
                                                int height = (int)(forThumbnail ? ths.Height : heightInInch * pm.DpiY);

                                                var writeableBitmap = new WriteableBitmap(
						    width, height,
                                                    pm.DpiX, pm.DpiY,
						    PixelFormats.Bgra32, null
						);

						writeableBitmap.Lock();
						try
						{
							nint backBuffer = writeableBitmap.BackBuffer;
							// Создаем нативный контекст битмапа вокруг памяти WPF (формат 4 = BGRA)
							var fpdfBitmap = fpdfview.FPDFBitmapCreateEx(width, height, 4, backBuffer, width * 4);
							// Белая подложка, чтобы прозрачные штампы не рендерились на черном фоне
							fpdfview.FPDFBitmapFillRect(fpdfBitmap, 0, 0, width, height, 0xFFFFFFFF);
							// Константа 0x01 (FPDF_ANNOT) — принудительный рендеринг штампов и правок
							int renderFlags = 0x01;
							fpdfview.FPDF_RenderPageBitmap(fpdfBitmap, pageHandle, 0, 0, width, height, 0, renderFlags);
							// Уничтожаем нативный мост битмапа (память BackBuffer при этом сохраняется)
							fpdfview.FPDFBitmapDestroy(fpdfBitmap);
							// Уведомляем WPF, что буфер обновлен
							writeableBitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
						}
						finally
						{
							writeableBitmap.Unlock();
						}

						writeableBitmap.Freeze();
						return writeableBitmap;
					}
					catch (Exception ex)
					{
						pm.ImageModelParent.Logger?.LogException(ex, "couldn't make bitmapSource");
						return null;
					}
					finally
					{
						fpdfview.FPDF_ClosePage(pageHandle);
						fpdfview.FPDF_CloseDocument(docHandle);
					}
				}
				catch (Exception ex)
				{
					pm.ImageModelParent.Logger?.LogException(ex, "couldn't make bitmapSource");
					return null;
				}
				finally
				{
					// И только в самом конце отпускаем массив байт в сборщик мусора .NET
					if (pinnedArray.IsAllocated)
					{
						pinnedArray.Free();
					}
				}
			}
		}

		public static List<PageSize> GetPdfStructureFromBytes(byte[] fileBytes, ILogger logger)
		{
			var structureList = new List<PageSize>();

			if (fileBytes == null || fileBytes.Length == 0)
			{
				return structureList;
			}

			var pdfBytes = fileBytes;

			if (!PdfiumCoreLifecycleManager.IsInitialized)
			{
				PdfiumCoreLifecycleManager.Initialize();
			}

			lock (renderLocker)
			{
				// Закрепляем (Pin) массив байт в памяти .NET для безопасного чтения из C++
				GCHandle pinnedArray = GCHandle.Alloc(pdfBytes, GCHandleType.Pinned);
				nint bufferAddr = pinnedArray.AddrOfPinnedObject();

				try
				{
					// Загружаем документ напрямую из закрепленного участка памяти
					var docHandle = fpdfview.FPDF_LoadMemDocument(bufferAddr, pdfBytes.Length, null);

					if (docHandle == null || docHandle.Equals(default) || docHandle.Equals(nint.Zero))
					{
						logger?.LogException(new Exception("Couldn't download PDF from bytes"), "Couldn't download PDF from bytes");
						return structureList;
					}

					try
					{
						int pageCount = fpdfview.FPDF_GetPageCount(docHandle);
						double pointsWidth = 0;
						double pointsHeight = 0;

						for (int i = 0; i < pageCount; i++)
						{
							int success = fpdfview.FPDF_GetPageSizeByIndex(docHandle, i, ref pointsWidth, ref pointsHeight);
							structureList.Add(new PageSize
							{
								PageIndex = i,
								PointsWidth = pointsWidth,
								PointsHeight = pointsHeight,
								Error = success == 0,
							});
						}
					}
					catch (Exception ex)
					{
						logger?.LogException(new Exception("Couldn't get existing page"), "Couldn't get existing page");
						return structureList;
					}
					finally
					{
						// Обязательно закрываем хэндл документа на стороне C++
						fpdfview.FPDF_CloseDocument(docHandle);
					}
				}
				catch (Exception ex)
				{
					logger?.LogException(new Exception("Couldn't load pdf"), "Couldn't load pdf");
					return structureList;
				}
				finally
				{
					// Только после закрытия нативного документа отпускаем массив байт в GC .NET
					if (pinnedArray.IsAllocated)
					{
						pinnedArray.Free();
					}
				}
			}

			return structureList;
		}

		public static bool AddPages(ImageModel im, CancellationToken token)
		{
			if (im is null || im.FileBytes is not byte[] fileBytes || fileBytes.Length == 0)
			{
				return false;
			}

			var pdfBytes = fileBytes;

			if (!PdfiumCoreLifecycleManager.IsInitialized)
			{
				PdfiumCoreLifecycleManager.Initialize();
			}

			lock (renderLocker)
			{
				// Закрепляем (Pin) массив байт в памяти .NET для безопасного чтения из C++
				GCHandle pinnedArray = GCHandle.Alloc(pdfBytes, GCHandleType.Pinned);
				nint bufferAddr = pinnedArray.AddrOfPinnedObject();

				try
				{
					// Загружаем документ напрямую из закрепленного участка памяти
					var docHandle = fpdfview.FPDF_LoadMemDocument(bufferAddr, pdfBytes.Length, null);

					if (docHandle == null || docHandle.Equals(default) || docHandle.Equals(nint.Zero))
					{
						im.Logger?.LogException(new Exception("Couldn't download PDF from bytes"), "Couldn't download PDF from bytes");
						return false;
					}

					try
					{
						int pageCount = fpdfview.FPDF_GetPageCount(docHandle);

						for (int i = 0; i < pageCount; i++)
						{
							double pointsWidth = 0;
							double pointsHeight = 0;

							if (token.IsCancellationRequested)
							{
								return false;
							}

							if (fpdfview.FPDF_GetPageSizeByIndex(docHandle, i, ref pointsWidth, ref pointsHeight) != 0)
							{
								im.AddNewPage(new Size(pointsWidth, pointsHeight));
							}
							else
							{
								im.Logger?.LogException(new Exception("Couldn't get existing page"), "Couldn't get existing page");
								return false;
							}
						}
					}
					catch (Exception ex)
					{
						im.Logger?.LogException(new Exception("Couldn't get existing page"), "Couldn't get existing page");
						return false;
					}
					finally
					{
						// Обязательно закрываем хэндл документа на стороне C++
						fpdfview.FPDF_CloseDocument(docHandle);
					}
				}
				catch (Exception ex)
				{
					im.Logger?.LogException(new Exception("Couldn't load pdf"), "Couldn't load pdf");
					return false;
				}
				finally
				{
					// Только после закрытия нативного документа отпускаем массив байт в GC .NET
					if (pinnedArray.IsAllocated)
					{
						pinnedArray.Free();
					}
				}

				return false;
			}
		}
	}
}