using System.Windows.Forms;

namespace SAW
{
	
	internal partial class frmMain : SAW.KeyForm
	{

		//NOTE: The following procedure is required by the Windows Form Designer
		//It can be modified using the Windows Form Designer.
		//Do not modify it using the code editor.
		[System.Diagnostics.DebuggerStepThrough()]
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.Windows.Forms.ToolStripMenuItem VerbCopyToolStripMenuItem;
			System.Windows.Forms.ToolStripMenuItem VerbCutToolStripMenuItem;
			System.Windows.Forms.ToolStripMenuItem VerbPasteToolStripMenuItem;
			System.Windows.Forms.ToolStripMenuItem VerbDeleteToolStripMenuItem1;
			System.Windows.Forms.ToolStripMenuItem VerbUndoToolStripMenuItem;
			System.Windows.Forms.ToolStripMenuItem VerbRedoToolStripMenuItem;
			System.Windows.Forms.ToolStripMenuItem VerbNewDocumentToolStripMenuItem;
			System.Windows.Forms.ToolStripMenuItem MenuLoadToolStripMenuItem;
			System.Windows.Forms.ToolStripMenuItem MenuSaveToolStripMenuItem;
			System.Windows.Forms.ToolStripMenuItem MenuSaveAsToolStripMenuItem;
			System.Windows.Forms.ToolStripMenuItem VerbPrintToolStripMenuItem;
			System.Windows.Forms.ToolStripMenuItem VerbPrintPreviewToolStripMenuItem;
			System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem6;
			System.Windows.Forms.ToolStripSeparator ToolStripMenuItem1;
			System.Windows.Forms.ToolStripMenuItem MenuSelectToolStripMenuItem;
			System.Windows.Forms.ToolStripMenuItem VerbAlignLeftToolStripMenuItem;
			System.Windows.Forms.ToolStripMenuItem VerbAlignCentreToolStripMenuItem;
			System.Windows.Forms.ToolStripMenuItem VerbAlignRightToolStripMenuItem;
			System.Windows.Forms.ToolStripMenuItem VerbAlignTopToolStripMenuItem;
			System.Windows.Forms.ToolStripMenuItem VerbAlignMiddleToolStripMenuItem;
			System.Windows.Forms.ToolStripMenuItem VerbAlignBottomToolStripMenuItem;
			System.Windows.Forms.ToolStripSeparator ToolStripMenuItem7;
			System.Windows.Forms.ToolStripSeparator ToolStripSeparator3;
			System.Windows.Forms.ToolStripMenuItem ApplyToolStripMenuItem;
			System.Windows.Forms.ToolStripSeparator ToolStripMenuItem2;
			System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem5;
			System.Windows.Forms.ToolStripMenuItem VerbZoomOutToolStripMenuItem;
			System.Windows.Forms.ToolStripMenuItem VerbZoom100ToolStripMenuItem;
			System.Windows.Forms.ToolStripSeparator ToolStripMenuItem4;
			System.Windows.Forms.ToolStripSeparator ToolStripSeparator1;
			System.Windows.Forms.ToolStripMenuItem VerbClearToolStripMenuItem;
			System.Windows.Forms.ToolStripMenuItem VerbDeleteToolStripMenuItem;
			System.Windows.Forms.ToolStripMenuItem MenuToolStripMenuItem;
			System.Windows.Forms.ToolStripMenuItem MenuBeforeToolStripMenuItem;
			System.Windows.Forms.ToolStripMenuItem MenuAfterToolStripMenuItem;
			System.Windows.Forms.ToolStripSeparator ToolStripSeparator2;
			System.Windows.Forms.ToolStripMenuItem VerbEditPaperToolStripMenuItem;
			System.Windows.Forms.ToolStripMenuItem PlaceholderToolStripMenuItem;
			System.Windows.Forms.Label Label3;
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
			System.Windows.Forms.ToolStripMenuItem VerbQuitToolStripMenuItem;
			System.Windows.Forms.ToolStripMenuItem VerbSendBackToolStripMenuItem;
			System.Windows.Forms.ToolStripMenuItem VerbBringFrontToolStripMenuItem;
			System.Windows.Forms.ToolStripMenuItem VerbDuplicatePageToolStripMenuItem;
			System.Windows.Forms.Label LabelWithBlend1;
			System.Windows.Forms.ToolStripMenuItem VerbPageSizeToolStripMenuItem;
			System.Windows.Forms.ToolStripMenuItem mnuEditPaperContext;
			System.Windows.Forms.ToolStripSeparator ToolStripMenuItem9;
			System.Windows.Forms.ToolStripSeparator ToolStripMenuItem10;
			System.Windows.Forms.ToolStripSeparator ToolStripMenuItem11;
			System.Windows.Forms.ToolStripMenuItem mnuPageNext;
			this.VerbSelectNextToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.VerbSelectPreviousToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.VerbSelectAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.VerbSelectNoneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuExportPage = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuExportEMF = new System.Windows.Forms.ToolStripMenuItem();
			this.VerbExportJPEGToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.VerbExportPNGToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.VerbExportSVGToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.VerbExportPageAsDocumentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuEdit = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuVerbActive = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuVerbInactive = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuDividerMakeActive = new System.Windows.Forms.ToolStripSeparator();
			this.verbCopyScriptsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.verbCopyPresentationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.MenuFrontBackToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.VerbSendBackOneStepToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.VerbBringFrontOneStepToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuAlignment = new System.Windows.Forms.ToolStripMenuItem();
			this.VerbEqualiseWidthToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.verbLargestWidthToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.VerbEqualiseHeightToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.verbLargestHeightToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.VerbSpreadHorizontalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.VerbSpreadVerticalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.VerbTidyGridToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.VerbTidyShapeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.VerbTidyAngleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.menuModifySelectedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.verbToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.verbHideTextSelectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.verbShowGraphicSelectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.verbHideGraphicSelectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuDoubleClick = new System.Windows.Forms.ToolStripMenuItem();
			this.verbConvertToPathToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.verbCCFUpdateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuMakeMask = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuRemoveMask = new System.Windows.Forms.ToolStripMenuItem();
			this.dlgPrint = new System.Windows.Forms.PrintDialog();
			this.dlgPageSetup = new System.Windows.Forms.PageSetupDialog();
			this.pnlConstruction = new System.Windows.Forms.Panel();
			this.chkFullDP = new System.Windows.Forms.CheckBox();
			this.lblDiagnostic = new System.Windows.Forms.TextBox();
			this.lblMouseDiagnostic = new System.Windows.Forms.Label();
			this.mnuTop = new System.Windows.Forms.MenuStrip();
			this.mnuFile = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuUserUser = new System.Windows.Forms.ToolStripMenuItem();
			this.VerbStartToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuGraphicsMode = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuRecent = new System.Windows.Forms.ToolStripMenuItem();
			this.verbYoctoTestToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.verbExportAsTextToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.sAWDesktopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuSaveDesktop = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuDesktopEdit = new System.Windows.Forms.ToolStripMenuItem();
			this.sAWMenuIRMToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.verbImportIRMToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.verbExportIRMToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuUpdate = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuOptions = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuSnapOff = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuSnapGrid = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuSnapShape = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuSnapAngle = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuResizeDoc = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuZoomPage = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuZoomWidth = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuConfigUser = new System.Windows.Forms.ToolStripMenuItem();
			this.sAWMenuSettingsFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.verbSaveSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.verbLoadSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.VerbResetPalettesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.verbViewPredictionWordsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuSystemConfig = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuActivityConfig = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuActivityConfigUser = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuSAWSet = new System.Windows.Forms.ToolStripMenuItem();
			this.menuDefaultScriptsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.verbStartupScriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.verbDefaultItemScriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.verbDefaultGroupScriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.verbDefaultEscapeScriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.verbShowAllPopopsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.verbHideAllPopupsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.verbSetHelpItemToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.verbGridWizardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuPageHeading = new System.Windows.Forms.ToolStripMenuItem();
			this.VerbPreviousToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuPageMove = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuPageMoveUp = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuPageMoveDown = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuVerbSetOrigin = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuSupport = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuOpenErrorReport = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuDiagnosticDivider = new System.Windows.Forms.ToolStripSeparator();
			this.mnuDisplayDiagnostic = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuGenerateError = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuTest = new System.Windows.Forms.ToolStripMenuItem();
			this.pnlPages = new System.Windows.Forms.Panel();
			this.ctrPageList = new SAW.ctrPages();
			this.pnlPageButtons = new System.Windows.Forms.Panel();
			this.btnPageNext = new System.Windows.Forms.Button();
			this.btnPagePrevious = new System.Windows.Forms.Button();
			this.dlgPreview = new System.Windows.Forms.PrintPreviewDialog();
			this.pnlLeft = new System.Windows.Forms.Panel();
			this.pnlTools = new SAW.ButtonPanel();
			this.pnlInfo = new System.Windows.Forms.Panel();
			this.pnlSnap = new System.Windows.Forms.Panel();
			this.rdoSnapAngle = new SAW.IconRadio();
			this.rdoSnapShape = new SAW.IconRadio();
			this.rdoSnapGrid = new SAW.IconRadio();
			this.pnlToolbar = new System.Windows.Forms.FlowLayoutPanel();
			this.ttMain = new System.Windows.Forms.ToolTip(this.components);
			this.dlgFolder = new System.Windows.Forms.FolderBrowserDialog();
			this.pnlLeftBorder = new System.Windows.Forms.Panel();
			this.ctxPalette = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.mnuPaletteReset = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuPaletteHide = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuPaletteSmaller = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuPaletteLarger = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuPaletteShowSettings = new System.Windows.Forms.ToolStripSeparator();
			this.mnuPaletteEdit = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuPaletteScale = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuPalette100 = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuPalette200 = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuPalette300 = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuPaletteChoose = new System.Windows.Forms.ToolStripMenuItem();
			this.ctrPromptArea = new SAW.ctrPrompts();
			this.ctrDocuments = new SAW.DocumentTab();
			this.ctxBackground = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.VerbPasteToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuPageSizeContext = new System.Windows.Forms.ToolStripMenuItem();
			VerbCopyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			VerbCutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			VerbPasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			VerbDeleteToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
			VerbUndoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			VerbRedoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			VerbNewDocumentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			MenuLoadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			MenuSaveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			MenuSaveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			VerbPrintToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			VerbPrintPreviewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			ToolStripMenuItem6 = new System.Windows.Forms.ToolStripMenuItem();
			ToolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
			MenuSelectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			VerbAlignLeftToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			VerbAlignCentreToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			VerbAlignRightToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			VerbAlignTopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			VerbAlignMiddleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			VerbAlignBottomToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			ToolStripMenuItem7 = new System.Windows.Forms.ToolStripSeparator();
			ToolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
			ApplyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			ToolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
			ToolStripMenuItem5 = new System.Windows.Forms.ToolStripMenuItem();
			VerbZoomOutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			VerbZoom100ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			ToolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
			ToolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			VerbClearToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			VerbDeleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			MenuToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			MenuBeforeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			MenuAfterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			ToolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			VerbEditPaperToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			PlaceholderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			Label3 = new System.Windows.Forms.Label();
			VerbQuitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			VerbSendBackToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			VerbBringFrontToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			VerbDuplicatePageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			LabelWithBlend1 = new System.Windows.Forms.Label();
			VerbPageSizeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			mnuEditPaperContext = new System.Windows.Forms.ToolStripMenuItem();
			ToolStripMenuItem9 = new System.Windows.Forms.ToolStripSeparator();
			ToolStripMenuItem10 = new System.Windows.Forms.ToolStripSeparator();
			ToolStripMenuItem11 = new System.Windows.Forms.ToolStripSeparator();
			mnuPageNext = new System.Windows.Forms.ToolStripMenuItem();
			this.pnlConstruction.SuspendLayout();
			this.mnuTop.SuspendLayout();
			this.pnlPages.SuspendLayout();
			this.pnlPageButtons.SuspendLayout();
			this.pnlLeft.SuspendLayout();
			this.pnlSnap.SuspendLayout();
			this.pnlToolbar.SuspendLayout();
			this.ctxPalette.SuspendLayout();
			this.ctxBackground.SuspendLayout();
			this.SuspendLayout();
			// 
			// VerbCopyToolStripMenuItem
			// 
			VerbCopyToolStripMenuItem.Name = "VerbCopyToolStripMenuItem";
			VerbCopyToolStripMenuItem.Size = new System.Drawing.Size(204, 22);
			VerbCopyToolStripMenuItem.Text = "[Verb_Copy]";
			// 
			// VerbCutToolStripMenuItem
			// 
			VerbCutToolStripMenuItem.Name = "VerbCutToolStripMenuItem";
			VerbCutToolStripMenuItem.Size = new System.Drawing.Size(204, 22);
			VerbCutToolStripMenuItem.Text = "[Verb_Cut]";
			// 
			// VerbPasteToolStripMenuItem
			// 
			VerbPasteToolStripMenuItem.Name = "VerbPasteToolStripMenuItem";
			VerbPasteToolStripMenuItem.Size = new System.Drawing.Size(204, 22);
			VerbPasteToolStripMenuItem.Text = "[Verb_Paste]";
			// 
			// VerbDeleteToolStripMenuItem1
			// 
			VerbDeleteToolStripMenuItem1.Name = "VerbDeleteToolStripMenuItem1";
			VerbDeleteToolStripMenuItem1.Size = new System.Drawing.Size(204, 22);
			VerbDeleteToolStripMenuItem1.Text = "[Verb_Delete]";
			// 
			// VerbUndoToolStripMenuItem
			// 
			VerbUndoToolStripMenuItem.Name = "VerbUndoToolStripMenuItem";
			VerbUndoToolStripMenuItem.Size = new System.Drawing.Size(204, 22);
			VerbUndoToolStripMenuItem.Tag = "";
			VerbUndoToolStripMenuItem.Text = "[Verb_Undo]";
			// 
			// VerbRedoToolStripMenuItem
			// 
			VerbRedoToolStripMenuItem.Name = "VerbRedoToolStripMenuItem";
			VerbRedoToolStripMenuItem.Size = new System.Drawing.Size(204, 22);
			VerbRedoToolStripMenuItem.Tag = "";
			VerbRedoToolStripMenuItem.Text = "[Verb_Redo]";
			// 
			// VerbNewDocumentToolStripMenuItem
			// 
			VerbNewDocumentToolStripMenuItem.Name = "VerbNewDocumentToolStripMenuItem";
			VerbNewDocumentToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
			VerbNewDocumentToolStripMenuItem.Text = "[Verb_NewDocument]";
			// 
			// MenuLoadToolStripMenuItem
			// 
			MenuLoadToolStripMenuItem.Name = "MenuLoadToolStripMenuItem";
			MenuLoadToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
			MenuLoadToolStripMenuItem.Tag = "";
			MenuLoadToolStripMenuItem.Text = "[Verb_Open]";
			// 
			// MenuSaveToolStripMenuItem
			// 
			MenuSaveToolStripMenuItem.Name = "MenuSaveToolStripMenuItem";
			MenuSaveToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
			MenuSaveToolStripMenuItem.Tag = "";
			MenuSaveToolStripMenuItem.Text = "[Verb_Save]";
			// 
			// MenuSaveAsToolStripMenuItem
			// 
			MenuSaveAsToolStripMenuItem.Name = "MenuSaveAsToolStripMenuItem";
			MenuSaveAsToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
			MenuSaveAsToolStripMenuItem.Tag = "";
			MenuSaveAsToolStripMenuItem.Text = "[Verb_SaveAs]";
			// 
			// VerbPrintToolStripMenuItem
			// 
			VerbPrintToolStripMenuItem.Name = "VerbPrintToolStripMenuItem";
			VerbPrintToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
			VerbPrintToolStripMenuItem.Text = "[Verb_Print]";
			// 
			// VerbPrintPreviewToolStripMenuItem
			// 
			VerbPrintPreviewToolStripMenuItem.Name = "VerbPrintPreviewToolStripMenuItem";
			VerbPrintPreviewToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
			VerbPrintPreviewToolStripMenuItem.Text = "[Verb_PrintPreview]";
			// 
			// ToolStripMenuItem6
			// 
			ToolStripMenuItem6.Name = "ToolStripMenuItem6";
			ToolStripMenuItem6.Size = new System.Drawing.Size(195, 22);
			ToolStripMenuItem6.Text = "[Verb_ImportImage]";
			// 
			// ToolStripMenuItem1
			// 
			ToolStripMenuItem1.Name = "ToolStripMenuItem1";
			ToolStripMenuItem1.Size = new System.Drawing.Size(201, 6);
			// 
			// MenuSelectToolStripMenuItem
			// 
			MenuSelectToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.VerbSelectNextToolStripMenuItem,
            this.VerbSelectPreviousToolStripMenuItem,
            this.VerbSelectAllToolStripMenuItem,
            this.VerbSelectNoneToolStripMenuItem});
			MenuSelectToolStripMenuItem.Name = "MenuSelectToolStripMenuItem";
			MenuSelectToolStripMenuItem.Size = new System.Drawing.Size(204, 22);
			MenuSelectToolStripMenuItem.Text = "[Menu_Select]";
			// 
			// VerbSelectNextToolStripMenuItem
			// 
			this.VerbSelectNextToolStripMenuItem.Name = "VerbSelectNextToolStripMenuItem";
			this.VerbSelectNextToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
			this.VerbSelectNextToolStripMenuItem.Text = "[Verb_SelectNext]";
			// 
			// VerbSelectPreviousToolStripMenuItem
			// 
			this.VerbSelectPreviousToolStripMenuItem.Name = "VerbSelectPreviousToolStripMenuItem";
			this.VerbSelectPreviousToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
			this.VerbSelectPreviousToolStripMenuItem.Text = "[Verb_SelectPrevious]";
			// 
			// VerbSelectAllToolStripMenuItem
			// 
			this.VerbSelectAllToolStripMenuItem.Name = "VerbSelectAllToolStripMenuItem";
			this.VerbSelectAllToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
			this.VerbSelectAllToolStripMenuItem.Text = "[Verb_SelectAll]";
			// 
			// VerbSelectNoneToolStripMenuItem
			// 
			this.VerbSelectNoneToolStripMenuItem.Name = "VerbSelectNoneToolStripMenuItem";
			this.VerbSelectNoneToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
			this.VerbSelectNoneToolStripMenuItem.Text = "[Verb_SelectNone]";
			// 
			// VerbAlignLeftToolStripMenuItem
			// 
			VerbAlignLeftToolStripMenuItem.Name = "VerbAlignLeftToolStripMenuItem";
			VerbAlignLeftToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
			VerbAlignLeftToolStripMenuItem.Text = "[Verb_AlignLeft]";
			// 
			// VerbAlignCentreToolStripMenuItem
			// 
			VerbAlignCentreToolStripMenuItem.Name = "VerbAlignCentreToolStripMenuItem";
			VerbAlignCentreToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
			VerbAlignCentreToolStripMenuItem.Text = "[Verb_AlignCentre]";
			// 
			// VerbAlignRightToolStripMenuItem
			// 
			VerbAlignRightToolStripMenuItem.Name = "VerbAlignRightToolStripMenuItem";
			VerbAlignRightToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
			VerbAlignRightToolStripMenuItem.Text = "[Verb_AlignRight]";
			// 
			// VerbAlignTopToolStripMenuItem
			// 
			VerbAlignTopToolStripMenuItem.Name = "VerbAlignTopToolStripMenuItem";
			VerbAlignTopToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
			VerbAlignTopToolStripMenuItem.Text = "[Verb_AlignTop]";
			// 
			// VerbAlignMiddleToolStripMenuItem
			// 
			VerbAlignMiddleToolStripMenuItem.Name = "VerbAlignMiddleToolStripMenuItem";
			VerbAlignMiddleToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
			VerbAlignMiddleToolStripMenuItem.Text = "[Verb_AlignMiddle]";
			// 
			// VerbAlignBottomToolStripMenuItem
			// 
			VerbAlignBottomToolStripMenuItem.Name = "VerbAlignBottomToolStripMenuItem";
			VerbAlignBottomToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
			VerbAlignBottomToolStripMenuItem.Text = "[Verb_AlignBottom]";
			// 
			// ToolStripMenuItem7
			// 
			ToolStripMenuItem7.Name = "ToolStripMenuItem7";
			ToolStripMenuItem7.Size = new System.Drawing.Size(198, 6);
			// 
			// ToolStripSeparator3
			// 
			ToolStripSeparator3.Name = "ToolStripSeparator3";
			ToolStripSeparator3.Size = new System.Drawing.Size(201, 6);
			// 
			// ApplyToolStripMenuItem
			// 
			ApplyToolStripMenuItem.Name = "ApplyToolStripMenuItem";
			ApplyToolStripMenuItem.Size = new System.Drawing.Size(204, 22);
			ApplyToolStripMenuItem.Text = "[Verb_Texture]";
			// 
			// ToolStripMenuItem2
			// 
			ToolStripMenuItem2.Name = "ToolStripMenuItem2";
			ToolStripMenuItem2.Size = new System.Drawing.Size(224, 6);
			// 
			// ToolStripMenuItem5
			// 
			ToolStripMenuItem5.Name = "ToolStripMenuItem5";
			ToolStripMenuItem5.Size = new System.Drawing.Size(227, 22);
			ToolStripMenuItem5.Text = "[Verb_ZoomIn]";
			// 
			// VerbZoomOutToolStripMenuItem
			// 
			VerbZoomOutToolStripMenuItem.Name = "VerbZoomOutToolStripMenuItem";
			VerbZoomOutToolStripMenuItem.Size = new System.Drawing.Size(227, 22);
			VerbZoomOutToolStripMenuItem.Text = "[Verb_ZoomOut]";
			// 
			// VerbZoom100ToolStripMenuItem
			// 
			VerbZoom100ToolStripMenuItem.Name = "VerbZoom100ToolStripMenuItem";
			VerbZoom100ToolStripMenuItem.Size = new System.Drawing.Size(227, 22);
			VerbZoom100ToolStripMenuItem.Text = "[Verb_Zoom100]";
			// 
			// ToolStripMenuItem4
			// 
			ToolStripMenuItem4.Name = "ToolStripMenuItem4";
			ToolStripMenuItem4.Size = new System.Drawing.Size(224, 6);
			// 
			// ToolStripSeparator1
			// 
			ToolStripSeparator1.Name = "ToolStripSeparator1";
			ToolStripSeparator1.Size = new System.Drawing.Size(211, 6);
			// 
			// VerbClearToolStripMenuItem
			// 
			VerbClearToolStripMenuItem.Name = "VerbClearToolStripMenuItem";
			VerbClearToolStripMenuItem.Size = new System.Drawing.Size(214, 22);
			VerbClearToolStripMenuItem.Text = "[Verb_PageClear]";
			// 
			// VerbDeleteToolStripMenuItem
			// 
			VerbDeleteToolStripMenuItem.Name = "VerbDeleteToolStripMenuItem";
			VerbDeleteToolStripMenuItem.Size = new System.Drawing.Size(214, 22);
			VerbDeleteToolStripMenuItem.Text = "[Verb_PageDelete]";
			// 
			// MenuToolStripMenuItem
			// 
			MenuToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            MenuBeforeToolStripMenuItem,
            MenuAfterToolStripMenuItem});
			MenuToolStripMenuItem.Name = "MenuToolStripMenuItem";
			MenuToolStripMenuItem.Size = new System.Drawing.Size(214, 22);
			MenuToolStripMenuItem.Text = "[Menu_PageInsert]";
			// 
			// MenuBeforeToolStripMenuItem
			// 
			MenuBeforeToolStripMenuItem.Name = "MenuBeforeToolStripMenuItem";
			MenuBeforeToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
			MenuBeforeToolStripMenuItem.Tag = "Verb/PageInsertBefore";
			MenuBeforeToolStripMenuItem.Text = "[Menu_Before]";
			// 
			// MenuAfterToolStripMenuItem
			// 
			MenuAfterToolStripMenuItem.Name = "MenuAfterToolStripMenuItem";
			MenuAfterToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
			MenuAfterToolStripMenuItem.Tag = "Verb/PageInsertAfter";
			MenuAfterToolStripMenuItem.Text = "[Menu_After]";
			// 
			// ToolStripSeparator2
			// 
			ToolStripSeparator2.Name = "ToolStripSeparator2";
			ToolStripSeparator2.Size = new System.Drawing.Size(211, 6);
			// 
			// VerbEditPaperToolStripMenuItem
			// 
			VerbEditPaperToolStripMenuItem.Name = "VerbEditPaperToolStripMenuItem";
			VerbEditPaperToolStripMenuItem.Size = new System.Drawing.Size(214, 22);
			VerbEditPaperToolStripMenuItem.Text = "[Verb_EditPaper]";
			// 
			// PlaceholderToolStripMenuItem
			// 
			PlaceholderToolStripMenuItem.Name = "PlaceholderToolStripMenuItem";
			PlaceholderToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
			PlaceholderToolStripMenuItem.Text = "placeholder";
			// 
			// Label3
			// 
			Label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
			Label3.Image = ((System.Drawing.Image)(resources.GetObject("Label3.Image")));
			Label3.ImageAlign = System.Drawing.ContentAlignment.BottomCenter;
			Label3.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			Label3.Location = new System.Drawing.Point(16, 8);
			Label3.Name = "Label3";
			Label3.Size = new System.Drawing.Size(157, 104);
			Label3.TabIndex = 42;
			Label3.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// VerbQuitToolStripMenuItem
			// 
			VerbQuitToolStripMenuItem.Name = "VerbQuitToolStripMenuItem";
			VerbQuitToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
			VerbQuitToolStripMenuItem.Text = "[Verb_Quit]";
			// 
			// VerbSendBackToolStripMenuItem
			// 
			VerbSendBackToolStripMenuItem.Name = "VerbSendBackToolStripMenuItem";
			VerbSendBackToolStripMenuItem.Size = new System.Drawing.Size(211, 22);
			VerbSendBackToolStripMenuItem.Text = "[Verb_SendBack]";
			// 
			// VerbBringFrontToolStripMenuItem
			// 
			VerbBringFrontToolStripMenuItem.Name = "VerbBringFrontToolStripMenuItem";
			VerbBringFrontToolStripMenuItem.Size = new System.Drawing.Size(211, 22);
			VerbBringFrontToolStripMenuItem.Text = "[Verb_BringFront]";
			// 
			// VerbDuplicatePageToolStripMenuItem
			// 
			VerbDuplicatePageToolStripMenuItem.Name = "VerbDuplicatePageToolStripMenuItem";
			VerbDuplicatePageToolStripMenuItem.Size = new System.Drawing.Size(214, 22);
			VerbDuplicatePageToolStripMenuItem.Text = "[Verb_PageDuplicate]";
			// 
			// LabelWithBlend1
			// 
			LabelWithBlend1.Dock = System.Windows.Forms.DockStyle.Left;
			LabelWithBlend1.Image = global::Resources.AM.Divider;
			LabelWithBlend1.Location = new System.Drawing.Point(144, 0);
			LabelWithBlend1.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			LabelWithBlend1.Name = "LabelWithBlend1";
			LabelWithBlend1.Size = new System.Drawing.Size(8, 48);
			LabelWithBlend1.TabIndex = 6;
			// 
			// VerbPageSizeToolStripMenuItem
			// 
			VerbPageSizeToolStripMenuItem.Name = "VerbPageSizeToolStripMenuItem";
			VerbPageSizeToolStripMenuItem.Size = new System.Drawing.Size(214, 22);
			VerbPageSizeToolStripMenuItem.Text = "[Verb_EditWindowBounds]";
			// 
			// mnuEditPaperContext
			// 
			mnuEditPaperContext.Name = "mnuEditPaperContext";
			mnuEditPaperContext.Size = new System.Drawing.Size(160, 22);
			mnuEditPaperContext.Text = "[Verb_EditPaper]";
			// 
			// ToolStripMenuItem9
			// 
			ToolStripMenuItem9.Name = "ToolStripMenuItem9";
			ToolStripMenuItem9.Size = new System.Drawing.Size(198, 6);
			// 
			// ToolStripMenuItem10
			// 
			ToolStripMenuItem10.Name = "ToolStripMenuItem10";
			ToolStripMenuItem10.Size = new System.Drawing.Size(192, 6);
			// 
			// ToolStripMenuItem11
			// 
			ToolStripMenuItem11.Name = "ToolStripMenuItem11";
			ToolStripMenuItem11.Size = new System.Drawing.Size(192, 6);
			// 
			// mnuPageNext
			// 
			mnuPageNext.Name = "mnuPageNext";
			mnuPageNext.Size = new System.Drawing.Size(214, 22);
			mnuPageNext.Text = "[Verb_PageNext]";
			// 
			// mnuExportPage
			// 
			this.mnuExportPage.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuExportEMF,
            this.VerbExportJPEGToolStripMenuItem,
            this.VerbExportPNGToolStripMenuItem,
            this.VerbExportSVGToolStripMenuItem,
            this.VerbExportPageAsDocumentToolStripMenuItem});
			this.mnuExportPage.Name = "mnuExportPage";
			this.mnuExportPage.Size = new System.Drawing.Size(195, 22);
			this.mnuExportPage.Text = "[Menu_ExportPage]";
			// 
			// mnuExportEMF
			// 
			this.mnuExportEMF.Name = "mnuExportEMF";
			this.mnuExportEMF.Size = new System.Drawing.Size(239, 22);
			this.mnuExportEMF.Text = "[Verb_ExportEMF]";
			// 
			// VerbExportJPEGToolStripMenuItem
			// 
			this.VerbExportJPEGToolStripMenuItem.Name = "VerbExportJPEGToolStripMenuItem";
			this.VerbExportJPEGToolStripMenuItem.Size = new System.Drawing.Size(239, 22);
			this.VerbExportJPEGToolStripMenuItem.Text = "[Verb_ExportJPEG]";
			// 
			// VerbExportPNGToolStripMenuItem
			// 
			this.VerbExportPNGToolStripMenuItem.Name = "VerbExportPNGToolStripMenuItem";
			this.VerbExportPNGToolStripMenuItem.Size = new System.Drawing.Size(239, 22);
			this.VerbExportPNGToolStripMenuItem.Text = "[Verb_ExportPNG]";
			// 
			// VerbExportSVGToolStripMenuItem
			// 
			this.VerbExportSVGToolStripMenuItem.Name = "VerbExportSVGToolStripMenuItem";
			this.VerbExportSVGToolStripMenuItem.Size = new System.Drawing.Size(239, 22);
			this.VerbExportSVGToolStripMenuItem.Text = "[Verb_ExportSVG]";
			// 
			// VerbExportPageAsDocumentToolStripMenuItem
			// 
			this.VerbExportPageAsDocumentToolStripMenuItem.Name = "VerbExportPageAsDocumentToolStripMenuItem";
			this.VerbExportPageAsDocumentToolStripMenuItem.Size = new System.Drawing.Size(239, 22);
			this.VerbExportPageAsDocumentToolStripMenuItem.Text = "[Verb_ExportPageAsDocument]";
			// 
			// mnuEdit
			// 
			this.mnuEdit.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuVerbActive,
            this.mnuVerbInactive,
            this.mnuDividerMakeActive,
            VerbCopyToolStripMenuItem,
            this.verbCopyScriptsToolStripMenuItem,
            this.verbCopyPresentationToolStripMenuItem,
            VerbCutToolStripMenuItem,
            VerbPasteToolStripMenuItem,
            VerbDeleteToolStripMenuItem1,
            VerbUndoToolStripMenuItem,
            VerbRedoToolStripMenuItem,
            ToolStripMenuItem1,
            MenuSelectToolStripMenuItem,
            this.MenuFrontBackToolStripMenuItem,
            this.mnuAlignment,
            this.menuModifySelectedToolStripMenuItem,
            ToolStripSeparator3,
            this.mnuDoubleClick,
            this.verbConvertToPathToolStripMenuItem,
            ApplyToolStripMenuItem,
            this.verbCCFUpdateToolStripMenuItem,
            this.mnuMakeMask,
            this.mnuRemoveMask});
			this.mnuEdit.Name = "mnuEdit";
			this.mnuEdit.Size = new System.Drawing.Size(83, 20);
			this.mnuEdit.Text = "[Menu_Edit]";
			this.mnuEdit.DropDownOpening += new System.EventHandler(this.mnuEdit_DropDownOpening);
			// 
			// mnuVerbActive
			// 
			this.mnuVerbActive.Name = "mnuVerbActive";
			this.mnuVerbActive.Size = new System.Drawing.Size(204, 22);
			this.mnuVerbActive.Text = "[Verb_MakeActive]";
			// 
			// mnuVerbInactive
			// 
			this.mnuVerbInactive.Name = "mnuVerbInactive";
			this.mnuVerbInactive.Size = new System.Drawing.Size(204, 22);
			this.mnuVerbInactive.Text = "[Verb_MakeInactive]";
			// 
			// mnuDividerMakeActive
			// 
			this.mnuDividerMakeActive.Name = "mnuDividerMakeActive";
			this.mnuDividerMakeActive.Size = new System.Drawing.Size(201, 6);
			// 
			// verbCopyScriptsToolStripMenuItem
			// 
			this.verbCopyScriptsToolStripMenuItem.Name = "verbCopyScriptsToolStripMenuItem";
			this.verbCopyScriptsToolStripMenuItem.Size = new System.Drawing.Size(204, 22);
			this.verbCopyScriptsToolStripMenuItem.Text = "[Verb_CopyScripts]";
			// 
			// verbCopyPresentationToolStripMenuItem
			// 
			this.verbCopyPresentationToolStripMenuItem.Name = "verbCopyPresentationToolStripMenuItem";
			this.verbCopyPresentationToolStripMenuItem.Size = new System.Drawing.Size(204, 22);
			this.verbCopyPresentationToolStripMenuItem.Text = "[Verb_CopyPresentation]";
			// 
			// MenuFrontBackToolStripMenuItem
			// 
			this.MenuFrontBackToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            VerbSendBackToolStripMenuItem,
            VerbBringFrontToolStripMenuItem,
            this.VerbSendBackOneStepToolStripMenuItem,
            this.VerbBringFrontOneStepToolStripMenuItem});
			this.MenuFrontBackToolStripMenuItem.Name = "MenuFrontBackToolStripMenuItem";
			this.MenuFrontBackToolStripMenuItem.Size = new System.Drawing.Size(204, 22);
			this.MenuFrontBackToolStripMenuItem.Text = "[Menu_FrontBack]";
			// 
			// VerbSendBackOneStepToolStripMenuItem
			// 
			this.VerbSendBackOneStepToolStripMenuItem.Name = "VerbSendBackOneStepToolStripMenuItem";
			this.VerbSendBackOneStepToolStripMenuItem.Size = new System.Drawing.Size(211, 22);
			this.VerbSendBackOneStepToolStripMenuItem.Text = "[Verb_SendBackOneStep]";
			// 
			// VerbBringFrontOneStepToolStripMenuItem
			// 
			this.VerbBringFrontOneStepToolStripMenuItem.Name = "VerbBringFrontOneStepToolStripMenuItem";
			this.VerbBringFrontOneStepToolStripMenuItem.Size = new System.Drawing.Size(211, 22);
			this.VerbBringFrontOneStepToolStripMenuItem.Text = "[Verb_BringFrontOneStep]";
			// 
			// mnuAlignment
			// 
			this.mnuAlignment.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            VerbAlignLeftToolStripMenuItem,
            VerbAlignCentreToolStripMenuItem,
            VerbAlignRightToolStripMenuItem,
            VerbAlignTopToolStripMenuItem,
            VerbAlignMiddleToolStripMenuItem,
            VerbAlignBottomToolStripMenuItem,
            ToolStripMenuItem7,
            this.VerbEqualiseWidthToolStripMenuItem,
            this.verbLargestWidthToolStripMenuItem,
            this.VerbEqualiseHeightToolStripMenuItem,
            this.verbLargestHeightToolStripMenuItem,
            this.VerbSpreadHorizontalToolStripMenuItem,
            this.VerbSpreadVerticalToolStripMenuItem,
            ToolStripMenuItem9,
            this.VerbTidyGridToolStripMenuItem,
            this.VerbTidyShapeToolStripMenuItem,
            this.VerbTidyAngleToolStripMenuItem});
			this.mnuAlignment.Name = "mnuAlignment";
			this.mnuAlignment.Size = new System.Drawing.Size(204, 22);
			this.mnuAlignment.Text = "[Menu_Alignment]";
			// 
			// VerbEqualiseWidthToolStripMenuItem
			// 
			this.VerbEqualiseWidthToolStripMenuItem.Name = "VerbEqualiseWidthToolStripMenuItem";
			this.VerbEqualiseWidthToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
			this.VerbEqualiseWidthToolStripMenuItem.Text = "[Verb_SmallestWidth]";
			// 
			// verbLargestWidthToolStripMenuItem
			// 
			this.verbLargestWidthToolStripMenuItem.Name = "verbLargestWidthToolStripMenuItem";
			this.verbLargestWidthToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
			this.verbLargestWidthToolStripMenuItem.Text = "[Verb_LargestWidth]";
			// 
			// VerbEqualiseHeightToolStripMenuItem
			// 
			this.VerbEqualiseHeightToolStripMenuItem.Name = "VerbEqualiseHeightToolStripMenuItem";
			this.VerbEqualiseHeightToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
			this.VerbEqualiseHeightToolStripMenuItem.Text = "[Verb_SmallestHeight]";
			// 
			// verbLargestHeightToolStripMenuItem
			// 
			this.verbLargestHeightToolStripMenuItem.Name = "verbLargestHeightToolStripMenuItem";
			this.verbLargestHeightToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
			this.verbLargestHeightToolStripMenuItem.Text = "[Verb_LargestHeight]";
			// 
			// VerbSpreadHorizontalToolStripMenuItem
			// 
			this.VerbSpreadHorizontalToolStripMenuItem.Name = "VerbSpreadHorizontalToolStripMenuItem";
			this.VerbSpreadHorizontalToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
			this.VerbSpreadHorizontalToolStripMenuItem.Text = "[Verb_SpreadHorizontal]";
			// 
			// VerbSpreadVerticalToolStripMenuItem
			// 
			this.VerbSpreadVerticalToolStripMenuItem.Name = "VerbSpreadVerticalToolStripMenuItem";
			this.VerbSpreadVerticalToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
			this.VerbSpreadVerticalToolStripMenuItem.Text = "[Verb_SpreadVertical]";
			// 
			// VerbTidyGridToolStripMenuItem
			// 
			this.VerbTidyGridToolStripMenuItem.Name = "VerbTidyGridToolStripMenuItem";
			this.VerbTidyGridToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
			this.VerbTidyGridToolStripMenuItem.Text = "[Verb_TidyGrid]";
			// 
			// VerbTidyShapeToolStripMenuItem
			// 
			this.VerbTidyShapeToolStripMenuItem.Name = "VerbTidyShapeToolStripMenuItem";
			this.VerbTidyShapeToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
			this.VerbTidyShapeToolStripMenuItem.Text = "[Verb_TidyShape]";
			// 
			// VerbTidyAngleToolStripMenuItem
			// 
			this.VerbTidyAngleToolStripMenuItem.Name = "VerbTidyAngleToolStripMenuItem";
			this.VerbTidyAngleToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
			this.VerbTidyAngleToolStripMenuItem.Text = "[Verb_TidyAngle]";
			// 
			// menuModifySelectedToolStripMenuItem
			// 
			this.menuModifySelectedToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.verbToolStripMenuItem,
            this.verbHideTextSelectionToolStripMenuItem,
            this.verbShowGraphicSelectionToolStripMenuItem,
            this.verbHideGraphicSelectionToolStripMenuItem});
			this.menuModifySelectedToolStripMenuItem.Name = "menuModifySelectedToolStripMenuItem";
			this.menuModifySelectedToolStripMenuItem.Size = new System.Drawing.Size(204, 22);
			this.menuModifySelectedToolStripMenuItem.Text = "[Menu_ModifySelected]";
			// 
			// verbToolStripMenuItem
			// 
			this.verbToolStripMenuItem.Name = "verbToolStripMenuItem";
			this.verbToolStripMenuItem.Size = new System.Drawing.Size(228, 22);
			this.verbToolStripMenuItem.Text = "[Verb_ShowTextSelection]";
			// 
			// verbHideTextSelectionToolStripMenuItem
			// 
			this.verbHideTextSelectionToolStripMenuItem.Name = "verbHideTextSelectionToolStripMenuItem";
			this.verbHideTextSelectionToolStripMenuItem.Size = new System.Drawing.Size(228, 22);
			this.verbHideTextSelectionToolStripMenuItem.Text = "[Verb_HideTextSelection]";
			// 
			// verbShowGraphicSelectionToolStripMenuItem
			// 
			this.verbShowGraphicSelectionToolStripMenuItem.Name = "verbShowGraphicSelectionToolStripMenuItem";
			this.verbShowGraphicSelectionToolStripMenuItem.Size = new System.Drawing.Size(228, 22);
			this.verbShowGraphicSelectionToolStripMenuItem.Text = "[Verb_ShowGraphicSelection]";
			// 
			// verbHideGraphicSelectionToolStripMenuItem
			// 
			this.verbHideGraphicSelectionToolStripMenuItem.Name = "verbHideGraphicSelectionToolStripMenuItem";
			this.verbHideGraphicSelectionToolStripMenuItem.Size = new System.Drawing.Size(228, 22);
			this.verbHideGraphicSelectionToolStripMenuItem.Text = "[Verb_HideGraphicSelection]";
			// 
			// mnuDoubleClick
			// 
			this.mnuDoubleClick.Name = "mnuDoubleClick";
			this.mnuDoubleClick.Size = new System.Drawing.Size(204, 22);
			this.mnuDoubleClick.Tag = "Verb/DoubleClick";
			this.mnuDoubleClick.Text = "DoubleClick";
			this.mnuDoubleClick.Visible = false;
			this.mnuDoubleClick.Click += new System.EventHandler(this.mnuDoubleClick_Click);
			// 
			// verbConvertToPathToolStripMenuItem
			// 
			this.verbConvertToPathToolStripMenuItem.Name = "verbConvertToPathToolStripMenuItem";
			this.verbConvertToPathToolStripMenuItem.Size = new System.Drawing.Size(204, 22);
			this.verbConvertToPathToolStripMenuItem.Text = "[Verb_ConvertToPath]";
			// 
			// verbCCFUpdateToolStripMenuItem
			// 
			this.verbCCFUpdateToolStripMenuItem.Name = "verbCCFUpdateToolStripMenuItem";
			this.verbCCFUpdateToolStripMenuItem.Size = new System.Drawing.Size(204, 22);
			this.verbCCFUpdateToolStripMenuItem.Text = "[Verb_CCFUpdate]";
			// 
			// mnuMakeMask
			// 
			this.mnuMakeMask.Name = "mnuMakeMask";
			this.mnuMakeMask.Size = new System.Drawing.Size(204, 22);
			this.mnuMakeMask.Text = "[Verb_MakeMask]";
			// 
			// mnuRemoveMask
			// 
			this.mnuRemoveMask.Name = "mnuRemoveMask";
			this.mnuRemoveMask.Size = new System.Drawing.Size(204, 22);
			this.mnuRemoveMask.Text = "[Verb_RemoveMask]";
			// 
			// dlgPrint
			// 
			this.dlgPrint.AllowCurrentPage = true;
			this.dlgPrint.AllowSelection = true;
			this.dlgPrint.AllowSomePages = true;
			this.dlgPrint.UseEXDialog = true;
			// 
			// dlgPageSetup
			// 
			this.dlgPageSetup.AllowMargins = false;
			// 
			// pnlConstruction
			// 
			this.pnlConstruction.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
			this.pnlConstruction.Controls.Add(this.chkFullDP);
			this.pnlConstruction.Controls.Add(this.lblDiagnostic);
			this.pnlConstruction.Controls.Add(this.lblMouseDiagnostic);
			this.pnlConstruction.Controls.Add(Label3);
			this.pnlConstruction.Dock = System.Windows.Forms.DockStyle.Right;
			this.pnlConstruction.Location = new System.Drawing.Point(928, 47);
			this.pnlConstruction.Name = "pnlConstruction";
			this.pnlConstruction.Size = new System.Drawing.Size(269, 651);
			this.pnlConstruction.TabIndex = 56;
			this.pnlConstruction.Visible = false;
			this.pnlConstruction.VisibleChanged += new System.EventHandler(this.pnlConstruction_VisibleChanged);
			// 
			// chkFullDP
			// 
			this.chkFullDP.AutoSize = true;
			this.chkFullDP.Location = new System.Drawing.Point(144, 144);
			this.chkFullDP.Name = "chkFullDP";
			this.chkFullDP.Size = new System.Drawing.Size(60, 17);
			this.chkFullDP.TabIndex = 64;
			this.chkFullDP.Text = "Full DP";
			this.chkFullDP.UseVisualStyleBackColor = true;
			this.chkFullDP.CheckedChanged += new System.EventHandler(this.chkFullDP_CheckedChanged);
			// 
			// lblDiagnostic
			// 
			this.lblDiagnostic.Location = new System.Drawing.Point(0, 208);
			this.lblDiagnostic.Multiline = true;
			this.lblDiagnostic.Name = "lblDiagnostic";
			this.lblDiagnostic.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.lblDiagnostic.Size = new System.Drawing.Size(264, 472);
			this.lblDiagnostic.TabIndex = 62;
			// 
			// lblMouseDiagnostic
			// 
			this.lblMouseDiagnostic.ForeColor = System.Drawing.Color.DimGray;
			this.lblMouseDiagnostic.Location = new System.Drawing.Point(0, 184);
			this.lblMouseDiagnostic.Name = "lblMouseDiagnostic";
			this.lblMouseDiagnostic.Size = new System.Drawing.Size(232, 23);
			this.lblMouseDiagnostic.TabIndex = 49;
			// 
			// mnuTop
			// 
			this.mnuTop.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuFile,
            this.mnuEdit,
            this.mnuOptions,
            this.mnuSAWSet,
            this.mnuPageHeading,
            this.mnuSupport});
			this.mnuTop.Location = new System.Drawing.Point(0, 23);
			this.mnuTop.Name = "mnuTop";
			this.mnuTop.Size = new System.Drawing.Size(1197, 24);
			this.mnuTop.TabIndex = 57;
			this.mnuTop.Text = "MenuStrip1";
			// 
			// mnuFile
			// 
			this.mnuFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuUserUser,
            this.VerbStartToolStripMenuItem,
            this.mnuGraphicsMode,
            VerbNewDocumentToolStripMenuItem,
            MenuLoadToolStripMenuItem,
            this.mnuRecent,
            this.verbYoctoTestToolStripMenuItem,
            ToolStripMenuItem10,
            MenuSaveToolStripMenuItem,
            MenuSaveAsToolStripMenuItem,
            this.mnuExportPage,
            this.verbExportAsTextToolStripMenuItem,
            this.sAWDesktopToolStripMenuItem,
            this.sAWMenuIRMToolStripMenuItem,
            ToolStripMenuItem11,
            VerbPrintToolStripMenuItem,
            VerbPrintPreviewToolStripMenuItem,
            ToolStripMenuItem6,
            this.mnuUpdate,
            VerbQuitToolStripMenuItem});
			this.mnuFile.Name = "mnuFile";
			this.mnuFile.Size = new System.Drawing.Size(81, 20);
			this.mnuFile.Text = "[Menu_File]";
			this.mnuFile.DropDownOpening += new System.EventHandler(this.mnuFile_DropDownOpening);
			// 
			// mnuUserUser
			// 
			this.mnuUserUser.Name = "mnuUserUser";
			this.mnuUserUser.Size = new System.Drawing.Size(195, 22);
			this.mnuUserUser.Text = "[Verb_UserUser]";
			// 
			// VerbStartToolStripMenuItem
			// 
			this.VerbStartToolStripMenuItem.Name = "VerbStartToolStripMenuItem";
			this.VerbStartToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
			this.VerbStartToolStripMenuItem.Text = "[Verb_Start]";
			this.VerbStartToolStripMenuItem.Visible = false;
			// 
			// mnuGraphicsMode
			// 
			this.mnuGraphicsMode.Name = "mnuGraphicsMode";
			this.mnuGraphicsMode.Size = new System.Drawing.Size(195, 22);
			this.mnuGraphicsMode.Text = "[Menu_GraphicsMode]";
			// 
			// mnuRecent
			// 
			this.mnuRecent.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            PlaceholderToolStripMenuItem});
			this.mnuRecent.Name = "mnuRecent";
			this.mnuRecent.Size = new System.Drawing.Size(195, 22);
			this.mnuRecent.Text = "[Menu_Recent]";
			this.mnuRecent.DropDownOpening += new System.EventHandler(this.mnuRecent_DropDownOpening);
			// 
			// verbYoctoTestToolStripMenuItem
			// 
			this.verbYoctoTestToolStripMenuItem.Name = "verbYoctoTestToolStripMenuItem";
			this.verbYoctoTestToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
			this.verbYoctoTestToolStripMenuItem.Text = "[Verb_YoctoTest]";
			// 
			// verbExportAsTextToolStripMenuItem
			// 
			this.verbExportAsTextToolStripMenuItem.Name = "verbExportAsTextToolStripMenuItem";
			this.verbExportAsTextToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
			this.verbExportAsTextToolStripMenuItem.Text = "[Verb_ExportAsText]";
			// 
			// sAWDesktopToolStripMenuItem
			// 
			this.sAWDesktopToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuSaveDesktop,
            this.mnuDesktopEdit});
			this.sAWDesktopToolStripMenuItem.Name = "sAWDesktopToolStripMenuItem";
			this.sAWDesktopToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
			this.sAWDesktopToolStripMenuItem.Text = "[SAW_Desktop]";
			// 
			// mnuSaveDesktop
			// 
			this.mnuSaveDesktop.Name = "mnuSaveDesktop";
			this.mnuSaveDesktop.Size = new System.Drawing.Size(183, 22);
			this.mnuSaveDesktop.Text = "[SAW_Desktop_Save]";
			this.mnuSaveDesktop.Click += new System.EventHandler(this.mnuSaveDesktop_Click);
			// 
			// mnuDesktopEdit
			// 
			this.mnuDesktopEdit.Name = "mnuDesktopEdit";
			this.mnuDesktopEdit.Size = new System.Drawing.Size(183, 22);
			this.mnuDesktopEdit.Text = "[SAW_Desktop_Edit]";
			this.mnuDesktopEdit.Click += new System.EventHandler(this.mnuDesktopEdit_Click);
			// 
			// sAWMenuIRMToolStripMenuItem
			// 
			this.sAWMenuIRMToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.verbImportIRMToolStripMenuItem,
            this.verbExportIRMToolStripMenuItem});
			this.sAWMenuIRMToolStripMenuItem.Name = "sAWMenuIRMToolStripMenuItem";
			this.sAWMenuIRMToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
			this.sAWMenuIRMToolStripMenuItem.Text = "[SAW_Menu_IRM]";
			// 
			// verbImportIRMToolStripMenuItem
			// 
			this.verbImportIRMToolStripMenuItem.Name = "verbImportIRMToolStripMenuItem";
			this.verbImportIRMToolStripMenuItem.Size = new System.Drawing.Size(167, 22);
			this.verbImportIRMToolStripMenuItem.Text = "[Verb_ImportIRM]";
			// 
			// verbExportIRMToolStripMenuItem
			// 
			this.verbExportIRMToolStripMenuItem.Name = "verbExportIRMToolStripMenuItem";
			this.verbExportIRMToolStripMenuItem.Size = new System.Drawing.Size(167, 22);
			this.verbExportIRMToolStripMenuItem.Text = "[Verb_ExportIRM]";
			// 
			// mnuUpdate
			// 
			this.mnuUpdate.Name = "mnuUpdate";
			this.mnuUpdate.Size = new System.Drawing.Size(195, 22);
			this.mnuUpdate.Text = "[Menu_Update]";
			// 
			// mnuOptions
			// 
			this.mnuOptions.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuSnapOff,
            this.mnuSnapGrid,
            this.mnuSnapShape,
            this.mnuSnapAngle,
            ToolStripMenuItem2,
            this.mnuResizeDoc,
            ToolStripMenuItem5,
            VerbZoomOutToolStripMenuItem,
            VerbZoom100ToolStripMenuItem,
            this.mnuZoomPage,
            this.mnuZoomWidth,
            ToolStripMenuItem4,
            this.mnuConfigUser,
            this.sAWMenuSettingsFileToolStripMenuItem,
            this.VerbResetPalettesToolStripMenuItem,
            this.verbViewPredictionWordsToolStripMenuItem,
            this.mnuSystemConfig,
            this.mnuActivityConfig,
            this.mnuActivityConfigUser});
			this.mnuOptions.Name = "mnuOptions";
			this.mnuOptions.Size = new System.Drawing.Size(105, 20);
			this.mnuOptions.Text = "[Menu_Options]";
			this.mnuOptions.DropDownOpening += new System.EventHandler(this.mnuOptions_DropDownOpening);
			// 
			// mnuSnapOff
			// 
			this.mnuSnapOff.Image = global::Resources.AM.NoSnap_16;
			this.mnuSnapOff.Name = "mnuSnapOff";
			this.mnuSnapOff.Size = new System.Drawing.Size(227, 22);
			this.mnuSnapOff.Text = "[Menu_SnapOff]";
			this.mnuSnapOff.Click += new System.EventHandler(this.mnuSnapOff_Click);
			// 
			// mnuSnapGrid
			// 
			this.mnuSnapGrid.Image = global::Resources.AM.GridSnap_16;
			this.mnuSnapGrid.Name = "mnuSnapGrid";
			this.mnuSnapGrid.Size = new System.Drawing.Size(227, 22);
			this.mnuSnapGrid.Text = "[Menu_SnapGrid]";
			this.mnuSnapGrid.Click += new System.EventHandler(this.mnuSnapGrid_Click);
			// 
			// mnuSnapShape
			// 
			this.mnuSnapShape.Image = global::Resources.AM.ShapeSnap_16;
			this.mnuSnapShape.Name = "mnuSnapShape";
			this.mnuSnapShape.Size = new System.Drawing.Size(227, 22);
			this.mnuSnapShape.Text = "[Menu_SnapShape]";
			this.mnuSnapShape.Click += new System.EventHandler(this.mnuSnapShape_Click);
			// 
			// mnuSnapAngle
			// 
			this.mnuSnapAngle.Image = global::Resources.AM.AngleSnap_16;
			this.mnuSnapAngle.Name = "mnuSnapAngle";
			this.mnuSnapAngle.Size = new System.Drawing.Size(227, 22);
			this.mnuSnapAngle.Text = "[Menu_SnapAngle]";
			this.mnuSnapAngle.Click += new System.EventHandler(this.mnuSnapAngle_Click);
			// 
			// mnuResizeDoc
			// 
			this.mnuResizeDoc.Name = "mnuResizeDoc";
			this.mnuResizeDoc.Size = new System.Drawing.Size(227, 22);
			this.mnuResizeDoc.Text = "[Menu_ResizeDocToWindow]";
			this.mnuResizeDoc.Click += new System.EventHandler(this.mnuResizeDoc_Click);
			// 
			// mnuZoomPage
			// 
			this.mnuZoomPage.Name = "mnuZoomPage";
			this.mnuZoomPage.Size = new System.Drawing.Size(227, 22);
			this.mnuZoomPage.Text = "[Verb_ZoomPage]";
			// 
			// mnuZoomWidth
			// 
			this.mnuZoomWidth.Name = "mnuZoomWidth";
			this.mnuZoomWidth.Size = new System.Drawing.Size(227, 22);
			this.mnuZoomWidth.Text = "[Verb_ZoomWidth]";
			// 
			// mnuConfigUser
			// 
			this.mnuConfigUser.Name = "mnuConfigUser";
			this.mnuConfigUser.Size = new System.Drawing.Size(227, 22);
			this.mnuConfigUser.Text = "[Verb_ConfigUser]";
			// 
			// sAWMenuSettingsFileToolStripMenuItem
			// 
			this.sAWMenuSettingsFileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.verbSaveSettingsToolStripMenuItem,
            this.verbLoadSettingsToolStripMenuItem});
			this.sAWMenuSettingsFileToolStripMenuItem.Name = "sAWMenuSettingsFileToolStripMenuItem";
			this.sAWMenuSettingsFileToolStripMenuItem.Size = new System.Drawing.Size(227, 22);
			this.sAWMenuSettingsFileToolStripMenuItem.Text = "[SAW_Menu_SettingsFile]";
			// 
			// verbSaveSettingsToolStripMenuItem
			// 
			this.verbSaveSettingsToolStripMenuItem.Name = "verbSaveSettingsToolStripMenuItem";
			this.verbSaveSettingsToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
			this.verbSaveSettingsToolStripMenuItem.Text = "[Verb_SaveSettings]";
			// 
			// verbLoadSettingsToolStripMenuItem
			// 
			this.verbLoadSettingsToolStripMenuItem.Name = "verbLoadSettingsToolStripMenuItem";
			this.verbLoadSettingsToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
			this.verbLoadSettingsToolStripMenuItem.Text = "[Verb_LoadSettings]";
			// 
			// VerbResetPalettesToolStripMenuItem
			// 
			this.VerbResetPalettesToolStripMenuItem.Name = "VerbResetPalettesToolStripMenuItem";
			this.VerbResetPalettesToolStripMenuItem.Size = new System.Drawing.Size(227, 22);
			this.VerbResetPalettesToolStripMenuItem.Text = "[Verb_ResetPalettes]";
			// 
			// verbViewPredictionWordsToolStripMenuItem
			// 
			this.verbViewPredictionWordsToolStripMenuItem.Name = "verbViewPredictionWordsToolStripMenuItem";
			this.verbViewPredictionWordsToolStripMenuItem.Size = new System.Drawing.Size(227, 22);
			this.verbViewPredictionWordsToolStripMenuItem.Text = "[Verb_ViewPredictionWords]";
			// 
			// mnuSystemConfig
			// 
			this.mnuSystemConfig.Name = "mnuSystemConfig";
			this.mnuSystemConfig.Size = new System.Drawing.Size(227, 22);
			this.mnuSystemConfig.Text = "Edit system config";
			this.mnuSystemConfig.Visible = false;
			this.mnuSystemConfig.Click += new System.EventHandler(this.mnuSystemConfig_Click);
			// 
			// mnuActivityConfig
			// 
			this.mnuActivityConfig.Name = "mnuActivityConfig";
			this.mnuActivityConfig.Size = new System.Drawing.Size(227, 22);
			this.mnuActivityConfig.Text = "Edit activity config";
			this.mnuActivityConfig.Visible = false;
			this.mnuActivityConfig.Click += new System.EventHandler(this.mnuActivityConfig_Click);
			// 
			// mnuActivityConfigUser
			// 
			this.mnuActivityConfigUser.Name = "mnuActivityConfigUser";
			this.mnuActivityConfigUser.Size = new System.Drawing.Size(227, 22);
			this.mnuActivityConfigUser.Text = "Edit activity user config";
			this.mnuActivityConfigUser.Visible = false;
			this.mnuActivityConfigUser.Click += new System.EventHandler(this.mnuActivityConfigUser_Click);
			// 
			// mnuSAWSet
			// 
			this.mnuSAWSet.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuDefaultScriptsToolStripMenuItem,
            this.verbShowAllPopopsToolStripMenuItem,
            this.verbHideAllPopupsToolStripMenuItem,
            this.verbSetHelpItemToolStripMenuItem,
            this.verbGridWizardToolStripMenuItem});
			this.mnuSAWSet.Name = "mnuSAWSet";
			this.mnuSAWSet.Size = new System.Drawing.Size(103, 20);
			this.mnuSAWSet.Text = "[Menu_SAWSet]";
			this.mnuSAWSet.DropDownOpening += new System.EventHandler(this.mnuSAWSet_DropDownOpening);
			// 
			// menuDefaultScriptsToolStripMenuItem
			// 
			this.menuDefaultScriptsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.verbStartupScriptToolStripMenuItem,
            this.verbDefaultItemScriptToolStripMenuItem,
            this.verbDefaultGroupScriptToolStripMenuItem,
            this.verbDefaultEscapeScriptToolStripMenuItem});
			this.menuDefaultScriptsToolStripMenuItem.Name = "menuDefaultScriptsToolStripMenuItem";
			this.menuDefaultScriptsToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
			this.menuDefaultScriptsToolStripMenuItem.Text = "[Menu_DefaultScripts]";
			// 
			// verbStartupScriptToolStripMenuItem
			// 
			this.verbStartupScriptToolStripMenuItem.Name = "verbStartupScriptToolStripMenuItem";
			this.verbStartupScriptToolStripMenuItem.Size = new System.Drawing.Size(214, 22);
			this.verbStartupScriptToolStripMenuItem.Text = "[Verb_StartupScript]";
			// 
			// verbDefaultItemScriptToolStripMenuItem
			// 
			this.verbDefaultItemScriptToolStripMenuItem.Name = "verbDefaultItemScriptToolStripMenuItem";
			this.verbDefaultItemScriptToolStripMenuItem.Size = new System.Drawing.Size(214, 22);
			this.verbDefaultItemScriptToolStripMenuItem.Text = "[Verb_DefaultItemScript]";
			// 
			// verbDefaultGroupScriptToolStripMenuItem
			// 
			this.verbDefaultGroupScriptToolStripMenuItem.Name = "verbDefaultGroupScriptToolStripMenuItem";
			this.verbDefaultGroupScriptToolStripMenuItem.Size = new System.Drawing.Size(214, 22);
			this.verbDefaultGroupScriptToolStripMenuItem.Text = "[Verb_DefaultGroupScript]";
			// 
			// verbDefaultEscapeScriptToolStripMenuItem
			// 
			this.verbDefaultEscapeScriptToolStripMenuItem.Name = "verbDefaultEscapeScriptToolStripMenuItem";
			this.verbDefaultEscapeScriptToolStripMenuItem.Size = new System.Drawing.Size(214, 22);
			this.verbDefaultEscapeScriptToolStripMenuItem.Text = "[Verb_DefaultEscapeScript]";
			// 
			// verbShowAllPopopsToolStripMenuItem
			// 
			this.verbShowAllPopopsToolStripMenuItem.Name = "verbShowAllPopopsToolStripMenuItem";
			this.verbShowAllPopopsToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
			this.verbShowAllPopopsToolStripMenuItem.Text = "[Verb_ShowAllPopups]";
			// 
			// verbHideAllPopupsToolStripMenuItem
			// 
			this.verbHideAllPopupsToolStripMenuItem.Name = "verbHideAllPopupsToolStripMenuItem";
			this.verbHideAllPopupsToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
			this.verbHideAllPopupsToolStripMenuItem.Text = "[Verb_HideAllPopups]";
			// 
			// verbSetHelpItemToolStripMenuItem
			// 
			this.verbSetHelpItemToolStripMenuItem.Name = "verbSetHelpItemToolStripMenuItem";
			this.verbSetHelpItemToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
			this.verbSetHelpItemToolStripMenuItem.Text = "[Verb_SetPromptItem]";
			// 
			// verbGridWizardToolStripMenuItem
			// 
			this.verbGridWizardToolStripMenuItem.Name = "verbGridWizardToolStripMenuItem";
			this.verbGridWizardToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
			this.verbGridWizardToolStripMenuItem.Text = "[Verb_GridWizard]";
			// 
			// mnuPageHeading
			// 
			this.mnuPageHeading.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            mnuPageNext,
            this.VerbPreviousToolStripMenuItem,
            ToolStripSeparator1,
            VerbClearToolStripMenuItem,
            VerbDeleteToolStripMenuItem,
            this.mnuPageMove,
            MenuToolStripMenuItem,
            VerbDuplicatePageToolStripMenuItem,
            ToolStripSeparator2,
            VerbEditPaperToolStripMenuItem,
            VerbPageSizeToolStripMenuItem,
            this.mnuVerbSetOrigin});
			this.mnuPageHeading.Name = "mnuPageHeading";
			this.mnuPageHeading.Size = new System.Drawing.Size(89, 20);
			this.mnuPageHeading.Text = "[Menu_Page]";
			this.mnuPageHeading.DropDownOpening += new System.EventHandler(this.mnuPageHeading_DropDownOpening);
			// 
			// VerbPreviousToolStripMenuItem
			// 
			this.VerbPreviousToolStripMenuItem.Name = "VerbPreviousToolStripMenuItem";
			this.VerbPreviousToolStripMenuItem.Size = new System.Drawing.Size(214, 22);
			this.VerbPreviousToolStripMenuItem.Text = "[Verb_PagePrevious]";
			// 
			// mnuPageMove
			// 
			this.mnuPageMove.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuPageMoveUp,
            this.mnuPageMoveDown});
			this.mnuPageMove.Name = "mnuPageMove";
			this.mnuPageMove.Size = new System.Drawing.Size(214, 22);
			this.mnuPageMove.Text = "[Menu_PageMove]";
			// 
			// mnuPageMoveUp
			// 
			this.mnuPageMoveUp.Name = "mnuPageMoveUp";
			this.mnuPageMoveUp.Size = new System.Drawing.Size(166, 22);
			this.mnuPageMoveUp.Tag = "Verb/PageMoveUp";
			this.mnuPageMoveUp.Text = "[List_MoveUp]";
			// 
			// mnuPageMoveDown
			// 
			this.mnuPageMoveDown.Name = "mnuPageMoveDown";
			this.mnuPageMoveDown.Size = new System.Drawing.Size(166, 22);
			this.mnuPageMoveDown.Tag = "Verb/PageMoveDown";
			this.mnuPageMoveDown.Text = "[List_MoveDown]";
			// 
			// mnuVerbSetOrigin
			// 
			this.mnuVerbSetOrigin.Name = "mnuVerbSetOrigin";
			this.mnuVerbSetOrigin.Size = new System.Drawing.Size(214, 22);
			this.mnuVerbSetOrigin.Text = "[Verb_SetOrigin]";
			// 
			// mnuSupport
			// 
			this.mnuSupport.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuOpenErrorReport,
            this.mnuDiagnosticDivider,
            this.mnuDisplayDiagnostic,
            this.mnuGenerateError,
            this.mnuTest});
			this.mnuSupport.Name = "mnuSupport";
			this.mnuSupport.Size = new System.Drawing.Size(61, 20);
			this.mnuSupport.Text = "Support";
			this.mnuSupport.Visible = false;
			this.mnuSupport.DropDownOpening += new System.EventHandler(this.mnuSupport_Click);
			// 
			// mnuOpenErrorReport
			// 
			this.mnuOpenErrorReport.Name = "mnuOpenErrorReport";
			this.mnuOpenErrorReport.Size = new System.Drawing.Size(202, 22);
			this.mnuOpenErrorReport.Text = "Open error report";
			this.mnuOpenErrorReport.Click += new System.EventHandler(this.mnuOpenErrorReport_Click);
			// 
			// mnuDiagnosticDivider
			// 
			this.mnuDiagnosticDivider.Name = "mnuDiagnosticDivider";
			this.mnuDiagnosticDivider.Size = new System.Drawing.Size(199, 6);
			// 
			// mnuDisplayDiagnostic
			// 
			this.mnuDisplayDiagnostic.Name = "mnuDisplayDiagnostic";
			this.mnuDisplayDiagnostic.Size = new System.Drawing.Size(202, 22);
			this.mnuDisplayDiagnostic.Text = "Display diagnostic panel";
			this.mnuDisplayDiagnostic.Visible = false;
			this.mnuDisplayDiagnostic.Click += new System.EventHandler(this.mnuDisplayDiagnostic_Click);
			// 
			// mnuGenerateError
			// 
			this.mnuGenerateError.Name = "mnuGenerateError";
			this.mnuGenerateError.Size = new System.Drawing.Size(202, 22);
			this.mnuGenerateError.Text = "Generate error";
			this.mnuGenerateError.Click += new System.EventHandler(this.mnuGenerateError_Click);
			// 
			// mnuTest
			// 
			this.mnuTest.Name = "mnuTest";
			this.mnuTest.Size = new System.Drawing.Size(202, 22);
			this.mnuTest.Text = "Test";
			this.mnuTest.Click += new System.EventHandler(this.mnuTest_Click);
			// 
			// pnlPages
			// 
			this.pnlPages.BackColor = System.Drawing.SystemColors.Control;
			this.pnlPages.Controls.Add(this.ctrPageList);
			this.pnlPages.Controls.Add(this.pnlPageButtons);
			this.pnlPages.Dock = System.Windows.Forms.DockStyle.Right;
			this.pnlPages.Location = new System.Drawing.Point(838, 47);
			this.pnlPages.Name = "pnlPages";
			this.pnlPages.Size = new System.Drawing.Size(90, 544);
			this.pnlPages.TabIndex = 59;
			// 
			// ctrPageList
			// 
			this.ctrPageList.BackColor = System.Drawing.SystemColors.ControlDark;
			this.ctrPageList.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ctrPageList.Location = new System.Drawing.Point(0, 48);
			this.ctrPageList.Name = "ctrPageList";
			this.ctrPageList.Padding = new System.Windows.Forms.Padding(5);
			this.ctrPageList.Size = new System.Drawing.Size(90, 496);
			this.ctrPageList.TabIndex = 58;
			this.ctrPageList.DisplayPage += new SAW.ctrPages.DisplayPageEventHandler(this.ctrPageList_DisplayPage);
			this.ctrPageList.CentreViewOn += new SAW.ctrPages.CentreViewOnEventHandler(this.ctrPageList_CentreViewOn);
			this.ctrPageList.DisplayContextMenu += new SAW.ctrPages.DisplayContextMenuEventHandler(this.ctrPageList_DisplayContextMenu);
			// 
			// pnlPageButtons
			// 
			this.pnlPageButtons.Controls.Add(this.btnPageNext);
			this.pnlPageButtons.Controls.Add(this.btnPagePrevious);
			this.pnlPageButtons.Dock = System.Windows.Forms.DockStyle.Top;
			this.pnlPageButtons.Location = new System.Drawing.Point(0, 0);
			this.pnlPageButtons.Name = "pnlPageButtons";
			this.pnlPageButtons.Padding = new System.Windows.Forms.Padding(6, 4, 2, 4);
			this.pnlPageButtons.Size = new System.Drawing.Size(90, 48);
			this.pnlPageButtons.TabIndex = 59;
			// 
			// btnPageNext
			// 
			this.btnPageNext.Dock = System.Windows.Forms.DockStyle.Left;
			this.btnPageNext.Image = global::Resources.AM.PageNew;
			this.btnPageNext.Location = new System.Drawing.Point(46, 4);
			this.btnPageNext.Name = "btnPageNext";
			this.btnPageNext.Size = new System.Drawing.Size(40, 40);
			this.btnPageNext.TabIndex = 1;
			this.btnPageNext.TabStop = false;
			this.btnPageNext.UseVisualStyleBackColor = true;
			this.btnPageNext.Click += new System.EventHandler(this.btnPageNext_Click);
			// 
			// btnPagePrevious
			// 
			this.btnPagePrevious.Dock = System.Windows.Forms.DockStyle.Left;
			this.btnPagePrevious.Image = global::Resources.AM.Verb_PagePrevious;
			this.btnPagePrevious.Location = new System.Drawing.Point(6, 4);
			this.btnPagePrevious.Name = "btnPagePrevious";
			this.btnPagePrevious.Size = new System.Drawing.Size(40, 40);
			this.btnPagePrevious.TabIndex = 0;
			this.btnPagePrevious.TabStop = false;
			this.btnPagePrevious.UseVisualStyleBackColor = true;
			this.btnPagePrevious.Click += new System.EventHandler(this.btnPagePrevious_Click);
			// 
			// dlgPreview
			// 
			this.dlgPreview.AutoScrollMargin = new System.Drawing.Size(0, 0);
			this.dlgPreview.AutoScrollMinSize = new System.Drawing.Size(0, 0);
			this.dlgPreview.ClientSize = new System.Drawing.Size(400, 300);
			this.dlgPreview.Enabled = true;
			this.dlgPreview.Icon = ((System.Drawing.Icon)(resources.GetObject("dlgPreview.Icon")));
			this.dlgPreview.Name = "dlgPreview";
			this.dlgPreview.UseAntiAlias = true;
			this.dlgPreview.Visible = false;
			// 
			// pnlLeft
			// 
			this.pnlLeft.Controls.Add(this.pnlTools);
			this.pnlLeft.Dock = System.Windows.Forms.DockStyle.Left;
			this.pnlLeft.Location = new System.Drawing.Point(0, 47);
			this.pnlLeft.Name = "pnlLeft";
			this.pnlLeft.Size = new System.Drawing.Size(128, 651);
			this.pnlLeft.TabIndex = 61;
			this.pnlLeft.SizeChanged += new System.EventHandler(this.pnlLeft_SizeChanged);
			this.pnlLeft.Resize += new System.EventHandler(this.pnlLeft_Resize);
			// 
			// pnlTools
			// 
			this.pnlTools.Blend = SAW.ButtonPanel.BlendDirection.Off;
			this.pnlTools.Dock = System.Windows.Forms.DockStyle.Top;
			this.pnlTools.DoubleBuffered = false;
			this.pnlTools.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.pnlTools.Location = new System.Drawing.Point(0, 0);
			this.pnlTools.Name = "pnlTools";
			this.pnlTools.Size = new System.Drawing.Size(128, 264);
			this.pnlTools.TabIndex = 42;
			this.pnlTools.Click += new System.EventHandler(this.pnlTools_Click);
			this.pnlTools.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlTools_Paint);
			// 
			// pnlInfo
			// 
			this.pnlInfo.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.pnlInfo.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.pnlInfo.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.pnlInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.pnlInfo.Location = new System.Drawing.Point(131, 591);
			this.pnlInfo.Name = "pnlInfo";
			this.pnlInfo.Size = new System.Drawing.Size(797, 29);
			this.pnlInfo.TabIndex = 62;
			this.pnlInfo.Visible = false;
			this.pnlInfo.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlInfo_Paint);
			// 
			// pnlSnap
			// 
			this.pnlSnap.Controls.Add(LabelWithBlend1);
			this.pnlSnap.Controls.Add(this.rdoSnapAngle);
			this.pnlSnap.Controls.Add(this.rdoSnapShape);
			this.pnlSnap.Controls.Add(this.rdoSnapGrid);
			this.pnlSnap.Location = new System.Drawing.Point(1, 1);
			this.pnlSnap.Margin = new System.Windows.Forms.Padding(0);
			this.pnlSnap.Name = "pnlSnap";
			this.pnlSnap.Size = new System.Drawing.Size(161, 48);
			this.pnlSnap.TabIndex = 2;
			// 
			// rdoSnapAngle
			// 
			this.rdoSnapAngle.Blend = SAW.ButtonPanel.BlendDirection.Off;
			this.rdoSnapAngle.Dock = System.Windows.Forms.DockStyle.Left;
			this.rdoSnapAngle.Image = global::Resources.AM.AngleSnap;
			this.rdoSnapAngle.LargeImage = global::Resources.AM.AngleSnap_48;
			this.rdoSnapAngle.Location = new System.Drawing.Point(96, 0);
			this.rdoSnapAngle.Name = "rdoSnapAngle";
			this.rdoSnapAngle.Size = new System.Drawing.Size(48, 48);
			this.rdoSnapAngle.TabIndex = 3;
			this.rdoSnapAngle.Toggle = true;
			this.rdoSnapAngle.VariableSizeImageName = "AngleSnap";
			this.rdoSnapAngle.UserChecked += new System.EventHandler(this.rdoSnapAngle_UserChecked);
			this.rdoSnapAngle.MouseEnter += new System.EventHandler(this.rdoSnapAngle_MouseEnter);
			this.rdoSnapAngle.MouseLeave += new System.EventHandler(this.rdoUser_MouseLeave);
			// 
			// rdoSnapShape
			// 
			this.rdoSnapShape.Blend = SAW.ButtonPanel.BlendDirection.Off;
			this.rdoSnapShape.Dock = System.Windows.Forms.DockStyle.Left;
			this.rdoSnapShape.Image = global::Resources.AM.ShapeSnap;
			this.rdoSnapShape.LargeImage = global::Resources.AM.ShapeSnap_48;
			this.rdoSnapShape.Location = new System.Drawing.Point(48, 0);
			this.rdoSnapShape.Name = "rdoSnapShape";
			this.rdoSnapShape.Size = new System.Drawing.Size(48, 48);
			this.rdoSnapShape.TabIndex = 2;
			this.rdoSnapShape.Toggle = true;
			this.rdoSnapShape.VariableSizeImageName = "ShapeSnap";
			this.rdoSnapShape.UserChecked += new System.EventHandler(this.rdoSnapShape_UserChecked);
			this.rdoSnapShape.MouseEnter += new System.EventHandler(this.rdoSnapShape_MouseEnter);
			this.rdoSnapShape.MouseLeave += new System.EventHandler(this.rdoUser_MouseLeave);
			// 
			// rdoSnapGrid
			// 
			this.rdoSnapGrid.Blend = SAW.ButtonPanel.BlendDirection.Off;
			this.rdoSnapGrid.Dock = System.Windows.Forms.DockStyle.Left;
			this.rdoSnapGrid.Image = global::Resources.AM.GridSnap;
			this.rdoSnapGrid.LargeImage = global::Resources.AM.GridSnap_48;
			this.rdoSnapGrid.Location = new System.Drawing.Point(0, 0);
			this.rdoSnapGrid.Name = "rdoSnapGrid";
			this.rdoSnapGrid.Size = new System.Drawing.Size(48, 48);
			this.rdoSnapGrid.TabIndex = 1;
			this.rdoSnapGrid.Toggle = true;
			this.rdoSnapGrid.VariableSizeImageName = "GridSnap";
			this.rdoSnapGrid.UserChecked += new System.EventHandler(this.rdoSnapGrid_UserChecked);
			this.rdoSnapGrid.DoubleClick += new System.EventHandler(this.rdoSnapGrid_DoubleClick);
			this.rdoSnapGrid.MouseEnter += new System.EventHandler(this.rdoSnapGrid_MouseEnter);
			this.rdoSnapGrid.MouseLeave += new System.EventHandler(this.rdoUser_MouseLeave);
			// 
			// pnlToolbar
			// 
			this.pnlToolbar.AutoSize = true;
			this.pnlToolbar.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.pnlToolbar.Controls.Add(this.pnlSnap);
			this.pnlToolbar.Dock = System.Windows.Forms.DockStyle.Top;
			this.pnlToolbar.Location = new System.Drawing.Point(131, 47);
			this.pnlToolbar.MinimumSize = new System.Drawing.Size(1, 50);
			this.pnlToolbar.Name = "pnlToolbar";
			this.pnlToolbar.Padding = new System.Windows.Forms.Padding(1);
			this.pnlToolbar.Size = new System.Drawing.Size(707, 50);
			this.pnlToolbar.TabIndex = 64;
			this.pnlToolbar.Resize += new System.EventHandler(this.pnlToolbar_Resize);
			// 
			// ttMain
			// 
			this.ttMain.AutoPopDelay = 10000;
			this.ttMain.InitialDelay = 500;
			this.ttMain.ReshowDelay = 100;
			// 
			// dlgFolder
			// 
			this.dlgFolder.ShowNewFolderButton = false;
			// 
			// pnlLeftBorder
			// 
			this.pnlLeftBorder.BackColor = System.Drawing.Color.DarkGray;
			this.pnlLeftBorder.Dock = System.Windows.Forms.DockStyle.Left;
			this.pnlLeftBorder.Location = new System.Drawing.Point(128, 47);
			this.pnlLeftBorder.Name = "pnlLeftBorder";
			this.pnlLeftBorder.Size = new System.Drawing.Size(3, 651);
			this.pnlLeftBorder.TabIndex = 69;
			// 
			// ctxPalette
			// 
			this.ctxPalette.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuPaletteReset,
            this.mnuPaletteHide,
            this.mnuPaletteSmaller,
            this.mnuPaletteLarger,
            this.mnuPaletteShowSettings,
            this.mnuPaletteEdit,
            this.mnuPaletteScale,
            this.mnuPaletteChoose});
			this.ctxPalette.Name = "ctxPalette";
			this.ctxPalette.Size = new System.Drawing.Size(164, 164);
			// 
			// mnuPaletteReset
			// 
			this.mnuPaletteReset.Name = "mnuPaletteReset";
			this.mnuPaletteReset.Size = new System.Drawing.Size(163, 22);
			this.mnuPaletteReset.Text = "[Palette_Reset]";
			this.mnuPaletteReset.Click += new System.EventHandler(this.mnuPaletteReset_Click);
			// 
			// mnuPaletteHide
			// 
			this.mnuPaletteHide.Name = "mnuPaletteHide";
			this.mnuPaletteHide.Size = new System.Drawing.Size(163, 22);
			this.mnuPaletteHide.Text = "[Palette_Hide]";
			this.mnuPaletteHide.Click += new System.EventHandler(this.mnuPaletteHide_Click);
			// 
			// mnuPaletteSmaller
			// 
			this.mnuPaletteSmaller.Name = "mnuPaletteSmaller";
			this.mnuPaletteSmaller.Size = new System.Drawing.Size(163, 22);
			this.mnuPaletteSmaller.Text = "[Palette_Smaller]";
			this.mnuPaletteSmaller.Click += new System.EventHandler(this.mnuPaletteLargerSmaller_Click);
			// 
			// mnuPaletteLarger
			// 
			this.mnuPaletteLarger.Name = "mnuPaletteLarger";
			this.mnuPaletteLarger.Size = new System.Drawing.Size(163, 22);
			this.mnuPaletteLarger.Text = "[Palette_Larger]";
			this.mnuPaletteLarger.Click += new System.EventHandler(this.mnuPaletteLargerSmaller_Click);
			// 
			// mnuPaletteShowSettings
			// 
			this.mnuPaletteShowSettings.Name = "mnuPaletteShowSettings";
			this.mnuPaletteShowSettings.Size = new System.Drawing.Size(160, 6);
			// 
			// mnuPaletteEdit
			// 
			this.mnuPaletteEdit.Name = "mnuPaletteEdit";
			this.mnuPaletteEdit.Size = new System.Drawing.Size(163, 22);
			this.mnuPaletteEdit.Text = "[Palette_Edit]";
			this.mnuPaletteEdit.Click += new System.EventHandler(this.mnuPaletteEdit_Click);
			// 
			// mnuPaletteScale
			// 
			this.mnuPaletteScale.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuPalette100,
            this.mnuPalette200,
            this.mnuPalette300});
			this.mnuPaletteScale.Name = "mnuPaletteScale";
			this.mnuPaletteScale.Size = new System.Drawing.Size(163, 22);
			this.mnuPaletteScale.Text = "[Palette_Scale]";
			// 
			// mnuPalette100
			// 
			this.mnuPalette100.Name = "mnuPalette100";
			this.mnuPalette100.Size = new System.Drawing.Size(102, 22);
			this.mnuPalette100.Text = "100%";
			this.mnuPalette100.Click += new System.EventHandler(this.mnuPaletteScale_Click);
			// 
			// mnuPalette200
			// 
			this.mnuPalette200.Name = "mnuPalette200";
			this.mnuPalette200.Size = new System.Drawing.Size(102, 22);
			this.mnuPalette200.Text = "200%";
			this.mnuPalette200.Click += new System.EventHandler(this.mnuPaletteScale_Click);
			// 
			// mnuPalette300
			// 
			this.mnuPalette300.Name = "mnuPalette300";
			this.mnuPalette300.Size = new System.Drawing.Size(102, 22);
			this.mnuPalette300.Text = "300%";
			this.mnuPalette300.Click += new System.EventHandler(this.mnuPaletteScale_Click);
			// 
			// mnuPaletteChoose
			// 
			this.mnuPaletteChoose.Name = "mnuPaletteChoose";
			this.mnuPaletteChoose.Size = new System.Drawing.Size(163, 22);
			this.mnuPaletteChoose.Text = "[Palette_Choose]";
			this.mnuPaletteChoose.Visible = false;
			// 
			// ctrPromptArea
			// 
			this.ctrPromptArea.BackColor = System.Drawing.Color.White;
			this.ctrPromptArea.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ctrPromptArea.Location = new System.Drawing.Point(131, 620);
			this.ctrPromptArea.Name = "ctrPromptArea";
			this.ctrPromptArea.Padding = new System.Windows.Forms.Padding(4);
			this.ctrPromptArea.Size = new System.Drawing.Size(797, 78);
			this.ctrPromptArea.TabIndex = 60;
			this.ctrPromptArea.Text = "CtrPrompts1";
			this.ctrPromptArea.VariableColour = System.Drawing.Color.White;
			// 
			// ctrDocuments
			// 
			this.ctrDocuments.Dock = System.Windows.Forms.DockStyle.Top;
			this.ctrDocuments.Location = new System.Drawing.Point(0, 0);
			this.ctrDocuments.Name = "ctrDocuments";
			this.ctrDocuments.Size = new System.Drawing.Size(1197, 23);
			this.ctrDocuments.TabIndex = 68;
			this.ctrDocuments.Visible = false;
			// 
			// ctxBackground
			// 
			this.ctxBackground.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.VerbPasteToolStripMenuItem1,
            mnuEditPaperContext,
            this.mnuPageSizeContext});
			this.ctxBackground.Name = "ctxBackground";
			this.ctxBackground.Size = new System.Drawing.Size(161, 70);
			this.ctxBackground.Text = "Context for background";
			// 
			// VerbPasteToolStripMenuItem1
			// 
			this.VerbPasteToolStripMenuItem1.Name = "VerbPasteToolStripMenuItem1";
			this.VerbPasteToolStripMenuItem1.Size = new System.Drawing.Size(160, 22);
			this.VerbPasteToolStripMenuItem1.Text = "[Verb_Paste]";
			// 
			// mnuPageSizeContext
			// 
			this.mnuPageSizeContext.Name = "mnuPageSizeContext";
			this.mnuPageSizeContext.Size = new System.Drawing.Size(160, 22);
			this.mnuPageSizeContext.Text = "[Verb_PageSize]";
			// 
			// frmMain
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.ClientSize = new System.Drawing.Size(1197, 698);
			this.Controls.Add(this.pnlToolbar);
			this.Controls.Add(this.pnlPages);
			this.Controls.Add(this.pnlInfo);
			this.Controls.Add(this.ctrPromptArea);
			this.Controls.Add(this.pnlConstruction);
			this.Controls.Add(this.pnlLeftBorder);
			this.Controls.Add(this.pnlLeft);
			this.Controls.Add(this.mnuTop);
			this.Controls.Add(this.ctrDocuments);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MainMenuStrip = this.mnuTop;
			this.Name = "frmMain";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Splash!";
			this.pnlConstruction.ResumeLayout(false);
			this.pnlConstruction.PerformLayout();
			this.mnuTop.ResumeLayout(false);
			this.mnuTop.PerformLayout();
			this.pnlPages.ResumeLayout(false);
			this.pnlPageButtons.ResumeLayout(false);
			this.pnlLeft.ResumeLayout(false);
			this.pnlSnap.ResumeLayout(false);
			this.pnlToolbar.ResumeLayout(false);
			this.ctxPalette.ResumeLayout(false);
			this.ctxBackground.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		private System.Windows.Forms.PrintDialog dlgPrint;
		private System.Windows.Forms.PageSetupDialog dlgPageSetup;
		private ButtonPanel pnlTools;
		private System.Windows.Forms.Panel pnlConstruction;
		private System.Windows.Forms.MenuStrip mnuTop;
		private System.Windows.Forms.ToolStripMenuItem mnuFile;
		private System.Windows.Forms.Label lblMouseDiagnostic;
		private System.Windows.Forms.ToolStripMenuItem mnuSupport;
		private SAW.ctrPages ctrPageList;
		private System.Windows.Forms.Panel pnlPages;
		private System.Windows.Forms.Panel pnlPageButtons;
		private System.Windows.Forms.Button btnPagePrevious;
		private System.Windows.Forms.Button btnPageNext;
		private System.Windows.Forms.ToolStripMenuItem mnuPageHeading;
		private System.Windows.Forms.ToolStripMenuItem VerbPreviousToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem mnuPageMove;
		private System.Windows.Forms.ToolStripMenuItem mnuPageMoveUp;
		private System.Windows.Forms.ToolStripMenuItem mnuPageMoveDown;
		private System.Windows.Forms.PrintPreviewDialog dlgPreview;
		private System.Windows.Forms.ToolStripMenuItem mnuDoubleClick;
		private System.Windows.Forms.ToolStripMenuItem mnuOptions;
		private System.Windows.Forms.ToolStripMenuItem mnuSnapGrid;
		private System.Windows.Forms.ToolStripMenuItem mnuSnapShape;
		private System.Windows.Forms.ToolStripMenuItem mnuSnapOff;
		private System.Windows.Forms.ToolStripMenuItem mnuDisplayDiagnostic;
		private System.Windows.Forms.ToolStripMenuItem mnuZoomPage;
		private System.Windows.Forms.ToolStripMenuItem mnuExportEMF;
		private System.Windows.Forms.ToolStripMenuItem VerbExportJPEGToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem VerbExportPNGToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem VerbExportPageAsDocumentToolStripMenuItem;
		private System.Windows.Forms.TextBox lblDiagnostic;
		private System.Windows.Forms.ToolStripMenuItem mnuZoomWidth;
		private SAW.ctrPrompts ctrPromptArea;
		private System.Windows.Forms.Panel pnlLeft;
		private System.Windows.Forms.Panel pnlInfo;
		private System.Windows.Forms.ToolStripMenuItem mnuGenerateError;
		private System.Windows.Forms.ToolStripMenuItem mnuOpenErrorReport;
		private System.Windows.Forms.ToolStripMenuItem VerbSelectNextToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem VerbSelectPreviousToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem VerbSelectAllToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem VerbSelectNoneToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem mnuSnapAngle;
		private System.Windows.Forms.Panel pnlSnap;
		private SAW.IconRadio rdoSnapAngle;
		private System.Windows.Forms.FlowLayoutPanel pnlToolbar;
		private System.Windows.Forms.ToolStripMenuItem mnuAlignment;
		private System.Windows.Forms.ToolStripMenuItem VerbEqualiseWidthToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem VerbEqualiseHeightToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem VerbSpreadHorizontalToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem VerbSpreadVerticalToolStripMenuItem;
		private SAW.IconRadio rdoSnapShape;
		private SAW.IconRadio rdoSnapGrid;
		private System.Windows.Forms.ToolStripMenuItem mnuEdit;
		private System.Windows.Forms.ToolTip ttMain;
		private System.Windows.Forms.ToolStripMenuItem VerbStartToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem mnuRecent;
		internal SAW.DocumentTab ctrDocuments;
		private System.Windows.Forms.ToolStripSeparator mnuDiagnosticDivider;
		private System.Windows.Forms.ToolStripMenuItem VerbTidyGridToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem VerbTidyShapeToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem VerbTidyAngleToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem mnuTest;
		private System.Windows.Forms.CheckBox chkFullDP;
		private System.Windows.Forms.FolderBrowserDialog dlgFolder;
		private System.Windows.Forms.Panel pnlLeftBorder;
		private System.Windows.Forms.ToolStripMenuItem mnuVerbSetOrigin;
		private System.Windows.Forms.ToolStripMenuItem VerbResetPalettesToolStripMenuItem;
		internal System.Windows.Forms.ToolStripMenuItem MenuFrontBackToolStripMenuItem;
		internal System.Windows.Forms.ToolStripMenuItem VerbSendBackOneStepToolStripMenuItem;
		internal System.Windows.Forms.ToolStripMenuItem VerbBringFrontOneStepToolStripMenuItem;
		private System.Windows.Forms.ContextMenuStrip ctxPalette;
		internal System.Windows.Forms.ToolStripMenuItem mnuPaletteReset;
		private System.Windows.Forms.ToolStripMenuItem mnuPaletteHide;
		private System.Windows.Forms.ToolStripMenuItem mnuPaletteEdit;
		private System.Windows.Forms.ToolStripMenuItem mnuPaletteScale;
		private System.Windows.Forms.ToolStripMenuItem mnuPalette100;
		private System.Windows.Forms.ToolStripMenuItem mnuPalette200;
		private System.Windows.Forms.ToolStripMenuItem mnuPalette300;
		private System.Windows.Forms.ContextMenuStrip ctxBackground;
		internal System.Windows.Forms.ToolStripMenuItem mnuPageSizeContext;
		internal System.Windows.Forms.ToolStripMenuItem VerbExportSVGToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem mnuPaletteChoose;
		internal System.Windows.Forms.ToolStripMenuItem VerbPasteToolStripMenuItem1;
		internal System.Windows.Forms.ToolStripMenuItem mnuPaletteSmaller;
		internal System.Windows.Forms.ToolStripMenuItem mnuPaletteLarger;
		private System.Windows.Forms.ToolStripMenuItem mnuExportPage;
		private System.ComponentModel.IContainer components;
		private ToolStripMenuItem mnuUserUser;
		private ToolStripMenuItem mnuSAWSet;
		private ToolStripMenuItem menuDefaultScriptsToolStripMenuItem;
		private ToolStripMenuItem verbStartupScriptToolStripMenuItem;
		private ToolStripMenuItem verbDefaultItemScriptToolStripMenuItem;
		private ToolStripMenuItem verbDefaultGroupScriptToolStripMenuItem;
		private ToolStripMenuItem verbDefaultEscapeScriptToolStripMenuItem;
		private ToolStripMenuItem mnuConfigUser;
		private ToolStripMenuItem verbShowAllPopopsToolStripMenuItem;
		private ToolStripMenuItem verbHideAllPopupsToolStripMenuItem;
		private ToolStripMenuItem verbSetHelpItemToolStripMenuItem;
		private ToolStripMenuItem menuModifySelectedToolStripMenuItem;
		private ToolStripMenuItem verbToolStripMenuItem;
		private ToolStripMenuItem verbHideTextSelectionToolStripMenuItem;
		private ToolStripMenuItem verbShowGraphicSelectionToolStripMenuItem;
		private ToolStripMenuItem verbHideGraphicSelectionToolStripMenuItem;
		private ToolStripMenuItem verbGridWizardToolStripMenuItem;
		private ToolStripMenuItem verbViewPredictionWordsToolStripMenuItem;
		private ToolStripMenuItem mnuSystemConfig;
		private ToolStripMenuItem mnuActivityConfig;
		private ToolStripMenuItem verbExportAsTextToolStripMenuItem;
		private ToolStripMenuItem mnuActivityConfigUser;
		private ToolStripMenuItem verbLargestWidthToolStripMenuItem;
		private ToolStripMenuItem verbLargestHeightToolStripMenuItem;
		private ToolStripMenuItem sAWDesktopToolStripMenuItem;
		private ToolStripMenuItem mnuSaveDesktop;
		private ToolStripMenuItem mnuDesktopEdit;
		private ToolStripMenuItem verbCopyScriptsToolStripMenuItem;
		private ToolStripMenuItem verbCopyPresentationToolStripMenuItem;
		private ToolStripMenuItem verbCCFUpdateToolStripMenuItem;
		private ToolStripMenuItem sAWMenuSettingsFileToolStripMenuItem;
		private ToolStripMenuItem verbSaveSettingsToolStripMenuItem;
		private ToolStripMenuItem verbLoadSettingsToolStripMenuItem;
		private ToolStripMenuItem mnuUpdate;
		private ToolStripMenuItem sAWMenuIRMToolStripMenuItem;
		private ToolStripMenuItem verbImportIRMToolStripMenuItem;
		private ToolStripMenuItem verbExportIRMToolStripMenuItem;
		private ToolStripMenuItem mnuGraphicsMode;
		private ToolStripSeparator mnuPaletteShowSettings;
		private ToolStripMenuItem verbConvertToPathToolStripMenuItem;
		private ToolStripMenuItem mnuMakeMask;
		private ToolStripMenuItem mnuRemoveMask;
		private ToolStripMenuItem mnuVerbActive;
		private ToolStripMenuItem mnuVerbInactive;
		private ToolStripMenuItem mnuResizeDoc;
		private ToolStripSeparator mnuDividerMakeActive;
		private ToolStripMenuItem verbYoctoTestToolStripMenuItem;
	}

}
