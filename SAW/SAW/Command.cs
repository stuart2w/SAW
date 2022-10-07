using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using SAW.CommandEditors;
using SAW.Commands;
using System.Linq;
using SAW.Shapes;

namespace SAW
{
	/// <summary>Was CommandObj in SAW6.  Base class for all script commands.  Most information about them, including editing info is obtained by instantiating one of these.
	/// The list of possible commands is in CommandList.  These are usually constructed via CommandList.Entry.CreateInstance</summary>
	public abstract class Command : IArchivable
	{
		/// <summary>The code used for this in command list and saving in Splash format.  Set by constructor using data from CommandList</summary>
		public readonly string Code;

		protected Command()
		{
			Code = CommandList.TypeLookup[GetType()].Code;
		}

		#region Parsing

		/// <summary>Any trailing comment on the line, or null.  An empty string would append the ';' to the line when made into a script, whereas null doesn't</summary>
		public string Comment;

		/// <summary>Returns the text version of the command itself, without any params or comment, in local language</summary>
		public virtual string GetCommandName()
		{ // if there is only one command we use it - only works if there are no params of course!
			if (CommandListEntry.PossibleCommandsLower.Length != 1)
				throw new Exception("GetCommandName not implemented for " + Code);
			return CommandListEntry.PossibleCommandsLower[0];
		}

		/// <summary>Returns the text version of the script in local language, excluding the comment, but including parameters</summary>
		public virtual string GetScriptWithParams(bool forSAW6)
		{
			string name = GetCommandName();
			if (ParamList.Count == 0)
				return name;
			StringBuilder output = new StringBuilder(name);
			foreach (var p in ParamList)
			{
				output.Append(' ');
				if ((p as StringParam)?.Quoted ?? false)
					output.Append('"');
				output.Append(p.ValueAsString());
				if ((p as StringParam)?.Quoted ?? false)
					output.Append('"');
			}
			return output.ToString();
		}

		/// <summary>Gets the full script with params AND comment</summary>
		public string GetScriptWithComment(bool forSAW6 = false)
		{
			string script = GetScriptWithParams(forSAW6);
			if (Comment == null)
				return script;
			return script + " ;" + Comment;
		}

		/// <summary>Generates the command object from a (1 line) script.  Comments should already have been removed.
		/// Throws UserException for reportable errors</summary>
		/// <param name="script">The full line without comments</param>
		/// <param name="withoutParams">If true InitialiseFromParams is not called, and no params should be included.  This will initialise a command matching the script (one command may get some parameters from the actual script used), but without any further info</param>
		public static Command FromScript(string script, bool withoutParams = false)
		{
			Debug.Assert(!script.Contains(";"));
			var tokens = Tokenise(script);
			string simplified = string.Join(" ", tokens).ToLower();
			foreach (var pair in CommandList.CommandTextLookup) // Dictionary pair of command text and the record
			{
				if (simplified.StartsWith(pair.Key) && (simplified.Length == pair.Key.Length || simplified[pair.Key.Length] == ' '))
				{ // the ( || ) is needed to check that the script "say speech text" doesn't match the script "s", for example
					Command created = pair.Value.CreateInstance();
					created.InitialiseFromCommandUsed(pair.Key);
					if (!withoutParams)
					{
						int tokensUsed = pair.Key.Count(c => c == ' ') + 1;
						created.InitialiseFromParams(tokens.Skip(tokensUsed).ToList(), pair.Key);
					}
					return created;
				}
			}
			throw new UserException(Strings.Item("Script_Error_UnknownCommand").Replace("%0", script));

			//// adding a space to both the given script and the reference one is needed so that "SE" isn't considered to start with "S", for example
			//string lower = script.ToLower().Trim() + " ";
			//foreach (var pair in CommandList.CommandTextLookup) // Dictionary pair of command text and the record
			//{
			//	if (lower.StartsWith(pair.Key + " "))
			//	{
			//		string remainder = script.Substring(pair.Key.Length);
			//		Command created = pair.Value.CreateInstance();
			//		created.InitialiseFromCommandUsed(pair.Key);
			//		if (!withoutParams)
			//			created.InitialiseFromParams(SplitParams(remainder), pair.Key);
			//		return created;
			//	}
			//}
			//throw new UserException(Strings.Item("Script_Error_UnknownCommand").Replace("%0", script));
		}

		/// <summary>Splits the source into "words" divided by spaces - but allows quoting to group them</summary>
		private static List<string> Tokenise(string source)
		{ // it is assumed scripts don't contain tabs or new lines.  New lines definitely not permitted (as each script command is defined as one line from a multi-line text box)
		  // I don't think there can be tabs lurking?
			List<string> output = new List<string>();
			if (string.IsNullOrEmpty(source))
				return output;
			char terminator = ' ';
			StringBuilder buffer = new StringBuilder();
			foreach (char c in source)
			{
				if (c == terminator)
				{// ignore multiple terminators (this is an expected condition as there will be a space following a closing quote typically)
				 // and multiple spaces are counted as one between tokens
				 // however something in quotes must always be stored when the end quote is encountered, even if the contained string was 0-length
					if (buffer.Length > 0 || c == '"')
						output.Add(buffer.ToString());
					buffer.Clear();
					terminator = ' '; // revert to space as the default terminator
				}
				else if ((c == '"' || c == '\'') && buffer.Length == 0)
				{
					terminator = c; // will read until the next quote now
				}
				else
					buffer.Append(c);
			}
			if (buffer.Length > 0)
				output.Add(buffer.ToString());
			return output;
		}

		/// <summary>Called by FromScript when this is being constructed from the text script to do any customisation based on the actual command (the bit that identified the script).  InitialiseFromParams will ALSO be called.</summary>
		/// <param name="commandUsed">The first part of the line which was used to identify the command (for command objects which support multiple items)</param>
		internal virtual void InitialiseFromCommandUsed(string commandUsed)
		{
		}

		/// <summary>Called by FromScript after InitialiseFromCommandUsed when this is being constructed from the text script.</summary>
		/// <param name="possibleParams">The remainder of the line, divided into words</param>
		/// <param name="commandUsed">The first part of the script, used to identify the command.  Included just for error reporting</param>
		protected virtual void InitialiseFromParams(List<string> possibleParams, string commandUsed)
		{
			if (possibleParams.Count > 0)
				throw new UserException(Strings.Item("Script_Error_ParamsNotExpected").Replace("%0", commandUsed));
		}

		/// <summary>Used when a new script is added to fill in any default params</summary>
		internal virtual void InitialiseDefaultsForCreation()
		{ }

		#endregion

		#region Editing Info
		/// <summary>The CommandList meta-data for this</summary>
		public CommandList.Entry CommandListEntry => CommandList.Entries[Code];

		/// <summary>This is called once when CommandList is being populated.  The object must fill in PossibleCommands, and can also use CustomData if desired</summary>
		internal virtual void CompleteCommandListEntry(CommandList.Entry entry)
		{
			entry.PossibleCommandsLower = new[] { Strings.Item("Script_Command_" + Code).ToLower() };
		}

		/// <summary>Returns the description of this command when adding it</summary>
		public virtual string GetDescription() => Strings.Item("Script_Desc_" + Code.ToUpper());

		/// <summary>Returns the control which edits custom properties of this command, if any.  Returns null if none is needed (typically because there are no parameters)</summary>
		internal virtual ICommandEditor GetEditor() => null;

		#endregion

		#region Parameters

		/// <summary>List of CParam objects.  However some classes write other values individually</summary>
		public List<Param> ParamList { get; private set; } = new List<Param>();

		/// <summary>Reads the param, optionally creating it if missing</summary>
		public short GetParamAsInt(int index, bool create = false)
		{
			if (create)
				EnsureParams(index + 1);
			else if (ParamList.Count < index + 1)
				throw new UserException(Strings.Item("Script_Error_TooFewParameters"));
			return ParamList[index].ValueAsInt();
		}

		/// <summary>Reads the param, optionally creating it if missing</summary>
		public bool GetParamAsBool(int index, bool create = false)
		{
			if (create)
				EnsureParams(index + 1);
			else if (ParamList.Count < index + 1)
				throw new UserException(Strings.Item("Script_Error_TooFewParameters"));
			return ParamList[index].ValueAsBool();
		}

		/// <summary>Reads the param, optionally creating it if missing</summary>
		public float GetParamAsFloat(int index, bool create = false)
		{
			if (create)
				EnsureParams(index + 1);
			else if (ParamList.Count < index + 1)
				throw new UserException(Strings.Item("Script_Error_TooFewParameters"));
			return ParamList[index].ValueAsFloat();
		}

		/// <summary>Reads the param, optionally creating it if missing</summary>
		public string GetParamAsString(int index, bool create = false)
		{
			if (create)
				EnsureParams(index + 1);
			else if (ParamList.Count < index + 1)
				throw new UserException(Strings.Item("Script_Error_TooFewParameters"));
			return ParamList[index].ValueAsString();
		}

		/// <summary>Ensure there are at least this many params in the list</summary>
		internal virtual void EnsureParams(int count)
		{
			while (ParamList.Count < count)
				ParamList.Add(new IntegerParam());
		}

		#endregion

		#region Execution
		/// <summary>Information passed to the Execute methods </summary>
		internal class ExecutionContext
		{
			public readonly Scriptable TargetItem;
			public readonly RunView View;
			public readonly Page Page;
			public readonly Document Document;
			public readonly Switches.Engine ScanEngine;
			public readonly Scriptable.ScriptTypes ScriptType;
			/// <summary>Set to true to terminate running any further actions.  open selection set does this, for example</summary>
			public bool Terminate;

			public ExecutionContext(Scriptable target, RunView view, Page page, Document doc, Switches.Engine scanEngine, Scriptable.ScriptTypes scriptType)
			{
				TargetItem = target;
				View = view;
				Page = page;
				Document = doc;
				ScanEngine = scanEngine;
				ScriptType = scriptType;
			}

		}

		internal abstract void Execute(ExecutionContext context);

		// TODO: all visit commands should be MouseUp (but not sure what these commands are?!)

		/// <summary>These were implemented by separate command lists in SAW5, but there's no need for that</summary>
		public enum ExecutionTimes
		{
			Immediate,
			MouseUp,
			Deferred
		}

		/// <summary>These were implemented by separate command lists in SAW5, but there's no need for that</summary>
		internal virtual ExecutionTimes ExecutionTime => ExecutionTimes.Immediate;

		#endregion

		#region Data
		public virtual void Read(ArchiveReader ar)
		{
			ParamList = ar.ReadList<Param>();
		}
		public virtual void Write(ArchiveWriter ar)
		{
			ar.WriteList(ParamList);
		}

		/// <summary>Returns a deep copy of the command.  The base class implements copying the standard parameter list; derived classes storing custom data must override</summary>
		public virtual Command Clone()
		{
			Command clone = CommandListEntry.CreateInstance();
			clone.Comment = Comment;
			foreach (var p in ParamList)
				clone.ParamList.Add(p.Clone());
			return clone;
		}

		/// <summary>Reads a command from the Splash data format reader</summary>
		internal static Command FromReader(DataReader reader)
		{
			string code = reader.ReadString();
			if (!CommandList.Entries.ContainsKey(code))
				throw new InvalidDataException("Script command '" + code + "' not recognised");
			Command create = CommandList.Entries[code].CreateInstance();
			create.Comment = reader.ReadOptionalString();
			create.Read(reader);
			return create;
		}

		/// <summary>Only used within FromReader which reads the script type and creates the object</summary>
		protected virtual void Read(DataReader reader)
		{
			int numberParams = reader.ReadByte();
			for (int i = 0; i < numberParams; i++)
			{
				ParamList.Add(Param.FromReader(reader));
			}
		}

		/// <summary>Writes the command to Splash format writer.  Must call through to base class first as this stores the command type</summary>
		public virtual void Write(DataWriter writer)
		{
			writer.Write(Code);
			writer.WriteOptionalString(Comment);// note that we must distinguish null and "", which is done by this method
			writer.WriteByte(ParamList.Count);
			foreach (Param p in ParamList)
				p.Write(writer);
		}

		public virtual bool IdenticalTo(Command other)
		{
			if (Code != other.Code || Comment != other.Comment)
				return false;
			if (ParamList.Count != other.ParamList.Count)
				return false;
			for (int i = 0; i < ParamList.Count; i++)
				if (!ParamList[i].Equals(other.ParamList[i]))
					return false;
			return true;
		}

		#endregion

	}


	/// <summary>Implemented by any command that can take all the following scan input and process it until finished.
	/// The command should store the view when it starts</summary>
	public interface IContinuousCommand
	{
		/// <summary>Called when the scans moves to the next item</summary>
		void Iterate();

		/// <summary>Called when the scan selects.  isRepeat = true if it is a repetition from a switch being held down.
		/// Returns whether it wants to remain continuous</summary>
		bool Trigger(bool isRepeat);

		/// <summary>Called if the command is terminated.  Must cope with spurious, repeat calls.  Will always be called, even if command requests to stop</summary>
		void Stop();

	}

	/// <summary>Implements a command object where there are several script commands visible to the user which use the same object, distinguished internally by another integer (or enum)
	/// In some cases it's actually 2 values, which can be packed into a 32-bit value to make this work.</summary>
	public abstract class CommandWithIntID : Command
	{// Derived class must implement GetPossibleIntIDs, GetDescription, GetCommand
	 // And perhaps the old SAW Read and Write commands - but this base has a default which stores the low word of SingleParam first, followed by the high word

		public override string GetCommandName()
		{ // classes derived from this MUST implement this
			throw new InvalidOperationException("GetCommandName was not overridden in " + GetType());
		}

		/// <summary>Derived class must return all the possible integer IDs used for the different commands visible to the user</summary>
		protected abstract int[] GetPossibleIntIDs();

		/// <summary>The ID which distinguishes between the user commands sharing one object</summary>
		public abstract int SingleParam { get; set; }

		internal override void CompleteCommandListEntry(CommandList.Entry entry)
		{
			int actualParam = SingleParam; // just in case it's meaningful
			var list = new List<string>();
			var listParams = new List<int>();
			foreach (int ID in GetPossibleIntIDs())
			{ // update param in this object, and get the script for each ID
				SingleParam = ID;
				list.Add(GetCommandName().ToLower());
				listParams.Add(ID);
			}
			entry.PossibleCommandsLower = list.ToArray();
			entry.CustomData = listParams.ToArray();
			SingleParam = actualParam;
		}

		internal override void InitialiseFromCommandUsed(string commandUsed)
		{
			int[] parms = (int[])CommandListEntry.CustomData;
			int index = Array.IndexOf(CommandListEntry.PossibleCommandsLower, commandUsed);
			if (index < 0)
				throw new Exception("Failed to identify: " + commandUsed);
			SingleParam = parms[index];
		}

		#region Data
		public override Command Clone()
		{
			CommandWithIntID clone = (CommandWithIntID)base.Clone();
			clone.SingleParam = SingleParam;
			return clone;
		}

		protected override void Read(DataReader reader)
		{
			base.Read(reader);
			SingleParam = reader.ReadInt32();
		}

		public override void Write(DataWriter writer)
		{
			base.Write(writer);
			writer.Write(SingleParam);
		}

		/// <summary>Default implementation assumes low word of SingleParam stored first, then high word</summary>
		public override void Read(ArchiveReader ar)
		{
			base.Read(ar);
			short low = ar.ReadInt16();
			short high = ar.ReadInt16();
			SingleParam = low + (high << 16);
		}
		/// <summary>Default implementation assumes low word of SingleParam stored first, then high word</summary>
		public override void Write(ArchiveWriter ar)
		{
			base.Write(ar);
			ar.Write((ushort)(SingleParam & 0xffff));
			ar.Write((ushort)(SingleParam >> 16));
		}

		/// <summary>Just calls through to Command.Read(ar) - used by derived classes that DON'T want this default implementation of reading the params</summary>
		protected void BaseRead(ArchiveReader ar)
		{
			base.Read(ar);
		}

		protected void BaseWrite(ArchiveWriter ar)
		{
			base.Write(ar);
		}

		public override bool IdenticalTo(Command other)
		{
			if ((other as CommandWithIntID)?.SingleParam != SingleParam)
				return false;
			return base.IdenticalTo(other);
		}

		#endregion

		public override string GetDescription()
		{
			Debug.Fail("Command derived from CommandWithIntID did not override GetDescription");
			return "?";
		}

	}

	/// <summary>Base class for commands which use params.  They are not REQUIRED to inherit from this and some don't.
	/// This maintains the list of expected parameter types and auto-creates default of the correct type as needed</summary>
	public abstract class ParamBasedCommand : Command
	{
		// it's slightly inefficient to store this in each instance, but it's really not going to make any difference
		protected readonly Param.ParamTypes[] ExpectedParams;
		/// <summary>Only relevant if this has a single string parameter; if true then what appear to be multiple parameters are accepted and concatenated into the one string</summary>
		protected bool TreatParametersAsSingleString;

		protected ParamBasedCommand(Param.ParamTypes[] expectedParams)
		{
			ExpectedParams = expectedParams;
		}

		protected override void InitialiseFromParams(List<string> possibleParams, string commandUsed)
		{
			if (possibleParams.Count < ExpectedParams.Length)
				throw new UserException(Strings.Item("Script_Error_TooFewParameters").Replace("%0", GetCommandName()));
			if (possibleParams.Count > ExpectedParams.Length)
			{
				if (ExpectedParams.Length == 1 && TreatParametersAsSingleString)
				{ // combined back into a single  string
					string combined = string.Join(" ", possibleParams.ToArray());
					possibleParams = new List<string>(new[] { combined });
				}
				else
					throw new UserException(Strings.Item("Script_Error_TooManyParameters").Replace("%0", GetCommandName()));
			}
			for (int i = 0; i < ExpectedParams.Length; i++)
			{
				switch (ExpectedParams[i])
				{
					case Param.ParamTypes.Bool:
						ParamList.Add(new BoolParam(possibleParams[i]));
						break;
					case Param.ParamTypes.Float:
						ParamList.Add(new FloatParam(possibleParams[i]));
						break;
					case Param.ParamTypes.Integer:
						ParamList.Add(new IntegerParam(possibleParams[i]));
						break;
					case Param.ParamTypes.String:
					case Param.ParamTypes.UnquotedString:
						ParamList.Add(new StringParam(possibleParams[i], ExpectedParams[i] != Param.ParamTypes.UnquotedString));
						break;
					default:
						throw new InvalidOperationException("Unexpected param type: " + ExpectedParams[i]);
				}
			}
		}

		internal sealed override void InitialiseDefaultsForCreation()
		{
			EnsureParams(ExpectedParams.Length);
		}

		/// <summary>Creates a param object of the given type with its default value (false/0/"") </summary>
		internal static Param CreateParamOfType(Param.ParamTypes type)
		{
			switch (type)
			{
				case Param.ParamTypes.Bool: return new BoolParam(false);
				case Param.ParamTypes.Float: return new FloatParam(0);
				case Param.ParamTypes.Integer: return new IntegerParam(0);
				case Param.ParamTypes.String: return new StringParam("");
				case Param.ParamTypes.UnquotedString: throw new InvalidOperationException("Cannot created UnquotedString");
				default:
					throw new InvalidOperationException("Unexpected param type: " + type);
			}
		}

		internal override void EnsureParams(int count)
		{ // overrides base class to create params of correct type
			while (ParamList.Count < count)
			{
				ParamList.Add(CreateParamOfType(ExpectedParams[ParamList.Count]));
			}
		}

	}

	#region Null command - placeholder for comments
	/// <summary>This is just used as a place holder for any line containing only a comment</summary>
	internal class NullCommand : Command
	{
		internal override void CompleteCommandListEntry(CommandList.Entry entry)
		{
			entry.PossibleCommandsLower = new[] { "null" };
		}

		public override string GetCommandName() => "";  // comment is not included in this command and will be added

		internal override void Execute(ExecutionContext context)
		{}

	}

	#endregion

}