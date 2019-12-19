using System.Diagnostics;
using System.Windows.Forms;

namespace SAW
{
	public class KeyForm : Form // can't be made abstract as that messes up the designer for derived forms
	{
		// base class for any form which wants to host controls which implement IKeyHandler

		private KeyEventArgs m_PendingDown; // the data from a KeyDown event which has not yet been sent onwards
		private KeyEventArgs m_LastDown;

		protected KeyForm()
		{
			this.KeyDown += KeyForm_KeyDown;
			this.KeyUp += KeyForm_KeyUp;
			this.KeyPress += KeyForm_KeyPress;
		}

		internal void KeyForm_KeyDown(object sender, KeyEventArgs e)
		{
			e.Handled = true;
			// KeyCode is the pure key. KeyData includes modifiers
			m_LastDown = e;
			m_PendingDown = e;
			// Mac conversion of Key done by constructing New CombinedKeyEvent from the event
			if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Right || e.KeyCode == Keys.Up || e.KeyCode == Keys.Down || e.KeyCode == Keys.Home || e.KeyCode == Keys.End || e.KeyCode == Keys.PageDown || e.KeyCode == Keys.PageUp || e.KeyCode == Keys.Insert || e.KeyCode >= Keys.F1 && e.KeyCode <= Keys.F24 || e.KeyCode == Keys.Delete || e.KeyCode == Keys.Pause || e.KeyCode == Keys.Apps) //, Keys.ShiftKey, Keys.ControlKey, Keys.Menu
			{
				// none of these generate KeyPress
				CombinedKeyEvent objEvent = new CombinedKeyEvent(e); // Character defaults to Null
				SendCombinedKeyEvent(objEvent, true);
				// We do need some of these keys to be returned to Windows with e.Handle = false (e.g. Alt-F4)
				e.Handled = objEvent.Handled;
			}
			else
			{
				if (e.Alt ^ e.Control || e.Alt && e.Control && e.Shift) // Xor because ctrl+Alt is AltGr which is used for key combos; so this should be treated as a character key not a combo
				{
					// but... now allowing all 3 keys to go here.  I don't think any character uses shift with ctrl+Alt
					CombinedKeyEvent objCustom = new CombinedKeyEvent(e);
					SendCombinedKeyEvent(objCustom, true);
					// some control combinations seem to send press events, and not others
					if (objCustom.Handled && e.Alt)
					{
						e.SuppressKeyPress = true; // must stop menu activating with Alt+key combinations which did something
					}
				}
			}
		}
		//functions are all Friend so popup forms can send here if they receive the keys instead
		internal void KeyForm_KeyPress(object sender, KeyPressEventArgs e)
		{
			e.Handled = true;
			if (m_PendingDown == null)
			{
				Debug.Assert(m_LastDown != null && m_LastDown.Control, "frmMain_KeyPress without KeyDown");
				// control combos all act on the KeyDown - some (rather unpredictably) do send the Press
			}
			else
			{
				CombinedKeyEvent eCombined = new CombinedKeyEvent(m_PendingDown);
				eCombined.Character = e.KeyChar;
				SendCombinedKeyEvent(eCombined, true);
				e.Handled = eCombined.Handled;
			}
		}

		protected virtual void SendCombinedKeyEvent(CombinedKeyEvent e, bool down)
		{
			// note e may not be the event sent by Windows; so changing Handled or SuppressKeyPress won't always do much
			// IF DOWN KEYS AREN'T ARRIVING FOR SPECIAL KEYS (cursors, etc) - check that controls all implement IsInputKey for relevant keys
			Control current = GUIUtilities.GetFocusControl();
			if (current is TextBoxAllKeys)
			{
				e.Handled = false;
				return;
			}
			m_PendingDown = null;
#if DEBUG
			if (down)
			{
				//Debug.WriteLine(IIf(e.Alt, "Alt-", "") + IIf(e.Shift, "Sh-", "") + IIf(e.Control, "Ctrl-", "") + e.KeyCode.ToString + "=" + CInt(e.KeyCode).ToString + " with " + IIf(CurrentFocus Is Nothing, " no focus", "focus in "+curr))
				//If Not CurrentFocus Is Nothing Then Debug.WriteLine("CurrentFocus = " + CurrentFocus.GetType.Name)
				//If Not GetFocusControlFromWindows() Is Nothing Then Debug.WriteLine("CurrentFocusFromWindows = " + GetFocusControlFromWindows.GetType.Name)
			}
#endif
			while (current != null && e.Handled == false)
			{
				if (current is IKeyControl control)
				{
					if (down)
						control.CombinedKeyDown(e);
					else
						control.CombinedKeyUp(e);
				}
				else if (current is Button button)
				{
					if (down && e.KeyCode == Keys.Space)
					{
						button.PerformClick();
						e.Handled = true;
					}
				}
				if (current.Parent == null && current is Form)
					// .Parent doesn't follow form ownership - doing this ensures that we end up back here, which is essential for misc control keys to work
					current = ((Form)current).Owner;
				else
					current = current.Parent;
			}
		}

		internal void KeyForm_KeyUp(object sender, KeyEventArgs e)
		{
			e.Handled = true;
			SendCombinedKeyEvent(new CombinedKeyEvent(e), false);
			// note this should not clear m_PendingDown because up and down events can intertwine for different keys erratically
		}

		protected override bool ProcessTabKey(bool forward)
		{
			return false; // says that we have processed the key.  We will do any normal moving between controls type navigation
						  //    Return MyBase.ProcessTabKey(forward)
		}

		protected override bool IsInputKey(Keys keyData)
		{
			return true;
		}

	}

}
