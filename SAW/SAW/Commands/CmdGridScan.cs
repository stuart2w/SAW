using System;
using System.Drawing;
using System.Windows.Forms;
using SAW.CommandEditors;

namespace SAW.Commands
{
	/// <summary>Grid scan command moves the mouse over current screen in a grid, using the switch output.  Parameters are (columns, rows)</summary>
	public class CmdGridScan : ParamBasedCommand, IContinuousCommand
	{
		public CmdGridScan(int columns, int rows) : this()
		{
			ParamList.Add(columns);
			ParamList.Add(rows);
		}

		public CmdGridScan() : base(new[] { Param.ParamTypes.Integer, Param.ParamTypes.Integer })
		{
		}

		internal override ICommandEditor GetEditor() => new GridScanEditor();

		internal override void Execute(ExecutionContext context)
		{
			int rows = Math.Max((int)GetParamAsInt(1), 1);
			m_NumberColumns = Math.Max((int)GetParamAsInt(0), 1);
			Point current = Cursor.Position;
			Rectangle screen = Screen.FromPoint(current).Bounds;
			int width = screen.Width / m_NumberColumns;
			int height = screen.Height / rows;
			m_Rows = new Rectangle[rows];
			m_Areas = new Rectangle[rows * m_NumberColumns];
			for (int y = 0; y < rows; y++)
			{
				m_Rows[y] = new Rectangle(screen.X, screen.Y + y * height, screen.Width, height);
				for (int x = 0; x < m_NumberColumns; x++)
				{
					int index = y * m_NumberColumns + x;
					m_Areas[index] = new Rectangle(screen.X + x * width, m_Rows[y].Top, width, height);
				}
			}
			m_ScanInRow = false;
			m_CurrentIndex = 0;

			m_View = context.View;
			m_View.StartContinuous(this);
			m_Overlay = new OverlayForm(this);
			m_Overlay.Display(screen);
			Cursor.Position = CurrentRect.Centre();
		}

		private Rectangle[] m_Rows;
		private Rectangle[] m_Areas;
		private bool m_ScanInRow; // true if row was selected and now scanning inside it
		private int m_CurrentIndex; // scan index in m_Areas;  can also be used to deduce current row when scanning between them
		private OverlayForm m_Overlay;
		private RunView m_View;
		private int m_NumberColumns;

		/// <summary>Either from m_Rows or m_Areas depending on m_ScanInRow</summary>
		private Rectangle CurrentRect
		{
			get
			{
				if (m_ScanInRow)
					return m_Areas[m_CurrentIndex];
				return m_Rows[m_CurrentIndex / m_NumberColumns];
			}
		}

		/// <summary>Called by the form paint background</summary>
		private void DrawCurrent(PaintEventArgs e)
		{
			var rectArea = new Rectangle(0, 0, m_Overlay.Width, m_Overlay.Height);
			if (m_ScanInRow)
			{ // draw non-row in white now, and row in grey
				e.Graphics.FillRectangle(Brushes.White, rectArea);
				rectArea = m_Overlay.RectangleToClient(m_Rows[m_CurrentIndex / m_NumberColumns]);
			}
			e.Graphics.FillRectangle(Brushes.LightGray, rectArea);
			rectArea = m_Overlay.RectangleToClient(CurrentRect);
			e.Graphics.FillRectangle(Brushes.LightGreen, rectArea);
		}

		#region IContinuousCommand

		void IContinuousCommand.Iterate()
		{
			m_Overlay?.Invalidate();
			if (m_ScanInRow)
			{
				m_CurrentIndex += 1;
				if (m_CurrentIndex % m_NumberColumns == 0) // if this is true we just iterated off the end of a row
					m_CurrentIndex -= m_NumberColumns;
			}
			else
				m_CurrentIndex = (m_CurrentIndex + m_NumberColumns) % m_Areas.Length;
			Cursor.Position = CurrentRect.Centre();
		}

		bool IContinuousCommand.Trigger(bool isRepeat)
		{
			if (m_ScanInRow)
				return false;
			m_ScanInRow = true;
			// m_CurrentIndex is set correctly
			m_Overlay.Invalidate();
			Cursor.Position = CurrentRect.Centre(); // CurrentRect will have changed due to change in m_ScanInRow
			return true;
		}

		void IContinuousCommand.Stop()
		{
			m_View = null;
			m_Overlay?.Dispose();
			m_Overlay = null;
			m_Rows = null;
			m_Areas = null;
		}

		#endregion

		#region Form

		private class OverlayForm : Form
		{
			private CmdGridScan m_Command;

			public OverlayForm(CmdGridScan command)
			{
				m_Command = command;
				TopMost = true;
				Opacity = 0.5;
				StartPosition = FormStartPosition.Manual;
				ShowIcon = false;
				ShowInTaskbar = false;
				ControlBox = false;
				MinimizeBox = false;
				MaximizeBox = false;
				FormBorderStyle = FormBorderStyle.None;
			}

			public void Display(Rectangle bounds)
			{
				Bounds = bounds;
				Show();
			}

			protected override void OnPaintBackground(PaintEventArgs e)
			{
				m_Command.DrawCurrent(e);
			}

		}

		#endregion

	}
}