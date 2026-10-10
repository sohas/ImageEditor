using System.Runtime.InteropServices;

namespace ImageEditorControl.Models.TiffAnnotations
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public class LOGFONTW
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

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32 / 2)] //private const Int32 LF_FACESIZE = 32
		public string lfFaceName = null;

		public LOGFONTW() { }

		public LOGFONTW(LOGFONTA font)
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
	}
}