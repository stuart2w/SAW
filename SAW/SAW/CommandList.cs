using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SAW.Commands;

namespace SAW
{
	/// <summary>List of possible script commands, and the classes to instantiate for them.  Most information about the script commands is obtained from instances of the class.
	/// This list does not match the list offered to the user, as that has separate entries for different parameters for some of the commands</summary>
	public static class CommandList
	{
		/* To add a command:
		 * Add in CommandList() here;
		 * Add Script_Command_Xyz and Script_Desc_XYZ in strings
		 * Add CmdXyz class
		 * If it exists in SAW6 add to SAW6Doc.NotifyArchiveTypes
		 */
		public class Entry
		{
			/// <summary>Used as identifier, and suffix for text lookup.  This is never seen by the user</summary>
			public readonly string Code;
			/// <summary>The run time class used for instances</summary>
			public Type Class;
			/// <summary>All the possible commands (first 1-N words of script) which can be used for this script IN THE LOCAL LANGUAGE.  All stored in lower case.</summary>
			public string[] PossibleCommandsLower;
			/// <summary>Data which can be stored by the command object.</summary>
			public object CustomData;
			// This is used mainly so that CommandWithIntID can store data for each of its derived types with all the implementation in its own class

			/// <summary>True if this exists in SAW 6 </summary>
			public bool SAW6 = true;
			public bool SAW7 = true;

			/// <summary>This is the folder path each command is listed in, within frmAddCommand. Each element, separated by / is the CODE of a group, looked up in strings table as "Script_Group_" + Code
			/// Can be null to indicate the command should not be listed at all.  Can either have 1 entry per possible command, or just a single entry to use for all possible commands.
			/// One for all is the default; to change a command must change it in CompleteCommandListEntry</summary>
			public string[] AddGUIPath;

			public Entry(string code, Type instanceClass, string addGuiPath)
			{
				Code = code;
				Class = instanceClass;
				AddGUIPath = new[] { addGuiPath }; // by default the same path is used for all 
			}

			/// <summary>Creates an instance of the Command object, using the default constructor</summary>
			public Command CreateInstance()
			{
				return (Command)Activator.CreateInstance(Class);
			}

		}

		/// <summary>All of the possible commands, indexed by code</summary>
		public static readonly Dictionary<string, Entry> Entries;
		public static readonly Dictionary<Type, Entry> TypeLookup;
		/// <summary>Values are all the possible commands, values are the entries.  And entry with multiple possible commands will be listed more than once.
		/// All keys are in lower case.</summary>
		public static readonly Dictionary<string, Entry> CommandTextLookup;

		#region Generating the list
		// list is in here, add new commands here>>>
		static CommandList()
		{
			Entries = new Dictionary<string, Entry>();
			TypeLookup = new Dictionary<Type, Entry>();

			// code | class | path in frmAddCommand
			Add("NormalItem", typeof(CmdNormalItem), "SAW/Item/State");
			Add("HighlightItem", typeof(CmdHighlightItem), "SAW/Item/State");
			Add("FlashItem", typeof(CmdFlashItem), "SAW/Item/State");
			Add("ExecItem", typeof(CmdExec), "SAW/Item");
			Add("VisitDown", typeof(_CmdVisit.CmdVisitDown), "SAW/Item/Visit");
			Add("VisitFirst", typeof(_CmdVisit.CmdVisitFirst), "SAW/Item/Visit");
			Add("VisitItem", typeof(_CmdVisit.CmdVisitItem), "SAW/Item/Visit");
			Add("VisitLast", typeof(_CmdVisit.CmdVisitLast), "SAW/Item/Visit");
			Add("VisitMe", typeof(_CmdVisit.CmdVisitMe), "SAW/Item/Visit");
			Add("VisitNext", typeof(_CmdVisit.CmdVisitNext), "SAW/Item/Visit");
			Add("VisitPrevious", typeof(_CmdVisit.CmdVisitPrevious), "SAW/Item/Visit");
			Add("VisitUp", typeof(_CmdVisit.CmdVisitUp), "SAW/Item/Visit");
			Add("Keys", typeof(CmdKey), "Keyboard");
			Add("SlowKeys", typeof(CmdSlowKeys), "Keyboard");
			Add("KeyDelay", typeof(CmdKeyDelay), "Keyboard");
			Add("KeysOut", typeof(CmdKeysOut), "Test");
			Add("SoundClick", typeof(CmdSoundClick), "Sound");
			Add("SpeakClipboard", typeof(CmdSpeakClipboard), "Sound").SAW6 = false;
			Add("Beep", typeof(CmdBeep), "Sound");
			Add("OpenApp", typeof(CmdOpenApp), "Application");
			Add("ParamApp", typeof(CmdParamApp), "Application");
			Add("AlternateApp", typeof(CmdAlternateApp), "Application");
			Add("SwitchToApp", typeof(CmdSwitchToApp), "Application");
			Add("CloseApp", typeof(CmdCloseApp), "Application");
			Add("SaveDesktop", typeof(CmdSaveDesktop), "Desktop");
			Add("OpenDesktop", typeof(CmdOpenDesktop), "Desktop");
			Add("MoveWindow", typeof(CmdMove), "Window"); // order of this may be important as command "move" is subset of others
			Add("SizeWindow", typeof(CmdSize), "Window");
			Add("MoveWindowTo", typeof(CmdMoveTo), "Window");
			Add("DockWindow", typeof(CmdDockWindow), "Window");
			Add("MouseMove", typeof(CmdMouseMove), "Mouse/MouseMove");
			Add("MouseMoveTo", typeof(CmdMouseMoveTo), "Mouse/MouseMove");
			Add("GridScan", typeof(CmdGridScan), "Mouse/MouseMove");
			Add("MouseSingle", typeof(CmdSingle), "Mouse/MouseStep");
			Add("MouseSmall", typeof(CmdFine), "Mouse/MouseStep");
			Add("MouseMedium", typeof(CmdNormal), "Mouse/MouseStep");
			Add("MouseLarge", typeof(CmdCoarse), "Mouse/MouseStep");
			Add("MouseContinuous", typeof(CmdContinuous), "Mouse/MouseStep");
			Add("MouseStep", typeof(CmdStep), "Mouse/MouseStep");
			Add("AdjustMouse", typeof(CmdAdjustMouse), "Settings/MouseStepSettings");
			Add("MouseOut", typeof(CmdMouseOut), "Mouse/MouseAction");
			Add("Wait", typeof(CmdWait), "SAW");
			Add("DisplayPromptText", typeof(CmdDisplayPromptText), "SAW");
			Add("GotoPage", typeof(CmdGotoPage), "SAW").SAW6 = false;
			Add("YoctoRelay", typeof(CmdYocto), "SAW").SAW6 = false;
			Add("OutText", typeof(CmdOutText), "SAW/Output");
			Add("SAWMoveToEdge", typeof(CmdSawMoveToEdge), "SAW/SAWWindow");
			Add("ShowSAW", typeof(CmdShowSAW), "SAW/SAWWindow");
			Add("SAWMove", typeof(CmdSawMove), "SAW/SAWWindow");
			Add("SAWSize", typeof(CmdSawSize), "SAW/SAWWindow");
			Add("CloseSAW", typeof(CmdCloseSAW), "SAW/SAWWindow");
			Add("PopupShow", typeof(CmdPopupShow), "SAW/Popup"); // is also hide
			Add("PopupItem", typeof(CmdPopupItem), "SAW/Popup");
			Add("PopupLast", typeof(CmdPopupLast), "SAW/Popup");
			Add("PopupSave", typeof(CmdPopupSave), "SAW/Popup");
			Add("PopupRestore", typeof(CmdPopupRestore), "SAW/Popup");
			Add("OpenSelectionSet", typeof(CmdOpenSelectionSet), "SAW/SelectionSets");
			Add("PreviousSet", typeof(CmdPreviousSelectionSet), "SAW/SelectionSets");
			Add("Message", typeof(CmdMessage), "Edit"); // actually in "Edit" and "Scroll".  CmdMessage updates the AddGuiPath list
			Add("LoadSettings", typeof(CmdLoadSettings), "Settings");
			Add("SaveSettings", typeof(CmdSaveSettings), "Settings");
			Add("OutTextOnOff", typeof(CmdOutTextOnOff), "Settings/Output");
			Add("SetHideSAW", typeof(CmdSetHideSAW), "Settings/Output");
			Add("AdjustScanTime", typeof(CmdAdjustScanTime), "Settings/Scan");
			Add("AdjustRestartTime", typeof(CmdAdjustRestart), "Settings/Scan");
			Add("AdjustInputAcc", typeof(CmdAdjustInputAcc), "Settings/Scan");
			Add("AdjustPostAcc", typeof(CmdAdjustPostAcc), "Settings/Scan");
			Add("AdjustRepeatDelay", typeof(CmdAdjustRepeatDelay), "Settings/Scan");
			Add("AdjustRepeatTime", typeof(CmdAdjustRepeatTime), "Settings/Scan");
			Add("PromptOnOff", typeof(CmdPromptOnOff), "Settings");
			Add("ClickSoundOnOff", typeof(CmdClickSoundOnOff), "Settings");
			Add("WordlistSet", typeof(CmdWordListSet), "Prediction");
			Add("WordlistSelect", typeof(CmdWordListSelect), "Prediction");
			Add("BladeSettings", typeof(CmdBladeSettings), "Prediction");
			Add("WordlistScroll", typeof(CmdWordListScroll), "Prediction"); // note we currently use lower case "list" in most, whereas SAW 6 used upper
			Add("EditPredictionWords", typeof(CmdEditPredictionWords), "Prediction");

			// Add("", typeof(), "");
			Add("null", typeof(NullCommand), null);
			Add("DDEExe", typeof(CmdDDEExe), null).SAW7 = false; // obsolete, but we need the object so that old files load

			CommandTextLookup = new Dictionary<string, Entry>();
			foreach (var e in Entries.Values)
			{
				foreach (string command in e.PossibleCommandsLower)
					CommandTextLookup.Add(command, e);
			}
		}

		/// <summary>Adds a record to the global list, returning the object so custom values can be modified</summary>
		private static Entry Add(string code, Type instanceClass, string addGUIPath)
		{
			if (!instanceClass.IsSubclassOf(typeof(Command)))
				throw new ArgumentException(instanceClass + " is not a CommandObj");
			var entry = new Entry(code, instanceClass, addGUIPath);
			Entries.Add(code, entry);
			TypeLookup.Add(instanceClass, entry);

			Command instance = entry.CreateInstance();
			instance.CompleteCommandListEntry(entry);
			Debug.Assert(entry.PossibleCommandsLower.All(x => x.ToLower() == x));
			return entry;
		}

		#endregion

	}
}
