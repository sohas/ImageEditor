using ImageEditorControl.Tools;

namespace ImageEditorControl.Models.TiffAnnotations
{
	/// <summary>
	/// Класс параметров отображения заметок
	/// </summary>
	public class TiffAnnotationView
	{
		public FilteringTiffAnnotationsType FilteringTiffAnnotations { get; set; } = FilteringTiffAnnotationsType.ShowAll;
		public string TiffAnnotationOiGroupCurrentUser { get; set; } = null;
		public HashSet<string> TiffAnnotationOiGroups { get; set; } = null;

		public static TiffAnnotationView NotFiltering => new TiffAnnotationView();
	}
}