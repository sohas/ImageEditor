using ImageEditorControl.Models;

using static ImageEditorControl.UIs.ImageEditor;

namespace ImageEditorControl.DisplayTypes
{
	public class DrawSizeTypeScaleDisplay : IComparable
	{
		private static readonly double[] standardScales = new double[]
		{
				0.1,
				0.25,
				0.33,
				0.5,
				0.75,
				1.0,
				2.0,
				5.0,
		};

		public DrawSizeType Type
		{
			get; private set;
		}

		public double Scale
		{
			get; private set;
		} = 1;

		public bool IsStandard => standardScales.Contains(Scale);
		public bool IsCustom => Type == DrawSizeType.Custom;
		public string Name => GetDrawSizeTypeName(Type, Scale);

		public DrawSizeTypeScaleDisplay(DrawSizeType type, double scale = 1)
		{
			this.Type = type;
			this.Scale = scale;
		}

		public DrawSizeTypeScaleDisplay(DrawSizeTypeScaleDisplay copy)
		{
			this.Type = copy.Type;
			this.Scale = copy.Scale;
		}

		public override bool Equals(object obj) => obj is DrawSizeTypeScaleDisplay dstd && dstd.Type == Type && dstd.Scale == Scale;
		public override int GetHashCode() => Type.GetHashCode() ^ Scale.GetHashCode();
		public override string ToString() => $"{Type}{(Type == DrawSizeType.Custom ? $", {Scale * 100: 0}%" : "")}";

		public int CompareTo(object obj)
		{
			if (obj is DrawSizeTypeScaleDisplay dstd)
			{
				if (Type == dstd.Type)
				{
					return Scale.CompareTo(dstd.Scale);
				}
				else
				{
					var t = ((DrawSizeType[])Enum.GetValues(typeof(DrawSizeType))).ToList();
					return t.IndexOf(Type).CompareTo(t.IndexOf(dstd.Type));
				}
			}

			throw new NotImplementedException();
		}

		public static DrawSizeTypeScaleDisplay[] DrawSizeTypeDisplaysStandard =>
		[
			.. standardScales
			.Select(x => new DrawSizeTypeScaleDisplay(DrawSizeType.Custom, x))
,
			.. ((DrawSizeType[])Enum.GetValues(typeof(DrawSizeType)))
			.Where(x => x != DrawSizeType.Custom)
			.Select(y => new DrawSizeTypeScaleDisplay(y)),
		];
	}
}