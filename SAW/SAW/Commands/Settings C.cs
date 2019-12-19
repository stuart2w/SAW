using System;
using System.Collections.Generic;
using System.Linq;
using SAW.CommandEditors;

namespace SAW.Commands
{

	#region Load/save settings

	public class CmdLoadSettings : ParamBasedCommand
	{
		public CmdLoadSettings() : base(new[] { Param.ParamTypes.String })
		{ }

		public override void Execute(ExecutionContext context)
		{
			string file = base.GetParamAsString(0);
			Globals.Root.ManualSettingsFile = file;

			try
			{
				var config = Config.FromFile(file, Config.Levels.User);
				config.EnsureUserResources();
				Config.UserUser = config;
			}
			catch (Exception ex)
			{
				Utilities.LogSubError(ex);
				context.View.OnFail(ex.Message);
			}
			Globals.OnSettingsChanged();
		}

		public override ICommandEditor GetEditor()
		{
			return new LoadSettingsEditor();
		}

	}

	public class CmdSaveSettings : Command
	{
		public override void Execute(ExecutionContext context)
		{
			if (string.IsNullOrEmpty(Globals.Root.ManualSettingsFile))
				context.View.OnFail("Cannot save settings: settings have not been loaded from an external file");
			else
				Globals.Root.SaveUserConfigs();
		}

	}

	#endregion

	#region Output toggles
	public class CmdOutTextOnOff : CommandWithIntID
	{
		// similar to ones in CmdOutText, but not identical.  That didn't have both Say and Play (well it sort of did!)
		public enum Action : ushort
		{
			Send = 20617,
			Say = 20618,
			Play = 20619 //, 
						 //Print = 20620
		}

		private Action m_Action;

		#region ID param

		protected override int[] GetPossibleIntIDs()
		{
			return (from Action action in Enum.GetValues(typeof(Action)) select (int)action).ToArray();
		}

		public override int SingleParam
		{
			get { return (int)m_Action; }
			set { m_Action = (Action)value; }
		}

		// must override legacy writers, as base class would write 32 bits
		public override void Read(ArchiveReader ar)
		{
			base.BaseRead(ar);
			m_Action = (Action)ar.ReadInt16();
		}

		public override void Write(ArchiveWriter ar)
		{
			base.BaseWrite(ar);
			ar.Write((short)m_Action);
		}

		public override string GetCommandName()
		{
			return Strings.Item("Script_Command_Output_" + m_Action);
		}

		public override string GetDescription()
		{
			return Strings.Item("Script_Desc_OUTPUT_TO_" + m_Action.ToString().ToUpper());
		}

		#endregion

		#region Other meta stuff

		public override ICommandEditor GetEditor()
		{
			return new OnOffEditor();
		}

		public override void InitialiseDefaultsForCreation()
		{
			base.InitialiseDefaultsForCreation();
			m_ParamList.Add(new BoolParam(true));
		}

		protected override void InitialiseFromParams(List<string> possibleParams, string commandUsed)
		{
			if (possibleParams.Count < 0)
				throw new UserException(Strings.Item("Script_Error_TooFewParameters", commandUsed));
			m_ParamList.Add(new BoolParam(possibleParams[0]));
		}

		#endregion

		private string SettingsField
		{
			get
			{
				switch (m_Action)
				{
					case Action.Send: return Config.Output_Send;
					case Action.Say: return Config.Output_Speech;
					case Action.Play: return Config.Output_Sound;
					default: throw new ArgumentOutOfRangeException();
				}
			}
		}

		public override void Execute(ExecutionContext context)
		{
			context.View.ConfigToEdit.Write(SettingsField, GetParamAsBool(0));
		}

	}

	public class CmdPromptOnOff : ParamBasedCommand
	{
		public CmdPromptOnOff() : base(new[] { Param.ParamTypes.Bool })
		{
		}

		public override void Execute(ExecutionContext context)
		{
			context.View.ConfigToEdit.Write(Config.SAW_Prompts, GetParamAsBool(0));
		}

		public override ICommandEditor GetEditor()
		{
			return new OnOffEditor();
		}

	}

	public class CmdClickSoundOnOff : ParamBasedCommand
	{ // command is "set click" here, but was "click" in saw

		public CmdClickSoundOnOff() : base(new[] { Param.ParamTypes.Bool })
		{
		}

		public override void Execute(ExecutionContext context)
		{
			context.View.ConfigToEdit.Write(Config.Sound_Click, GetParamAsBool(0));
		}

		public override ICommandEditor GetEditor()
		{
			return new OnOffEditor();
		}

		public override string GetScriptWithParams(bool forSAW6)
		{ // script was different in SAW6
			return "click " + m_ParamList[0].ValueAsString();
		}

	}
	#endregion

	#region Scan adjust
	/// <summary>Base class for all the scan time adjusters.  They all have pairs of inc/dec command which need a load of identical meta-stuff</summary>
	public class _AdjustScan : Command
	{
		/// <summary>The partial name of this command, eg ScanTime</summary>
		protected readonly string m_Code;
		protected readonly Switches.Engine.Timings m_Timing;
		protected short m_Step;

		protected _AdjustScan(string code, Switches.Engine.Timings timing)
		{
			m_Code = code;
			m_Timing = timing;
		}

		protected string StepCode
		{ get { return m_Step < 0 ? "Dec" : "Inc"; } }

		#region Meta

		public override string GetCommandName()
		{
			return Strings.Item("Script_CommandPart_" + StepCode) + " " + Strings.Item("Script_CommandPart_" + m_Code);
		}

		public override string GetDescription()
		{
			return Strings.Item("Script_Desc_" + StepCode.ToUpper() + "_" + m_Code.ToUpper());
		}

		internal override void CompleteCommandListEntry(CommandList.Entry entry)
		{
			entry.PossibleCommandsLower = new[] { Strings.Item("Script_CommandPart_Dec" ) + " " + Strings.Item("Script_CommandPart_" + m_Code),
				Strings.Item("Script_CommandPart_Inc" ) + " " + Strings.Item("Script_CommandPart_" + m_Code)};
			entry.PossibleCommandsLower[0] = entry.PossibleCommandsLower[0].ToLower(); // just in case!
			entry.PossibleCommandsLower[1] = entry.PossibleCommandsLower[1].ToLower();
			entry.CustomData = new[] { -1, +1 };
		}

		public override void InitialiseFromCommandUsed(string commandUsed)
		{
			int[] parms = (int[])CommandListEntry.CustomData;
			int index = Array.IndexOf(CommandListEntry.PossibleCommandsLower, commandUsed);
			if (index < 0)
				throw new Exception("Failed to identify: " + commandUsed);
			m_Step = (short)parms[index];
		}

		#endregion

		#region Data

		public override void Read(ArchiveReader ar)
		{
			base.Read(ar);
			m_Step = ar.ReadInt16();
		}

		protected override void Read(DataReader reader)
		{
			base.Read(reader);
			m_Step = reader.ReadInt16();
		}

		public override void Write(ArchiveWriter ar)
		{
			base.Write(ar);
			ar.Write(m_Step);
		}

		public override void Write(DataWriter writer)
		{
			base.Write(writer);
			writer.Write(m_Step);
		}

		public override Command Clone()
		{
			_AdjustScan result = (_AdjustScan)base.Clone();
			result.m_Step = m_Step;
			return result;
		}

		#endregion

		public override void Execute(ExecutionContext context)
		{
			string field = Switches.Engine.TimingConfigField(m_Timing);
			int value = Globals.Root.CurrentConfig.ReadInteger(field, context.ScanEngine.DefaultTiming(m_Timing));
			value += 100 * m_Step; // steps are actually 0.1s
			value = Math.Max(0, Math.Min(10000, value)); // limit to 0 to 10 seconds
			context.View.ConfigToEdit.Write(field, value);
		}

	}

	public class CmdAdjustScanTime : _AdjustScan
	{
		public CmdAdjustScanTime() : base("ScanTime", Switches.Engine.Timings.ScanTime)
		{
		}
	}

	public class CmdAdjustRestart : _AdjustScan
	{
		public CmdAdjustRestart() : base("RestartTime", Switches.Engine.Timings.CriticalReverse)
		{
		}
	}

	public class CmdAdjustInputAcc : _AdjustScan
	{
		public CmdAdjustInputAcc() : base("InputAccept", Switches.Engine.Timings.AcceptanceTime)
		{ }
	}

	public class CmdAdjustPostAcc : _AdjustScan
	{
		public CmdAdjustPostAcc() : base("PostAccept", Switches.Engine.Timings.PostActivation)
		{ }
	}

	public class CmdAdjustRepeatDelay : _AdjustScan
	{
		public CmdAdjustRepeatDelay() : base("RepeatDelay", Switches.Engine.Timings.FirstRepeat)
		{ }
	}

	public class CmdAdjustRepeatTime : _AdjustScan
	{
		public CmdAdjustRepeatTime() : base("RepeatTime", Switches.Engine.Timings.SubsequentRepeat)
		{ }

	}

	#endregion

}
