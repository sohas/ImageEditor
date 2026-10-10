using ImageEditorControl.Models;
using System.Windows;

namespace ImageEditorControl.Undo
{
	public class UndoPageCrop(PageModel pm, Rect lastCrop, Rect currentCrop) : UndoAbstract(pm)
	{
		private readonly Rect lastCrop = lastCrop;
		private readonly Rect currentCrop = currentCrop;
		public bool IsUncrop => currentCrop == new Rect(0, 0, pm.WidthPXLOrigin, pm.HeightPXLOrigin);

		public override void Undo()
		{
			pm.TrySetCropRectangleDirectly(lastCrop);
			SelectPageModel();
		}

		public override UndoAbstract MakeUndoCloneUnsafe(ImageModel imageModelClone)
		{
			var index = pm.PageNumberInitial;
			var pmClone = imageModelClone.GetPageModelByPageNumberInitial(index);

			return new UndoPageCrop(pmClone, lastCrop, currentCrop);
		}

		public override UndoAbstract MakeRedoObject() =>
			new UndoPageCrop(pm, currentCrop, lastCrop);
	}
}