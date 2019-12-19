using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Windows.Forms;
using SAW;

namespace Switches.Switching
{
	/// <summary>Represents a single one of any of the physical switches (joystick or keyboard)</summary>
	public abstract class PhysicalSwitch : ISwitch
	{

		public enum Types
		{
			Null = 0,
			Keyboard = 1,
			Joystick = 2,
			/// <summary>Using pointer to directly click in selection set, NOT intercepting mouse buttons generally.  This is a ZERO switch setup</summary>
			Pointer = 16
			// if adding update AssignSwitch, GetParamDescription, Initialise
		}

		#region Master state, detection
		public enum MasterModes
		{
			Off = 0,
			On = 1,
			Detect = 2 // used to detect any switch pressed - for setup
		}
		private static MasterModes m_MasterSwitch = MasterModes.Off; // turns all switch input on or off

		public delegate void MasterSwitchChangedEventHandler();
		static public event MasterSwitchChangedEventHandler MasterSwitchChanged;

		public delegate void SwitchDetectedEventHandler(Types type, int param); // raised when MasterMode = Detect, for any FALLING edge
		static public event SwitchDetectedEventHandler SwitchDetected;

		/// <summary>A master control which enables/disables the physical switches, without destroying them all.  Also supports a detect mode where all switch
		/// activations are reported even if not currently configured for use</summary>
		public static MasterModes MasterSwitch
		{
			get { return m_MasterSwitch; }
			set
			{
				if (m_MasterSwitch == value)
					return;
				m_MasterSwitch = value;
				MasterSwitchChanged?.Invoke();
			}
		}

		protected static void OnSwitchDetected(Types type, int param)
		{
			SwitchDetected?.Invoke(type, param);
		}
		#endregion

		#region Event form (ands hot keys)
		protected static EventForm m_EventForm; // can be used by switches where an HWND is needed (or keyboard switch just posts KeyDown / KeyUp here)

		protected class EventForm : Form
		{
			/// <summary>Raised for WM_USER+104 messages, sent by the keyhook to report characters typed</summary>
			public event EventHandler<SingleFieldEventClass<char>> CharacterCaptured;

			public EventForm()
			{
				Left = -20000;
				this.KeyPreview = true;
				this.FormBorderStyle = FormBorderStyle.None;
				this.StartPosition = FormStartPosition.Manual;
				this.ShowInTaskbar = false;
			}

			protected override bool IsInputKey(Keys keyData)
			{
				return true;
			}

			public const int WM_USER = 0x400;
			private const int WM_HOTKEY = 0x0312;
			protected override void WndProc(ref Message m)
			{
				base.WndProc(ref m);
				if (m.Msg == WM_USER + 104)
				{ // character typed message
					char ch = (char)m.LParam;
					CharacterCaptured?.Invoke(this, ch);
				}
				if (m.Msg == WM_HOTKEY)
				{
					// get the keys - not needed here
					//Keys key = (Keys)(((int)m.LParam >> 16) & 0xFFFF);
					//ModifierKeys modifier = (ModifierKeys)((int)m.LParam & 0xFFFF);
					// ID gives us the callback
					int index = HotKeyIDs.IndexOf((int) m.WParam);
					if (index >= 0)
						HotKeysCallbacks[index].Invoke();
				}
			}

			private List<Action> HotKeysCallbacks=new List<Action>();
			private List<Keys> HotKeyKeys= new List<Keys>();
			private List<int> HotKeyIDs= new List<int>();
			private int NextID;

			public void AddHotKey(Keys key, Action callback)
			{
				HotModifierKeys modifiers = 0;
				Keys windowKey = key;
				if ((windowKey & Keys.Shift) > 0)
				{ windowKey -= Keys.Shift; modifiers |= HotModifierKeys.Shift; }
				if ((windowKey & Keys.Control) > 0)
				{ windowKey -= Keys.Control; modifiers |= HotModifierKeys.Control; }
				if ((windowKey & Keys.Alt) > 0)
				{ windowKey -= Keys.Alt; modifiers |= HotModifierKeys.Alt; }
				if (Windows.RegisterHotKey(Handle, NextID, (uint) modifiers, (uint) windowKey))
				{
					HotKeyKeys.Add(key);
					HotKeysCallbacks.Add(callback);
					HotKeyIDs.Add(NextID++);
				}
			}

			/// <summary>Removes the key.  If second param is true there is no error logged if the hot key didn't exist</summary>
			public void RemoveHotKey(Keys key, bool noWarnIfUnknown= false)
			{
				int index = HotKeyKeys.IndexOf(key);
				if (index >= 0)
				{
					Windows.UnregisterHotKey(Handle, HotKeyIDs[index]);
					HotKeyIDs.RemoveAt(index);
					HotKeyKeys.RemoveAt(index);
					HotKeysCallbacks.RemoveAt(index);
				}
				else if (!noWarnIfUnknown)
					Utilities.LogSubError("Unregister unknown hotkey: " + key);
			}


			[Flags]
			private enum HotModifierKeys : uint
			{// these differ from Keys ones
				Alt = 1,
				Control = 2,
				Shift = 4,
				Win = 8
			}
		}

		/// <summary>Adds a system wide HotKey with RegisterHotKey.  This is in switch handler just since it has a convenient form</summary>
		public static void AddHotKey(Keys key, Action callback)
		{
			m_EventForm.AddHotKey(key, callback);
		}

		/// <summary>Removes the key.  If second param is true there is no error logged if the hot key didn't exist</summary>
		public static void RemoveHotKey(Keys key, bool noWarnIfUnknown = false)
		{
			m_EventForm.RemoveHotKey(key, noWarnIfUnknown);
		}

		#endregion

		#region State
		/// <summary>false = up.  True = down</summary>
		protected bool m_State;
		/// <summary>which switch is currently pressed - others will be ignored</summary>
		protected static PhysicalSwitch g_SelectedSwitch;

		public event StateEventHandler StateChanged;

		/// <summary>called by the 'hardware' to initiate state change</summary>
		protected virtual void OnStateChange(bool down)
		{
			// keyboard in particular creates repeat downs, so ignore if already in this state:
			if (down == m_State) return;
			// only one can be selected at a time, record which, and ignore presses of other switches if one is downl
			if (down)
			{
				if (g_SelectedSwitch != null && g_SelectedSwitch != this)
					return;
				g_SelectedSwitch = this;
			}
			else
			{
				if (g_SelectedSwitch == this)
					g_SelectedSwitch = null;
			}
			m_State = down;
			StateChanged?.Invoke(down);
		}

		public bool State => m_State;

		#endregion

		#region (Overridable) member stuff
		public override string ToString() => Type + "(" + ParamDescription + ")";

		public abstract int Param { get; }
		public abstract string ParamDescription { get; }
		public abstract Types Type { get; }
		public abstract void Destroy(); // called by AssignSwitches to remove old one

		#endregion

		#region Global indexed list
		/// <summary>Must be called once before using other methods.  Further calls ignored</summary>
		static public void Initialise()
		{
			if (g_Switches != null)
				return;
			// faster(?) than using Shared Sub?.  Should be called on app startup
			g_Switches = new List<PhysicalSwitch>();
			m_EventForm = new EventForm();
			m_EventForm.Visible = true;
			KeySwitch.Initialise();
			JoySwitch.Initialise();
			// PointerSwitch doesn't require anything
		}

		private static List<PhysicalSwitch> g_Switches;

		/// <summary>As AssignSwitch(index, type, param), but reads type and param from config</summary>
		public static void AssignSwitchFromConfig(int index)
		{
			int defaultParam = index == 0 ? 32 : 13; // use space then enter as defaults
			AssignSwitch(index,
				(Types)Globals.Root.CurrentConfig.ReadInteger(Config.Switch_Type_ + index, (int)Types.Keyboard),
				Globals.Root.CurrentConfig.ReadInteger(Config.Switch_Param_ + index, defaultParam));
		}

		/// <summary>this is the ONLY correct public way of creating any switch</summary>
		public static void AssignSwitch(int index, Types type, int param)
		{
			if (g_Switches == null)
				throw new InvalidOperationException("Initialise not called on Physical switch");
			if (index < 0 || index > 7)
				throw (new ArgumentException("Switch index should be 0-7")); // could be increased, this is just arbitrary sanity limit
			while (g_Switches.Count <= index)
			{
				g_Switches.Add(null);
			}
			if (g_Switches[index] != null)
			{
				if (g_SelectedSwitch == g_Switches[index])
					g_SelectedSwitch = null;
				g_Switches[index].Destroy();
				g_Switches[index] = null;
			}
			PhysicalSwitch create = null;
			switch (type)
			{
				case Types.Null: // leave as nothing
					break;
				case Types.Joystick:
					create = (PhysicalSwitch)JoySwitch.CreateSwitch(param);
					break;
				case Types.Keyboard:
					create = KeySwitch.CreateSwitch((Keys)param);
					break;
				case Types.Pointer:
					create = PointerSwitch.Create(param);
					break;
				default:
					throw new ArgumentException("Invalid switch type: " + type);
			}
			g_Switches[index] = create;
		}

		public static void ClearSwitches()
		{
			for (int index = 0; index <= g_Switches.Count - 1; index++)
			{
				g_Switches[index]?.Destroy();
				g_Switches[index] = null;
			}
			g_SelectedSwitch = null;
		}

		public static PhysicalSwitch Switch(int index)
		{
			if (g_Switches == null || index >= g_Switches.Count)
				return null;
			return g_Switches[index];
		}

		#endregion

		protected static void OnError(string description)
		{
			Debug.Fail(description);
		}

		public static string GetParamDescription(Types type, int param)
		{
			// get description for Param for any type
			switch (type)
			{
				case Types.Null:
					return "--";
				case Types.Joystick:
					return JoySwitch.GetParamDescription(param);
				case Types.Keyboard:
					return KeySwitch.GetParamDescription(param);
				case Types.Pointer:
					return "";
				default:
					OnError("Unexpected type in GetParamDescription");
					return "?";
			}
		}

	}
}
