using System.Windows.Media.Imaging;
using ImageEditorControl.Models.TiffAnnotations;
using ImageEditorControl.States;
using ImageEditorControl.Tools;
using ImageEditorControl.Models.PdfAnnotations;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Markup;

namespace ImageEditorControl.Models
{
	/// <summary>
	/// модель отображения и редактирования отдельной страницы файла изображения 
	/// </summary>
	public class PageModel : IDisposable, INotifyPropertyChanged
	{
		private readonly ImageModel imageModelParent;
		private readonly List<TiffAnnotationModel> tiffAnnotations = new List<TiffAnnotationModel>();
		private readonly List<StampModel> stamps = new List<StampModel>();
		private readonly List<TitleModel> titles = new List<TitleModel>();
		private readonly List<PdfAnnotationModel> pdfAnnotations = new List<PdfAnnotationModel>();
		private readonly ILogger logger;
		private readonly SynchronizationContext uiContext = SynchronizationContext.Current;

		private readonly int pageNumberInitial;
		private readonly bool isBWColorOrigin = false;
		private readonly bool isIndexedOrigin = false;
		private readonly double widthPXLOrigin;
		private readonly double heightPXLOrigin;
		private readonly double dpiXOrigin;
		private readonly double dpiYOrigin;
		//private readonly double dpiXThumbnail;
		//private readonly double dpiYThumbnail;
		private readonly double widthMMOrigin;
		private readonly double heightMMOrigin;
		private readonly PageViewState viewState = new PageViewState();
		private readonly object tiffAnnotationsLocker = new object();

		private bool isGenerating = false; //true когда строится миниатюра
		private bool thumbnailHasBeenCreated = false; //true когда уже построена миниатюра
		private bool thumbnailNeedToBeUpdated = false; //true когда миниатюру необходимо построить заново
		private bool disposedValue;
		private bool isDeleted;
		private bool isSelectedSingle;
		private bool isSelectedMultiple;
		private bool tiffAnnotationsHasBeenChanged;
		private int pageNumberCurrent;
		private Rect rectangleCrop = new Rect();
		private bool hasBeenCroped = false;
		private Rect rectangleSelectionInner = new Rect();
		private BitmapSource bitmapForView = null;
		private BitmapSource bitmapThumbnail = null;
		private PixelFormat? pixelFormatOrigin = null;
		private StampModel stampSelected = null;
		private TitleModel titleSelected = null;
		private TiffAnnotationModel tiffAnnotationSelected = null;
		private ColorizeType colorizeTypeChosen = ColorizeType.Colorize;
		private string pageText = null;

		public int PageNumberCurrent
		{
			get => pageNumberCurrent;
			set
			{
				pageNumberCurrent = value;
				OnPropertyChanged(NotifiedPropertyType.PageNumberCurrent);
			}
		}
		public string PageNumberDisplayedSimple => $"{(imageModelParent.IsExtra ? $"+{imageModelParent.ModelNumber}:" : "")}{PageNumberSorted + 1}";
		public string PageNumberDisplayedFull =>
			$"{(imageModelParent.IsExtra ? $"+{imageModelParent.ModelNumber}:" : "")}{PageNumberSorted + 1} ({pageNumberInitial + 1}) / " +
			$"{(imageModelParent.IsExtra ? imageModelParent.ImageModelParent.Count : imageModelParent.Count)}";
		public int PageNumberInitial => pageNumberInitial;
		public bool PageNumberHasBeenChanged => pageNumberCurrent != pageNumberInitial;
		public ImageModel ImageModelParent => imageModelParent;
		public bool IsBWColorOrigin => isBWColorOrigin;
		public bool IsIndexedOrigin => isIndexedOrigin;
		public double DpiX => dpiXOrigin;
		public double DpiY => dpiYOrigin;
		//public double DpiXThumbnail => dpiXThumbnail;
		//public double DpiYThumbnail => dpiYThumbnail;
		public double WidthPXLOrigin => widthPXLOrigin;
		public double HeightPXLOrigin => heightPXLOrigin;
		//для идентификации при клонировании
		public int TiffAnnotationIndex { get; set; } = 0;
		public int StampIndex { get; set; } = 0;
		public int TitleIndex { get; set; } = 0;
		public BitmapSource BitmapThumbnail => bitmapThumbnail;

		public void BuildBitmapThumbnail()
		{
			if (!(isGenerating || (thumbnailHasBeenCreated && !thumbnailNeedToBeUpdated)))
			{
				try
				{
					isGenerating = true;
					this.ReadPageForAction(image => SetBitmapThumbnail(image), true);
					thumbnailHasBeenCreated = true;
					thumbnailNeedToBeUpdated = false;
					//OnPropertyChanged(NotifiedPropertyType.BitmapThumbnail);//уже есть в SetBitmapThumbnail
				}
				catch (Exception ex)
				{
					logger?.LogException(ex, "Couldn't get BitmapThumbnail");
				}
				finally
				{
					isGenerating = false;
				}
			}
		}

		public BitmapSource BitmapForView
		{
			get
			{
				if (bitmapForView is null && !IsDisposing)
				{
					this.ReadPageForAction(image => SetBitmapForView(image), false);
					OnPropertyChanged(NotifiedPropertyType.BitmapForView);
				}

				return bitmapForView;
			}
		}

		public PixelFormat PixelFormatOrigin
		{
			get
			{
				if (pixelFormatOrigin is null)
				{
					this.ReadPageForAction(image => pixelFormatOrigin = image.Format, true);
				}

				return (PixelFormat)pixelFormatOrigin;
			}
		}

		public List<TiffAnnotationModel> TiffAnnotations
		{
			get
			{
				lock (tiffAnnotationsLocker)
				{
					return [.. tiffAnnotations.OrderBy(x => x.TiffAnnotationItem.OiIndex.GetParsedIndex())];
				}
			}
		}

		public List<StampModel> Stamps => stamps;
		public List<TitleModel> Titles => titles;
		public List<PdfAnnotationModel> PdfAnnotations => pdfAnnotations;
		public bool HasTiffAnnotations => tiffAnnotations.Any();
		public bool HasStamps => stamps.Any();
		public bool HasTitles => titles.Any();
		public bool HasPdfAnnotations => pdfAnnotations.Any();
		public bool TiffAnnotationsHasBeenChanged => tiffAnnotationsHasBeenChanged;
		public RotateType RotateType { get; set; } = RotateType.None;
		public FlipXType FlipXType { get; set; } = FlipXType.None;
		public FlipYType FlipYType { get; set; } = FlipYType.None;
		public RotateFlipType RotateFlipType => GetRotateFlipType(RotateType, FlipXType, FlipYType);
		public bool IsRotated => (int)RotateType % 2 == 1;
		public ExtraObjectsSaveType TiffAnnotationSaveTypeChosen { get; set; } = ExtraObjectsSaveType.Keep;
		public ExtraObjectsSaveType StampSaveTypeChosen { get; set; } = ExtraObjectsSaveType.Burn;
		public Pen GetPenStamp(Rect bounds)
		{
			var interval = (int)(Math.Min(bounds.Width, bounds.Height) / 100.0);
			interval = interval < 1 ? 1 : interval;
			return new Pen(Brushes.MediumBlue, 2) { DashStyle = new DashStyle(new double[] { interval, interval }, 0) };
		}

		public SolidColorBrush BrushStamp = new SolidColorBrush(Color.FromArgb(20, Colors.MediumBlue.R, Colors.MediumBlue.G, Colors.MediumBlue.B));
		public SolidColorBrush BrushTiffAnnotation = new SolidColorBrush(Color.FromArgb(20, Colors.OrangeRed.R, Colors.OrangeRed.G, Colors.OrangeRed.B));

		public ColorizeType ColorizeTypeChosen
		{
			get => colorizeTypeChosen;
			set
			{
				if (colorizeTypeChosen != value)
				{
					colorizeTypeChosen = value;
					thumbnailNeedToBeUpdated = true;
					OnPropertyChanged(NotifiedPropertyType.ColorizeTypeChosen);
					//_ = BuildForceBitmapThumbnailTask();
					OnPropertyChanged(NotifiedPropertyType.BitmapThumbnail);
				}
			}
		}

		public PageViewState ViewState => viewState;
		public bool ColorizeChanged => ColorizeTypeChosen != ColorizeType.Colorize;
		public bool HasBeenCroped => hasBeenCroped;
		public Rect RectangleCrop => rectangleCrop;
		public Rect RectangleSelectionInner => rectangleSelectionInner;
		public ImageFileFormat ImageFileFormatOrigin => imageModelParent.ImageFileFormatOrigin;
		public TiffAnnotationModel TiffAnnotationSelected => tiffAnnotationSelected;
		public StampModel StampSelected => stampSelected;
		public TitleModel TitleSelected => titleSelected;

		public bool IsSimplePdf =>
			ImageFileFormatOrigin == ImageFileFormat.Pdf &&
			ColorizeTypeChosen == ColorizeType.Colorize &&
			(int)RotateFlipType < 4 &&
			!(HasTiffAnnotations && TiffAnnotationSaveTypeChosen == ExtraObjectsSaveType.Burn) &&
			!(HasStamps && StampSaveTypeChosen == ExtraObjectsSaveType.Burn) &&
			!HasPdfAnnotations;

		public bool IsBig => WidthPXLOrigin * (long)HeightPXLOrigin > 10000000; //А4 с разрешением 300 дпи с небольшим запасом
		public bool IsPlainImage => imageModelParent.IsPlainImage;
		public int PageNumberSorted { get; set; } = 0;
		public bool Failed => ImageFileFormatOrigin == ImageFileFormat.Unknown;
		public bool IsSaving => imageModelParent.IsSaving;
		public bool IsDisposing => disposedValue;
		public bool PageTextHasBeenExtracted
		{
			get; private set;
		}

		public bool IsDeleted
		{
			get => isDeleted;
			set
			{
				isDeleted = value;
				OnPropertyChanged(NotifiedPropertyType.IsDeleted);
			}
		}


		/// <summary>
		/// свойство выбора страницы для отображения
		/// </summary>
		public bool IsSelectedSingle
		{
			get => isSelectedSingle;
			set
			{
				if (value != isSelectedSingle)
				{
					if (value)
					{
						imageModelParent.DeselectSingleAllPagesExceptThis(this);
					}
					else if (!(bitmapForView is null))
					{
						bitmapForView = null;
					}

					isSelectedSingle = value;
					OnPropertyChanged(NotifiedPropertyType.IsSelectedSingle);
				}
			}
		}

		/// <summary>
		/// свойство для свободного множественного выбора
		/// </summary>
		public bool IsSelectedMultiple
		{
			get => isSelectedMultiple;
			set
			{
				if (value != isSelectedMultiple)
				{
					isSelectedMultiple = value;
					OnPropertyChanged(NotifiedPropertyType.IsSelectedMultiple);
				}
			}
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		public void OnPropertyChanged(NotifiedPropertyType type) =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs($"{type}"));

                /// <summary>
                /// модель отображения и редактирования отдельной страницы файла изображения (pdf файлы)
                /// </summary>
                /// <param name="imageModel">модель файла, к которой относится страница</param>
                /// <param name="initialPageNumber">изначальный номер страницы в файле</param>
                /// <param name="pageSizeInPoins">размеры страницы в точках (1/72 дюйма)</param>
                /// <param name="pdfResolution">разрешение в пикселях на дюйм</param>
                /// <param name="pdfResolutionForThumbnail">разрешение в пикселях на дюйм для превьюшек</param>
                public PageModel(ImageModel imageModel, int initialPageNumber, Size pageSizeInPoins, int pdfResolution)
                        : this(imageModel, initialPageNumber)
                {
                        this.dpiXOrigin = pdfResolution;
                        this.dpiYOrigin = pdfResolution;
                        this.widthMMOrigin = pageSizeInPoins.Width * 25.4 / 72;
                        this.heightMMOrigin = pageSizeInPoins.Height * 25.4 / 72;
                        this.widthPXLOrigin = (int)(pageSizeInPoins.Width * dpiXOrigin / 72);
                        this.heightPXLOrigin = (int)(pageSizeInPoins.Height * dpiXOrigin / 72);
                        this.rectangleCrop = new Rect(0, 0, widthPXLOrigin, heightPXLOrigin);
                        this.isBWColorOrigin = false;
                }

                public PageModel(ImageModel imageModel, int initialPageNumber, Size pageSizeInPixel, double dpiX, double dpiY, PixelFormat format, BitmapPalette? palette)
		       : this(imageModel, initialPageNumber)
                {
                        this.dpiXOrigin = dpiX;
                        this.dpiYOrigin = dpiY;
                        this.widthPXLOrigin = pageSizeInPixel.Width;
                        this.heightPXLOrigin = pageSizeInPixel.Height;
                        this.widthMMOrigin = widthPXLOrigin / dpiX * 25.4;
                        this.heightMMOrigin = heightPXLOrigin / dpiY * 25.4;
                        this.rectangleCrop = new Rect(0, 0, widthPXLOrigin, heightPXLOrigin);
			this.pixelFormatOrigin = format;
                        this.isBWColorOrigin = format.IsBWColorViaWindowsMedia(palette, out bool isIndexed);
                }

                /// <summary>
                /// модель отображения и редактирования отдельной страницы файла изображения 
                /// </summary>
                /// <param name="imageModel">модель файла, к которой относится страница</param>
                /// <param name="initialPageNumber">изначальный номер страницы в файле</param>
                /// <param name="fullBitmap">битмап страницы файла полученного при сканировании</param>
                public PageModel(ImageModel imageModel, int initialPageNumber, BitmapSource fullBitmap)
			: this(imageModel, initialPageNumber)
		{
			this.widthPXLOrigin = fullBitmap.Width;
			this.heightPXLOrigin = fullBitmap.Height;
			this.rectangleCrop = new Rect(0, 0, widthPXLOrigin, heightPXLOrigin);
			this.dpiXOrigin = fullBitmap.DpiX;
			this.dpiYOrigin = fullBitmap.DpiY;
			this.widthMMOrigin = widthPXLOrigin * 25.4 / dpiXOrigin;
			this.heightMMOrigin = heightPXLOrigin * 25.4 / dpiYOrigin;
			this.isBWColorOrigin = fullBitmap.IsBWColorViaWindowsMedia(out isIndexedOrigin);
		}

		/// <summary>
		/// модель отображения и редактирования отдельной страницы файла изображения 
		/// </summary>
		/// <param name="imageModel">модель файла, к которой относится страница</param>
		/// <param name="initialPageNumber">изначальный номер страницы в файле</param>
		/// <param name="frame">битмап-фрейм страницы файла - для файлов, у которых есть декодеры в WindowsMedia (tiff, simple image)</param>
		public PageModel(ImageModel imageModel, int initialPageNumber, BitmapFrame frame)
			: this(imageModel, initialPageNumber)
		{
			this.widthPXLOrigin = frame.PixelWidth;
			this.heightPXLOrigin = frame.PixelHeight;
			this.rectangleCrop = new Rect(0, 0, widthPXLOrigin, heightPXLOrigin);
			this.dpiXOrigin = frame.DpiX;
			this.dpiYOrigin = frame.DpiY;
			this.widthMMOrigin = widthPXLOrigin * 25.4 / dpiXOrigin;
			this.heightMMOrigin = heightPXLOrigin * 25.4 / dpiYOrigin;
			this.isBWColorOrigin = frame.IsBWColorViaWindowsMedia(out isIndexedOrigin);
		}

		private PageModel(ImageModel imageModel, int initialPageNumber)
		{
			this.logger = imageModel.Logger;
			var pageNumberOffset = imageModel.IsExtra ? imageModel.ImageModelParent.Count : 0;
			this.imageModelParent = imageModel;
			this.pageNumberInitial = initialPageNumber;
			this.pageNumberCurrent = initialPageNumber + pageNumberOffset;
			this.imageModelParent.PagesByCurrentPageNumber[pageNumberCurrent] = this;
		}

		/// <summary>
		/// устанавливает текущий номер страницы
		/// </summary>
		/// <param name="pageNumber">номер страницы</param>
		/// <param name="clearDictionary">если True, очищаем словарь доступа к страницам по номеру (для полного удаления экстра-модели)</param>
		/// <param name="ignorePagesAllCount">если True, не проверяем общее число страниц (для ручного добавления экстра-модели)</param>
		public void SetCurrentPageNumber(int pageNumber, bool clearDictionary = false, bool ignorePagesAllCount = false)
		{
			if (pageNumber >= 0 && (ignorePagesAllCount || pageNumber < imageModelParent.PagesAllCount))
			{
				if (clearDictionary && imageModelParent.PagesByCurrentPageNumber.ContainsKey(pageNumberCurrent))
				{
					imageModelParent.PagesByCurrentPageNumber.Remove(pageNumberCurrent);
				}

				pageNumberCurrent = pageNumber;
				imageModelParent.PagesByCurrentPageNumber[pageNumber] = this;
			}
		}

		/// <summary>
		/// меняет местами с другой страницей
		/// </summary>
		/// <param name="nextPm"></param>
		public void SwapWithNextPageModel(PageModel nextPm)
		{
			var ci = pageNumberCurrent;
			var nci = nextPm.PageNumberCurrent;
			pageNumberCurrent = nci;
			nextPm.pageNumberCurrent = ci;

			if (ci >= 0 && ci < imageModelParent.PagesAllCount && nci >= 0 && nci < imageModelParent.PagesAllCount)
			{
				imageModelParent.PagesByCurrentPageNumber[ci] = nextPm;
				imageModelParent.PagesByCurrentPageNumber[nci] = this;
			}
		}

		/// <summary>
		/// поворот по часовой стрелке
		/// </summary>
		public void RotateR()
		{
			RotateType = (RotateType)(((int)RotateType + 1) % 4);
			OnPropertyChanged(NotifiedPropertyType.RotateFlipType);
		}

		/// <summary>
		/// поворот против часовой стрелке
		/// </summary>
		public void RotateL()
		{
			RotateType = (RotateType)(((int)RotateType + 3) % 4);
			OnPropertyChanged(NotifiedPropertyType.RotateFlipType);
		}

		/// <summary>
		/// отражение оригинала по горизонтали
		/// </summary>
		public void FlipX()
		{
			FlipXType = (FlipXType)(((int)FlipXType + 1) % 2);
			OnPropertyChanged(NotifiedPropertyType.RotateFlipType);
		}

		/// <summary>
		/// отражение оригинала по вертикали
		/// </summary>
		public void FlipY()
		{
			FlipYType = (FlipYType)(((int)FlipYType + 1) % 2);
			OnPropertyChanged(NotifiedPropertyType.RotateFlipType);
		}

		#region read
		/// <summary>
		/// устаналивает битмап для отображения страницы
		/// </summary>
		/// <param name="source">изображение-источник</param>
		public void SetBitmapForView(BitmapSource source)
		{
			if (source is null)
			{
				return;
			}

			bitmapForView = source;

			if (colorizeTypeChosen != ColorizeType.Colorize)
			{
				var grayBitmap = new FormatConvertedBitmap();
				grayBitmap.BeginInit();
				grayBitmap.Source = bitmapForView;
				grayBitmap.DestinationFormat = colorizeTypeChosen == ColorizeType.BW ? PixelFormats.BlackWhite : PixelFormats.Gray8;
				grayBitmap.EndInit();
			}

			bitmapForView.Freeze();
		}

		/// <summary>
		/// устаналивает битмап для отображения миниатюры страницы
		/// </summary>
		/// <param name="source">изображение-источник</param>
		public void SetBitmapThumbnail(BitmapSource source)
		{
			if (source is BitmapSource bitmapSource)
			{
				try
				{
					//var ths = ImageModel.ThumbnailSize.GetFitSize(heightMMOrigin / widthMMOrigin);
					//var kx = ths.Width / source.PixelWidth;
					//var ky = ths.Height / source.PixelHeight;
					//var transformedBitmap = new TransformedBitmap(source, new ScaleTransform(kx, ky));

					if (ColorizeTypeChosen != ColorizeType.Colorize)
					{
						FormatConvertedBitmap convertedBitmap = new FormatConvertedBitmap();
						convertedBitmap.BeginInit();
						convertedBitmap.Source = bitmapSource;
						convertedBitmap.DestinationFormat = ColorizeTypeChosen == ColorizeType.Gray ? PixelFormats.Gray8 : PixelFormats.BlackWhite;
						convertedBitmap.EndInit();
						convertedBitmap.Freeze();

						bitmapThumbnail = convertedBitmap;
					}
					else
					{
						bitmapThumbnail = bitmapSource;
					}

					bitmapThumbnail.Freeze();
				}
				catch (Exception ex)
				{
					bitmapThumbnail = null;
					logger.LogException(ex, "Couldn't get BitmapThumbnail");
				}
				finally 
				{
					OnPropertyChanged(NotifiedPropertyType.BitmapThumbnail);
				}
			}
		}

		/// <summary>
		/// Возвращает текст страницы (для Pdf-файлов)
		/// </summary>
		/// <returns></returns>
		public string ExtractPageText()
		{
			if (!PageTextHasBeenExtracted)
			{
				if (ImageFileFormatOrigin == ImageFileFormat.Pdf)
				{
					try
					{
						pageText = this.ExtractPagePdfText();
					}
					catch (Exception ex)
					{
						logger?.LogException(ex, "Impossible to extract page text");
					}
				}

				PageTextHasBeenExtracted = true;
			}

			return pageText;
		}
		#endregion

		#region draw
		private static RotateFlipType GetRotateFlipType(RotateType rt, FlipXType fx, FlipYType fy)
		{
			var r = (int)rt;
			var x = (int)fx;

			if (fy == FlipYType.FlipY)
			{
				r = (r + 2) % 4;
				x = (x + 1) % 2;
			}

			if (x == 1)
			{
				r = (4 - r) % 4 + 4;
			}

			return (RotateFlipType)r;
		}

		///// <summary>
		///// создаёт шаблонный пустой битмап для заполнения с размерами и ориентацией оригинала
		///// </summary>
		///// <returns>шаблонный пустой битмап для заполнения</returns>
		//public BitmapSource GetBitmapToFill()
		//{
		//	BitmapSource res = new Bitmap(WidthPXLOrigin, HeightPXLOrigin);
		//	res.SetResolution((float)DpiX, (float)DpiY);

		//	return res;
		//}


		private Size GetDrawSize(double innerWidth, double innerHeight, double innerVerticalResolution, double innerHorizontalResolution, Size panelSize, DrawSizeType drawSizeType, double scale = 1)
		{
			Size drawSize;

			switch (drawSizeType)
			{
				case DrawSizeType.FitToWidth:
					drawSize = new Size(panelSize.Width, (int)(panelSize.Width * innerHeight / (double)innerWidth));
					break;
				case DrawSizeType.FitToHeight:
					drawSize = new Size((int)(panelSize.Height * innerWidth / (double)innerHeight), panelSize.Height);
					break;
				case DrawSizeType.FitToWindow:
					drawSize = panelSize.GetFitSize(innerHeight / (double)innerWidth);
					break;
				case DrawSizeType.PhysicalSize:
					var monitorSizes = ScreenMesurer.GetPhysicalDisplaySize();
					var h = innerHeight * 25.4 / innerVerticalResolution / monitorSizes.HeightInMm * monitorSizes.PxlScreenHeight;
					var w = innerWidth * 25.4 / innerHorizontalResolution / monitorSizes.WidthInMm * monitorSizes.PxlScreenWidth;
					drawSize = new Size((int)w, (int)h);
					break;
				case DrawSizeType.Custom:
				default:
					drawSize = new Size((int)Math.Round(innerWidth * scale), (int)Math.Round(innerHeight * scale));
					break;
			}

			return drawSize;
		}

		/// <summary>
		/// вычисляет размер отображения внешне определённого битмапа отображения страницы в зависимости от размера панели отрисовки и типа масштабирования
		/// </summary>
		/// <param name="outerBitmap">внешне определённый битмап отображения страницы</param>
		/// <param name="panelSize">размер панели отрисовки</param>
		/// <param name="drawSizeType">тип масштабирования</param>
		/// <param name="scale">коэффициент масштабирования</param>
		/// <returns>размер отображения</returns>
		public Size GetDrawSizeForOuterBitmap(BitmapSource outerBitmap, Size panelSize, DrawSizeType drawSizeType, double scale = 1)
		{
			var innerWidth = outerBitmap.Width;
			var innerHeight = outerBitmap.Height;
			var innerVerticalResolution = outerBitmap.DpiY;
			var innerHorizontalResolution = outerBitmap.DpiX;

			return GetDrawSize(
				innerWidth, innerHeight, innerVerticalResolution, innerHorizontalResolution,
				panelSize, drawSizeType, scale);
		}

		/// <summary>
		/// вычисляет размер отображения страницы в зависимости от размера панели отрисовки и типа масштабирования
		/// </summary>
		/// <param name="panelSize">размер панели отрисовки</param>
		/// <param name="drawSizeType">тип масштабирования</param>
		/// <param name="scale">коэффициент масштабирования</param>
		/// <returns>размер отображения</returns>
		public Size GetDrawSizeForPage(Size panelSize, DrawSizeType drawSizeType, double scale = 1)
		{
			var innerWidth = IsRotated ? HeightPXLOrigin : WidthPXLOrigin;
			var innerHeight = IsRotated ? WidthPXLOrigin : HeightPXLOrigin;
			var innerVerticalResolution = IsRotated ? DpiX : DpiY;
			var innerHorizontalResolution = IsRotated ? DpiY : DpiX;

			return GetDrawSize(
				innerWidth, innerHeight, innerVerticalResolution, innerHorizontalResolution,
				panelSize, drawSizeType, scale);
		}

		/// <summary>
		/// вычисляет точку на странице по точке на панели отображения, величине сдвига и типа масштабирования
		/// </summary>
		/// <param name="pointOnPanel">точка на панели отрисовки</param>
		/// <param name="shift">сдвиг документа в панели отрисовки</param>
		/// <param name="panelSize">размер панели отрисовки</param>
		/// <param name="drawSizeType">тип масштабирования</param>
		/// <returns></returns>
		public Point GetPointOnPageFromPointOnPanel(Point pointOnPanel, Point shift, Size panelSize, DrawSizeType drawSizeType, double scale)
		{
			var p = new Point(pointOnPanel.X - shift.X, pointOnPanel.Y - shift.Y);
			var drawSize = GetDrawSizeForPage(panelSize, drawSizeType, scale);
			var k = heightPXLOrigin / (double)drawSize.Height;

			switch (RotateFlipType)
			{
				case RotateFlipType.Rotate90FlipNone:
					p = new Point(p.Y, drawSize.Width - p.X);
					k = heightPXLOrigin / (double)drawSize.Width;
					break;
				case RotateFlipType.Rotate180FlipNone:
					p = new Point(drawSize.Width - p.X, drawSize.Height - p.Y);
					break;
				case RotateFlipType.Rotate270FlipNone:
					p = new Point(drawSize.Height - p.Y, p.X);
					k = heightPXLOrigin / (double)drawSize.Width;
					break;
				case RotateFlipType.RotateNoneFlipX:
					p = new Point(drawSize.Width - p.X, p.Y);
					break;
				case RotateFlipType.Rotate90FlipX:
					p = new Point(p.Y, p.X);
					k = heightPXLOrigin / (double)drawSize.Width;
					break;
				case RotateFlipType.Rotate180FlipX:
					p = new Point(p.X, drawSize.Height - p.Y);
					break;
				case RotateFlipType.Rotate270FlipX:
					p = new Point(drawSize.Height - p.Y, drawSize.Width - p.X);
					k = heightPXLOrigin / (double)drawSize.Width;
					break;
				case RotateFlipType.RotateNoneFlipNone:
				default:
					break;
			}

			var res = new Point((int)(p.X * k), (int)(p.Y * k));

			return res;
		}
		#endregion

		#region selection & crop
		/// <summary>
		/// возвращает прямоугольник выделения на изображении исходного размера
		/// </summary>
		/// <param name="drawSize">размер отображения</param>
		/// <param name="shift">вектор сдвига в окне отображения</param>
		/// <param name="panelSelection">выделение в окне отображения</param>
		/// <returns>прямоугольник выделения на изображении исходного размера</returns>
		public Rect GetInnerSelectionRectangle(Size drawSize, Point shift, Rect panelSelection)
		{
			panelSelection = panelSelection.NormalizeRectangle();

			var x1 = panelSelection.Left - shift.X;
			var y1 = panelSelection.Top - shift.Y;
			var x2 = panelSelection.Right - shift.X;
			var y2 = panelSelection.Bottom - shift.Y;
			var w = Math.Abs(x1 - x2);
			var h = Math.Abs(y1 - y2);

			Point p = new Point(x1, y1);
			Size s = new Size(w, h);
			var k = HeightPXLOrigin / (double)drawSize.Height;

			switch (RotateFlipType)
			{
				case RotateFlipType.Rotate90FlipNone:
					p = new Point(y1, drawSize.Width - x2);
					s = new Size(h, w);
					k = HeightPXLOrigin / (double)drawSize.Width;
					break;
				case RotateFlipType.Rotate270FlipNone:
					p = new Point(drawSize.Height - y2, x1);
					s = new Size(h, w);
					k = HeightPXLOrigin / (double)drawSize.Width;
					break;
				case RotateFlipType.Rotate180FlipNone:
					p = new Point(drawSize.Width - x2, drawSize.Height - y2);
					break;
				case RotateFlipType.RotateNoneFlipX:
					p = new Point(drawSize.Width - x2, y1);
					break;
				case RotateFlipType.Rotate90FlipX:
					p = new Point(y1, x1);
					s = new Size(h, w);
					k = HeightPXLOrigin / (double)drawSize.Width;
					break;
				case RotateFlipType.Rotate180FlipX:
					p = new Point(x1, drawSize.Height - y2);
					break;
				case RotateFlipType.Rotate270FlipX:
					p = new Point(drawSize.Height - y2, drawSize.Width - x2);
					s = new Size(h, w);
					k = HeightPXLOrigin / (double)drawSize.Width;
					break;
				case RotateFlipType.RotateNoneFlipNone:
				default:
					break;
			}

			var res = new Rect((int)(p.X * k), (int)(p.Y * k), (int)(s.Width * k), (int)(s.Height * k)).NormalizeRectangle();

			var newX1 = res.Left;
			var newY1 = res.Top;
			var newX2 = res.Right;
			var newY2 = res.Bottom;

			newX1 = newX1 < 0 ? 0 : newX1 > WidthPXLOrigin ? WidthPXLOrigin : newX1;
			newX2 = newX2 < 0 ? 0 : newX2 > WidthPXLOrigin ? WidthPXLOrigin : newX2;
			newY1 = newY1 < 0 ? 0 : newY1 > HeightPXLOrigin ? HeightPXLOrigin : newY1;
			newY2 = newY2 < 0 ? 0 : newY2 > HeightPXLOrigin ? HeightPXLOrigin : newY2;

			return new Rect(newX1, newY1, newX2 - newX1, newY2 - newY1);
		}

		/// <summary>
		/// возвращает прямоугольник выделения на панели отображения
		/// </summary>
		/// <param name="drawSize">размер отображения</param>
		/// <param name="shift">вектор сдвига в окне отображения</param>
		/// <param name="rectangleToDraw">прямоугольник в координатах изображения, по умолчанию - rectangleSelectionInner</param>
		/// <returns>прямоугольник выделения на изображении исходного размера</returns>
		public Rect GetOuterSelectionRectangle(Size drawSize, Point shift, Rect? rectangleToDraw = null)
		{
			var rectangle = rectangleToDraw is null ? rectangleSelectionInner : (Rect)rectangleToDraw;

			var x1 = rectangle.Left;
			var y1 = rectangle.Top;
			var x2 = rectangle.Right;
			var y2 = rectangle.Bottom;
			var w = Math.Abs(x1 - x2);
			var h = Math.Abs(y1 - y2);

			Point p = new Point(x1, y1);
			Size s = new Size(w, h);
			var k = drawSize.Height / (double)HeightPXLOrigin;

			switch (RotateFlipType)
			{
				case RotateFlipType.Rotate90FlipNone:
					p = new Point(HeightPXLOrigin - y2, x1);
					s = new Size(h, w);
					k = drawSize.Width / (double)HeightPXLOrigin;
					break;
				case RotateFlipType.Rotate270FlipNone:
					p = new Point(y1, WidthPXLOrigin - x2);
					s = new Size(h, w);
					k = drawSize.Width / (double)HeightPXLOrigin;
					break;
				case RotateFlipType.Rotate180FlipNone:
					p = new Point(WidthPXLOrigin - x2, HeightPXLOrigin - y2);
					break;
				case RotateFlipType.RotateNoneFlipX:
					p = new Point(WidthPXLOrigin - x2, y1);
					break;
				case RotateFlipType.Rotate90FlipX:
					p = new Point(y1, x1);
					s = new Size(h, w);
					k = drawSize.Width / (double)HeightPXLOrigin;
					break;
				case RotateFlipType.Rotate180FlipX:
					p = new Point(x1, HeightPXLOrigin - y2);
					break;
				case RotateFlipType.Rotate270FlipX:
					p = new Point(HeightPXLOrigin - y2, WidthPXLOrigin - x2);
					s = new Size(h, w);
					k = drawSize.Width / (double)HeightPXLOrigin;
					break;
				case RotateFlipType.RotateNoneFlipNone:
				default:
					break;
			}

			var newPoint = new Point(p.X * k, p.Y * k).Plus(shift);
			var newSize = new Size(s.Width * k, s.Height * k);

			return new Rect(newPoint, newSize).NormalizeRectangle();
		}

		/// <summary>
		/// устанавливает прямоугольник для выделения на изображении исходного размера
		/// </summary>
		/// <param name="outerSize"></param>
		/// <param name="dst">тип масштабирования</param>
		/// <param name="shift">вектор сдвига в окне отображения</param>
		/// <param name="selection">выделение в окне отображения</param>
		public void SetInnerSelectionRectangle(Size outerSize, DrawSizeType dst, double scale, Point shift, Rect selection)
		{
			var drawSize = GetDrawSizeForPage(outerSize, dst, scale);
			rectangleSelectionInner = GetInnerSelectionRectangle(drawSize, shift, selection);
		}

		/// <summary>
		/// устанавливает прямоугольник для выделения напрямую (нужно для BuildCopy)
		/// </summary>
		/// <param name="savedInnerRectangle">прямоугольник для выделения</param>
		/// <returns>true если успешно</returns>
		public bool TrySetInnerRectangleDirectly(Rect savedInnerRectangle)
		{
			var res = savedInnerRectangle.Left >= 0 && savedInnerRectangle.Top >= 0 && savedInnerRectangle.Width <= WidthPXLOrigin && savedInnerRectangle.Height <= HeightPXLOrigin;

			if (res)
			{
				rectangleSelectionInner = savedInnerRectangle;
			}

			return res;
		}

		/// <summary>
		/// устанавливает прямоугольник для кропа на изображении исходного размера
		/// </summary>
		public void SetCropRectangle()
		{
			rectangleCrop = rectangleSelectionInner;
			hasBeenCroped = true;
		}

		/// <summary>
		/// устанавливает прямоугольник для кропа напрямую (нужно для undo/redo)
		/// </summary>
		/// <param name="savedCropRectangle">прямоугольник для кропа</param>
		/// <returns>true если успешно</returns>
		public bool TrySetCropRectangleDirectly(Rect savedCropRectangle)
		{
			var res = savedCropRectangle.Left >= 0 && savedCropRectangle.Top >= 0 && savedCropRectangle.Width <= WidthPXLOrigin && savedCropRectangle.Height <= HeightPXLOrigin;

			if (res)
			{
				rectangleCrop = savedCropRectangle;
				hasBeenCroped = savedCropRectangle != new Rect(0, 0, WidthPXLOrigin, HeightPXLOrigin);
			}

			return res;
		}

		/// <summary>
		/// сбрасывает кроп
		/// </summary>
		public void ClearCrop()
		{
			rectangleCrop = new Rect(0, 0, WidthPXLOrigin, HeightPXLOrigin);
			hasBeenCroped = false;
		}

		/// <summary>
		/// сбрасывает выделение
		/// </summary>
		public void ClearSelection() => rectangleSelectionInner = new Rect();
		#endregion

		#region TiffAnnotations
		/// <summary>
		/// устанавливает заметки при загрузке файла
		/// </summary>
		/// <param name="tiffAnnotations"></param>
		public void SetInitialTiffAnnotations(IEnumerable<TiffAnnotationItem> tiffAnnotations)
		{
			lock (tiffAnnotationsLocker)
			{
				foreach (var am in tiffAnnotations)
				{
					this.tiffAnnotations.Add(new TiffAnnotationModel(this, am));
				}
			}
		}

		/// <summary>
		/// добавляет заметку
		/// </summary>
		public void AddTiffAnnotation(TiffAnnotationModel am)
		{
			if (am is not null)
			{
				lock (tiffAnnotationsLocker)
				{
					tiffAnnotations.Add(am);
					tiffAnnotationsHasBeenChanged = true;
				}
			}
		}

		/// <summary>
		/// помечает заметку на удаление при записи модели в файл
		/// </summary>
		public bool RemoveTiffAnnotation(TiffAnnotationModel am)
		{
			var res = false;

			lock (tiffAnnotationsLocker)
			{
				res = tiffAnnotations.Remove(am);

				if (res)
				{
					tiffAnnotationsHasBeenChanged = true;
				}
			}

			return res;
		}

		/// <summary>
		/// убрать выделение всех аннотаций
		/// </summary>
		public void DeselectAllTiffAnnotations()
		{
			lock (tiffAnnotationsLocker)
			{
				foreach (var tam in tiffAnnotations)
				{
					tam.IsSelected = false;
				}
			}

			tiffAnnotationSelected = null;
		}

		/// <summary>
		/// установить заданную аннотацию выделенной
		/// </summary>
		/// <param name="am">аннотация</param>
		public void SetSelectedTiffAnnotation(TiffAnnotationModel am) => tiffAnnotationSelected = am;

		/// <summary>
		/// временный метод. адаптирует криво записанные заметки. подлежит удалению
		/// </summary>
		public void AdaptTiffAnnotations()
		{
			lock (tiffAnnotationsLocker)
			{
				foreach (var am in tiffAnnotations)
				{
					am.SetAlphaRgb1(255);
				}
			}
		}

		/// <summary>
		/// возращает подписи всех заметок на странице
		/// </summary>
		/// <returns>подписи всех заметок на странице</returns>
		public HashSet<string> GetTiffAnnotationOiGroups()
		{
			var res = new HashSet<string>();

			lock (tiffAnnotationsLocker)
			{
				foreach (var am in tiffAnnotations)
				{
					res.Add(am.TiffAnnotationItem.OiGroup);
				}
			}

			return res;
		}
		#endregion

		#region stamps
		private Rect GetStampRectangleOnOriginal(SetStampType type, Point locationOnPage, BitmapSource bm)
		{
			Rect res;
			var p = locationOnPage;
			var s = new Size();

			if (type == SetStampType.FullImage)
			{
				s = new Size(WidthPXLOrigin, HeightPXLOrigin);
			}
			else if (type == SetStampType.FitToImage)
			{
				var k = bm.Height / (double)bm.Width;
				var w = WidthPXLOrigin;
				var h = HeightPXLOrigin;

				if (IsRotated)
				{
					if (w / k > h)
					{
						w = (int)(HeightPXLOrigin * k);
					}
					else
					{
						h = (int)(WidthPXLOrigin / k);
					}
				}
				else
				{
					if (w * k > h)
					{
						w = (int)(HeightPXLOrigin / k);
					}
					else
					{
						h = (int)(WidthPXLOrigin * k);
					}
				}

				s = new Size(w, h);
			}
			else if (type == SetStampType.OnLocation)
			{
				var winch = bm.Width / bm.DpiX;
				var hinch = bm.Height / bm.DpiY;
				var w = (IsRotated ? hinch : winch) * this.DpiX;
				var h = (IsRotated ? winch : hinch) * this.DpiY;
				s = new Size(w, h);
			}

			switch (RotateFlipType)
			{
				case RotateFlipType.Rotate90FlipNone:
					p = new Point(p.X, p.Y - s.Height);
					break;
				case RotateFlipType.Rotate180FlipNone:
					p = new Point(p.X - s.Width, p.Y - s.Height);
					break;
				case RotateFlipType.Rotate270FlipNone:
					p = new Point(p.X - s.Width, p.Y);
					break;
				case RotateFlipType.RotateNoneFlipX:
					p = new Point(p.X - s.Width, p.Y);
					break;
				case RotateFlipType.Rotate180FlipX:
					p = new Point(p.X, p.Y - s.Height);
					break;
				case RotateFlipType.Rotate270FlipX:
					p = new Point(p.X - s.Width, p.Y - s.Height);
					break;
				case RotateFlipType.RotateNoneFlipNone:
				case RotateFlipType.Rotate90FlipX:
				default:
					break;
			}

			res = new Rect(p, s);

			return res;
		}

		/// <summary>
		/// устанавливает штамп в зависимости от типа штампа с заданным углом поворота
		/// </summary>
		/// <param name="bytesBitmap">битмап штампа</param>
		/// <param name="type">тип штампа</param>
		/// <param name="locationOnOriginal">координаты LT штампа на НЕ ПОВЁРНУТОМ изображении страницы (для простых штампов)</param>
		/// <param name="angle">угол поворота (для простых штампов)</param>
		/// <param name="makeTransparent">если true, при отрисовке к битмапу применять MakeTransparent()</param>
		public void SetStamp(byte[] bytesBitmap, SetStampType type, Point locationOnOriginal, double angle, bool makeTransparent = true)
		{
			//using var ms = new MemoryStream(bytesBitmap);
			//using var bm = new Bitmap(ms);
			//var h = bm.HorizontalResolution;
			//var v = bm.VerticalResolution;

			//if (makeTransparent)
			//{
			//	bm.MakeTransparent();
			//}

			//bm.SetResolution(h, v);//иначе разрешение "уезжает", хз почему

			//var bytes = (byte[])new ImageConverter().ConvertTo(bm, typeof(byte[]));
			//var stumpRectangle = GetStampRectangleOnOriginal(type, locationOnOriginal, bm);
			//var stamp = new StampModel(this, bytes, type, angle, stumpRectangle, RotateFlipType);

			//stamps.Add(stamp);
		}

		/// <summary>
		/// убрать выделение всех штампов
		/// </summary>
		public void DeselectAllStamps()
		{
			foreach (var stamp in Stamps.Where(x => x.IsSelected))
			{
				stamp.IsSelected = false;
			}

			stampSelected = null;
		}

		/// <summary>
		/// установить заданный штамп выделенным
		/// </summary>
		/// <param name="sm">штамп</param>
		public void SetSelectedStamp(StampModel sm) => stampSelected = sm;
		#endregion

		#region Titles
		/// <summary>
		/// убрать выделение всех надписей
		/// </summary>
		public void DeselectAllTitles()
		{
			foreach (var title in Titles.Where(x => x.IsSelected))
			{
				title.IsSelected = false;
			}

			titleSelected = null;
		}

		/// <summary>
		/// установить заданную надпись выделенной
		/// </summary>
		/// <param name="tm">надпись</param>
		public void SetSelectedTitle(TitleModel tm) => titleSelected = tm;

		/// <summary>
		/// устанавливает надпись поверх страницы
		/// </summary>
		/// <param name="text">текст надписи</param>
		/// <param name="horizontalLocation">горизонтальное расположение надписи</param>
		/// <param name="vericalLocation">вертикальное расположение надписи</param>
		/// <param name="font">шрифт надписи</param>
		/// <param name="color">цвет надписи</param>
		/// <param name="relativeHeight">высота надписи по отношению к диагонали страницы</param>
		public void SetTitle(string text, TitleHorizontalAlignment horizontalLocation, TitleVerticalAlignment vericalLocation,
			Font font = null, double? fontSize = null, Color? color = null, double? relativeHeight = null)
		{
			var title = new TitleModel(this, text, horizontalLocation, vericalLocation, font, color, relativeHeight);
			titles.Add(title);
		}
		#endregion

		#region PdfAnnotations
		public void SetInitialPdfAnnotations(IEnumerable<PdfAnnotationModel> pdfAnnotations)
		{
			foreach (var pdfAnnotation in pdfAnnotations)
			{
				this.pdfAnnotations.Add(pdfAnnotation);
			}
		}
		#endregion

		public void Dispose(bool disposing)
		{
			if (disposing && !disposedValue)
			{
				disposedValue = true;
			}
		}

		void IDisposable.Dispose()
		{
			//Dispose(disposing: true);
			//GC.SuppressFinalize(this);
		}
	}

	public enum DrawSizeType
	{
		Custom,
		PhysicalSize,
		FitToWindow,
		FitToWidth,
		FitToHeight,
	}

	public enum ColorizeType
	{
		Colorize,
		Gray,
		BW,
	}

	public enum RotateType
	{
		None = 0,
		ToRight90 = 1,
		UpDown = 2,
		ToLeft90 = 3,
	}

	public enum FlipXType
	{
		None = 0,
		FlipX = 1,
	}

	public enum FlipYType
	{
		None = 0,
		FlipY = 1,
	}

	public enum RotateFlipType
	{
		RotateNoneFlipNone = 0,
		Rotate90FlipNone = 1,
		Rotate180FlipNone = 2,
		Rotate270FlipNone = 3,
		RotateNoneFlipX = 4,
		Rotate90FlipX = 5,
		Rotate180FlipX = 6,
		Rotate270FlipX = 7,
		RotateNoneFlipY = 6,
		Rotate90FlipY = 7,
		Rotate180FlipY = 4,
		Rotate270FlipY = 5,
		RotateNoneFlipXY = 2,
		Rotate90FlipXY = 3,
		Rotate180FlipXY = 0,
		Rotate270FlipXY = 1,
	}

	public enum ExtraObjectsSaveType
	{
		Keep = 0,
		Burn = 1,
		Lose = 2,
	}

	public enum SetStampType
	{
		OnLocation,
		FullImage,
		FitToImage,
	}

	public enum TitleVerticalAlignment
	{
		Top,
		Middle,
		Bottom,
		DiagonalLTRB,
		DiagonalLBRT,
	}

	public enum TitleHorizontalAlignment
	{
		Left,
		Centre,
		Multiple,
		Stretched,
	}

	public enum PrintArea
	{
		Selection,
		Page,
	}

	public enum PrintOrientation
	{
		Auto,
		Landscape,
		Portrait,
	}
}