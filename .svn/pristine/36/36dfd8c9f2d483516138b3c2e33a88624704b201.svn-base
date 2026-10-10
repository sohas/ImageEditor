using ImageEditorControl.Models;
using ImageEditorControl.Models.TiffAnnotations;

namespace ImageEditorControl.Undo
{
	public class UndoTiffAnnotationAdd(PageModel pm, TiffAnnotationModel am) : UndoAbstract(pm)
	{
		public TiffAnnotationModel Am { get; private set; } = am;

		public override bool NeedToRedrawPagePreview => true;

		public override void Undo()
		{
			if (
				Am is not null && pm is not null &&
				pm.TiffAnnotations.FirstOrDefault(x => x.Index == Am.Index) is TiffAnnotationModel ams)
			{
				pm.RemoveTiffAnnotation(ams);

				if (pm.TiffAnnotationSelected == ams)
				{
					pm.DeselectAllTiffAnnotations();
				}

				SelectPageModel();
			}
		}

		public override UndoAbstract MakeUndoCloneUnsafe(ImageModel imageModelClone)
		{
			if (pm is not null && Im is not null)
			{
				var pmClone = imageModelClone.GetPageModelByPageNumberInitial(pm.PageNumberInitial);
				var amClone = Am?.Clone(pmClone);

				if (pmClone is not null && amClone is not null)
				{
					return new UndoTiffAnnotationAdd(pmClone, amClone);
				}
			}

			return new UndoStop();
		}

		public override UndoAbstract MakeRedoObject() => new UndoTiffAnnotationRemove(pm, Am);
	}
}