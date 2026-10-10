namespace ImageEditorControl.Models.TiffAnnotations
{
	public static class TiffBitConverter
	{
		public static Int32 SizeInt = 1;

		/// <summary>
		/// Returns a 32-bit unsigned integer converted from four bytes at a specified position in a byte array.
		/// </summary>
		/// <param name="value">An array of bytes.</param>
		/// <param name="startIndex">The starting position within value.</param>
		/// <returns>A 32-bit unsigned integer formed by four bytes beginning at startIndex.</returns>
		/// <exception cref="System.ArgumentNullException">value is null</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">startIndex is less than zero or greater than the length of value minus 4</exception>
#pragma warning disable 3021
		[CLSCompliant(false)]
		public static uint ToUInt32(byte[] value, ref int startIndex)
		{
			if (Convert.ToBoolean(SizeInt))
			{
				uint result = BitConverter.ToUInt32(value, startIndex);
				startIndex += 4;
				return result;
			}
			else
			{
				uint result = BitConverter.ToUInt16(value, startIndex);
				startIndex += 2;
				return result;
			}
		}
#pragma warning restore 3021

		/// <summary>
		/// Returns a 32-bit signed integer converted from four bytes at a specified position in a byte array.
		/// </summary>
		/// <param name="value">An array of bytes.</param>
		/// <param name="startIndex">The starting position within value.</param>
		/// <returns> A 32-bit signed integer formed by four bytes beginning at startIndex.</returns>
		/// <exception cref="System.ArgumentNullException">value is null</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">startIndex is less than zero or greater than the length of value minus 4</exception>
		public static int ToInt32(byte[] value, ref int startIndex)
		{
			if (Convert.ToBoolean(SizeInt))
			{
				int result = BitConverter.ToInt32(value, startIndex);
				startIndex += 4;
				return result;
			}
			else
			{
				int result = BitConverter.ToInt16(value, startIndex);
				startIndex += 2;
				return result;
			}
		}
	}
}