using ImageEditorControl.Models;

using static ImageEditorControl.UIs.ImageEditor;

namespace ImageEditorControl.DisplayTypes
{
	public class ColorizeTypeDisplay(ColorizeType type)
	{
		public ColorizeType Type { get; private set; } = type;
		public string Name => GetColorizeTypeName(Type);
	}
}