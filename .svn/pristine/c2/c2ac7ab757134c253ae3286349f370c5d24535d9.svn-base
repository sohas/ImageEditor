using System.Windows;

namespace ImageEditorControl.Models.TiffAnnotations
{
	/// <summary>
	/// Показан в OiAnoDat, используется для Straight Line, Freehand Line
	/// </summary>
	public class AN_POINTS
	{
		/// <summary>
		/// Points marking the beginning and 
		/// ending of the line segment(s); in 
		/// FULLSIZE (not scaled) coordinates 
		/// relative to the upper left corner 
		/// of lrBounds in 
		/// OIAN_MARK_ATTRIBUTES.
		/// </summary>
		public Point[] PtPoint { get; set; }

		/// <summary>
		/// The current number of points.
		/// </summary>
		public int NPoints { get; set; }

		/// <summary>
		/// The maximum number of points; must 
		/// be equal to the value of nPoints.
		/// </summary>
		public int NMaxPoints { get; set; }

		public AN_POINTS Clone() 
		{
			var res = new AN_POINTS
			{
				PtPoint = new Point[this.PtPoint.Length],
				NPoints = this.NPoints,
				NMaxPoints = this.NMaxPoints,
			};

			Array.Copy(this.PtPoint, res.PtPoint, this.PtPoint.Length);

			return res;
		}
	}
}