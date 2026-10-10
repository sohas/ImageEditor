using System.Runtime.InteropServices;

namespace ImageEditorControl.Tools
{
        static class ScreenMesurer
        {
                [DllImport("gdi32.dll")]
                static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

		[DllImport("gdi32.dll")]
		private static extern IntPtr CreateDC(string lpszDriver, string lpszDevice, string lpszOutput, IntPtr lpInitData);

		[DllImport("gdi32.dll")]
		private static extern bool DeleteDC(IntPtr hdc);

		private enum DeviceCap
                {
                        HORZSIZE = 4,
                        VERTSIZE = 6,
                        HORZRES = 8,
                        VERTRES = 10,
                }

                public static MonitorSizes GetPhysicalDisplaySize()
                {
                        IntPtr desktop = CreateDC("DISPLAY", null, null, IntPtr.Zero);

                        if (desktop != IntPtr.Zero)
                        {
                                try
                                {
                                        int mmScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.VERTSIZE);
                                        int mmScreenWidth = GetDeviceCaps(desktop, (int)DeviceCap.HORZSIZE);
                                        int pxlScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.VERTRES);
                                        int pxlScreenWidth = GetDeviceCaps(desktop, (int)DeviceCap.HORZRES);

                                        return new MonitorSizes()
                                        {
                                                WidthInMm = mmScreenWidth,
                                                HeightInMm = mmScreenHeight,
                                                PxlScreenHeight = pxlScreenHeight,
                                                PxlScreenWidth = pxlScreenWidth,
                                        };
                                }
                                catch
                                {
					return new MonitorSizes();
				}
				finally
                                {
                                        // Обязательно освобождаем ресурсы контекста устройства
                                        DeleteDC(desktop);
                                }
                        }
                        else
                        {
                                return new MonitorSizes();
                        }
                }
        }

        public struct MonitorSizes
        {
                public int WidthInMm;
                public int HeightInMm;
                public int PxlScreenHeight;
                public int PxlScreenWidth;
        }
}