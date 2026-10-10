using PDFiumCore;

namespace ImageEditorControl.Tools
{
	public static class PdfiumCoreLifecycleManager
	{
		private static readonly object _lock = new object();
		public static bool IsInitialized { get; private set; } = false;

		public static void Initialize()
		{
			lock (_lock)
			{
				if (!IsInitialized)
				{
					fpdfview.FPDF_InitLibrary();
					IsInitialized = true;
				}
			}
		}

		public static void Shutdown()
		{
			lock (_lock)
			{
				if (IsInitialized)
				{
					fpdfview.FPDF_DestroyLibrary();
					IsInitialized = false;
				}
			}
		}
	}
}