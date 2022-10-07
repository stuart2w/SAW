using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using SAW.Functions;
using Action = SAW.Functions.Action;
using SAW.Shapes;

namespace SAW
{
	internal static class ContextMenuTools
	{

		#region creating and initialising for first time
		/// <summary>Sets tag and text based on an action deduced from initial tag OR text </summary>
		public static void InitialiseMenu(MenuStrip strip)
		{
			foreach (ToolStripMenuItem item in strip.Items)
			{
				InitialiseMenu(item);
			}
		}

		/// <summary>Sets tag and text based on an action deduced from initial tag OR text </summary>
		public static void InitialiseMenu(ContextMenuStrip strip)
		{
			foreach (ToolStripMenuItem item in strip.Items.OfType<ToolStripMenuItem>())
			{
				InitialiseMenu(item);
			}
		}

		/// <summary>Sets tag and text based on an action deduced from initial tag OR text </summary>
		public static void InitialiseMenu(ToolStripMenuItem item)
		{
			string tag = null;
			if (item.Tag is string stringTag && stringTag != "")
				tag = stringTag;
			else if (item.Text.StartsWith("[Verb_") && item.Text.EndsWith("]")) // if text is a translation code for a verb, then the tag is inferred
				tag = item.Text.Substring(1, item.Text.Length - 2).Replace("Verb_", "Verb/");
			if (!string.IsNullOrEmpty(tag))
			{
				try
				{
					if (tag.Contains("/"))
					{
						string[] parts = tag.Split('/');
						if (string.Compare(parts[0], "verb", true) == 0)
						{
							Codes code = (Codes)Enum.Parse(typeof(Codes), parts[1]);
							Action action = Verb.Find(code);
							item.Tag = action;
							item.Image = action.CreateSampleImage(16);
							item.Text = Strings.Translate(item.Text); // would usually happen anyway, but this is run for ctx after main translation
						}
						else
						{
							// is a parameter
							Parameters parameter = (Parameters)Enum.Parse(typeof(Parameters), parts[0]);
							if (parameter == Parameters.Tool)
							{
								// second parameter can be parsed as a shape
								Shape.Shapes eShape = (Shape.Shapes)Enum.Parse(typeof(Shape.Shapes), parts[1]);
								item.Tag = new ToolAction(eShape);
							}
							else
								item.Tag = Action.Create(parameter, Utilities.IntVal(parts[1]));
						}
						item.Click += Menu_Select;
					}
				}
				catch (Exception ex)
				{
					item.Text += " (FAILED)";
					Debug.WriteLine("Parsing of menu tag (" + tag + ") failed: " + ex.Message);
				}
			}
			if (item.DropDownItems.Count > 0)
				item.DropDownOpening += Menu_Open;
			foreach (var subItem in item.DropDownItems.OfType<ToolStripMenuItem>())
			{
				InitialiseMenu(subItem);
			}
		}

		public static bool DuplicateMenu(ToolStripItemCollection entries, ToolStripItemCollection intoCollection)
		{
			// returns true is any invalid items were added
			bool added = false;
			bool allowSeparator = false; // only allowed after we have had something valid
			foreach (ToolStripItem entry in entries)
			{
				if (entry is ToolStripMenuItem)
				{
					if (entry.Tag != null && !((Action)entry.Tag).IncludeOnContextMenu)
						continue;
					ToolStripMenuItem menu = (ToolStripMenuItem)entry;
					ToolStripMenuItem create = new ToolStripMenuItem() { Text = entry.Text, Image = entry.Image, Tag = entry.Tag };
					if (menu.DropDownItems.Count > 0)
					{
						if (!DuplicateMenu(menu.DropDownItems, create.DropDownItems))
						{
							// If a header has no valid subitems we want to add, then the header should not be added to the new menu
							create.Dispose();
							continue;
						}
						create.DropDownOpening += Menu_Open;
					}
					intoCollection.Add(create);
					added = true;
					if (entry.Tag is Action)
						create.Click += Menu_Select;
					allowSeparator = true;
				}
				else if (entry is ToolStripSeparator)
				{
					if (allowSeparator)
						intoCollection.Add(new ToolStripSeparator());
					allowSeparator = false;
				}
				else
					Debug.Fail("Unexpected ToolStrip entry type in DuplicateMenu: " + entry.GetType());
			}
			if (intoCollection.Count > 0 && intoCollection[intoCollection.Count - 1] is ToolStripSeparator)
				intoCollection.RemoveAt(intoCollection.Count - 1); // don't allow a separate at the end
			return added;
		}

		#endregion

		#region Updating state before display

		public static void UpdateMenuKeys(ToolStripMenuItem item)
		{
			try
			{
				foreach (ToolStripItem sub in item.DropDownItems)
				{
					if (sub is ToolStripMenuItem menuItem)
					{
						if (menuItem.Tag is Action action)
						{
							if (!action.IsEmpty)
							{
								Keys key = Globals.Root.CurrentConfig.GetFirstKeyForAction(action);
								if (key != Keys.None)
									menuItem.ShortcutKeyDisplayString = GUIUtilities.KeyDescription(key);
								else
									menuItem.ShortcutKeyDisplayString = "";
							}
						}
					}
					if (!(sub is ToolStripSeparator))
						UpdateMenuKeys(sub as ToolStripMenuItem);
				}
			}
			catch (Exception ex) // otherwise errors here can stop the software from starting up, but silently
			{
				Utilities.LogSubError(ex);
			}
		}

		/// <summary>Updated state of menu if it is about to be displayed </summary>
		public static void PrepareContextMenu(ContextMenuStrip mnu)
		{
			// This is roughly copied from frmMain.Menu_Open, but modified.  (Not easy to share the code as the menu is to not have a shared base type with the Items collection)
			// This is only used for the top-level menu.  Menu_Open is triggered for submenus and works as normal
			foreach (ToolStripMenuItem sub in mnu.Items.OfType<ToolStripMenuItem>())
			{
				if (sub.Tag is Action tag)
				{
					bool applicable = Globals.ActionApplicable(tag);
					sub.Enabled = applicable; // The enabled flag is set even if the item is hidden (it's easier this way, and makes no difference)
					sub.Visible = (applicable || !tag.HideFromContextMenuIfUnavailable) && !string.IsNullOrWhiteSpace(sub.Text); // last condition needed for DoubleClick entry - which can have empty text meaning it is available, but omitted from menu
					if (tag is ParameterAction parameterAction) // i.e. genuine parameter
						sub.Checked = Globals.ParameterValue(parameterAction.Change) == parameterAction.Value;
				}
			}
		}

		public static void PrepareEditMenu(ToolStripItemCollection items)
		{
			// used both by the edit menu on the main bar and the context menu to do the custom updates.  Menu_Open will also be triggered to do automatic things
			// At the moment this doesn't iterate to the submenus as there is no custom behaviour on them
			foreach (ToolStripMenuItem mnu in items.OfType<ToolStripMenuItem>()) // OfType excludes the dividers
			{
				Verb action = (mnu.Tag as Verb); // note might be other Action not verb, which deliberately fails below and skips
				if (action == null)
					continue; // actually I think they are all verbs
				switch (action.Code)
				{
					case Codes.DoubleClick:
						string text = "";
						List<Shape> selection = Globals.Root.CurrentPage.SelectedShapes;
						if (selection.Count >= 1 && selection.First().CanDoubleClickWith(selection))
							text = selection.First().DoubleClickText();
						mnu.Text = text;
						mnu.Visible = !string.IsNullOrEmpty(text);
						break;
					case Codes.ConvertToPath:
					case Codes.CCFUpdate:
						// these are rare enough to hide unless allowed (actually only appears on the main edit menu)
						mnu.Visible = Globals.VerbApplicable(action.Code);
						break;
					case Codes.PageNext: // from the page menu
						if (Globals.Root.CurrentPageIndex >= Globals.Root.CurrentDocument.Count - 1)
						{
							mnu.Text = Strings.Item("Menu_PageCreateNext");
							mnu.Image = Resources.AM.PageNew16;
						}
						else
						{
							mnu.Text = Strings.Item("Menu_PageNext");
							mnu.Image = Resources.AM.Verb_16_PageNext;
						}
						break;
				}
			}
		}

		#endregion

		/// <summary>Handler for sub-menu opening, which updates items</summary>
		private static void Menu_Open(object sender, EventArgs e)
		{
			// we need to go through any items which refer to parameters and tick them
			foreach (var sub in ((ToolStripMenuItem)sender).DropDownItems.OfType<ToolStripMenuItem>())
			{
				if (sub.Tag is Action)
				{
					Action tag = (Action)sub.Tag;
					sub.Enabled = Globals.ActionApplicable(tag);
					if (tag is ParameterAction)// i.e. genuine parameter
						sub.Checked = Globals.ParameterValue(tag.Change) == (tag as ParameterAction).Value;
				}
			}
		}

		/// <summary>This is called when any menu item which had a tag specifying the action is selected</summary>
		public static void Menu_Select(object sender, EventArgs e)
		{
			Action tag = (Action)((ToolStripItem)sender).Tag;
			if (tag.IsEmpty)
				return;
			Globals.PerformAction(tag, ClickPosition.Sources.Mouse);
		}


	}
}