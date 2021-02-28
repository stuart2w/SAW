<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="History.aspx.cs" Inherits="WebServer.History" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>SAW - Version history</title>
</head>
<body>
	<form id="form1" runat="server">
		<h2>Version history</h2>
		<h3>7.02.2</h3>
		<ul>
			<li>Minor fixes</li>
			<li>Also error report to server fixed - the new server had been failing to accept them</li>
		</ul>
		<h3>7.02.1</h3>
		<ul>
			<li>Fixes to key output handling</li>
			<li>Changed behaviour of system menu (top left) in run mode</li>
		</ul>
		<h3>7.02.0</h3>
		<ul>
			<li>Added Swedish word prediction lexicon to the installer</li>
			<li>Added buttons in item editor to shrink/expand current item(s)</li>
		</ul>
		<h3>7.01.9</h3>
		<ul>
			<li>Added buttons in item editor to adjust position of selected item(s)</li>
		</ul>
		<h3>7.01.8</h3>
		<ul>
			<li>Grid and background screen redesigned.  New background mode added that locks screen A:R to that of image so that buttons stay in the same position relative to the background even if the set is resized</li>
			<li>Added functions on control+left/right cursor keys to move the selected item in or out of the nesting hierarchy.  Also control+up/down now move it up or down list of items within it's current container.</li>
		</ul>
		<h3>7.01.5</h3>
		<ul>
			<li>Added option to show dotted border when editing for any invisible items</li>
			<li>Added snap-to-shape and edit paper (background + grid) buttons in toolbox</li>
			<li>Added configuration deltas on installation (can push through setting changes after an update)</li>
			<li>Improvements to copy-paste semantics</li>
			<li>Improvements to handling of &lt; when NOT intended to introduce a special code in keyboard output text</li>
			<li>Added option that the graphic on a button is shown only when the button is highlighted/visited</li>
		</ul>
		<h2>Version 7</h2>
		<div>
			Version 7 is a complete rewrite of SAW 6 in more modern programming languages.
			<br />
			Although this broadly replicated SAW6, the UI has also been modernised significantly, and some functionality added
		</div>
		<div>7.01.4 was the first prototype that was functionally complete. Details of prototypes up to this point are not recorded.</div>
	</form>
</body>
</html>
