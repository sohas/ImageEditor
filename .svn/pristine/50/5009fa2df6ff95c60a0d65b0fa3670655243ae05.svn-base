using ImageEditorControl.Tools;
using System.Runtime.InteropServices;

namespace ImageEditorControl.Models.TiffAnnotations
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public class LOGFONTA
	{
		public int lfHeight = 0;
		public int lfWidth = 0;
		public int lfEscapement = 0;
		public int lfOrientation = 0;
		public int lfWeight = 0;
		public byte lfItalic = 0;
		public byte lfUnderline = 0;
		public byte lfStrikeOut = 0;
		public byte lfCharSet = 0;
		public byte lfOutPrecision = 0;
		public byte lfClipPrecision = 0;
		public byte lfQuality = 0;
		public byte lfPitchAndFamily = 0;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] //private const Int32 LF_FACESIZE = 32
		public string lfFaceName = null;

		public LOGFONTA() { }

		public LOGFONTA Clone()
		{
			var res = new LOGFONTA
			{
				lfHeight = lfHeight,
				lfWidth = lfWidth,
				lfEscapement = lfEscapement,
				lfOrientation = lfOrientation,
				lfWeight = lfWeight,
				lfItalic = lfItalic,
				lfUnderline = lfUnderline,
				lfStrikeOut = lfStrikeOut,
				lfCharSet = lfCharSet,
				lfOutPrecision = lfOutPrecision,
				lfClipPrecision = lfClipPrecision,
				lfQuality = lfQuality,
				lfPitchAndFamily = lfPitchAndFamily,
				lfFaceName = lfFaceName
			};

			return res;
		}

		public LOGFONTA(Font font)
		{
			lfItalic = (byte)(font.IsItalic ? 1 : 0);
			lfStrikeOut = (byte)(font.IsStrikethrough ? 1 : 0);
			lfUnderline = (byte)(font.IsUnderline ? 1 : 0);
			lfWeight = font.Weight;
			lfFaceName = font.FontFamily;
			lfHeight = (int)font.FontSize;
		}

		public LOGFONTA(LOGFONTW font)
		{
			lfHeight = font.lfHeight;
			lfWidth = font.lfWidth;
			lfEscapement = font.lfEscapement;
			lfOrientation = font.lfOrientation;
			lfWeight = font.lfWeight;
			lfItalic = font.lfItalic;
			lfUnderline = font.lfUnderline;
			lfStrikeOut = font.lfStrikeOut;
			lfCharSet = font.lfCharSet;
			lfOutPrecision = font.lfOutPrecision;
			lfClipPrecision = font.lfClipPrecision;
			lfQuality = font.lfQuality;
			lfPitchAndFamily = font.lfPitchAndFamily;
			lfFaceName = font.lfFaceName;
		}

		public Font GetFont()
		{
			var fs = (FontDecoration)0;
			fs |= (lfItalic == 1 ? FontDecoration.Italic : 0);
			fs |= (lfStrikeOut == 1 ? FontDecoration.Strikethrough : 0);
			fs |= (lfUnderline == 1 ? FontDecoration.Underline : 0);

			if (string.IsNullOrEmpty(lfFaceName) || lfHeight <= 0)
			{
				return null;
			}

			return new Font(lfFaceName, lfHeight, fs, lfWeight);
		}
	}
}