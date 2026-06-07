using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace XeXtractor
{
    public partial class Form1 : Form
    {
        private const string SupportUrl = "https://www.paypal.com/donate/?hosted_button_id=XGX526XVYTNR8";

        public Form1()
        {
            InitializeComponent();
            treeView1.MouseUp += treeView1_MouseUp;
            FileHandler.ParseCompleted += FileHandler_ParseCompleted;
            Log.getInstance().LogChanged += Form1_LogChanged;
        }

        private void FileHandler_ParseCompleted(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new EventHandler(FileHandler_ParseCompleted), sender, e);
                return;
            }

            Cursor.Current = Cursors.Default;
            FileEntry[] files = InnerFileStructure.getInstance().getFiles();
            treeView1.Nodes.Clear();
            foreach (FileEntry file in files)
            {
                TreeNode parentNode = getParentNode(file);
                TreeNode node = new TreeNode(file.fileName);
                node.ImageIndex = GetIconIndex(file.fileName);
                node.SelectedImageIndex = node.ImageIndex;
                node.ContextMenuStrip = contextMenuStrip1;
                node.Tag = file;
                parentNode.Nodes.Add(node);
            }
            Form1_LogChanged(null, EventArgs.Empty);
        }

        private static int GetIconIndex(string fileName)
        {
            switch (Path.GetExtension(fileName).ToLower())
            {
                case ".xui":
                case ".xur":
                    return 8;
                case ".txt":
                    return 0;
                case ".xlast":
                    return 4;
                case ".png":
                case ".jpg":
                    return 7;
                default:
                    return 1;
            }
        }

        private void treeView1_MouseUp(object sender, MouseEventArgs e)
        {
            TreeNode nodeAt = treeView1.GetNodeAt(e.X, e.Y);
            if (nodeAt == treeView1.SelectedNode)
                return;
            treeView1.SelectedNode = nodeAt;
            HandleSelectedNode();
        }

        private void HandleSelectedNode()
        {
            if (treeView1.SelectedNode != null)
            {
                TreeNode selectedNode = treeView1.SelectedNode;
                if (selectedNode.Tag == null)
                {
                    grpInfo.Text = selectedNode.Text;
                    long totalSize = getTotalSize(selectedNode.Nodes);
                    lblFsize.Text = totalSize < 1024L
                        ? totalSize + " Bytes"
                        : (totalSize / 1024L) + " KB";
                    pctPreview.Image = null;
                    lblType.Text = "";
                }
                else
                {
                    FileEntry tag = (FileEntry)selectedNode.Tag;
                    grpInfo.Text = tag.fileName;
                    int dataLength = tag.Data != null ? tag.Data.Length : 0;
                    lblFsize.Text = dataLength < 1024
                        ? dataLength + " Bytes"
                        : (dataLength / 1024) + " KB";
                    lblType.Text = tag.type;
                    if (selectedNode.ImageIndex == 7 && tag.Data != null)
                    {
                        using (MemoryStream memoryStream = new MemoryStream(tag.Data))
                            pctPreview.Image = new Bitmap(memoryStream);
                    }
                    else
                        pctPreview.Image = null;
                }
            }
            else
            {
                lblType.Text = "";
                grpInfo.Text = "";
                lblFsize.Text = "";
                pctPreview.Image = null;
            }
        }

        private TreeNode getParentNode(FileEntry entr)
        {
            TreeNode node1 = null;
            foreach (TreeNode node2 in treeView1.Nodes)
            {
                if (node2.Text == entr.type)
                {
                    node1 = node2;
                    break;
                }
            }
            if (node1 == null)
            {
                node1 = new TreeNode(entr.type);
                node1.ImageIndex = 3;
                node1.SelectedImageIndex = 3;
                node1.ContextMenuStrip = contextMenuStrip1;
                treeView1.Nodes.Add(node1);
            }

            if (entr.folder == "")
                return node1;

            TreeNode treeNode = node1;
            string[] separator = new string[] { "\\" };
            foreach (string text in entr.folder.Split(separator, StringSplitOptions.RemoveEmptyEntries))
            {
                bool found = false;
                foreach (TreeNode node3 in treeNode.Nodes)
                {
                    if (node3.Text == text)
                    {
                        treeNode = node3;
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    TreeNode node4 = new TreeNode(text);
                    node4.ImageIndex = 2;
                    node4.SelectedImageIndex = 2;
                    node4.ContextMenuStrip = contextMenuStrip1;
                    treeNode.Nodes.Add(node4);
                    treeNode = node4;
                }
            }
            return treeNode;
        }

        private void Form1_LogChanged(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new EventHandler(Form1_LogChanged), sender, e);
                return;
            }

            textBox1.Text = Log.getInstance().getLog();
            if (textBox1.Text.Length <= 0)
                return;
            textBox1.Select(textBox1.Text.Length - 1, 0);
            textBox1.ScrollToCaret();
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e) => Close();

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new AboutBox1().ShowDialog();
        }

        private void ExtractSubNodesToFolder(string folder, TreeNodeCollection root)
        {
            foreach (TreeNode treeNode in root)
            {
                if (treeNode.Tag == null)
                {
                    ExtractSubNodesToFolder(Path.Combine(folder, treeNode.Text), treeNode.Nodes);
                }
                else
                {
                    FileEntry tag = (FileEntry)treeNode.Tag;
                    if (!folder.Contains("\\XACH\\"))
                    {
                        string file = Path.Combine(folder, tag.fileName);
                        tag.SaveAs(file);
                    }
                }
            }
        }

        private void extractToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode selectedNode = treeView1.SelectedNode;
            if (selectedNode.Tag == null)
            {
                FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
                if (folderBrowserDialog.ShowDialog() != DialogResult.OK)
                    return;
                ExtractSubNodesToFolder(folderBrowserDialog.SelectedPath, selectedNode.Nodes);
            }
            else
            {
                FileEntry tag = (FileEntry)selectedNode.Tag;
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.FileName = tag.fileName;
                if (saveFileDialog.ShowDialog() != DialogResult.OK)
                    return;
                tag.SaveAs(saveFileDialog.FileName);
            }
        }

        private void openFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "XEX file|*.xex|XSTR file|*.xstr|XSCR file|*.xscr|XDBF file|*.xdbf|XUIZ file|*.xuiz";
            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;
            Cursor.Current = Cursors.WaitCursor;
            Log.getInstance().Clear();
            InnerFileStructure.getInstance().Clear();
            FileHandler.HandleFile(openFileDialog.FileName);
        }

        private void extractEverthingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() != DialogResult.OK)
                return;
            ExtractSubNodesToFolder(folderBrowserDialog.SelectedPath, treeView1.Nodes);
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) => OpenSupportUrl();

        private void pictureBox1_Click(object sender, EventArgs e) => OpenSupportUrl();

        private static void OpenSupportUrl()
        {
            Process.Start(SupportUrl);
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop, false))
                return;
            e.Effect = DragDropEffects.All;
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string[] data = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (data == null || data.Length == 0)
                return;
            Log.getInstance().Clear();
            InnerFileStructure.getInstance().Clear();
            Cursor.Current = Cursors.WaitCursor;
            FileHandler.HandleFile(data[0]);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string[] commandLineArgs = Environment.GetCommandLineArgs();
            if (commandLineArgs.Length <= 1)
                return;
            Log.getInstance().Clear();
            InnerFileStructure.getInstance().Clear();
            Cursor.Current = Cursors.WaitCursor;
            FileHandler.HandleFile(commandLineArgs[1]);
        }

        private void treeView1_KeyUp(object sender, KeyEventArgs e) => HandleSelectedNode();

        private long getTotalSize(TreeNodeCollection tnc)
        {
            long totalSize = 0;
            foreach (TreeNode treeNode in tnc)
            {
                if (treeNode.Tag != null)
                {
                    FileEntry tag = (FileEntry)treeNode.Tag;
                    totalSize += tag.Data != null ? tag.Data.Length : 0;
                }
                totalSize += getTotalSize(treeNode.Nodes);
            }
            return totalSize;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            TreeNode selectedNode = treeView1.SelectedNode;
            if (selectedNode == null)
                return;
            if (selectedNode.Tag == null)
            {
                FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
                if (folderBrowserDialog.ShowDialog() != DialogResult.OK)
                    return;
                ExtractSubNodesToFolder(folderBrowserDialog.SelectedPath, selectedNode.Nodes);
            }
            else
            {
                FileEntry tag = (FileEntry)selectedNode.Tag;
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.FileName = tag.fileName;
                if (saveFileDialog.ShowDialog() != DialogResult.OK)
                    return;
                tag.SaveAs(saveFileDialog.FileName);
            }
        }

        private void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (treeView1.SelectedNode == null)
                return;
            TreeNode selectedNode = treeView1.SelectedNode;
            if (selectedNode.Tag == null)
                return;
            const string tempFolder = "TEMP";
            if (!Directory.Exists(tempFolder))
                Directory.CreateDirectory(tempFolder);
            try
            {
                FileEntry tag = (FileEntry)selectedNode.Tag;
                string tempPath = Path.Combine(tempFolder, tag.fileName);
                tag.SaveAs(tempPath);
                Process.Start(tempPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
