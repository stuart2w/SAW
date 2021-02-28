using System.Collections.Generic;
using System.Linq;
using SAW.CommandEditors;

namespace SAW.Commands
{
	/// <summary>Shows or hides a popup item - either the current one, or a specified one</summary>
	public class CmdPopupShow : CommandWithIntID
	{
		private bool m_Show;
		// the item ID is specified in an OPTIONAL param.  If no param is present, then this acts upon self or an ancestor (whichever is a popup)

		#region Meta
		protected override int[] GetPossibleIntIDs()
		{
			return new[] { 0, 1 };
		}

		public override int SingleParam
		{
			get { return m_Show ? 1 : 0; }
			set { m_Show = (value != 0); }
		}

		public override string GetCommandName()
		{
			return Strings.Item(m_Show ? "Script_Command_PopupShow" : "Script_Command_PopupHide");
		}

		public override string GetDescription()
		{
			return Strings.Item(m_Show ? "Script_Desc_POPUPSHOW" : "Script_Desc_POPUPHIDE");
		}

		public override ICommandEditor GetEditor()
		{
			return new PopupShowEditor();
		}

		protected override void InitialiseFromParams(List<string> possibleParams, string commandUsed)
		{
			if (possibleParams.Count > 1)
				throw new UserException(Strings.Item("Script_Error_TooManyParameters", commandUsed));
			if (possibleParams.Count == 1)
				m_ParamList.Add(new IntegerParam(possibleParams[0]));
		}

		#endregion

		#region Data
		// Has to replace the SAW6 Read/Write from base clase as that tries to write 2 short ints
		// the SAW7 one can safely write SingleParam as a 32-bit, however

		public override void Read(ArchiveReader ar)
		{
			base.BaseRead(ar);
			m_Show = (ar.ReadUInt16() != 0);
		}
		public override void Write(ArchiveWriter ar)
		{
			base.BaseWrite(ar);
			ar.Write((short)(m_Show ? 1 : 0));
		}
		#endregion

		public override void Execute(ExecutionContext context)
		{
			Scriptable popup;
			if (m_ParamList.Any())
			{
				int ID = GetParamAsInt(0);
				popup = context.Page.FindScriptableByID(ID);
			}
			else
			{
				Shape search = context.TargetItem;
				while (search != null && (!(search is Scriptable) || !(search as Scriptable).Popup))
				{
					search = (search.Parent as Shape); // will become null if parent is not a shape (ie we reached the page)
				}
				popup = search as Scriptable;
			}
			if (popup == null)
				context.View.OnError(Strings.Item("Script_Fail_CannotFindPopup").Replace("%0", m_ParamList.Any() ? m_ParamList[0].ValueAsString() : "this"));
			else
			{
				popup.Shown = m_Show;
				popup.NotifyIndirectChange(popup, ChangeAffects.RepaintNeeded, popup.RefreshBounds());
				context.View.LastPopupShown = popup;
			}
		}

	}


	public class CmdPopupItem : Command
	{

		public override ICommandEditor GetEditor() => new PopupItemEditor();

		public override void Execute(ExecutionContext context)
		{
			int ID = GetParamAsInt(0);
			Scriptable item = context.Page.FindScriptableByID(ID);
			if (item == null)
			{
				context.View.OnFail("Item " + ID + " not found for " + GetCommandName());
				return;
			}
			context.View.InvokeScript(item, Scriptable.ScriptTypes.Select, true, item); // note item is set as the context - unlike exec item
		}

		protected override void InitialiseFromParams(List<string> possibleParams, string commandUsed)
		{
			if (possibleParams.Count > 1)
				throw new UserException(Strings.Item("Script_Error_TooManyParameters", commandUsed));
			if (possibleParams.Count == 1)
				m_ParamList.Add(new IntegerParam(possibleParams[0]));
		}

	}

	/// <summary>Runs select script of last item popped up</summary>
	public class CmdPopupLast : Command
	{
		public override void Execute(ExecutionContext context)
		{
			if (context.View.LastPopupShown == null || context.View.LastPopupShown.FindPage() != context.Page)
				return;
			context.View.InvokeScript(context.View.LastPopupShown, Scriptable.ScriptTypes.Select);
		}
	}

	public class CmdPopupSave : Command
	{
		public override void Execute(ExecutionContext context)
		{
			context.View.SavedPopup = context.View.LastPopupShown;
		}
	}

	public class CmdPopupRestore : Command
	{
		public override void Execute(ExecutionContext context)
		{
			var saved = context.View.SavedPopup;
			if (saved == null)
				return;
			context.View.InvokeScript(saved, Scriptable.ScriptTypes.Select);
		}
	}

	public class CmdGotoPage : ParamBasedCommand
	{
		public CmdGotoPage() : base(new[] { Param.ParamTypes.Integer })
		{
		}

		public CmdGotoPage(int page) : this()
		{
			m_ParamList.Add(new IntegerParam(page));
		}

		public override void Execute(ExecutionContext context)
		{
			int page = base.GetParamAsInt(0);
			Globals.Root.CurrentPageIndex = page - 1; // data is 1-based
		}

		public override void EnsureParams(int count)
		{// overrides base because the param can't have the default value (0)
			if (m_ParamList.Count < 1)
				m_ParamList.Add(new IntegerParam(1));
		}

		public override ICommandEditor GetEditor() => new GotoPageEditor();

	}
}
