using System.IO;
using ImageEditorControl.Models;
using ImageEditorControl.Models.PdfAnnotations;

using System.Windows;
using System.Windows.Media;
using ImageEditorControl.Tools;
using System.Windows.Media.Imaging;

/// <summary>
/// Класс методов работы с pdf-аннотациями.
/// </summary>
public static class PdfAnnotationsExtensions
{
	///// <summary>
	///// Извлекает все Stamp-аннотации с JPEG из указанной страницы. Нумерация страниц с 1.
	///// </summary>
	///// <param name="reader">PdfReader</param>
	///// <param name="pageNumber">Номер страницы (1-based)</param>
	///// <returns>Список PdfStamp</returns>
	//public static List<PdfAnnotationModel> ExtractPdfAnnotations(this PdfReader reader, PageModel pm, int pageNumber)
	//{
	//	var result = new List<PdfAnnotationModel>();
	//	PdfDictionary pageDict = reader?.GetPageN(pageNumber);
	//	PdfArray annotArray = pageDict?.GetAsArray(PdfName.ANNOTS);

	//	if (annotArray is not null && annotArray.Any())
	//	{
	//		for (var i = 0; i < annotArray.Count(); i++)
	//		{
	//			PdfDictionary annotationDictionary = (PdfDictionary)PdfReader.GetPdfObject(annotArray[i]);

	//			if (annotationDictionary is not null) // || !PdfName.STAMP.Equals(subtype))
	//			{
	//				PdfAnnotationSubtype annotationSubtype = PdfAnnotationSubtype.None;
	//				string annotName = annotationDictionary.GetAsString(PdfName.T)?.ToUnicodeString() ?? "";
	//				var annotText = annotationDictionary.GetAsString(PdfName.CONTENTS)?.ToUnicodeString() ?? "";
	//				var annotFont = GetFontFromString(annotationDictionary.GetAsString(PdfName.DA)?.ToUnicodeString() ?? "");
	//				var annotForeColor = Color.Black;
	//				var annotBackColor = Color.Transparent;
	//				DateTime? annotDate = null;

	//				PdfArray rectArray = annotationDictionary.GetAsArray(PdfName.RECT);
	//				float[] rect = rectArray?.Select(x => ((PdfNumber)x).FloatValue) is IEnumerable<float> fr ? [..fr] : [0,0,0,0];
	//				float[] bbox = [0, 0, rect[2] - rect[0], rect[3] - rect[1]];
	//				float[] matrix = [1, 0, 0, 1, 0, 0];
	//				Bitmap bm = null;

	//				PdfName subtype = annotationDictionary.GetAsName(PdfName.SUBTYPE);

	//				if (PdfName.FREETEXT.Equals(subtype))
	//				{
	//					annotationSubtype = PdfAnnotationSubtype.FreeText;
	//				}
	//				else if (PdfName.STAMP.Equals(subtype))
	//				{
	//					annotationSubtype = PdfAnnotationSubtype.Stamp;
	//				}

	//				if (annotationDictionary.Get(PdfName.AP) is PdfDictionary ap)
	//				{
	//					// AP/N — Normal appearance
	//					// Может быть прямым Stream или indirect reference на Stream
	//					PdfObject nObj = PdfReader.GetPdfObject(ap.Get(PdfName.N));

	//					if (nObj is PRStream apStream)
	//					{
	//						PdfArray bboxArray = apStream.GetAsArray(PdfName.BBOX);
	//						bbox = bboxArray?.Select(x => ((PdfNumber)x).FloatValue) is IEnumerable<float> fsBB ? [.. fsBB] : bbox;

	//						// --- Matrix из AP stream ---
	//						PdfArray matrixArray = apStream.GetAsArray(PdfName.MATRIX);
	//						matrix = matrixArray?.Select(x => ((PdfNumber)x).FloatValue) is IEnumerable<float> fs ? [..fs] : matrix;

	//						// --- Resources/XObject/Im0 ---
	//						PdfDictionary resources = apStream.GetAsDict(PdfName.RESOURCES);

	//						if (resources is not null)// || xObjects is null)
	//						{
	//							PdfDictionary xObjects = resources.GetAsDict(PdfName.XOBJECT);

	//							if (xObjects is not null)
	//							{
	//								foreach (PdfName xKey in xObjects.Keys)
	//								{
	//									PdfObject xObjRaw = PdfReader.GetPdfObject(xObjects.Get(xKey));

	//									if (xObjRaw is PRStream imgStream)
	//									{
	//										PdfName xSubtype = imgStream.GetAsName(PdfName.SUBTYPE);

	//										if (PdfName.IMAGE.Equals(xSubtype))
	//										{
	//											PdfName filter = imgStream.GetAsName(PdfName.FILTER);

	//											// Проверяем Filter — нас интересует DCTDecode (JPEG)
	//											// Документ использует /DCTDecode напрямую
	//											if (!PdfName.DCTDECODE.Equals(filter))
	//											{
	//												// На случай если Filter задан как массив [/DCTDecode]
	//												PdfArray filterArr = imgStream.GetAsArray(PdfName.FILTER);
	//												PdfName firstFilter = filterArr?.GetAsName(0);

	//												if (filterArr is not null && filterArr.Size != 0 && PdfName.DCTDECODE.Equals(firstFilter))
	//												{
	//													bm = imgStream.GetBitmapFromStream();
	//												}
	//											}
	//											else
	//											{
	//												bm = imgStream.GetBitmapFromStream();
	//											}
	//										}
	//									}
	//								}
	//							}
	//						}
	//					}
	//				}

	//				result.Add(new PdfAnnotationModel
	//				{
	//					PageModelParent = pm,
	//					Subtype = annotationSubtype,
	//					Index = i,
	//					Name = annotName,
	//					Date = annotDate,
	//					Text = annotText,
	//					Font = annotFont,
	//					ForeColor = annotForeColor,
	//					BackColor = annotBackColor,
	//					Rectangle = rect,
	//					BoundingBox = bbox,
	//					Matrix = matrix,
	//					Bitmap = bm,
	//				});
	//			}
	//		}
	//	}

	//	return result;
	//}

	//public static BitmapSource GetBitmapFromStream(this PRStream imgStream)
	//{
	//	BitmapImage res = null;

	//	// Получаем сырые байты (PdfReader.GetStreamBytesRaw возвращает
	//	// данные ДО декодирования, т.е. готовый JPEG без дополнительной обработки)
	//	var jpegBytes = PdfReader.GetStreamBytesRaw(imgStream);

	//	if (jpegBytes is not null)
	//	{
	//		using var ms = new MemoryStream(jpegBytes);
	//		res = new BitmapImage();
	//		res.BeginInit();
	//		res.CacheOption = BitmapCacheOption.OnLoad; // Важно для загрузки в память и закрытия потока
	//		res.StreamSource = ms;
	//		res.EndInit();

	//		res.Freeze();
	//	}

	//	return res;
	//}

	public static Font GetFontFromString(string fontAsString)
	{
		Font res = null;

		if (
			!string.IsNullOrEmpty(fontAsString) &&
			fontAsString.TrimStart('/').TrimStart('/').Split(' ') is string[] arr &&
			arr.Length > 1 &&
			!string.IsNullOrEmpty(arr[0]) &&
			float.TryParse(arr[1], out float fontHeight) &&
			fontHeight > 0)
		{
			res = new Font(arr[0], fontHeight);
		}

		return res;
	}

	///// <summary>
	///// отрисовка pdf-аннотации
	///// </summary>
	///// <param name="g">объект Graphics</param>
	///// <param name="pm">модель страницы</param>
	//public static void DrawPdfAnnotations(this Graphics g, PageModel pm)
	//{
	//	float scaleX0 = (float)g.DpiX / 72f;
	//	float scaleY0 = (float)g.DpiY / 72f;

	//	foreach (var annot in pm.PdfAnnotations)
	//	{
	//		float a = annot.Matrix[0];
	//		float b = annot.Matrix[1];
	//		float c = annot.Matrix[2];
	//		float d = annot.Matrix[3];
	//		float e = annot.Matrix[4];
	//		float f = annot.Matrix[5];

	//		// Угол поворота из коэффициентов a, b
	//		float angleRad = (float)Math.Atan2(b, a);
	//		float angleDeg = angleRad * 180f / (float)Math.PI;

	//		// Масштаб: длина вектора (a, b) — это масштаб по X
	//		//          длина вектора (c, d) — это масштаб по Y
	//		float scaleX = (float)Math.Sqrt(a * a + b * b);
	//		float scaleY = (float)Math.Sqrt(c * c + d * d);

	//		// BBox размеры AP stream в pt
	//		float bboxX0 = annot.BoundingBox[0];
	//		float bboxY0 = annot.BoundingBox[1];
	//		float bboxW = annot.BoundingBox[2] - annot.BoundingBox[0];
	//		float bboxH = annot.BoundingBox[3] - annot.BoundingBox[1];

	//		// Rect: левый нижний угол в PDF coords (Y от низа)
	//		float rx1 = annot.Rectangle[0];
	//		float ry2 = annot.Rectangle[3];

	//		// Переводим точку привязки (rx1 + e, ry1 + f) в экранные пиксели.
	//		// PDF Y от низа → экран Y от верха:
	//		float originXpx = (rx1 + e) * scaleX0;
	//		float originYpx = g.VisibleClipBounds.Height - (ry2 + f) * scaleY0;

	//		scaleX *= scaleX0;
	//		scaleY *= scaleY0;

	//		if (scaleX > 0 && scaleY > 0)
	//		{
	//			// Сохраняем состояние Graphics
	//			var saved = g.Transform.Clone();

	//			// Переносим начало координат в точку привязки штампа
	//			g.TranslateTransform(originXpx, originYpx);

	//			// Применяем поворот (PDF поворачивает против часовой,
	//			g.RotateTransform(angleDeg);

	//			// Применяем масштаб (scaleX/scaleY из матрицы + перевод pt→px)
	//			g.ScaleTransform(scaleX, scaleY);

	//			// AP stream обрезается по BBox — его (0,0) может быть не в углу изображения.
	//			// Сдвигаем так чтобы BBox.x0, BBox.y0 совпал с origin.
	//			g.TranslateTransform(-bboxX0, -bboxY0);

	//			if (annot.Subtype == PdfAnnotationSubtype.FreeText && !string.IsNullOrEmpty(annot.Text) && annot.Font is Font font && font.Size > 0)
	//			{
	//				g.FillRectangle(new SolidBrush(annot.BackColor), 0, 0, bboxW, bboxH);
	//				var scaledFont = new Font(font.FontFamily, font.Size / scaleY);
	//				var size = g.MeasureString(annot.Text, scaledFont);
	//				var rec = new Rect(new Point((bboxW - size.Width) / 2, (bboxH - size.Height) / 2), new Size(bboxW, bboxH));
	//				g.DrawString(annot.Text, scaledFont, new SolidBrush(annot.ForeColor), rec);
	//			}
	//			else if (annot.Subtype == PdfAnnotationSubtype.Stamp && annot.Bitmap is not null)
	//			{
	//				g.DrawImage(annot.Bitmap, 0, 0, bboxW, bboxH);
	//			}

	//			// Восстанавливаем трансформацию
	//			g.Transform = saved;
	//		}
	//	}
	//}
}