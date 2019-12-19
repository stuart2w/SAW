using System;
using System.Linq;
using SAW.CommandEditors;

namespace SAW.Commands
{
	public class CmdKey : ParamBasedCommand
	{

		public CmdKey() : base(new[] { Param.ParamTypes.String })
		{
			TreatParametersAsSingleString = true;
		}

		public override ICommandEditor GetEditor()
		{
			return new KeysEditor();
		}

		public override void Execute(ExecutionContext context)
		{
			context.View.SendKeys(GetParamAsString(0, true));
		}

	}

	public class CmdSlowKeys : ParamBasedCommand
	{
		public const int MAXDELAY = 1000; // if this is changed some translations will need changing
		public CmdSlowKeys() : base(new[] { Param.ParamTypes.Integer, Param.ParamTypes.Float })
		{

		}

		public override ICommandEditor GetEditor()
		{
			return new KeysEditor();
		}

		public override void Execute(ExecutionContext context)
		{ // keys are now second param, delay is first
			context.View.SendKeys(GetParamAsString(1, true), GetParamAsInt(0));
		}
	}

	public class CmdKeyDelay : ParamBasedCommand
	{

		public CmdKeyDelay() : base(new[] { Param.ParamTypes.Integer })
		{
		}

		public override ICommandEditor GetEditor()
		{
			return new KeyDelayEditor();
		}

		public override void Execute(ExecutionContext context)
		{
			context.View.KeyDelay = GetParamAsInt(0);
		}

	}


	/// <summary>Does not let user specify output string.  Instead this implements what appear to be custom commands by means of sending key presses.
	/// The key strings to send are looked up in translations (since the special key codes are themselves translateable)</summary>
	public class CmdKeysOut : CommandWithIntID
	{ // a lot of all caps here, as the definitions were copied from SAW6

		public enum Combinations : short
		{
			SELECT_PREV = 20508,
			SEL_NEXT = 20510,
			MAX = 20512,
			MIN = 20514,
			RESTORE = 20516,
			CLOSE = 20518,
			OK = 20520,
			CANCEL = 20522,
			SHOWMENU = 20524,
			SHOWCTRMENU = 20526,
			SHOWMENUBAR = 20528,
			ESC = 20530,
			PREVAREA = 20532,
			NEXTAREA = 20534,
			SELECT = 20536,
			HOME = 20538,
			ENDL = 20540,
			//SHON = 20575,
			//SHOFF = 20577,
			KEYUP = 20579,
			KEYDOWN = 20581,
			KEYLEFT = 20583,
			KEYRIGHT = 20585,
			//			KEYPGUP = 20587,
			//			KEYPGDN = 20589
		}

		public const short CURSOR_CONTINUOUS = 1;

		/// <summary>Which key combination is to be used.  Defines what command this really is</summary>
		public Combinations Combination;
		/// <summary>Only used for CURSOR_CONTINUOUS.  I don't know when/why that is used</summary>
		public short Modifier;

		public override void Execute(ExecutionContext context)
		{
			if (Modifier == CURSOR_CONTINUOUS)
				throw new NotImplementedException();
			if (Combination == Combinations.OK || Combination == Combinations.CANCEL)
			{ // these might cancel any 
				if (context.View.DoOKCancel(Combination == Combinations.OK))
					return; // this command was consumed ending a window move;  we don't actually send the OK/Cancel
			}
			context.View.SendKeys(Strings.Item("KEYSOUT_" + Combination));
		}

		#region Meta stuff
		protected override int[] GetPossibleIntIDs()
		{
			return (from Combinations value in Enum.GetValues(typeof(Combinations)) select (int)value).ToArray();
		}

		public override int SingleParam
		{
			get { return (int)Combination + (Modifier << 16); }
			set
			{
				Combination = (Combinations)(value & 0xffff);
				Modifier = (short)(value >> 16);
			}
		}

		public override string GetCommandName()
		{
			return Strings.Item("Script_Command_UsingKeys_" + Combination);
		}

		public override string GetDescription()
		{
			return Strings.Item("Script_Desc_" + Combination);
		}

		internal override void CompleteCommandListEntry(CommandList.Entry entry)
		{ // the commands need to be assigned to different folders in frmAddCommand
			base.CompleteCommandListEntry(entry);
			int[] combinations = (int[])CommandListEntry.CustomData;
			entry.AddGUIPath = new string[combinations.Count()];
			for (int index = 0; index < combinations.Length; index++)
			{
				Combinations combination = (Combinations)combinations[index];
				switch (combination)
				{
					case Combinations.OK:
					case Combinations.CANCEL:
					case Combinations.NEXTAREA:
					case Combinations.PREVAREA:
					case Combinations.SELECT:
					case Combinations.ENDL:
					case Combinations.HOME:
						entry.AddGUIPath[index] = "Dialog";
						break;
					case Combinations.KEYUP:
					case Combinations.KEYDOWN:
					case Combinations.KEYLEFT:
					case Combinations.KEYRIGHT:
					case Combinations.SHOWMENU:
					case Combinations.SHOWCTRMENU:
					case Combinations.SHOWMENUBAR:
					case Combinations.ESC:
						entry.AddGUIPath[index] = "Menu";
						break;

					case Combinations.SELECT_PREV:
					case Combinations.SEL_NEXT:
					case Combinations.MAX:
					case Combinations.MIN:
					case Combinations.RESTORE:
					case Combinations.CLOSE:
						entry.AddGUIPath[index] = "Window";
						break;

					//case Combinations.SHON:
					//case Combinations.SHOFF:
					//case Combinations.KEYPGUP:
					//case Combinations.KEYPGDN:
					default:
						entry.AddGUIPath[index] = "Unknown";
						break;
				}
			}
		}

		#endregion

	}

}