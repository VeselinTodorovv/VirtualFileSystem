namespace VirtualFileSystem.GUI;

partial class Form1
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        splitContainer1 = new SplitContainer();
        treeView1 = new TreeView();
        panelButtons = new Panel();
        btnMd = new Button();
        btnRoot = new Button();
        btnUp = new Button();
        btnImport = new Button();
        listView1 = new ListView();
        columnName = new ColumnHeader();
        columnSize = new ColumnHeader();
        contextMenuFiles = new ContextMenuStrip(components);
        menuItemCpout = new ToolStripMenuItem();
        menuItemRm = new ToolStripMenuItem();
        contextMenuDirs = new ContextMenuStrip(components);
        menuItemRd = new ToolStripMenuItem();
        ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
        splitContainer1.Panel1.SuspendLayout();
        splitContainer1.Panel2.SuspendLayout();
        splitContainer1.SuspendLayout();
        panelButtons.SuspendLayout();
        contextMenuFiles.SuspendLayout();
        contextMenuDirs.SuspendLayout();
        SuspendLayout();
        // 
        // splitContainer1
        // 
        splitContainer1.Dock = DockStyle.Fill;
        splitContainer1.Location = new Point(0, 0);
        splitContainer1.Name = "splitContainer1";
        // 
        // splitContainer1.Panel1
        // 
        splitContainer1.Panel1.Controls.Add(treeView1);
        splitContainer1.Panel1.Controls.Add(panelButtons);
        // 
        // splitContainer1.Panel2
        // 
        splitContainer1.Panel2.Controls.Add(listView1);
        splitContainer1.Size = new Size(800, 450);
        splitContainer1.SplitterDistance = 261;
        splitContainer1.TabIndex = 0;
        splitContainer1.SplitterMoved += splitContainer1_SplitterMoved;
        // 
        // treeView1
        // 
        treeView1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        treeView1.Location = new Point(3, 42);
        treeView1.Name = "treeView1";
        treeView1.Size = new Size(255, 405);
        treeView1.TabIndex = 0;
        treeView1.NodeMouseDoubleClick += TreeView1_NodeMouseDoubleClick;
        // 
        // panelButtons
        // 
        panelButtons.Controls.Add(btnMd);
        panelButtons.Controls.Add(btnRoot);
        panelButtons.Controls.Add(btnUp);
        panelButtons.Controls.Add(btnImport);
        panelButtons.Dock = DockStyle.Top;
        panelButtons.Location = new Point(0, 0);
        panelButtons.Name = "panelButtons";
        panelButtons.Size = new Size(261, 36);
        panelButtons.TabIndex = 1;
        // 
        // btnMd
        // 
        btnMd.Location = new Point(176, 6);
        btnMd.Name = "btnMd";
        btnMd.Size = new Size(75, 24);
        btnMd.TabIndex = 3;
        btnMd.Text = "Make Dir";
        btnMd.UseVisualStyleBackColor = true;
        btnMd.Click += btnMd_Click;
        // 
        // btnRoot
        // 
        btnRoot.Location = new Point(3, 6);
        btnRoot.Name = "btnRoot";
        btnRoot.Size = new Size(40, 24);
        btnRoot.TabIndex = 0;
        btnRoot.Text = "\\";
        btnRoot.UseVisualStyleBackColor = true;
        btnRoot.Click += BtnRoot_Click;
        // 
        // btnUp
        // 
        btnUp.Location = new Point(49, 6);
        btnUp.Name = "btnUp";
        btnUp.Size = new Size(40, 24);
        btnUp.TabIndex = 1;
        btnUp.Text = "..";
        btnUp.UseVisualStyleBackColor = true;
        btnUp.Click += BtnUp_Click;
        // 
        // btnImport
        // 
        btnImport.Location = new Point(95, 6);
        btnImport.Name = "btnImport";
        btnImport.Size = new Size(75, 24);
        btnImport.TabIndex = 2;
        btnImport.Text = "Import";
        btnImport.UseVisualStyleBackColor = true;
        btnImport.Click += BtnImport_Click;
        // 
        // listView1
        // 
        listView1.Columns.AddRange(new ColumnHeader[] { columnName, columnSize });
        listView1.Dock = DockStyle.Fill;
        listView1.FullRowSelect = true;
        listView1.Location = new Point(0, 0);
        listView1.MultiSelect = false;
        listView1.Name = "listView1";
        listView1.Size = new Size(535, 450);
        listView1.TabIndex = 0;
        listView1.UseCompatibleStateImageBehavior = false;
        listView1.View = View.Details;
        listView1.SelectedIndexChanged += listView1_SelectedIndexChanged;
        listView1.DoubleClick += ListView1_DoubleClick;
        listView1.MouseClick += ListView1_MouseClick;
        // 
        // columnName
        // 
        columnName.Text = "Name";
        columnName.Width = 350;
        // 
        // columnSize
        // 
        columnSize.Text = "Size";
        columnSize.Width = 100;
        // 
        // contextMenuFiles
        // 
        contextMenuFiles.Items.AddRange(new ToolStripItem[] { menuItemCpout, menuItemRm });
        contextMenuFiles.Name = "contextMenuFiles";
        contextMenuFiles.Size = new Size(108, 48);
        // 
        // menuItemCpout
        // 
        menuItemCpout.Name = "menuItemCpout";
        menuItemCpout.Size = new Size(107, 22);
        menuItemCpout.Text = "Cpout";
        menuItemCpout.Click += MenuItemCpout_Click;
        // 
        // menuItemRm
        // 
        menuItemRm.Name = "menuItemRm";
        menuItemRm.Size = new Size(107, 22);
        menuItemRm.Text = "Rm";
        menuItemRm.Click += MenuItemRm_Click;
        // 
        // contextMenuDirs
        // 
        contextMenuDirs.Items.AddRange(new ToolStripItem[] { menuItemRd });
        contextMenuDirs.Name = "contextMenuDirs";
        contextMenuDirs.Size = new Size(89, 26);
        // 
        // menuItemRd
        // 
        menuItemRd.Name = "menuItemRd";
        menuItemRd.Size = new Size(88, 22);
        menuItemRd.Text = "Rd";
        menuItemRd.Click += MenuItemRd_Click;
        // 
        // Form1
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(800, 450);
        Controls.Add(splitContainer1);
        Name = "Form1";
        Text = "Virtual File System";
        splitContainer1.Panel1.ResumeLayout(false);
        splitContainer1.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
        splitContainer1.ResumeLayout(false);
        panelButtons.ResumeLayout(false);
        contextMenuFiles.ResumeLayout(false);
        contextMenuDirs.ResumeLayout(false);
        ResumeLayout(false);
        Load += Form1_Load!;
    }

    #endregion

    private System.Windows.Forms.SplitContainer splitContainer1;
    private System.Windows.Forms.TreeView treeView1;
    private System.Windows.Forms.ListView listView1;
    private System.Windows.Forms.Panel panelButtons;
    private System.Windows.Forms.Button btnRoot;
    private System.Windows.Forms.Button btnUp;
    private System.Windows.Forms.Button btnImport;
    private System.Windows.Forms.ColumnHeader columnName;
    private System.Windows.Forms.ColumnHeader columnSize;
    private System.Windows.Forms.ContextMenuStrip contextMenuFiles;
    private System.Windows.Forms.ToolStripMenuItem menuItemCpout;
    private System.Windows.Forms.ToolStripMenuItem menuItemRm;
    private System.Windows.Forms.ContextMenuStrip contextMenuDirs;
    private System.Windows.Forms.ToolStripMenuItem menuItemRd;
    private Button btnMd;
}