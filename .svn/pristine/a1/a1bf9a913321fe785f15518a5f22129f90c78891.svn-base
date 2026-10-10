using ImageEditorControl.Models;

namespace ImageEditorControl.Undo
{
	public class UndoStop : UndoAbstract
	{
		public UndoStop() : base(null) { }
		public override void Undo() {}
		public override UndoAbstract MakeUndoCloneUnsafe(ImageModel ImageModelClone) => new UndoStop();
		public override UndoAbstract MakeRedoObject() => new UndoStop();
	}
}