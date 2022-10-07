using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SAW.Shapes;

namespace SAW
{
	internal partial class ctrRotation : PalettePanel
	{
		private bool m_Filling;
		/// <summary>True when this invokes the global event (so we can ignore our own invocation) </summary>
		private bool m_TriggeringEvent;

		public ctrRotation()
		{
			m_Filling = true;
			InitializeComponent();
			AddGripper();
			if (TransformRotate.RotateAboutCentres)
				rdoCentres.Checked = true;
			else
				rdoPoint.Checked = true;
			chkIncludeText.Checked = TransformRotate.IncludeText;
			Globals.RotationInfoChanged += Globals_RotationInfoChanged;
			Globals.Root.CurrentPageChanged += Globals_RotationInfoChanged;
			Globals.ApplicabilityChanged += Globals_ApplicabilityChanged;
			m_Filling = false;
		}

		#region Global events

		protected override void Dispose(bool disposing)
		{
			if (disposing)
				components?.Dispose();
			m_ctxPrevious?.Dispose();
			Globals.RotationInfoChanged -= Globals_RotationInfoChanged;
			Globals.Root.CurrentPageChanged -= Globals_RotationInfoChanged;
			Globals.ApplicabilityChanged -= Globals_ApplicabilityChanged;
			base.Dispose(disposing);
		}

		private void OnChangeMade()
		{
			m_TriggeringEvent = true;
			try
			{
				Globals.OnRotationInfoChanged();
			}
			finally
			{
				m_TriggeringEvent = false;
			}
		}

		private void Globals_RotationInfoChanged()
		{
			if (m_Filling || m_TriggeringEvent)
				return;
			btnPreviousPoints.Enabled = (Globals.Root.CurrentPage?.RecentRotationPoints?.Count ?? 0) > 1;
			if (!(Globals.Root.CurrentPage?.RecentRotationPoints?.Any() ?? false))
				return;
			PointF point = Globals.Root.CurrentPage.RecentRotationPoints.First();
			m_Filling = true;
			txtX.Text = point.X.ToString("0");
			txtY.Text = (-point.Y).ToString("0");
			m_Filling = false;
		}

		private void Globals_ApplicabilityChanged()
		{
			btnDoRotate.Enabled = (Globals.Root.CurrentPage?.SelectedCount ?? 0) > 0
								  && (TransformRotate.IncludeText || Globals.Root.CurrentPage.Any(s => !TransformRotate.ShapeCountsAsText(s)));
		}

		#endregion

		#region Other control events

		private void rdoPoint_CheckedChanged(object sender, EventArgs e)
		{
			txtX.Enabled = txtY.Enabled = rdoPoint.Checked;
			// we must manually implement the radio group as they are in different containers, so the default behaviour doesn't apply
			if (rdoPoint.Checked)
				rdoCentres.Checked = false;
		}

		private void btnSet_Click(object sender, EventArgs e)
		{
			Globals.Root.Editor.pnlView.StartCustomShape(new SetRotation());
		}

		private void rdoCentres_CheckedChanged(object sender, EventArgs e)
		{
			if (rdoCentres.Checked)
				rdoPoint.Checked = false;
			if (m_Filling)
				return;
			TransformRotate.RotateAboutCentres = rdoCentres.Checked;
		}

		private void chkIncludeText_CheckedChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			TransformRotate.IncludeText = chkIncludeText.Checked;
		}

		/// <summary>Called by ToggleTextRotate verb if it changes the option so the UI can be updated </summary>
		public void IncludeTextChanged()
		{
			chkIncludeText.Checked = TransformRotate.IncludeText;
		}

		#endregion

		#region User typing

		private void txtXY_TextChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			tmrTextChange.Enabled = true;
		}

		private void tmrTextChange_Tick(object sender, EventArgs e)
		{
			if (m_Filling || Globals.Root.CurrentPage == null)
				return;
			tmrTextChange.Enabled = false;
			m_Filling = true;
			if (txtX.IsValidAndNotEmpty && txtY.IsValidAndNotEmpty)
			{
				Globals.Root.Editor?.pnlView?.ConcludeOngoing();
				PointF point = new PointF(txtX.Value, -txtY.Value);
				Globals.Root.CurrentPage.SetCurrentRotationPoint(point, true);
				if (Globals.Root.CurrentPage.SelectedShapes.Any())
					Globals.Root.Editor?.pnlView?.ForceUpdateGrabSpots();
			}
			m_Filling = false;
		}

		#endregion

		private void btnDoRotate_Click(object sender, EventArgs e)
		{
			int angle = 0;
			Page page = Globals.Root.CurrentPage;
			if (txtAngle.IsValidAndNotEmpty)
				angle = txtAngle.Value;
			if (angle == 0 || angle <= -360 || angle >= 360)
			{
				MessageBox.Show(Strings.Item("SAW_Rotate_BadAngle"));
				return;
			}
			// this can use the point in the page, rather than the GUI, since the GUI updates the page automatically
			if (rdoPoint.Checked && !page.RecentRotationPoints.Any())
			{
				MessageBox.Show(Strings.Item("SAW_Rotate_NoPosition"));
				return;
			}
			if (page.SelectedShapes.Any(s => (s.Allows & Shape.AllowedActions.TransformRotate) == 0))
			{
				MessageBox.Show(Strings.Item("SAW_Rotate_Cannot"));
				return;
			}
			TransformRotate rotate = new TransformRotate(page.RecentRotationPoints.FirstOrDefault(), Geometry.Radians(angle));
			Transaction transaction = new Transaction();
			if (rdoPoint.Checked)
				page.RotationPointUsed = true;
			foreach (Shape s in page.SelectedShapes)
			{
				transaction.Edit(s);
				rotate.DoTransform(s);
			}
			Globals.Root.StoreNewTransaction(transaction, true);
		}

		#region Previous popup menu

		private ContextMenu m_ctxPrevious;

		private void btnPreviousPoints_Click(object sender, EventArgs e)
		{
			m_ctxPrevious?.Dispose();
			m_ctxPrevious = new ContextMenu();
			foreach (PointF point in Globals.Root.CurrentPage.RecentRotationPoints.Skip(1))
			{
				m_ctxPrevious.MenuItems.Add(new MenuItem($"({point.X},{-point.Y})", Previous_Clicked) { Tag = point });
			}
			m_ctxPrevious.Show(btnPreviousPoints, new Point(0, btnPreviousPoints.Height));
		}

		private void Previous_Clicked(object sender, EventArgs a)
		{
			PointF point = (PointF) (sender as MenuItem).Tag;
			txtX.Text = point.X.ToString();
			txtY.Text = point.Y.ToString();
			// and then we can let the normal event handlers do everything else
		}

		#endregion

	}
}
