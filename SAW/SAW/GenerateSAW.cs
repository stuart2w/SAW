using SAW.Commands;

namespace SAW
{
	public class GenerateSAW
	{
		public static void FillDefaultScripts(Doc d)
		{
			d._itemDefault.m_pVisitScript.CommandList.Add(new CmdHighlightItem());
			d._itemDefault.m_pVisitScript.CommandList.Add(new CmdSoundClick());
			d._itemDefault.m_pVisitScript.CommandList.Add(new CmdOutText(CmdOutText.Action.Say, CmdOutText.Source.SpeechText));
			d._itemDefault.m_pSelectScript.CommandList.Add(new CmdNormalItem());
			d._itemDefault.m_pSelectScript.CommandList.Add(new CmdSoundClick());
			d._itemDefault.m_pSelectScript.CommandList.Add(new CmdOutText(CmdOutText.Action.Say, CmdOutText.Source.SpeechText));
			d._itemDefault.m_pSelectScript.CommandList.Add(new CmdOutText(CmdOutText.Action.Send, CmdOutText.Source.OutputText));
			d._itemDefault.m_pNextScript.CommandList.Add(new CmdNormalItem());
			d._itemDefault.m_pNextScript.Visit.VisitType = Script.VisitTarget.VisitTypes.Next;

			d._itemEscapeDefault.m_pVisitScript.CommandList.Add(new CmdHighlightItem());
			d._itemEscapeDefault.m_pVisitScript.CommandList.Add(new CmdSoundClick());
			d._itemEscapeDefault.m_pVisitScript.CommandList.Add(new CmdOutText(CmdOutText.Action.Say, CmdOutText.Source.SpeechText));
			d._itemEscapeDefault.m_pSelectScript.CommandList.Add(new CmdNormalItem());
			d._itemEscapeDefault.m_pSelectScript.CommandList.Add(new CmdSoundClick());
			d._itemEscapeDefault.m_pSelectScript.Visit.VisitType = Script.VisitTarget.VisitTypes.Up;
			d._itemEscapeDefault.m_pNextScript.CommandList.Add(new CmdNormalItem());
			d._itemEscapeDefault.m_pNextScript.Visit.VisitType = Script.VisitTarget.VisitTypes.Next;

			d._itemGroupDefault.m_pVisitScript.CommandList.Add(new CmdHighlightItem());
			d._itemGroupDefault.m_pVisitScript.CommandList.Add(new CmdSoundClick());
			d._itemGroupDefault.m_pVisitScript.CommandList.Add(new CmdOutText(CmdOutText.Action.Say, CmdOutText.Source.SpeechText));
			d._itemGroupDefault.m_pSelectScript.CommandList.Add(new CmdNormalItem());
			d._itemGroupDefault.m_pSelectScript.CommandList.Add(new CmdSoundClick());
			d._itemGroupDefault.m_pSelectScript.Visit.VisitType = Script.VisitTarget.VisitTypes.Down;
			d._itemGroupDefault.m_pNextScript.CommandList.Add(new CmdNormalItem());
			d._itemGroupDefault.m_pNextScript.Visit.VisitType = Script.VisitTarget.VisitTypes.Next;
		}
	}
}