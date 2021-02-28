using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Switches;
using Switches.Switching;

namespace SAW
{
	public partial class cfgSwitching : ConfigPage
	{
		private readonly Label[] TimingLabels;
		private readonly ctrEditTiming[] TimingControls;

		public cfgSwitching()
		{
			PhysicalSwitch.Initialise(); // will be ignored if already done.  Required for detect mechanism to work

			InitializeComponent();
			Strings.Translate(this);
			TimingLabels = new[] { lblAcceptance, lblPostActivation, lblLongPress, lblScanTime, lblFirstRepeat, lblSubsequentRepeat, lblDwellSelect, lblCriticalReverse };
			TimingControls = new[] { ctrTimingAcceptance, ctrTimingPostActivation, ctrTimingLongPress, ctrTimingScanTime, ctrTimingFirstRepeat, ctrTimingSubsequentRepeat, ctrTimingDwellTime, ctrTimingCriticalReverse };
			foreach (Label lbl in TimingLabels)
			{
				Engine.Timings timing = (Engine.Timings)Convert.ToInt32(lbl.Tag);
				lbl.Text = Strings.Item("Switch_Timing_" + timing);
			}
			foreach(ctrEditTiming ctr in TimingControls)
			{
				ctr.UserChangedValue += ctrTiming_UserChangedValue;
			}
		}

		public override void OnDisplay()
		{
			base.OnDisplay();
			m_Filling = true;
			switch (m_Applied.ReadInteger(Config.Number_Switches, 1))
			{
				case 0: rdoMouse.Checked = true; break;
				case 1: rdoOne.Checked = true; break;
				case 2: rdoTwo.Checked = true; break;
			}
			ReflectSwitches();
			ctrSwitch1.Type = (PhysicalSwitch.Types) m_Applied.ReadInteger(Config.Switch_Type_ + "0", (int) PhysicalSwitch.Types.Keyboard);
			ctrSwitch2.Type = (PhysicalSwitch.Types)m_Applied.ReadInteger(Config.Switch_Type_ + "1", (int)PhysicalSwitch.Types.Keyboard);
			ctrSwitch1.Parameter = m_Applied.ReadInteger(Config.Switch_Param_ + "0", 32);
			ctrSwitch2.Parameter = m_Applied.ReadInteger(Config.Switch_Param_ + "1", 13);
			m_Filling = false;
		}

		/// <summary>The list of engines currently displayed in the drop-down; depends on the number of switches selected</summary>
		private List<Engine> m_PossibleEngines;
		private void ReflectSwitches()
		{
			bool oldFilling = m_Filling;
			SuspendLayout();
			int switches = m_Applied.ReadInteger(Config.Number_Switches, 1);
			lblSwitch1.Visible = switches >= 1;
			ctrSwitch1.Visible = switches >= 1;
			ctrSwitch2.Visible = switches >= 2;

			cmbScanType.Items.Clear();
			m_PossibleEngines = Engine.PossibleEngines(switches).ToList();
			foreach (Engine possible in m_PossibleEngines)
			{
				cmbScanType.Items.Add(possible.DisplayName);
			}
			ReflectEngine();
			ResumeLayout();
			m_Filling = oldFilling;
			Engine.Methods configured = (Engine.Methods)m_Applied.ReadInteger(Config.Switch_Engine, -1);
			if (configured < 0 || !m_PossibleEngines.Any(e => e.Method == configured))
			{
				configured = Engine.DefaultMethodForSwitches(switches);
			}
			cmbScanType.SelectedIndex = m_PossibleEngines.IndexOf(m_PossibleEngines.FirstOrDefault(e => e.Method == configured));
			// will fire the event to save the value, which is important if it was change, and just has no effect otherwise
		}

		/// <summary>Updates timing GUI based on the selected engine</summary>
		private void ReflectEngine()
		{
			SuspendLayout();
			int switches = m_Applied.ReadInteger(Config.Number_Switches, 1);
			Engine engine = Engine.Create((Engine.Methods)m_Applied.ReadInteger(Config.Switch_Engine, (int)Engine.DefaultMethodForSwitches(switches)));
			var timings = engine.RelevantTimings; // bit field of ones which apply
			foreach (Label ctr in TimingLabels)
			{
				int time = Convert.ToInt32(ctr.Tag);
				ctr.Visible = (timings & (Engine.Timings)time) > 0;
			}
			foreach (ctrEditTiming ctr in TimingControls)
			{
				Engine.Timings time = (Engine.Timings)Convert.ToInt32(ctr.Tag);
				ctr.Visible = (timings & time) > 0;
				ctr.Value = engine.ConfiguredTiming(time);
			}
			ResumeLayout();
		}

		#region Control events
		private void rdoSwitchCount_CheckedChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			RadioButton rdo = (RadioButton)sender;
			if (rdo.Checked)
			{
				int switches = Convert.ToInt32(rdo.Tag);
				m_Config.Write(Config.Number_Switches, switches);
				//Switches.Engine engine = Switches.Engine.Create((Switches.Engine.Methods)m_Applied.ReadInteger(Config.Switch_Engine, (int)Switches.Engine.DefaultMethodForSwitches(switches)));
				//if (engine.AcceptShortLongSwitches
				ReflectSwitches();
				WrittenToCurrent();
			}
		}

		private void cmbScanType_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			if (cmbScanType.SelectedIndex >= 0)
			{
				m_Config.Write(Config.Switch_Engine, (int)m_PossibleEngines[cmbScanType.SelectedIndex].Method);
				WrittenToCurrent();
			}
			ReflectEngine();
		}

		private void ctrTiming_UserChangedValue(object sender, EventArgs e)
		{ // used for all of them
			if (m_Filling)
				return;
			ctrEditTiming ctr = (ctrEditTiming)sender;
			Engine.Timings time = (Engine.Timings)Convert.ToInt32(ctr.Tag);
			m_Config.Write(Engine.TimingConfigField(time), ctr.Value);
			WrittenToCurrent();
		}

		private void ctrSwitch1_UserChangedSwitch(object sender, EventArgs e)
		{
			m_Config.Write(Config.Switch_Type_ + "0", (int)ctrSwitch1.Type);
			m_Config.Write(Config.Switch_Param_ + "0", ctrSwitch1.Parameter);
			WrittenToCurrent();
		}

		private void ctrSwitch2_UserChangedSwitch(object sender, EventArgs e)
		{
			m_Config.Write(Config.Switch_Type_ + "1", (int)ctrSwitch2.Type);
			m_Config.Write(Config.Switch_Param_ + "1", ctrSwitch2.Parameter);
			WrittenToCurrent();
		}

		#endregion

	}
}
