using System.Runtime.InteropServices;

namespace ImageEditorControl.Models.TiffAnnotations
{
	/// <summary>
	/// OiAnText для Attach-a-Note, Typed Text, Text From File, Text Stamp, OCR region
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public class OIAN_TEXTPRIVDATA
	{
		/// <summary>
		/// Angle of text baseline to image in tenths of a degree; valid  values are 0, 900, 1800, 2700.
		/// </summary>
		[MarshalAs(UnmanagedType.I4)]
		int nCurrentOrientation;

		/// <summary>
		///  Always 1000 when writing; ignore when reading.
		/// </summary>
		[MarshalAs(UnmanagedType.U4)]
		uint uReserved1;

		/// <summary>
		/// Always 72000 divided by the vertical resolution of the  base image when writing;
		/// Used to modify the Attributes.lfFont.lfHeight variable for display. 
		/// </summary>
		[MarshalAs(UnmanagedType.U4)]
		uint uCreationScale;

		/// <summary>
		/// 64K byte limit (32K for multi- byte data) for Attach-a-Note, typed text, text from file; 255 byte limit for text stamp.
		/// </summary>
		[MarshalAs(UnmanagedType.U4)]
		uint uAnoTextLength;

		/// <summary>
		/// Text string for text mark types.
		/// </summary>
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64000)]
		string szAnoText;

		public uint UReserved1
		{
			get => uReserved1;
			set => uReserved1 = value;
		}
		public uint UCreationScale
		{
			get => uCreationScale;
			set => uCreationScale = value;
		}

		public int NCurrentOrientation
		{
			get => nCurrentOrientation;
			set => nCurrentOrientation = value;
		}

		public uint UAnoTextLength
		{
			get => uAnoTextLength;
			set => uAnoTextLength = value;
		}

		public string SzAnoText
		{
			get => szAnoText;
			set => szAnoText = value; //szAnoText+='\0';
		}

		public OIAN_TEXTPRIVDATA Clone()
		{
			return new OIAN_TEXTPRIVDATA 
			{ 
				nCurrentOrientation = this.nCurrentOrientation, 
				uReserved1 = this.uReserved1,
				uCreationScale = this.uCreationScale,
				uAnoTextLength = this.uAnoTextLength,
				szAnoText = this.szAnoText,
			};	
		}
	}
}