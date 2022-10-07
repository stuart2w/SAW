using System;
using System.Collections.Generic;
using SAW.CommandEditors;
using SAW.Functions;

namespace SAW.Commands
{
	/// <summary>Used during startup script to specify the word prediction item and settings</summary>
	public class CmdWordListSet : ParamBasedCommand
	{
		public CmdWordListSet() : base(new[] { Param.ParamTypes.String, Param.ParamTypes.Integer })
		{
		}

		internal override ICommandEditor GetEditor()
		{
			return new BladeEditor();
		}

		internal override void Execute(ExecutionContext context)
		{
			int itemID = GetParamAsInt(1);
			context.View.SetWordList(itemID);
			string[] settings = GetParamAsString(0).Split('|');
			if (settings[0].ToLower() != "blade")
				context.View.OnError("Only 'Blade' prediction engines supported");

		}

		protected override void InitialiseFromParams(List<string> possibleParams, string commandUsed)
		{
			base.InitialiseFromParams(possibleParams, commandUsed);
			CmdBladeSettings.VerifyTextParam(GetParamAsString(0));
		}

	}

	public class CmdBladeSettings : ParamBasedCommand
	{
		public CmdBladeSettings() : base(new[] { Param.ParamTypes.String })
		{
		}

		internal override ICommandEditor GetEditor()
		{
			return new BladeEditor();
		}

		internal override void Execute(ExecutionContext context)
		{
			context.View.BladeSettings(GetParamAsString(0));
		}

		protected override void InitialiseFromParams(List<string> possibleParams, string commandUsed)
		{
			base.InitialiseFromParams(possibleParams, commandUsed);
			VerifyTextParam(GetParamAsString(0));
		}

		/// <summary>Used when the user edits via text - verifies that the settings parameter is valid</summary>
		internal static void VerifyTextParam(string param)
		{
			foreach (string field in param.Split(','))
			{
				if (string.IsNullOrEmpty(field)) // in particular needed if entire settings is ""
					continue;
				string[] halves = field.Split('=');
				if (halves.Length != 2)
					throw new UserException("[Script_Error_BladeSettingsWrong]");
			}
		}
	}


	public class CmdWordListSelect : Command
	{

		internal override void Execute(ExecutionContext context)
		{
			context.View.SelectPrediction(context.TargetItem.Element.LabelText);
		}

	}

	public class CmdWordListScroll : Command
	{
		public enum Directions
		{ // numbers are for compatibility with SAW6 which uses SB_xxx windows scroll bar codes
			Top = 6,
			Up = 2,
			Down = 3,
			Bottom = 7
		}

		public Directions Direction;

		protected override void InitialiseFromParams(List<string> possibleParams, string commandUsed)
		{
			if (possibleParams.Count != 1)
				throw new UserException("[Script_Error_WordListScrollWrong]");
			if (possibleParams[0] == Strings.Item("Script_CommandPart_PageUp"))
				Direction = Directions.Up;
			else if (possibleParams[0] == Strings.Item("Script_CommandPart_PageDown"))
				Direction = Directions.Down;
			else if (possibleParams[0] == Strings.Item("Script_CommandPart_Top"))
				Direction = Directions.Top;
			else if (possibleParams[0] == Strings.Item("Script_CommandPart_Bottom"))
				Direction = Directions.Bottom;
			else
				throw new UserException("[Script_Error_WordListScrollWrong]");
		}

		public override string GetScriptWithParams(bool forSAW6)
		{
			return GetCommandName() + " " + DirectionName;
		}

		public string DirectionName
		{
			get
			{
				switch (Direction)
				{
					case Directions.Bottom: return Strings.Item("Script_CommandPart_Bottom");
					case Directions.Down: return Strings.Item("Script_CommandPart_PageDown");
					case Directions.Up: return Strings.Item("Script_CommandPart_PageUp");
					case Directions.Top: return Strings.Item("Script_CommandPart_Top");
					default: return "?";
				}
			}
		}

		internal override void Execute(ExecutionContext context)
		{
			context.View.ScrollWordList(Direction);
		}

		#region Data
		public override void Write(ArchiveWriter ar)
		{
			base.Write(ar);
			ar.Write((Int16)Direction);
		}

		public override void Read(ArchiveReader ar)
		{
			base.Read(ar);
			Direction = (Directions)ar.ReadInt16();
		}

		public override Command Clone()
		{
			CmdWordListScroll clone = (CmdWordListScroll)base.Clone();
			clone.Direction = Direction;
			return clone;
		}

		protected override void Read(DataReader reader)
		{
			base.Read(reader);
			Direction = (Directions)reader.ReadInt16();
		}

		public override void Write(DataWriter writer)
		{
			base.Write(writer);
			writer.Write((Int16)Direction);
		}

		public override bool IdenticalTo(Command other)
		{
			if (Direction != ((CmdWordListScroll)other).Direction)
				return false;
			return base.IdenticalTo(other);
		}
		#endregion

		internal override ICommandEditor GetEditor()
		{
			return new WordListScrollEditor();
		}

	}

	public class CmdEditPredictionWords : Command
	{
		internal override void Execute(ExecutionContext context)
		{
			Globals.Root.PerformAction(Verb.Find(Codes.ViewPredictionWords));
		}
	}

	/// <summary>obsolete in SAW7;  CommandList entry marked SAW7=false</summary>
	public class CmdDDEExe : ParamBasedCommand
	{ 

		public CmdDDEExe() : base(new[] { Param.ParamTypes.String, Param.ParamTypes.String })
		{
		}

		internal override void Execute(ExecutionContext context)
		{
			Console.Beep();
		}

	}
}
