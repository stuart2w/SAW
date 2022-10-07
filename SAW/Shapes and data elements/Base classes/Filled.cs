using System;
using System.Collections.Generic;
using System.Drawing;

namespace SAW.Shapes
{
	public abstract class Filled : Lined
	{
		// base class for any shape which can be filled with a colour

		protected Filled()
		{
			FillStyle = new FillStyleC();
		}

		public override VerbResult Start(ClickPosition position)
		{
			FillStyle.SetDefaults();
			return base.Start(position);
		}

		#region Drawing

		protected override void PrepareDraw(DrawResources resources)
		{
			base.PrepareDraw(resources);
			DefaultPrepareFillDraw(FillStyle, resources, HasText(true));
		}

		internal static void DefaultPrepareFillDraw(FillStyleC fillStyle, DrawResources resources, bool hasText)
		{
			// note ONLY does this level - also need to call MyBase.PrepareDraw or similar
			Color colour = fillStyle.Colour;
			if (resources.FillAlpha != 255)
				colour = Color.FromArgb(resources.FillAlpha * fillStyle.Colour.A / 256, fillStyle.Colour);
			switch (fillStyle.Pattern)
			{
				case FillStyleC.Patterns.Solid:
					resources.MainBrush = resources.Graphics.CreateFill(colour);
					break;
				case FillStyleC.Patterns.Empty:
					resources.MainBrush = null;
					break;
				case FillStyleC.Patterns.Texture:
					if (resources.FillAlpha >= 128 || hasText == false)
					{
						// note: textures not drawn if v transparent (cos can't draw them transparent at all) and has text
						// this means they don't draw in the selection layer for example
						resources.MainBrush = resources.Graphics.CreateTextureBrush(fillStyle.Texture, Geometry.INCH / 92);
					}
					break;
				default:
					resources.MainBrush = resources.Graphics.CreateHatchBrush(colour, Color.Empty, fillStyle.Pattern);
					break;
			}
		}

		protected override Color GetDefaultTextColour()
		{
			if (FillStyle == null)
				return Color.Black;
			return TextStyleC.DefaultTextColourOn(FillStyle.Colour);
		}

		#endregion

		#region Basic information

		/// <summary>Styling information for the background of the shape - mainly the colour, although patterns or textures may be supported</summary>
		public FillStyleC FillStyle;

		/// <summary>True if shape actually draws a non-transparent background (can vary)</summary>
		public override bool IsFilled => FillStyle.Colour.A > 0;

		internal override StyleBase StyleObjectForParameter(Parameters parameter, bool applyingDefault = false)
		{
			switch (parameter)
			{
				case Parameters.FillColour:
				case Parameters.FillPattern:
					return FillStyle;
				default:
					return base.StyleObjectForParameter(parameter, applyingDefault);
			}
		}

		public override AllowedActions Allows => (base.Allows & ~AllowedActions.Arrowheads) | AllowedActions.PermittedArea;

		internal override void AddToFlatList(List<Shape> list, FlatListPurpose purpose)
		{
			if (purpose == FlatListPurpose.HasTexture)
			{
				// will be ignored by base class
				if (FillStyle.Texture != null)
					list.Add(this);
			}
			else
				base.AddToFlatList(list, purpose);
		}

		#endregion

		#region Datum and Copy
		protected internal override void Load(DataReader reader)
		{
			base.Load(reader);
			FillStyle = FillStyleC.Read(reader);
		}

		protected internal override void Save(DataWriter writer)
		{
			base.Save(writer);
			FillStyle.Write(writer);
		}

		public override void CopyFrom(Datum other, CopyDepth depth, Mapping mapID)
		{
			base.CopyFrom(other, depth, mapID);
			Filled filled = (Filled)other;
			if (depth >= CopyDepth.Undo)
			{
				if (FillStyle == null || FillStyle == filled.FillStyle)
					FillStyle = new FillStyleC();
				FillStyle.CopyFrom(filled.FillStyle);
			}
			else
			{
				if (FillStyle == null)
					FillStyle = filled.FillStyle;
			}
		}

		// note similar stuff in ButtonStyle
		protected internal override void AddRequiredReferences(Action<Datum> fnAdd, Mapping mapID)
		{
			base.AddRequiredReferences(fnAdd, mapID);
			FillStyle?.AddReferences(fnAdd, mapID);
		}

		protected internal override void UpdateReferencesObjectsCreated(Document document, DataReader reader)
		{
			base.UpdateReferencesObjectsCreated(document, reader);
			FillStyle?.UpdateReferencesObjectsCreated(document, reader);
		}

		//public override void Iterate(DatumFunction fn)
		//{
		//	base.Iterate(fn);
		//}

		protected override bool IdenticalToShape(Shape other)
		{
			if (!base.IdenticalToShape(other))
				return false;
			Filled filled = (Filled)other;
			if (FillStyle == null != (filled.FillStyle == null))
				return false;
			if (FillStyle != null)
			{
				if (!FillStyle.IdenticalTo(filled.FillStyle))
					return false;
			}
			return true;
		}
		#endregion

	}
}