﻿using System.Windows.Forms;

namespace SAW
{
	public class CombinedKeyEvent
	{
		// See Keys region in frmMain.  Note this is not sent as an event, rather the functions of IKeyControl are called directly
		// not using actual KeyEventArgs because I want to modify the key code.  Constructing new KeyEventArgs doesn't really work
		// as it checks the actual state of the keys on the keyboard rather than using the provided key data
		public char Character = '\0';
		/// <summary>true if this was generated by a call to frmMain.SimulateKey (a few shapes react differently to this) </summary>
		public bool Simulated = false;
		public bool Handled = false;
		/// <summary>If set to true further splash handling will be skipped and the key allowed to drop through to Windows</summary>
		public bool DoNotHandle = false;
		private readonly Keys m_Key;

		public CombinedKeyEvent(KeyEventArgs e)
		{
			m_Key = e.KeyData;
		}

		public CombinedKeyEvent(Keys key)
		{
			m_Key = key;
		}

		public Keys KeyCode => m_Key & ~Keys.Modifiers;
		public Keys KeyData => m_Key;
		public bool Shift => (m_Key & Keys.Shift) > 0;
		public bool Control => (m_Key & Keys.Control) > 0;
		public bool Alt => (m_Key & Keys.Alt) > 0;
		public Keys Modifiers => m_Key & Keys.Modifiers;
	}

	internal interface IKeyControl
	{
		// no implementation of these functions should block - because this may cause Windows to handle the key itself, or let it flow further
		// any implementer must set CurrentFocus on GotFocus/LostFocus (can't use GetFocus as that's Win/API and won't work on a Mac)
		void CombinedKeyDown(CombinedKeyEvent e);
		void CombinedKeyUp(CombinedKeyEvent e);
		// the KeyUp is sent in the same way, for convenience.  At the moment the Character will always be empty
	}


}
