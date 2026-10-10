namespace ImageEditorControl.Models.TiffAnnotations
{
	/// <summary>
	/// Показан в OiAnoDat, используется для Form, Image Embedded, Image Reference
	/// </summary>
	public class AN_NEW_ROTATE_STRUCT
	{
		/// <summary>
		/// 1=Original
		/// 2=Rotate right (90 degrees clockwise)
		/// 3=Flip (180 degrees clockwise)
		/// 4=Rotate left (270 degrees clockwise)
		/// 5=Vertical mirror (reflected around a 
		/// vertical line)
		/// 6=Vertical mirror + Rotate right
		/// 7=Vertical mirror + Flip
		/// 8=Vertical mirror + Rotate left
		/// </summary>
		public int Rotation { get; set; }

		/// <summary>
		///  Set to 1000.
		/// </summary>
		public int Scale { get; set; }

		/// <summary>
		///  Set to value of nOrigHRes.
		/// </summary>
		public int NHRes { get; set; }

		/// <summary>
		/// Set to value of nOrigVRes.
		/// </summary>
		public int NVRes { get; set; }

		/// <summary>
		/// Resolution of image mark in DPI.
		/// </summary>
		public int NOrigHRes { get; set; }

		/// <summary>
		/// Resolution of image mark in DPI.
		/// </summary>
		public int NOrigVRes { get; set; }

		/// <summary>
		/// Set to 0.
		/// </summary>
		public bool BReserved1 { get; set; }

		/// <summary>
		/// Set to 0.
		/// </summary>
		public bool BReserved2 { get; set; }

		public int[] NReserved { get; set; }

		public AN_NEW_ROTATE_STRUCT()
		{
			NHRes = 96;
			NVRes = 96;
			NOrigHRes = 96;
			NOrigVRes = 96;
			Rotation = 1;
			Scale = 1000;
			BReserved1 = false;
			BReserved2 = false;
			NReserved = new int[0];
		}

		public AN_NEW_ROTATE_STRUCT Clone()
		{
			var res =  new AN_NEW_ROTATE_STRUCT() 
			{
				NHRes = this.NHRes,
				NVRes = this.NVRes,
				NOrigHRes = this.NOrigHRes,
				NOrigVRes = this.NOrigVRes,
				Rotation = this.Rotation,
				Scale = this.Scale,
				BReserved1 = this.BReserved1,
				BReserved2 = this.BReserved2,
				NReserved = new int[this.NReserved.Length],
			};

			Array.Copy(this.NReserved, res.NReserved, this.NReserved.Length);

			return res;
		}
	}
}