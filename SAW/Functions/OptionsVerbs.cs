namespace SAW.Functions
{
	internal static class OptionsVerbs
	{
		public static void RegisterVerbs()
		{
			Verb.Register(Codes.Zoom100, new Zoom() { Value = 1 });
			Verb.Register(Codes.ZoomIn, new Zoom() { Multiplier = StaticView.ZOOMSTEP });
			Verb.Register(Codes.ZoomOut, new Zoom() { Multiplier = 1 / StaticView.ZOOMSTEP });
			Verb.Register(Codes.ZoomPage, new Zoom() { Value = (float)StaticView.SpecialZooms.FitPage });
			Verb.Register(Codes.ZoomWidth, new Zoom() { Value = (float)StaticView.SpecialZooms.FitWidth });
			Verb.Register(Codes.ConfigUser, (source, pnlView, tx) => { frmEditConfig.EditConfig(Config.Levels.User, Globals.Root.CurrentDocument); }).MightOpenDialogValue = true; 
			Verb.Register(Codes.StoreStylesAsDefault, (source, pnlView, tx) => { Globals.Root.Editor.StoreStyleDefaults(true, tx); }, true, pnlView => !Globals.Root.CurrentDocument.IsPaletteWithin);
			Verb.Register(Codes.StoreStylesAsUserDefault, (source, pnlView, tx) => { Globals.Root.Editor.StoreStyleDefaults(false, tx); });
			Verb.Register(Codes.ResetPalettes, (source, pnlView, tx) => { Globals.Root.Editor.ResetPalettes(); });
			Verb.Register(Codes.TogglePrompts, new TogglePrompts());
			Verb.Register(Codes.MovePalette, (source, pnlView, tx) => { Globals.Root.Editor.FocalPaletteContainer()?.PaletteVerb(Codes.MovePalette); });
			Verb.Register(Codes.ResizePalette, (source, pnlView, tx) => { Globals.Root.Editor.FocalPaletteContainer()?.PaletteVerb(Codes.ResizePalette); });
		}

	}

	internal class Zoom : Verb
	{
		/// <summary>If this is non-zero it is used, otherwise value is used </summary>
		public float Multiplier = 0;
		public float Value = 1;
		public override void Trigger(EditableView.ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			pnlView.ChangeZoom(Multiplier != 0 ? pnlView.Zoom * Multiplier : Value);
			Globals.NotifyVerbApplicabilityChanged();
		}

		public override bool AbandonsCurrent
		{ get { return false; } }

		public override bool IsApplicable(EditableView pnlView)
		{
			switch (Code)
			{
				case Codes.Zoom100:
					return pnlView.Zoom != 1;
				case Codes.ZoomIn:
					return pnlView.Zoom < StaticView.MAXIMUMZOOM || Globals.Root.IsDebug;
				// if diagnostic panel visible there is no limit to zoom - now always in DEBUG
				case Codes.ZoomOut:
					return pnlView.Zoom > StaticView.MINIMUMZOOM;
				default: return true;
			}
		}

	}

	internal class TogglePrompts : Verb
	{
		public override void Trigger(EditableView.ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			bool current = Globals.Root.CurrentConfig.ReadBoolean(Config.SAW_Prompts, true);
			transaction.Edit(CurrentDocument.UserSettings);
			transaction.Edit(CurrentDocument.BothSettings);
			CurrentDocument.UserSettings.RemoveValue(Config.SAW_Prompts); // Just in case because this would override the other one:
			CurrentDocument.BothSettings.Write(Config.SAW_Prompts, !current);
		}
	}

	internal class ConfigDocument : Verb
	{
		public override void Trigger(EditableView.ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			CurrentDocument.GetCreateUserSettings(transaction);
			frmEditConfig.EditConfig(Config.Levels.DocumentUser, CurrentDocument);
		}

		public override bool IsApplicable(EditableView pnlView) =>  !Globals.Root.CurrentDocument.IsPaletteWithin;
		public override bool MightOpenModalDialog => true;
	}
}
