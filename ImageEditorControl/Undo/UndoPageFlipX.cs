using ImageEditorControl.Models;

namespace ImageEditorControl.Undo
{
	public class UndoPageFlipX(PageModel pm) : UndoAbstract(pm)
	{
		public override bool NeedToRedrawPagePreview => true;

		public override void Undo()
		{
			pm.FlipX();
			SelectPageModel();
		}

		public override UndoAbstract MakeUndoCloneUnsafe(ImageModel imageModelClone)
		{
			var index = pm.PageNumberInitial;
			var pmClone = imageModelClone.GetPageModelByPageNumberInitial(index);

			return new UndoPageFlipX(pmClone);
		}

		public override UndoAbstract MakeRedoObject() => new UndoPageFlipX(pm);
	}
}