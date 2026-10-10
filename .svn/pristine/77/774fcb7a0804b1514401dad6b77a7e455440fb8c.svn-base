namespace ImageEditorControl.Models.TiffAnnotations
{
	public class DocumentImageTiffAnnotationsModel
	{
		private readonly List<TiffAnnotationItem> tiffAnnotationItems = new List<TiffAnnotationItem>();

		public uint SizeInt { get; set; }
		public uint Header { get; set; }
		public string OiGroup { get; set; }
		public string OiIndex { get; set; }

		public List<TiffAnnotationItem> TiffAnnotationItems => tiffAnnotationItems;

		public DocumentImageTiffAnnotationsModel()
		{
			Header = 196612;
			SizeInt = 1;
			OiGroup = "[Без имени]";
			OiIndex = "1";
		}
	}
}