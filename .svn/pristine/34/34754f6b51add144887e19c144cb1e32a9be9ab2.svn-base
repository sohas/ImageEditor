using ImageEditorControl.Models;

namespace ImageEditorControl.Undo 
{
	public class UndoDocumentNameChanged(ImageModel im, string documentName) : UndoAbstract(null)
	{
		private readonly ImageModel im = im;
		private readonly string documentName = documentName;

		public override void Undo() => im.DocumentNameChosen = documentName;

		public override UndoAbstract MakeUndoCloneUnsafe(ImageModel imageModelClone) =>
			new UndoDocumentNameChanged(imageModelClone, documentName);

		public override UndoAbstract MakeRedoObject() => new UndoDocumentNameChanged(im, im.DocumentNameChosen);
	}
}