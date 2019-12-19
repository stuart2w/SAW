using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using SAW.Functions;
using Action = SAW.Functions.Action;


namespace SAW
{
	public partial class ctrActionSelect : UserControl
	{
		private TreeNode m_KeyNode; // the node, if any, representing keypress
		private TreeNode m_CharacterNode;
		private TreeNode m_TextNode;
		private TreeView TV;
		private Label lblSearch;
		private TextBoxAllKeys txtSearch;
		private ToolTip ttActions;
		private System.ComponentModel.Container components;
		// ditto for typing a symbol
		private static readonly Font m_SymbolFont = new Font("Symbol", 14);

		/// <summary>If an entry in the TV which requires customisation has been selected, this is the value, fully specified.  Only meaningful if Change of this matches Change of selected node </summary>
		private static Action m_CustomValue = Action.Empty;

		//private static string m_strCustomValue = ""; //If an entry with a customisable value has been selected, this is the selected value
		//private static Parameters m_eCustomFor; // The parameter for which m_strCustomValue is valid.  Changing the parameter probably renders the value meaningless
		//private bool m_bolChanging; // true when the code is updating the selected node
		public ImageList ActionImageList { get; private set; } // config form wants to reuse same for TB editor

		public enum Modes
		{
			Key, // selecting what a key does
			Button // the action for a button (within a palette).  This is slightly more restrictive, as it doesn't make sense to do things like mouse movement
		}

		#region Construct/dispose
		public ctrActionSelect()
		{
			ActionImageList = new ImageList() { ColorDepth = ColorDepth.Depth32Bit, ImageSize = new Size(32, 32), TransparentColor = Color.Transparent };
			InitializeComponent();
			TV.HideSelection = false;
			if (!this.DesignMode)
				lblSearch.Text = Strings.Item("Search");
			Strings.Translate(this, ttActions);
			if (!this.DesignMode)
				TV.ImageList = ActionImageList;
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing)
			{
				ActionImageList?.Dispose();
				ActionImageList = null;
			}
		}

		#endregion

		public Modes Mode { get; set; }

		public Action SelectedAction
		{
			get
			{
				if (TV.SelectedNode?.Tag == null)
					return Action.Empty;
				Action action = (Action)TV.SelectedNode.Tag;
				if (((action as ParameterAction)?.CustomRequiresParameter ?? false) && m_CustomValue.Change == action.Change)
				{
					// A customisable action has been selected; select the value stored in the form
					return m_CustomValue; // was already cloned when set, so no need to do so again!
				}
				return action;
			}
			set
			{
				m_CustomValue = Action.Empty;
				if (value.IsEmpty)
					TV.SelectedNode = null;
				else
				{
					TV.SelectedNode = FindAction(value);
					if (TV.SelectedNode != null)
						TV.SelectedNode.EnsureVisible();
					else if (value is ParameterAction) // see if there is a "Other" option for this parameter
					{
						Action custom = new ParameterAction(value.Change, 0, true);
						var node = FindAction(custom); // Cannot assign directly to SelectedNode because the change handler needs the params to be selected
						if (node != null)
						{
							m_CustomValue = value.Clone();
							TV.SelectedNode = node;
							node.EnsureVisible();
						}
					}
				}
			}
		}

		private TreeNode FindAction(Action action)
		{
			switch (action.Change)
			{
				case Parameters.Action_Key:
					return m_KeyNode; // this won't be found otherwise, because the exact value won't match
				case Parameters.Action_Character:
					return m_CharacterNode;
				case Parameters.Action_Text:
					return m_TextNode;
				default:
					return FindAction(action, TV.Nodes);
			}
		}

		private static TreeNode FindAction(Action action, TreeNodeCollection list)
		{
			foreach (TreeNode test in list)
			{
				if (test != null && action.Equals(test.Tag))
					return test;
				TreeNode found = FindAction(action, test.Nodes);
				if (found != null)
					return found;
			}
			return null;
		}

		public bool IsFilled
		{ get { return TV.Nodes.Count > 0; } }

		public TreeNode GetNodeAt(Point pt)
		{
			return TV.GetNodeAt(pt);
		}

		public event EventHandler SelectionChanged;

		private void TV_AfterSelect(object sender, EventArgs e)
		{
			SelectionChanged?.Invoke(this, EventArgs.Empty);
		}

		private void TV_DoubleClick(object sender, EventArgs e)
		{
			base.OnDoubleClick(EventArgs.Empty);
		}

		private void TV_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
		{
			// occurs before select
			Action action;
			if (e.Node.Tag != null)
				action = (Action)e.Node.Tag; // TV.SelectedNode.Tag
			else
				action = Action.Empty;
			if ((action as ParameterAction)?.CustomRequiresParameter ?? false)
			{
				// must select value
				string value = "Cancel"; // If a valid selection is made the TextValue is put here
				Action selected = SelectedAction;
				switch (action.Change)
				{
					case Parameters.LineWidth:
						float width = frmLineWidth.Ask(selected.Change == Parameters.LineWidth ? (selected as ParameterAction).Value / 100f : float.NaN);
						if (!float.IsNaN(width))
							value = (width * 100).ToString();
						break;
					case Parameters.LineColour:
					case Parameters.FillColour:
						Color col = Color.Empty;
						if (selected.Change == Parameters.FillColour || selected.Change == Parameters.LineColour)
							col = Color.FromArgb((selected as ParameterAction).Value);
						col = frmMoreColours.DisplayPickColour(FindForm(), col, useSettings: false);
						if (!col.IsEmpty)
							value = col.ToArgb().ToString();
						break;
					default:
						Utilities.LogSubError("Unexpected Change in ctrActionSelect.TV_AfterSelect");
						break;
				}
				if (value == "Cancel")
					TV.SelectedNode = null;
				else
				{
					m_CustomValue = new ParameterAction(action.Change, int.Parse(value));
					SelectionChanged?.Invoke(this, EventArgs.Empty);
				}
			}
		}

		#region Filling tree
		public void Fill(AppliedConfig applied)
		{
			// fill the Treeview showing the possible actions
			try
			{
				TV.Nodes.Clear(); // should already be empty, but just in case...
				ActionImageList.Images.Clear();
				ActionImageList.Images.Add("Folder", Resources.AM.folder); // must be first, because by default the Treeview uses index 0 for the folders
				ActionImageList.Images.Add("Empty", Resources.AM.Empty);
				ActionImageList.Images.Add("CursorClick", Resources.AM.CursorClick);

				// shapes...
				TreeNode subFolder;
				TreeNode folder = TV.Nodes.Add(Strings.Item("Config_ShapesTools"));
				foreach (Shape.Shapes eShape in Shape.UserShapes)
				{
					//If Shape.HideShapeType (eShape) andalso not **advanced continue for
					var action = new ToolAction(eShape);
					TreeNode node = folder.Nodes.Add(action.DescriptionWithoutAccelerator());
					ActionImageList.Images.Add(action.ToString(), GUIUtilities.ShapeImage(eShape, 32));
					node.ImageKey = action.ToString();
					node.SelectedImageKey = action.ToString();
					node.Tag = action;
				}


				// Drawing area...
				folder = TV.Nodes.Add(Strings.Item("Config_KeysDrawing"));
				AddKeyVerbs(1000, (int)Codes.Decrement, "Config_ShapeVerbs", folder.Nodes); // excludes ChooseExisting
				if (Mode != Modes.Button)
					AddKeyVerbs(1100, 1149, "Config_MouseControl", folder.Nodes);
				AddKeyVerbs(1150, 1199, "Config_Scrolling", folder.Nodes);
				AddKeyVerbs(1200, 1249, "Config_ShapeSelection", folder.Nodes);
				subFolder = folder.Nodes.Add(Strings.Item("Config_Keys_Snapping"));
				foreach (Shape.SnapModes snap in new[] { Shape.SnapModes.Off, Shape.SnapModes.Grid, Shape.SnapModes.Shape, Shape.SnapModes.Angle })
				{
					AddAction(subFolder, new SnapAction(snap));
				}
				AddAction(subFolder, new ShowGridAction(1));
				if (Mode != Modes.Button)
				{
					Action tools = new PaletteAction(Parameters.Tool);
					AddAction(folder, tools, tools.DescriptionWithoutAccelerator(), "CursorClick");
				}

				// Verbs...
				TreeNode verbs = AddKeyVerbs(0, 999, "Config_Verbs");

				// Styles...
				folder = TV.Nodes.Add(Strings.Item("Config_Keys_Styles"));
				FillStyleActions(folder, Parameters.FillColour, true);
				FillStyleActions(folder, Parameters.FillPattern);
				FillStyleActions(folder, Parameters.LineColour, true);
				FillStyleActions(folder, Parameters.LineWidth, true);
				FillStyleActions(folder, Parameters.LinePattern);

				// font doesn't use normal parameters
				subFolder = folder.Nodes.Add(Strings.Item("Palette_TextStyle"));
				if (Mode == Modes.Key)
					AddAction(subFolder, new PaletteAction(Parameters.FontSize), Strings.Item("Config_SwitchToPalette"), "CursorClick");
				AddAction(subFolder, new ParameterAction(Parameters.TextAlignment, (int)StringAlignment.Near), Strings.Item("Align_Left"));
				AddAction(subFolder, new ParameterAction(Parameters.TextAlignment, (int)StringAlignment.Center), Strings.Item("Align_Centre"));
				AddAction(subFolder, new ParameterAction(Parameters.TextAlignment, (int)StringAlignment.Far), Strings.Item("Align_Right"));
				AddAction(subFolder, new ParameterAction(Parameters.TextVerticalAlignment, (int)StringAlignment.Near), Strings.Item("Align_Top"));
				AddAction(subFolder, new ParameterAction(Parameters.TextVerticalAlignment, (int)StringAlignment.Center), Strings.Item("Align_Middle"));
				AddAction(subFolder, new ParameterAction(Parameters.TextVerticalAlignment, (int)StringAlignment.Far), Strings.Item("Align_Bottom"));
				AddAction(subFolder, Verb.Find(Codes.TextSmaller));
				AddAction(subFolder, Verb.Find(Codes.TextLarger));


				// arrows just select palette
				subFolder = folder.Nodes.Add(Strings.Item("Palette_Arrowheads"));
				if (Mode == Modes.Key)
					AddAction(subFolder, new PaletteAction(Parameters.ArrowheadEndType), Strings.Item("Config_SwitchToPalette"), "CursorClick");

				subFolder = folder.Nodes.Add(Strings.Item("Config_Keys_Palettes"));
				AddAction(subFolder, new PaletteButtonAction(1), Strings.Item("Config_Keys_Palette_Refine"), "Empty");
				AddAction(subFolder, new PaletteButtonAction(2), Strings.Item("Config_Keys_Palette_Transparent"), "Empty");
				AddAction(subFolder, new PaletteButtonAction(3), Strings.Item("Config_Keys_Palette_Rainbow"), "Empty");
				// in palettes main folder:
				AddAction(folder, Verb.Find(Codes.MovePalette));
				AddAction(folder, Verb.Find(Codes.ResizePalette));
				if (Mode == Modes.Key)
				{
					subFolder = folder.Nodes.Add(Strings.Item("Config_SwitchToPalette_Other"));
					foreach (Palette objPalette in from p in Palette.List.Values where p.PalettePurpose.IsCustom orderby Strings.Translate(p.Title) select p)
					{
						// only lists custom ones - assuming all purposes can be/have been listed separately
						AddAction(subFolder, objPalette.PalettePurpose.GetSelectAction(), Strings.Translate(objPalette.EditDescription));
					}
				}

				if (Mode == Modes.Button)
				{
					m_KeyNode = TV.Nodes.Add(Strings.Item("Action_SimulateKey"));
					m_KeyNode.ImageIndex = -1;
					m_KeyNode.Tag = new KeyAction(Keys.None); // actual key will be selected in the editing screen
					m_CharacterNode = TV.Nodes.Add(Strings.Item("Action_TypeCharacter"));
					m_CharacterNode.ImageIndex = -1;
					m_CharacterNode.Tag = new CharAction((char)0); // actual text will be selected in the editing screen
					m_TextNode = TV.Nodes.Add(Strings.Item("Action_TypeText"));
					m_TextNode.ImageIndex = -1;
					m_TextNode.Tag = new TextAction(""); // actual text will be selected in the editing screen
				}

			}
			catch (Exception ex) when (!Globals.Root.IsDebug)
			{
				MessageBox.Show(ex.Message);
			}
		}


		/// <summary>Adds verbs for which there is a text description within the given range</summary>
		private TreeNode AddKeyVerbs(int first, int last, string folderName, TreeNodeCollection within = null)
		{
			if (within == null)
				within = TV.Nodes;
			TreeNode folder = within.Add(Strings.Item(folderName));
			foreach (Codes verb in Enum.GetValues(typeof(Codes)))
			{
				if (verb != Codes.None && (int)verb >= first && (int)verb <= last && Strings.Exists("Verb_" + verb))
					AddAction(folder, Verb.Find(verb));
			}
			return folder;
		}

		private void FillStyleActions(TreeNode top, Parameters parameter, bool withCustom = false)
		{
			TreeNode folder = top.Nodes.Add(ParameterSupport.GetParameterTypeName(parameter));
			// the first entry selects the palette, but limited for one or two parameters where they share a palette
			if (Mode != Modes.Button) // buttons on palettes don't switch to other palettes
			{
				switch (parameter)
				{
					case Parameters.LineWidth:
						break;
					default:
						AddAction(folder, new PaletteAction(parameter), Strings.Item("Config_SwitchToPalette"), "CursorClick");
						break;
				}
			}
			foreach (int value in ParameterSupport.GetStandardParameterValues(parameter))
			{
				AddAction(folder, Action.Create(parameter, value), ParameterSupport.GetParameterValueName(parameter, value));
			}
			if (withCustom)
				AddAction(folder, new ParameterAction(parameter, 0, true), Strings.Item("Custom_Style_Value"));
		}

		private void AddAction(TreeNode folder, Action action, string title = "?", string imageName = "?")
		{
			if (action == null)
				return; // can happen for verb enums that aren't used
			// we don't usually use objaction.Description, because sometimes that is a bit longer (describing context which is obvious in the Treeview)
			// strImage is the key of the image to use; pass "" to omit the image.  If "?" it is created from the action
			if (title == "?")
				title = action.DescriptionWithoutAccelerator();
			TreeNode node = folder.Nodes.Add(title);
			if (imageName == "?")
			{
				Image bitmap = action.CreateSampleImage();
				if (bitmap != null)
				{
					ActionImageList.Images.Add(action.ToString(), bitmap);
					imageName = action.ToString();
				}
				else
					imageName = "Empty";
			}
			node.ImageKey = imageName;
			node.SelectedImageKey = imageName;
			node.Tag = action;
		}

		#endregion

		#region Search

		private void txtSearch_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == (char)13)
			{
				Search();
				e.Handled = true;
			}
		}

		private void txtSearch_TextChanged(object sender, EventArgs e)
		{
			if (txtSearch.Text.Length > 0)
				Search();
		}

		private void Search()
		{
			if (txtSearch.Text.Length == 0)
				return;
			string search = txtSearch.Text.ToLower();
			TreeNode start = TV.SelectedNode != null ? TV.SelectedNode : TV.Nodes[0];
			TreeNode node = start;
			do
			{
				if (node.Nodes.Count > 0)
					node = node.Nodes[0]; // Move into first child
				else
				{
					TreeNode test;
					do
					{
						test = node.NextNode; // No children, try sibling
						if (test == null)
						{
							// If no siblings, move up to parent again
							if (node.Parent == null)
							{
								node = TV.Nodes[0]; // At the top level, start back at the beginning
								test = node;
								break;
							}
							else
								node = node.Parent;
						}
					} while (test == null);
					node = test;
				}
				if (node == start)
				{
					// Nothing found
					Console.Beep();
					return;
				}
				if (node.Text.ToLower().Contains(search))
				{
					TV.SelectedNode = node;
					node.EnsureVisible();
					return;
				}
			} while (true);
		}

		#endregion

		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Panel pnlSearch;
			this.txtSearch = new TextBoxAllKeys();
			this.txtSearch.KeyPress += this.txtSearch_KeyPress;
			this.txtSearch.TextChanged += this.txtSearch_TextChanged;
			this.lblSearch = new Label();
			this.TV = new TreeView();
			this.TV.AfterSelect += this.TV_AfterSelect;
			this.TV.DoubleClick += this.TV_DoubleClick;
			this.TV.NodeMouseClick += this.TV_NodeMouseClick;
			this.ttActions = new ToolTip(this.components);
			pnlSearch = new Panel();
			pnlSearch.SuspendLayout();
			this.SuspendLayout();
			//
			//pnlSearch
			//
			pnlSearch.Controls.Add(this.txtSearch);
			pnlSearch.Controls.Add(this.lblSearch);
			pnlSearch.Dock = DockStyle.Top;
			pnlSearch.Location = new Point(0, 0);
			pnlSearch.Name = "pnlSearch";
			pnlSearch.Size = new Size(287, 24);
			pnlSearch.TabIndex = 2;
			//
			//txtSearch
			//
			this.txtSearch.AcceptsReturn = true;
			this.txtSearch.Dock = DockStyle.Fill;
			this.txtSearch.Location = new Point(39, 0);
			this.txtSearch.Name = "txtSearch";
			this.txtSearch.Size = new Size(248, 20);
			this.txtSearch.TabIndex = 2;
			this.ttActions.SetToolTip(this.txtSearch, "[Config_Search_Tooltip]");
			//
			//lblSearch
			//
			this.lblSearch.AutoSize = true;
			this.lblSearch.Dock = DockStyle.Left;
			this.lblSearch.Location = new Point(0, 0);
			this.lblSearch.Name = "lblSearch";
			this.lblSearch.Size = new Size(39, 13);
			this.lblSearch.TabIndex = 1;
			this.lblSearch.Text = "Label1";
			//
			//TV
			//
			this.TV.Dock = DockStyle.Fill;
			this.TV.Font = new Font("Microsoft Sans Serif", 10.0F, FontStyle.Regular, GraphicsUnit.Point, 0);
			this.TV.Location = new Point(0, 24);
			this.TV.Name = "TV";
			this.TV.Size = new Size(287, 248);
			this.TV.TabIndex = 0;
			//
			//ctrActionSelect
			//
			this.Controls.Add(this.TV);
			this.Controls.Add(pnlSearch);
			this.Name = "ctrActionSelect";
			this.Size = new Size(287, 272);
			pnlSearch.ResumeLayout(false);
			pnlSearch.PerformLayout();
			this.ResumeLayout(false);

		}

	}

}
