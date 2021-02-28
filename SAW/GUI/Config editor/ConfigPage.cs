using System.Diagnostics;
using System.Windows.Forms;

namespace SAW
{
	[System.ComponentModel.ToolboxItem(false)]
	public class ConfigPage : UserControl
	{

		internal frmEditConfig Form;

		protected void WrittenToCurrent()
		{
			Form.WrittenToCurrent();
		}

		/// <summary>Called when the containing form is about to display this panel.  Should fill the UI</summary>
		public virtual void OnDisplay()
		{ }

		/// <summary>Called by the form when the user is leaving this page.</summary>
		public virtual void OnHide()
		{ }

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			Form = null;
		}

		#region Replication of fields on form

		protected Config.Levels Level => Form.m_Level;

		protected Config m_Config
		{[DebuggerStepThrough] get { return Form.m_Config; } }

		protected bool m_Filling
		{
			get
			{
				if (Form == null)
					return true; // this is the case during construction of the control
				return Form.m_Filling;
			}
			set
			{
				if (Form != null)
					Form.m_Filling = value;
			}
		}

		protected Document m_Document => Form.m_Document;

		protected Transaction m_Transaction
		{ [DebuggerStepThrough]  get { return Form.m_Transaction; } }

		protected AppliedConfig m_Applied
		{ [DebuggerStepThrough]  get { return Form.m_Applied; } }

		/// <summary>Conceptually ignoring any higher priority levels.  Current just same as m_Applied on the assumption only user settings are edited</summary>
		protected AppliedConfig m_SubsequentApplied
		{ [DebuggerStepThrough] get { return Form.m_Applied; } }

		#endregion


	}

}
