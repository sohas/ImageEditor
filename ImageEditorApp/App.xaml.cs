using System.Windows;

namespace ImageEditorApp
{
	public partial class App : Application
	{
		public static IServiceProvider ServiceProvider { get; private set; } = null!;

		private void Application_Startup(object sender, StartupEventArgs e)
		{
			MainWindow mainWindow = new MainWindow();
			mainWindow.Show();
		}
	}
}