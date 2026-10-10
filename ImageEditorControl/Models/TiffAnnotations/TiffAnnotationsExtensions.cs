using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace ImageEditorControl.Models.TiffAnnotations
{
	public static class TiffAnnotationsExtensions
	{
		private static readonly int LOGFONTSIZE = 60;
		//private static int LF_FACESIZE = 32;

		/// <summary>
		/// создаёт байтовую последовательность для Bitmap из Dib-структуры
		/// </summary>
		/// <param name="dib"></param>
		/// <returns></returns>
		public static byte[] GetBitmapBytesFromDibBytes(this byte[] dib)
		{
			var bitmapHeader = new byte[] { 66, 77, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };//66 77 - 'BM' маркер Bitmap в заголовке
			var res = bitmapHeader.Concat(dib).ToArray();
			return res;
		}

		/// <summary>
		/// возвращает массив байт аннотаций для записи в тифф-файл
		/// </summary>
		/// <param name="ditasm"></param>
		/// <param name="withImageEmbedded"></param>
		/// <returns></returns>
		public static byte[] GetAnnotationTiffBytes(this DocumentImageTiffAnnotationsModel ditasm, bool withImageEmbedded)
		{
			if (
				ditasm.TiffAnnotationItems == null || 
				ditasm.TiffAnnotationItems.Count == 0 || 
				(!withImageEmbedded && !ditasm.TiffAnnotationItems.Exists(x => x.UType != TiffAnnotationMarkType.ImageEmbedded)))
			{
				return null;
			}

			var result = new List<byte>();

			result.AddBytes(BitConverter.GetBytes(ditasm.Header));
			result.AddBytes(BitConverter.GetBytes(ditasm.SizeInt));

			// данные по блоку
			result.AddBytes(BitConverter.GetBytes(2));
			result.AddBytes(BitConverter.GetBytes(12));
			result.AddBytes(Encoding.Default.GetBytes("OiGroup" + '\0'), 8);

			byte[] boiGroup = Encoding.Default.GetBytes(ditasm.OiGroup + '\0');
			result.AddBytes(BitConverter.GetBytes(boiGroup.Length));
			result.AddBytes(boiGroup);

			// данные по блоку
			result.AddBytes(BitConverter.GetBytes(2));
			result.AddBytes(BitConverter.GetBytes(12));
			result.AddBytes(Encoding.Default.GetBytes("OiIndex" + '\0'), 8);

			byte[] boiIndex = Encoding.Default.GetBytes(ditasm.OiIndex + '\0');
			result.AddBytes(BitConverter.GetBytes(10));
			result.AddBytes(boiIndex, 10);

			foreach (var attr in ditasm.TiffAnnotationItems)
			{
				if (!withImageEmbedded && attr.UType == TiffAnnotationMarkType.ImageEmbedded)
				{
					continue;
				}

				// данные по блоку
				result.AddBytes(BitConverter.GetBytes(5));
				result.AddBytes(BitConverter.GetBytes(164));

				var battr = new List<byte>();//UType

				battr.AddBytes(BitConverter.GetBytes((uint)attr.UType));//UType

				battr.AddBytes(BitConverter.GetBytes(attr.LrBoundsChosen.Left));//lrBounds, после преобразования
				battr.AddBytes(BitConverter.GetBytes(attr.LrBoundsChosen.Top));//lrBounds, после преобразования
				battr.AddBytes(BitConverter.GetBytes(attr.LrBoundsChosen.Right));//lrBounds, после преобразования
				battr.AddBytes(BitConverter.GetBytes(attr.LrBoundsChosen.Bottom));//lrBounds, после преобразования

				battr.AddBytes(new byte[] { attr.RgbColor1.B, attr.RgbColor1.G, attr.RgbColor1.R, attr.RgbColor1.A });//rgbColor1
				battr.AddBytes(new byte[] { attr.RgbColor2.B, attr.RgbColor2.G, attr.RgbColor2.R, attr.RgbColor2.A });//rgbColor2

				battr.AddBytes(BitConverter.GetBytes(Convert.ToInt32(attr.BHighlighting)));// bHighlighting
				battr.AddBytes(BitConverter.GetBytes(Convert.ToInt32(attr.BTransparent)));// bTransparent
				battr.AddBytes(BitConverter.GetBytes(Convert.ToInt32(attr.ULineSize)));//ULineSize
				battr.AddBytes(BitConverter.GetBytes(Convert.ToInt32(attr.UReserved1)));//UReserved1
				battr.AddBytes(BitConverter.GetBytes(Convert.ToInt32(attr.UReserved2)));//UReserved2

				battr.AddBytes(GetTiffBytesFormLogFont(attr.LfFont));//lfFont    
				battr.AddBytes(BitConverter.GetBytes(attr.BReserved3));//BReserved3 
				battr.AddBytes(BitConverter.GetBytes(Convert.ToInt32((int)attr.Time)));//time 
				battr.AddBytes(BitConverter.GetBytes(Convert.ToInt32(attr.BVisible)));//BVisible
				battr.AddBytes(BitConverter.GetBytes(Convert.ToInt32(attr.DwReserved4)));//DwReserved4

				for (int i = 0; i < attr.LReserved.Length; i++)
				{
					battr.AddBytes(BitConverter.GetBytes(attr.LReserved[i]));//LReserved
				}

				if (battr.Count != 164)
				{
					throw new Exception("Плохой размер OIAN_MARK_ATTRIBUTES");
				}

				result.AddBytes(battr);

				if (attr.OiAnText_OIAN_TEXTPRIVDATA != null)
				{

					attr.OiAnText_OIAN_TEXTPRIVDATA.SzAnoText += '\0';
					attr.OiAnText_OIAN_TEXTPRIVDATA.UAnoTextLength = (uint)attr.OiAnText_OIAN_TEXTPRIVDATA.SzAnoText.Length;

					IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(attr.OiAnText_OIAN_TEXTPRIVDATA));
					//Marshal.WriteInt32(ptr, attr.OiAnText_OIAN_TEXTPRIVDATA.NCurrentOrientation);
					//Marshal.WriteInt32(new IntPtr(ptr.ToInt32() + 4), (int)attr.OiAnText_OIAN_TEXTPRIVDATA.UReserved1);
					//Marshal.WriteInt32(new IntPtr(ptr.ToInt32() + 8), (int)attr.OiAnText_OIAN_TEXTPRIVDATA.UCreationScale);
					//Marshal.WriteInt32(new IntPtr(ptr.ToInt32() + 12), (int)attr.OiAnText_OIAN_TEXTPRIVDATA.UAnoTextLength);
					Marshal.StructureToPtr(attr.OiAnText_OIAN_TEXTPRIVDATA, ptr, true);
					byte[] res = new byte[16 + attr.OiAnText_OIAN_TEXTPRIVDATA.SzAnoText.Length];
					Marshal.Copy(ptr, res, 0, 16 + attr.OiAnText_OIAN_TEXTPRIVDATA.SzAnoText.Length);
					Marshal.FreeCoTaskMem(ptr);

					// данные по блоку
					result.AddBytes(BitConverter.GetBytes(6));
					result.AddBytes(BitConverter.GetBytes(12));

					result.AddBytes(Encoding.Default.GetBytes("OiAnText"), 8);
					result.AddBytes(BitConverter.GetBytes(res.Length));
					result.AddBytes(res);
				}

				if (attr.OiFilNam_AN_NAME_STRUCT != null)
				{
					byte[] res = Encoding.Default.GetBytes(attr.OiFilNam_AN_NAME_STRUCT.Name + '\0');
					// данные по блоку
					result.AddBytes(BitConverter.GetBytes(6));
					result.AddBytes(BitConverter.GetBytes(12));

					result.AddBytes(Encoding.Default.GetBytes("OiFilNam"), 8);
					result.AddBytes(BitConverter.GetBytes(res.Length));
					result.AddBytes(res);
				}
				if (attr.OiDIB_AN_IMAGE_STRUCT != null)
				{
					// данные по блоку
					result.AddBytes(BitConverter.GetBytes(6));
					result.AddBytes(BitConverter.GetBytes(12));

					result.AddBytes(Encoding.Default.GetBytes("OiDIB" + '\0'), 8);
					result.AddBytes(BitConverter.GetBytes(attr.OiDIB_AN_IMAGE_STRUCT.DibInfo.Length));
					result.AddBytes(attr.OiDIB_AN_IMAGE_STRUCT.DibInfo);
				}
				if (attr.OiAnoDat_AN_NEW_ROTATE_STRUCT_Chosen != null)
				{
					byte[] resrotate = GetTiffBytesFromRotateStruct(attr.OiAnoDat_AN_NEW_ROTATE_STRUCT_Chosen);
					// данные по блоку
					result.AddBytes(BitConverter.GetBytes(6));
					result.AddBytes(BitConverter.GetBytes(12));

					result.AddBytes(Encoding.Default.GetBytes("OiAnoDat"), 8);
					result.AddBytes(BitConverter.GetBytes(resrotate.Length));
					result.AddBytes(resrotate);
				}
				// данные по блоку
				result.AddBytes(BitConverter.GetBytes(6));
				result.AddBytes(BitConverter.GetBytes(12));

				result.AddBytes(Encoding.Default.GetBytes("OiGroup" + '\0'), 8);
				byte[] boiGroupa = Encoding.Default.GetBytes(attr.OiGroup + '\0');
				result.AddBytes(BitConverter.GetBytes(boiGroupa.Length));
				result.AddBytes(boiGroupa);
				// данные по блоку
				result.AddBytes(BitConverter.GetBytes(6));
				result.AddBytes(BitConverter.GetBytes(12));

				result.AddBytes(Encoding.Default.GetBytes("OiIndex" + '\0'), 8);
				byte[] boiIndexa = Encoding.Default.GetBytes(attr.OiIndex + '\0');
				result.AddBytes(BitConverter.GetBytes(10));
				result.AddBytes(boiIndex, 10);
			}

			return [.. result];
		}

		/// <summary>
		/// Создает коллекцию аттрибутов аннотации из байтового массива тифф-файла
		/// </summary>
		/// <param name="annotationBytes">массив байтов, записанный в файле Tiff</param>
		public static void CreateAnnotationItemsOnTiffAnnotationBytes(this DocumentImageTiffAnnotationsModel ditasm, byte[] annotationBytes)
		{
			if (annotationBytes.Length < 1)
			{
				return;
			}

			ditasm.Header = BitConverter.ToUInt32(annotationBytes, 0);
			ditasm.SizeInt = BitConverter.ToUInt32(annotationBytes, 4);//тип определяющий размер инта в байтах(1-нормально, 0 - два байта)
			byte[] dataMain = new byte[annotationBytes.Length - 8];
			Array.Copy(annotationBytes, 8, dataMain, 0, annotationBytes.Length - 8);

			ditasm.FetchTheNextAnnotationItemFieldFromTheCurrentTiffAnnotationBytes(dataMain, null);
		}

		#region private methods
		/// <summary>
		/// добавляет коллекцию байтов с учётом заданной длины дополнения 
		/// (если заданная длина дополнения превышает размер дополнения, хвост заполняется нулевыми байтами)
		/// </summary>
		/// <param name="srcBytes">дополняемая коллекция</param>
		/// <param name="addBytes">дополняющая коллекция</param>
		/// <param name="length">заданная длина дополнения. если передан 0, то равна длина дополнения. по умолчанию = 0</param>
		private static void AddBytes(this List<byte> srcBytes, IEnumerable<byte> addBytes, int length = 0)
		{
			var addLength = addBytes.Count();
			var newLength = length == 0 ? addLength : length;
			var extraLength = newLength - addLength;

			srcBytes.AddRange(addBytes.Take(newLength));

			if (extraLength > 0)
			{
				srcBytes.AddRange(new byte[extraLength]);
			}
		}

		private static void FetchTheNextAnnotationItemFieldFromTheCurrentTiffAnnotationBytes(this DocumentImageTiffAnnotationsModel ditasm, byte[] currentAnnotationData, TiffAnnotationItem currentAnnotationItem)
		{
			TiffAnnotationItem newAnnotationItem = currentAnnotationItem;

			try
			{
				var type = BitConverter.ToInt32(currentAnnotationData, 0);//это тип(2,5,6) следующих данных
				var size = BitConverter.ToInt32(currentAnnotationData, 4);//размер данных следующего блока

				if (size == 0)
				{
					return;
				}

				var sizen = 0;

				switch (type)
				{
					case 2://Annotation Named Blocks(тип 2)
						var name = Encoding.ASCII.GetString(currentAnnotationData, 8, 8).Trim('\0');//имя
						var offset = 16;

						sizen = TiffBitConverter.ToInt32(currentAnnotationData, ref offset);//рамер блока

						if (size > 12)
						{
							var _ = TiffBitConverter.ToInt32(currentAnnotationData, ref offset);//резерв //offset6 = 4;
						}

						byte[] data = new byte[sizen];
						Array.Copy(currentAnnotationData, offset, data, 0, sizen);

						switch (name)
						{
							case "OiGroup":
								var nameOiGroup = Encoding.Default.GetString(data, 0, sizen).Trim('\0');
								ditasm.OiGroup = nameOiGroup;
								break;
							case "OiIndex":
								var oiIndex = Encoding.Default.GetString(data, 0, sizen).Trim('\0');
								ditasm.OiIndex = oiIndex;
								break;
							default:
								break;
						}
						break;

					case 5://mark(тип 5)
						byte[] mark = new byte[size];
						Array.Copy(currentAnnotationData, 8, mark, 0, size);

						newAnnotationItem = CreateAnnotationItemFromTiffBytes(mark);
						ditasm.TiffAnnotationItems.Add(newAnnotationItem);
						break;

					case 6://Annotation Named Blocks(тип 6)
						if (newAnnotationItem == null)
						{
							throw new Exception("Должен быть создан экземпляр атрибута");
						}

						var name6 = Encoding.ASCII.GetString(currentAnnotationData, 8, 8).Trim('\0');//имя
						var offset6 = 16;
						sizen = TiffBitConverter.ToInt32(currentAnnotationData, ref offset6);//рамер блока

						if (size > 12)
						{
							var _ = TiffBitConverter.ToInt32(currentAnnotationData, ref offset6);//резерв //offset6 = 4;
						}

						var data6 = new byte[sizen];
						Array.Copy(currentAnnotationData, offset6, data6, 0, sizen);

						switch (name6)
						{
							case "OiGroup":
								var nameOiGroup = Encoding.Default.GetString(data6, 0, sizen).Trim('\0');
								newAnnotationItem.OiGroup = nameOiGroup;
								break;
							case "OiIndex":
								var OiIndex = Encoding.Default.GetString(data6, 0, sizen).Trim('\0');
								newAnnotationItem.OiIndex = OiIndex;
								break;
							case "OiAnoDat":
								switch (newAnnotationItem.UType)
								{
									case TiffAnnotationMarkType.AttachNote://добавлено
									case TiffAnnotationMarkType.TypedText://добавлено
									case TiffAnnotationMarkType.ImageEmbedded:
									case TiffAnnotationMarkType.ImageReference:
									case TiffAnnotationMarkType.Form:
										newAnnotationItem.OiAnoDat_AN_NEW_ROTATE_STRUCT = CreateRotateStructFromTiffBytes(data6);
										break;
									case TiffAnnotationMarkType.StraightLine:
									case TiffAnnotationMarkType.FreehandLine:
										newAnnotationItem.OiAnoDat_AN_POINTS = CreateAnPpointsFromTiffBytes(data6);
										break;
								}
								break;
							case "OiFilNam":
								var nameStruct = new AN_NAME_STRUCT
								{
									Name = Encoding.Default.GetString(data6, 0, sizen).Trim('\0')
								};
								newAnnotationItem.OiFilNam_AN_NAME_STRUCT = nameStruct;
								break;
							case "OiAnText":
								newAnnotationItem.OiAnText_OIAN_TEXTPRIVDATA = CreateTextPrivateData(data6);
								break;
							case "OiDIB":
								var image = new AN_IMAGE_STRUCT
								{
									DibInfo = data6
								};
								newAnnotationItem.OiDIB_AN_IMAGE_STRUCT = image;
								break;
							case "OiHypLnk":
								throw new Exception("OiHypLnk не реализована");
							case "MarkBnds":
								//не описано в спецификации
								break;
							default:
								//throw new Exception("Не учтенное имя в типе 6: " + name6);
								break;
						}
						break;
				}

				var nextSizeBlock = 8 + size + sizen;

				if (currentAnnotationData.Length - nextSizeBlock < 8)
				{
					return;
				}

				var nextdata = new byte[currentAnnotationData.Length - nextSizeBlock];
				Array.Copy(currentAnnotationData, nextSizeBlock, nextdata, 0, currentAnnotationData.Length - nextSizeBlock);

				ditasm.FetchTheNextAnnotationItemFieldFromTheCurrentTiffAnnotationBytes(nextdata, newAnnotationItem);
			}
			catch //в случае ошибки чтения заметок удаляем текущую заметку и прерываем процесс
			{
				if (newAnnotationItem is not null && ditasm.TiffAnnotationItems.Contains(newAnnotationItem))
				{
					ditasm.TiffAnnotationItems.Remove(newAnnotationItem);
				}
			}
		}

		private static OIAN_TEXTPRIVDATA CreateTextPrivateData(byte[] data)
		{
			OIAN_TEXTPRIVDATA textPrivate = new OIAN_TEXTPRIVDATA();
			int offset = 0;
			textPrivate.NCurrentOrientation = TiffBitConverter.ToInt32(data, ref offset);
			textPrivate.UReserved1 = TiffBitConverter.ToUInt32(data, ref offset);
			textPrivate.UCreationScale = TiffBitConverter.ToUInt32(data, ref offset);
			textPrivate.UAnoTextLength = TiffBitConverter.ToUInt32(data, ref offset);
			textPrivate.SzAnoText = Encoding.Default.GetString(data, offset, (int)textPrivate.UAnoTextLength).Trim('\0');

			return textPrivate;
		}

		private static AN_POINTS CreateAnPpointsFromTiffBytes(byte[] data)
		{
			AN_POINTS points = new AN_POINTS();
			int offset = 0;
			points.NMaxPoints = TiffBitConverter.ToInt32(data, ref offset);
			points.NPoints = TiffBitConverter.ToInt32(data, ref offset);
			Point[] point = new Point[(data.Length - offset) / (TiffBitConverter.SizeInt == 1 ? 8 : 4)];
			int statOffset = offset;

			for (int i = 0; i < point.Length; i++)
			{
				offset = statOffset + i * (TiffBitConverter.SizeInt == 1 ? 8 : 4);
				point[i] = new Point(TiffBitConverter.ToInt32(data, ref offset), TiffBitConverter.ToInt32(data, ref offset));
			}
			points.PtPoint = point;

			return points;
		}

		private static TiffAnnotationItem CreateAnnotationItemFromTiffBytes(byte[] data)
		{
			int offsetMark = 0;

			TiffAnnotationItem attr = new TiffAnnotationItem((TiffAnnotationMarkType)TiffBitConverter.ToUInt32(data, ref offsetMark));

			int x1 = TiffBitConverter.ToInt32(data, ref offsetMark);
			int y1 = TiffBitConverter.ToInt32(data, ref offsetMark);
			int x2 = TiffBitConverter.ToInt32(data, ref offsetMark);
			int y2 = TiffBitConverter.ToInt32(data, ref offsetMark);

			attr.LrBounds = new Rect(x1, y1, x2 - x1, y2 - y1);

			Color color = Color.FromArgb(data[offsetMark + 3], data[offsetMark + 2], data[offsetMark + 1], data[offsetMark]);
			attr.RgbColor1 = color;
			offsetMark += 4;

			color = Color.FromArgb(data[offsetMark + 3], data[offsetMark + 2], data[offsetMark + 1], data[offsetMark]);
			attr.RgbColor2 = color;
			offsetMark += 4;

			attr.BHighlighting = Convert.ToBoolean(TiffBitConverter.ToInt32(data, ref offsetMark));
			attr.BTransparent = Convert.ToBoolean(TiffBitConverter.ToInt32(data, ref offsetMark));
			attr.ULineSize = TiffBitConverter.ToUInt32(data, ref offsetMark);
			attr.UReserved1 = TiffBitConverter.ToUInt32(data, ref offsetMark);
			attr.UReserved2 = TiffBitConverter.ToUInt32(data, ref offsetMark);

			byte[] dataFont = new byte[LOGFONTSIZE];
			Array.Copy(data, offsetMark, dataFont, 0, LOGFONTSIZE);
			TextRenderingHint quality = TextRenderingHint.SystemDefault;
			attr.LfFont = GetFontFromLogFontTiffBytes(dataFont, ref quality);
			attr.FontRenderingHint = quality;
			offsetMark += LOGFONTSIZE;

			attr.BReserved3 = TiffBitConverter.ToUInt32(data, ref offsetMark);

			if (BitConverter.ToInt32(data, offsetMark) > 0)
			{
				attr.Time = TiffBitConverter.ToInt32(data, ref offsetMark);
			}

			attr.BVisible = Convert.ToBoolean(TiffBitConverter.ToInt32(data, ref offsetMark));
			attr.DwReserved4 = TiffBitConverter.ToUInt32(data, ref offsetMark);

			if (attr.DwReserved4 != 1046591)
			{
				throw new Exception("DwReserved4  не валидный");
			}

			int length = (data.Length - offsetMark) / (TiffBitConverter.SizeInt == 1 ? 4 : 2);
			int statOffset = offsetMark;
			attr.LReserved = new int[length];

			for (int i = 0; i < length; i++)
			{
				offsetMark = statOffset + i * (TiffBitConverter.SizeInt == 1 ? 4 : 2);
				attr.LReserved[i] = TiffBitConverter.ToInt32(data, ref offsetMark);
			}

			return attr;
		}

		private static byte[] GetTiffBytesFromRotateStruct(AN_NEW_ROTATE_STRUCT rstruct)
		{
			var result = new List<byte>();

			result.AddBytes(BitConverter.GetBytes(rstruct.Rotation));
			result.AddBytes(BitConverter.GetBytes(rstruct.Scale));
			result.AddBytes(BitConverter.GetBytes(rstruct.NHRes));
			result.AddBytes(BitConverter.GetBytes(rstruct.NVRes));
			result.AddBytes(BitConverter.GetBytes(rstruct.NOrigHRes));
			result.AddBytes(BitConverter.GetBytes(rstruct.NOrigVRes));
			result.AddBytes(BitConverter.GetBytes(Convert.ToInt32(rstruct.BReserved1)));
			result.AddBytes(BitConverter.GetBytes(Convert.ToInt32(rstruct.BReserved2)));

			for (int i = 0; i < rstruct.NReserved.Length; i++)
			{
				result.AddBytes(BitConverter.GetBytes(rstruct.NReserved[i]));
			}

			return [.. result];
		}

		private static AN_NEW_ROTATE_STRUCT CreateRotateStructFromTiffBytes(byte[] data)
		{
			AN_NEW_ROTATE_STRUCT ROTATE_STRUCT = new AN_NEW_ROTATE_STRUCT();
			int offset = 0;
			ROTATE_STRUCT.Rotation = TiffBitConverter.ToInt32(data, ref offset);
			ROTATE_STRUCT.Scale = TiffBitConverter.ToInt32(data, ref offset);
			ROTATE_STRUCT.NHRes = TiffBitConverter.ToInt32(data, ref offset);
			ROTATE_STRUCT.NVRes = TiffBitConverter.ToInt32(data, ref offset);
			ROTATE_STRUCT.NOrigHRes = TiffBitConverter.ToInt32(data, ref offset);
			ROTATE_STRUCT.NOrigVRes = TiffBitConverter.ToInt32(data, ref offset);

			try
			{
				ROTATE_STRUCT.BReserved1 = Convert.ToBoolean(TiffBitConverter.ToInt32(data, ref offset));
				ROTATE_STRUCT.BReserved2 = Convert.ToBoolean(TiffBitConverter.ToInt32(data, ref offset));
			}
			catch { }

			int count = (data.Length - offset) >> 2;
			ROTATE_STRUCT.NReserved = new int[count];

			for (int i = 0; i < count; i++)
			{
				ROTATE_STRUCT.NReserved[i] = TiffBitConverter.ToInt32(data, ref offset);
			}

			return ROTATE_STRUCT;
		}

		private static byte[] GetTiffBytesFormLogFont(LOGFONTA font)
		{
			IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(font));
			Marshal.StructureToPtr(font, ptr, true);
			byte[] result = new byte[Marshal.SizeOf(font)];
			Marshal.Copy(ptr, result, 0, result.Length);
			Marshal.FreeCoTaskMem(ptr);

			return result;
		}

		private static LOGFONTA GetFontFromLogFontTiffBytes(byte[] data, ref TextRenderingHint fontRenderingHint)//не дописан
		{
			LOGFONTA lf = new LOGFONTA();

			if (BitConverter.ToInt32(data, 0) > 0)
			{
				GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);

				try
				{
					IntPtr ptr = handle.AddrOfPinnedObject();
					lf = (LOGFONTA)Marshal.PtrToStructure(ptr, typeof(LOGFONTA));
					fontRenderingHint = (TextRenderingHint)Convert.ToUInt32(lf.lfQuality);
				}
				finally
				{
					handle.Free();
				}

			}

			return lf;
		}
		#endregion
	}


	/// <summary>
	/// полный аналог System.Drawing.Text.TextRenderingHint
	/// </summary>
	public enum TextRenderingHint
	{
		SystemDefault,
		SingleBitPerPixelGridFit,
		SingleBitPerPixel,
		AntiAliasGridFit,
		AntiAlias,
		ClearTypeGridFit
	}
}