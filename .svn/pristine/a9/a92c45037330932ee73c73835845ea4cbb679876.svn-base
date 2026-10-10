using ImageEditorControl.Models;

namespace ImageEditorControl.Undo
{
	public class UndoPageRotateL(PageModel pm) : UndoAbstract(pm)
	{
		public override bool NeedToRedrawPagePreview => true;

		public override void Undo()
		{
			pm.RotateR();
			SelectPageModel();
		}

		public override UndoAbstract MakeUndoCloneUnsafe(ImageModel imageModelClone)
		{
			var index = pm.PageNumberInitial;
			var pmClone = imageModelClone.GetPageModelByPageNumberInitial(index);

			return new UndoPageRotateL(pmClone);
		}

		public override UndoAbstract MakeRedoObject() => new UndoPageRotateR(pm);
	}
}