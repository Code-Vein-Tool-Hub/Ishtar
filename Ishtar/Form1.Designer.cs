namespace Ishtar
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.TB_ModsPath = new System.Windows.Forms.TextBox();
            this.B_GetModsPath = new System.Windows.Forms.Button();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.button2 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            this.TSB_PartialMerge = new System.Windows.Forms.ToolStripMenuItem();
            this.TSB_Nested = new System.Windows.Forms.ToolStripMenuItem();
            this.TSB_Scan = new System.Windows.Forms.ToolStripMenuItem();
            this.TSB_Patch = new System.Windows.Forms.ToolStripMenuItem();
            this.parallelScaningToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.makeIshtarPatchAfterScanToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // TB_ModsPath
            // 
            this.TB_ModsPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TB_ModsPath.Location = new System.Drawing.Point(12, 41);
            this.TB_ModsPath.Name = "TB_ModsPath";
            this.TB_ModsPath.ReadOnly = true;
            this.TB_ModsPath.Size = new System.Drawing.Size(405, 20);
            this.TB_ModsPath.TabIndex = 0;
            // 
            // B_GetModsPath
            // 
            this.B_GetModsPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.B_GetModsPath.Location = new System.Drawing.Point(423, 40);
            this.B_GetModsPath.Name = "B_GetModsPath";
            this.B_GetModsPath.Size = new System.Drawing.Size(51, 20);
            this.B_GetModsPath.TabIndex = 1;
            this.B_GetModsPath.Text = "...";
            this.B_GetModsPath.UseVisualStyleBackColor = true;
            this.B_GetModsPath.Click += new System.EventHandler(this.B_GetModsPath_Click);
            // 
            // richTextBox1
            // 
            this.richTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBox1.Location = new System.Drawing.Point(12, 67);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.Size = new System.Drawing.Size(462, 294);
            this.richTextBox1.TabIndex = 2;
            this.richTextBox1.Text = resources.GetString("richTextBox1.Text");
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.button2.Location = new System.Drawing.Point(130, 367);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(228, 38);
            this.button2.TabIndex = 4;
            this.button2.Text = "Merge Data Tables";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.Merge_Tables_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(90, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Mods Folder Path";
            // 
            // button1
            // 
            this.button1.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.button1.Location = new System.Drawing.Point(364, 382);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(110, 23);
            this.button1.TabIndex = 6;
            this.button1.Text = "Make .IshtarPatch";
            this.toolTip1.SetToolTip(this.button1, "Create an Ishtar Patch file of a mod used for merging\r\ninstead of scanning and un" +
        "packing each mod.");
            this.button1.Click += new System.EventHandler(this.CreateIshtarPatch_Click);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton1});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(486, 25);
            this.toolStrip1.TabIndex = 9;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.TSB_PartialMerge,
            this.TSB_Nested,
            this.TSB_Patch,
            this.TSB_Scan,
            this.parallelScaningToolStripMenuItem,
            this.makeIshtarPatchAfterScanToolStripMenuItem});
            this.toolStripButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton1.Image")));
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(62, 22);
            this.toolStripButton1.Text = "Options";
            // 
            // TSB_PartialMerge
            // 
            this.TSB_PartialMerge.CheckOnClick = true;
            this.TSB_PartialMerge.Name = "TSB_PartialMerge";
            this.TSB_PartialMerge.Size = new System.Drawing.Size(180, 22);
            this.TSB_PartialMerge.Text = "Partial Merge";
            this.TSB_PartialMerge.ToolTipText = "Enables Partial Merging which only merges new\r\nentries while ignoring edited vani" +
    "lla ones.\r\n";
            this.TSB_PartialMerge.Click += new System.EventHandler(this.TSB_PartialMerge_Click);
            // 
            // TSB_Nested
            // 
            this.TSB_Nested.Name = "TSB_Nested";
            this.TSB_Nested.Size = new System.Drawing.Size(180, 22);
            this.TSB_Nested.Text = "Nested Merge Pak";
            this.TSB_Nested.ToolTipText = "When enabled merged pak with by placed in a";
            this.TSB_Nested.Click += new System.EventHandler(this.nestedPak_CheckedChanged);
            // 
            // TSB_Scan
            // 
            this.TSB_Scan.Checked = true;
            this.TSB_Scan.CheckOnClick = true;
            this.TSB_Scan.CheckState = System.Windows.Forms.CheckState.Checked;
            this.TSB_Scan.Name = "TSB_Scan";
            this.TSB_Scan.Size = new System.Drawing.Size(180, 22);
            this.TSB_Scan.Text = "Enable Scan Merge";
            this.TSB_Scan.ToolTipText = "Merge mods by scanning and unpacking mods, method\r\nis much slower but will be the" +
    " most acurite.";
            this.TSB_Scan.Click += new System.EventHandler(this.TSB_Scan_Click);
            // 
            // TSB_Patch
            // 
            this.TSB_Patch.Checked = true;
            this.TSB_Patch.CheckOnClick = true;
            this.TSB_Patch.CheckState = System.Windows.Forms.CheckState.Checked;
            this.TSB_Patch.Name = "TSB_Patch";
            this.TSB_Patch.Size = new System.Drawing.Size(180, 22);
            this.TSB_Patch.Text = "Enable Patch Merge";
            this.TSB_Patch.ToolTipText = "Merge mods based on .IshtarPatch files, method is much\r\nfaster but can be out of " +
    "date compared to the mod.";
            this.TSB_Patch.Click += new System.EventHandler(this.TSB_Patch_Click);
            // 
            // parallelScaningToolStripMenuItem
            // 
            this.parallelScaningToolStripMenuItem.CheckOnClick = true;
            this.parallelScaningToolStripMenuItem.Name = "parallelScaningToolStripMenuItem";
            this.parallelScaningToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.parallelScaningToolStripMenuItem.Text = "Parallel Scanning";
            this.parallelScaningToolStripMenuItem.ToolTipText = "Use asynchronous Paralle.ForEach while scanning paks,\r\nthis method might miss som" +
    "e mods but is much faster.\r\n";
            this.parallelScaningToolStripMenuItem.Click += new System.EventHandler(this.parallelScaningToolStripMenuItem_Click);
            // 
            // makeIshtarPatchAfterScanToolStripMenuItem
            // 
            this.makeIshtarPatchAfterScanToolStripMenuItem.Checked = true;
            this.makeIshtarPatchAfterScanToolStripMenuItem.CheckOnClick = true;
            this.makeIshtarPatchAfterScanToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.makeIshtarPatchAfterScanToolStripMenuItem.Name = "makeIshtarPatchAfterScanToolStripMenuItem";
            this.makeIshtarPatchAfterScanToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            this.makeIshtarPatchAfterScanToolStripMenuItem.Text = "Generate Ishtar Patch";
            this.makeIshtarPatchAfterScanToolStripMenuItem.ToolTipText = "If a pak is mergerable and the patch is either outdated\r\nor missing this will gen" +
    "erate a new IshtarPatch file.";
            this.makeIshtarPatchAfterScanToolStripMenuItem.Click += new System.EventHandler(this.makeIshtarPatchAfterScanToolStripMenuItem_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(486, 411);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.B_GetModsPath);
            this.Controls.Add(this.TB_ModsPath);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(502, 450);
            this.Name = "Form1";
            this.Text = "Ishtar";
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox TB_ModsPath;
        private System.Windows.Forms.Button B_GetModsPath;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripDropDownButton toolStripButton1;
        private System.Windows.Forms.ToolStripMenuItem TSB_PartialMerge;
        private System.Windows.Forms.ToolStripMenuItem TSB_Nested;
        private System.Windows.Forms.ToolStripMenuItem TSB_Patch;
        private System.Windows.Forms.ToolStripMenuItem TSB_Scan;
        private System.Windows.Forms.ToolStripMenuItem parallelScaningToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem makeIshtarPatchAfterScanToolStripMenuItem;
    }
}

