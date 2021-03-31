using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SAW
{
	public partial class ctrDocumentOutline : PalettePanel
	{
		/// <summary>A list of all the nodes in the tree, for convenience.  We can look up a node using the data object as index.
		/// Or iterate all of them using .Values</summary>
		private Dictionary<Datum, TreeNode> NodeList;

		private Document m_Document;
		private bool m_Filling;
		/// <summary>True if the TV has not been updated as this is not visible</summary>
		private bool m_Stale;

		public event EventHandler<SingleFieldEventClass<Page>> RequestSelectPage;

		public ctrDocumentOutline()
		{
			m_Filling = true;
			InitializeComponent();
			AddGripper();
			Globals.Root.CurrentDocumentChanged += CurrentDocumentChanged;
			Globals.TransactionStored += Engine_TransactionStored;
			Globals.UpdateInfo += UpdateInfo;
			this.VisibleChanged += ctrDocumentOutline_VisibleChanged;
			this.tvOutline.NodeMoved += tvOutline_NodeMoved;

			imlItems.Images.Add(Resources.AM.InactiveItem_16);
			imlItems.Images.Add(Resources.AM.ActiveItem_16); // =1
			imlItems.Images.Add(Resources.AM.Page_16); // =2
			imlItems.Images.Add(Resources.AM.Popup_16); // =3
			imlItems.Images.Add(Resources.AM.PopupHidden_16); // =4
			imlItems.Images.Add(Resources.AM.help_16); // =5 for help item

			ContextMenuTools.InitialiseMenu(ctxEdit);
			Strings.Translate(ctxEdit); // must be after initialising

			m_Filling = false;
		}

		protected override void Dispose(bool disposing)
		{
			Globals.Root.CurrentDocumentChanged -= CurrentDocumentChanged;
			Globals.TransactionStored -= Engine_TransactionStored;
			Globals.UpdateInfo -= UpdateInfo;
			tvOutline.NodeMoved -= tvOutline_NodeMoved;
			RequestSelectPage = null;
			if (disposing)
				components?.Dispose();
			base.Dispose(disposing);
		}

		private void CurrentDocumentChanged()
		{
			m_Document = Globals.Root.CurrentDocument;
			FillTree();
		}

		private void Engine_TransactionStored(object sender, Globals.TransactionEventArgs e)
		{
			if (m_Stale)
			{
				if (Visible)
					FillTree();
				return;
			}
			if (!Visible)
			{
				m_Stale = true;
				tvOutline.Nodes.Clear();
				NodeList.Clear();
				return;
			}
			bool wipeout = false;
			foreach (Change change in e.Transaction)
			{
				if (change.Key is IShapeContainer && !change.IsCreate && !change.IsDelete)
				{
					IShapeContainer current = (IShapeContainer)change.Current;
					IShapeContainer previous = (IShapeContainer)change.Previous;
					if (current.Contents.Count != previous.Contents.Count)
						wipeout = true;
					else
						wipeout = Enumerable.Range(0, current.Contents.Count).Any(i => current.Contents[i] != previous.Contents[i]);
				}
				if (change.Key is Page)
				{
					if (change.IsCreate || change.IsDelete)
						wipeout = true;
				}
				else if (change.Key is Document)
					wipeout = true;
				else if (change.Key is Shape)
				{
					if (NodeList.ContainsKey(change.Key))
					{
						if (change.IsDelete)
						{
							NodeList[change.Key].Remove(); // removes the node from the tree
							NodeList.Remove(change.Key); // And update our list
						}
						else if (!change.IsCreate)
						{
							Shape currentShape = change.Current as Shape;
							Shape previousShape = (Shape)change.Previous;
							if (currentShape.Parent != previousShape.Parent || currentShape.Z != previousShape.Z)
								wipeout = true; // A shape has been moved from one container to another; just update the entire tree
							else if (currentShape.LabelText != previousShape.LabelText)
								NodeList[change.Current].Text = currentShape.Description;
							StyleNode(NodeList[change.Current], currentShape);
						}
					}
					else if (change.IsCreate)
					{
						Shape shp = change.Current as Shape;
						if (NodeList.ContainsKey((Datum)shp.Parent))
							AddTreeNode(shp, NodeList[(Datum)shp.Parent].Nodes);
						else
							wipeout = true; // can't find parent of new shape.  Just  give up and start again
					}
				}
				if (wipeout)
					break;
			}
			if (wipeout)
				FillTree();
			UpdateInfo();
		}

		public void ReflectSelection()
		{ // mostly only called on selection change (also when tree rebuilt)
			List<Shape> selected = Globals.Root.CurrentPage.SelectedShapes;
			foreach (TreeNode node in NodeList.Values)
			{
				node.BackColor = selected.Contains(node.Tag) ? Color.Cyan : Color.White;
			}
			UpdateInfo();
			if (selected?.Count == 1 && NodeList.ContainsKey(selected.First())) // second condition should be redundant, but kept in case we have selected as Item not the scriptable
			{
				TreeNode nodeForSelected = NodeList[selected.First()];
				nodeForSelected.EnsureVisible();
			}
		}

		private void UpdateInfo()
		{ // directly invoked from the Globals.UpdateInfo event, but also used as a local function
			List<Shape> selected = Globals.Root.CurrentPage.SelectedShapes;
			switch (selected.Count)
			{
				case 0:
					txtInfo.Text = "";
					txtInfo.Cursor = Cursors.Arrow;
					break;
				case 1:
					txtInfo.Text = selected.First().StatusInformation(false);
					txtInfo.Cursor = Cursors.Hand;
					break;
				default:
					txtInfo.Text = Strings.Item("N_Selected", selected.Count.ToString());
					txtInfo.Cursor = Cursors.Arrow;
					break;
			}
		}

		private void txtInfo_Click(object sender, EventArgs e)
		{
			List<Shape> selected = Globals.Root.CurrentPage.SelectedShapes;
			if (selected.Count != 1)
				return;
			Globals.PerformAction(Functions.Verb.Find(Functions.Codes.EditItemBounds), EditableView.ClickPosition.Sources.Irrelevant);
		}

		#region Filling tree

		private void FillTree()
		{
			m_Filling = true;
			NodeList = new Dictionary<Datum, TreeNode>();
			tvOutline.Nodes.Clear();
			if (m_Document != null)
			{
				foreach (var page in m_Document.Pages)
				{
					var node = tvOutline.Nodes.Add(Strings.Item("Page_N").Replace("%0", (Globals.Root.CurrentDocument.IndexOf(page) + 1).ToString()));
					node.Tag = page;
					node.ImageIndex = 2;
					node.SelectedImageIndex = 2;
					NodeList.Add(page, node);
					FillTreeNode(page, node.Nodes);
					node.Expand();
				}
				ReflectSelection();
			}
			m_Filling = false;
			m_Stale = false;
		}

		private void FillTreeNode(IShapeContainer container, TreeNodeCollection nodes)
		{
			foreach (Shape shape in container.Contents)
				AddTreeNode(shape, nodes);
		}

		private void AddTreeNode(Shape shape, TreeNodeCollection nodes)
		{
			TreeNode node = nodes.Add(shape.Description);
			node.Tag = shape;
			StyleNode(node, shape);
			NodeList.Add(shape, node);
			if (shape is IShapeContainer container)
				FillTreeNode(container, node.Nodes);
		}

		/// <summary>Updates the general appearance of the node.  Both used when creating the tree and if a node is edited</summary>
		private void StyleNode(TreeNode node, Shape shape)
		{
			if (shape is Scriptable scriptable)
			{
				if (!scriptable.Popup)
				{
					if (scriptable.SAWID == Globals.Root.CurrentPage.HelpSAWID && scriptable.SAWID > 0)
						node.ImageIndex = 5;
					else
						node.ImageIndex = 1; // active
				}
				else if (scriptable.Shown)
					node.ImageIndex = 3; // popup
				else
				{
					node.ImageIndex = 4; // hidden popup
					node.ForeColor = Color.Gray;
				}
			}
			else
				node.ImageIndex = 0; // misc shape
			node.SelectedImageIndex = node.ImageIndex;
		}

		#endregion

		private void tvOutline_AfterSelect(object sender, TreeViewEventArgs e)
		{
			if (m_Filling)
				return;
			TreeNode node = tvOutline.SelectedNode;
			if (node == null)
				return;
			object selected = node.Tag;
			while (node.Parent != null)
				node = node.Parent;
			Page pageWithin = (Page)node.Tag;
			if (pageWithin != Globals.Root.CurrentPage)
				RequestSelectPage?.Invoke(this, pageWithin);
			if (selected is Page page)
			{
				txtInfo.Text = Strings.Item("Info_PageBounds") + " " + GUIUtilities.RectangleToUserString(page.Bounds, page);
				txtInfo.Cursor = Cursors.Arrow;
			}
			if (!(selected is Shape)) // presumably the page itself was clicked on
				return;
			pageWithin.SelectOnly((Shape)selected);
		}

		private void ctrDocumentOutline_VisibleChanged(object sender, EventArgs e)
		{
			if (m_Stale && Visible)
				FillTree();
		}

		#region Search
		private void txtSearch_TextChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			tmrSearch.Enabled = true;
		}

		private Datum LastSearch;
		private void tmrSearch_Tick(object sender, EventArgs e)
		{
			tmrSearch.Enabled = false;
			int ID;
			List<Datum> listMatches = null;
			if (int.TryParse(txtSearch.Text, out ID))
				listMatches = (from item in NodeList.Keys where (item as Scriptable)?.SAWID == ID select item).ToList();
			else if (txtSearch.Text.Length > 3)
			{
				string search = txtSearch.Text.ToLower();
				listMatches = (from s in NodeList.Keys where s is Shape && ((Shape)s).Description.ToLower().Contains(search) select s).ToList();
			}
			if (listMatches == null || listMatches.Count == 0)
				return;
			int index = listMatches.IndexOf(LastSearch);
			if (index < 0 || index == listMatches.Count - 1)
				index = 0; // select first if new search OR previously highlighted item is last available
			else
				index += 1;
			LastSearch = listMatches[index];
			NodeList[LastSearch].EnsureVisible();
			tvOutline.SelectedNode = NodeList[LastSearch];
		}

		private void txtSearch_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == '\r')
			{
				tmrSearch_Tick(sender, null);
				e.Handled = true;
			}
		}

		#endregion

		#region Gripper and mouse events passed out to make resize in palette container work
		// all of these need to raise the click as if on this control, not the TV, so the palette form can do resizing
		// gripper now done by base class
		private void tvOutline_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
				OnMouseDown(new MouseEventArgs(e.Button, 1, e.X + tvOutline.Left, e.Y + tvOutline.Top, e.Delta));
			if (e.Button != MouseButtons.Right)
				return;
			if (tvOutline.SelectedNode == null)
				return;
			ContextMenuTools.PrepareEditMenu(ctxEdit.Items);
			ContextMenuTools.PrepareContextMenu(ctxEdit);
			ctxEdit.Show(tvOutline, e.Location);
		}

		private void tvOutline_MouseMove(object sender, MouseEventArgs e)
		{
			//if (e.Button == MouseButtons.Left)
			OnMouseMove(new MouseEventArgs(e.Button, 1, e.X + tvOutline.Left, e.Y + tvOutline.Top, e.Delta));
		}

		private void tvOutline_MouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
				OnMouseUp(new MouseEventArgs(e.Button, 1, e.X + tvOutline.Left, e.Y + tvOutline.Top, e.Delta));
		}

		#endregion

		private void tvOutline_NodeMoved(object sender, TreeViewWithMove.MoveEventArgs e)
		{
			Transaction transaction = new Transaction();
			Shape moved = (Shape)e.Moved.Tag;
			IShapeContainer parent = (IShapeContainer)moved.Parent;
			transaction.Edit(moved);
			transaction.Edit(moved.Parent as Datum);
			//int currentIndex = parent.Contents.IndexOf(moved);
			parent.Contents.Remove(moved);
			int newIndex = e.NewIndex;
			//if (currentIndex < e.NewIndex)
			//	newIndex--;
			parent.Contents.Insert(newIndex, moved);
			parent.FinishedModifyingContents(transaction);
			Globals.Root.StoreNewTransaction(transaction, true);
		}

	}
}
