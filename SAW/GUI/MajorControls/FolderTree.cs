using System;
using System.Drawing;
using System.Diagnostics;
using System.Collections;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;

namespace SAW
{
	[ToolboxBitmap(typeof(FolderTree), "tree.gif"), System.ComponentModel.DefaultEvent("PathChanged")]
	public class FolderTree : UserControl
	{
		public event EventHandler PathChanged;

		private bool m_ShowMyDocuments = true;
		private bool m_ShowMyFavorites = true;
		private bool m_ShowMyNetwork = true;
		private TreeNode m_MyComputerNode; // is not actually the root node - which is the desktop

		public string SelectedPath { get; private set; } = "";

		#region Component Designer generated code
		private System.Windows.Forms.ImageList imageList1;
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.TreeView tvwMain;

		public FolderTree()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.Load += new System.EventHandler(ExplorerTree_Load);
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FolderTree));
			this.tvwMain = new System.Windows.Forms.TreeView();
			this.tvwMain.AfterExpand += new System.Windows.Forms.TreeViewEventHandler(this.tvwMain_AfterExpand);
			this.tvwMain.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvwMain_AfterSelect);
			this.imageList1 = new System.Windows.Forms.ImageList(this.components);
			this.SuspendLayout();
			//
			//tvwMain
			//
			this.tvwMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tvwMain.ImageIndex = 0;
			this.tvwMain.ImageList = this.imageList1;
			this.tvwMain.Location = new System.Drawing.Point(0, 0);
			this.tvwMain.Name = "tvwMain";
			this.tvwMain.SelectedImageIndex = 0;
			this.tvwMain.Size = new System.Drawing.Size(240, 336);
			this.tvwMain.TabIndex = 1;
			//
			//imageList1
			//
			this.imageList1.ImageStream = (System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream"));
			this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
			this.imageList1.Images.SetKeyName(0, "Drive");
			this.imageList1.Images.SetKeyName(1, "Folder");
			this.imageList1.Images.SetKeyName(2, "FolderOpen");
			this.imageList1.Images.SetKeyName(3, "CDDrive");
			this.imageList1.Images.SetKeyName(4, "NetworkDrive");
			this.imageList1.Images.SetKeyName(5, "MyDocuments");
			this.imageList1.Images.SetKeyName(6, "Desktop");
			this.imageList1.Images.SetKeyName(7, "MyComputer");
			this.imageList1.Images.SetKeyName(8, "MyNetworkPlaces");
			this.imageList1.Images.SetKeyName(9, "EntireNetwork");
			this.imageList1.Images.SetKeyName(10, "NetworkNode");
			this.imageList1.Images.SetKeyName(11, "Network");
			this.imageList1.Images.SetKeyName(12, "FloppyDrive");
			this.imageList1.Images.SetKeyName(13, "MyFavourites");
			this.imageList1.Images.SetKeyName(14, "SharedFolder");
			//
			//FolderTree
			//
			this.BackColor = System.Drawing.Color.White;
			this.Controls.Add(this.tvwMain);
			this.Name = "FolderTree";
			this.Size = new System.Drawing.Size(240, 336);
			this.ResumeLayout(false);

		}
		#endregion

		private void ExplorerTree_Load(object sender, EventArgs e)
		{
			if (DesignMode)
				return;
			PopulateTree();
			SelectedPath = "";
		}

		// names for dummy nodes placed within parents to ensure that the parent shows the expansion icon.  These are placed as parent is expanded
		private const string ENTIRE_NETWORK_DUMMY = "Network Node";
		private const string COMPUTER_DUMMY = "my Node";
		private const string NETWORK_DUMMY = "my netNode";

		private void PopulateTree()
		{
			tvwMain.Nodes.Clear();

			TreeNode node;
			TreeNode desktop = new TreeNode
			{
				Tag = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
				Text = Strings.Item("Desktop"),
				ImageKey = "Desktop",
				SelectedImageKey = "Desktop"
			};
			tvwMain.Nodes.Add(desktop);

			if (m_ShowMyDocuments)
			{
				//Add My Documents and Desktop folder outside
				node = new TreeNode
				{
					Tag = Environment.GetFolderPath(Environment.SpecialFolder.Personal),
					Text = Strings.Item("My_Documents"),
					ImageKey = "MyDocuments",
					SelectedImageKey = "MyDocuments"
				};
				desktop.Nodes.Add(node);
				GetDirectories(node);
			}

			m_MyComputerNode = new TreeNode
			{
				Tag = "My Computer",
				Text = Strings.Item("My_Computer"),
				ImageKey = "My Computer",
				SelectedImageKey = "My Computer"
			};
			desktop.Nodes.Add(m_MyComputerNode);

			var dummy = new TreeNode(COMPUTER_DUMMY) { Tag = COMPUTER_DUMMY };
			m_MyComputerNode.Nodes.Add(dummy);

			if (m_ShowMyNetwork)
			{
				var myNetwork = new TreeNode
				{
					Tag = "My Network Places",
					Text = Strings.Item("My_Network_Places"),
					ImageKey = "MyNetworkPlaces",
					SelectedImageKey = "MyNetworkPlaces"
				};
				desktop.Nodes.Add(myNetwork);

				TreeNode entireNetwork = new TreeNode
				{
					Tag = "Entire Network",
					Text = Strings.Item("Entire_Network"),
					ImageKey = "EntireNetwork",
					SelectedImageKey = "EntireNetwork"
				};
				myNetwork.Nodes.Add(entireNetwork);

				dummy = new TreeNode
				{
					Tag = ENTIRE_NETWORK_DUMMY,
					Text = ENTIRE_NETWORK_DUMMY
				};
				entireNetwork.Nodes.Add(dummy);
			}

			if (m_ShowMyFavorites)
			{
				node = new TreeNode
				{
					Tag = Environment.GetFolderPath(Environment.SpecialFolder.Favorites),
					Text = Strings.Item("My_Favourites"),
					ImageKey = "MyFavourites",
					SelectedImageKey = "MyFavourites"
				};
				desktop.Nodes.Add(node);
				GetDirectories(node);
			}
			desktop.Expand();
			ExpandFileNode(desktop);
		}

		private void ExpandFileNode(TreeNode parent)
		{
			Cursor.Current = Cursors.WaitCursor;
			try
			{
				GetDirectories(parent);
				//get dirs one more level deep in current dir so user can see there is more dirs underneath current dir
				foreach (TreeNode node in parent.Nodes)
				{
					if (parent.Text != Strings.Item("Desktop"))
						GetDirectories(node);
				}
			}
			catch (Exception ex)
			{ Utilities.LogSubError(ex.Message); }
			finally
			{ Cursor.Current = Cursors.Default; }
		}

		private void GetDirectories(TreeNode parentNode)
		{
			// get the subdirectories, if this fails it is probably because the parent doesn't exist/is not available, so show the parent as empty
			string[] directories = null;
			try
			{
				directories = Directory.GetDirectories(parentNode.Tag.ToString());
				Array.Sort(directories);
				if (directories.Length == parentNode.Nodes.Count)
					return;
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
				parentNode.Nodes.Clear();
				return;
			}

			for (int index = 0; index <= directories.Length - 1; index++)
			{
				TreeNode node = new TreeNode
				{
					Tag = directories[index],               //store path in tag
					Text = Path.GetFileName(directories[index]),
					ImageKey = "Folder",
					SelectedImageKey = "Folder"
				};
				parentNode.Nodes.Add(node);
			}
		}

		private void tvwMain_AfterExpand(object sender, TreeViewEventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;
			TreeNode node = e.Node;
			if (node.Text.IndexOf(":", 1) > 0)
				ExpandFileNode(node);
			else if (node.Text == Strings.Item("Desktop"))
			{
				// is already expanded
			}
			else if (string.Compare((string)node.Tag, "My Network Places") == 0)
			{
				// is already expanded (?)
			}
			else if (string.Compare((string)node.Tag, "Microsoft Windows Network") == 0)
			{
				// is already expanded (?)
			}
			else if (node == m_MyComputerNode)
				ExpandMyComputer();
			else if (string.Compare((string)node.Tag, "Entire Network") == 0)
				ExpandEntireNetwork(node);
			else if (node.Parent != null && string.Compare((string)node.Parent.Tag, "Microsoft Windows Network") == 0)
				ExpandNetworkNode(node);
			else
				ExpandFileNode(node);
			Cursor.Current = Cursors.Default;
		}

		private void tvwMain_AfterSelect(object sender, TreeViewEventArgs e)
		{
			try
			{
				// ignore some special locations - it doesn't make sense to show files then these.  All these just report as path ""
				TreeNode node = e.Node;
				if (string.Compare((string)node.Tag, "My Network Places") == 0)
				{ }
				else if (string.Compare((string)node.Tag, "Microsoft Windows Network") == 0)
				{ }
				else if (node == m_MyComputerNode)
				{ }
				else if (string.Compare((string)node.Tag, "Entire Network") == 0)
				{ }
				else if (node.Parent != null && string.Compare((string)node.Parent.Tag, "Microsoft Windows Network") == 0)
				{ }
				else
				{
					SelectedPath = (string)node.Tag;
					PathChanged?.Invoke(this, EventArgs.Empty);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
			}
		}

		private void ExpandMyComputer()
		{
			if (m_MyComputerNode.GetNodeCount(true) >= 2)
				return; // is already expanded
			Cursor.Current = Cursors.WaitCursor;
			string[] drives = Environment.GetLogicalDrives();
			m_MyComputerNode.FirstNode.Remove();
			foreach (string drive in drives)
			{
				TreeNode nodeDrive = new TreeNode
				{
					Tag = drive,
					Text = drive
				};
				switch (Win32.GetDriveType(drive))
				{
					case 2: nodeDrive.ImageKey = "FloppyFive"; break;
					case 3: nodeDrive.ImageKey = "Drive"; break;
					case 4: nodeDrive.ImageKey = "NetworkDrive"; break;
					case 5: nodeDrive.ImageKey = "CDDrive"; break;
					default: nodeDrive.ImageKey = "Drive"; break;
				}
				nodeDrive.SelectedImageKey = nodeDrive.ImageKey;
				m_MyComputerNode.Nodes.Add(nodeDrive);
				// at the top level folders within the drive
				try
				{
					if (Directory.Exists(drive))
					{
						foreach (string directory in Directory.GetDirectories(drive))
						{
							TreeNode node = new TreeNode
							{
								Tag = directory,
								Text = directory.Substring(directory.LastIndexOf(Path.DirectorySeparatorChar) + 1),
								ImageKey = "Folder",
								SelectedImageKey = "Folder"
							};
							nodeDrive.Nodes.Add(node);
						}
					}
				}
				catch (Exception ex)
				{
					Utilities.LogSubError("Error while Filling the Explorer:" + ex.Message);
				}
			}
			m_MyComputerNode.Expand();
		}

		private void ExpandEntireNetwork(TreeNode network)
		{
			TreeNode nodeNN;
			if (network.FirstNode.Text != ENTIRE_NETWORK_DUMMY)
				return; // already expanded.  The dummy node is called Network Node
			network.FirstNode.Remove();

			ServerEnum servers = new ServerEnum(ResourceScope.RESOURCE_GLOBALNET, ResourceType.RESOURCETYPE_DISK, ResourceUsage.RESOURCEUSAGE_ALL, ResourceDisplayType.RESOURCEDISPLAYTYPE_NETWORK, "");

			foreach (string server in servers)
			{
				string name = server.Substring(0, server.IndexOf("|", 1));
				if (server.IndexOf("NETWORK", 1) > 0)
				{
					nodeNN = new TreeNode
					{
						Tag = name,
						Text = name,
						ImageKey = "NetworkNode",
						SelectedImageKey = "NetworkNode"
					};
					network.Nodes.Add(nodeNN);
				}
				else
				{
					TreeNode nodemN = new TreeNode
					{
						Tag = name,
						Text = name,
						ImageKey = "Network",
						SelectedImageKey = "Network"
					};
					network.LastNode.Nodes.Add(nodemN);

					TreeNode nodemNc = new TreeNode
					{
						Tag = NETWORK_DUMMY,
						Text = NETWORK_DUMMY,
						ImageKey = "MyComputer",
						SelectedImageKey = "MyComputer"
					};
					nodemN.Nodes.Add(nodemNc);
				}
			}

		}

		private void ExpandNetworkNode(TreeNode parent)
		{
			if (parent.FirstNode.Text != NETWORK_DUMMY)
				return; // already expanded
			parent.FirstNode.Remove();
			ServerEnum servers = new ServerEnum(ResourceScope.RESOURCE_GLOBALNET, ResourceType.RESOURCETYPE_DISK, ResourceUsage.RESOURCEUSAGE_ALL, ResourceDisplayType.RESOURCEDISPLAYTYPE_SERVER, parent.Text);
			foreach (string serverName in servers)
			{
				if (serverName.Length < 6 || string.Compare(serverName.Substring(serverName.Length - 6, 6), "-share") != 0)
				{
					string name = serverName; //.Substring(s1.IndexOf("\\",2))
					TreeNode serverNode = new TreeNode
					{
						Tag = serverName,
						Text = serverName.Substring(2),
						ImageKey = "MyComputer",
						SelectedImageKey = "MyComputer"
					};
					parent.Nodes.Add(serverNode);
					foreach (string sub in servers) // look for items to display within this server (?)
					{
						if (sub.Length > 6 && sub.Substring(sub.Length - 6, 6) == "-share" && name.Length <= sub.Length)
						{
							try
							{
								if (sub.Substring(0, name.Length + 1) == name + Path.DirectorySeparatorChar)
								{
									TreeNode folder = new TreeNode
									{
										Tag = sub.Substring(0, sub.Length - 6),
										Text = sub.Substring(name.Length + 1, sub.Length - name.Length - 7),
										ImageKey = "SharedFolder",
										SelectedImageKey = "SharedFolder"
									};
									serverNode.Nodes.Add(folder);
								}
							}
							catch
							{
							}
						}
					}
				}
			}
		}


		#region Windows stuff
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct SHQUERYRBINFO
		{
			public uint cbSize;
			public ulong i64Size;
			public ulong i64NumItems;
		}

		//Shell functions
		public class Win32
		{
			public const uint SHGFI_ICON = 0x100;
			public const uint SHGFI_SMALLICON = 1;

			[DllImport("shell32.dll")]
			public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

			[DllImport("kernel32")]
			public static extern uint GetDriveType(string lpRootPathName);

			[DllImport("shell32.dll")]
			public static extern bool SHGetDiskFreeSpaceEx(string pszVolume, ref ulong pqwFreeCaller, ref ulong pqwTot, ref ulong pqwFree);

			[DllImport("shell32.Dll")]
			public static extern int SHQueryRecycleBin(string pszRootPath, ref SHQUERYRBINFO pSHQueryRBInfo);

			[StructLayout(LayoutKind.Sequential)]
			public struct SHFILEINFO
			{
				public IntPtr hIcon;
				public IntPtr iIcon;
				public uint dwAttributes;
				[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
				public string szDisplayName;
				[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
				public string szTypeName;
			}

			[StructLayout(LayoutKind.Sequential)]
			public class BITMAPINFO
			{
				public int biSize;
				public int biWidth;
				public int biHeight;
				public short biPlanes;
				public short biBitCount;
				public int biCompression;
				public int biSizeImage;
				public int biXPelsPerMeter;
				public int biYPelsPerMeter;
				public int biClrUsed;
				public int biClrImportant;
				public int colors;
			}
			[DllImport("comctl32.dll")]
			public static extern bool ImageList_Add(IntPtr hImageList, IntPtr hBitmap, IntPtr hMask);
			//[DllImport("kernel32.dll")]
			//private static extern bool RtlMoveMemory(IntPtr dest, IntPtr source, int dwcount);
			[DllImport("shell32.dll")]
			public static extern IntPtr DestroyIcon(IntPtr hIcon);
			[DllImport("gdi32.dll")]
			public static extern IntPtr CreateDIBSection(IntPtr hdc, BITMAPINFO pbmi, uint iUsage, ref IntPtr ppvBits, IntPtr hSection, uint dwOffset);

		}

		public enum ResourceScope
		{
			RESOURCE_CONNECTED = 1,
			RESOURCE_GLOBALNET,
			RESOURCE_REMEMBERED,
			RESOURCE_RECENT,
			RESOURCE_CONTEXT
		}

		public enum ResourceType
		{
			RESOURCETYPE_ANY,
			RESOURCETYPE_DISK,
			RESOURCETYPE_PRINT,
			RESOURCETYPE_RESERVED
		}

		public enum ResourceUsage
		{
			RESOURCEUSAGE_CONNECTABLE = 0x1,
			RESOURCEUSAGE_CONTAINER = 0x2,
			RESOURCEUSAGE_NOLOCALDEVICE = 0x4,
			RESOURCEUSAGE_SIBLING = 0x8,
			RESOURCEUSAGE_ATTACHED = 0x10,
			RESOURCEUSAGE_ALL = RESOURCEUSAGE_CONNECTABLE | RESOURCEUSAGE_CONTAINER | RESOURCEUSAGE_ATTACHED
		}

		public enum ResourceDisplayType
		{
			RESOURCEDISPLAYTYPE_GENERIC,
			RESOURCEDISPLAYTYPE_DOMAIN,
			RESOURCEDISPLAYTYPE_SERVER,
			RESOURCEDISPLAYTYPE_SHARE,
			RESOURCEDISPLAYTYPE_FILE,
			RESOURCEDISPLAYTYPE_GROUP,
			RESOURCEDISPLAYTYPE_NETWORK,
			RESOURCEDISPLAYTYPE_ROOT,
			RESOURCEDISPLAYTYPE_SHAREADMIN,
			RESOURCEDISPLAYTYPE_DIRECTORY,
			RESOURCEDISPLAYTYPE_TREE,
			RESOURCEDISPLAYTYPE_NDSCONTAINER
		}

		public class ServerEnum : IEnumerable
		{

			internal enum ErrorCodes
			{
				NO_ERROR = 0,
				ERROR_NO_MORE_ITEMS = 259
			}

			[StructLayout(LayoutKind.Sequential)]
			private class NETRESOURCE
			{
				public ResourceScope dwScope = 0;
				public ResourceType dwType = 0;
				public ResourceDisplayType dwDisplayType = 0;
				public ResourceUsage dwUsage = 0;
				public string lpLocalName = null;
				public string lpRemoteName = null;
				public string lpComment = null;
				public string lpProvider = null;
			}

			private ArrayList aData = new ArrayList();

			public int Count
			{
				get
				{
					return aData.Count;
				}
			}

			[DllImport("Mpr.dll", EntryPoint = "WNetOpenEnumA", CallingConvention = CallingConvention.Winapi)]
			private static extern ErrorCodes WNetOpenEnum(ResourceScope dwScope, ResourceType dwType, ResourceUsage dwUsage, NETRESOURCE p, ref IntPtr lphEnum);

			[DllImport("Mpr.dll", EntryPoint = "WNetCloseEnum", CallingConvention = CallingConvention.Winapi)]
			private static extern ErrorCodes WNetCloseEnum(IntPtr hEnum);

			[DllImport("Mpr.dll", EntryPoint = "WNetEnumResourceA", CallingConvention = CallingConvention.Winapi)]
			private static extern ErrorCodes WNetEnumResource(IntPtr hEnum, ref uint lpcCount, IntPtr buffer, ref uint lpBufferSize);

			private void EnumerateServers(NETRESOURCE pRsrc, ResourceScope scope, ResourceType type, ResourceUsage usage, ResourceDisplayType displayType, string kPath)
			{
				uint bufferSize = 16384;
				IntPtr buffer = Marshal.AllocHGlobal((IntPtr)bufferSize);
				IntPtr Handle = IntPtr.Zero;
				ErrorCodes result;
				uint cEntries = 1;
				bool ServerEnum = false;

				result = WNetOpenEnum(scope, type, usage, pRsrc, ref Handle); // out

				if (result == ErrorCodes.NO_ERROR)
				{
					do
					{
						result = WNetEnumResource(Handle, ref cEntries, buffer, ref bufferSize); // ref ref

						if (result == ErrorCodes.NO_ERROR)
						{
							Marshal.PtrToStructure(buffer, pRsrc);

							if (string.Compare(kPath, "") == 0)
							{
								if (pRsrc.dwDisplayType == displayType || pRsrc.dwDisplayType == ResourceDisplayType.RESOURCEDISPLAYTYPE_DOMAIN)
								{
									aData.Add(pRsrc.lpRemoteName + "|" + pRsrc.dwDisplayType);
									if ((pRsrc.dwUsage | ResourceUsage.RESOURCEUSAGE_CONTAINER) == ResourceUsage.RESOURCEUSAGE_CONTAINER)
									{
										if (pRsrc.dwDisplayType == displayType)
											EnumerateServers(pRsrc, scope, type, usage, displayType, kPath);
									}
								}
								else
								{
									if (pRsrc.dwDisplayType == displayType)
									{
										aData.Add(pRsrc.lpRemoteName);
										EnumerateServers(pRsrc, scope, type, usage, displayType, kPath);
										//return
										ServerEnum = true;
									}
									if (!ServerEnum)
									{
										if (pRsrc.dwDisplayType == ResourceDisplayType.RESOURCEDISPLAYTYPE_SHARE)
											aData.Add(pRsrc.lpRemoteName + "-share");
									}
									else
										ServerEnum = false;
									if (kPath.IndexOf(pRsrc.lpRemoteName) >= 0 || string.Compare(pRsrc.lpRemoteName, "Microsoft Windows Network") == 0)
										EnumerateServers(pRsrc, scope, type, usage, displayType, kPath);
								}
							}
							else if (result != ErrorCodes.ERROR_NO_MORE_ITEMS)
								break;
						}
					} while (result != ErrorCodes.ERROR_NO_MORE_ITEMS);

					WNetCloseEnum(Handle);
				}

				Marshal.FreeHGlobal(buffer);
			}

			public ServerEnum(ResourceScope scope, ResourceType type, ResourceUsage usage, ResourceDisplayType displayType, string kPath)
			{
				NETRESOURCE netRoot = new NETRESOURCE();
				EnumerateServers(netRoot, scope, type, usage, displayType, kPath);
			}

			#region IEnumerable Members

			public IEnumerator GetEnumerator()
			{
				return aData.GetEnumerator();
			}
			#endregion

		}
		#endregion
	}

}
