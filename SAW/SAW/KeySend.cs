using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32;

// to load SAW scripts: check for mouse commands.  If new mode, replace <esc> with <escape>

namespace SAW
{
	/// <summary>Support for sending keys to Windows. Use through Instance.  This is fairly slow to construct so is not static, but single so construction can be controlled</summary>
	internal class KeySend
	{
		// GIDEI uses odd codes which are in translations as KeyScript_xyz
		// New script uses .Net names for keys????
		// Names of keys (ie for display in menu) are taken from Splash translations Key_NetName.  This has no effect on scripting

		private static KeySend g_Instance;
		private static KeySend g_TestInstance;
		public static KeySend Instance
		{
			[DebuggerStepThrough]
			get { return g_Instance ?? (g_Instance = new KeySend()); }
		}
		/// <summary>Instance of class that doesn't/can't send anything but can be used to test scripts.
		/// Just call SendKeys and check for errors, nothing will be sent</summary>
		public static KeySend TestInstance
		{
			[DebuggerStepThrough]
			get { return g_TestInstance ?? (g_TestInstance = new TestSend()); }
		}

		/// <summary>This event is raised when a key is generated that will not automatically be detected by the keyswitch hook character detector (usually an Alt+0123 sequence).</summary>
		public event EventHandler<SingleFieldEventClass<char>> ExtraKeyForPredictions;

		#region Info about codes used for keys, some public, and constructor which initialises this
		/// <summary>look up from translations to virtual keycodes for GIDEI format</summary>
		private readonly Dictionary<string, Keys> m_GIDEICodes = new Dictionary<string, Keys>();
		/// <summary>Look up from script name to key code for new format</summary>
		private readonly Dictionary<string, Keys> m_NewCodes = new Dictionary<string, Keys>();
		/// <summary>Reverse lookup giving script name of key.  Only available in new form, there is no GIDEI version of this</summary>
		private readonly Dictionary<Keys, string> m_NewCodesReverse = new Dictionary<Keys, string>();
		public readonly string LockCode; // "lock"  some items from Strings stored here for speed (and convenience: IntelliSense!) of lookup
		public readonly string UnlockCode; // "unlock"
		public readonly string HoldCode; // "hold"
		public readonly string PressCode;
		public readonly string ReleaseCode; // "rel"
		public readonly string GIDEI_EscapeCode; // "esc"
		public readonly string OnCode; // "on"
		public readonly string OffCode; // "off"
		public readonly string RepeatCode;
		public readonly string EndRepeatCode;
		public readonly string DelayCode;
		/// <summary>this does not translate and cannot be changed.  Indicates the command uses new codes.
		/// Technically only applies to commands following itself, but should only be used at the beginning.  Mixing both formats is not officially supported</summary>
		public const string NEWFORMATINDICATOR = "<?new?>";
		/// <summary>The return value from VkKeyScan for '?' character - we need to know this since Windows returns this for many high char codes (eg Cyrillic, and other weirder alphabets) rather than 
		/// returning the no-key response.  (Which isn't documented?)</summary>
		private readonly byte[] m_QuestionVirtualKey;
		private KeySend()
		{
			m_QuestionVirtualKey = BitConverter.GetBytes(Windows.VkKeyScan('?'));

			LockCode = Strings.Item("KeyScript__Lock").ToLower();
			HoldCode = Strings.Item("KeyScript__Hold").ToLower();
			PressCode = Strings.Item("KeyScript__Press").ToLower();
			ReleaseCode = Strings.Item("KeyScript__Release").ToLower();
			UnlockCode = Strings.Item("KeyScript__Unlock").ToLower();
			GIDEI_EscapeCode = Strings.Item("KeyScript__GIDEI_Escape").ToLower();
			OnCode = Strings.Item("KeyScript__On").ToLower();
			OffCode = Strings.Item("KeyScript__Off").ToLower();
			RepeatCode = Strings.Item("KeyScript__Repeat").ToLower() + " "; // note this includes the space (will have been trimmed by Strings even if in file)
			EndRepeatCode = Strings.Item("KeyScript__EndRepeat").ToLower();
			DelayCode = Strings.Item("KeyScript__Delay").ToLower();

			foreach (string key in Strings.Keys) // Bit naff scanning them all, but I'd rather just keep them all in the same text file as usual
			{
				if (key.StartsWith("KeyScript_"))
				{
					string right = key.Substring(10); // 10 = "KeyScript_".Length
													  // After this should either be GIDEI_xyz or NEW_xyz or _whatever (last is for any text which is not a key, but needed here)
					if (right.StartsWith("GIDEI_"))
					{
						right = right.Substring(6);
						// the rest of the name is the keycode.  The text is the user text
						// text system won't allow 2 lines with the same ID, so single apostrophes can be appended to the ID, and these are removed here to get the actual key name
						Keys keyCode = (Keys)Enum.Parse(typeof(Keys), right.Trim('\''));
						string name = Strings.Item(key).ToLower();
						m_GIDEICodes.Add(name, keyCode);
					}
					else if (right.StartsWith("NEW_"))
					{
						right = right.Substring(4);
						// the rest of the name is the keycode.  The text is the user text
						Keys keyCode = (Keys)Enum.Parse(typeof(Keys), right.Trim('\'')); // permits the ' as above, but shouldn't be needed?
						string name = Strings.Item(key).ToLower();
						m_NewCodes.Add(name, keyCode);
						if (!m_NewCodesReverse.ContainsKey(keyCode))
							m_NewCodesReverse.Add(keyCode, name);
					}
				}
			}
			foreach (Keys key in Enum.GetValues(typeof(Keys)))
			{
				string name = key.ToString();
				string lower = name.ToLower();
				if (name.Length > 1)// ignore single letters!
				{
					if (!m_NewCodes.ContainsKey(lower))
					{
						// and keys in translations have priority
						m_NewCodes.Add(lower, key);
						if (!m_NewCodesReverse.ContainsKey(key))
							m_NewCodesReverse.Add(key, name);
					}
					if (!m_GIDEICodes.ContainsKey(lower))
						m_GIDEICodes.Add(lower, key);
				}
			}
		}

		/// <summary>strFullToken is used in any error messages if provided</summary>
		private Keys LookupGIDEIKeyName(string token, string fullToken = "")
		{
			Debug.Assert(token.ToLower() == token, "LookupKeyName should already be lower case");
			if (m_GIDEICodes.ContainsKey(token))
				return m_GIDEICodes[token];
			throw new UserException(Strings.Item("KeyScript__UnknownKey").Replace("%0", string.IsNullOrEmpty(fullToken) ? token : fullToken));
		}

		/// <summary>strFullToken is used in any error messages if provided</summary>
		private Keys LookupNewKeyName(string token, string fullToken = "")
		{
			Debug.Assert(token.ToLower() == token, "LookupKeyName should already be lower case");
			if (m_NewCodes.ContainsKey(token))
				return m_NewCodes[token];
			throw new UserException(Strings.Item("KeyScript__UnknownKey").Replace("%0", string.IsNullOrEmpty(fullToken) ? token : fullToken));
		}

		private Modes LookupModeName(string text)
		{
			if (text == HoldCode)
				return Modes.Hold;
			if (text == LockCode)
				return Modes.Lock;
			if (text == ReleaseCode || text == UnlockCode)
				return Modes.Release;
			// although press is the default usually modifiers will hold, so this may need to be explicit
			if (text == PressCode)
				return Modes.Press;
			return Modes.None;
		}

		/// <summary>Returns the code to use for the given key in new format scripts.
		/// Currently matches the .net name, but that may need to change?</summary>
		public string GetKeyNewCode(Keys key)
		{
			string code;
			if (m_NewCodesReverse.ContainsKey(key))
				code = m_NewCodesReverse[key];
			else
			{
				code = key.ToString();
				Debug.Fail("Key " + key + " not in new code list in KeySend");
			}
			if (code.Length > 1)
				return "<" + m_NewCodesReverse[key] + ">";
			return code;
		}

		public string GetKeyDescription(Keys key)
		{
			switch (key)
			{
				case Keys.Shift:
				case Keys.Control:
				case Keys.Alt:
					return Strings.Item("Key_" + key);
				default:
					// this cannot be used for Shift/Control/Alt since it always treats them as modifiers
					return GUIUtilities.KeyDescription(key);
			}
		}

		public string GetModeName(Modes mode)
		{
			switch (mode)
			{
				case Modes.Hold: return HoldCode;
				case Modes.Lock: return LockCode;
				case Modes.Press: return PressCode;
				case Modes.Release: return ReleaseCode;
				default:
					Debug.Fail("Unexpected mode in GetModeName: " + mode);
					return "";
			}
		}

		#endregion

		/// <summary>all the possible ways that a key can be processed </summary>
		public enum Modes
		{
			/// <summary>Presses and releases the key</summary>
			Press,
			/// <summary>Presses and holds the key until the next non-modifier key has been pressed</summary>
			Hold,
			Lock,
			/// <summary>sends key up and releases ALL locks</summary>
			Release,
			/// <summary>Invalid</summary>
			None
		}

		public int GetKeyboardCode(char value)
		{
			byte[] scan = BitConverter.GetBytes(Windows.VkKeyScan(value));
			return scan[0];
		}

		/// <summary>all keys which have been Locked.  This doesn't TECHNICALLY need to be only modifier keys
		/// note that this is maintained between calls to SendStringKeys.  There is no need to do any indexing of this; it would typically contain zero or one items</summary>
		private readonly List<Keys> m_Locked = new List<Keys>();
		/// <summary>this is automatically cleared at the end of each keypress</summary>
		private readonly List<Keys> m_Held = new List<Keys>();
		// member variables used during SendStringKeys.  A bit nasty to store these as effectively globals, but otherwise they would ALL need to be passed everywhere
		private string m_Send; // the text sent to SendStringKeys
		private List<Windows.tagINPUT> m_Output = new List<Windows.tagINPUT>();
		private int m_RepeatCount;
		/// <summary>The value of m_Send after hitting a REPEAT tag - m_Send is reset to this value to perform the repeat.  Cleared to null when repeat completed</summary>
		private string m_SendFromRepeat;

		/// <summary>Sends the given key sequence, in either new or GIDEI format (former if enclosed in '' or contains &lt;?new?&gt;</summary>
		public bool SendStringKeys(string send, int delay)
		{
			Debug.WriteLine("SendKeys: " + send);
			lock (this)
			{
				try
				{
					bool newMode = false; // true if we've encountered <?new?> which forces into new only mode <esc> then acts as a key not a GIDEI
					if (send.StartsWith("'") && send.EndsWith("'"))
					{
						send = send.Substring(1, send.Length - 2);
						newMode = true;
					}
					m_Send = send;
					m_Output.Clear();
					// returns true if all succeeded.  strSend can be coded using GIDEI or "new" SAW codes
					while (!string.IsNullOrEmpty(m_Send)) // m_Send is reduced as chars are pulled from it
					{
						bool validToken = false;
						if (m_Send[0] == '<' && m_Send.Length >= 2 && m_Send[1] != ' ') // ignores any < immediately followed by space
						{
							// introduces all sorts of codes
							var token = GetToken(1, false, '>')?.ToLower(); // returns null if invalid
							if (!string.IsNullOrEmpty(token))
							{
								validToken = true;
								m_Send = m_Send.Substring(token.Length + 2);
								if (token == GIDEI_EscapeCode && !newMode)
									ProcessGIDEI();
								else if (token == "?new?")
									newMode = true;
								else if (token == "less")
								{
									validToken = false;
									m_Send = "<" + m_Send; // will cause the character to be emitted in next branch
								}
								else // is one of the new codes
									ProcessNew(token);
							}
						}
						if (!validToken) // is not an escape sequence starting with <
						{
							char character = m_Send[0];
							if (m_Held.Any()) // following something like <Ctrl> we don't want to interpret a capital as meaning shift is also pressed
								character = char.ToLower(character);
							byte[] intVirtual = BitConverter.GetBytes(Windows.VkKeyScan(character));
							// VkKeyScan does not return an Int16 compatible with Windows.Forms.Keys
							// rather the bottom byte is the key code, and the top byte is the modifier state
							if (intVirtual[0] == 0 || intVirtual[0] == 255
								|| (intVirtual[0] == m_QuestionVirtualKey[0] && intVirtual[1] == m_QuestionVirtualKey[1] && character != '?')) // failed - no key found for this char
																																			   // last condition checks for "?" returned, but character requested was not "?".  Windows does seem to return a valid response of Shift+OemQuestion for chars in very different languages
								AddAltKey(character);
							else
							{ // valid key - queue any modifiers first
								if ((intVirtual[1] & 1) > 0 && !m_Locked.Contains(Keys.ShiftKey))
									AddCommand(Modes.Hold, Keys.ShiftKey);
								if ((intVirtual[1] & 2) > 0 && !m_Locked.Contains(Keys.ControlKey))
									AddCommand(Modes.Hold, Keys.ControlKey); // Not sure how many plain characters need to be typed using control!
								if ((intVirtual[1] & 4) > 0 && !m_Locked.Contains(Keys.LMenu))
									AddCommand(Modes.Hold, Keys.LMenu);
								AddCommand(Modes.Press, (Keys)intVirtual[0]);
							}
							m_Send = m_Send.Substring(1);
						}
						if (m_Output.Any() && delay > 0)
						{ // with a delay between keys, they need to be sent now
							SendQueuedOutput();
							Thread.Sleep(delay);
						}
					}
					if (m_SendFromRepeat != null)
						throw new UserException("Invalid key command: no <END_REPEAT> following <REPEAT n>");
					//AddReleaseHeld();
					return SendQueuedOutput();
				}
				finally
				{
					m_Output.Clear();
				}
			}
		}

		// not sure this quite works as SAW.  The mode applies to all following keys within the escape, whereas I think for SAW it only does the next key
		// GIDEI: http://trace.wisc.edu/docs/gidei/gidei.htm#glance
		// current GIDEI mouse unsupported
		private void ProcessGIDEI()
		{
			bool first = true; // the mode modifiers can only be used immediately following the escape code
			while (!string.IsNullOrEmpty(m_Send))
			{
				string token = GetToken(0, true, '.', ',');
				// note that strToken might be empty, because GIDEI usually had a, after the initial escape to introduce hold or lock
				// it is just ignored here
				char terminator = m_Send[token.Length];
				m_Send = m_Send.Substring(token.Length + 1);
				token = token.Trim().ToLower();
				if (!string.IsNullOrEmpty(token))
				{
					Modes mode = LookupModeName(token);
					if (token == Strings.Item("KeyScript__GIDEI_Combine"))
						mode = Modes.Hold; // should other modes only work for one key
					if (mode != Modes.None)
					{
						if (!first)
							throw new UserException("[KeyScript__GIDEI_ModeNotFirst]");
						if (mode == Modes.Release && terminator == '.')
							// special case.  Release without any keys releases all locked keys
							AddReleaseAll();
					}
					else // should be the name of a key
					{
						Keys eKey = LookupGIDEIKeyName(token);
						AddCommand(Modes.Press, eKey);
					}
					first = false; // note that an empty item does not clear bolFirst.  There can be a spurious comma after <esc>
				}
				if (terminator == '.')
					break;
			}
		}

		/// <summary>Processes one token in new format;  the containing &lt;&gt; has already been removed</summary>
		private void ProcessNew(string token)
		{
			if (token == EndRepeatCode)
				ProcessEndRepeat();
			// must be before suffix stuff as this contains Return
			//else if (strToken == m_strEndRepeat)  // ?????
			//{
			//	ProcessEndRepeat();
			//	return;
			//}
			int suffix = token.LastIndexOf('_');
			Modes mode = Modes.Press;
			string remaining = token;
			bool modeWasExplicit = false; // true if the mode was explicitly stated
			if (suffix > 0)
			{
				// ends _Hold _Release _Lock _Unlock
				if (token.Contains('+'))
					throw new UserException(Strings.Item("KeyScript__PlusUnderscore").Replace("%0", token));
				if (suffix == token.Length - 1)
					throw new UserException(Strings.Item("KeyScript__InvalidSuffix").Replace("%0", token));
				string command = token.Substring(suffix + 1);
				remaining = token.Substring(0, suffix);
				if (command == OffCode || command == OnCode)
				{
					ProcessOnOff(remaining, command == OnCode, token);
					return;
				}
				mode = LookupModeName(command);
				if (mode == Modes.None)
					throw new UserException(Strings.Item("KeyScript__InvalidSuffix").Replace("%0", token));
				modeWasExplicit = true;
			}
			// single key
			if (token == ReleaseCode || token == UnlockCode) // not in SAW 6, but like GIDEI a <rel> alone releases all
				AddReleaseAll();
			else if (token.StartsWith(RepeatCode)) // m_strRepeat includes final space required
				ProcessRepeat(token);
			else if (token.StartsWith(DelayCode))
			{
				int delay;
				if (!int.TryParse(token.Substring(DelayCode.Length), out delay))
					throw new UserException("Delay value not understood");
				SendQueuedOutput();
				Thread.Sleep(delay);
			}
			else
			{
				while (!string.IsNullOrEmpty(remaining)) // allows any number to be linked by +.  All keys before + are held regardless of whats after
				{
					string keyName = remaining;
					if (remaining.Contains("+"))
					{
						keyName = remaining.Substring(0, remaining.IndexOf("+"));
						remaining = remaining.Substring(remaining.IndexOf("+") + 1);
					}
					else
						remaining = "";
					Keys key = LookupNewKeyName(keyName, "<" + token + ">");
					if (IsModifier(key))
						AddCommand(mode == Modes.Press && !modeWasExplicit ? Modes.Hold : mode, key); // holds, unless overridden by _xyz
					else
						AddCommand(remaining == "" && !modeWasExplicit ? mode : Modes.Hold, key); // see above - keys before + automatically hold
				}
			}
		}

		/// <summary>Implements the command starting a repeating sequence </summary>
		private void ProcessRepeat(string token)
		{// some of the errors here don't translate - rather rare(?)
			if (m_SendFromRepeat != null)
				throw new UserException("Nested <REPEAT n> commands are not permitted");
			string number = token.Substring(RepeatCode.Length); // skip - m_strRepeat includes space
			if (!int.TryParse(number, out m_RepeatCount))
				throw new UserException("<" + token + ">: Number not understood");
			if (m_RepeatCount < 0 || m_RepeatCount > 100)
				throw new UserException("<REPEAT n>: only values 1 to 100 are permitted");
			// remember the entire remaining script after the <REPEAT> - resetting m_Send to this will reset to start
			m_SendFromRepeat = m_Send;
		}

		private void ProcessEndRepeat()
		{
			if (m_SendFromRepeat == null)
				throw new UserException("<END_REPEAT> can only follow <REPEAT n>");
			m_RepeatCount--;
			if (m_RepeatCount > 0)
				m_Send = m_SendFromRepeat;
			else
				m_SendFromRepeat = null; // so we know that it was properly completed
		}

		private void ProcessOnOff(string key, bool isOn, string token)
		{
			Keys eKey = LookupNewKeyName(key);
			if (eKey != Keys.NumLock && eKey != Keys.CapsLock && eKey != Keys.Scroll)
				throw new UserException(Strings.Item("KeyScript__InvalidOnOff").Replace("%0", token));
			int state = isOn ? 1 : 0;
			int current = Windows.GetKeyState((int)eKey) & 1;
			if (state != current)
				AddCommand(Modes.Press, eKey);
		}

		/// <summary>Reads text up to any of the given terminators, returning it (without the terminator).  Optionally throws an exception if it reaches EOL, otherwise returns null</summary>
		private string GetToken(int startIndex, bool errorIfUnterminated, params char[] terminator)
		{
			// Returns the content of the following token within the text.  I.e. the text terminated with any of chTerminator
			// returns the text without the terminating character
			System.Text.StringBuilder output = new System.Text.StringBuilder();
			while (startIndex < m_Send.Length)
			{
				char ch = m_Send[startIndex];
				if (Array.IndexOf(terminator, ch) >= 0)
					return output.ToString();
				output.Append(ch);
				startIndex += 1;
			}
			if (!errorIfUnterminated)
				return null;
			output.Length = 0;
			foreach (char ch in terminator)
			{
				output.Append(ch).Append(" ");
			}
			throw new UserException(Strings.Item("KeyScript__Unterminated_Escape").Replace("%0", output.ToString()));
		}

		#region Adding events
		private void AddCommand(Modes mode, Keys eKey)
		{
			// note that holding all locking a key which is already down just send another key down
			switch (mode)
			{
				case Modes.Lock:
					AddKeyEvent(eKey, true);
					if (!m_Locked.Contains(eKey))
						m_Locked.Add(eKey);
					if (m_Held.Contains(eKey))
						m_Held.Remove(eKey); // to make sure that ReleaseHeld doesn't release it
					break;
				case Modes.Release:
					AddKeyEvent(eKey, false);
					if (m_Locked.Contains(eKey))
						m_Locked.Remove(eKey);
					if (m_Held.Contains(eKey))
						m_Held.Remove(eKey);
					break;
				case Modes.Hold:
					AddKeyEvent(eKey, true);
					if (!m_Locked.Contains(eKey) && !m_Held.Contains(eKey))
						m_Held.Add(eKey);
					break;
				case Modes.Press:
					AddKeyEvent(eKey, true);
					if (!m_Locked.Contains(eKey))
						AddKeyEvent(eKey, false);
					AddReleaseHeld();
					break;
				case Modes.None:
					Debug.Fail("AddCommand (None)");
					break;
			}
		}

		private bool m_AltMode;
		/// <summary>Adds an event changing the state of the given key.  Only for low level use - usually use SendStringKeys</summary>
		public void AddKeyEvent(Keys key, bool down)
		{
			Windows.tagINPUT create = new Windows.tagINPUT(Windows.INPUTTYPE.KEYBOARD);
			create.union.ki.dwExtraInfo = UIntPtr.Zero;
			if (!down)
				create.union.ki.dwFlags = Windows.KEYEVENTF.KEYUP;
			if (key != Keys.Menu && key != Keys.ControlKey && m_AltMode == false) // eKey <> Keys.LMenu And eKey <> Keys.RMenu
			{
				// must NOT be used when sending Alt codes.  But is required to make (eg) ctrl-shift-left work
				create.union.ki.dwFlags = create.union.ki.dwFlags | Windows.KEYEVENTF.EXTENDEDKEY;
			}
			create.union.ki.time = 0;
			create.union.ki.wScan = (short)Windows.MapVirtualKey((uint)key, Windows.MAPVK_VK_TO_VSC); // required to make Alt+NNNN work
			if (Switches.Switching.KeySwitch.KeyCurrentlyUsedForSwitching(key))
				create.union.ki.wScan = 0; // the keyhook will ignore keys if the scan code is not set at all
			create.union.ki.wVk = (short)key;
			Debug.WriteLine(key + " " + (down ? "down" : "up"));
			m_Output.Add(create);
		}

		private void AddReleaseAll()
		{
			AddReleaseHeld();
			foreach (Keys key in m_Locked)
			{
				AddKeyEvent(key, false);
			}
			m_Locked.Clear();
		}

		private void AddReleaseHeld()
		{
			foreach (Keys key in m_Held)
			{
				AddKeyEvent(key, false);
			}
			m_Held.Clear();
		}

		/// <summary>types a character using Alt + keypad sequence</summary>
		private void AddAltKey(char ch)
		{
			AddReleaseHeld();
			m_AltMode = true; // blocks using ExtendedKey flag.  God knows what this does, but from experience it must be used normally, but not here
			AddKeyEvent(Keys.LMenu, true);
			string coded;
			if (ch <= 999) // up to 999 we can sent Alt N N N N where N is a numeric keypad number
				coded = ((int)ch).ToString("0000"); // leading zeroes required
			else // above that send Alt + "+ H H H" where H is a hex digit
			{// method D in this: https://altcodeunicode.com/how-to-use-alt-codes/
			 // requires registry: HKEY_CURRENT_USER/Control Panel/Input Method/EnableHexNumpad with string type REG_SZ assigned a data value of 1
				CheckHexRegistry();
				AddCommand(Modes.Press, Keys.Add);
				coded = ((int)ch).ToString("x").ToUpper(); // ToUpper needed due to code below
			}
			foreach (char send in coded)
			{
				if (send >= '0' && send <= '9')
					AddCommand(Modes.Press, Keys.NumPad0 + send - '0');
				else // A to F
				{
					Debug.Assert(send >= 'A' && send <= 'F');
					AddCommand(Modes.Press, Keys.A + char.ToUpper(send) - 'A');
				}
			}
			AddKeyEvent(Keys.LMenu, false);
			m_AltMode = false;
			ExtraKeyForPredictions?.Invoke(this, ch);
		}

		private static bool g_HexRegistryChecked;
		/// <summary>Checks the registry has been configured to enable Alt++xxx codes
		/// 
		/// </summary>
		private static void CheckHexRegistry()
		{
			if (g_HexRegistryChecked)
				return;
			g_HexRegistryChecked = true;
			RegistryKey key = Registry.CurrentUser.OpenSubKey("Control Panel\\Input Method");
			var value = key.GetValue("EnableHexNumpad");
			if (value != null && value.ToString() == "1")
				return;
			throw new UserException("[SAW_Run_NoHexNumpad]");
		}

		/// <summary>Adds a mouse event to the queue, with no movement in mouse location.  Data param only needed for wheel/XUP/XDOWN events</summary>
		public virtual void AddMouseEvent(Windows.MOUSEEVENTF eventType, int mouseData = 0)
		{
			Debug.Assert((eventType & Windows.MOUSEEVENTF.ABSOLUTE) == 0); // should be relative (as we will supply coords = 0)
			var data = new Windows.tagINPUT(Windows.INPUTTYPE.MOUSE);
			data.union.mi.dwFlags = eventType;
			// coords left at 0,0 (relative)
			data.union.mi.mouseData = mouseData;
			// time stamp left at zero
			// likewise extra info
			m_Output.Add(data);
		}

		#endregion

		/// <summary>Sends all queued commands to windows.  Generally use SendStringKeys instead; this is only for low level control</summary>
		public virtual bool SendQueuedOutput()
		{
			Windows.tagINPUT[] send = m_Output.ToArray();
			int entries = send.Length;
			m_Output.Clear();
			return entries == Windows.SendInput((uint)entries, send, Marshal.SizeOf(typeof(Windows.tagINPUT)));
		}

		/// <summary>Releases all locked keys</summary>
		public bool ReleaseAll()
		{
			if (m_Locked.Count == 0)
				return true;
			if (m_Output.Any())
				throw new InvalidOperationException("KeySend.ReleaseAll is the external method.  Use AddReleaseAll during parsing");
			lock (this)
			{
				m_Output = new List<Windows.tagINPUT>();
				AddReleaseAll();
				return SendQueuedOutput();
			}
		}

		public bool HasHeldKeys => m_Held.Any();

		/// <summary>True if key is any of the 3 modifier key types (works for either the modifier codes or the individual key codes such as LShift)</summary>
		private static bool IsModifier(Keys key)
		{
			return key == Keys.ShiftKey || key == Keys.LShiftKey || key == Keys.RShiftKey || key == Keys.ControlKey ||
				   key == Keys.LControlKey || key == Keys.RControlKey || key == Keys.LMenu || key == Keys.RMenu || key == Keys.Menu
				   || key == Keys.Alt || key == Keys.Shift || key == Keys.Control;
		}

		#region Windows methods
		private static class NativeMethods
		{
		}
		#endregion

		/// <summary>Version of class that doesn't send anything but can be used to test scripts </summary>
		private class TestSend : KeySend
		{

			public override bool SendQueuedOutput()
			{
				return true;
			}

			public override void AddMouseEvent(Windows.MOUSEEVENTF eventType, int mouseData = 0)
			{ }

		}
	}
}
