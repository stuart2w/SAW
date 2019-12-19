using System;
using System.Windows.Forms;

namespace SAW.CommandEditors
{
	/// <summary>Generic control which edits the details of a command - internally constructs specific controls depending on the command being edited</summary>
	public partial class CommandEditor : UserControl
	{
		private Command m_Command;
		private ICommandEditor m_Control;

		/// <summary>Fired when the user has made a valid(?) change to the data</summary>
		public event EventHandler UserChanged;

		public CommandEditor()
		{
			InitializeComponent();
		}

		/// <summary>The actual item that the script is in, if known.  MAY BE NULL</summary>
		public Scriptable ContainerScriptable { get; private set; }

		/// <summary>Start editing the given command.  The scriptable can be given for context, or can be null (it is never essential)</summary>
		public void EditCommand(Command value, Scriptable scriptable)
		{
			if (value == m_Command) // no nothing if the value isn't actually changed
				return;
			m_Command = value;
			ContainerScriptable = scriptable;
			if (m_Control != null)
			{
				m_Control.UserChanged -= Control_UserChanged;
				(m_Control as Control).Dispose();
				Controls.Remove(m_Control as Control);
			}
			m_Control = m_Command?.GetEditor();
			if (m_Control != null)
			{
				Control control = m_Control as Control;
				control.Dock = DockStyle.Fill;
				Controls.Add(control);
				m_Control.EditCommand = m_Command;
				m_Control.UserChanged += Control_UserChanged;
			}
		}

		/// <summary>Returns error message, or null if none</summary>
		public string GetValidationError()
		{
			return m_Control?.GetValidationError();
		}

		private void Control_UserChanged(object sender, EventArgs e)
		{
			UserChanged?.Invoke(this, e);
		}

	}
}
