using ImageEditorControl.Models;

namespace ImageEditorControl.Undo
{
	public class UndoExtraModelAdd(ImageModel extra, int pageNumber) : UndoAbstract(null)
	{
		private readonly ImageModel extra = extra;
		private readonly int pageNumber = pageNumber;

		public override void Undo()
		{
			var parent = extra.ImageModelParent;
			
			if (!(parent is null) && !parent.IsExtra)
			{
				parent.RemoveExtraModel(extra, pageNumber);
			}
		}

		public override UndoAbstract MakeUndoCloneUnsafe(ImageModel ImageModelClone) => new UndoStop();
		public override UndoAbstract MakeRedoObject() => new UndoStop();
	}
}