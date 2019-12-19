using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using SAW.CommandEditors;

namespace SAW.Commands
{
	public class CmdSoundClick : Command
	{
		// uses its own audio player so it doesn't interrupt any other sounds
		private static AudioPlayer Player = new AudioPlayer();

		public override void Execute(ExecutionContext context)
		{
			if (!Globals.Root.CurrentConfig.ReadBoolean(Config.Sound_Click))
				return;
			Player.Stop();
			Player.PlayAsync(Resources.AM.SoundClick);
		}

	}

	public class CmdBeep : Command
	{
		public override void Execute(ExecutionContext context)
		{
			Console.Beep();
		}

	}

	public class CmdOutText : CommandWithIntID
	{

		#region Fields
		public enum Source : ushort
		{
			DisplayText = 20613,
			OutputText = 20614,
			PromptText = 20615,
			SpeechText = 20616
		}
		public enum Action : ushort
		{
			Send = 20617,
			Say = 20618,
			//Play = 20619, // no longer used?, but an equivalent is used in CmdOutTextOnOff
			Print = 20620
		}
		public Action m_Action;
		public Source m_Source;
		#endregion

		public CmdOutText() { }
		public CmdOutText(Action action, Source source)
		{
			m_Action = action;
			m_Source = source;
		}

		public override int SingleParam
		{
			get { return (int)m_Source + ((int)m_Action << 16); }
			set
			{
				m_Action = (Action)(value >> 16);
				m_Source = (Source)(value & 0xffff);
			}
		}

		#region Other meta
		public override string GetCommandName()
		{
			return Strings.Item("Script_CommandPart_" + m_Action) + " " + Strings.Item("Script_CommandPart_" + m_Source);
		}

		public override string GetDescription()
		{
			return Strings.Item("Script_Desc_" + m_Action.ToString().ToUpper() + "_" + m_Source.ToString().ToUpper());
		}

		protected override int[] GetPossibleIntIDs()
		{
			return (from Action action in Enum.GetValues(typeof(Action)) where action != Action.Print from Source source in Enum.GetValues(typeof(Source)) select (int)source + ((int)action << 16)).ToArray();
		}

		#endregion

		public override void Execute(ExecutionContext context)
		{
			string text;
			switch (m_Source)
			{
				case Source.DisplayText:
					text = context.TargetItem.Element.LabelText;
					break;
				case Source.OutputText:
					text = (context.TargetItem.Element as Item)?.OutputText ?? context.TargetItem.Element.LabelText; // defaults to same text for any non-SAW items
					break;
				case Source.PromptText:
					text = (context.TargetItem.Element as Item)?.PromptText;
					break;
				case Source.SpeechText:
					text = (context.TargetItem.Element as Item)?.SpeechText;
					// this also plays any sound file (now embedded) in addition
					var sound = (context.TargetItem.Element as Item)?.Sound?.Content;
					if (sound != null && Globals.Root.CurrentConfig.ReadBoolean(Config.Output_Sound, true))
					{
						context.View.PlaySoundResource(sound.GetStream());
						return; // if there was an actual sound, then the text is not played as well
					}
					break;
				default:
					context.View.OnError("Unexpected Out text source: " + m_Source);
					return;
			}
			if (string.IsNullOrEmpty(text))
				return;

			switch (m_Action)
			{
				case Action.Print:
					context.View.OnError("Print output no longer supported");
					break;
				case Action.Say:
					if (!Globals.Root.CurrentConfig.ReadBoolean(Config.Output_Speech, true))
						return;
					// visit scripts have a second option, that must also be set:
					if (context.ScriptType == Scriptable.ScriptTypes.Visit && !Globals.Root.CurrentConfig.ReadBoolean(Config.Output_SpeechVisit, true))
						return;
					context.View.Speak(text);
					break;
				case Action.Send:
					if (Globals.Root.CurrentConfig.ReadBoolean(Config.Output_Send, true))
					{
						context.View.SendKeys(text);
						float hideTime = Globals.Root.CurrentConfig.ReadSingle(Config.HideTime_OnSend);
						if (hideTime > 0) // hide window for N time
						{
							IntPtr handle = context.View.FindForm().Handle;
							Windows.ShowWindow(handle, Windows.SW_MINIMIZE);
							int startTime = Environment.TickCount;
							int endTime = startTime + (int)(hideTime * 1000);
							Task.Run(() =>
							{
								while (Environment.TickCount < endTime)
									Thread.Sleep(Math.Min(100, endTime - Environment.TickCount));
								context.View.FindForm().Invoke(new System.Action(() => Windows.ShowWindow(handle, Windows.SW_RESTORE)));
							});
						}
					}
					if (context.View.HideTime > 0)
					{
						Windows.ShowWindow(context.View.FindForm().Handle, Windows.SW_HIDE);
						Task.Run(() =>
						{
							Thread.Sleep((int)(1000 * context.View.HideTime));
							context.View.Invoke(new System.Action(() => Windows.ShowWindow(context.View.FindForm().Handle, Windows.SW_SHOWNOACTIVATE)));
						});
					}
					break;
			}
		}

	}

	/// <summary>Sets the time that SAW is hidden for after each text output</summary>
	public class CmdSetHideSAW : ParamBasedCommand
	{
		public CmdSetHideSAW() : base(new[] { Param.ParamTypes.Float })
		{
		}

		public override void Execute(ExecutionContext context)
		{
			context.View.HideTime = GetParamAsFloat(0);
		}

		public override ICommandEditor GetEditor()
		{
			return new FloatingTimeEditor();
		}

	}


	public class CmdMessage : CommandWithIntID
	{
		private short m_Message;
		private short m_Param;

		protected override int[] GetPossibleIntIDs()
		{
			List<int> list = new List<int>();
			list.AddRange(from short i in (new[] { WM_CUT, WM_COPY, WM_PASTE, WM_CLEAR, WM_UNDO }) select (int)i); // the LINQ is needed to convert from short to int
																												   // adds every combination of message and direction:
			list.AddRange(from message in new[] { WM_HSCROLL, WM_VSCROLL } from direction in new[] { SB_PAGEUP, SB_LINEUP, SB_LINEDOWN, SB_PAGEDOWN } select message + (direction << 16));
			return list.ToArray();
		}

		public override int SingleParam
		{
			get { return m_Message + (m_Param << 16); }
			set
			{
				m_Message = (short)(value & 0xffff);
				m_Param = (short)(value >> 16);
			}
		}

		private string SubCode
		{
			get
			{
				string code;
				switch (m_Message)
				{
					case WM_CLEAR: code = "Clear"; break;
					case WM_UNDO: code = "Undo"; break;
					case WM_PASTE: code = "Paste"; break;
					case WM_COPY: code = "Copy"; break;
					case WM_CUT: code = "Cut"; break;
					case WM_HSCROLL: code = "HScroll"; break;
					case WM_VSCROLL: code = "VScroll"; break;
					default:
						code = "?";
						Debug.Fail("Unexpected CmdMessage message");
						break;
				}
				if (MessageIsScroll(m_Message))
					switch (m_Param)
					{
						case SB_LINEDOWN: code += "_Down"; break;
						case SB_LINEUP: code += "_Up"; break;
						case SB_PAGEDOWN: code += "_PageDown"; break;
						case SB_PAGEUP: code += "_PageUp"; break;
						default:
							code += "_?";
							Debug.Fail("Unexpected CmdMessage scroll part");
							break;
					}
				return code;
			}
		}

		public override string GetCommandName()
		{
			return Strings.Item("Script_Command_" + SubCode);
		}

		public override string GetDescription()
		{
			return Strings.Item("Script_Desc_" + SubCode.ToUpper());
		}

		internal override void CompleteCommandListEntry(CommandList.Entry entry)
		{ // the commands need to be assigned to different folders in frmAddCommand
			base.CompleteCommandListEntry(entry);
			int[] singleParams = (int[])CommandListEntry.CustomData;
			entry.AddGUIPath = new string[singleParams.Count()];
			for (int index = 0; index < singleParams.Length; index++)
			{
				int code = singleParams[index] & 0xffff;
				if (MessageIsScroll((short)code))
					entry.AddGUIPath[index] = "Scroll";
				else
					entry.AddGUIPath[index] = "Edit";
			}
		}

		/// <summary>True if this WM_ code is one of the scroll ones, in which case the other param is significant</summary>
		private bool MessageIsScroll(short messageCode)
		{
			switch (messageCode)
			{
				case WM_HSCROLL:
				case WM_VSCROLL:
					return true;
				default:
					return false;
			}

		}

		public override void Execute(ExecutionContext context)
		{
			Windows.GUITHREADINFO info = new Windows.GUITHREADINFO();
			info.cbSize = Marshal.SizeOf(info);
			var guiRes = Windows.GetGUIThreadInfo(0, ref info); // thread 0 returns the Foreground thread
			if (!guiRes)
				context.View.OnError("GetGUIThreadInfo failed");
			else if (info.hwndFocus.IsZero())
				context.View.OnError("Null focus window in CmdMessage");
			else
			{
				IntPtr result = Windows.SendMessage(info.hwndFocus, m_Message, m_Param, IntPtr.Zero);
				//Debug.WriteLine("SendMessage, result = " + result + ", GetLastError = " + GetLastError());
			}
		}

		#region Windows definitions

		private const short WM_HSCROLL = 0x0114;
		private const short WM_VSCROLL = 0x0115;
		private const short WM_CUT = 0x0300;
		private const short WM_COPY = 0x0301;
		private const short WM_PASTE = 0x0302;
		private const short WM_CLEAR = 0x0303;
		private const short WM_UNDO = 0x0304;

		// up/down can also be used for left/right.  Although C++ defines left/right versions they have the same values
		private const short SB_LINEUP = 0;
		private const short SB_LINEDOWN = 1;
		private const short SB_PAGEUP = 2;
		private const short SB_PAGEDOWN = 3;



		#endregion
	}

	public class CmdSpeakClipboard : Command
	{

		public override void Execute(ExecutionContext context)
		{
			string text = System.Windows.Forms.Clipboard.GetText();
			if (string.IsNullOrEmpty(text))
				return;
			context.View.Speak(text);
		}

		public override string GetScriptWithParams(bool forSAW6)
		{
			if (forSAW6)
			{
				Globals.NonFatalOperationalError(Strings.Item("SAW_FILE_SAW6_Unsupported", GetCommandName()));
				return "; Speak clipboard text";
			}
			return base.GetScriptWithParams(false);
		}

	}


	public class CmdDisplayPromptText : Command
	{

		public override void Execute(ExecutionContext context)
		{
			if (!Globals.Root.CurrentConfig.ReadBoolean(Config.SAW_Prompts, true))
				return;
			int ID = context.Page.HelpSAWID;
			if (ID < 0)
				return;
			var scriptable = context.Page.FindScriptableByID(ID);
			if (scriptable == null)
				return;
			string text = (context.TargetItem?.Element as Item)?.PromptText;
			if (text == null)
				return;
			scriptable.Element.LabelText = text;
		}
	}


}