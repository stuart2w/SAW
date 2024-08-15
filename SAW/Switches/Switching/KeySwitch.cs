using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using System.Linq;
using SAW;

namespace Switches.Switching
{
	internal class KeySwitch : PhysicalSwitch
	{

		#region DLL imports
		[System.Runtime.InteropServices.DllImport("SwitchCpp.dll", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
		private static extern bool MonitorKeys(IntPtr hWnd);

		[System.Runtime.InteropServices.DllImport("SwitchCpp.dll", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
		private static extern bool UnmonitorKeys();

		[System.Runtime.InteropServices.DllImport("SwitchCpp.dll", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
		private static extern bool SendAll(bool send);

		[System.Runtime.InteropServices.DllImport("SwitchCpp.dll", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
		private static extern bool ResetKeys();
		[System.Runtime.InteropServices.DllImport("SwitchCpp.dll", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
		private static extern bool AddKey(int key);

		[System.Runtime.InteropServices.DllImport("SwitchCpp.dll", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
		private static extern bool SendCharacters(bool bolSend);

		#endregion

		#region Shared list of switches
		private static Dictionary<Keys, KeySwitch> g_Switches;
		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		internal static new void Initialise()
		{
			// faster(?) than using Shared Sub
			// doesn't actually enable the key hook yet
			MasterSwitchChanged += PhysicalMasterSwitchChanged;
			g_Switches = new Dictionary<Keys, KeySwitch>();
			m_EventForm.KeyDown += KeyDown_Event;
			m_EventForm.KeyUp += KeyUp_Event;
			m_EventForm.CharacterCaptured += CharacterCaptured_Event;
			// this should be the first attempt to access the DLL - just a dummy call to test
			try
			{
				SendAll(false);
			}
			catch (Exception ex)
			{
				OnError("Keyboard switching unavailable: did not connect to SwitchCpp.dll");
			}
		}

		private static void PhysicalMasterSwitchChanged()
		{
			// global on/off has changed
			SendAll(MasterSwitch == MasterModes.Detect);
			switch (MasterSwitch)
			{
				case MasterModes.Off:
					UnmonitorKeys();
					break;
				case MasterModes.On: // set also PauseToAllowApplicationToSend set
					if (g_Switches.Count > 0 || _CharacterTyped != null) // monitoring is switched on if we are tracking any characters
					{
						MonitorKeys(m_EventForm.Handle);
						SendCharacters(_CharacterTyped != null);
						Debug.WriteLine("SendChars: " + (_CharacterTyped != null));
					}
					else
						UnmonitorKeys();
					break;
				case MasterModes.Detect:
					MonitorKeys(m_EventForm.Handle);
					SendCharacters(false); // not while doing switch detection
					break;
				default:
					OnError("Unexpected MasterMode: " + MasterSwitch);
					break;
			}
		}

		private static void UpdateKeyList()
		{
			// sends list of keys to DLL
			try
			{
				ResetKeys();
				foreach (Keys keys in g_Switches.Keys) // the order doesn't matter - it just needs a list of what to send
				{
					AddKey((int)keys);
				}
			}
			catch (Exception)
			{
				OnError("Failed to update key list");
			}
		}

		/// <summary>call PhysicalSwitch.AssignSwitch, NOT this</summary>
		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		internal static PhysicalSwitch CreateSwitch(Keys key)
		{
			// 
			if (g_Switches.ContainsKey(key))
				OnError("Creating duplicate switch");
			else
				g_Switches.Add(key, new KeySwitch(key));
			UpdateKeyList();
			return g_Switches[key];
		}

		public override void Destroy()
		{
			if (g_Switches.ContainsKey(m_Key))
			{
				g_Switches.Remove(m_Key);
				UpdateKeyList();
			}
			else
			{
				OnError("Destroying non-existent key switch: " + m_Key);
			}
		}

		#endregion

		#region Key handling, including events from form

		private static void KeyDown_Event(object sender, KeyEventArgs e)
		{
			Keys key = e.KeyCode & ~Keys.Modifiers;
			switch (MasterSwitch)
			{
				case MasterModes.Off:
					break;
				case MasterModes.On:
					if (g_Switches.ContainsKey(key))
						g_Switches[key].OnStateChange(true);
					break;
				case MasterModes.Detect:
					OnSwitchDetected(Types.Keyboard, (int)key);
					break;
				default:
					Debug.Fail("Unexpected MasterSwitch: " + MasterSwitch);
					break;
			}
		}

		/// <summary>Returns true if the given key is currently being intercepted for switching.  Ie only if switching is actually active.
		/// Does not account for Detect mode (all keys are intercepted then, so calling this is pointless)</summary>
		public static bool KeyCurrentlyUsedForSwitching(Keys key)
		{
			if (MasterSwitch == MasterModes.Off)
				return false;
			return g_Switches.Values.Any(s => s.m_Key == key);
		}

		private static void KeyUp_Event(object sender, KeyEventArgs e)
		{
			Keys key = e.KeyCode & ~Keys.Modifiers;
			switch (MasterSwitch)
			{
				case MasterModes.Off:
					break;
				case MasterModes.On:
					if (g_Switches.ContainsKey(key))
						g_Switches[key].OnStateChange(false);
					break;
				case MasterModes.Detect: // only falling is sent
					break;
				default:
					Debug.Fail("Unexpected MasterSwitch: " + MasterSwitch);
					break;
			}
		}

		private static void CharacterCaptured_Event(object sender, SingleFieldEventClass<char> e)
		{ // triggered by event form for the WM_User+104 message notifying characters for word prediction
			_CharacterTyped?.Invoke(null, e);
		}

		#endregion

		#region Member stuff

		private readonly Keys m_Key;

		private KeySwitch(Keys key)
		{
			if (key == Keys.None)
				throw new ArgumentException();
			key = key & ~Keys.Modifiers; // removes any shift or similar
			m_Key = key;
		}

		public override Types Type => Types.Keyboard;
		public override int Param => (int)m_Key;
		public override string ParamDescription => m_Key.ToString();
		public static string GetParamDescription(int param) => GUIUtilities.KeyDescription((Keys) param);

		#endregion

		#region Character tracking for predictions

		private static EventHandler<SingleFieldEventClass<char>> _CharacterTyped;

		/// <summary>Raised from the keyhook when a character is typed (excludes certain special ones that are untrackable).
		/// Only functions while scan master switch is on; but does not require any active key switches (this will enable key monitoring if needed) </summary>
		public static event EventHandler<SingleFieldEventClass<char>> CharacterTyped
		{ // custom event logic will enable key monitoring if needed
			add
			{
				_CharacterTyped += value;
				PhysicalMasterSwitchChanged();
			}
			remove
			{
				// ReSharper disable once DelegateSubtraction
				if (_CharacterTyped != null) _CharacterTyped -= value;
				Debug.WriteLine("Character typed delete.  Null? = " + (_CharacterTyped == null));
				PhysicalMasterSwitchChanged();
			}
		}

		#endregion

	}
}