using System.Collections.Generic;
using System;
using System.Drawing;
using System.Diagnostics;
using System.IO;

namespace SAW
{

	public class ImportedImage : AbstractRectangle
	{
		// an image which has been imported into the document
		// the "Start" verb just places at an arbitrary position; Choose makes the final placement
		// because .net keeps the file open all the time, we copy the entire data into a memory buffer and open a memory stream to get the actual image
		// in v1 was BoundsDefines

		protected SharedReference<SharedImage> m_Data;

		protected Size m_ImageSize;
		/// <summary>may be blank (eg if pasted in) </summary>
		protected string m_OriginalName = "";
		/// <summary>remembers whether the image has been flipped horizontally or vertically.  BUT NO LONGER USED - instead points are manipulated </summary>
		protected Size m_Flip = new Size(1, 1);
		internal static bool SingleClickPlacement = true; // currently defined by Start

		public const float NOMINALPIXELSPERMILLIMETRE = 96 / 25.4f;
		// when deciding how big to make an image on the page we do not need/want to use the actual DPI of the screen, because this page might be displayed later
		// on a different machine.  All that we want is that the image is a natural size, which works best if we take the standard DPI (96)

		#region Constructors and stream control
		public ImportedImage()
		{
			// should only be used when loading an existing one from file
		}

		public ImportedImage(string file, Document document, Transaction transaction)
		{
			// should be used when creating a new one; i.e. importing from disk
			m_Data = (SharedImage)document.AddSharedResourceFromFile(file, transaction);
			//m_Image = m_Data.Content.GetImage();
			m_ImageSize = m_Data.Content.GetSize();
			SetDefaultSize();
			m_OriginalName = Path.GetFileName(file);
		}

		/// <summary>Does not support SVG</summary>
		public ImportedImage(Stream strm, Document document, Transaction transaction)
		{
			// should be used when creating a new one; i.e. importing from disk
			m_Data = (SharedImage)document.AddSharedResourceFromStream(strm, transaction);
			document.AddSharedResource(m_Data.Content);
			//m_Image = m_Data.Content.GetImage();
			m_ImageSize = m_Data.Content.GetSize();
			SetDefaultSize();
		}

		public void PlaceAt(PointF centre)
		{
			// places this image as if it has just been pasted onto the page
			float height = m_ImageSize.Height / NOMINALPIXELSPERMILLIMETRE;
			float width = m_ImageSize.Width / NOMINALPIXELSPERMILLIMETRE;
			SetRectangle(new RectangleF(centre.X - width / 2, centre.Y - height / 2, width, height));
			//Debug.WriteLine(m_Bounds)
			m_PositionFixed = true;
			m_SizeAcceptable = true;
		}

		private const int MAXIMUMDEFAULTSIZE = 400; // in pixels
		protected void SetDefaultSize(float ratio = 1)
		{
			float scale = 1;
			if (m_ImageSize.Width > MAXIMUMDEFAULTSIZE || m_ImageSize.Height > MAXIMUMDEFAULTSIZE)
				scale = 400f / Math.Max(m_ImageSize.Width, m_ImageSize.Height);
			scale *= ratio;
			SetRectangle(new RectangleF(Vertices[0], new SizeF(scale * m_ImageSize.Width / NOMINALPIXELSPERMILLIMETRE, scale * m_ImageSize.Height / NOMINALPIXELSPERMILLIMETRE)));
			m_SizeAcceptable = true;
		}

		#endregion

		#region Information

		public override Shapes ShapeCode => Shapes.Image;

		public override SnapModes SnapNext(SnapModes requested)
		{
			if (!m_PositionFixed)
				return SnapModes.Off;
			if (requested == SnapModes.Angle)
				return SnapModes.Off; // doesn't make much sense placing bottom right to adjust angle
			return requested; // do not try and snap the size of the image
		}

		public override AllowedActions Allows
		{ get { return (base.Allows | AllowedActions.TransformRotate  | AllowedActions.MirrorFlipOnly) & ~(AllowedActions.TransformMirror | AllowedActions.Merge); } }

		public override void Diagnostic(System.Text.StringBuilder output)
		{
			base.Diagnostic(output);
			output.AppendLine("Shared image ID: " + (m_Data?.ID ?? Guid.Empty));
			if (m_Data == null)
				output.AppendLine("Image missing!");
			else
				output.AppendLine("SharedBitmap present");
		}

		#endregion

		#region Verbs
		private bool m_PositionFixed = false; // true once the top left position has been fixed
											  // not applicable after the image has been added to the page - this is not specifically set if the image is loaded from a file, for example
		private bool m_SizeAcceptable = false;
		// the minimum size for the image is usually a fraction of the image, but only within a certain range
		// much below a couple of pixels is always rejected; and very large images can always be scaled-down to a couple of centimetres
		private const float ABSOLUTEMINIMUMSIZE = 1;
		private const float ALWAYSALLOWEDSIZE = 20; // 2 cm
		private const float MINIMUMRATIO = 0.1f; // otherwise we impose a limit of 10% scale

		public override VerbResult Start(EditableView.ClickPosition position)
		{
			//Debug.Assert(m_Image != null);
			float height = m_ImageSize.Height / NOMINALPIXELSPERMILLIMETRE;
			float width = m_ImageSize.Width / NOMINALPIXELSPERMILLIMETRE;
			// if the image will occupy most of the page in either direction we force the user to set the scale
			SingleClickPlacement = height < position.Page.Size.Height * 0.7 && width < position.Page.Size.Width * 0.7;
			if (SingleClickPlacement)
				SetRectangle(new RectangleF(Vertices[0], new SizeF(width, height))); // SetSefaultSize may have made this smaller
			Float(position);
			return VerbResult.Continuing;
		}

		public override VerbResult Cancel(EditableView.ClickPosition position)
		{
			if (!m_PositionFixed)
				return VerbResult.Destroyed;
			m_PositionFixed = false;
			SetDefaultSize();
			return VerbResult.Continuing;
		}

		public override VerbResult Choose(EditableView.ClickPosition position)
		{
			if (SingleClickPlacement)
			{
				PlaceAt(position.Snapped);
				return VerbResult.Completed;
			}
			Float(position);
			if (m_PositionFixed)
				return !m_SizeAcceptable ? VerbResult.Rejected : VerbResult.Completed;
			m_PositionFixed = true;
			// jump mouse to bottom right...
			Parent.NotifyIndirectChange(this, ChangeAffects.MoveMouse, new RectangleF(Vertices[2], SizeF.Empty));
			return VerbResult.Continuing;
		}

		public override VerbResult Complete(EditableView.ClickPosition position)
		{
			Float(position);
			return CompleteRetrospective();
		}

		public override VerbResult CompleteRetrospective()
		{
			// this resets to the default size because we offer the user the choice of Complete to keep the original size
			// or pressing Choose again to resize
			m_PositionFixed = true;
			SetDefaultSize();
			return VerbResult.Completed;
		}

		public override VerbResult Float(EditableView.ClickPosition position)
		{
			PointF pt = position.Snapped;
			if (!m_PositionFixed)
			{
				var sz = GetSize();
				if (SingleClickPlacement)
				{
					// In this case we are placing the centre
					pt.X -= m_ImageSize.Width / NOMINALPIXELSPERMILLIMETRE * 0.5f;
					pt.Y -= m_ImageSize.Height / NOMINALPIXELSPERMILLIMETRE * 0.5f;
					//Debug.WriteLine(m_Bounds)
				}
				SetRectangle(new RectangleF(pt, sz));
			}
			else
			{
				SizeF newSize = new SizeF(pt.X - Vertices[0].X, pt.Y - Vertices[0].Y);
				float imageRatio = (float)m_ImageSize.Width / m_ImageSize.Height;
				float adjustment = imageRatio / (newSize.Width / newSize.Height);
				// correction needed to the aspect ratio
				// we add on half each on width and height
				newSize.Width *= (float)Math.Sqrt(adjustment);
				newSize.Height /= (float)Math.Sqrt(adjustment);
				m_SizeAcceptable = DimensionAcceptable(newSize.Width, m_ImageSize.Width) && DimensionAcceptable(newSize.Height, m_ImageSize.Height);
				if (m_SizeAcceptable)
					SetRectangle(new RectangleF(Vertices[0], newSize));
			}
			return VerbResult.Continuing;
		}

		private bool DimensionAcceptable(float newData, float imagePixels)
		{
			if (newData < ABSOLUTEMINIMUMSIZE)
				return false;
			if (newData >= ALWAYSALLOWEDSIZE)
				return true;
			return newData >= imagePixels * MINIMUMRATIO / NOMINALPIXELSPERMILLIMETRE;
		}

		/// <summary>Only valid when placing a shape (at which point it will be orthogonal)</summary>
		protected SizeF GetSize()
		{
			Debug.Assert(Status != StatusValues.Complete);
			return new SizeF(Vertices[1].X - Vertices[0].X, Vertices[3].Y - Vertices[0].Y);
		}

		#endregion

		#region Coordinate calculations, GrabSpots

		public override List<GrabSpot> GetGrabSpots(float scale)
		{
			List<GrabSpot> list = new List<GrabSpot>();
			base.AddBoundingGrabSpots(list, scale);
			base.AddStandardRotationGrabSpot(list);
			return list;
		}

		#endregion

		#region Load/Save/CopyFrom
		public override void CopyFrom(Datum other, CopyDepth depth, Mapping mapID)
		{
			base.CopyFrom(other, depth, mapID);
			ImportedImage imported = (ImportedImage)other;
			m_Bounds = imported.m_Bounds;
			m_Flip = imported.m_Flip;
			m_PositionFixed = imported.m_PositionFixed;
			m_SizeAcceptable = imported.m_SizeAcceptable;
			// image data never needs to be duplicated, it is always shared
			m_Data = imported.m_Data?.Clone();
			m_ImageSize = imported.m_ImageSize;
		}

		public override void Load(DataReader reader)
		{
			base.Load(reader);
			m_Data = new SharedReference<SharedImage> { ID = reader.ReadGuid() };
			m_OriginalName = reader.ReadString();
			m_Flip = reader.ReadSize();
		}

		public override void Save(DataWriter writer)
		{
			base.Save(writer);
			writer.Write(m_Data?.ID ?? Guid.Empty);
			writer.Write(m_OriginalName);
			writer.Write(m_Flip);
		}

		public override void AddRequiredReferences(Action<Datum> fnAdd, Mapping mapID)
		{
			base.AddRequiredReferences(fnAdd, mapID);
			if (m_Data?.Content != null)
				fnAdd.Invoke(m_Data.Content);
		}

		public override void UpdateReferencesObjectsCreated(Document document, DataReader reader)
		{
			base.UpdateReferencesObjectsCreated(document, reader);
			if (m_Data != null)
			{
				m_Data.DereferenceOnLoad(document);
				//m_Image = m_Data.Failed ? Resources.AM.Missing_128 : m_Data.Content.GetImage();
				m_ImageSize = m_Data.Failed ? new Size(64, 64) : m_Data.Content.GetSize();
			}
			//if (m_Data == null && !m_idData.IsEmpty())
			//{
			//	// first condition is needed because this might have been loaded from an old file, in which case the data will already be set
			//	m_Data = objDocument.FindExistingSharedBitmap(m_idData);
			//	if (m_Data != null)
			//		m_Image = m_Data.GetImage();
			//	else
			//	{
			//		Misc.LogSubError("Cannot find shared image: " + m_idData);
			//		m_Image = Resources.AM.Missing_128;
			//	}
			//}
		}

		public override void UpdateReferencesIDsChanged(Mapping mapID, Document document)
		{
			base.UpdateReferencesIDsChanged(mapID, document);
			m_Data?.UpdateIDsReferencesChanged();
		}

		#endregion

		#region Graphics and returning contained image

		protected override void InternalDraw(Canvas gr, DrawResources resources)
		{
			if (resources.Highlight)
			{
				using (var highlight = resources.Graphics.CreateStroke(CurrentHighlightColour, resources.HIGHLIGHTEXTRAWIDTH))
				{
					gr.DrawLines(Vertices, highlight);
					gr.DrawLine(Vertices[3], Vertices[0], highlight);
					//gr.DrawRectangleF(pnHighlight, m_Bounds)
				}

			}
			else
			{
				RectangleF source = new RectangleF(PointF.Empty, m_ImageSize);
				if (!m_Data.Content.IsLightweight && !m_Data.Content.MemoryImage.IsSVG)
				{// can't remember why, but it was important to specify pixles - but can only apply to actual .net images (not resources or SVG)
					var temp = GraphicsUnit.Pixel;
					source = m_Data.Content.GetNetImage().GetBounds(ref temp);
				}
				if (m_Flip.Width == -1)
				{
					source.X += m_ImageSize.Width;
					source.Width = -m_ImageSize.Width;
				}
				if (m_Flip.Height == -1)
				{
					source.Y += m_ImageSize.Height;
					source.Height = -m_ImageSize.Height;
				}
				if (m_Data.Failed)
				{
					using (var red = resources.Graphics.CreateStroke(Color.Red))
					{
						gr.DrawLine(Vertices[0], Vertices[2], red);
						gr.DrawLine(Vertices[1], Vertices[2], red);
					}
				}
				else if (resources.FillAlpha < 1150 && !Utilities.Low_Graphics_Safe())
					gr.DrawImage(m_Data.Content.MemoryImage, new[] { Vertices[0], Vertices[1], Vertices[3] }, source, GUIUtilities.GetImageAttrForAlpha(resources.FillAlpha));
				else if (IsOrthogonal())
					//Faster to use this version
					gr.DrawImage(m_Data.Content.MemoryImage, Bounds, source);
				else
					gr.DrawImage(m_Data.Content.MemoryImage, new[] { Vertices[0], Vertices[1], Vertices[3] }, source);
			}
		}

		public MemoryImage GetImage()
		{
			// note this only used on support menu - so not critical
			return m_Data?.Content?.MemoryImage;
		}

		public string GetEstimatedFileExtention()
		{
			// note this only used on support menu - so not critical
			if (!string.IsNullOrEmpty(m_OriginalName))
				return Path.GetExtension(m_OriginalName);
			return ".png";
		}

		#endregion

	}
}
