using ImageEditorControl.Tools;
using OperationDispatcher;
using System.Windows;

namespace ImageEditorApp
{
	public class ImageEditorAppLogger : ILogger
	{
		public void LogException(Exception ex, string text)
		{
			Action act = () => MessageBox.Show($"{ex}", text);
			Application.Current.Dispatcher.BeginInvoke(act);
		}

		public void LogInfo(string text)
		{
			Console.WriteLine($"{DateTime.Now:O} {text}");
		}

		public void LogOperation(LogType type, ImageOperationID id, Exception? ex)
		{
			Console.WriteLine($"{DateTime.Now:O} {type} {id.OperationType} {id.Comment}");
		}
	}
}