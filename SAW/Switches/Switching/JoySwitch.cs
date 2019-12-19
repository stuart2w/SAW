using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using Microsoft.DirectX.DirectInput;
using SAW;

//https://stackoverflow.com/questions/3929764/taking-input-from-a-joystick-with-c-sharp-net

namespace Switches.Switching
{
	internal class JoySwitch : PhysicalSwitch
	{

		private int m_switch;

		#region Shared list of switches
		private static Dictionary<int, JoySwitch> g_Switches;
		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		internal static new void Initialise()
		{
			// faster(?) than using Shared Sub
#if !DEBUG
			// causes LoaderLock warnings, but I think it's OK?
			MasterSwitchChanged += PhysicalMasterSwitchChanged;
			g_Switches = new Dictionary<int, JoySwitch>();

			m_Timer = new Timer();
			m_Timer.Interval = 10;
			m_Timer.Enabled = false;
			m_Timer.Tick += Timer_Tick;
#endif
		}

		private static void PhysicalMasterSwitchChanged()
		{
			// global on/off has changed
			switch (MasterSwitch)
			{
				case MasterModes.Off:
					m_Timer.Enabled = false;
					break;
				case MasterModes.On:
					if (g_Switches.Count > 0)
					{
						Attach();
						if (m_Joystick != null)
							m_Timer.Enabled = true;
					}
					else
						m_Timer.Enabled = false;
					break;
				case MasterModes.Detect:
					Attach();
					if (m_Joystick != null)
						m_Timer.Enabled = true;
					break;
				default:
					OnError("Unexpected MasterMode: " + MasterSwitch);
					break;
			}
			if (MasterSwitch != MasterModes.Detect)
				g_LastDetect = null; // to avoid keeping this in mem (it came from win anyway)
		}

		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		internal static object CreateSwitch(int number)
		{
			if (number < 0)
				throw new ArgumentException("Joystick switch index must be >=0"); // excessively high numbers just do nothing - state is never changed
																				  // call PhysicalSwitch.AssignSwitch, NOT this
			if (g_Switches.ContainsKey(number))
				OnError("Creating duplicate switch");
			else
				g_Switches.Add(number, new JoySwitch(number));
			return g_Switches[number];
		}

		public override void Destroy()
		{
			if (g_Switches.ContainsKey(m_switch))
				g_Switches.Remove(m_switch);
			else
				OnError("Destroying non-existent joystick switch: " + m_switch);
		}
		#endregion

		#region Global control stuff
		// http://msdn.microsoft.com/en-us/library/windows/desktop/ms837190.aspx
		private static Device m_Joystick;
		private static Timer m_Timer;
		private static byte[] g_LastDetect; // used by timer when in detect mode: want to check ALL switches

		private static void Attach()
		{
			// creates DirectX device, but does not start timer
			if (m_Joystick != null)
				return;

			try
			{
				// if VS gives waffle here about LoaderLock when debugging, turn off Debug menu > Exceptions > Managed Debug Assistants > LoaderLock
				foreach (DeviceInstance di in Manager.GetDevices(DeviceClass.GameControl, EnumDevicesFlags.AttachedOnly))
				{
					m_Joystick = new Device(di.InstanceGuid);
					break;
				}

				if (m_Joystick == null)
				{
					OnError("Failed to connect to DirectX joystick device - no joystick attached?");
					return;
				}

				m_Joystick.SetCooperativeLevel(m_EventForm.Handle, CooperativeLevelFlags.Background | CooperativeLevelFlags.NonExclusive);
				m_Joystick.Acquire();

			}
			catch (Exception ex)
			{
				OnError("Error connecting to DirectX joystick device: " + ex.Message);
				return;
			}

		}

		private static void Detach()
		{
			// NB only used on error
			try
			{
				m_Joystick.Unacquire();
				m_Joystick.Dispose();
			}
			catch
			{ }
			m_Joystick = null;
		}

		private static void Timer_Tick(object sender, EventArgs e)
		{
			if (m_Joystick == null)
				return;
			try
			{
				m_Joystick.Poll();
				JoystickState state = m_Joystick.CurrentJoystickState;
				byte[] buttons = state.GetButtons();
				switch (MasterSwitch)
				{
					case MasterModes.On:
						foreach (JoySwitch joySwitch in g_Switches.Values)
						{
							if (joySwitch.m_switch > buttons.Length - 1)
								continue;
							bool isDown = buttons[joySwitch.m_switch] >= 128; // hi bit set marks down.  otherwise up or non-existent
							if (isDown != joySwitch.State)
							{
								Debug.WriteLine("joy state change " + joySwitch.ParamDescription);
								joySwitch.OnStateChange(isDown);
							}
						}
						break;
					case MasterModes.Detect:
						// will be null on first pass - can't detect change until 2 passes!
						if (g_LastDetect?.Length == buttons.Length) // just in case Windows does a nasty
						{
							for (int index = 0; index <= (g_LastDetect?.Length - 1); index++)
							{
								if (buttons[index] >= 128 && (g_LastDetect[index] < 128))
									// if was up and is now down
									OnSwitchDetected(Types.Joystick, index);
							}
						}
						g_LastDetect = buttons;
						break;
				}
			}
			catch (InputLostException)
			{
				OnError("Joystick disconnected");
				Detach();
			}
			catch (Exception ex) when (!Globals.Root.IsDebug)
			{
				Utilities.LogSubError(ex);
				OnError("Joystick error: disconnecting...");
				Detach();
			}
		}
		#endregion

		#region Member stuff

		private JoySwitch(int which)
		{
			m_switch = which;
		}

		public override int Param => m_switch;
		public override string ParamDescription => m_switch.ToString();
		public override Types Type => Types.Joystick;
		public static string GetParamDescription(int param) => Strings.Item("Switch_ButtonNumber").Replace("%0", (param + 1).ToString());

		#endregion

	}
}