using ImageEditorControl.Models;

using static ImageEditorControl.UIs.ImageEditor;

namespace ImageEditorControl.DisplayTypes
{
	public class ExtraObjectsSaveTypeDisplay(ExtraObjectsSaveType saveType, ExtraObjectType objectType)
	{
		public ExtraObjectsSaveType Type { get; private set; } = saveType;
		private readonly ExtraObjectType objectType = objectType;
		public string Name => objectType == ExtraObjectType.TiffAnnotationExtraObject ? GetAnotationTypeName(Type) : GetStampTypeName(Type);
	}

	public enum ExtraObjectType
	{
		TiffAnnotationExtraObject,
		StampExtraObject,
	}
}