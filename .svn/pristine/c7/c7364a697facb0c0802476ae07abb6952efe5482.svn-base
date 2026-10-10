using ImageEditorControl.Models;

using System.ComponentModel;

namespace ImageEditorControl.UIs
{
	/// <summary>
	/// класс панели управления множественным выделением страниц модели изображения
	/// </summary>
	//public class PanelSelectionMultiple : FlowLayoutPanel
	//{
	//	private static readonly ComponentResourceManager resources = new ComponentResourceManager(typeof(PanelSelectionMultiple));
	//	private readonly static string deleteSelectedMultiplePagesName = resources.GetString("deleteSelectedMultiplePagesName");
	//	private readonly static string deleteNOTSelectedMultiplePagesName = resources.GetString("deleteNOTSelectedMultiplePagesName");
	//	private readonly static string selectMultiplePagesCaptionName = resources.GetString("selectMultiplePagesCaptionName");
	//	private readonly static string selectAllPagesName = resources.GetString("selectAllPagesName");
	//	private readonly static string deselectAllPagesName = resources.GetString("deselectAllPagesName");
	//	private readonly static string invertPagesSelectionName = resources.GetString("invertPagesSelectionName");

	//	protected bool canReactOnTextChanged = true;//чтобы предотвратить цикл ввод -> изменение текста -> изменение модели -> изменение текста
	//	protected readonly RichTextBox textBoxPagesSelectedMultiple = new RichTextBox();
	//	protected readonly Button buttonPagesSelectionMultipleApply = new Button();
	//	protected readonly ToolTip toolTip = new ToolTip() { AutoPopDelay = 5000, ReshowDelay = 100, };

	//	/// <summary>
	//	/// в редакторе запускает panelView.Focus()
	//	/// </summary>
	//	public event EventHandler BeforeSelectedMultiple;//selectMultipleTimer.Stop() в DocPrintDialog 

	//	/// <summary>
	//	/// в редакторе запускает panelView.Focus()
	//	/// </summary>
	//	public event EventHandler AfterSelectedMultiple;//panelView.Focus() в редакторе 

	//	/// <summary>
	//	/// в редакторе запускает panelView.Focus()
	//	/// </summary>
	//	public event SelectedPageNumbersEventHandler AfterSetTextBoxPagesMulipleSelectedText;//в DocPrintDialog 

	//	/// <summary>
	//	/// в редакторе запускает удаление страниц
	//	/// </summary>
	//	public event DeleteSelectedPagesEventHandler DeleteSelectetPages;

	//	/// <summary>
	//	/// модельный объект, страницами которого управляет панель
	//	/// </summary>
	//	public ImageModel ImageModelCurrent { get; set; }

	//	/// <summary>
	//	/// позволяет разрешать/запрещать операции. в редакторе должно быть эквивалентно !isFreezing
	//	/// </summary>
	//	public bool CanProcess { get; set; } = true;

	//	/// <summary>
	//	/// позволяет разрешать/запрещать удаление выделенных страниц в редакторе должно быть эквивалентно canEdit
	//	/// </summary>
	//	public bool CanDeletePagesOnContentMenu { get; set; } = false;

	//	/// <summary>
	//	/// переход на последнюю выделенную страницу после множественного выделения если true
	//	/// </summary>
	//	public bool SelectLastSelectedMultiplePage { get; set; } = false;

	//	/// <summary>
	//	/// показывает или скрывает кнопку применения изменений с контекстным меню
	//	/// </summary>
	//	public bool ShowButtonPagesSelectedMultipleApply
	//	{ 
	//		get => buttonPagesSelectionMultipleApply.Visible; 
	//		set => buttonPagesSelectionMultipleApply.Visible = value; 
	//	} 

	//	public Size TextBoxSize
	//	{
	//		get => textBoxPagesSelectedMultiple.Size;
	//		set => textBoxPagesSelectedMultiple.Size = value;
	//	}

	//	public PanelSelectionMultiple()
	//	{
	//		InitializeComponent();
	//	}

	//	private void InitializeComponent()
	//	{
	//		textBoxPagesSelectedMultiple.Height = 16;
	//		textBoxPagesSelectedMultiple.Width = 80;
	//		textBoxPagesSelectedMultiple.BorderStyle = BorderStyle.None;
	//		textBoxPagesSelectedMultiple.Margin = new Padding(4, 4, 1, 1);
	//		textBoxPagesSelectedMultiple.Multiline = false;
	//		textBoxPagesSelectedMultiple.MouseEnter += MakePanelSelectionMultipleBorder;
	//		textBoxPagesSelectedMultiple.MouseLeave += ClearPanelSelectionMultipleGraphics;
	//		textBoxPagesSelectedMultiple.KeyPress += FilterTextBoxPagesSelectedMultipleInput;
	//		textBoxPagesSelectedMultiple.PreviewKeyDown += SelectMultiplePagesOnTextChanged;
	//		textBoxPagesSelectedMultiple.LostFocus += SelectMultiplePagesSafeThread;
	//		toolTip.SetToolTip(textBoxPagesSelectedMultiple, selectMultiplePagesCaptionName);

	//		buttonPagesSelectionMultipleApply.SetButtonStyle(Resources.ImageDropDownArrow);
	//		buttonPagesSelectionMultipleApply.BackColor = SystemColors.Control;
	//		buttonPagesSelectionMultipleApply.MouseEnter += MakePanelSelectionMultipleBorder;
	//		buttonPagesSelectionMultipleApply.MouseLeave += ClearPanelSelectionMultipleGraphics;
	//		buttonPagesSelectionMultipleApply.Click += SelectMultiplePagesSafeThread;
	//		buttonPagesSelectionMultipleApply.MouseHover += BuildPagesSelectionMultipleContextMenu;

	//		this.AutoSize = true;
	//		this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
	//		this.BackColor = textBoxPagesSelectedMultiple.BackColor;
	//		this.BorderStyle = BorderStyle.None;
	//		this.Controls.Add(textBoxPagesSelectedMultiple);
	//		this.Controls.Add(buttonPagesSelectionMultipleApply);
	//		this.MouseEnter += MakePanelSelectionMultipleBorder;
	//		this.MouseLeave += ClearPanelSelectionMultipleGraphics;
	//		this.EnabledChanged += (o, e) =>
	//			this.BackColor = this.Enabled ? textBoxPagesSelectedMultiple.BackColor : SystemColors.Control;
	//	}

	//	/// <summary>
	//	/// выставляет текст, соответствующий выделенным страницам в текстбоксе
	//	/// </summary>
	//	public void SetTextBoxPagesMulipleSelectedTextSafeThread(object _ = null, EventArgs __ = null)
	//	{
	//		this.SafeThreadInvoke(() =>
	//		{
	//			List<int> selected = [];

	//			if (ImageModelCurrent is not null && ImageModelCurrent.GetSortedPages().ToList() is List<PageModel> pagesSorted && pagesSorted.Any())
	//			{
	//				selected = ImageModelCurrent.PagesSelectedMultiple
	//				.Where(x => pagesSorted.Contains(x))
	//				.Select(y => pagesSorted.IndexOf(y))
	//				.OrderBy(z => z).ToList();

	//				var newText = SelectMultipleHandler.GetPagesSelectedMultipleAsText(selected);

	//				textBoxPagesSelectedMultiple.SafeThreadInvoke(() => 
	//				{
	//					if (textBoxPagesSelectedMultiple.Text != newText)
	//					{
	//						canReactOnTextChanged = false;
	//						textBoxPagesSelectedMultiple.Text = newText;
	//						textBoxPagesSelectedMultiple.Select(textBoxPagesSelectedMultiple.Text.Length, 0);
	//						textBoxPagesSelectedMultiple.Focus();
	//						canReactOnTextChanged = true;
	//					}
	//				});
	//			}
	//			else
	//			{
	//				textBoxPagesSelectedMultiple.SafeThreadInvoke(() => textBoxPagesSelectedMultiple.Text = "");
	//			}

	//			OnAfterSetTextBoxPagesMulipleSelectedText(selected);
	//		});
	//	}

	//	/// <summary>
	//	/// создаёт элемент контекстного меню для удаления выделенных страниц
	//	/// </summary>
	//	/// <param name="cm">контекстное меню</param>
	//	public void CreateDeleteSelectedMuliplePagesMenu(ContextMenuStrip cm)
	//	{
	//		var menuDeleteSelectedPages = new ToolStripMenuItem { Text = deleteSelectedMultiplePagesName };//
	//		cm.Items.Add(menuDeleteSelectedPages);

	//		menuDeleteSelectedPages.Click += (_, __) =>
	//		{
	//			if (CanProcess && ImageModelCurrent is not null && ImageModelCurrent.IsSelectedMultiple)
	//			{
	//				var selected = ImageModelCurrent.PagesSelectedMultiple.OrderBy(x => x.PageNumberCurrent).ToList();
	//				ImageModelCurrent.DeselectMultipleAllPages();
	//				OnDeleteSelectedPages(selected);
	//			}

	//			OnAfterSelectedMultiple();
	//		};
	//	}

	//	/// <summary>
	//	/// создаёт элемент контекстного меню для удаления всех страниц кроме выделенных
	//	/// </summary>
	//	/// <param name="cm">контекстное меню</param>
	//	public void CreateDeleteNOTSelectedMuliplePagesMenu(ContextMenuStrip cm)
	//	{
	//		var menuDeleteNotSelectedPages = new ToolStripMenuItem { Text = deleteNOTSelectedMultiplePagesName };//перенос
	//		cm.Items.Add(menuDeleteNotSelectedPages);

	//		menuDeleteNotSelectedPages.Click += (_, __) =>
	//		{
	//			if (CanProcess && ImageModelCurrent is not null && ImageModelCurrent.IsSelectedMultiple)
	//			{
	//				var notSelected = ImageModelCurrent.PagesNOTSelectedMultiple.OrderBy(x => x.PageNumberCurrent).ToList();
	//				ImageModelCurrent.DeselectMultipleAllPages();
	//				OnDeleteSelectedPages(notSelected);
	//			}

	//			OnAfterSelectedMultiple();
	//		};
	//	}

	//	/// <summary>
	//	/// выделение помеченных на выделение страниц
	//	/// </summary>
	//	public virtual void SelectMultiplePagesSafeThread(object _ = null, EventArgs __ = null)
	//	{
	//		this.SafeThreadInvoke(() =>
	//		{
	//			if (
	//				ImageModelCurrent is not null &&
	//				ImageModelCurrent.GetSortedPages().ToList() is List<PageModel> pagesSorted && pagesSorted.Any())
	//			{
	//				OnBeforeSelectedMultiple();

	//				if (SelectMultipleHandler.TryParsePagesSelectedMultipleText(
	//					textBoxPagesSelectedMultiple.Text, pagesSorted.Count, out List<int> parsed) && parsed.Any())
	//				{
	//					PageModel lastPage = ImageModelCurrent.GetPageModelCurrent();

	//					for (var i = 0; i < pagesSorted.Count; i++)
	//					{
	//						var pm = pagesSorted[i];
	//						pm.IsSelectedMultiple = parsed.Contains(i);
	//						lastPage = pm.IsSelectedMultiple ? pm : lastPage;
	//					}

	//					if (SelectLastSelectedMultiplePage && lastPage is not null)//последняя отмеченная страница
	//					{
	//						lastPage.IsSelectedSingle = true;
	//					}
	//				}

	//				//проверка, нужно ли менять текст.
	//				//иногда текст м.б. новым, но ни одного события об изменении коллекции выделенных страниц не вызывается.
	//				//например, ранее уже были выбраны все страницы (1-8), а мы выбрали больший диапазон, чем всего страниц (напр., 1-20).
	//				var selectedNew = ImageModelCurrent.PagesSelectedMultiple
	//					.Where(x => pagesSorted.Contains(x))
	//					.Select(y => pagesSorted.IndexOf(y))
	//					.OrderBy(z => z).ToList();
	//				var newText = SelectMultipleHandler.GetPagesSelectedMultipleAsText(selectedNew);

	//				//тогда надо переписать значение текстбокса
	//				if (textBoxPagesSelectedMultiple.Text != newText)
	//				{
	//					textBoxPagesSelectedMultiple.Text = newText;
	//				}

	//				OnAfterSelectedMultiple();
	//			}
	//		});
	//	}

	//	/// <summary>
	//	/// вызывает событие для удаления выделенных страниц
	//	/// </summary>
	//	/// <param name="pagesToDelete">страницы, подлежащие удалению</param>
	//	protected virtual void OnDeleteSelectedPages(List<PageModel> pagesToDelete) =>
	//		DeleteSelectetPages?.Invoke(this, new DeleteSelectedPagesEventArgs(pagesToDelete));

	//	/// <summary>
	//	/// вызывает событие после выделения страниц
	//	/// </summary>
	//	protected virtual void OnAfterSelectedMultiple() => AfterSelectedMultiple?.Invoke(this, EventArgs.Empty);

	//	/// <summary>
	//	/// вызывает событие после выделения страниц
	//	/// </summary>
	//	protected virtual void OnBeforeSelectedMultiple() => BeforeSelectedMultiple?.Invoke(this, EventArgs.Empty);

	//	/// <summary>
	//	/// вызывает событие после изменение текста текстбокса
	//	/// </summary>
	//	protected virtual void OnAfterSetTextBoxPagesMulipleSelectedText(List<int> selected) => 
	//		AfterSetTextBoxPagesMulipleSelectedText?.Invoke(this, new SelectedPageNumbersEventArgs(selected));

	//	/// <summary>
	//	/// проверка и фильтрация символов ввода
	//	/// </summary>
	//	protected virtual void FilterTextBoxPagesSelectedMultipleInput(object _, KeyPressEventArgs e)
	//	{
	//		if (!canReactOnTextChanged)
	//		{
	//			return;
	//		}

	//		var s = e.KeyChar;
	//		e.Handled = !(char.IsDigit(s) || s == SelectMultipleHandler.Div1 || s == SelectMultipleHandler.Div2);
	//	}

	//	#region design
	//	private void MakePanelSelectionMultipleBorder(object _, EventArgs __)
	//	{
	//		var g1 = this.CreateGraphics();
	//		var rec = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
	//		var pen = new Pen(SystemColors.ControlDark, 1);
	//		g1.DrawRectangle(pen, rec);
	//		var g2 = buttonPagesSelectionMultipleApply.CreateGraphics();
	//		g2.DrawLine(pen, 0, 0, 0, rec.Height);
	//	}

	//	private void ClearPanelSelectionMultipleGraphics(object _, EventArgs __)
	//	{
	//		var g1 = this.CreateGraphics();
	//		g1.Clear(this.BackColor);
	//		var g2 = buttonPagesSelectionMultipleApply.CreateGraphics();
	//		var h = buttonPagesSelectionMultipleApply.Height;
	//		g2.DrawLine(new Pen(buttonPagesSelectionMultipleApply.BackColor, 1), 0, 0, 0, h - 1);
	//	}
	//	#endregion

	//	#region selection
	//	private void SelectMultiplePagesOnTextChanged(object _, EventArgs e)
	//	{
	//		if (e is PreviewKeyDownEventArgs pe && pe.KeyData == Keys.Enter)//запуск по Enter
	//		{
	//			SelectMultiplePagesSafeThread();
	//		}
	//	}
	//	#endregion

	//	#region context menu
	//	private void BuildPagesSelectionMultipleContextMenu(object o, EventArgs _)
	//	{
	//		if (
	//			o is Button b && b == buttonPagesSelectionMultipleApply &&
	//			textBoxPagesSelectedMultiple is not null &&
	//			ImageModelCurrent is not null)
	//		{
	//			var timer = new Timer() { Interval = 500 };

	//			timer.Tick += (o, e) =>
	//			{
	//				timer.Stop();

	//				SelectMultiplePagesSafeThread();

	//				var cm = new ContextMenuStrip() { ShowImageMargin = false, BackColor = SystemColors.Control, AutoClose = true };
	//				var selected = ImageModelCurrent.PagesSelectedMultiple.ToList();
	//				var chosen = ImageModelCurrent.PagesChosen.ToList();

	//				if (selected.Any())
	//				{
	//					CreateSelectAllPagesMenu(cm, false);
	//				}

	//				if (selected.Count != chosen.Count)
	//				{
	//					CreateSelectAllPagesMenu(cm, true);
	//				}

	//				if (selected.Count > 0 && selected.Count < chosen.Count)
	//				{
	//					CreateInvertPagesSelectionMenu(cm);

	//					if (CanDeletePagesOnContentMenu)
	//					{
	//						CreateDeleteSelectedMuliplePagesMenu(cm);
	//						CreateDeleteNOTSelectedMuliplePagesMenu(cm);
	//					}
	//				}

	//				if (cm.Items.Count > 0)
	//				{
	//					cm.MouseLeave += (__, ___) => cm.Close();
	//					cm.Show(b, new Point(b.Width, b.Height));
	//				}

	//				timer.Dispose();
	//			};

	//			timer.Start();
	//		}
	//	}

	//	private void CreateSelectAllPagesMenu(ContextMenuStrip cm, bool select)
	//	{
	//		var menuSelectAllPages = new ToolStripMenuItem { Text = select ? selectAllPagesName : deselectAllPagesName };
	//		cm.Items.Add(menuSelectAllPages);

	//		menuSelectAllPages.Click += (_, __) =>
	//		{
	//			if (
	//				CanProcess &&
	//				ImageModelCurrent is not null &&
	//				ImageModelCurrent.PagesChosen.ToList() is List<PageModel> chosen &&
	//				chosen.Any())
	//			{
	//				foreach (var pm in chosen)
	//				{
	//					pm.IsSelectedMultiple = select;
	//				}

	//				OnAfterSelectedMultiple();
	//			}
	//		};
	//	}

	//	private void CreateInvertPagesSelectionMenu(ContextMenuStrip cm)
	//	{
	//		var menuInvertPagesSelection = new ToolStripMenuItem { Text = invertPagesSelectionName };
	//		cm.Items.Add(menuInvertPagesSelection);

	//		menuInvertPagesSelection.Click += (_, __) =>
	//		{
	//			if (
	//				CanProcess &&
	//				ImageModelCurrent is not null &&
	//				ImageModelCurrent.PagesChosen.ToList() is List<PageModel> chosen &&
	//				chosen.Any())
	//			{
	//				foreach (var pm in chosen)
	//				{
	//					pm.IsSelectedMultiple = !pm.IsSelectedMultiple;
	//				}
	//			}

	//			OnAfterSelectedMultiple();
	//		};
	//	}
	//	#endregion
	//}

	//public class DeleteSelectedPagesEventArgs(List<PageModel> pagesToDelete) : EventArgs
	//{
	//	public List<PageModel> PagesToDelete { get; private set; } = pagesToDelete;
	//}

	//public delegate void DeleteSelectedPagesEventHandler(object sender, DeleteSelectedPagesEventArgs e);

	//public class SelectedPageNumbersEventArgs(List<int> selected) : EventArgs
	//{
	//	public List<int> Selected { get; private set; } = selected;
	//}

	//public delegate void SelectedPageNumbersEventHandler(object sender, SelectedPageNumbersEventArgs e);
}