using VirtualFileSystem.API;

namespace VirtualFileSystem.GUI;

public partial class Form1 : Form
{
    private readonly IFileSystem _fs;
    
    public Form1()
    {
        InitializeComponent();
        _fs = new FileSystemService("container.vfs", 4096);
    }
    
    private void Form1_Load(object sender, EventArgs e)
    {
        BuildTree();
        RefreshViews();
    }
    
    private void RefreshViews()
    {
        listView.Items.Clear();
        var entries = _fs.Ls();
        
        foreach (var e in entries)
        {
            var item = new ListViewItem(e.Name);
            
            item.SubItems.Add($"{e.Size}");
            item.Tag = e;
            
            listView.Items.Add(item);
        }
    }
    
    private void BuildTree()
    {
        treeView.Nodes.Clear();
        
        var root = new TreeNode("/")
        {
            Tag = Array.Empty<string>()
        };
        
        treeView.Nodes.Add(root);
        
        BuildTreeRecursive(root, []);
        root.Expand();
        
        _fs.Cd("\\");
    }
    
    private void BuildTreeRecursive(TreeNode node, string[] segments)
    {
        _fs.Cd("\\");
        foreach (var s in segments)
        {
            _fs.Cd(s);
        }
        
        var entries = _fs.Ls();
        foreach (var e in entries)
        {
            if (!e.IsDir)
            {
                continue;
            }
            
            var childSegments = new string[segments.Length + 1];
            for (int i = 0; i < segments.Length; i++)
            {
                childSegments[i] = segments[i];
            }
            
            childSegments[segments.Length] = e.Name;
            
            var child = new TreeNode(e.Name)
            {
                Tag = childSegments
            };
            
            node.Nodes.Add(child);
            
            BuildTreeRecursive(child, childSegments);
        }
    }
    
    private void ListViewDoubleClick(object? sender, EventArgs e)
    {
        if (listView.SelectedItems.Count == 0)
        {
            return;
        }
        
        var item = listView.SelectedItems[0];
        var info = (FileEntryInfo)item.Tag!;
        if (!info.IsDir)
        {
            return;
        }
        
        _fs.Cd(info.Name);
        RefreshViews();
    }
    
    private void TreeViewNodeMouseDoubleClick(object? sender, TreeNodeMouseClickEventArgs e)
    {
        if (e.Node?.Tag is not string[] segments)
        {
            return;
        }
        
        _fs.Cd("\\");
        foreach (var s in segments)
        {
            _fs.Cd(s);
        }
        
        RefreshViews();
    }
    
    private void BtnRoot_Click(object? sender, EventArgs e)
    {
        _fs.Cd("\\");
        RefreshViews();
    }
    
    private void BtnUp_Click(object? sender, EventArgs e)
    {
        _fs.Cd("..");
        RefreshViews();
    }
    
    private void BtnImport_Click(object? sender, EventArgs e)
    {
        using var ofd = new OpenFileDialog();
        if (ofd.ShowDialog() != DialogResult.OK)
        {
            return;
        }
        
        var fileName = Path.GetFileName(ofd.FileName);
        
        try
        {
            _fs.Cpin(ofd.FileName, fileName);
            RefreshViews();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Import failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    private void ListViewMouseClick(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Right)
        {
            return;
        }
        
        var item = listView.GetItemAt(e.X, e.Y);
        if (item == null)
        {
            return;
        }
        
        var info = (FileEntryInfo)item.Tag!;
        if (info.IsDir)
        {
            contextMenuDirs.Tag = info;
            contextMenuDirs.Show(listView, e.Location);
        }
        else
        {
            contextMenuFiles.Tag = info;
            contextMenuFiles.Show(listView, e.Location);
        }
    }
    
    private void MenuItemCpout_Click(object? sender, EventArgs e)
    {
        if (contextMenuFiles.Tag is not FileEntryInfo info)
        {
            return;
        }
        
        using var sfd = new SaveFileDialog();
        sfd.FileName = info.Name;
        
        if (sfd.ShowDialog() != DialogResult.OK)
        {
            return;
        }
        
        try
        {
            _fs.Cpout(info.Name, sfd.FileName);
            MessageBox.Show(this, "Exported", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Export failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    private void MenuItemRm_Click(object? sender, EventArgs e)
    {
        if (contextMenuFiles.Tag is not FileEntryInfo info)
        {
            return;
        }
        
        var ok = MessageBox.Show(this, $"Remove file '{info.Name}'?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (ok != DialogResult.Yes)
        {
            return;
        }
        
        try
        {
            _fs.Rm(info.Name);
            RefreshViews();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Remove failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    private void MenuItemRd_Click(object? sender, EventArgs e)
    {
        if (contextMenuDirs.Tag is not FileEntryInfo info)
        {
            return;
        }
        
        var ok = MessageBox.Show(this, $"Remove directory '{info.Name}'?", "Confirm",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        
        if (ok != DialogResult.Yes)
        {
            return;
        }
        
        try
        {
            _fs.Rd(info.Name);
            RefreshViews();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Remove failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    private void btnMd_Click(object sender, EventArgs e)
    {
        var name = ShowInputDialog("New folder name:", "Create Folder");
        if (name == null || name.Length == 0)
        {
            return;
        }
        
        try
        {
            _fs.Md(name);
            RefreshViews();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Create folder failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    private string? ShowInputDialog(string text, string caption)
    {
        using var prompt = new Form();
        
        prompt.Width = 400;
        prompt.Height = 140;
        prompt.Text = caption;
        prompt.StartPosition = FormStartPosition.CenterParent;
        prompt.FormBorderStyle = FormBorderStyle.FixedDialog;
        prompt.MinimizeBox = false;
        prompt.MaximizeBox = false;
        
        var lbl = new Label { Left = 10, Top = 10, Text = text, AutoSize = true };
        var textBox = new TextBox { Left = 10, Top = 35, Width = 360 };
        
        var confirm = new Button { Text = "OK", Left = 220, Width = 75, Top = 70, DialogResult = DialogResult.OK };
        var cancel = new Button { Text = "Cancel", Left = 300, Width = 75, Top = 70, DialogResult = DialogResult.Cancel };
        
        prompt.Controls.Add(lbl);
        prompt.Controls.Add(textBox);
        prompt.Controls.Add(confirm);
        prompt.Controls.Add(cancel);
        
        prompt.AcceptButton = confirm;
        prompt.CancelButton = cancel;
        
        return prompt.ShowDialog(this) == DialogResult.OK ? textBox.Text : null;
    }
}