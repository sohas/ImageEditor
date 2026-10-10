using ImageEditorControl.Tools;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageEditorControl.Models.PdfAnnotations
{
	public class PdfAnnotationModel
	{
		/// <summary> родительская модель страницы </summary>
		public PageModel PageModelParent { get; set; }

		/// <summary> тип pdf-аннотации </summary>
		public PdfAnnotationSubtype Subtype { get; set; }

		/// <summary>Порядковый номер аннотации (0-based)</summary>
		public int Index { get; set; }

		/// <summary>Имя поля /T аннотации, если есть</summary>
		public string Name { get; set; }
		
		/// <summary> Дата </summary>
		public DateTime? Date { get; set; }
		
		/// <summary>Имя поля /CONTENTS аннотации, если есть</summary>
		public string Text { get; set; }

		/// <summary> Шрифт </summary>
		public Font Font { get; set; }

		/// <summary> Цвет элемента </summary>
		public Color ForeColor { get; set; }
		
		/// <summary> Цвет фона </summary>
		public Color BackColor { get; set; }
		
		/// <summary>
		/// Rect аннотации в PDF-координатах (origin = левый нижний угол страницы).
		/// Порядок: [x1, y1, x2, y2]
		/// </summary>
		public float[] Rectangle { get; set; }

		/// <summary>
		/// BBox из AP/N stream (координаты внутри AP, до применения Matrix).
		/// Порядок: [x1, y1, x2, y2]
		/// </summary>
		public float[] BoundingBox { get; set; }

		/// <summary>
		/// Matrix из AP/N stream [a b c d e f].
		/// Задаёт трансформацию AP-пространства в пространство страницы (relative to Rect).
		/// </summary>
		public float[] Matrix { get; set; }

		/// <summary> битмап штампа </summary>
		public BitmapSource Bitmap { get; set; }
	}

	/// <summary>
	/// The type of pdf annotation.
	/// </summary>
	public enum PdfAnnotationSubtype
	{
		None,
		FreeText,
		Stamp
	}
}