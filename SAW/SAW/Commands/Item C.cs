using System;
using System.Collections.Generic;
using SAW.CommandEditors;
using System.Linq;
using System.Threading;
using SAW.Shapes;

namespace SAW.Commands
{

	#region State
	/// <summary>Not from SAW6.  Base for CmdNormalItem etc.</summary>
	public abstract class _CmdItemState : Command
	{ // cannot used ParamBasedCommand since param is optional

		protected override void InitialiseFromParams(List<string> possibleParams, string commandUsed)
		{
			if (possibleParams.Count > 1)
				throw new UserException(Strings.Item("Script_Error_TooManyParameters").Replace("%0", commandUsed));
			if (possibleParams.Count == 1)
				ParamList.Add(new IntegerParam(possibleParams[0]));
		}

		/// <summary>Note: if setting to false, then the ID should also be assigned</summary>
		public bool AffectsThis
		{
			get { return ParamList.Count == 0; }
			set
			{
				if (value)
					ParamList.Clear();
				else if (ParamList.Count == 0)
					ParamList.Add(new IntegerParam(0));
			}
		}

		/// <summary>The ID of the SAW item to affect, or -1 to act on self </summary>
		public int ItemID
		{
			get
			{
				if (ParamList.Count == 0) return -1;
				return (ParamList[0] as IntegerParam)?.Value ?? -1;
			}
			set
			{
				if (ParamList.Count == 0)
					ParamList.Add(new IntegerParam(value));
				else
					(ParamList[0] as IntegerParam).Value = (Int16)value;
			}
		}

		#region Meta

		internal override ICommandEditor GetEditor() => new AnyItemStateEditor();

		#endregion

		private protected Scriptable ResolveTarget(ExecutionContext context)
		{
			if (AffectsThis)
				return context.TargetItem;
			Scriptable result = context.Page.FindScriptableByID(ItemID);
			if (result == null)
				throw new UserException(Strings.Item("Script_Error_CannotFindItem").Replace("%0", ItemID.ToString()));
			return result;
		}

	}

	/// <summary>Clears the highlight on a scriptable </summary>
	public class CmdNormalItem : _CmdItemState
	{

		public CmdNormalItem() { }

		public CmdNormalItem(int itemID)
		{ ParamList.Add(itemID); }

		internal override void Execute(ExecutionContext context)
		{
			context.View.SetItemState(ResolveTarget(context), ButtonShape.States.Normal);
		}

	}

	/// <summary>Sets the visual highlight on an item.  Does not set this as the "current" item at runtime; this only affects the appearance. </summary>
	public class CmdHighlightItem : _CmdItemState
	{

		public CmdHighlightItem() { }

		public CmdHighlightItem(int itemID)
		{ ParamList.Add(itemID); }

		internal override void Execute(ExecutionContext context)
		{
			context.View.SetItemState(ResolveTarget(context), ButtonShape.States.Highlight);
		}

	}

	/// <summary>Sets the highlight on then off again.</summary>
	public class CmdFlashItem : _CmdItemState
	{

		public CmdFlashItem() { }

		public CmdFlashItem(int itemID)
		{ ParamList.Add(itemID); }

		internal override void Execute(ExecutionContext context)
		{
			context.View.SetItemState(ResolveTarget(context), ButtonShape.States.Highlight);
			System.Threading.Tasks.Task.Run(() =>
			   {
				   Thread.Sleep(150); // was 100 in SAW
				   context.View.SetItemState(ResolveTarget(context), ButtonShape.States.Normal);
			   });
		}

	}

	#endregion

	/// <summary>Executes the Select script on the item with the given SAWID in the current document </summary>
	public class CmdExec : ParamBasedCommand
	{

		public CmdExec() : base(new[] { Param.ParamTypes.Integer }) { }

		public CmdExec(int itemID) : this() { ParamList.Add(itemID); }

		internal override ICommandEditor GetEditor() => new PickItemEditor();

		internal override ExecutionTimes ExecutionTime => ExecutionTimes.Deferred;

		internal override void Execute(ExecutionContext context)
		{
			int id = GetParamAsInt(0);
			Scriptable item = context.Page.FindScriptableByID(id);
			if (item == null)
				throw new UserException(Strings.Item("Script_Error_CannotFindItem").Replace("%0", GetParamAsString(0)));
			context.View.InvokeScript(item, Scriptable.ScriptTypes.Select, false, context.TargetItem);
		}

	}

	public class _CmdVisit : Command
	{
		// in principle the derivatives of this shouldn't exist in SAW files as they go in the DeviceUpList in a script now, which isn't serialised.
		// BUT... we have files containing them in the main list.  Therefore we need to be able to load them at least

		protected Script.VisitTarget.VisitTypes m_Target;

		public override string GetCommandName() => Strings.Item("Script_CommandPart_Visit") + " " + Strings.Item("SAW_Visit_" + m_Target).ToLower();

		internal override void CompleteCommandListEntry(CommandList.Entry entry)
		{
			entry.PossibleCommandsLower = new[] { Strings.Item("Script_CommandPart_Visit") + " " + Strings.Item("SAW_Visit_" + m_Target).ToLower() };
		}

		public override string GetDescription() => Strings.Item("Script_Desc_VISIT_" + m_Target.ToString().ToUpper());

		#region Other meta

		internal override ExecutionTimes ExecutionTime => ExecutionTimes.MouseUp;

		protected override void InitialiseFromParams(List<string> possibleParams, string commandUsed)
		{
			if (m_Target == Script.VisitTarget.VisitTypes.Item)
			{
				if (possibleParams.Count > 1)
					throw new UserException(Strings.Item("Script_Error_TooManyParameters").Replace("%0", GetCommandName()));
				if (possibleParams.Count < 1)
					throw new UserException(Strings.Item("Script_Error_TooFewParameters").Replace("%0", GetCommandName()));
				int value;
				if (!int.TryParse(possibleParams[0], out value))
					throw new UserException(Strings.Item("Script_Error_ParameterNotInt").Replace("%0", "1"));
				ParamList.Clear();
				ParamList.Add(new IntegerParam(value));
			}
			else if (possibleParams.Any())
				throw new UserException(Strings.Item("Script_Error_ParamsNotExpected").Replace("%0", GetCommandName()));
		}

		internal override ICommandEditor GetEditor()
		{
			// only for Item visit do we need an editor:
			if (m_Target == Script.VisitTarget.VisitTypes.Item)
				return new PickItemEditor();
			return base.GetEditor();
		}

		#endregion

		internal override void Execute(ExecutionContext context)
		{
			Script.VisitTarget temp = new Script.VisitTarget() { VisitType = m_Target };
			if (m_Target == Script.VisitTarget.VisitTypes.Item)
				temp.ItemID = GetParamAsInt(0);
			Scriptable item = context.View.ResolveVisitTarget(temp, context.TargetItem);
			if (item != null) // i think a  null here is not any sort of error.  Eg Down in an item with no contents is just ignored
				context.View.SelectItem(item, false); // must be false or View will enter infinite loop as the leave script calls back to this
		}

		#region Individual classes

		public class CmdVisitNext : _CmdVisit
		{
			public CmdVisitNext()
			{
				m_Target = Script.VisitTarget.VisitTypes.Next;
			}
		}

		public class CmdVisitDown : _CmdVisit
		{
			public CmdVisitDown()
			{
				m_Target = Script.VisitTarget.VisitTypes.Down;
			}
		}

		public class CmdVisitFirst : _CmdVisit
		{
			public CmdVisitFirst()
			{
				m_Target = Script.VisitTarget.VisitTypes.First;
			}
		}

		public class CmdVisitItem : _CmdVisit
		{
			public CmdVisitItem()
			{
				m_Target = Script.VisitTarget.VisitTypes.Item;
			}
		}

		public class CmdVisitLast : _CmdVisit
		{
			public CmdVisitLast()
			{
				m_Target = Script.VisitTarget.VisitTypes.Last;
			}
		}

		public class CmdVisitMe : _CmdVisit
		{
			public CmdVisitMe()
			{
				m_Target = Script.VisitTarget.VisitTypes.Me;
			}
		}

		public class CmdVisitPrevious : _CmdVisit
		{
			public CmdVisitPrevious()
			{
				m_Target = Script.VisitTarget.VisitTypes.Previous;
			}
		}

		public class CmdVisitUp : _CmdVisit
		{
			public CmdVisitUp()
			{
				m_Target = Script.VisitTarget.VisitTypes.Up;
			}
		}

		#endregion

	}


}