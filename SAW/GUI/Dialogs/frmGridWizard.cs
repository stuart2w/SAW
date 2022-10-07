using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using SAW.Shapes;

namespace SAW
{
	internal partial class frmGridWizard : Form
	{
		private int m_Step = 0;
		private TableLayoutPanel[] m_Panels;
		public frmGridWizard()
		{
			InitializeComponent();
			Strings.Translate(this);
			m_Panels = new[] { tblLayout, tblOrder, tblEscape };
			tblOrder.SetBounds(tblLayout.Left, tblLayout.Top, tblLayout.Width, tblLayout.Height);
			tblEscape.SetBounds(tblLayout.Left, tblLayout.Top, tblLayout.Width, tblLayout.Height);
			tblOrder.Bounds = tblLayout.Bounds;
			tblEscape.Bounds = tblLayout.Bounds;
			cmbEscapeRatio.SelectedIndex = 2;
			cmbEscapes.SelectedIndex = 2;
			UpdateButtons();
		}

		public static void Display(IShapeContainer container, Document doc)
		{
			var frm = new frmGridWizard();
			Transaction transaction = new Transaction();
			if (frm.ShowDialog() == DialogResult.OK)
			{
				if (container is Scriptable)
					container = (IShapeContainer) (container as Scriptable).Element; // buttons go in the item not the scriptable!
				transaction.Edit((Datum) container);
				RectangleF bounds;
				if (container is Page)
					bounds = (container as Page).Bounds;
				else
					bounds = (container as Shape).Bounds;
				container.Contents.Clear();
				var layout = GridLayout.CalculateAreas(bounds, (int) frm.nudRows.Value, (int) frm.nudColumns.Value, (int) frm.nudSpacing.Value, frm.SelectedOrder, (GridLayout.Escapes) frm.cmbEscapes.SelectedIndex, frm.SelectedEscapeRatio);
				GenerateLayoutLevel(container, layout, transaction);
				Globals.Root.StoreNewTransaction(transaction, true);
			}
		}

		private static void GenerateLayoutLevel(IShapeContainer container, GridLayout layout, Transaction transaction)
		{
			foreach (var sub in layout.Contents)
			{
				var item = new Item
				{
					ItemType = sub.Type,
					StyleType = Item.DefaultItemDisplayTypeForType(sub.Type),
					LineStyle = {Colour = Color.Black},
					FillStyle = {Colour = Color.LightGray},
					LabelText = ""
				};
				item.SetBounds(sub.Bounds);
				var scriptable = new Scriptable(item);
				container.Contents.Add(scriptable);
				scriptable.Parent = container;
				transaction.Create(item);
				transaction.Create(scriptable);
				scriptable.SelectScript = new Script();
				if (sub.Contents != null)
				{
					scriptable.SelectScript.Visit.VisitType = Script.VisitTarget.VisitTypes.Down;
					GenerateLayoutLevel(item, sub, transaction);
				}
				else
					scriptable.SelectScript.Visit.VisitType = Script.VisitTarget.VisitTypes.Up;
			}
			container.FinishedModifyingContents(transaction);
		}

		#region Pagination
		private void UpdateButtons()
		{
			btnPrevious.Enabled = m_Step > 0;
			if (m_Step == 2 || chkUseDefaults.Checked)
				btnNext.Text = Strings.Item("SAW_Edit_Finish");
			else
				btnNext.Text = Strings.Item("SAW_Edit_Next");
		}

		private void btnPrevious_Click(object sender, EventArgs e)
		{
			m_Step = Math.Max(m_Step - 1, 0);
			DisplayStep();
			UpdatePreview();
		}

		private void btnNext_Click(object sender, EventArgs e)
		{
			if (m_Step == 2 || chkUseDefaults.Checked)
				this.DialogResult = DialogResult.OK;
			else
			{
				m_Step++;
				DisplayStep();
				UpdatePreview();
			}
		}

		private void DisplayStep()
		{
			for (int i = 0; i < m_Panels.Length; i++)
				m_Panels[i].Visible = (i == m_Step);
			UpdateButtons();
		}


		#endregion

		#region Preview

		private void UpdatePreview()
		{
			pnlPreview.Invalidate();
		}

		private void pnlPreview_Paint(object sender, PaintEventArgs e)
		{
			GridLayout.Orders order = GridLayout.Orders.Individual; // on first page always show as individual
			int spacing = chkUseDefaults.Checked ? 4 : (int)nudSpacing.Value;
			if (m_Step >= 1)
				order = SelectedOrder;
			GridLayout.Escapes escapes = m_Step >= 2 ? (GridLayout.Escapes)cmbEscapes.SelectedIndex : GridLayout.Escapes.None;

			var layout = GridLayout.CalculateAreas(new RectangleF(0, 0, pnlPreview.Width - 1, pnlPreview.Height - 1), (int)nudRows.Value, (int)nudColumns.Value, spacing, order, escapes, SelectedEscapeRatio);
			PaintLayoutLevel(e, layout);
		}

		private void PaintLayoutLevel(PaintEventArgs e, GridLayout container)
		{
			foreach (GridLayout item in container.Contents)
			{
				Rectangle rct = item.Bounds.ToRectangle();
				switch (item.Type)
				{
					case Item.ItemTypes.IT_Group:
						e.Graphics.DrawRectangle(Pens.Red, rct);
						break;
					case Item.ItemTypes.IT_Escape:
						e.Graphics.FillRectangle(Brushes.Blue, rct);
						break;
					default: // item
						e.Graphics.FillRectangle(Brushes.White, rct);
						e.Graphics.DrawRectangle(Pens.Black, rct);
						break;
				}
				if (item.Contents != null)
					PaintLayoutLevel(e, item);
			}
		}


		#endregion

		#region Page 1
		private void nudColumns_ValueChanged(object sender, EventArgs e)
		{
			UpdatePreview();
		}

		private void nudRows_ValueChanged(object sender, EventArgs e)
		{
			UpdatePreview();
		}

		private void chkUseDefaults_CheckedChanged(object sender, EventArgs e)
		{
			UpdateButtons();
		}

		#endregion

		#region Page 2

		private GridLayout.Orders SelectedOrder
		{
			get
			{
				if (chkUseDefaults.Checked)
					return GridLayout.Orders.Rows;
				if (rdoIndividual.Checked)
					return GridLayout.Orders.Individual;
				if (rdoColumns.Checked)
					return GridLayout.Orders.Columns;
				return GridLayout.Orders.Rows;
			}
		}
		private void rdoIndividual_CheckedChanged(object sender, EventArgs e)
		{
			UpdatePreview();
			cmbEscapes.Enabled = !rdoIndividual.Checked;
			cmbEscapeRatio.Enabled = !rdoIndividual.Checked;
		}

		private void rdoRows_CheckedChanged(object sender, EventArgs e)
		{
			UpdatePreview();
		}

		private void rdoColumns_CheckedChanged(object sender, EventArgs e)
		{
			UpdatePreview();
		}

		#endregion

		#region Page 3

		private void cmbEscapes_SelectedIndexChanged(object sender, EventArgs e)
		{
			UpdatePreview();
		}

		private void cmbEscapeRatio_SelectedIndexChanged(object sender, EventArgs e)
		{
			UpdatePreview();
		}

		private void nudSpacing_ValueChanged(object sender, EventArgs e)
		{
			UpdatePreview();
		}

		private float SelectedEscapeRatio
		{
			get
			{
				try
				{ // if nothing is selected it would just crash.  Better to just squash errors here
					return int.Parse(cmbEscapeRatio.Text.Trim('%')) / 100f;
				}
				catch
				{return 0.25f;}
			}
		}

		#endregion

	}

	public class GridLayout
	{
		public List<GridLayout> Contents;
		public Item.ItemTypes Type;
		public RectangleF Bounds;

		public enum Orders
		{
			Rows,
			Columns,
			Individual
		}

		[Flags]
		public enum Escapes
		{
			None = 0,
			Start = 1,
			End = 2,
			Both = 3
		}


		public static GridLayout CalculateAreas(RectangleF bounds, int rows, int columns, int spacing, Orders order, Escapes escapes, float escapeRatio)
		{
			GridLayout top = new GridLayout() { Bounds = bounds, Type = Item.ItemTypes.IT_TopItem };

			switch (order)
			{
				case Orders.Individual:
					top.Contents = CalculateSimpleGrid(bounds, rows, columns, spacing, 0, 0, 0, Item.ItemTypes.IT_Item);
					break;
				case Orders.Rows:
					var rowContainers = CalculateSimpleGrid(bounds, rows, 1, spacing, 0, 0, 0, Item.ItemTypes.IT_Group);
					foreach (var row in rowContainers)
					{
						row.Contents = CalculateSimpleGrid(row.Bounds, 1, columns, spacing, 0, escapes, escapeRatio, Item.ItemTypes.IT_Item);
					}
					top.Contents = rowContainers;
					break;
				case Orders.Columns:
					var columnContainers = CalculateSimpleGrid(bounds, 1, columns, spacing, 0, 0, 0, Item.ItemTypes.IT_Group);
					foreach (var row in columnContainers)
					{
						row.Contents = CalculateSimpleGrid(row.Bounds, rows, 1, spacing, escapes, 0, escapeRatio, Item.ItemTypes.IT_Item);
					}
					top.Contents = columnContainers;
					break;
			}
			return top;
		}

		/// <summary>Number of escapes in given code </summary>
		private static int N(Escapes escapes)
		{
			switch (escapes)
			{
				case Escapes.None: return 0;
				case Escapes.Both: return 2;
				default: return 1;
			}
		}

		private static List<GridLayout> CalculateSimpleGrid(RectangleF bounds, int rows, int columns, int spacing, Escapes escapeRows, Escapes escapeColumns, float escapeRatio, Item.ItemTypes type)
		{ // do simple spacing within 1 bounds.  May be used with rows=1 or cols=1 to work within a row/col or to generate rows/cols
		  // returns list of items in grid
			SizeF available = new SizeF(Math.Max(3, bounds.Width - (columns + N(escapeColumns) + 1) * spacing),
			Math.Max(3, bounds.Height - (rows + N(escapeRows) + 1) * spacing));
			SizeF cellSize = new SizeF(available.Width / (columns + N(escapeColumns) * escapeRatio), available.Height / (rows + N(escapeRows) * escapeRatio)); // size of standard cell
			PointF location = new PointF(spacing + bounds.X, spacing + bounds.Y);
			List<GridLayout> list = new List<GridLayout>();
			for (int Y = 0; Y < rows + N(escapeRows); Y++)
			{
				location.X = spacing + bounds.X;
				bool isEscapeRow = Y == 0 && (escapeRows & Escapes.Start) > 0 || Y == rows + N(escapeRows) - 1 && (escapeRows & Escapes.End) > 0;
				SizeF thisSize = cellSize;
				thisSize.Height = isEscapeRow ? cellSize.Height * escapeRatio : cellSize.Height;
				for (int X = 0; X < columns + N(escapeColumns); X++)
				{
					bool isEscapeColumn = X == 0 && (escapeColumns & Escapes.Start) > 0 || X == columns + N(escapeColumns) - 1 && (escapeColumns & Escapes.End) > 0;
					thisSize.Width = isEscapeColumn ? cellSize.Width * escapeRatio : cellSize.Width;
					list.Add(new GridLayout() { Bounds = new RectangleF(location, thisSize), Type = isEscapeColumn || isEscapeRow ? Item.ItemTypes.IT_Escape : type });
					location.X += spacing + thisSize.Width;
				}
				location.Y += spacing + thisSize.Height;
			}
			return list;
		}

	}
}
