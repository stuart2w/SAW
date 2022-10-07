using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SAW.CommandEditors;
using System.Diagnostics;
using SAW.Shapes;

namespace SAW
{
	/// <summary>The floating palette which allows coordinate entry/editing for the selected shape</summary>
	internal partial class ctrCoordEdit : PalettePanel
	{
		private IntegerTextBox[] m_X;
		private IntegerTextBox[] m_Y;
		private Label[] m_Labels;
		private bool m_Filling;
		///// <summary>Once displayed, the actual target identifying the item being edited (can be updated by GetEditableCoords - so doesn't necessarily match the one on the page) </summary>
		//private Target m_DisplayedTarget;
		//private int m_NumberPoints;
		private Shape.GrabSpot[] m_GrabSpots;
		/// <summary>Which index the user has typed in (set to -1 at other times)</summary>
		private int m_IndexTyped = -1;

		public ctrCoordEdit()
		{
			InitializeComponent();
			m_X = new[] { txt1X, txt2X, txt3X, txt4X };
			m_Y = new[] { txt1Y, txt2Y, txt3Y, txt4Y };
			m_Labels = new[] { label1, label2, label3, label4 };
			for (int i = 0; i < 3; i++)
			{
				m_X[i].Tag = i;
				m_Y[i].Tag = i;
				m_X[i].TextChanged += Text_Changed;
				m_Y[i].TextChanged += Text_Changed;
			}
			// there is no selection changed event in globals as such, but this one is effectively raised as needed
			//Globals.ApplicabilityChanged += RefillContent;
			//Globals.UpdateInfo += RefillContent;
			Globals.TransactionStored += Globals_TransactionStored;
		}

		private void Globals_TransactionStored(object sender, Globals.TransactionEventArgs e)
		{ // only update if the transaction is an edit of the shape we are displaying, if any
			if (m_GrabSpots == null || !m_GrabSpots.Any())
				return;
			if (e.Transaction.Contains(m_GrabSpots.First().Shape))
				RefillContent();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
				components?.Dispose();
			base.Dispose(disposing);
			//Globals.ApplicabilityChanged -= RefillContent;
			//Globals.UpdateInfo -= RefillContent;
			Globals.TransactionStored -= Globals_TransactionStored;
		}

		private void Text_Changed(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			int index = (int)((IntegerTextBox)sender).Tag;
			if (m_IndexTyped >= 0 && m_IndexTyped != index) // user has quickly typed in a second box - need to send changes to first one immediately
				tmrTyping_Tick(this, EventArgs.Empty);
			m_IndexTyped = index;
			tmrTyping.Enabled = true;
		}

		private void tmrTyping_Tick(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			tmrTyping.Enabled = false;
			int index = m_IndexTyped;
			m_IndexTyped = -1;
			if (!m_X[index].IsValidAndNotEmpty || (!m_Y[index].IsValidAndNotEmpty && m_Y[index].Visible))
				return;

			Transaction transaction = new Transaction();
			Shape.GrabSpot grab = m_GrabSpots[index];
			PointF point = new PointF(m_X[index].Value, -m_Y[index].Value);
			if (grab.Focus.IsEmpty)
			{// edge with no focus is actually X/Y coords - which is a move.  This needs to be changed to a move as the GrabMove will fail for these without a real focus
				switch (grab.GrabType)
				{
					case Shape.GrabTypes.EdgeMoveH:
						grab = new Shape.GrabSpot(grab.Shape, Shape.GrabTypes.Move, grab.Position);
						point.Y = grab.Position.Y; // no vertical movement
						break;
					case Shape.GrabTypes.EdgeMoveV:
						grab = new Shape.GrabSpot(grab.Shape, Shape.GrabTypes.Move, grab.Position);
						point = new PointF(grab.Position.X, -m_X[index].Value); // only the normal X box was used even though it's a Y coord
						break;
				}
			}
			Page page = grab.Shape.FindPage();
			Shape.GrabMovement move = new Shape.GrabMovement(grab, page, Shape.SnapModes.Off, transaction);
			move.SetGrabMoveTransform(page, grab);
			move.Shape.StartGrabMove(move);

			switch (grab.GrabType)
			{// focus is set if these are width/height logic.  In fact same code can run either way - since if not set then Focus.X/Y = 0
				case Shape.GrabTypes.EdgeMoveH:
					point = new PointF(grab.Focus.X + m_X[index].Value * Math.Sign(grab.Position.X - grab.Focus.X), grab.Focus.Y);
					break;
				case Shape.GrabTypes.EdgeMoveV:
					point = new PointF(grab.Focus.X, grab.Focus.Y + m_X[index].Value * Math.Sign(grab.Position.Y - grab.Focus.Y)); // correct that this still uses m_x - it's always m_X when just one box is used
					break;
			}
			move.Current = new ClickPosition(point, null, 1, Shape.SnapModes.Off, Shape.SnapModes.Off, null, ClickPosition.Sources.Irrelevant);
			move.Transform?.SetGrabTransform(grab.Position, move.Current.Exact);

			move.Shape.GrabMove(move);
			Globals.Root.StoreNewTransaction(transaction, true);
			move.Shape.Parent.NotifyIndirectChange(move.Shape, ChangeAffects.GrabSpots | ChangeAffects.Bounds | ChangeAffects.Diagnostic);
			grab.Position = point;
		}

		internal void RefillContent()
		{ // done on a timer to avoid duplicates
			tmrRefill.Enabled = true;
		}

		private void tmrRefill_Tick(object sender, EventArgs e)
		{
			tmrRefill.Enabled = false;
			tableMain.SuspendLayout();
			m_Filling = true;
			List<Shape> selection = Globals.Root.CurrentPage?.SelectedShapes;
			if (selection == null || selection.Count != 1)
			{
				lblSelect.Visible = true;
				foreach (IntegerTextBox txt in m_X.Union(m_Y))
					txt.Enabled = false;
			}
			else
			{
				lblSelect.Visible = false;
				Shape shape = selection.First();
				Target m_DisplayedTarget = Globals.Root.CurrentPage.SelectedPath; // item within the current shape (can be null)
				string[] names;
				(m_GrabSpots, names) = shape.GetEditableCoords(m_DisplayedTarget);
				for (int i = 0; i <= 3; i++)
				{
					m_Labels[i].Visible = m_X[i].Visible = m_Y[i].Visible = i < names.Length;
					if (i < names.Length)
					{
						m_Labels[i].Text = Strings.Translate(names[i]);
						switch (m_GrabSpots[i].GrabType)
						{
							case Shape.GrabTypes.EdgeMoveH:
								m_X[i].Value = (int)Math.Abs(m_GrabSpots[i].Position.X - m_GrabSpots[i].Focus.X);
								m_Y[i].Visible = false;
								break;
							case Shape.GrabTypes.EdgeMoveV:
								m_X[i].Value = (int)Math.Abs(m_GrabSpots[i].Position.Y - m_GrabSpots[i].Focus.Y);
								m_Y[i].Visible = false;
								break;
							default:
								m_X[i].Value = (int)m_GrabSpots[i].Position.X;
								m_Y[i].Value = -(int)m_GrabSpots[i].Position.Y;
								m_Y[i].Visible = true;
								break;
						}
						m_X[i].Enabled = m_Y[i].Enabled = true;
					}
				}
			}
			m_Filling = false;
			tableMain.ResumeLayout();
		}

	}
}
