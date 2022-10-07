using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;
using SAW.CommandEditors;
using SAW.Shapes;

namespace SAW.Commands
{

	#region App launch/close commands
	public class CmdOpenApp : ParamBasedCommand
	{
		public CmdOpenApp() : base(new[] { Param.ParamTypes.String })
		{ }

		internal override void Execute(ExecutionContext context)
		{
			string target = ParamList[0].ValueAsString();
			ProcessOutputDirective(ref target, context);
			try
			{
				Process.Start(target);
			}
			catch (Exception e)
			{ context.View.OnError(e.Message); }
		}

		/// <summary>Checks the provided app or file to see if it is ":output" and replaces it if so</summary>
		/// <remarks>The ":output" deliberately doesn't translate</remarks>
		internal static void ProcessOutputDirective(ref string requested, ExecutionContext context)
		{
			if (requested == USEOUTPUT && context.TargetItem.Element is Item)
				requested = context.TargetItem.OutputText;
		}

		/// <summary>If the app is this, then the item output field is used instead</summary>
		public const string USEOUTPUT = ":output";

		internal override ICommandEditor GetEditor() => new AppCommandEditor();
	}

	public class CmdParamApp : ParamBasedCommand
	{
		public CmdParamApp() : base(new[] { Param.ParamTypes.String, Param.ParamTypes.String })
		{ }

		internal override void Execute(ExecutionContext context)
		{
			string target = ParamList[0].ValueAsString();
			CmdOpenApp.ProcessOutputDirective(ref target, context);
			string param = ParamList[1].ValueAsString();
			try
			{
				Process.Start(target, param);
			}
			catch (Exception e)
			{ context.View.OnError(e.Message); }
		}

		internal override ICommandEditor GetEditor() => new AppCommandEditor();
	}

	public class CmdAlternateApp : ParamBasedCommand
	{
		public CmdAlternateApp() : base(new[] { Param.ParamTypes.String, Param.ParamTypes.String, Param.ParamTypes.String, Param.ParamTypes.String })
		{ }

		internal override void Execute(ExecutionContext context)
		{
			string target = ParamList[0].ValueAsString();
			CmdOpenApp.ProcessOutputDirective(ref target, context);
			string ID = ParamList[1].ValueAsString();
			int state = 0; // 1 if THIS item is alternate
			try
			{
				RegistryKey alternation = Registry.CurrentUser.CreateSubKey(@"Software\SAW\Alternation");
				state = (int)alternation.GetValue(ID, 0);
				alternation.SetValue(ID, 1 - state); // store other state for next time
			}
			catch (Exception e)
			{
				context.View.OnError(Strings.Item("Script_Fail_Alternation").Replace("%0", e.Message));
			}
			Debug.Assert(state == 0 || state == 1);
			string param = ParamList[2 + state].ValueAsString();
			try
			{
				Process.Start(target, param);
			}
			catch (Exception e)
			{ context.View.OnError(e.Message); }
		}

		internal override ICommandEditor GetEditor() => new AppCommandEditor();
	}

	public class CmdSwitchToApp : ParamBasedCommand
	{
		public CmdSwitchToApp() : base(new[] { Param.ParamTypes.String })
		{ }

		internal override ICommandEditor GetEditor() => new AppCommandEditor();

		internal override void Execute(ExecutionContext context)
		{
			string application = ParamList[0].ValueAsString().ToLower();
			CmdOpenApp.ProcessOutputDirective(ref application, context);
			// it looks like this should work, without all the P/Invoke stuff.  But it doesn't seem to
			// (it doesn't access some processes.  And the SetForegroundWindow seems to do nothing on the rest
			//foreach (var process in Process.GetProcesses())
			//{
			//	try
			//	{
			//		string filename = process.MainModule.FileName;
			//		Debug.WriteLine("Checking: " + filename);
			//		if (filename.ToLower().Contains(application))
			//		{
			//			SetForegroundWindow(process.MainWindowHandle);
			//			Debug.WriteLine("found");
			//			return;
			//		}
			//	}
			//	catch  // there will be errors for processes we cannot access
			//	{ }

			//}
			//Debug.WriteLine("Process not found for " + application);

			int processID = Desktop.GetProcessID(application);
			string errorMessage;
			IntPtr hwnd = Desktop.FindWindowForProcess(new IntPtr(processID), out errorMessage);
			if (!hwnd.IsZero())
				Windows.SetForegroundWindow(hwnd);
			else if (!string.IsNullOrEmpty(errorMessage))
				context.View.OnError(errorMessage);
		}

		/// <summary>Returns the file name (not path) of all detectable running processes</summary>
		public static List<string> GetRunningProcesses()
		{
			List<string> running = new List<string>();
			Desktop.IterateRunningProcesses((pe32, name) =>
			{
				running.Add(System.IO.Path.GetFileName(name));
				return true;
			});
			return running;
		}
	}

	public class CmdCloseApp : Command
	{
		internal override void Execute(ExecutionContext context)
		{
			context.View.SendKeys("'<ALT><F4>'");
		}
	}

	#endregion

	#region Desktop files
	public class CmdOpenDesktop : ParamBasedCommand
	{
		public CmdOpenDesktop() : base(new[] { Param.ParamTypes.String })
		{
		}

		internal override void Execute(ExecutionContext context)
		{
			try
			{
				string file = ParamList[0].ValueAsString();
				CmdOpenApp.ProcessOutputDirective(ref file, context);
				Desktop desktop = Desktop.LoadFrom(file);
				desktop.OpenApplications();
			}
			catch (Exception e)
			{
				context.View.OnError(Strings.Item("Script_Fail_Desktop").Replace("%0", e.Message));
			}
		}

		internal override ICommandEditor GetEditor()
		{
			return new DesktopCommandEditor();
		}

	}
	public class CmdSaveDesktop : ParamBasedCommand
	{
		public CmdSaveDesktop() : base(new[] { Param.ParamTypes.String })
		{
		}

		internal override void Execute(ExecutionContext context)
		{
			try
			{
				string file = ParamList[0].ValueAsString();
				Desktop desktop = new Desktop();
				desktop.PopulateFromWindows();
				desktop.Save(file);
			}
			catch (Exception e)
			{
				context.View.OnError(Strings.Item("Script_Fail_Desktop").Replace("%0", e.Message));
			}
		}

		internal override ICommandEditor GetEditor()
		{
			return new DesktopCommandEditor();
		}

	}
	#endregion

	#region External window move/size

	/// <summary>"Move" command - starts moving current window.  Actual movement comes from subsequent direction commands (that would otherwise move the mouse)</summary>
	public class CmdMove : Command, IMoveCommand
	{
		// state is for while moving
		private IntPtr m_HWND;
		/// <summary>Initial position of window (ie top left)</summary>
		private Point m_StartPosition;

		private Point m_Current;

		internal override void Execute(ExecutionContext context)
		{
			m_HWND = Windows.GetForegroundWindow();
			if (m_HWND.IsZero())
				context.View.OnFail();
			else
			{
				Windows.RECT start;
				Windows.GetWindowRect(m_HWND, out start);
				m_StartPosition = start.Location;
				m_Current = m_StartPosition;
				context.View.SetMouseMove(this);
				Cursor.Position = new Point(start.X + start.Width / 2, start.Y + 5); // sort of middle of the title bar
			}
		}

		void IMoveCommand.Move(int deltaX, int deltaY)
		{
			m_Current.X += deltaX;
			m_Current.Y += deltaY;
			Windows.SetWindowPos(m_HWND, 0, m_Current.X, m_Current.Y, 0, 0, Windows.SWP_NOSIZE | Windows.SWP_NOACTIVATE);
			Cursor.Position = new Point(Cursor.Position.X + deltaX, Cursor.Position.Y + deltaY);
		}

		void IMoveCommand.End(bool ok)
		{
			if (!ok)
				Windows.SetWindowPos(m_HWND, 0, m_StartPosition.X, m_StartPosition.Y, 0, 0, Windows.SWP_NOSIZE | Windows.SWP_NOACTIVATE);
		}

	}

	/// <summary>Like CmdMove this starts moving and consumes subsequent direction commands)</summary>
	public class CmdSize : Command, IMoveCommand
	{
		// state is for while moving
		private IntPtr m_HWND;
		/// <summary>Initial position of window (ie top left)</summary>
		private Size m_Start;

		private Size m_Current;

		internal override void Execute(ExecutionContext context)
		{
			m_HWND = Windows.GetForegroundWindow();
			if (m_HWND.IsZero())
				context.View.OnFail();
			else
			{
				Windows.RECT start;
				Windows.GetWindowRect(m_HWND, out start);
				m_Start = start.Size;
				m_Current = m_Start;
				context.View.SetMouseMove(this);
				Cursor.Position = new Point(start.Right, start.Bottom); // I think SAW6 went in centre, but this better tracks the resize point of window
			}
		}

		public void Move(int deltaX, int deltaY)
		{
			m_Current.Width = Math.Max(100, m_Current.Width + deltaX);
			m_Current.Height = Math.Max(100, m_Current.Height + deltaY);
			Windows.SetWindowPos(m_HWND, 0, 0, 0, m_Current.Width, m_Current.Height, Windows.SWP_NOMOVE | Windows.SWP_NOACTIVATE);
			Cursor.Position = new Point(Cursor.Position.X + deltaX, Cursor.Position.Y + deltaY);
		}

		public void End(bool ok)
		{
			if (!ok)
				Windows.SetWindowPos(m_HWND, 0, 0, 0, m_Start.Width, m_Start.Height, Windows.SWP_NOSIZE | Windows.SWP_NOACTIVATE);
		}
	}

	public class CmdDockWindow : Command
	{
		// In SAW 6 this had a single string parameter, in English.  However, that would not translate, so this version stores the direction internally as well
		// if the internal value is set, then the parameter is ignored
		public enum Directions : byte
		{
			Unknown = 0,
			Above,
			Below,
			Left,
			Right
		}

		private Directions m_Direction = Directions.Unknown;

		public Directions Direction
		{
			get
			{
				if (m_Direction == Directions.Unknown && ParamList.Count == 1)
					m_Direction = ParseDirectionText(GetParamAsString(0));
				return m_Direction;
			}
			set
			{
				m_Direction = value;
				ParamList.Clear();
				ParamList.Add(new StringParam(Strings.Item("Script_CommandPart_" + value)));
			}
		}

		public static Directions ParseDirectionText(string text)
		{
			text = text.ToLower();
			if (text == Strings.Item("Script_CommandPart_Above") || text == "above") // In English the two options will be the same, but they won't when translated
				return Directions.Above; // this ensures that the SAW 6 scripts will load correctly in any language
			if (text == Strings.Item("Script_CommandPart_Below") || text == "below")
				return Directions.Above;
			if (text == Strings.Item("Script_CommandPart_Left") || text == "left")
				return Directions.Above;
			if (text == Strings.Item("Script_CommandPart_Right") || text == "right")
				return Directions.Above;
			return Directions.Unknown;
		}

		#region Meta

		internal override void InitialiseDefaultsForCreation()
		{
			base.InitialiseDefaultsForCreation();
			Direction = Directions.Above;
		}

		protected override void InitialiseFromParams(List<string> possibleParams, string commandUsed)
		{
			if (m_Direction == Directions.Unknown && possibleParams.Count > 0)
				Direction = ParseDirectionText(possibleParams[0]); // by assigning the property this sets both the value and the parameter
		}

		internal override ICommandEditor GetEditor() => new DockEditor();

		#endregion

		#region Data

		protected override void Read(DataReader reader)
		{
			base.Read(reader);
			m_Direction = (Directions)reader.ReadByte();
		}

		public override void Write(DataWriter writer)
		{
			base.Write(writer);
			writer.Write((byte)m_Direction);
		}

		public override bool IdenticalTo(Command other)
		{
			if (!base.IdenticalTo(other)) return false;
			return Direction == (other as CmdDockWindow).Direction;
		}

		public override Command Clone()
		{
			var create = (CmdDockWindow)base.Clone();
			create.Direction = Direction;
			return create;
		}

		#endregion

		internal override void Execute(ExecutionContext context)
		{
			Rectangle sawBounds = context.View.FindForm().Bounds;
			Rectangle screen = Screen.FromPoint(sawBounds.Centre()).Bounds; // screen containing our window
			Rectangle target = screen;
			switch (Direction)
			{
				case Directions.Above:
					target.Height = sawBounds.Top - target.Y;
					break;
				case Directions.Below:
					target.Y = sawBounds.Bottom;
					break;
				case Directions.Left:
					target.Width = sawBounds.Left - target.X;
					break;
				case Directions.Right:
					target.X = sawBounds.Right;
					break;
				default:
					context.View.OnError("Unknown Dock window direction: " + m_Direction);
					return;
			}
			IntPtr window = Windows.GetForegroundWindow();
			if (window.Equals(context.View.FindForm().Handle))
				context.View.OnFail();
			else
				Windows.SetWindowPos(window, 0, target.X, target.Y, target.Width, target.Height, Windows.SWP_NOACTIVATE);
		}

	}

	/// <summary>"movewindow" command which sets a window to the given coordinates.
	/// Didn't appear in SAW6 "add command" screen</summary>
	public class CmdMoveTo : ParamBasedCommand
	{
		public CmdMoveTo() : base(new[] { Param.ParamTypes.String, Param.ParamTypes.Integer, Param.ParamTypes.Integer })
		{
		}

		internal override void Execute(ExecutionContext context)
		{
			string application = ParamList[0].ValueAsString().ToLower();
			CmdOpenApp.ProcessOutputDirective(ref application, context);

			int processID = Desktop.GetProcessID(application);
			string errorMessage;
			IntPtr hwnd = Desktop.FindWindowForProcess(new IntPtr(processID), out errorMessage);
			if (!hwnd.IsZero())
				Windows.SetWindowPos(hwnd, 0, GetParamAsInt(1), GetParamAsInt(2), 0, 0, Windows.SWP_NOSIZE | Windows.SWP_NOZORDER | Windows.SWP_ASYNCWINDOWPOS);
			else if (!string.IsNullOrEmpty(errorMessage))
				context.View.OnError(errorMessage);
		}

		internal override ICommandEditor GetEditor()
		{
			return new MoveWindowEditor();
		}
	}


	#endregion

}
