using ImageEditorControl.Models;

namespace ImageEditorControl.Undo
{
	public class UndoPageRotateR(PageModel pm) : UndoAbstract(pm)
	{
		public override bool NeedToRedrawPagePreview => true;

		public override void Undo()
		{
			pm.RotateL();
			SelectPageModel();
		}

		public override UndoAbstract MakeUndoCloneUnsafe(ImageModel imageModelClone)
		{
			var index = pm.PageNumberInitial;
			var pmClone = imageModelClone.GetPageModelByPageNumberInitial(index);

			return new UndoPageRotateR(pmClone);
		}

		public override UndoAbstract MakeRedoObject() => new UndoPageRotateL(pm);
	}
}