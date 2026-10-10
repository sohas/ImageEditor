using ImageEditorControl.Models.TiffAnnotations;
using ImageEditorControl.Models;
using ImageEditorControl.States;
using ImageEditorControl.Undo;
using System.Windows;

namespace ImageEditorControl.Tools
{
	public delegate void FileOperationEventHandler(object sender, FileOperationEventArgs args);

	public class FileOperationEventArgs(string fileName, string extraFileName, bool success) : EventArgs
	{
		public string FileName { get; private set; } = fileName;
		public string ExtraFileName { get; private set; } = extraFileName;
		public bool Success { get; private set; } = success;
	}

	public delegate void TiffAnnotationEventHandler(object sender, TiffAnnotationEventArgs e);

	public class TiffAnnotationEventArgs(TiffAnnotationModel tiffAnnotationModel, ModificationType modificationType) : EventArgs
	{
		public TiffAnnotationModel TiffAnnotationModel { get; private set; } = tiffAnnotationModel;
		public ModificationType ModificationType { get; private set; } = modificationType;
	}

	public delegate void SetStampEventHandler(object sender, SetStampEventArgs e);

	public class SetStampEventArgs(PageModel pageModel, Point location, SetStampType setStampType) : EventArgs
	{
		public PageModel PageModel { get; private set; } = pageModel;
		public Point Location { get; private set; } = location;
		public SetStampType SetStampType { get; private set; } = setStampType;
	}

	public delegate void AddExtraFileBeforePageEventHandler(object sender, AddExtraFileBeforePageEventArgs e);

	public class AddExtraFileBeforePageEventArgs(PageModel pageModel) : EventArgs
	{
		public PageModel PageModel { get; private set; } = pageModel;
		public SetStampType SetStampType
		{
			get; private set;
		}
	}

	public delegate void ApplyChangesEventHandler(object sender, ApplyChangesEventArgs e);

	public class ApplyChangesEventArgs(ApplyChangesState applyChangesState) : EventArgs
	{
		public ApplyChangesState ApplyChangesState { get; private set; } = applyChangesState;
	}

	public delegate void ChangePageModelEventHandler(object sender, ChangePageModelEventArgs e);

	public class ChangePageModelEventArgs(PageModel pageModel) : EventArgs
	{
		public PageModel PageModel { get; private set; } = pageModel;
	}

	public delegate void ChangeImageModelEventHandler(object sender, ChangeImageModelEventArgs e);

	public class ChangeImageModelEventArgs(ImageModel imageModel, ChangeType changeType) : EventArgs
	{
		public ImageModel ImageModel { get; private set; } = imageModel;
		public ChangeType ChangeType { get; private set; } = changeType;
	}

	public delegate void RegisterUndoObjectEventHandler(object sender, RegisterUndoObjectEventArgs e);

	public class RegisterUndoObjectEventArgs(ImageModel imageModel, UndoAbstract undoObject) : EventArgs
	{
		public ImageModel ImageModel { get; private set; } = imageModel;
		public UndoAbstract UndoObject { get; private set; } = undoObject;
	}
}