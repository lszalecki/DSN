namespace DiagnostykaStanuNawierzchni
{
    partial class XMLMerge
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
            this.listView1 = new System.Windows.Forms.ListView();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.panel1 = new System.Windows.Forms.Panel();
            this.buttonMergeSelected = new System.Windows.Forms.Button();
            this.button1MergeAll = new System.Windows.Forms.Button();
            this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.statusStrip1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // listView1
            // 
            this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView1.FullRowSelect = true;
            this.listView1.Location = new System.Drawing.Point(12, 12);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(326, 225);
            this.listView1.TabIndex = 0;
            this.listView1.UseCompatibleStateImageBehavior = false;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripProgressBar1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 240);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(556, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add(this.buttonMergeSelected);
            this.panel1.Controls.Add(this.button1MergeAll);
            this.panel1.Location = new System.Drawing.Point(344, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(200, 225);
            this.panel1.TabIndex = 2;
            // 
            // buttonMergeSelected
            // 
            this.buttonMergeSelected.Image = global::DiagnostykaStanuNawierzchni.Properties.Resources.documents_preferences;
            this.buttonMergeSelected.Location = new System.Drawing.Point(17, 67);
            this.buttonMergeSelected.Name = "buttonMergeSelected";
            this.buttonMergeSelected.Size = new System.Drawing.Size(125, 40);
            this.buttonMergeSelected.TabIndex = 1;
            this.buttonMergeSelected.Text = "Merge Selected";
            this.buttonMergeSelected.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.buttonMergeSelected.UseVisualStyleBackColor = true;
            this.buttonMergeSelected.Click += new System.EventHandler(this.buttonMergeSelected_Click);
            // 
            // button1MergeAll
            // 
            this.button1MergeAll.Image = global::DiagnostykaStanuNawierzchni.Properties.Resources.documents_preferences;
            this.button1MergeAll.Location = new System.Drawing.Point(17, 21);
            this.button1MergeAll.Name = "button1MergeAll";
            this.button1MergeAll.Size = new System.Drawing.Size(125, 40);
            this.button1MergeAll.TabIndex = 0;
            this.button1MergeAll.Text = "Merge All";
            this.button1MergeAll.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.button1MergeAll.UseVisualStyleBackColor = true;
            // 
            // toolStripProgressBar1
            // 
            this.toolStripProgressBar1.Name = "toolStripProgressBar1";
            this.toolStripProgressBar1.Size = new System.Drawing.Size(200, 16);
            // 
            // XMLMerge
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(556, 262);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.listView1);
            this.Name = "XMLMerge";
            this.Text = "XMLMerge";
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button buttonMergeSelected;
        private System.Windows.Forms.Button button1MergeAll;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
    }
}