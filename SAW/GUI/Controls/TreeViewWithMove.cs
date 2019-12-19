using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SAW
{
	/// <summary>Derivative of Treeview which implements moving a node within its siblings (only)</summary>
	[ToolboxItem(false)]
	public class TreeViewWithMove : TreeView
	{
		public class MoveEventArgs : EventArgs
		{
			public readonly TreeNode Moved;
			public readonly int NewIndex;

			public MoveEventArgs(TreeNode moved, int newIndex)
			{
				Moved = moved;
				NewIndex = newIndex;
			}

		}

		public event EventHandler<MoveEventArgs> NodeMoved;

		/// <summary>The node being dragged</summary>
		private TreeNode m_Moving;
		/// <summary>The node on which it is being dropped</summary>
		private TreeNode m_DropTarget;
		private bool m_DropAbove;

		private Indicator m_Indicator;

		public TreeViewWithMove()
		{
			m_Indicator = new Indicator();
			Controls.Add(m_Indicator);
		}

		protected override void OnItemDrag(ItemDragEventArgs e)
		{
			DoDragDrop(e.Item, DragDropEffects.Move);
		}

		protected override void OnDragOver(DragEventArgs e)
		{
			base.OnDragOver(e);
			Point pt = PointToClient(new Point(e.X, e.Y));
			if (m_DropTarget != null)
				Invalidate(DropIndicatorBounds);
			TreeNode target = GetNodeAt(pt);
			if (target != null)
			{
				if (target == m_Moving.Parent)
				{
					// if pointing at parent, put as first item in container
					target = target.Nodes[0];
					m_DropAbove = true;
					if (m_Moving == m_Moving.Parent.FirstNode)
						target = null; // if we are moving the first node, there's no point moving to start of list!
				}
				else if (target.Parent != m_Moving.Parent)
					target = null;
				else if (target == m_Moving)
					target = null;
				else
					m_DropAbove = (target.Index < m_Moving.Index);
			}
			m_DropTarget = target;
			m_Indicator.Visible = (m_DropTarget != null);
			m_Indicator.Bounds = DropIndicatorBounds;
			e.Effect = (m_DropTarget != null) ? DragDropEffects.Move : DragDropEffects.None;
			if (m_DropTarget != null)
				Invalidate(DropIndicatorBounds);
		}

		private Rectangle DropIndicatorBounds
		{
			get
			{
				if (m_DropTarget == null)
					return Rectangle.Empty;
				Rectangle bounds = m_DropTarget.Bounds;
				bounds.X = 0;
				bounds.Width = bounds.Height;
				if (m_DropAbove)
					bounds.Y -= bounds.Height / 2;
				else
					bounds.Y += bounds.Height / 2;
				return bounds;
			}
		}

		protected override void OnDragDrop(DragEventArgs e)
		{
			if (m_Moving != null)
				m_Moving.ForeColor = m_MovingOriginalColour;
			m_Indicator.Visible = false;

			if (!e.Data.GetDataPresent(typeof(TreeNode)))
				return;
			if (m_Moving == null || m_DropTarget == null)
				return;

			int newIndex = m_Moving.Parent.Nodes.IndexOf(m_DropTarget);
			if (newIndex < 0)
			{
				Utilities.LogSubError("TreeviewWithMove, target not found within parent");
				return;
			}
			NodeMoved?.Invoke(this, new MoveEventArgs(m_Moving, newIndex));
		}

		private Color m_MovingOriginalColour = Color.Black;
		protected override void OnDragEnter(DragEventArgs e)
		{
			base.OnDragEnter(e);
			TreeNode movingNode = (TreeNode)e.Data.GetData(typeof(TreeNode));
			if (movingNode.TreeView != this)
				movingNode = null;
			if (movingNode != null)
			{
				m_MovingOriginalColour = movingNode.ForeColor;
				movingNode.ForeColor = Color.LightGray;
			}
			m_Moving = movingNode;
			e.Effect = movingNode != null ? DragDropEffects.Move : DragDropEffects.None;
		}

		protected override void OnDragLeave(EventArgs e)
		{
			base.OnDragLeave(e);
			if (m_Moving != null)
				m_Moving.ForeColor = m_MovingOriginalColour;
			m_Moving = null;
			m_Indicator.Visible = false;
		}

		/// <summary>Shows a marker where the element will be dropped when the user drags and drops</summary>
		private class Indicator : Panel
		{
			public Indicator()
			{
				SetStyle(ControlStyles.SupportsTransparentBackColor | ControlStyles.UserPaint, true);
				BackColor = Color.Transparent;
				Visible = false;
			}

			protected override void OnPaint(PaintEventArgs e)
			{
				e.Graphics.DrawImage(Resources.AM.ConfigFlow, new Rectangle(0, 0, Width, Height));
			}

			protected override void OnPaintBackground(PaintEventArgs e)
			{
			}

		}

	}
}