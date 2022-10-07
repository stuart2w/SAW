using System.Collections.Generic;
using System;
using System.Drawing;
using System.Diagnostics;
using System.Collections;
using System.Windows.Forms;

namespace SAW
{
	internal sealed class TileList : ScrollableControl, IEnumerable<TileList.Tile>
	{

		public TileList()
		{
			BackColor = Color.White;
			AllowDrop = true;
			this.Resize += TileList_Resize;
			this.DragDrop += TileList_DragDrop;
			this.DragLeave += TileList_DragLeave;
			this.DragOver += TileList_DragOver;
			this.MouseDown += TileList_MouseDown;
			this.MouseMove += TileList_MouseMove;
			this.MouseUp += TileList_MouseUp;
			this.Paint += TileList_Paint;
			this.Resize += TileList_Resize;
		}

		public class Tile
		{
			public Image Image;
			public string Text;
			public object Tag;

			public virtual void Paint(Graphics gr, Rectangle bounds, TileList list)
			{
				// If only one item is provided it fills the tile; otherwise the image is on the left and the text on the right, unless the tile is
				// not very wide in which case both are superimposed
				if (Image != null)
				{
					Rectangle imageRect = bounds;
					if (!string.IsNullOrEmpty(Text ) && bounds.Width > bounds.Height * 2)
						imageRect.Width = imageRect.Height; // just uses the left-hand part
					GUIUtilities.CalcDestRect(Image.Size, ref imageRect);
					gr.DrawImage(Image, imageRect);
				}
				if (!string.IsNullOrEmpty(Text ))
				{
					Rectangle textRect = bounds;
					if (Image != null && bounds.Width > bounds.Height * 2)
					{
						textRect.X += bounds.Height;
						textRect.Width -= bounds.Height;
					}
					var format = GUIUtilities.StringFormatCentreLeft;
					if (bounds.Width <= bounds.Height * 2)
						format = GUIUtilities.StringFormatCentreCentre;
					gr.DrawString(Text, list.Font, Brushes.Black, textRect.ToRectangleF(), format);
				}
			}

		}

		/// <summary>Is true the user cannot move items around in the list; items dropped in appear at the end</summary>
		[System.ComponentModel.DefaultValue(false)]
		public bool NoRearrange { get; set; }
		[System.ComponentModel.DefaultValue(true)]
		public bool AllowSelection { get; set; } = true;

		public event EventHandler UserChangedSelection;
		/// <summary>Raised whenever a tile is moved by dragging and dropping</summary>
		public event EventHandler TileMoved;


		#region List
		private readonly List<Tile> m_Tiles = new List<Tile>();
		public void Add(Tile objTile)
		{
			Debug.Assert(!m_Tiles.Contains(objTile));
			m_Tiles.Add(objTile);
			SetLayout();
		}

		public void Clear()
		{
			m_Tiles.Clear();
			m_Selected = -1;
			m_Hover = -1;
			m_Drag = -1;
			SetLayout();
			Invalidate();
		}

		IEnumerator<Tile> IEnumerable<Tile>.GetEnumerator() => m_Tiles.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => m_Tiles.GetEnumerator();

		public void Remove(Tile objTile)
		{
			var index = m_Tiles.IndexOf(objTile);
			if (index < 0)
				return;
			m_Hover = -1;
			m_Drag = -1;
			if (m_Selected == index)
				m_Selected = -1;
			else if (m_Selected > index) 
				m_Selected -= 1;
			m_Tiles.RemoveAt(index);
			Invalidate();
		}

		public Tile Selected => m_Selected < 0 ? null : m_Tiles[m_Selected];

		#endregion

		#region Layout
		private const int SPACING = 4; // space between tiles; half to the left of the first one et cetera
		private Size m_TileSize = new Size(40, 40);
		private int m_Columns;
		private int m_Rows; // last row might not be fully filled

		public Size TileSize
		{
			get { return m_TileSize; }
			set
			{
				if (m_TileSize.Equals(value))
					return;
				m_TileSize = value;
				SetLayout();
			}
		}

		private void SetLayout()
		{
			m_Columns = Math.Max(1, Width / (m_TileSize.Width + SPACING));
			m_Rows = (m_Tiles.Count + m_Columns - 1) / m_Columns;
			base.AutoScrollMinSize = new Size(10, m_Rows * (m_TileSize.Height + SPACING));
			Invalidate();
		}

		private Rectangle TileBounds(int index)
		{
			int X = index % m_Columns;
			int Y = index / m_Columns;
			return new Rectangle(X * (m_TileSize.Width + SPACING) + SPACING / 2, Y * (m_TileSize.Height + SPACING) + SPACING / 2, m_TileSize.Width, m_TileSize.Height);
		}

		private void TileList_Resize(object sender, EventArgs e)
		{
			SetLayout();
		}
		#endregion

		#region Graphics

		private void TileList_Paint(object sender, PaintEventArgs e)
		{
			Rectangle clip = e.ClipRectangle;
			for (int index = 0; index <= m_Tiles.Count - 1; index++)
			{
				var rct = TileBounds(index);
				var test = rct; // Need to test the clipping using a slightly larger rectangle, because the border seems to draw around the outside
				test.Inflate(1, 1);
				if (!clip.IntersectsWith(test))
					continue;
				if (index == m_Selected)
					e.Graphics.FillRectangle(Brushes.Orange, rct);
				else if (index == m_Hover) 
					e.Graphics.FillRectangle(Brushes.AliceBlue, rct);
				e.Graphics.DrawRectangle(Pens.LightGray, rct);
				e.Graphics.SetClip(rct);
				m_Tiles[index].Paint(e.Graphics, rct, this);
				if (index == m_Drag)
				{
					// Dragged item is faded by drawing the background colour back over the top
					using (SolidBrush br = new SolidBrush(Color.FromArgb(160, BackColor)))
					{
						e.Graphics.FillRectangle(br, rct);
					}

				}
				e.Graphics.SetClip(clip);
			}
			if (m_DropAt >= 0) 
				e.Graphics.DrawImage(Resources.AM.MoveUp_24, DropBounds());
			if (!Enabled)
			{
				using (SolidBrush br = new SolidBrush(Color.FromArgb(160, Color.FromKnownColor(KnownColor.Control))))
				{
					e.Graphics.FillRectangle(br, 0, 0, Width, Height);
				}

			}
		}

		/// <summary>Invalidates the tile the given index, also allowing for -1, which does nothing</summary>
		private void InvalidateTile(int index)
		{
			if (index < 0)
				return;
			Invalidate(TileBounds(index));
		}

		#endregion

		#region Mouse
		private int m_Hover = -1;
		private int m_Selected = -1;
		private int m_Hit = -1; // Last item clicked on whether that selected it or not
		private Rectangle m_DragTrigger = Rectangle.Empty; // is defined after a click when the user might be initiating a drag.  Anything outside this rectangle starts dragging
		private int m_Drag = -1; // item being dragged
		private int m_DropAt = -1; // item dropped before
		private static Tile g_DragTile;

		/// <summary>Returns the index of the item at this location or -1</summary>
		private int HitTest(Point client)
		{
			// Calculate coordinate without checking (yet)if we're actually inside the bounds or possibly just in the spacing
			int X = (client.X - SPACING / 2) / m_TileSize.Width;
			int Y = (client.Y - SPACING / 2) / m_TileSize.Height;
			int index = X + Y * m_Columns;
			if (index < 0 || index >= m_Tiles.Count)
				return -1;
			// Lazy way of checking whether inside spacing:
			if (!TileBounds(index).Contains(client))
				return -1;
			return index;
		}

		private void TileList_MouseMove(object sender, MouseEventArgs e)
		{
			int hit = HitTest(e.Location);
			if (hit != m_Hover && m_Drag < 0) // Hovering not shown while dragging
			{
				InvalidateTile(m_Hover); // InvalidateTile checks for -1
				InvalidateTile(hit);
				m_Hover = hit;
			}
			if (!m_DragTrigger.IsEmpty && !m_DragTrigger.Contains(e.X, e.Y) && m_Hit >= 0)
			{
				m_Drag = m_Hit;
				m_Hover = -1;
				InvalidateTile(m_Drag);
				m_DragTrigger = Rectangle.Empty;
				Invalidate();
				g_DragTile = m_Tiles[m_Drag];
				var effect = this.DoDragDrop(m_Tiles[m_Drag], DragDropEffects.Move | DragDropEffects.Copy);
				if (effect == DragDropEffects.Move)
					// Must remove it from this list; it has been moved to another list
					m_Tiles.Remove(g_DragTile);
				if (effect == DragDropEffects.None)
					InvalidateTile(m_Drag);
				else
				{
					TileMoved?.Invoke(this, EventArgs.Empty);
					Invalidate();
				}
				m_Drag = -1;
				g_DragTile = null;
			}

		}

		private void TileList_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left)
				return;
			m_Hit = HitTest(e.Location);
			if (m_Hit != m_Selected && AllowSelection)
			{
				InvalidateTile(m_Selected); // InvalidateTile checks for -1
				InvalidateTile(m_Hit);
				m_Selected = m_Hit;
				UserChangedSelection?.Invoke(this, EventArgs.Empty);
			}
			if (m_Hit >= 0)
			{
				Size szDragSize = SystemInformation.DragSize;
				m_DragTrigger = new Rectangle(e.X - szDragSize.Width / 2, e.Y - szDragSize.Height / 2, szDragSize.Width, szDragSize.Height);
			}
		}

		private void TileList_MouseUp(object sender, MouseEventArgs e)
		{
			m_DragTrigger = Rectangle.Empty;
		}

		private void TileList_DragLeave(object sender, EventArgs e)
		{
			Invalidate(DropBounds());
			m_DropAt = -1;
		}

		private void TileList_DragOver(object sender, DragEventArgs e)
		{
			if (g_DragTile == null)
				e.Effect = DragDropEffects.None;
			else if (NoRearrange && m_Tiles.Contains(g_DragTile))
			{
				// Not allowed to move things around inside this list
				e.Effect = DragDropEffects.None;
			}
			else
			{
				// Can't use HitTest as the logic does need to be somewhat different
				var local = PointToClient(new Point(e.X, e.Y));
				int X = (local.X - SPACING / 2 + m_TileSize.Width / 2) / m_TileSize.Width;
				int Y = (local.Y - SPACING / 2) / m_TileSize.Height;
				int newIndex = X + Y * m_Columns;
				newIndex = Math.Min(newIndex, m_Tiles.Count); // Note: can be Count i.e. off the end of the current list
				if (NoRearrange)
					newIndex = m_Tiles.Count;
				if (newIndex != m_DropAt)
				{
					Invalidate(DropBounds());
					m_DropAt = newIndex;
					Invalidate(DropBounds());
				}
				e.Effect = DragDropEffects.Move;
			}
		}

		private void TileList_DragDrop(object sender, DragEventArgs e)
		{
			if (g_DragTile == null || m_DropAt < 0)
				e.Effect = DragDropEffects.None;
			else
			{
				// Returns Move is the source control is to remove the tile (i.e. this is not the source control); otherwise Copy
				if (!m_Tiles.Contains(g_DragTile))
					e.Effect = DragDropEffects.Move; // Tells the source control to remove it
				else
				{
					e.Effect = DragDropEffects.Copy;
					// Can remove it now
					if (m_DropAt > m_Tiles.IndexOf(g_DragTile))
						m_DropAt -= 1; // update the drop position to account for the removed tile
					m_Tiles.Remove(g_DragTile);
				}
				m_Tiles.Insert(m_DropAt, g_DragTile);
				TileMoved?.Invoke(this, EventArgs.Empty);
				Invalidate();
			}
			Invalidate(DropBounds()); // Might be redundant is we actually dropped anything
			m_DropAt = -1;
		}

		private const int DROPINDICATORSIZE = 24;
		/// <summary>Bounds of the drag-and-drop indicator (red arrow between 2 tiles)</summary>
		private Rectangle DropBounds()
		{
			if (m_DropAt < 0)
				return Rectangle.Empty;
			Point position; // the location of the top of the centre of the image
			if (m_DropAt == 0)
				position = new Point(SPACING / 2, SPACING / 2 + m_TileSize.Height);
			else
			{
				var previous = TileBounds(m_DropAt - 1);
				position = new Point(previous.Right + SPACING / 2, previous.Bottom);
			}
			return new Rectangle(position.X - DROPINDICATORSIZE / 2, position.Y, DROPINDICATORSIZE, DROPINDICATORSIZE);
		}

		#endregion

	}

}
