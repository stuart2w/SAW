using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RedRat.AvDeviceDb;
using SAW.Commands;

namespace SAW.CommandEditors
{
	public partial class RedRatSendEditor : UserControl, ICommandEditor
	{

		private bool m_Filling;

		public RedRatSendEditor()
		{
			m_Filling = true;
			InitializeComponent();
			Strings.Translate(this);
			RedRatSupport.Prepare();
			foreach (AVDevice device in RedRatSupport.SignalDatabase.AVDevices)
			{
				cmbDevice.Items.Add(device.Name);
			}
			m_Filling = false;
		}

		private void FillUI()
		{
			m_Filling = true;
			try
			{
				cmbDevice.Text = m_Command.Device;
				cmbSignal.Text = m_Command.Signal;
			}
			finally { m_Filling = false; }
		}

		#region ICommandEditor

		private CmdRedRatSend m_Command;
		public event EventHandler UserChanged;

		public Command EditCommand
		{
			get { return m_Command; }
			set
			{
				m_Command = (CmdRedRatSend)value;
				FillUI();
			}
		}

		public string GetValidationError()
		{
			if (string.IsNullOrEmpty(m_Command.Device) || string.IsNullOrEmpty(m_Command.Signal))
				return Strings.Item("[Script_Error_RedRatNotDefined]");
			return null;
		}

		#endregion

		private void cmbDevice_SelectedIndexChanged(object sender, EventArgs e)
		{
			bool old = m_Filling;
			m_Filling = true;
			cmbSignal.Items.Clear();
			try
			{
				if (cmbDevice.SelectedIndex < 0)
					return;
				AVDevice device = RedRatSupport.SignalDatabase.GetAVDevice(cmbDevice.Text);
				foreach (var signal in device.Signals)
				{
					cmbSignal.Items.Add(signal.Name);
				}
			}
			finally
			{
				m_Filling = old;
			}
			if (m_Command != null)
				m_Command.Device = cmbDevice.Text;
			if (!m_Filling)
				UserChanged?.Invoke(this, EventArgs.Empty);
		}

		private void cmbSignal_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			m_Command.Signal = cmbSignal.Text;
			UserChanged?.Invoke(this, EventArgs.Empty);
		}

		private void btnTest_Click(object sender, EventArgs e)
		{
			// none of the Redrat commands actually use the context, so it's OK to create a dummy one
			m_Command.Execute(new Command.ExecutionContext(null, null, null, null, null, Shapes.Scriptable.ScriptTypes.Visit));
		}
	}
}
