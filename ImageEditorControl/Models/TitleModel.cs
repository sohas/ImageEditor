using ImageEditorControl.Tools;
using System.Windows.Media;

namespace ImageEditorControl.Models
{
	/// <summary>
	/// модель-представление текстовой надписи поверх страницы
	/// </summary>
	public class TitleModel
	{
		private readonly PageModel pageModelParent;
		private readonly string text;
		private readonly TitleHorizontalAlignment horizontalAlignment;
		private readonly TitleVerticalAlignment verticalAlignment;
		private readonly Font font = new Font("Arial", 12);
		private readonly Color color = Colors.Red;
		private readonly double relativeHeight = 0.025;
		private bool isSelected = false;
		private readonly int index = 0;

		public int Index => index;
		public PageModel PageModelParent => pageModelParent;
		public string Text => text;
		public TitleHorizontalAlignment HorizontalAlignment => horizontalAlignment;
		public TitleVerticalAlignment VerticalAlignment => verticalAlignment;
		public Font Font => font;
		public Color Color => color;
		public double RelativeHigh => relativeHeight;
		public bool IsDiagonal => verticalAlignment == TitleVerticalAlignment.DiagonalLTRB || verticalAlignment == TitleVerticalAlignment.DiagonalLBRT;

		public bool IsSelected
		{
			get => isSelected;
			set
			{
				if (!value)
				{
					isSelected = false;
				}
				else
				{
					foreach (var title in pageModelParent.Titles.Where(x => x != this))
					{
						title.isSelected = false;
					}

					isSelected = true;
					pageModelParent.SetSelectedTitle(this);
				}
			}
		}

		/// <summary>
		/// модель-представление текстовой надписи поверх страницы
		/// </summary>
		/// <param name="pm"></param>
		/// <param name="title">текст надписи</param>
		/// <param name="horizontalAlignment">горизонтальное расположение надписи</param>
		/// <param name="verticalAlignment">вертикальное расположение надписи</param>
		/// <param name="font">шрифт надписи</param>
		/// <param name="relativeHeight">высота надписи по отношению к диагонали страницы</param>
		public TitleModel(PageModel pm, string title, TitleHorizontalAlignment horizontalAlignment, TitleVerticalAlignment verticalAlignment,
			Font font = null, Color? color = null, double? relativeHeight = null)
		{
			this.pageModelParent = pm;
			this.text = title;
			this.horizontalAlignment = horizontalAlignment;
			this.verticalAlignment = verticalAlignment;
			this.index = pm.TitleIndex++;

			if (font is Font f)
			{
				this.font = f;
			}

			if (color is Color c)
			{
				this.color = c;
			}

			if (relativeHeight is double rh)
			{
				this.relativeHeight = rh;
			}
		}
	}
}