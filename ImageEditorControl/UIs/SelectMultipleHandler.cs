using System.Text;

namespace ImageEditorControl.UIs
{
	/// <summary>
	/// класс статических методов парсинга строкового представления выбранных страниц
	/// </summary>
	public static class SelectMultipleHandler
	{
		private static readonly char div1 = ',';
		private static readonly char div2 = '-';

		public static char Div1 => div1;
		public static char Div2 => div2;

		/// <summary>
		/// возвращает строку отображения списка номеров страниц
		/// </summary>
		/// <param name="input">список номеров(нумерация с 0)</param>
		/// <returns>строка представления списка номеров страниц (нумерация с 1)</returns>
		public static string GetPagesSelectedMultipleAsText(List<int> input)
		{
			if (input is null || !input.Any())
				return "";
			else
			{
				var l = input.Distinct().ToList();
				l.Sort();
				var sb = new StringBuilder();

				if (l.Count == 1)
					return $"{l[0] + 1}";
				else
				{
					var start = 0;

					for (var i = 1; i <= l.Count; i++)
						if (i < l.Count && l[i] - l[i - 1] == 1)
							continue;
						else
						{
							var s = start == i - 1 ? $"{l[start] + 1}" : $"{l[start] + 1}-{l[i - 1] + 1}";
							sb.Append(sb.Length == 0 ? s : $",{s}");

							if (i < l.Count)
								start = i;
						}

					return sb.ToString();
				}
			}
		}

		/// <summary>
		/// по строковому представлению списка номеров страниц возвращает список номеров страниц
		/// </summary>
		/// <param name="text">строковое представление номеров страниц (нумерация с 1)</param>
		/// <param name="size">общее число страниц</param>
		/// <param name="result">список номеров страниц (нумерация с 0)</param>
		/// <returns>false если какой-либо парсинг был неуспешен</returns>
		public static bool TryParsePagesSelectedMultipleText(string text, int size, out List<int> result)
		{
			result = new List<int>();
			var hsForResult = new HashSet<int>();

			var intervals = text.Split([div1], StringSplitOptions.RemoveEmptyEntries);

			if (intervals.Length == 0)
				return true;

			foreach (var interval in intervals)
			{
				var numbers = interval.Split([div2], StringSplitOptions.RemoveEmptyEntries);

				if (numbers.Length == 2)
					if (long.TryParse(numbers[0], out long number1) && long.TryParse(numbers[1], out long number2))
						if (number1 <= size && number2 >= number1)
						{
							number2 = number2 > size ? size : number2;

							for (var i = number1; i <= number2; i++)
								hsForResult.Add((int)i - 1);
						}
					else
						return false;
				else if (numbers.Length > 0)//1 превращается в 1. 1-3 превращается в 1,2,3. 1-3-6 превращается в 1,3,6. 
				{
					long f(string x) => long.TryParse(x, out long r) ? r : -1;
					var longNumbers = numbers.Select(x => f(x)).ToList();

					if (longNumbers.Contains(-1))
						return false;

					foreach (var number in longNumbers)
						if (number <= size)
							hsForResult.Add((int)number - 1);
				}
				else
					return false;
			}

			result = new List<int>(hsForResult);
			result.Sort();

			return true;
		}
	}
}
