using System.Collections.Generic;
using System;
using System.Drawing;
using System.Windows.Forms;
using SAW;
using Switches.Switching;


namespace Switches
{
	/// <summary>The UI to chooe a switch - both the type key/joystick and the actual switch/key number </summary>
	[System.ComponentModel.DefaultEvent("UserChangedSwitch")]
	public partial class ctrSelectSwitch
	{

		public event EventHandler UserChangedSwitch;

		private readonly List<PhysicalSwitch.Types> m_Types = new List<PhysicalSwitch.Types>(); // as listed in the combo
		private int m_Number; // 0-based; but displayed 1-based
		private readonly List<int> m_DisplayedParams = new List<int>();
		private bool m_Filling;

		public ctrSelectSwitch()
		{
			InitializeComponent();
			if (this.DesignMode)
				return;
			foreach (PhysicalSwitch.Types type in Enum.GetValues(typeof(PhysicalSwitch.Types)))
			{
				if (type != PhysicalSwitch.Types.Null && type != PhysicalSwitch.Types.Pointer)
				{ // at the moment Pointer is for direct clicking; which is selected effectively as 0 switches
					m_Types.Add(type);
					cmbType.Items.Add(Strings.Item("Switch_" + type));
				}
			}
		}

		public int Number
		{
			get // this can be persisted in form designer
			{ return m_Number; }
			set
			{
				m_Number = value;
				lblNumber.Text = "#" + (value + 1);
			}
		}

		[System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
		public PhysicalSwitch.Types Type
		{
			get
			{
				if (cmbType.SelectedIndex < 0)
					return PhysicalSwitch.Types.Null;
				return m_Types[cmbType.SelectedIndex];
			}
			set
			{
				m_Filling = true;
				cmbType.SelectedIndex = m_Types.IndexOf(value);
				m_Filling = false;
			}
		}

		[System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
		public int Parameter
		{
			get
			{
				if (cmbSwitch.SelectedIndex < 0)
					return 0;
				return m_DisplayedParams[cmbSwitch.SelectedIndex];
			}
			set
			{
				m_Filling = true;
				cmbSwitch.SelectedIndex = m_DisplayedParams.IndexOf(value);
				m_Filling = false;
			}
		}

		public void cmbType_SelectedIndexChanged(object sender, EventArgs e)
		{
			// need to fill the switches list
			cmbSwitch.Items.Clear();
			m_DisplayedParams.Clear();
			switch (Type)
			{
				case PhysicalSwitch.Types.Joystick:
					AddParam(0);
					AddParam(1);
					cmbSwitch.Enabled = true;
					break;
				case PhysicalSwitch.Types.Keyboard:
					AddParam((int)Keys.Enter);
					AddParam((int)Keys.Space);
					AddParam((int)Keys.Tab);
					AddParam((int)Keys.Escape);
					cmbSwitch.Enabled = true;
					break;
				default:
					cmbSwitch.Enabled = false; // mainly for when this is first created
					break;
			}
			if (!m_Filling)
			{
				// if user has changed we will need to reassign the param
				Parameter = m_DisplayedParams[0];
				UserChangedSwitch?.Invoke(this, EventArgs.Empty);
			}
		}

		public void cmbSwitch_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (!m_Filling)
				UserChangedSwitch?.Invoke(this, EventArgs.Empty);
		}

		private void AddParam(int param)
		{
			if (Type <= PhysicalSwitch.Types.Null)
				return;
			m_DisplayedParams.Add(param);
			cmbSwitch.Items.Add(PhysicalSwitch.GetParamDescription(Type, param));
		}

		#region Auto detect
		private int m_DetectCycles;
		private PhysicalSwitch.MasterModes m_OldState;
		public void btnSet_Click(object sender, EventArgs e)
		{
			m_DetectCycles = 0;
			lblAutoDetect.Text = Strings.Item("SAW_Settings_PressSwitch") + " 10";
			tmrDetect.Enabled = true;
			m_OldState = PhysicalSwitch.MasterSwitch;
			PhysicalSwitch.MasterSwitch = PhysicalSwitch.MasterModes.Detect;
			lblAutoDetect.Visible = true;
			PhysicalSwitch.SwitchDetected += SwitchDetected;
		}

		public void tmrDetect_Tick(object sender, EventArgs e)
		{
			m_DetectCycles += 1;
			lblAutoDetect.Text = Strings.Item("SAW_Settings_PressSwitch") + " " + (10 - m_DetectCycles);
			if (m_DetectCycles == 10)
			{
				StopDetection();
				return;
			}
			if ((m_DetectCycles % 2) == 1)
				lblAutoDetect.BackColor = Color.FromKnownColor(KnownColor.Highlight);
			else
				lblAutoDetect.BackColor = Color.FromKnownColor(KnownColor.Control);
		}

		private void StopDetection()
		{
			tmrDetect.Enabled = false;
			lblAutoDetect.Visible = false;
			PhysicalSwitch.SwitchDetected -= SwitchDetected;
			PhysicalSwitch.MasterSwitch = m_OldState;
		}

		private void SwitchDetected(PhysicalSwitch.Types type, int param)
		{
			StopDetection();
			Type = type;
			if (m_DisplayedParams.IndexOf(param) < 0)
				AddParam(param);
			Parameter = param;
			UserChangedSwitch?.Invoke(this, EventArgs.Empty);
			lblAutoDetect.Visible = false;
		}

		#endregion
	}

}
