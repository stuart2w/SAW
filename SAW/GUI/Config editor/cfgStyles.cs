using System;
using System.Drawing;
using System.Windows.Forms;

namespace SAW
{
	public partial class cfgStyles : ConfigPage
	{
		private Panel[] m_Previews;
		private Button[] m_EditButtons;

		public cfgStyles()
		{
			InitializeComponent();
			Strings.Translate(this);

			pnlOutputNormal.Tag = new Preview(SAW.Item.ItemDisplayTypes.IDT_Output, ButtonShape.States.Normal);
			pnlOutputHigh.Tag = new Preview(SAW.Item.ItemDisplayTypes.IDT_Output, ButtonShape.States.Highlight);
			pnlGroupNormal.Tag = new Preview(SAW.Item.ItemDisplayTypes.IDT_Group, ButtonShape.States.Normal);
			pnlGroupHigh.Tag = new Preview(SAW.Item.ItemDisplayTypes.IDT_Group, ButtonShape.States.Highlight);
			pnlEscapeNormal.Tag = new Preview(SAW.Item.ItemDisplayTypes.IDT_Escape, ButtonShape.States.Normal);
			pnlEscapeHighlight.Tag = new Preview(SAW.Item.ItemDisplayTypes.IDT_Escape, ButtonShape.States.Highlight);
			pnlHelpNormal.Tag = new Preview(SAW.Item.ItemDisplayTypes.IDT_Help, ButtonShape.States.Normal);
			pnlHelpHighlight.Tag = new Preview(SAW.Item.ItemDisplayTypes.IDT_Help, ButtonShape.States.Highlight);
			pnlNotScannedNormal.Tag = new Preview(SAW.Item.ItemDisplayTypes.IDT_NotScanned, ButtonShape.States.Normal);
			pnlNotScannedHighlight.Tag = new Preview(SAW.Item.ItemDisplayTypes.IDT_NotScanned, ButtonShape.States.Highlight);

			m_Previews = new[] { pnlOutputNormal, pnlOutputHigh, pnlGroupNormal, pnlGroupHigh, pnlEscapeNormal, pnlEscapeHighlight, pnlHelpNormal, pnlHelpHighlight, pnlNotScannedNormal, pnlNotScannedHighlight };
			foreach (Panel p in m_Previews)
				p.Paint += PreviewPanel_Paint;
			m_EditButtons = new[] { btnEditOutput, btnEditGroup, btnEditEscape, btnEditHelp, btnEditNotScanned };
			foreach (Button b in m_EditButtons)
				b.Click += EditButton_Click;
		}

		public override void OnDisplay()
		{
			m_Filling = true;
			base.OnDisplay();
			rdoOff.Checked = !m_Applied.ReadBoolean(Config.SAW_EnableItemStyles);
			rdoOn.Checked = m_Applied.ReadBoolean(Config.SAW_EnableItemStyles);
			m_Filling = false;
		}

		private class Preview
		{
			public readonly SAW.Item.ItemDisplayTypes Style;
			public readonly ButtonShape.States State;

			public Preview(SAW.Item.ItemDisplayTypes style, ButtonShape.States state)
			{
				Style = style;
				State = state;
			}
		}

		//private Preview GetPreviewFromIndex(int index)
		//{// uses panel/button list to  get the settings for this line
		//	return (Preview)m_Previews[index * 2].Tag;
		//}

		private void PreviewPanel_Paint(object sender, PaintEventArgs e)
		{
			Panel panel = (Panel)sender;
			Preview info = (Preview)(sender as Control).Tag;
			NetCanvas canvas = new NetCanvas(e.Graphics);
			//using (var back = canvas.CreateFill(
			var style = m_Config.ButtonStyle[(int)(info.Style + 2)];
			Shape.DrawResources resources = new Shape.DrawResources(1, 255, 255, false, false, canvas, StaticView.InvalidationBuffer.Base, 1);
			style.PrepareResources(info.State, resources);
			RectangleF rct = new RectangleF(1, 1, panel.Width - 2, panel.Height - 2);
			style.Draw(canvas, resources, rct, info.State);
			using (Brush br = new SolidBrush(style.TextColour[(int)info.State]))
				e.Graphics.DrawString(Strings.Item("SAW_Settings_StyleSample"), panel.Font, br, rct, GUIUtilities.StringFormatCentreCentre);
		}

		private void EditButton_Click(object sender, EventArgs e)
		{
			Button btn = (Button)sender;
			int index = Array.IndexOf(m_EditButtons, btn);
			Preview info = (Preview) m_Previews[index * 2].Tag;
			frmUserButtonStyle.Display(m_Config, (int) (info.Style + 2));
			m_Previews[index * 2].Invalidate();
			m_Previews[index * 2 + 1].Invalidate();
			WrittenToCurrent();
		}

		private void rdoOn_CheckedChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			m_Config.Write(Config.SAW_EnableItemStyles, rdoOn.Checked);
			WrittenToCurrent();
		}
	}
}
