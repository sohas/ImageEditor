using ImageEditorControl.Models;

namespace ImageEditorControl.Undo
{
	public abstract class UndoAbstract(PageModel pm)
	{
		protected readonly PageModel pm = pm;
		protected ImageModel Im => pm?.ImageModelParent;
		public virtual bool NeedToRedrawPagePreview => false;

		/// <summary>
		/// создаёт точную копию undo-объекта для модели, построенной из того же источника, что и данная модель.
		/// в противном случае метод не будет работать корректно. также корректность не гарантируется, если добавлялась экстра-модель.
		/// применяется только для создания полной копии модели.
		/// </summary>
		/// <param name="imageModelClone">клон текущей модели изображения</param>
		/// <returns>undo-объект для клона модели изображения</returns>
		public abstract UndoAbstract MakeUndoCloneUnsafe(ImageModel imageModelClone);
		public abstract UndoAbstract MakeRedoObject();
		public abstract void Undo();

		protected void SelectPageModel()
		{
			if (!(pm is null) && !pm.IsSelectedSingle)
			{
				pm.IsSelectedSingle = true;
			}
		}
	}
}