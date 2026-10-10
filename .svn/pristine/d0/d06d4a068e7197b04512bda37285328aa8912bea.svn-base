using System.IO;
using System.Windows.Input;

namespace ImageEditorControl.Tools
{
	public static class CursorHelper
	{
		public static Cursor CreateCursorFromPng(byte[] pngBytes, byte hotX = 0, byte hotY = 0)
		{
			using var curStream = new MemoryStream();
			using var writer = new BinaryWriter(curStream);
			// 1. ICONDIR структура
			writer.Write((short)0);     // Reserved (всегда 0)
			writer.Write((short)2);     // Type: 1 = иконка, 2 = курсор (CUR)
			writer.Write((short)1);     // Количество изображений (1)

			// 2. ICONDIRENTRY структура
			writer.Write((byte)0);      // Width (0 означает 256 пикселей или берется из PNG)
			writer.Write((byte)0);      // Height
			writer.Write((byte)0);      // Color count
			writer.Write((byte)0);      // Reserved

			// В формате CUR вместо планов и битности записывается Hotspot
			writer.Write(hotX);         // Hotspot X (координата клика)
			writer.Write((byte)0);      // (Второй байт для X)
			writer.Write(hotY);         // Hotspot Y (координата клика)
			writer.Write((byte)0);      // (Второй байт для Y)

			writer.Write(pngBytes.Length); // Размер самого PNG в байтах
			writer.Write(22);           // Смещение, где начинается PNG (6 байт ICONDIR + 16 байт ICONDIRENTRY)

			// 3. Записываем само PNG изображение сразу за заголовком
			writer.Write(pngBytes);

			// Сбрасываем позицию потока на начало
			curStream.Position = 0;

			// Передаем поток в стандартный безопасный класс WPF
			return new Cursor(curStream);
		}
	}
}