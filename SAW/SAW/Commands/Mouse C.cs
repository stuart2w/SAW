using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SAW.CommandEditors;

namespace SAW.Commands
{
	#region Mouse out
	public class CmdMouseOut : CommandWithIntID
	{

		public enum Actions : short
		{
			// these codes from SAW6 which had them in pairs (they are string resource IDs)
			// there was always MOUSE_XYZ which was the command name and MOUSEOUT_XYZ which was the command to send to windows
			// but it was the MOUSEOUT ID stored in the data, so I have kept those IDs, but without the MOUSEOUT. 
			CLICK = 20559,
			DBLCLICK = 20563,
			RTCLICK = 20561,
			DBLRTCLICK = 20565,
			DRAGON = 20542,
			DRAGOFF = 20544,
			SHCLICK = 20571,
			SHDBLCLICK = 20567,
			CTRLCLICK = 20595,
			CTRLDBLCLICK = 20591,
			//			SHDBLRTCLICK = 20569,
			SHRTCLICK = 20573, // these 2 weren't actually in SAW6, although the resources were
			CTRLRTCLICK = 20597
			//			CTRLDBLRTCLICK = 20593,
			// if adding here, update GetPossibleIntIDs, because it has a manual list in custom order
		}

		private Actions m_Action;

		#region Data
		public override void Read(ArchiveReader ar)
		{
			base.BaseRead(ar);
			m_Action = (Actions)ar.ReadUInt16();
			Debug.WriteLine("Mouse out: " + m_Action);
		}
		public override void Write(ArchiveWriter ar)
		{
			base.BaseWrite(ar);
			ar.Write((short)m_Action);
		}

		#endregion

		#region Meta

		protected override int[] GetPossibleIntIDs()
		{
			// manual list to get the order we want
			Actions[] ordered = {Actions.CLICK, Actions.DBLCLICK,  Actions.RTCLICK, Actions.DBLRTCLICK, Actions.DRAGON, Actions.DRAGOFF,
			 Actions.SHCLICK, Actions.SHDBLCLICK, Actions.SHRTCLICK, Actions.CTRLCLICK, Actions.CTRLDBLCLICK, Actions.CTRLRTCLICK};
			return (from Actions a in ordered select (int)a).ToArray();
		}

		public override int SingleParam
		{
			get { return (int)m_Action; }
			set { m_Action = (Actions)value; }
		}

		private string SubCode
		{
			get { return "MOUSE_" + m_Action; }
		}

		public override string GetCommandName()
		{
			return Strings.Item("Script_Command_" + SubCode);
		}

		public override string GetDescription()
		{
			return Strings.Item("Script_Desc_" + SubCode);
		}


		#endregion

		public override void Execute(ExecutionContext context)
		{
			switch (m_Action)
			{
				case Actions.CLICK:
					KeySend.Instance.AddMouseEvent(Windows.MOUSEEVENTF.LEFTDOWN);
					KeySend.Instance.AddMouseEvent(Windows.MOUSEEVENTF.LEFTUP);
					break;
				case Actions.DBLCLICK:
					KeySend.Instance.AddMouseEvent(Windows.MOUSEEVENTF.LEFTDOWN);
					KeySend.Instance.AddMouseEvent(Windows.MOUSEEVENTF.LEFTUP);
					KeySend.Instance.AddMouseEvent(Windows.MOUSEEVENTF.LEFTDOWN);
					KeySend.Instance.AddMouseEvent(Windows.MOUSEEVENTF.LEFTUP);
					break;
				case Actions.RTCLICK:
					KeySend.Instance.AddMouseEvent(Windows.MOUSEEVENTF.RIGHTDOWN);
					KeySend.Instance.AddMouseEvent(Windows.MOUSEEVENTF.RIGHTUP);
					break;
				case Actions.DBLRTCLICK:
					KeySend.Instance.AddMouseEvent(Windows.MOUSEEVENTF.RIGHTDOWN);
					KeySend.Instance.AddMouseEvent(Windows.MOUSEEVENTF.RIGHTUP);
					KeySend.Instance.AddMouseEvent(Windows.MOUSEEVENTF.RIGHTDOWN);
					KeySend.Instance.AddMouseEvent(Windows.MOUSEEVENTF.RIGHTUP);
					break;
				case Actions.DRAGON:
					KeySend.Instance.AddMouseEvent(Windows.MOUSEEVENTF.LEFTDOWN);
					break;
				case Actions.DRAGOFF:
					KeySend.Instance.AddMouseEvent(Windows.MOUSEEVENTF.LEFTUP);
					break;
				case Actions.SHCLICK:
					KeySend.Instance.AddKeyEvent( Keys.LShiftKey, true);
					KeySend.Instance.AddMouseEvent(Windows.MOUSEEVENTF.LEFTDOWN);
					KeySend.Instance.AddMouseEvent(Windows.MOUSEEVENTF.LEFTUP);
					KeySend.Instance.AddKeyEvent(Keys.LShiftKey, false);
					break;
				case Actions.SHDBLCLICK:
					KeySend.Instance.AddKeyEvent(Keys.LShiftKey, true);
					KeySend.Instance.AddMouseEvent(Windows.MOUSEEVENTF.LEFTDOWN);
					KeySend.Instance.AddMouseEvent(Windows.MOUSEEVENTF.LEFTUP);
					KeySend.Instance.AddMouseEvent(Windows.MOUSEEVENTF.LEFTDOWN);
					KeySend.Instance.AddMouseEvent(Windows.MOUSEEVENTF.LEFTUP);
					KeySend.Instance.AddKeyEvent(Keys.LShiftKey, false);
					break;
				case Actions.CTRLCLICK:
					KeySend.Instance.AddKeyEvent(Keys.LControlKey, true);
					KeySend.Instance.AddMouseEvent(Windows.MOUSEEVENTF.LEFTDOWN);
					KeySend.Instance.AddMouseEvent(Windows.MOUSEEVENTF.LEFTUP);
					KeySend.Instance.AddKeyEvent(Keys.LControlKey, false);
					break;
				case Actions.CTRLDBLCLICK:
					KeySend.Instance.AddKeyEvent(Keys.LControlKey, true);
					KeySend.Instance.AddMouseEvent(Windows.MOUSEEVENTF.LEFTDOWN);
					KeySend.Instance.AddMouseEvent(Windows.MOUSEEVENTF.LEFTUP);
					KeySend.Instance.AddMouseEvent(Windows.MOUSEEVENTF.LEFTDOWN);
					KeySend.Instance.AddMouseEvent(Windows.MOUSEEVENTF.LEFTUP);
					KeySend.Instance.AddKeyEvent(Keys.LControlKey, true);
					break;
				case Actions.SHRTCLICK:
					KeySend.Instance.AddKeyEvent(Keys.LShiftKey, true);
					KeySend.Instance.AddMouseEvent(Windows.MOUSEEVENTF.RIGHTDOWN);
					KeySend.Instance.AddMouseEvent(Windows.MOUSEEVENTF.RIGHTUP);
					KeySend.Instance.AddKeyEvent(Keys.LShiftKey, false);
					break;
				case Actions.CTRLRTCLICK:
					KeySend.Instance.AddKeyEvent(Keys.LControlKey, true);
					KeySend.Instance.AddMouseEvent(Windows.MOUSEEVENTF.RIGHTDOWN);
					KeySend.Instance.AddMouseEvent(Windows.MOUSEEVENTF.RIGHTUP);
					KeySend.Instance.AddKeyEvent(Keys.LControlKey, false);
					break;
				default:
					context.View.OnError("Unknown MouseOut code: " + m_Action);
					return;
			}
			KeySend.Instance.SendQueuedOutput();
		}

	}
	#endregion

	#region Move mouse
	/// <summary>Implemented by any command which can receive the mouse commands</summary>
	public interface IMoveCommand
	{
		/// <summary>Moves by the given amount</summary>
		void Move(int deltaX, int deltaY);
		/// <summary>Called movement ends on OK or cancel, losing the capture of mouse move commands</summary>
		void End(bool ok);
	}

	public class CmdMouseMove : CommandWithIntID, IMoveCommand, IContinuousCommand
	{// this has both Move and Continuous command features.
		// movement goes through IMoveCommand, as it can be redirected to keyboard instead
		// but also command can be continuous effectively executing on the .Iterate

		private short X;
		private short Y;

		public override int SingleParam
		{
			get { return X + 1 + ((Y + 1) << 16); }
			set
			{
				X = (short)((value & 0xffff) - 1);
				Y = (short)((value >> 16) - 1);
			}
		}

		#region Meta data
		protected override int[] GetPossibleIntIDs()
		{
			List<int> possible = new List<int>();
			for (int x = -1; x <= 1; x++)
				for (int y = -1; y <= 1; y++)
					if (x != 0 || y != 0) // don't add 0,0
						possible.Add(x + 1 + ((y + 1) << 16));
			return possible.ToArray();
		}

		/// <summary>Returns a code for the X/Y combination used to look up command IDs</summary>
		private string SubCode
		{
			get
			{
				string code;
				if (Y > 0)
					code = "Down_";
				else if (Y < 0)
					code = "Up_";
				else
					code = "";
				if (X < 0)
					code += "Left";
				else if (X > 0)
					code += "Right";
				else
					code = code.Trim('_');
				return code;
			}
		}

		public override string GetCommandName()
		{
			return Strings.Item("Script_Command_Mouse_" + SubCode);
		}

		public override string GetDescription()
		{
			return Strings.Item("Script_Desc_MOUSE_" + SubCode.ToUpper());
		}
		#endregion

		#region Data
		public override void Write(ArchiveWriter ar)
		{
			base.BaseWrite(ar);
			ar.Write(X);
			ar.Write(Y);
		}

		public override void Read(ArchiveReader ar)
		{
			base.BaseRead(ar);
			X = ar.ReadInt16();
			Y = ar.ReadInt16();
		}
		#endregion

		public override void Execute(ExecutionContext context)
		{
			context.View.DoMoveCommand(X, Y);
			if (Globals.Root.CurrentConfig.ReadBoolean(Config.Mouse_Continuous))
			{
				m_ContinuousContext = context.View;
				context.View.StartContinuous(this);
			}
		}

		#region IMoveCommand
		public void Move(int deltaX, int deltaY)
		{
			Cursor.Position = new Point(Cursor.Position.X + deltaX, Cursor.Position.Y + deltaY);
		}

		public void End(bool ok)
		{
			// no need to do anything.  Can't cancel mouse movement
		}
		#endregion

		#region Continuous

		private RunView m_ContinuousContext;
		public void Iterate()
		{
			m_ContinuousContext.DoMoveCommand(X, Y);
		}

		public bool Trigger(bool isRepeat)
		{
			m_ContinuousContext?.StopContinuous(this);
			return false;
		}

		public void Stop()
		{
			m_ContinuousContext = null;
		}
		#endregion
	}

	public class CmdMouseMoveTo : ParamBasedCommand
	{
		public CmdMouseMoveTo() : base(new[] { Param.ParamTypes.Integer, Param.ParamTypes.Integer })
		{
		}

		public override ICommandEditor GetEditor()
		{
			return new MoveToEditor();
		}

		public override void Execute(ExecutionContext context)
		{
			Cursor.Position = new Point(GetParamAsInt(0), GetParamAsInt(1));
		}

	}

	#endregion

	#region Mouse steps and continuous
	public class CmdSingle : Command
	{
		public override void Execute(ExecutionContext context)
		{
			context.View.MouseStepType = AppliedConfig.MouseSteps.Single;
		}
	}
	public class CmdCoarse : Command
	{
		public override void Execute(ExecutionContext context)
		{
			context.View.MouseStepType = AppliedConfig.MouseSteps.Large;
		}
	}
	public class CmdFine : Command
	{
		public override void Execute(ExecutionContext context)
		{
			context.View.MouseStepType = AppliedConfig.MouseSteps.Small;
		}
	}
	public class CmdNormal : Command
	{
		public override void Execute(ExecutionContext context)
		{
			context.View.MouseStepType = AppliedConfig.MouseSteps.Medium;
		}
	}

	public class CmdContinuous : Command
	{
		public override void Execute(ExecutionContext context)
		{
			Config.UserUser.Write(Config.Mouse_Continuous, true);
		}
	}

	public class CmdStep : Command
	{
		public override void Execute(ExecutionContext context)
		{
			Config.UserUser.Write(Config.Mouse_Continuous, false);
		}
	}

	public class CmdAdjustMouse : CommandWithIntID
	{
		public short m_wStepType;
		public short m_wAdjustAmount;

		#region Meta

		private static readonly int[] DEFAULTADJUSTMENTS = { 0, 1, 4, 10 };
		protected override int[] GetPossibleIntIDs()
		{
			List<int> list = new List<int>();
			for (int stepType = 1; stepType <= 3; stepType++) // "single" is not adjusted
			{
				int adjust = DEFAULTADJUSTMENTS[stepType];
				list.Add(stepType + (adjust << 16));
				list.Add(stepType + (-adjust << 16));
			}
			return list.ToArray();
		}

		public override int SingleParam
		{
			get { return m_wStepType + (m_wAdjustAmount << 16); }
			set
			{
				m_wStepType = (short)(value & 0xffff);
				m_wAdjustAmount = (short)(value >> 16);
			}
		}

		private string SubCode
		{
			get
			{
				string code = m_wAdjustAmount < 0 ? "Dec" : "Inc";
				code += ((AppliedConfig.MouseSteps)m_wStepType).ToString();
				return code;
			}
		}

		public override string GetCommandName()
		{
			return Strings.Item("Script_Command_Mouse_" + SubCode);
		}

		public override string GetDescription()
		{
			return Strings.Item("Script_Desc_MOUSE_" + SubCode.ToUpper());
		}

		#endregion

		#region Data

		public override void Read(ArchiveReader ar)
		{
			base.BaseRead(ar);
			m_wStepType = ar.ReadInt16();
			m_wAdjustAmount = ar.ReadInt16();
		}
		public override void Write(ArchiveWriter ar)
		{
			base.BaseWrite(ar);
			ar.Write(m_wStepType);
			ar.Write(m_wAdjustAmount);
		}

		#endregion

		public override void Execute(ExecutionContext context)
		{
			AppliedConfig.MouseSteps type = (AppliedConfig.MouseSteps)m_wStepType;
			int newValue = Globals.Root.CurrentConfig.MouseStep(type);
			newValue += m_wAdjustAmount;
			Config.UserUser.Write("Mouse_Step" + type, newValue);
		}

	}

	#endregion

}
