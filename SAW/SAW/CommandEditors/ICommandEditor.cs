using System;

namespace SAW.CommandEditors
{
	/// <summary>Implemented by the custom editors for each type of command.  These are always contained in CommandEditor and external code is unlikely to need to interact with this</summary>
	public interface ICommandEditor
	{
		Command EditCommand { get; set; }
		string GetValidationError();

		/// <summary>Fired when the user makes a (valid?) change to the data (ideally only fired on acceptable changes, but might not be always enforced)</summary>
		event EventHandler UserChanged;

	}
	// default implementation which can be pasted into controls:

	//private bool m_Filling;

	//public CLASSNAME()
	//{
	//	m_Filling = true;
	//	InitializeComponent();
	//	Strings.Translate(this);
	//	m_Filling = false;
	//}

	//private void FillUI()
	//{
	//	m_Filling = true;
	//	try
	//	{
	//	}
	//	finally{m_Filling = false;}
	//}

	//#region ICommandEditor
	//private Command m_Command;
	//public event EventHandler UserChanged;

	//public Command EditCommand
	//{
	//	get { return m_Command; }
	//	set
	//	{
	//		m_Command = value;
	//		FillUI();
	//	}
	//}

	//public string GetValidationError()
	//{
	//	return null;
	//}
	//#endregion

	//#region Events

	//#endregion


}
