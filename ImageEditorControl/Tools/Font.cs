using System.Windows;
using System.Windows.Media;

namespace ImageEditorControl.Tools
{
	public class Font(string fontFamily, double fontSize, FontDecoration fontDecoration = FontDecoration.None, int weight = 400)
	{
		public string FontFamily => fontFamily;
		public double FontSize => fontSize;
		public FontDecoration FontDecoration => fontDecoration;
		public int Weight => weight;

		// Стили шрифта
		public bool IsBold => weight > 700;
		public bool IsItalic => fontDecoration.HasFlag(FontDecoration.Italic);
		public bool IsUnderline => fontDecoration.HasFlag(FontDecoration.Underline);
		public bool IsStrikethrough => fontDecoration.HasFlag(FontDecoration.Strikethrough);

		public Typeface TypeFace
		{
			get 
			{
				var ff = new FontFamily(fontFamily);
				var fs = IsItalic ? FontStyles.Italic : FontStyles.Normal;
				var fw = FontWeight.FromOpenTypeWeight(weight == 0 ? 400 : weight);
				var fst = FontStretches.Normal;
				var tf = new Typeface(ff, fs, fw, fst);

				return tf;
			}
		}
	}
}

[Flags]
public enum FontDecoration
{
	None = 0,
	Italic = 1,
	Underline = 2,
	Strikethrough = 4,
}