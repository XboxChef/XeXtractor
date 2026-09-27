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

        private static readonly string TempFolder = Path.Combine(Path.GetTempPath(), "XeXtractor");

        private bool isParsing;

        public Form1()
        {
            InitializeComponent();
            treeView1.MouseUp += treeView1_MouseUp;
            FileHandler.ParseCompleted += FileHandler_ParseCompleted;
            Log.getInstance().LogChanged += Form1_LogChanged;
            FormClosed += Form1_FormClosed;
        }

        // The parser thread can outlive the form, so a closed window must not crash it.
        private void InvokeIfAlive(Delegate method, params object[] args)
        {
            try
            {
                if (!IsDisposed)
                    Invoke(method, args);
            }
            catch (ObjectDisposedException)
            {
            }
            catch (InvalidOperationException)
            {
                // Window handle already destroyed.
            }
        }

        private void StartParse(string fileName)
        {
            if (isParsing)
            {
                MessageBox.Show("Please wait until the current file has finished loading.", "XeXtractor",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            isParsing = true;
            UseWaitCursor = true;
            Log.getInstance().Clear();
            InnerFileStructure.getInstance().Clear();
            FileHandler.HandleFile(fileName);
        }

        private void FileHandler_ParseCompleted(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                InvokeIfAlive(new EventHandler(FileHandler_ParseCompleted), sender, e);
                return;
            }

            isParsing = false;
            UseWaitCursor = false;
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
                    SetPreviewImage(null);
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
                    SetPreviewImage(selectedNode.ImageIndex == 7 ? tag.Data : null);
                }
            }
            else
            {
                lblType.Text = "";
                grpInfo.Text = "";
                lblFsize.Text = "";
                SetPreviewImage(null);
            }
        }

        private void SetPreviewImage(byte[] data)
        {
            Image previous = pctPreview.Image;
            pctPreview.Image = null;
            if (previous != null)
                previous.Dispose();

            if (data == null || data.Length == 0)
                return;

            try
            {
                // GDI+ needs the source stream for the image's lifetime, so copy it into a standalone bitmap.
                using (MemoryStream memoryStream = new MemoryStream(data))
                using (Image image = Image.FromStream(memoryStream))
                    pctPreview.Image = new Bitmap(image);
            }
            catch (ArgumentException)
            {
                // Not a readable image; leave the preview empty.
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
                InvokeIfAlive(new EventHandler(Form1_LogChanged), sender, e);
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

        // Returns the number of entries that failed to save; failures are logged and extraction continues.
        private int ExtractSubNodesToFolder(string folder, TreeNodeCollection root)
        {
            int failures = 0;
            foreach (TreeNode treeNode in root)
            {
                try
                {
                    if (treeNode.Tag == null)
                    {
                        failures += ExtractSubNodesToFolder(SafePath.Combine(folder, treeNode.Text), treeNode.Nodes);
                    }
                    else
                    {
                        FileEntry tag = (FileEntry)treeNode.Tag;
                        if (!folder.Contains("\\XACH\\"))
                            tag.SaveAs(SafePath.Combine(folder, tag.fileName));
                    }
                }
                catch (Exception ex)
                {
                    failures++;
                    Log.getInstance().AddEntry("Failed to extract " + treeNode.Text + ": " + ex.Message);
                }
            }
            return failures;
        }

        private void ExtractNodesToFolder(TreeNodeCollection nodes)
        {
            string selectedPath;
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                if (folderBrowserDialog.ShowDialog() != DialogResult.OK)
                    return;
                selectedPath = folderBrowserDialog.SelectedPath;
            }

            int failures = ExtractSubNodesToFolder(selectedPath, nodes);
            if (failures > 0)
                MessageBox.Show(failures + " item(s) could not be extracted. See the log for details.", "XeXtractor",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void ExtractSelectedNode()
        {
            TreeNode selectedNode = treeView1.SelectedNode;
            if (selectedNode == null)
                return;
            if (selectedNode.Tag == null)
            {
                ExtractNodesToFolder(selectedNode.Nodes);
                return;
            }

            FileEntry tag = (FileEntry)selectedNode.Tag;
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.FileName = SafePath.SanitizeName(tag.fileName);
                if (saveFileDialog.ShowDialog() != DialogResult.OK)
                    return;
                try
                {
                    tag.SaveAs(saveFileDialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not save file: " + ex.Message, "XeXtractor",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void extractToolStripMenuItem_Click(object sender, EventArgs e) => ExtractSelectedNode();

        private void openFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "XEX file|*.xex|XSTR file|*.xstr|XSCR file|*.xscr|XDBF file|*.xdbf|XUIZ file|*.xuiz";
                if (openFileDialog.ShowDialog() != DialogResult.OK)
                    return;
                StartParse(openFileDialog.FileName);
            }
        }

        private void extractEverthingToolStripMenuItem_Click(object sender, EventArgs e) => ExtractNodesToFolder(treeView1.Nodes);

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
            StartParse(data[0]);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string[] commandLineArgs = Environment.GetCommandLineArgs();
            if (commandLineArgs.Length <= 1)
                return;
            StartParse(commandLineArgs[1]);
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

        private void button1_Click(object sender, EventArgs e) => ExtractSelectedNode();

        private void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (treeView1.SelectedNode == null)
                return;
            TreeNode selectedNode = treeView1.SelectedNode;
            if (selectedNode.Tag == null)
                return;
            try
            {
                FileEntry tag = (FileEntry)selectedNode.Tag;
                Directory.CreateDirectory(TempFolder);
                string tempPath = SafePath.Combine(TempFolder, tag.fileName);
                tag.SaveAs(tempPath);
                Process.Start(tempPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                if (Directory.Exists(TempFolder))
                    Directory.Delete(TempFolder, true);
            }
            catch (IOException)
            {
                // A previewed file may still be open in another program.
            }
            catch (UnauthorizedAccessException)
            {
            }
        }
    }
}
