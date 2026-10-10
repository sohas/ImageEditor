using BitMiracle.LibTiff.Classic;
using ImageEditorControl.Models;
using ImageEditorControl.Models.TiffAnnotations;
using ImageEditorControl.Tools;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Tif = BitMiracle.LibTiff.Classic.Tiff;

namespace ImageEditorControl.ImageReaders.Tiff
{
        internal class BitMiracleRenderer
        {
                // Универсальный и безопасный метод распаковки сканлайна для любых внутренних форматов TIFF
                private static void ExtractChannelsFromScanline(
                    byte[] scanline, int x, int scanlineSize, int samplesPerPixel,
                    PixelFormat origFormat, BitmapPalette palette,
                    ref byte r, ref byte g, ref byte b, ref byte a)
                {
                        if (samplesPerPixel >= 3) // RGB / RGBA
                        {
                                int origPixelOffset = x * samplesPerPixel;
                                if (origPixelOffset + 2 < scanlineSize)
                                {
                                        r = scanline[origPixelOffset];
                                        g = scanline[origPixelOffset + 1];
                                        b = scanline[origPixelOffset + 2];
                                        if (samplesPerPixel == 4) a = scanline[origPixelOffset + 3];
                                }
                        }
                        else // 1-bit ЧБ или 8-bit Грейскейл
                        {
                                byte gray = 0;
                                if (origFormat == PixelFormats.Indexed1)
                                {
                                        int byteIndex = x >> 3;
                                        int bitIndex = 7 - (x & 7);
                                        if (byteIndex < scanlineSize)
                                        {
                                                bool isBitSet = (scanline[byteIndex] & (1 << bitIndex)) != 0;
                                                if (palette != null && palette.Colors.Count > 1)
                                                {
                                                        Color targetColor = isBitSet ? palette.Colors[1] : palette.Colors[0];
                                                        gray = targetColor.R;
                                                }
                                                else
                                                {
                                                        gray = isBitSet ? (byte)255 : (byte)0;
                                                }
                                        }
                                }
                                else // Gray8
                                {
                                        if (x < scanlineSize) gray = scanline[x];
                                }
                                r = g = b = gray;
                        }
                }

                internal static BitmapSource? RenderPageFromBytes(PageModel pm, bool forThumbnail)
                {
                        if (!(pm is not null && pm.ImageModelParent is ImageModel im && im.FileBytes is byte[] bytes && bytes.Length > 0))
                        {
                                return null;
                        }

                        var tiffBytes = bytes;
                        //forThumbnail = false;

                        try
                        {

                                using MemoryStream ms = new MemoryStream(tiffBytes);
                                using Tif tiff = Tif.ClientOpen("InMemoryTiff", "r", ms, new TiffStream());

                                if (tiff == null) 
                                {
                                        throw new Exception("Не удалось инициализировать TIFF из предоставленных байтов.");
                                }

                                if (GetPageParameters(tiff, pm.PageNumberInitial, out int origWidth, out int origHeight, out double dpiX, out double dpiY, out int stride, out PixelFormat format, out BitmapPalette? palette, out bool isExotic))
                                {
                                        var targetWidth = origWidth;
                                        var targetHeight = origHeight;

                                        int scale = 1;

                                        if (forThumbnail)
                                        {
                                                var ths = ImageModel.ThumbnailSize.GetFitSize(pm.HeightPXLOrigin / pm.WidthPXLOrigin);
                                                scale = (int)(pm.WidthPXLOrigin / ths.Width);
                                                targetWidth = (int)ths.Width;
                                                targetHeight = (int)ths.Height;
                                        }

                                        if (!forThumbnail && isExotic)
                                        {
                                                int w = forThumbnail ? targetWidth : origWidth;
                                                int h = forThumbnail ? targetHeight : origHeight;
                                                return DecodeFallbackAsRgba32(tiff, w, h, dpiX, dpiY);
                                        }

                                        int finalWidth = forThumbnail ? targetWidth : origWidth;
                                        int finalHeight = forThumbnail ? targetHeight : origHeight;

                                        // Создаем WriteableBitmap. Для обеих веток используем Bgra32, 
                                        // так как это "родной" и самый быстрый формат для рендеринга в WPF
                                        //WriteableBitmap writeableBitmap = new WriteableBitmap(
                                        //    finalWidth,
                                        //    finalHeight,
                                        //    dpiX,
                                        //    dpiY,
                                        //    PixelFormats.Bgra32,
                                        //    null
                                        //);

                                        //writeableBitmap.Lock();
                                        //IntPtr backBuffer = writeableBitmap.BackBuffer;
                                        //int targetStride = writeableBitmap.BackBufferStride;

                                        var samplesField = tiff.GetField(TiffTag.SAMPLESPERPIXEL);
                                        int samplesPerPixel = (samplesField != null && samplesField.Length > 0) ? samplesField[0].ToShort() : 1;

                                        int scanlineSize = tiff.ScanlineSize();
                                        byte[] scanline = new byte[scanlineSize];

                                        // ВЕТКА 2: ГЕНЕРАЦИЯ МИНЬАТЮРЫ (ПРИВЕДЕНИЕ К BGRA32 С ПРОРЕЖИВАНИЕМ)
                                        if (forThumbnail)
                                        {
                                                // Создаем WriteableBitmap строго по размерам превью с оригинальным DPI
                                                WriteableBitmap writeableBitmap = new WriteableBitmap(
                                                    targetWidth,
                                                    targetHeight,
                                                    dpiX,
                                                    dpiY,
                                                    PixelFormats.Bgra32,
                                                    null
                                                );

                                                writeableBitmap.Lock();
                                                IntPtr backBuffer = writeableBitmap.BackBuffer;
                                                int targetStride = writeableBitmap.BackBufferStride;

                                                int pixelCount = targetWidth * targetHeight;
                                                long[] accumB = new long[pixelCount];
                                                long[] accumG = new long[pixelCount];
                                                long[] accumR = new long[pixelCount];
                                                long[] accumA = new long[pixelCount];
                                                int[] counts = new int[pixelCount];

                                                double scaleY = (double)targetHeight / origHeight;
                                                double scaleX = (double)targetWidth / origWidth;

                                                // СЦЕНАРИЙ А: Для ЧБ документов (Indexed1 / Gray8) читаем через Scanlines
                                                if (format == PixelFormats.Indexed1 || format == PixelFormats.Gray8)
                                                {
                                                        for (int y = 0; y < origHeight; y++)
                                                        {
                                                                tiff.ReadScanline(scanline, y);

                                                                int targetY = (int)(y * scaleY);
                                                                if (targetY >= targetHeight) targetY = targetHeight - 1;
                                                                int targetRowOffset = targetY * targetWidth;

                                                                for (int x = 0; x < origWidth; x++)
                                                                {
                                                                        int targetX = (int)(x * scaleX);
                                                                        if (targetX >= targetWidth) targetX = targetWidth - 1;

                                                                        byte gray = 0;
                                                                        if (format == PixelFormats.Indexed1)
                                                                        {
                                                                                int byteIndex = x >> 3;
                                                                                int bitIndex = 7 - (x & 7);
                                                                                if (byteIndex < scanlineSize)
                                                                                {
                                                                                        bool isBitSet = (scanline[byteIndex] & (1 << bitIndex)) != 0;
                                                                                        if (palette != null && palette.Colors.Count > 1)
                                                                                        {
                                                                                                gray = isBitSet ? palette.Colors[1].R : palette.Colors[0].R;
                                                                                        }
                                                                                        else
                                                                                        {
                                                                                                gray = isBitSet ? (byte)255 : (byte)0;
                                                                                        }
                                                                                }
                                                                        }
                                                                        else // Gray8
                                                                        {
                                                                                if (x < scanlineSize) gray = scanline[x];
                                                                        }

                                                                        int targetPixelIdx = targetRowOffset + targetX;
                                                                        accumB[targetPixelIdx] += gray;
                                                                        accumG[targetPixelIdx] += gray;
                                                                        accumR[targetPixelIdx] += gray;
                                                                        accumA[targetPixelIdx] += 255;
                                                                        counts[targetPixelIdx]++;
                                                                }
                                                        }
                                                }
                                                // СЦЕНАРИЙ Б: Для цветных изображений используем быстрый RGBA-растр
                                                else
                                                {
                                                        int[] raster = new int[origWidth * origHeight];
                                                        if (!tiff.ReadRGBAImageOriented(origWidth, origHeight, raster, BitMiracle.LibTiff.Classic.Orientation.TOPLEFT, false))
                                                        {
                                                                throw new Exception("Ошибка чтения RGBA-растра для превью.");
                                                        }

                                                        unsafe
                                                        {
                                                                fixed (int* pRaster = raster)
                                                                {
                                                                        for (int y = 0; y < origHeight; y++)
                                                                        {
                                                                                int targetY = (int)(y * scaleY);
                                                                                if (targetY >= targetHeight) targetY = targetHeight - 1;

                                                                                int origRowOffset = y * origWidth;
                                                                                int targetRowOffset = targetY * targetWidth;

                                                                                for (int x = 0; x < origWidth; x++)
                                                                                {
                                                                                        int targetX = (int)(x * scaleX);
                                                                                        if (targetX >= targetWidth) targetX = targetWidth - 1;

                                                                                        int pixel = pRaster[origRowOffset + x];
                                                                                        int targetPixelIdx = targetRowOffset + targetX;

                                                                                        accumR[targetPixelIdx] += (pixel) & 0xFF;
                                                                                        accumG[targetPixelIdx] += (pixel >> 8) & 0xFF;
                                                                                        accumB[targetPixelIdx] += (pixel >> 16) & 0xFF;
                                                                                        accumA[targetPixelIdx] += (pixel >> 24) & 0xFF;
                                                                                        counts[targetPixelIdx]++;
                                                                                }
                                                                        }
                                                                }
                                                        }
                                                }

                                                // ВЫГРУЗКА РЕЗУЛЬТАТА: Считаем среднее и пишем в видеопамять BackBuffer
                                                unsafe
                                                {
                                                        byte* pBackBuffer = (byte*)backBuffer.ToPointer();

                                                        for (int y = 0; y < targetHeight; y++)
                                                        {
                                                                byte* pDstRow = pBackBuffer + (y * targetStride);
                                                                int targetRowOffset = y * targetWidth;

                                                                for (int x = 0; x < targetWidth; x++)
                                                                {
                                                                        int pixelIdx = targetRowOffset + x;
                                                                        int count = counts[pixelIdx];
                                                                        if (count == 0) count = 1;

                                                                        int byteOffset = x * 4;

                                                                        pDstRow[byteOffset] = (byte)(accumB[pixelIdx] / count); // B
                                                                        pDstRow[byteOffset + 1] = (byte)(accumG[pixelIdx] / count); // G
                                                                        pDstRow[byteOffset + 2] = (byte)(accumR[pixelIdx] / count); // R
                                                                        pDstRow[byteOffset + 3] = (byte)(accumA[pixelIdx] / count); // Alpha
                                                                }
                                                        }
                                                }

                                                writeableBitmap.AddDirtyRect(new System.Windows.Int32Rect(0, 0, targetWidth, targetHeight));
                                                writeableBitmap.Unlock();

                                                writeableBitmap.Freeze();
                                                return BitmapFrame.Create(writeableBitmap);
                                        }
                                        // ВЕТКА 3: ОРИГИНАЛЬНЫЙ РАЗМЕР И ФОРМАТ (ПИШЕМ НАПРЯМУЮ В СТРУКТУРУ BGRA32 СТРАНИЦЫ)
                                        else
                                        {
                                                WriteableBitmap writeableBitmap = new WriteableBitmap(
    finalWidth,
    finalHeight,
    dpiX,
    dpiY,
    PixelFormats.Bgra32,
    null
);

                                                writeableBitmap.Lock();
                                                IntPtr backBuffer = writeableBitmap.BackBuffer;
                                                int targetStride = writeableBitmap.BackBufferStride;

                                                unsafe
                                                {
                                                        byte* pBackBuffer = (byte*)backBuffer.ToPointer();

                                                        // Фиксируем массив сканлайна в памяти один раз для всего цикла
                                                        fixed (byte* pSrcScanline = scanline)
                                                        {
                                                                // 1. Оптимизированный сценарий для чистого ЧБ (Indexed1) или Серого (Gray8)
                                                                if (format == PixelFormats.Indexed1 || format == PixelFormats.Gray8)
                                                                {
                                                                        for (int y = 0; y < finalHeight; y++)
                                                                        {
                                                                                tiff.ReadScanline(scanline, y);
                                                                                byte* pDstRow = pBackBuffer + (y * targetStride);

                                                                                // ИСПРАВЛЕНО: Чистый, безопасный и максимально быстрый перенос памяти
                                                                                int bytesToCopy = Math.Min(scanlineSize, targetStride);
                                                                                Buffer.MemoryCopy(pSrcScanline, pDstRow, targetStride, bytesToCopy);
                                                                        }
                                                                }
                                                                // 2. Сценарий для полноцветных картинок (Bgr24 / Bgra32)
                                                                else
                                                                {
                                                                        int bytesPerPixel = samplesPerPixel; // 3 для RGB, 4 для RGBA

                                                                        for (int y = 0; y < finalHeight; y++)
                                                                        {
                                                                                tiff.ReadScanline(scanline, y);
                                                                                byte* pDstRow = pBackBuffer + (y * targetStride);

                                                                                if (bytesPerPixel == 3) // RGB -> BGR
                                                                                {
                                                                                        byte* pSrc = pSrcScanline;
                                                                                        byte* pDst = pDstRow;

                                                                                        for (int x = 0; x < finalWidth; x++)
                                                                                        {
                                                                                                *(pDst) = *(pSrc + 2); // B = R
                                                                                                *(pDst + 1) = *(pSrc + 1); // G = G
                                                                                                *(pDst + 2) = *(pSrc);     // R = B
                                                                                                *(pDst + 3) = 255;         // A

                                                                                                pSrc += 3;
                                                                                                pDst += 4;
                                                                                        }
                                                                                }
                                                                                else if (bytesPerPixel == 4) // RGBA -> BGRA
                                                                                {
                                                                                        byte* pSrc = pSrcScanline;
                                                                                        byte* pDst = pDstRow;

                                                                                        for (int x = 0; x < finalWidth; x++)
                                                                                        {
                                                                                                *(pDst) = *(pSrc + 2); // B
                                                                                                *(pDst + 1) = *(pSrc + 1); // G
                                                                                                *(pDst + 2) = *(pSrc);     // R
                                                                                                *(pDst + 3) = *(pSrc + 3); // A

                                                                                                pSrc += 4;
                                                                                                pDst += 4;
                                                                                        }
                                                                                }
                                                                        }
                                                                }
                                                        }
                                                }

                                                writeableBitmap.AddDirtyRect(new System.Windows.Int32Rect(0, 0, finalWidth, finalHeight));
                                                writeableBitmap.Unlock();

                                                writeableBitmap.Freeze();
                                                return BitmapFrame.Create(writeableBitmap);

                                        }

                                        //writeableBitmap.AddDirtyRect(new System.Windows.Int32Rect(0, 0, finalWidth, finalHeight));
                                        //writeableBitmap.Unlock();

                                        //writeableBitmap.Freeze();
                                        //return BitmapFrame.Create(writeableBitmap);
                                }
                                else 
                                {
                                        return null;
                                }
                        }
                        catch (Exception ex)
                        {
                                return null;
                        }
                }

                // Универсальный резервный метод для сложных TIFF-вариантов
                private static BitmapFrame DecodeFallbackAsRgba32(Tif tiff, int width, int height, double dpiX, double dpiY)
                {
                        int[] raster = new int[width * height];
                        if (!tiff.ReadRGBAImageOriented(width, height, raster, BitMiracle.LibTiff.Classic.Orientation.TOPLEFT, false))
                                throw new Exception("Ошибка чтения резервного RGBA-растра.");

                        byte[] buffer = new byte[raster.Length * 4];
                        for (int i = 0; i < raster.Length; i++)
                        {
                                int pixel = raster[i];
                                int offset = i * 4;
                                buffer[offset] = (byte)Tif.GetB(pixel);
                                buffer[offset + 1] = (byte)Tif.GetG(pixel);
                                buffer[offset + 2] = (byte)Tif.GetR(pixel);
                                buffer[offset + 3] = (byte)Tif.GetA(pixel);
                        }

                        BitmapSource bitmapSource = BitmapSource.Create(
                            width, height, dpiX, dpiY, PixelFormats.Bgra32, null, buffer, width * 4
                        );

                        return BitmapFrame.Create(bitmapSource);
                }

                internal static List<PageSize> GetTiffStructureFromBytes(byte[] fileBytes, ILogger logger)
                {
                        var structureList = new List<PageSize>();

                        if (fileBytes == null || fileBytes.Length == 0)
                        {
                                return structureList;
                        }

                        var tiffBytes = fileBytes;

                        return structureList;
                }

                private static bool GetPageParameters(Tif tiff, int pageNumber, out int width, out int height, out double dpiX, out double dpiY, out int stride, out PixelFormat format, out BitmapPalette? palette, out bool isExotic) 
                {
                        width = 0; 
                        height = 0; 
                        dpiX = 0;
                        dpiY = 0;
                        stride = 0;
                        format = PixelFormats.Bgr32;
                        palette = null;
                        isExotic = false;

                        try
                        {
                                if (!tiff.SetDirectory((short)pageNumber))
                                {
                                        throw new ArgumentOutOfRangeException(nameof(pageNumber), "Указанная страница не найдена.");
                                }

                                width = tiff.GetField(TiffTag.IMAGEWIDTH)?[0].ToInt() ?? 0;
                                height = tiff.GetField(TiffTag.IMAGELENGTH)?[0].ToInt() ?? 0;

                                // 2. Читаем и вычисляем оригинальное разрешение (DPI)
                                FieldValue[] xResFields = tiff.GetField(TiffTag.XRESOLUTION);
                                FieldValue[] yResFields = tiff.GetField(TiffTag.YRESOLUTION);
                                FieldValue[] resUnitFields = tiff.GetField(TiffTag.RESOLUTIONUNIT);

                                dpiX = 96.0;
                                dpiY = 96.0;

                                if (xResFields != null && xResFields.Length > 0 && yResFields != null && yResFields.Length > 0)
                                {
                                        // Теги разрешения по спецификации TIFF хранятся как float (RATIONAL)
                                        double rawX = xResFields[0].ToFloat();
                                        double rawY = yResFields[0].ToFloat();

                                        // Тег единиц измерения хранится как short (SHORT)
                                        int unit = (resUnitFields != null && resUnitFields.Length > 0) ? resUnitFields[0].ToShort() : 2;

                                        if (unit == 3) // 3 = Сантиметры (ResUnit.CENTIMETER)
                                        {
                                                dpiX = rawX * 2.54; // Переводим точки/см в точки/дюйм (DPI)
                                                dpiY = rawY * 2.54;
                                        }
                                        else
                                        {
                                                dpiX = rawX;
                                                dpiY = rawY;
                                        }
                                }

                                // Защита от некорректных или нулевых значений DPI в файле
                                if (dpiX <= 0) dpiX = 96.0;
                                if (dpiY <= 0) dpiY = 96.0;

                                // 3. Определяем формат пикселей на основе тегов TIFF
                                FieldValue[] bitsPerSampleField = tiff.GetField(TiffTag.BITSPERSAMPLE);
                                FieldValue[] samplesPerPixelField = tiff.GetField(TiffTag.SAMPLESPERPIXEL);
                                FieldValue[] photometricField = tiff.GetField(TiffTag.PHOTOMETRIC);

                                int bitsPerSample = (bitsPerSampleField != null && bitsPerSampleField.Length > 0)
                                    ? bitsPerSampleField[0].ToShort() // Для тега BitPerSample стандарт требует short
                                    : 1;

                                int samplesPerPixel = (samplesPerPixelField != null && samplesPerPixelField.Length > 0)
                                    ? samplesPerPixelField[0].ToShort() // Для SamplesPerPixel также возвращается short
                                    : 1;

                                int photometric = (photometricField != null && photometricField.Length > 0)
                                    ? photometricField[0].ToInt()
                                    : 0;

                                // Классификация форматов
                                if (samplesPerPixel == 1 && bitsPerSample == 1)
                                {
                                        // Черно-белое изображение (1 бит на пиксель)
                                        format = PixelFormats.Indexed1;

                                        // Корректируем палитру в зависимости от фотометрии (инверсия ЧБ)
                                        if (photometric == 0) // MinIsWhite
                                        {
                                                palette = new BitmapPalette(new List<Color> { Colors.White, Colors.Black });
                                        }
                                        else // MinIsBlack
                                        {
                                                palette = new BitmapPalette(new List<Color> { Colors.Black, Colors.White });
                                        }

                                        // Выравнивание длины строки по границе 4 байт (WPF Stride constraint)
                                        stride = ((width * bitsPerSample + 31) & ~31) >> 3;
                                }
                                else if (samplesPerPixel == 1 && bitsPerSample == 8)
                                {
                                        // Градации серого (Grayscale, 8 бит на пиксель)
                                        format = PixelFormats.Gray8;
                                        stride = width;
                                }
                                else if (samplesPerPixel >= 3 && bitsPerSample == 8)
                                {
                                        // Полноцветное изображение (RGB / RGBA)
                                        format = (samplesPerPixel == 4) ? PixelFormats.Bgra32 : PixelFormats.Bgr24;
                                        stride = width * samplesPerPixel;
                                }
                                else
                                {
                                        // Для экзотических форматов откатываемся на универсальный RGBA-декодер
                                        isExotic = true;
                                }

                                return true;
                        }
                        catch (Exception ex)
                        {
                                return false;
                        }
                }

                internal static bool AddPages(ImageModel im, CancellationToken token)
                {
                        if (im is null || im.FileBytes is not byte[] bytes || bytes.Length == 0)
                        {
                                return false;
                        }

                        var tiffBytes = bytes;

                        try
                        {
                                if (token.IsCancellationRequested)
                                {
                                        return false;
                                }

                                using MemoryStream ms = new MemoryStream(tiffBytes);
                                using Tif tiff = Tif.ClientOpen("InMemoryTiff", "r", ms, new TiffStream());

                                if (tiff == null)
                                {
                                        throw new Exception("Не удалось инициализировать TIFF из предоставленных байтов.");
                                }

                                short count = tiff.NumberOfDirectories();

                                try
                                {
                                        for (short i = 0; i < count; i++)
                                        {
                                                if (GetPageParameters(tiff, i, out int width, out int height, out double dpiX, out double dpiY, out int stride, out PixelFormat format, out BitmapPalette? palette, out bool isExotic))
                                                {
                                                        if (token.IsCancellationRequested)
                                                        {
                                                                return false;
                                                        }

                                                        IEnumerable<TiffAnnotationItem>? tiffAnnotations = null;
                                                        var annotTag = tiff.GetField(ImageModel.OIAN_MARK_ATTRIBUTES);

                                                        if (!(annotTag is null) && annotTag.Length > 1)
                                                        {
                                                                var annotationAsBytes = (byte[])annotTag[1].Value;
                                                                var ta = new DocumentImageTiffAnnotationsModel();
                                                                ta.CreateAnnotationItemsOnTiffAnnotationBytes(annotationAsBytes);
                                                                tiffAnnotations = ta.TiffAnnotationItems.OrderBy(x => x.OiIndex.GetParsedIndex());
                                                        }

                                                        im.AddNewPage(i, new Size(width, height), dpiX, dpiY, format, palette, tiffAnnotations);
                                                }
                                                else
                                                {
                                                        return false;
                                                }
                                        }
                                }
                                catch (Exception ex)
                                {
                                        im.Logger?.LogException(ex, "Couldn't get existing page");
                                        return false;
                                }

                                return true;
                        }
                        catch (Exception ex)
                        {
                                im.Logger?.LogException(ex, "cannot read file");
                                return false;
                        }
                }
        }
}