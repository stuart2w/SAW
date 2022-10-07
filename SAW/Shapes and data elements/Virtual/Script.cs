using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace SAW.Shapes
{
	/// <summary>Represents one of the individual scripts on a scriptable item (can visit/select/next/...)
	/// These do not derive from Datum and are just entirely cloned by the containing object when needed for transaction control</summary>
	/// <remarks>The script is stored as a series of object in CommandList.  The text form of the script is not stored and is regenerated as needed.</remarks>
	public class Script : IArchivable
	{
		/// <summary>A record of where the selection should move next - contains a type and an ID which is used if Type == Item</summary>
		public struct VisitTarget : IArchivable // was called VisitScript in C++ code
		{
			public enum VisitTypes
			{ None = 0, Me, Next, Previous, Up, Down, First, Last, Item }

			/// <summary>What to visit - usually target is assigned relative to the current item, but if VisitType=Item then a specific numbered item can be assigned</summary>
			public VisitTypes VisitType;

			/// <summary>Stores the ID of the item to visit if the visit type is Item otherwise undefined</summary>
			public int ItemID;

			internal void Read(ArchiveReader ar)
			{
				VisitType = (VisitTypes)ar.ReadInt32();
				ItemID = ar.ReadInt32();
			}

			void IArchivable.Read(ArchiveReader ar) => Read(ar);

			internal void Write(ArchiveWriter ar)
			{
				ar.Write((int)VisitType);
				ar.Write(ItemID);
			}

			void IArchivable.Write(ArchiveWriter ar) => Write(ar);

			/// <summary>Copy constructor</summary>
			public VisitTarget(VisitTarget other)
			{
				ItemID = other.ItemID;
				VisitType = other.VisitType;
			}

			public string GetDescription()
			{ // translation items are SAW_Visit_ plus the enum value
				if (VisitType == VisitTypes.Item)
					return Strings.Item("SAW_Visit_Item") + "=" + ItemID;
				else
					return Strings.Item("SAW_Visit_" + VisitType);
			}

			public override string ToString() => GetDescription();

		}

		/// <summary>Creates empty script object.  Visit must be assigned, and CommandList begins empty. </summary>
		public Script()
		{ }

		/// <summary>Creates from the given text script, raising an exception if there were errors parsing.
		/// Multiple commands must be separated by new lines (\r\n)</summary>
		public  Script(string script, VisitTarget.VisitTypes visit = VisitTarget.VisitTypes.None)
		{
			string error = ParseFromScript(script, false);
			Visit = new VisitTarget() { VisitType = visit };
			if (error != null)
				throw new Exception(error);
		}

		public static implicit operator Script(string s) => new Script(s);

		/// <summary>Creates the script from a list of command objects</summary>
		public Script(IEnumerable<Command> commands, VisitTarget.VisitTypes visit = VisitTarget.VisitTypes.None)
		{
			Visit = new VisitTarget() { VisitType = visit };
			CommandList.AddRange(commands);
		}

		#region Properties
		// Unlike C++ code this does not store the combined text.  It is generated when needed

		/// <summary>Where the selection should move to when the script is activated</summary>
		public VisitTarget Visit;
		/// <summary>parsed CCommandObj's, one per script textline.
		/// Deferred and DeviceUp are not separated in SAW7.  They are all in this list</summary>
		public List<Command> CommandList = new List<Command>();

		/// <summary>If true default script is run first.  Unlike SAW6 where they were only run if this script is empty</summary>
		public bool RunDefault = true;

		#endregion

		#region Data

		internal void Read(ArchiveReader ar)
		{
			string script = ar.ReadStringL();
			Visit.Read(ar);
			ar.ReadList(CommandList);
			ar.ReadList(CommandList);// deferred items in SAW 6 are in a separate list
									 // then the script is read in order to preserve comments
			string error = ParseFromScript(script, true);
			if (error != null)
				Globals.NonFatalOperationalError(Strings.Item("Script_Error_LoadSAW6") + error + " (" + script.Replace("\n", "\\n").Replace("\r", "\\r") + ")");
			RunDefault = !CommandList.Any();
		}

		internal void Write(ArchiveWriter ar)
		{
			ar.WriteStringL(GenerateScript(true), true);
			Visit.Write(ar);
			if (RunDefault && CommandList.Any())
				Globals.NonFatalOperationalError("[SAW_FILE_SAW6_CannotMixedScript]");
			// in SAW 6 there were separate lists
			// non-SAW6 commands are filtered.  They should generally give an Globals.NonFatalOperationalError during the script generation (above)
			ar.WriteFiltered(CommandList, x => x.ExecutionTime == Command.ExecutionTimes.Immediate && x.CommandListEntry.SAW6);
			ar.WriteFiltered(CommandList, x => x.ExecutionTime == Command.ExecutionTimes.Deferred && x.CommandListEntry.SAW6);  // not sure what happened to any device up???
		}

		void IArchivable.Read(ArchiveReader ar) => Read(ar);
		void IArchivable.Write(ArchiveWriter ar) => Write(ar);

		/// <summary>Reads from Splash format data file.  This object doesn't inherit from Datum, so it is necessary to call this directly</summary>
		internal void Read(DataReader reader)
		{
			Visit.VisitType = (VisitTarget.VisitTypes)reader.ReadByte();
			Visit.ItemID = reader.ReadInt32();
			RunDefault = reader.ReadBoolean();
			int commands = reader.ReadInt32();
			CommandList.Clear();
			for (int i = 0; i < commands; i++)
				CommandList.Add(Command.FromReader(reader));
		}

		/// <summary>Writes to Splash format data file.  This object doesn't inherit from Datum, so it is necessary to call this directly</summary>
		internal void Write(DataWriter writer)
		{
			writer.Write((byte)Visit.VisitType);
			writer.Write(Visit.ItemID);
			writer.Write(RunDefault);
			writer.Write(CommandList.Count);
			foreach (Command c in CommandList)
				c.Write(writer);
		}

		public bool IdenticalTo(Script other)
		{
			if (Visit.VisitType != other.Visit.VisitType || Visit.ItemID != other.Visit.ItemID)
				return false;
			if (CommandList.Count != other.CommandList.Count)
				return false;
			if (RunDefault != other.RunDefault)
				return false;
			for (int i = 0; i < CommandList.Count; i++)
				if (!CommandList[i].IdenticalTo(other.CommandList[i]))
					return false;
			return true;
		}

		public Script Clone()
		{
			var clone = new Script();
			clone.Visit.ItemID = Visit.ItemID;
			clone.Visit.VisitType = Visit.VisitType;
			clone.RunDefault = RunDefault;
			foreach (Command c in CommandList)
				clone.CommandList.Add(c.Clone());
			return clone;
		}

		#endregion

		#region Conversion between command objects and text

		/// <summary>Generates a text script from the stored command objects </summary>
		public string GenerateScript(bool forSAW6 = false)
		{
			StringBuilder s = new StringBuilder();
			foreach (Command c in CommandList)
			{
				s.Append(c.GetScriptWithComment(forSAW6));
				s.Append("\r\n");
			}
			return s.ToString();
		}

		/// <summary>Parses the given script and stores in the command list if OK; if not it returns an error message (null on success)</summary>
		/// <remarks>Multiple commands must be separated by new lines (\r\n)</remarks>
		/// <returns>Error message or Null on success</returns>
		public string ParseFromScript(string script, bool fromSAW6)
		{
			try
			{
				List<Command> newList = new List<Command>(); // don't update real list unless the entire parse succeeds
				string[] lines = script.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
				for (int i = 0; i < lines.Length; i++)
				{
					// not using foreach as I want to be able to write to the line variable
					string line = lines[i];
					if (fromSAW6 && line.StartsWith("click o"))
						line = "set " + line; // have changed click on/off to set click on/off to remove ambiguity with click  (mouse command)
					string comment = null;
					int commentIndex = line.IndexOf(';');
					if (commentIndex >= 0)
					{
						comment = line.Substring(commentIndex + 1);
						line = line.Substring(0, commentIndex).Trim();
					}
					if (string.IsNullOrWhiteSpace(line) && string.IsNullOrWhiteSpace(comment))
						continue;

					var command = string.IsNullOrWhiteSpace(line) ? new NullCommand() : Command.FromScript(line);
					command.Comment = comment;
					newList.Add(command);
					if (!command.CommandListEntry.SAW7 && fromSAW6)
						Globals.NonFatalOperationalError(Strings.Item("Script_Obsolete", command.GetCommandName()));
				}
				CommandList = newList;
				return null;
			}
			catch (UserException ue)
			{ return ue.Message; }
			catch (Exception e)
			{ return Strings.Item("Script_Error_Exception").Replace("%0", e.Message); }
		}

		#endregion

		public override string ToString() => GenerateScript(false);

		/// <summary>Does the output for the "output as text" command.  This should be given the description which is prefixed on the beginning.  This allows an optional call to this which will be ignored if it is null </summary>
		internal void WriteExportText(IndentStringBuilder output, string description)
		{
			if (Visit.VisitType == VisitTarget.VisitTypes.None && !CommandList.Any())
				return;
			output.Append(description).Append(" = ");
			if (RunDefault)
				output.Append("Run default script + ");
			output.AppendEncoded(GenerateScript());
			if (Visit.VisitType != VisitTarget.VisitTypes.None)
				output.Append(", Visit=").Append(Visit.GetDescription());
			output.AppendLine();
		}

		/// <summary>True if the script contains nothing, but DOES run default scripts.  ie the user has not changed anything here</summary>
		public bool IsEmpty => RunDefault && !CommandList.Any() && Visit.VisitType == VisitTarget.VisitTypes.None;

	}
}