using ImageEditorControl.Models;

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using ImageEditorControl.Models.TiffAnnotations;
using PdfSharpCore.Drawing;

namespace ImageEditorControl.UIs
{
	public class StaticImageComposer : FrameworkElement
	{
		// Карта геометрий для будущего Hit Test (кликов) в масштабе 1:1
		protected readonly Dictionary<Geometry, TiffAnnotationModel> GeometryMap = new Dictionary<Geometry, TiffAnnotationModel>();

		public static readonly DependencyProperty ViewModelProperty =
		    DependencyProperty.Register(
			nameof(ViewModel),
			typeof(PageModel),
			typeof(StaticImageComposer),
			new PropertyMetadata(null, OnViewModelChanged));

		public PageModel ViewModel
		{
			get => (PageModel)GetValue(ViewModelProperty);
			set => SetValue(ViewModelProperty, value);
		}

		public StaticImageComposer()
		{
			// Разрешаем элементу принимать фокус для интерактивности
			Focusable = true;
			ClipToBounds = true;
		}

		private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var control = (StaticImageComposer)d;

			if (e.OldValue is PageModel oldModel)
			{
				oldModel.PropertyChanged -= control.OnModelPropertyChanged;
				//if (oldModel.TiffAnnotations != null)
				//	oldModel.TiffAnnotations.CollectionChanged -= control.OnItemsCollectionChanged;
			}

			if (e.NewValue is PageModel newModel)
			{
				newModel.PropertyChanged += control.OnModelPropertyChanged;
				//if (newModel.TiffAnnotations != null)
				//	newModel.TiffAnnotations.CollectionChanged += control.OnItemsCollectionChanged;

				control.SubscribeToItems();
			}

			control.RefreshScene();
		}

		private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(PageModel.BitmapForView))
			{
				// Если изменился базовый битмап, нужно пересчитать размеры самого элемента
				InvalidateMeasure();
			}
			RefreshScene();
		}

		private void OnItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			SubscribeToItems();
			RefreshScene();
		}

		private void SubscribeToItems()
		{
			if (ViewModel?.TiffAnnotations == null)
				return;
			foreach (var item in ViewModel.TiffAnnotations)
			{
				//item.PropertyChanged -= OnItemPropertyChanged;
				//item.PropertyChanged += OnItemPropertyChanged;
			}
		}

		private void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e) => RefreshScene();

		// Заставляем систему выделить элементу ровно столько пикселей, сколько занимает BitmapForView
		protected override Size MeasureOverride(Size availableSize)
		{
			if (ViewModel?.BitmapForView != null)
			{
				return new Size(ViewModel.BitmapForView.PixelWidth, ViewModel.BitmapForView.PixelHeight);
			}
			return new Size(0, 0);
		}

		// Принудительный вызов перерисовки WPF
		public void RefreshScene()
		{
			//InvalidateVisual();
		}

		// Центральный метод сборки слоев
		protected override void OnRender(DrawingContext dc)
		{
			base.OnRender(dc);
			GeometryMap.Clear();

			if (ViewModel == null)
				return;

			// Слой 1: Отрисовка базового изображения (компонуется в левый верхний угол 0,0)
			if (ViewModel.BitmapForView != null)
			{
				dc.DrawImage(ViewModel.BitmapForView, new Rect(0, 0, ViewModel.BitmapForView.PixelWidth, ViewModel.BitmapForView.PixelHeight));
			}

			// Слой 2: Отрисовка наложенных слоев прямоугольников
			if (ViewModel.TiffAnnotations != null)
			{
				var regularPen = new Pen(Brushes.Red, 2);
				var selectedPen = new Pen(Brushes.Blue, 3); // Выделенный прямоугольник

				foreach (var item in ViewModel.TiffAnnotations)
				{
					var ma = item.TiffAnnotationItem;
					var www = ViewModel.DpiY;
					// Положение прямоугольника строго в координатах базового битмапа
					var imageData = ma.OiDIB_AN_IMAGE_STRUCT;
					var rotateData = ma.OiAnoDat_AN_NEW_ROTATE_STRUCT;

					var fontData = ma.LfFont;
					var font = fontData.GetFont();
					var bounds = ma.LrBounds;
					var color1 = ma.RgbColor1;
					var color2 = ma.RgbColor2;
					var thickness = ma.ULineSize;
					var textdata = ma.OiAnText_OIAN_TEXTPRIVDATA;

					var rectGeometry = new RectangleGeometry(bounds);
					// Сохраняем геометрию для будущего Hit Test
					GeometryMap[rectGeometry] = item;

					// Дополнительное изображение внутри прямоугольника (если задано)
					//if (item.InnerImage != null)
					//{
					//	dc.DrawImage(item.InnerImage, rect);
					//}

					// Рамка поверх изображения
					Pen currentPen = item.IsSelected ? selectedPen : regularPen;

					if (ma.UType == TiffAnnotationMarkType.FilledRectangle)
					{
						// Brushes.Transparent важен: он делает внутренность прямоугольника 
						// чувствительной к кликам мыши, не перекрывая нижележащий рисунок
						dc.DrawGeometry(Brushes.Red, currentPen, rectGeometry);
						//ga.DrawTiffAnnotationMarker(color1, new Rectangle(new Point(), annotationBitmap.Size));
					}
					else if (ma.UType == TiffAnnotationMarkType.HollowRectangle)
					{
						dc.DrawGeometry(Brushes.Red, currentPen, rectGeometry);
						//ga.DrawTiffAnnotationFrame(color1, thickness, new Rectangle(new Point(), annotationBitmap.Size));
					}
					else if (ma.UType == TiffAnnotationMarkType.TypedText)
					{
						dc.DrawRectangle(Brushes.Transparent, currentPen, bounds);//потом обработать выделение

						if (!(textdata is null) && !string.IsNullOrEmpty(textdata.SzAnoText) && !(font is null))
						{
							var text = textdata.SzAnoText;

							var angle = textdata.NCurrentOrientation / 10;
							var x = angle == 0 || angle == 90 ? bounds.Left : bounds.Right;
							var y = angle == 0 || angle == 270 ? bounds.Top : bounds.Bottom;
							var drawRectangle = new Rect(0, 0, bounds.Width, bounds.Height);

							dc.PushTransform(new TranslateTransform(x, y));
							dc.PushTransform(new RotateTransform(-angle));

							var tf = font.TypeFace;

							var ft = new FormattedText(
								text,
								System.Globalization.CultureInfo.CurrentCulture,
								FlowDirection.LeftToRight,
								tf,
								font.FontSize * ViewModel.DpiY / 72,
								new SolidColorBrush(color2),
								1.0);

							ft.MaxTextWidth = angle == 0 || angle == 180 ? bounds.Width : bounds.Height;
							ft.MaxTextHeight = angle == 0 || angle == 180 ? bounds.Height : bounds.Width;
							ft.Trimming = TextTrimming.None;

							dc.DrawText(ft, new Point());

							dc.PushTransform(new RotateTransform(angle));
							dc.PushTransform(new TranslateTransform(-x, -y));
						}
					}
					else if (ma.UType == TiffAnnotationMarkType.AttachNote)
					{
						//ga.DrawTiffAnnotationTextWithBackgroundData(textData, font, color2, thickness, color1, new Rectangle(new Point(), annotationBitmap.Size));
					}










					// Brushes.Transparent важен: он делает внутренность прямоугольника 
					// чувствительной к кликам мыши, не перекрывая нижележащий рисунок
				}
			}
		}

		// Метод генерации результирующего битмапа 1:1 в памяти
		public BitmapSource ComposeToSingleBitmap()
		{
			if (ViewModel?.BitmapForView == null)
				return null;

			int width = ViewModel.BitmapForView.PixelWidth;
			int height = ViewModel.BitmapForView.PixelHeight;

			// Создаем буфер в памяти
			RenderTargetBitmap renderTarget = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);

			// Принудительно разворачиваем элемент в границах изображения в памяти перед снимком
			this.Measure(new Size(width, height));
			this.Arrange(new Rect(0, 0, width, height));

			renderTarget.Render(this);
			return renderTarget;
		}
	}
}