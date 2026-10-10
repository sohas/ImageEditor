using ImageEditorControl.Models;

namespace ImageEditorControl.Undo
{
	public class UndoPageDelete(IEnumerable<PageModel> pms) : UndoAbstract(pms.LastOrDefault())
	{
		private readonly List<PageModel> pms = [.. pms];

		public override void Undo()
		{
			foreach (var p in this.pms)
			{
				if (p is not null && !p.ImageModelParent.PagesChosen.Contains(p))
				{
					p.ImageModelParent.RestorePage(p);
				}
			}
		}

		public override UndoAbstract MakeUndoCloneUnsafe(ImageModel imageModelClone) =>
			new UndoPageDelete(pms.Select(x => imageModelClone.GetPageModelByPageNumberInitial(x.PageNumberInitial)));

		public override UndoAbstract MakeRedoObject() => new UndoPageRestore(pms);
		public bool IsMultiple => pms.Count > 1;
	}
}