using ImageEditorControl.Tools;
using System.Text;

namespace ImageEditorControl.ImageReaders
{
	public static class ImageTypeChecker
	{
		public static bool IsBmp(byte[] fileBytes)
		{
			// Минимальный размер заголовка BMP составляет 54 байта (File Header + Info Header)
			if (fileBytes == null || fileBytes.Length < 54)
				return false;

			// 1. Проверяем сигнатуру "BM" (первые 2 байта: 0x42, 0x4D)
			if (fileBytes[0] != 0x42 || fileBytes[1] != 0x4D)
			{
				return false;
			}

			// 2. Считываем размер файла, указанный в самом BMP заголовке (байты 2, 3, 4, 5)
			// Формат записи - Int32 в порядке Little-Endian (младший байт вперед)
			int sizeInHeader = fileBytes[2] | (fileBytes[3] << 8) | (fileBytes[4] << 16) | (fileBytes[5] << 24);

			// 3. Проверяем целостность: реальный размер массива не должен быть меньше указанного в заголовке
			// (Иногда размер в заголовке может быть ровно равен bytes.Length или чуть меньше из-за выравнивания)
			if (fileBytes.Length < sizeInHeader)
			{
				return false; // Файл явно недогружен или обрезан
			}

			return true;
		}

		public static bool IsGif(byte[] fileBytes)
		{
			// Минимальный размер GIF-файла составляет около 14-20 байт
			if (fileBytes == null || fileBytes.Length < 14)
				return false;

			// 1. Проверяем заголовок (первые 3 байта должны быть "GIF" -> 0x47, 0x49, 0x46)
			if (fileBytes[0] != 0x47 || fileBytes[1] != 0x49 || fileBytes[2] != 0x46)
				//     'G'           'I'           'F'
				return false;

			// 2. Проверяем версию (следующие 3 байта должны быть "89a" или "87a")
			// "89a" -> 0x38, 0x39, 0x61
			bool is89a = fileBytes[3] == 0x38 && fileBytes[4] == 0x39 && fileBytes[5] == 0x61;
			// "87a" -> 0x38, 0x37, 0x61
			bool is87a = fileBytes[3] == 0x38 && fileBytes[4] == 0x37 && fileBytes[5] == 0x61;

			if (!is89a && !is87a)
				return false;

			// 3. Проверяем маркер конца файла (последний значащий байт должен быть 0x3B)
			// Сканируем последние 5 байт с конца на случай технических нулевых байтов выравнивания.
			int checkWindow = Math.Min(fileBytes.Length, 5);
			int startIndex = fileBytes.Length - checkWindow;

			for (int i = fileBytes.Length - 1; i >= startIndex; i--)
			{
				if (fileBytes[i] == 0x3B) // Символ ';'
				{
					return true; // Нашли терминатор, файл пригоден для парсинга
				}
				if (fileBytes[i] != 0x00)
				{
					// Если встретили не нуль и не 0x3B, значит файл оборван или поврежден
					break;
				}
			}

			return false;
		}

		public static bool IsPng(byte[] fileBytes)
		{
			// Минимальный размер пустого PNG составляет 67 байт
			if (fileBytes == null || fileBytes.Length < 67)
				return false;

			// 1. Проверяем строго зафиксированный заголовок (первые 8 байт)
			if (fileBytes[0] != 0x89 || fileBytes[1] != 0x50 || fileBytes[2] != 0x4E || fileBytes[3] != 0x47 ||
			    fileBytes[4] != 0x0D || fileBytes[5] != 0x0A || fileBytes[6] != 0x1A || fileBytes[7] != 0x0A)
			{
				return false;
			}

			// 2. Ищем маркер конца IEND в последних байтах файла
			// Сам маркер "IEND" занимает 4 байта, и после него идут еще 4 байта CRC32.
			// На случай технических байтов выравнивания (например, нулей) проверим последние 20 байт.
			int checkWindow = Math.Min(fileBytes.Length, 20);
			int startIndex = fileBytes.Length - checkWindow;

			string footer = Encoding.ASCII.GetString(fileBytes, startIndex, checkWindow);

			return footer.Contains("IEND");
		}

		public static bool IsJpg(byte[] fileBytes)
		{
			// Минимальный размер JPEG обычно не меньше 100-200 байт
			if (fileBytes == null || fileBytes.Length < 4)
				return false;

			// 1. Проверяем маркер начала (SOI): 0xFF, 0xD8
			if (fileBytes[0] != 0xFF || fileBytes[1] != 0xD8)
				return false;

			// 2. Ищем маркер конца (EOI): 0xFF, 0xD9 в последних байтах
			// Обычно маркер стоит в самом конце, но мы проверяем последние 10 байт на случай выравнивания нулевыми байтами
			int checkLength = Math.Min(fileBytes.Length, 10);
			int startIndex = fileBytes.Length - checkLength;

			bool hasEoi = false;
			for (int i = fileBytes.Length - 2; i >= startIndex; i--)
			{
				if (fileBytes[i] == 0xFF && fileBytes[i + 1] == 0xD9)
				{
					hasEoi = true;
					break;
				}
			}

			return hasEoi;
		}

		public static bool IsTiff(byte[] fileBytes)
		{
			if (fileBytes == null || fileBytes.Length < 4)
				return false;

			// 1. Проверяем классический TIFF (Little-Endian / Intel)
			if (fileBytes[0] == 0x49 && fileBytes[1] == 0x49 && fileBytes[2] == 0x2A && fileBytes[3] == 0x00)
				return true;

			// 2. Проверяем классический TIFF (Big-Endian / Motorola)
			if (fileBytes[0] == 0x4D && fileBytes[1] == 0x4D && fileBytes[2] == 0x00 && fileBytes[3] == 0x2A)
				return true;

			// 3. Опционально: Проверка BigTIFF (для файлов крупнее 4 ГБ)
			if (fileBytes.Length >= 4)
			{
				// Little-Endian BigTIFF (0x49, 0x49, 0x2B, 0x00)
				if (fileBytes[0] == 0x49 && fileBytes[1] == 0x49 && fileBytes[2] == 0x2B && fileBytes[3] == 0x00)
					return true;

				// Big-Endian BigTIFF (0x4D, 0x4D, 0x00, 0x2B)
				if (fileBytes[0] == 0x4D && fileBytes[1] == 0x4D && fileBytes[2] == 0x00 && fileBytes[3] == 0x2B)
					return true;
			}

			return false;
		}

		public static bool IsPdf(byte[] fileBytes)
		{
			// Минимальный размер пустого PDF обычно не меньше 30-40 байт
			if (fileBytes == null || fileBytes.Length < 32)
			{
				return false;
			}

			// 1. Проверяем заголовок %PDF- (первые 4 байта: 0x25, 0x50, 0x44, 0x46)
			if (fileBytes[0] != 0x25 || fileBytes[1] != 0x50 || fileBytes[2] != 0x44 || fileBytes[3] != 0x70)
			{
				// Небольшое исключение: иногда перед %PDF могут идти 1-2 мусорных байта BOM
				// Если строго с нуля не совпало, можно проверить чуть глубже (опционально)
				string headerCheck = Encoding.ASCII.GetString(fileBytes, 0, Math.Min(fileBytes.Length, 10));

				if (!headerCheck.Contains("%PDF-"))
				{
					return false;
				}
			}

			// 2. Ищем маркер %%EOF в конце файла (в последних 1024 байтах)
			int searchWindow = Math.Min(fileBytes.Length, 1024);
			int startIndex = fileBytes.Length - searchWindow;

			// Переводим этот хвост в строку ASCII для быстрого поиска подстроки
			string footer = Encoding.ASCII.GetString(fileBytes, startIndex, searchWindow);

			// Если "%%EOF" отсутствует в хвосте, парсер не сможет построить дерево объектов
			return footer.Contains("%%EOF");
		}

		public static ImageFileFormat GetImageFileFormat(byte[] fileBytes)
		{
			if (IsBmp(fileBytes))
			{
				return ImageFileFormat.Bmp;
			}

			if (IsGif(fileBytes))
			{
				return ImageFileFormat.Gif;
			}

			if (IsPng(fileBytes))
			{
				return ImageFileFormat.Png;
			}

			if (IsJpg(fileBytes))
			{
				return ImageFileFormat.Jpg;
			}

			if (IsTiff(fileBytes))
			{
				return ImageFileFormat.Tiff;
			}

			if (IsPdf(fileBytes))
			{
				return ImageFileFormat.Pdf;
			}

			return ImageFileFormat.Unknown;
		}
	}
}