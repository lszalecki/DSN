namespace DiagnostykaStanuNawierzchni
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.toolStripMenuItemFile = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemOpenFile = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemOpenFolder = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemClose = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemSqlite = new System.Windows.Forms.ToolStripMenuItem();
            this.importResultingData = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemGmlId = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemPhoto = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemOpenPhotoFolder = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemXmlFolder = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButtonOpenFile = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonOpenFolder = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonStart = new System.Windows.Forms.ToolStripButton();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripTextBoxOutputFolder = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripButtonOutputFolder = new System.Windows.Forms.ToolStripButton();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.folderBrowserDialogOutput = new System.Windows.Forms.FolderBrowserDialog();
            this.folderBrowserDialogPhoto = new System.Windows.Forms.FolderBrowserDialog();
            this.toolStripButtonProcessPhoto = new System.Windows.Forms.ToolStripButton();
            this.menuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemFile,
            this.toolStripMenuItemSqlite,
            this.toolStripMenuItemPhoto,
            this.toolStripMenuItem1});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(951, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // toolStripMenuItemFile
            // 
            this.toolStripMenuItemFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemOpenFile,
            this.toolStripMenuItemOpenFolder,
            this.toolStripMenuItemClose});
            this.toolStripMenuItemFile.Name = "toolStripMenuItemFile";
            this.toolStripMenuItemFile.Size = new System.Drawing.Size(37, 20);
            this.toolStripMenuItemFile.Text = "File";
            // 
            // toolStripMenuItemOpenFile
            // 
            this.toolStripMenuItemOpenFile.Name = "toolStripMenuItemOpenFile";
            this.toolStripMenuItemOpenFile.Size = new System.Drawing.Size(139, 22);
            this.toolStripMenuItemOpenFile.Text = "Open File";
            this.toolStripMenuItemOpenFile.Click += new System.EventHandler(this.toolStripMenuItemOpenFile_Click);
            // 
            // toolStripMenuItemOpenFolder
            // 
            this.toolStripMenuItemOpenFolder.Name = "toolStripMenuItemOpenFolder";
            this.toolStripMenuItemOpenFolder.Size = new System.Drawing.Size(139, 22);
            this.toolStripMenuItemOpenFolder.Text = "Open Folder";
            this.toolStripMenuItemOpenFolder.Click += new System.EventHandler(this.toolStripMenuItemOpenFolder_Click);
            // 
            // toolStripMenuItemClose
            // 
            this.toolStripMenuItemClose.Name = "toolStripMenuItemClose";
            this.toolStripMenuItemClose.Size = new System.Drawing.Size(139, 22);
            this.toolStripMenuItemClose.Text = "Close";
            this.toolStripMenuItemClose.Click += new System.EventHandler(this.toolStripMenuItemClose_Click);
            // 
            // toolStripMenuItemSqlite
            // 
            this.toolStripMenuItemSqlite.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.importResultingData,
            this.toolStripMenuItemGmlId});
            this.toolStripMenuItemSqlite.Name = "toolStripMenuItemSqlite";
            this.toolStripMenuItemSqlite.Size = new System.Drawing.Size(53, 20);
            this.toolStripMenuItemSqlite.Text = "SQLite";
            // 
            // importResultingData
            // 
            this.importResultingData.Name = "importResultingData";
            this.importResultingData.Size = new System.Drawing.Size(185, 22);
            this.importResultingData.Text = "Import resulting data";
            this.importResultingData.Click += new System.EventHandler(this.importResultingData_Click);
            // 
            // toolStripMenuItemGmlId
            // 
            this.toolStripMenuItemGmlId.Name = "toolStripMenuItemGmlId";
            this.toolStripMenuItemGmlId.Size = new System.Drawing.Size(185, 22);
            this.toolStripMenuItemGmlId.Text = "Import gml id";
            this.toolStripMenuItemGmlId.Click += new System.EventHandler(this.toolStripMenuItemGmlId_Click);
            // 
            // toolStripMenuItemPhoto
            // 
            this.toolStripMenuItemPhoto.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemOpenPhotoFolder});
            this.toolStripMenuItemPhoto.Name = "toolStripMenuItemPhoto";
            this.toolStripMenuItemPhoto.Size = new System.Drawing.Size(110, 20);
            this.toolStripMenuItemPhoto.Text = "Photorejestration";
            // 
            // toolStripMenuItemOpenPhotoFolder
            // 
            this.toolStripMenuItemOpenPhotoFolder.Name = "toolStripMenuItemOpenPhotoFolder";
            this.toolStripMenuItemOpenPhotoFolder.Size = new System.Drawing.Size(152, 22);
            this.toolStripMenuItemOpenPhotoFolder.Text = "Open Folder";
            this.toolStripMenuItemOpenPhotoFolder.Click += new System.EventHandler(this.toolStripMenuItemOpenPhotoFolder_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemXmlFolder});
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(88, 20);
            this.toolStripMenuItem1.Text = "XML Validate";
            // 
            // toolStripMenuItemXmlFolder
            // 
            this.toolStripMenuItemXmlFolder.Name = "toolStripMenuItemXmlFolder";
            this.toolStripMenuItemXmlFolder.Size = new System.Drawing.Size(139, 22);
            this.toolStripMenuItemXmlFolder.Text = "Open Folder";
            this.toolStripMenuItemXmlFolder.Click += new System.EventHandler(this.toolStripMenuItemXmlFolder_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripProgressBar1,
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 330);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(951, 22);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripProgressBar1
            // 
            this.toolStripProgressBar1.Name = "toolStripProgressBar1";
            this.toolStripProgressBar1.Size = new System.Drawing.Size(150, 16);
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(0, 17);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonOpenFile,
            this.toolStripButtonOpenFolder,
            this.toolStripButtonStart,
            this.toolStripButtonProcessPhoto,
            this.toolStripLabel1,
            this.toolStripLabel2,
            this.toolStripTextBoxOutputFolder,
            this.toolStripButtonOutputFolder});
            this.toolStrip1.Location = new System.Drawing.Point(0, 24);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(951, 39);
            this.toolStrip1.TabIndex = 3;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButtonOpenFile
            // 
            this.toolStripButtonOpenFile.Image = global::DiagnostykaStanuNawierzchni.Properties.Resources.document;
            this.toolStripButtonOpenFile.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonOpenFile.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonOpenFile.Name = "toolStripButtonOpenFile";
            this.toolStripButtonOpenFile.Size = new System.Drawing.Size(93, 36);
            this.toolStripButtonOpenFile.Text = "Open File";
            this.toolStripButtonOpenFile.Click += new System.EventHandler(this.toolStripButtonOpenFile_Click);
            // 
            // toolStripButtonOpenFolder
            // 
            this.toolStripButtonOpenFolder.Image = global::DiagnostykaStanuNawierzchni.Properties.Resources.folder_document;
            this.toolStripButtonOpenFolder.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonOpenFolder.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonOpenFolder.Name = "toolStripButtonOpenFolder";
            this.toolStripButtonOpenFolder.Size = new System.Drawing.Size(108, 36);
            this.toolStripButtonOpenFolder.Text = "Open Folder";
            this.toolStripButtonOpenFolder.Click += new System.EventHandler(this.toolStripButtonOpenFolder_Click);
            // 
            // toolStripButtonStart
            // 
            this.toolStripButtonStart.Enabled = false;
            this.toolStripButtonStart.Image = global::DiagnostykaStanuNawierzchni.Properties.Resources.bullet_triangle_glass_green;
            this.toolStripButtonStart.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonStart.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonStart.Name = "toolStripButtonStart";
            this.toolStripButtonStart.Size = new System.Drawing.Size(67, 36);
            this.toolStripButtonStart.Text = "Start";
            this.toolStripButtonStart.Click += new System.EventHandler(this.toolStripButtonStart_Click);
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(52, 36);
            this.toolStripLabel1.Text = "               ";
            // 
            // toolStripLabel2
            // 
            this.toolStripLabel2.Name = "toolStripLabel2";
            this.toolStripLabel2.Size = new System.Drawing.Size(84, 36);
            this.toolStripLabel2.Text = "Output Folder:";
            // 
            // toolStripTextBoxOutputFolder
            // 
            this.toolStripTextBoxOutputFolder.Name = "toolStripTextBoxOutputFolder";
            this.toolStripTextBoxOutputFolder.Size = new System.Drawing.Size(350, 39);
            this.toolStripTextBoxOutputFolder.TextChanged += new System.EventHandler(this.toolStripTextBoxOutputFolder_TextChanged);
            // 
            // toolStripButtonOutputFolder
            // 
            this.toolStripButtonOutputFolder.Image = global::DiagnostykaStanuNawierzchni.Properties.Resources.folder_out;
            this.toolStripButtonOutputFolder.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonOutputFolder.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonOutputFolder.Name = "toolStripButtonOutputFolder";
            this.toolStripButtonOutputFolder.Size = new System.Drawing.Size(52, 36);
            this.toolStripButtonOutputFolder.Text = "...";
            this.toolStripButtonOutputFolder.ToolTipText = "Select output folder.";
            this.toolStripButtonOutputFolder.Click += new System.EventHandler(this.toolStripButtonOutputFolder_Click);
            // 
            // richTextBox1
            // 
            this.richTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBox1.Location = new System.Drawing.Point(0, 66);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(951, 265);
            this.richTextBox1.TabIndex = 4;
            this.richTextBox1.Text = "";
            // 
            // toolStripButtonProcessPhoto
            // 
            this.toolStripButtonProcessPhoto.Enabled = false;
            this.toolStripButtonProcessPhoto.Image = global::DiagnostykaStanuNawierzchni.Properties.Resources.bullet_triangle_glass_green;
            this.toolStripButtonProcessPhoto.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonProcessPhoto.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonProcessPhoto.Name = "toolStripButtonProcessPhoto";
            this.toolStripButtonProcessPhoto.Size = new System.Drawing.Size(118, 36);
            this.toolStripButtonProcessPhoto.Text = "Process Photo";
            this.toolStripButtonProcessPhoto.Click += new System.EventHandler(this.toolStripButtonProcessPhoto_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(951, 352);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "DSN";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemFile;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemOpenFile;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemClose;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemOpenFolder;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSqlite;
        private System.Windows.Forms.ToolStripMenuItem importResultingData;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButtonOpenFile;
        private System.Windows.Forms.ToolStripButton toolStripButtonOpenFolder;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.ToolStripButton toolStripButtonStart;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripLabel toolStripLabel2;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBoxOutputFolder;
        private System.Windows.Forms.ToolStripButton toolStripButtonOutputFolder;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialogOutput;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemGmlId;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemPhoto;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemOpenPhotoFolder;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialogPhoto;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemXmlFolder;
        private System.Windows.Forms.ToolStripButton toolStripButtonProcessPhoto;
    }
}

