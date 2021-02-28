using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using SAW.CommandEditors;

namespace SAW.Commands
{
	public class CmdSawMoveToEdge : CommandWithIntID
	{
		/// <summary>0, 1, 2 = position in half-screen units</summary>
		public int X;
		public int Y;

		public override int SingleParam
		{
			get { return X + (Y << 16); }
			set
			{
				X = value & 0xffff;
				Y = value >> 16;
			}
		}

		protected override int[] GetPossibleIntIDs() => new[] { 0, 1, 2, 0x20000, 0x20001, 0x20002 };  // top/bottom and left/centre/right

		#region Info
		public override string GetCommandName() => Strings.Item("Script_CommandPart_" + YName) + " " + Strings.Item("Script_CommandPart_" + XName);

		public override string GetDescription() => Strings.Item("Script_Desc_" + YName.ToUpper() + "_" + XName.ToUpper());

		private string XName
		{
			get
			{
				switch (X)
				{
					case 0: return "Left";
					case 1: return "Centre";
					case 2: return "Right";
					default:
						Debug.Fail("Invalid CmdSawMoveEdge X");
						return "?";
				}
			}
		}

		private string YName
		{
			get
			{
				switch (Y)
				{
					case 0: return "Top";
					case 1: return "Middle";
					case 2: return "Bottom";
					default:
						Debug.Fail("Invalid CmdSawMoveEdge Y");
						return "?";
				}
			}
		}

		#endregion

		public override void Execute(ExecutionContext context)
		{
			Form window = context.View.FindForm();
			Screen screen = Screen.FromRectangle(window.Bounds);
			int xSpare = screen.WorkingArea.Width - window.Width;
			int ySpare = screen.WorkingArea.Height - window.Height;
			int x = screen.WorkingArea.Left + X * xSpare / 2;
			int y = screen.WorkingArea.Top + Y * ySpare / 2;
			window.Location = new System.Drawing.Point(x, y);
		}

	}

	/// <summary>Implements Minimise SAW, Hide SAW and Restore SAW</summary>
	public class CmdShowSAW : CommandWithIntID
	{

		public short m_WindowsCommand;

		#region ID based command

		protected override int[] GetPossibleIntIDs() => new[] { Windows.SW_SHOWNOACTIVATE, Windows.SW_MINIMIZE, Windows.SW_HIDE, Windows.SW_MAXIMIZE };

		public override int SingleParam
		{
			get { return m_WindowsCommand; }
			set { m_WindowsCommand = (short)value; }
		}

		private string SubCode
		{
			get
			{
				switch (m_WindowsCommand)
				{
					case Windows.SW_HIDE: return "Hide";
					case Windows.SW_MINIMIZE: return "Minimize";
					case Windows.SW_SHOWNOACTIVATE: return "Restore";
					case Windows.SW_MAXIMIZE: return "Maximize";
					default: return "?";
				}
			}
		}

		public override string GetCommandName() => Strings.Item("Script_Command_" + SubCode + "_SAW");

		public override string GetDescription() => Strings.Item("Script_Desc_" + SubCode.ToUpper() + "_SAW");

		// must override legacy data as this command only stores 16 bits
		public override void Read(ArchiveReader ar)
		{
			base.BaseRead(ar);
			m_WindowsCommand = ar.ReadInt16();
		}

		public override void Write(ArchiveWriter ar)
		{
			base.BaseWrite(ar);
			if (m_WindowsCommand == Windows.SW_MAXIMIZE)
			{
				ar.Write((short)Windows.SW_RESTORE);
				Globals.NonFatalOperationalError(Strings.Item("SAW_FILE_SAW6_Unsupported", "Maximize Window command"));
			}
			else
				ar.Write(m_WindowsCommand);
		}

		#endregion

		public override void Execute(ExecutionContext context)
		{
			Debug.WriteLine("Execute ShowWindow/" + m_WindowsCommand + " on SAW window due to script");
			Windows.ShowWindow(context.View.FindForm().Handle, m_WindowsCommand);
		}

	}

	public class CmdSawMove : CommandWithIntID
	{
		private short m_X;
		private short m_Y;

		#region ID stuff
		protected override int[] GetPossibleIntIDs() => new[] { 1, 0x10000, 0xffff, unchecked((int)0xffff0000) };

		public override int SingleParam
		{
			get { return m_X + (m_Y << 16); }
			set
			{
				m_X = (short)(value & 0xffff);
				m_Y = (short)(value >> 16);
			}
		}

		private string SubCode
		{
			get
			{
				if (m_X == -1)
					return "Left";
				if (m_X == 1)
					return "Right";
				if (m_Y == 1)
					return "Up";
				if (m_Y == -1)
					return "Down";
				return "?";
			}
		}

		public override string GetCommandName() => Strings.Item("Script_Command_Move_" + SubCode);

		public override string GetDescription() => Strings.Item("Script_Desc_MOVE_" + SubCode.ToUpper());

		#endregion

		public override void Execute(ExecutionContext context)
		{
			int step = context.View.MouseStep;
			Form window = context.View.FindForm();
			window.Location = new System.Drawing.Point(window.Left + m_X * step, window.Top + m_Y * step);
		}

	}


	public class CmdSawSize : CommandWithIntID
	{
		private short m_X;
		private short m_Y;

		#region ID stuff
		protected override int[] GetPossibleIntIDs() => new[] { 1, 0x10000, 0xffff, unchecked((int)0xffff0000) };

		public override int SingleParam
		{
			get { return m_X + (m_Y << 16); }
			set
			{
				m_X = (short)(value & 0xffff);
				m_Y = (short)(value >> 16);
			}
		}

		private string SubCode
		{
			get
			{
				if (m_X == -1)
					return "Narrower";
				if (m_X == 1)
					return "Wider";
				if (m_Y == 1)
					return "Taller";
				if (m_Y == -1)
					return "Shorter";
				return "?";
			}
		}

		public override string GetCommandName() => Strings.Item("Script_Command_Size_" + SubCode);
		public override string GetDescription() => Strings.Item("Script_Desc_SIZE_" + SubCode.ToUpper());

		#endregion

		public override void Execute(ExecutionContext context)
		{
			int step = context.View.MouseStep / 2; // this changes half as much as others
			Form window = context.View.FindForm();
			window.Size = new System.Drawing.Size(Math.Max(100, window.Width + m_X * step), Math.Max(100, window.Height + m_Y * step));
			// those minima weren't in SAW6, but I'm concerned it will crash completely if it goes negative
		}

	}

	public class CmdWait : ParamBasedCommand
	{
		public CmdWait() : base(new[] { Param.ParamTypes.Float })
		{ }

		public override void Execute(ExecutionContext context)
		{
			int mult = Globals.Root.CurrentConfig.ReadInteger(Config.Wait_Multipler, Config.Wait_Multipler_Default);
			float seconds = GetParamAsFloat(0) / 100 * mult; // time in seconds
			Thread.Sleep((int)(seconds * 1000));
		}

		public override ICommandEditor GetEditor() => new FloatingTimeEditor();
	}

	public class CmdCloseSAW : Command
	{
		public override void Execute(ExecutionContext context)
		{
			Globals.Root.CloseApplication();
		}
	}


	#region Selection Sets

	public class CmdOpenSelectionSet : ParamBasedCommand
	{ // just a placeholder atm
		public CmdOpenSelectionSet() : base(new[] { Param.ParamTypes.String })
		{
		}

		public override void Execute(ExecutionContext context)
		{
				context.View.PreviousSelectionSet = Globals.Root.CurrentDocument.Filename; // doesn't matter if not defined - PreviousSelectionSet null or empty just means that previous cannot work
				string target = m_ParamList[0].ValueAsString();
				CmdOpenApp.ProcessOutputDirective(ref target, context);
				TidyFilename(ref target, new[] {Document.StandardExtension, Document.SAW6Extension});
				if (!System.IO.File.Exists(target))
					context.View.OnFail("open selection set file not found: " + target);
				else
				{
					context.Terminate = true;
					Globals.QueueDelayedAction(() => Globals.Root.OpenFile(target, true, true));
				}
		}

		/// <summary>Adds the folder if it's missing, and also the extension.  For the extension this is only added if it results in a file which exists - searching the listed ones in order</summary>
		public static void TidyFilename(ref string filename, string[] defaultExtensions)
		{
			Debug.Assert(defaultExtensions.All(e => e.StartsWith(".")));
			if (!filename.Contains(System.IO.Path.DirectorySeparatorChar.ToString()))
			{ // need to add folder (if we can deduce one from current file)
				string current = Globals.Root.CurrentDocument.Filename;
				if (!string.IsNullOrEmpty(current))
					filename = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(current), filename);
			}
			if (!filename.Contains("."))
			{
				foreach (string extension in defaultExtensions)
					if (System.IO.File.Exists(filename + extension))
					{
						filename += extension;
						break;
					}
			}
		}

		public override void Read(ArchiveReader ar)
		{ // warn if the old "user selection set" is used.  Noone knows how it worked anyway
			base.Read(ar);
			if (m_ParamList.Count > 0 && m_ParamList[0].ValueAsString().Contains("##"))
				Globals.NonFatalOperationalError("[SAW_FILE_HASHSET]");
		}

		public override ICommandEditor GetEditor()
		{
			return new AppCommandEditor();
		}

	}

	public class CmdPreviousSelectionSet : Command
	{
		public override void Execute(ExecutionContext context)
		{
			if (string.IsNullOrEmpty(context.View.PreviousSelectionSet))
				context.View.OnFail("No previous selection set");
			else
			{
				context.Terminate = true;
				Globals.Root.OpenFile(context.View.PreviousSelectionSet, true, true);
			}
		}
	}

	public class CmdOpenItemTemplateSet : Command
	{
		public override void Execute(ExecutionContext context)
		{
			throw new NotImplementedException();
		}
	}


	#endregion


}