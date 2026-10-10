using System.Windows;
using System.Windows.Media;

namespace ImageEditorControl.Models.TiffAnnotations
{
	public class TiffAnnotationModel(PageModel pm, TiffAnnotationItem ai)
	{
		private readonly PageModel pageModelParent = pm;
		private readonly TiffAnnotationItem tiffAnnotationItem = ai;
		private readonly int index = pm.TiffAnnotationIndex++;
		private bool isSelected = false;

		public int Index => index;
		public TiffAnnotationItem TiffAnnotationItem => tiffAnnotationItem;
		public PageModel PageModelParent => pageModelParent;

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
					foreach (var tam in pageModelParent.TiffAnnotations.Where(x => x != this))
					{
						tam.isSelected = false;
					}

					isSelected = true;
					pageModelParent.SetSelectedTiffAnnotation(this);
				}
			}
		}

		/// <summary>
		/// создаёт клон (глубокую копию) модели заметки для страницы-клона
		/// </summary>
		/// <param name="pmClone">страница-клон</param>
		/// <returns>клон модели заметки</returns>
		public TiffAnnotationModel Clone(PageModel pmClone) =>
			new TiffAnnotationModel(pmClone, tiffAnnotationItem?.Clone());

		public void ShiftBounds(int dx, int dy) => tiffAnnotationItem.LrBounds = new Rect(
			tiffAnnotationItem.LrBounds.X + dx, tiffAnnotationItem.LrBounds.Y + dy, tiffAnnotationItem.LrBounds.Width, tiffAnnotationItem.LrBounds.Height);

		public void ShiftLeftBorder(int dx) => tiffAnnotationItem.LrBounds = new Rect(
			tiffAnnotationItem.LrBounds.X + dx, tiffAnnotationItem.LrBounds.Y, tiffAnnotationItem.LrBounds.Width - dx, tiffAnnotationItem.LrBounds.Height);

		public void ShiftRightBorder(int dx) => tiffAnnotationItem.LrBounds = new Rect(
			tiffAnnotationItem.LrBounds.X, tiffAnnotationItem.LrBounds.Y, tiffAnnotationItem.LrBounds.Width + dx, tiffAnnotationItem.LrBounds.Height);

		public void ShiftTopBorder(int dy) => tiffAnnotationItem.LrBounds = new Rect(
			tiffAnnotationItem.LrBounds.X, tiffAnnotationItem.LrBounds.Y + dy, tiffAnnotationItem.LrBounds.Width, tiffAnnotationItem.LrBounds.Height - dy);

		public void ShiftBottomBorder(int dy) => tiffAnnotationItem.LrBounds = new Rect(
			tiffAnnotationItem.LrBounds.X, tiffAnnotationItem.LrBounds.Y, tiffAnnotationItem.LrBounds.Width, tiffAnnotationItem.LrBounds.Height + dy);

		public void SetAlphaRgb1(byte alpha)
		{
			var color = TiffAnnotationItem.RgbColor1;

			if (color.A == 0 && color.A != alpha)
			{
				TiffAnnotationItem.RgbColor1 = Color.FromArgb(alpha, color.R, color.G, color.B);
			}
		}
	}
}