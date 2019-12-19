using System;
using System.Drawing;
using System.Windows.Forms;

namespace SAW
{
	public partial class cfgMouseOutput : ConfigPage
	{
		public cfgMouseOutput()
		{
			InitializeComponent();
			Strings.Translate(this);
		}

		public override void OnDisplay()
		{
			base.OnDisplay();
			m_Filling = true;
			if (m_Applied.ReadBoolean(Config.Mouse_Continuous))
				rdoContinuous.Checked = true;
			else
				rdoStep.Checked = true;
			nudWaitMultiplier.SafeSetValue(m_Applied.ReadInteger(Config.Wait_Multipler, Config.Wait_Multipler_Default));
			nudSmall.SafeSetValue(m_Applied.ReadInteger(Config.MouseStep_Small, (int) Config.MouseStepDefaults.Small));
			nudMedium.SafeSetValue(m_Applied.ReadInteger(Config.MouseStep_Medium, (int)Config.MouseStepDefaults.Medium));
			nudLarge.SafeSetValue(m_Applied.ReadInteger(Config.MouseStep_Large, (int)Config.MouseStepDefaults.Large));

			m_Filling = false;
		}


		#region Control events

		private void rdoStep_CheckedChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			if (rdoStep.Checked)
				m_Config.Write(Config.Mouse_Continuous, false);
			WrittenToCurrent();
		}

		private void rdoContinuous_CheckedChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			if (rdoContinuous.Checked)
				m_Config.Write(Config.Mouse_Continuous, true);
			WrittenToCurrent();
		}

		private void nudWaitMultiplier_ValueChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			m_Config.Write(Config.Wait_Multipler, (int)nudWaitMultiplier.Value);
			WrittenToCurrent();
		}

		private void nudSmall_ValueChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			m_Config.Write(Config.MouseStep_Small, (int)nudSmall.Value);
			WrittenToCurrent();
		}

		private void nudMedium_ValueChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			m_Config.Write(Config.MouseStep_Medium, (int)nudMedium.Value);
			WrittenToCurrent();
		}

		private void nudLarge_ValueChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			m_Config.Write(Config.MouseStep_Large, (int)nudLarge.Value);
			WrittenToCurrent();
		}

		private void btnTestSmall_Click(object sender, EventArgs e)
		{
			Cursor.Position = new Point(Cursor.Position.X + (int)nudSmall.Value, Cursor.Position.Y);
		}

		private void btnTestMedium_Click(object sender, EventArgs e)
		{
			Cursor.Position = new Point(Cursor.Position.X + (int)nudMedium.Value, Cursor.Position.Y);
		}

		private void btnTestLarge_Click(object sender, EventArgs e)
		{
			Cursor.Position = new Point(Cursor.Position.X + (int)nudLarge.Value, Cursor.Position.Y);
		}

		#endregion

	}
}
