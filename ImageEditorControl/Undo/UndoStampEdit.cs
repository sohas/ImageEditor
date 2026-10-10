using ImageEditorControl.Models;
using System.Windows;

namespace ImageEditorControl.Undo
{
	public class UndoStampEdit(StampModel sm, Rect? bounds) : UndoAbstract(sm?.PageModelParent)
	{
		private readonly StampModel sm = sm;
		private readonly Rect? bounds = bounds;
		public override bool NeedToRedrawPagePreview => true;

		public override void Undo()
		{
			if (sm is not null && bounds is Rect b && pm is not null)
			{
				sm.RectangleStamp = b;
				SelectPageModel();
			}
		}

		public override UndoAbstract MakeUndoCloneUnsafe(ImageModel imageModelClone)
		{
			if (pm is not null && Im is not null)
			{
				var pmClone = imageModelClone.GetPageModelByPageNumberInitial(pm.PageNumberInitial);
				var smClone = pmClone.Stamps.FirstOrDefault(x => x.Index == sm.Index);

				if (pmClone is not null && smClone is not null)
				{
					return new UndoStampEdit(smClone, bounds);
				}
			}

			return new UndoStop();
		}

		public override UndoAbstract MakeRedoObject() => new UndoStampEdit(sm, sm.RectangleStamp);
	}
}