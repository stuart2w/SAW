using System;
using System.Windows.Forms;
using SAW;
using Switches.Switching;


namespace Switches
{
	/// <summary>Not actually used in SAW.  Used as a test form during development only (modify app to open one of these instead of SAW)</summary>
	public partial class DevelopTest
	{
		public DevelopTest()
		{
			InitializeComponent();
		}

		private Engine m_Engine;
		private bool m_Loaded;

		public void Form1_Load(object sender, EventArgs e)
		{
			PhysicalSwitch.Initialise();
			FillEngines();
			m_Loaded = true;
			ctrSwitch0.Type = PhysicalSwitch.Types.Keyboard;
			ctrSwitch0.Parameter = (int)Keys.Space;
			ctrSwitch1.Type = PhysicalSwitch.Types.Keyboard;
			ctrSwitch1.Parameter = (int)Keys.Enter;
			cmbMethods.SelectedIndex = 1;
		}

		private void FillEngines()
		{
			cmbMethods.DataSource = Engine.PossibleEngines(rdoTwo.Checked ? 2 : 1);
		}

		public void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			PhysicalSwitch.ClearSwitches();
			m_Engine.Dispose();
		}

		private void KillScan()
		{
			// kills the engine.  Leaves the switches
			PhysicalSwitch.MasterSwitch = PhysicalSwitch.MasterModes.Off;
			ctrDiagram.Clear();
			if (m_Engine != null)
			{
				pnlTest.Engine = null;
				m_Engine.Dispose();
				m_Engine = null;
			}
		}

		private void StartScan()
		{
			KillScan();
			PhysicalSwitch.AssignSwitch(0, ctrSwitch0.Type, ctrSwitch0.Parameter);
			if (rdoTwo.Checked)
				PhysicalSwitch.AssignSwitch(1, ctrSwitch1.Type, ctrSwitch1.Parameter);
			else
				PhysicalSwitch.AssignSwitch(1, PhysicalSwitch.Types.Null, 0);
			m_Engine = Engine.Create((Engine.Methods)cmbMethods.SelectedValue);
			// note must be created before Logical bit to get switch count, but Initialised after the switches are available
			pnlTest.Engine = m_Engine;
			if (rdoTwo.Checked)
				Logical.CreateSwitches(Logical.Number.Two, m_Engine);
			else if (m_Engine.NumberSwitchInputs == 2)
				Logical.CreateSwitches(Logical.Number.ShortLong, m_Engine);
			else
				Logical.CreateSwitches(Logical.Number.One, m_Engine);
			m_Engine.Initialise();
			ctrDiagram.Construct(m_Engine);
			PhysicalSwitch.MasterSwitch = PhysicalSwitch.MasterModes.On;
			m_Engine.StartScanning();
		}

		public void cmbMethods_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (cmbMethods.SelectedIndex < 0 || !m_Loaded)
				return;
			Engine.Methods eMethod = (Engine.Methods)cmbMethods.SelectedValue;
			if (m_Engine != null && m_Engine.Method == eMethod)
				return;
			StartScan();
			FillTimings();
		}

		private void FillTimings()
		{
			tblTimings.SuspendLayout();
			while (tblTimings.Controls.Count > 0)
			{
				tblTimings.Controls[0].Dispose();
			}
			if (m_Engine == null || !m_Loaded)
				return;
			Engine.Timings needed = m_Engine.RelevantTimings;
			for (int index = 0; index <= Logical.Count - 1; index++)
			{
				Logical logical = Logical.Switch(index);
				if (logical != null)
					needed = needed | logical.RequiredTimings;
			}
			int row = 0;
			foreach (Engine.Timings timing in Enum.GetValues(typeof(Engine.Timings)))
			{
				if ((needed & timing) > 0)
				{
					Label label = new Label { Text = Strings.Item("Switch_Timing_" + timing), TextAlign = System.Drawing.ContentAlignment.MiddleRight, Dock = DockStyle.Fill };
					tblTimings.Controls.Add(label);
					tblTimings.SetRow(label, row);
					ctrEditTiming ctr = new ctrEditTiming { Meaning = timing, Value = m_Engine.ConfiguredTiming(timing), Dock = DockStyle.Fill };
					ctr.UserChangedValue += TimingChanged;
					tblTimings.Controls.Add(ctr);
					tblTimings.SetRow(ctr, row);
					tblTimings.SetColumn(ctr, 1);
					row++;
				}
			}
			tblTimings.ResumeLayout();
		}

		private void TimingChanged(object sender, EventArgs e)
		{
			if (m_Engine == null)
				return;
			ctrEditTiming ctr = (ctrEditTiming)sender;
			Engine.Timings eTiming = ctr.Meaning;
			Config.UserUser.Write(Engine.TimingConfigField(eTiming), ctr.Value);
		}

		public void ctrSwitch0_UserChangedSwitch(object sender, EventArgs e)
		{
			KillScan();
			StartScan();
		}

		public void ctrSwitch1_UserChangedSwitch(object sender, EventArgs e)
		{
			KillScan();
			StartScan();
		}

		public void rdoTwo_CheckedChanged(object sender, EventArgs e)
		{
			ctrSwitch1.Enabled = rdoTwo.Checked;
			FillEngines();
		}

		public void rdoOne_CheckedChanged(object sender, EventArgs e)
		{
			FillEngines();
		}

		#region Test panel
		// always displays 10 boxes

		#endregion
	}

}
