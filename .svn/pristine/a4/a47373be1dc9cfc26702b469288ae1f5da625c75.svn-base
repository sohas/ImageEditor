using ImageEditorControl.Models;

namespace ImageEditorControl.Undo
{
	public class UndoPageFlipY(PageModel pm) : UndoAbstract(pm)
	{
		public override bool NeedToRedrawPagePreview => true;

		public override void Undo()
		{
			pm.FlipY();
			SelectPageModel();
		}

		public override UndoAbstract MakeUndoCloneUnsafe(ImageModel imageModelClone)
		{
			var index = pm.PageNumberInitial;
			var pmClone = imageModelClone.GetPageModelByPageNumberInitial(index);

			return new UndoPageFlipY(pmClone);
		}

		public override UndoAbstract MakeRedoObject() => new UndoPageFlipY(pm);
	}
}