using System.Drawing;
using SAW.Commands;
using System.Linq;
using SAW.Shapes;

namespace SAW
{
	/// <summary>Represents document object in previous version.  Only used for old files.</summary>
	public class SAW6Doc : IArchivable
	{
		public Header m_Header = new Header();
		public Item.LegacyItem m_ItemDefault = new Item.LegacyItem();
		public Item.EscapeItem m_ItemEscapeDefault = new Item.EscapeItem();
		public Item.GroupItem m_ItemGroupDefault = new Item.GroupItem();
		public Item.LegacyItem m_ItemStartup = new Item.LegacyItem();  // holds startup script (as Select)
		public Item.LegacyItem m_itemTop = new Item.LegacyItem();       // top level item, holds all others

		public SAW6Doc()
		{
		}

		/// <summary>Creates an old SAW6 container for a new version document</summary>
		public SAW6Doc(Document newDocument)
		{
			var firstPage = newDocument.Pages.First();
			m_Header = newDocument.SAWHeader;
			m_Header.NextControlID = newDocument.Pages.First().FindHighestUsedID() + 1;
			m_Header.Version = 6200;
			m_Header.PromptID = firstPage.HelpSAWID;
			m_ItemDefault = new Item.LegacyItem(newDocument.DefaultItemScripts);
			m_ItemEscapeDefault = new Item.EscapeItem(newDocument.DefaultEscapeScripts);
			m_ItemGroupDefault = new Item.GroupItem(newDocument.DefaultGroupScripts);
			m_ItemStartup = new Item.LegacyItem(new Scriptable() { SelectScript = newDocument.StartupScript });
			m_itemTop = new Item.LegacyItem { Scripting = new Scriptable(), Main = new Item() };
			m_itemTop.Main.Image = firstPage.BackgroundImage;
			m_itemTop.Main.FillStyle.Colour = firstPage.Colour;
			m_itemTop.Main.Contents.AddRange(newDocument.Pages.First().Contents);
		}

		private void NotifyArchiveTypes(IArchiver ar)
		{
			ar.NotifyType("CScript", typeof(Script));
			//			ar.NotifyType("CObList", typeof(CObList));
			ar.NotifyType("CCmdHighlightItem", typeof(CmdHighlightItem));
			ar.NotifyType("CIntegerParam", typeof(IntegerParam));
			ar.NotifyType("CRealParam", typeof(FloatParam));
			ar.NotifyType("CBooleanParam", typeof(BoolParam));
			ar.NotifyType("CStringParam", typeof(StringParam));
			ar.NotifyType("", typeof(CmdSoundClick));
			ar.NotifyType("", typeof(CmdBeep));
			ar.NotifyType("", typeof(CmdWait));
			ar.NotifyType("", typeof(CmdNormalItem));
			ar.NotifyType("", typeof(CmdFlashItem));
			ar.NotifyType("", typeof(CmdExec));
			ar.NotifyType("", typeof(CmdOpenApp));
			ar.NotifyType("", typeof(CmdParamApp));
			ar.NotifyType("", typeof(CmdAlternateApp));
			ar.NotifyType("", typeof(CmdSwitchToApp));
			ar.NotifyType("", typeof(CmdOpenDesktop));
			ar.NotifyType("", typeof(CmdSaveDesktop));
			ar.NotifyType("", typeof(CmdOpenSelectionSet));
			ar.NotifyType("", typeof(CmdOpenItemTemplateSet));
			ar.NotifyType("", typeof(CmdPreviousSelectionSet));
			ar.NotifyType("", typeof(CmdCloseSAW));
			ar.NotifyType("", typeof(CmdSetHideSAW));
			ar.NotifyType("", typeof(CmdPopupItem));
			ar.NotifyType("", typeof(CmdPopupLast));
			ar.NotifyType("", typeof(CmdPopupSave));
			ar.NotifyType("", typeof(CmdPopupRestore));
			ar.NotifyType("", typeof(CmdDisplayPromptText));
			ar.NotifyType("", typeof(CmdSawMove));
			ar.NotifyType("", typeof(CmdSawMoveToEdge));
			ar.NotifyType("", typeof(CmdSawSize));
			ar.NotifyType("", typeof(CmdOutText));
			ar.NotifyType("", typeof(CmdShowSAW));
			ar.NotifyType("", typeof(CmdPopupShow));
			ar.NotifyType("CEscapeItem", typeof(Item.EscapeItem));
			ar.NotifyType("CGroupItem", typeof(Item.GroupItem));
			ar.NotifyType("CItem", typeof(Item.LegacyItem));
			//ar.NotifyType("", typeof(WordPredictionControl));
			//ar.NotifyType("", typeof(GridControl));
			ar.NotifyType("", typeof(CmdKey));
			ar.NotifyType("", typeof(CmdKeyDelay));
			ar.NotifyType("CCmdSlowKey", typeof(CmdSlowKeys));
			ar.NotifyType("", typeof(CmdKeysOut));
			ar.NotifyType("", typeof(CmdMouseOut));
			ar.NotifyType("", typeof(CmdMouseMove));
			ar.NotifyType("", typeof(CmdMouseMoveTo));
			ar.NotifyType("", typeof(CmdGridScan));
			ar.NotifyType("", typeof(CmdSingle));
			ar.NotifyType("", typeof(CmdCoarse));
			ar.NotifyType("", typeof(CmdFine));
			ar.NotifyType("", typeof(CmdNormal));
			ar.NotifyType("", typeof(CmdContinuous));
			ar.NotifyType("", typeof(CmdStep));
			ar.NotifyType("", typeof(CmdAdjustMouse));
			ar.NotifyType("", typeof(CmdMessage));
			//ar.NotifyType("", typeof(CmdSet)); // bogus?
			ar.NotifyType("", typeof(CmdMoveTo));
			ar.NotifyType("", typeof(CmdMove));
			ar.NotifyType("", typeof(CmdSize));
			ar.NotifyType("", typeof(CmdWordListSet));
			ar.NotifyType("", typeof(CmdWordListSelect));
			ar.NotifyType("", typeof(CmdWordListScroll));
			ar.NotifyType("", typeof(CmdBladeSettings));
			ar.NotifyType("", typeof(CmdDockWindow));
			ar.NotifyType("", typeof(CmdLoadSettings));
			ar.NotifyType("", typeof(CmdSaveSettings));
			ar.NotifyType("", typeof(CmdOutTextOnOff));
			ar.NotifyType("", typeof(CmdAdjustScanTime));
			ar.NotifyType("", typeof(CmdAdjustRestart));
			ar.NotifyType("", typeof(CmdAdjustInputAcc));
			ar.NotifyType("", typeof(CmdAdjustPostAcc));
			ar.NotifyType("", typeof(CmdAdjustRepeatDelay));
			ar.NotifyType("", typeof(CmdAdjustRepeatTime));
			ar.NotifyType("", typeof(CmdPromptOnOff));
			ar.NotifyType("", typeof(CmdDDEExe));
			ar.NotifyType("", typeof(_CmdVisit.CmdVisitDown));
			ar.NotifyType("", typeof(_CmdVisit.CmdVisitFirst));
			ar.NotifyType("", typeof(_CmdVisit.CmdVisitItem));
			ar.NotifyType("", typeof(_CmdVisit.CmdVisitLast));
			ar.NotifyType("", typeof(_CmdVisit.CmdVisitMe));
			ar.NotifyType("", typeof(_CmdVisit.CmdVisitNext));
			ar.NotifyType("", typeof(_CmdVisit.CmdVisitPrevious));
			ar.NotifyType("", typeof(_CmdVisit.CmdVisitUp));
			ar.NotifyType("", typeof(CmdEditPredictionWords));
			ar.NotifyType("CCmdClickOnOff", typeof(CmdClickSoundOnOff));

			//ar.NotifyType("", typeof());
		}

		/// <summary>Also creates a Splash document - reference is added to the archivereader</summary>
		public void Read(ArchiveReader ar)
		{
			NotifyArchiveTypes(ar);
			m_Header.Read(ar);
			ar.SAWVersion = m_Header.Version;
			ar.PageHeight = m_Header.MainWindowBounds.Height;
			var page = new Page() { ReverseRenderOrder = true, BackgroundImageMode = Page.BackgroundImageModes.FitWithin };
			page.SetSize(m_Header.MainWindowBounds.Size, 0);
			ar.IntoDocument = new Document(new[] { page }) { ReverseRenderOrder = true };

			if (m_Header.Version == 5005 || m_Header.Version > 5009)
			{ // note these items were not serialised normally - the actual serialise method was called
				m_ItemDefault.Read(ar);
				m_ItemEscapeDefault.Read(ar);
				m_ItemGroupDefault.Read(ar);
			}
			m_ItemStartup.Read(ar);
			m_itemTop.Read(ar);
			page.BackgroundImage = m_itemTop.Main.Image;
			page.Colour = m_itemTop.Main.FillStyle.Colour;
			page.HelpSAWID = m_Header.PromptID;

			foreach (Item.LegacyItem item in m_itemTop.SubItems)
			{
				ar.ReadTransaction.Edit(item.Scripting); // not needed, but the Page asserts that it is in the transaction
				page.AddNew(item.Scripting, ar.ReadTransaction);
			}

			ar.IntoDocument.DefaultEscapeScripts = m_ItemEscapeDefault.Scripting ?? new Scriptable();
			ar.IntoDocument.DefaultGroupScripts = m_ItemGroupDefault.Scripting ?? new Scriptable();
			ar.IntoDocument.DefaultItemScripts = m_ItemDefault.Scripting ?? new Scriptable();
			ar.IntoDocument.SAWHeader = m_Header;
			ar.IntoDocument.StartupScript = m_ItemStartup?.Scripting?.GetScript(Scriptable.ScriptTypes.Select) ?? new Script();

		}

		public void Write(ArchiveWriter ar)
		{ // this resets the item types of the special items as the item type is not really set in SAW7
			NotifyArchiveTypes(ar);
			ar.PageHeight = m_Header.MainWindowBounds.Height;
			m_Header.Write(ar);
			if (m_Header.Version == 5005 || m_Header.Version > 5009)
			{
				m_ItemDefault.Main.ItemType = Item.ItemTypes.IT_Default;
				m_ItemEscapeDefault.Main.ItemType = Item.ItemTypes.IT_Default;
				m_ItemGroupDefault.Main.ItemType = Item.ItemTypes.IT_Default;
				m_ItemDefault.Write(ar);
				m_ItemEscapeDefault.Write(ar);
				m_ItemGroupDefault.Write(ar);
			}
			m_ItemStartup.Main.ItemType = Item.ItemTypes.IT_StartUp;
			m_ItemStartup.Write(ar);
			m_itemTop.Main.ItemType = Item.ItemTypes.IT_TopItem;
			m_itemTop.Main.SetBounds(new RectangleF(0, 0, m_Header.MainWindowBounds.Width, m_Header.MainWindowBounds.Height));
			m_itemTop.AddContents();
			m_itemTop.Write(ar);
		}

	}
}
