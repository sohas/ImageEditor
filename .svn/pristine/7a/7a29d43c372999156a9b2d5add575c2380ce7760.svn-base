using ImageEditorControl.Models;
using ImageEditorControl.Tools;

using static ImageEditorControl.UIs.ImageEditor;

namespace ImageEditorControl.DisplayTypes
{
	public class ImageFileFormatDisplay(ImageFileFormat type)
	{
		public ImageFileFormat Type { get; private set;	} = type;
		public string Name => GetImageFileFormatName(Type);
	}
}