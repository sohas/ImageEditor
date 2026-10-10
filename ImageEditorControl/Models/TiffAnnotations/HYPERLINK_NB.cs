namespace ImageEditorControl.Models.TiffAnnotations
{
	/// <summary>
	/// OiHypLnk 
	/// </summary>
#pragma warning disable 169
	public class HYPERLINK_NB
	{
		/// <summary>
		/// The version number of this data.
		/// </summary>
		int nVersion;
		/// <summary>
		/// The size of the link string in bytes
		/// </summary>
		int nLinkSize;
		/// <summary>
		/// The variable length multi-byte name string.
		/// </summary>
		char szLinkString;

		/// <summary>
		/// The size of the location string.
		/// </summary>
		int nLocationSize;
		/// <summary>
		/// The variable length multi-byte location string.
		/// </summary>
		char szLocationString;

		/// <summary>
		/// The size of the working directory string.
		/// </summary>
		int nWorkDirSize;

		/// <summary>
		/// The variable length multi-byte working directory string.
		/// </summary>
		char szWorkDirString;

		/// <summary>
		/// One or more of the following flags
		/// 1 = Can remove hyperlink from mark.
		/// 2 = Hyperlink refers to this document.
		/// </summary>
		int nFlags;

		public HYPERLINK_NB Clone()
		{
			return new HYPERLINK_NB
			{
				nVersion = this.nVersion,
				nLinkSize = this.nLinkSize,
				szLinkString = this.szLinkString,
				nLocationSize = this.nLocationSize,
				szLocationString = this.szLocationString,
				nWorkDirSize = this.nWorkDirSize,
				szWorkDirString = this.szWorkDirString,
				nFlags = this.nFlags
			};
		}
	}
#pragma warning restore 169
}