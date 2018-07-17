namespace EncryptedSyslogReader
{
    partial class EncryptedPGPSyslogReader
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
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuSetPGPKeyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuOpenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuCloseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuClearToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuExitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lblConfigFile = new System.Windows.Forms.Label();
            this.txtPrivateKeyFileNameAndPath = new System.Windows.Forms.TextBox();
            this.lblLogTime = new System.Windows.Forms.Label();
            this.txtLogFileName = new System.Windows.Forms.TextBox();
            this.openSyslogFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.setPrivateKeyFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.lblPassphrase = new System.Windows.Forms.Label();
            this.txtPassphrase = new System.Windows.Forms.TextBox();
            this.txtDecryptedLog = new System.Windows.Forms.TextBox();
            this.groupDecryptedSyslog = new System.Windows.Forms.GroupBox();
            this.groupRawSyslog = new System.Windows.Forms.GroupBox();
            this.txtRawSyslog = new System.Windows.Forms.TextBox();
            this.menuStrip1.SuspendLayout();
            this.groupDecryptedSyslog.SuspendLayout();
            this.groupRawSyslog.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(813, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuSetPGPKeyToolStripMenuItem,
            this.menuOpenToolStripMenuItem,
            this.menuCloseToolStripMenuItem,
            this.menuClearToolStripMenuItem,
            this.menuExitToolStripMenuItem});
            this.fileToolStripMenuItem.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(38, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // menuSetPGPKeyToolStripMenuItem
            // 
            this.menuSetPGPKeyToolStripMenuItem.Name = "menuSetPGPKeyToolStripMenuItem";
            this.menuSetPGPKeyToolStripMenuItem.Size = new System.Drawing.Size(143, 22);
            this.menuSetPGPKeyToolStripMenuItem.Text = "Set PGP Key";
            this.menuSetPGPKeyToolStripMenuItem.Click += new System.EventHandler(this.setPGPKeyToolStripMenuItem_Click);
            // 
            // menuOpenToolStripMenuItem
            // 
            this.menuOpenToolStripMenuItem.Name = "menuOpenToolStripMenuItem";
            this.menuOpenToolStripMenuItem.Size = new System.Drawing.Size(143, 22);
            this.menuOpenToolStripMenuItem.Text = "Open";
            this.menuOpenToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // menuCloseToolStripMenuItem
            // 
            this.menuCloseToolStripMenuItem.Name = "menuCloseToolStripMenuItem";
            this.menuCloseToolStripMenuItem.Size = new System.Drawing.Size(143, 22);
            this.menuCloseToolStripMenuItem.Text = "Close";
            this.menuCloseToolStripMenuItem.Click += new System.EventHandler(this.menuCloseToolStripMenuItem_Click);
            // 
            // menuClearToolStripMenuItem
            // 
            this.menuClearToolStripMenuItem.Name = "menuClearToolStripMenuItem";
            this.menuClearToolStripMenuItem.Size = new System.Drawing.Size(143, 22);
            this.menuClearToolStripMenuItem.Text = "Clear All";
            this.menuClearToolStripMenuItem.Click += new System.EventHandler(this.menuClearToolStripMenuItem_Click);
            // 
            // menuExitToolStripMenuItem
            // 
            this.menuExitToolStripMenuItem.Name = "menuExitToolStripMenuItem";
            this.menuExitToolStripMenuItem.Size = new System.Drawing.Size(143, 22);
            this.menuExitToolStripMenuItem.Text = "Exit";
            this.menuExitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // lblConfigFile
            // 
            this.lblConfigFile.AutoSize = true;
            this.lblConfigFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblConfigFile.Location = new System.Drawing.Point(12, 40);
            this.lblConfigFile.Name = "lblConfigFile";
            this.lblConfigFile.Size = new System.Drawing.Size(100, 13);
            this.lblConfigFile.TabIndex = 6;
            this.lblConfigFile.Text = "Private Key File:";
            this.lblConfigFile.Click += new System.EventHandler(this.lblConfigFile_Click);
            // 
            // txtPrivateKeyFileNameAndPath
            // 
            this.txtPrivateKeyFileNameAndPath.Location = new System.Drawing.Point(118, 40);
            this.txtPrivateKeyFileNameAndPath.Name = "txtPrivateKeyFileNameAndPath";
            this.txtPrivateKeyFileNameAndPath.ReadOnly = true;
            this.txtPrivateKeyFileNameAndPath.Size = new System.Drawing.Size(562, 20);
            this.txtPrivateKeyFileNameAndPath.TabIndex = 7;
            // 
            // lblLogTime
            // 
            this.lblLogTime.AutoSize = true;
            this.lblLogTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLogTime.Location = new System.Drawing.Point(12, 108);
            this.lblLogTime.Name = "lblLogTime";
            this.lblLogTime.Size = new System.Drawing.Size(56, 13);
            this.lblLogTime.TabIndex = 9;
            this.lblLogTime.Text = "Log File:";
            // 
            // txtLogFileName
            // 
            this.txtLogFileName.Location = new System.Drawing.Point(118, 101);
            this.txtLogFileName.Name = "txtLogFileName";
            this.txtLogFileName.ReadOnly = true;
            this.txtLogFileName.Size = new System.Drawing.Size(562, 20);
            this.txtLogFileName.TabIndex = 10;
            // 
            // openSyslogFileDialog
            // 
            this.openSyslogFileDialog.FileName = "configfile.xml";
            // 
            // setPrivateKeyFileDialog
            // 
            this.setPrivateKeyFileDialog.FileName = "configfile.xml";
            // 
            // lblPassphrase
            // 
            this.lblPassphrase.AutoSize = true;
            this.lblPassphrase.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPassphrase.Location = new System.Drawing.Point(12, 74);
            this.lblPassphrase.Name = "lblPassphrase";
            this.lblPassphrase.Size = new System.Drawing.Size(76, 13);
            this.lblPassphrase.TabIndex = 11;
            this.lblPassphrase.Text = "Passphrase:";
            // 
            // txtPassphrase
            // 
            this.txtPassphrase.Location = new System.Drawing.Point(118, 71);
            this.txtPassphrase.Name = "txtPassphrase";
            this.txtPassphrase.Size = new System.Drawing.Size(322, 20);
            this.txtPassphrase.TabIndex = 12;
            this.txtPassphrase.TextChanged += new System.EventHandler(this.txtPassphrase_TextChanged);
            // 
            // txtDecryptedLog
            // 
            this.txtDecryptedLog.Location = new System.Drawing.Point(11, 23);
            this.txtDecryptedLog.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.txtDecryptedLog.Multiline = true;
            this.txtDecryptedLog.Name = "txtDecryptedLog";
            this.txtDecryptedLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtDecryptedLog.Size = new System.Drawing.Size(722, 178);
            this.txtDecryptedLog.TabIndex = 13;
            // 
            // groupDecryptedSyslog
            // 
            this.groupDecryptedSyslog.Controls.Add(this.txtDecryptedLog);
            this.groupDecryptedSyslog.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupDecryptedSyslog.Location = new System.Drawing.Point(15, 135);
            this.groupDecryptedSyslog.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupDecryptedSyslog.Name = "groupDecryptedSyslog";
            this.groupDecryptedSyslog.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupDecryptedSyslog.Size = new System.Drawing.Size(751, 212);
            this.groupDecryptedSyslog.TabIndex = 14;
            this.groupDecryptedSyslog.TabStop = false;
            this.groupDecryptedSyslog.Text = "DecryptedSysLog";
            this.groupDecryptedSyslog.Enter += new System.EventHandler(this.groupBox1_Enter);
            // 
            // groupRawSyslog
            // 
            this.groupRawSyslog.Controls.Add(this.txtRawSyslog);
            this.groupRawSyslog.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupRawSyslog.Location = new System.Drawing.Point(15, 369);
            this.groupRawSyslog.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupRawSyslog.Name = "groupRawSyslog";
            this.groupRawSyslog.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupRawSyslog.Size = new System.Drawing.Size(751, 222);
            this.groupRawSyslog.TabIndex = 15;
            this.groupRawSyslog.TabStop = false;
            this.groupRawSyslog.Text = "RawSyslog";
            // 
            // txtRawSyslog
            // 
            this.txtRawSyslog.Location = new System.Drawing.Point(11, 27);
            this.txtRawSyslog.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.txtRawSyslog.Multiline = true;
            this.txtRawSyslog.Name = "txtRawSyslog";
            this.txtRawSyslog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtRawSyslog.Size = new System.Drawing.Size(722, 179);
            this.txtRawSyslog.TabIndex = 0;
            // 
            // EncryptedPGPSyslogReader
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(813, 623);
            this.Controls.Add(this.groupRawSyslog);
            this.Controls.Add(this.groupDecryptedSyslog);
            this.Controls.Add(this.txtPassphrase);
            this.Controls.Add(this.lblPassphrase);
            this.Controls.Add(this.txtLogFileName);
            this.Controls.Add(this.lblLogTime);
            this.Controls.Add(this.txtPrivateKeyFileNameAndPath);
            this.Controls.Add(this.lblConfigFile);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "EncryptedPGPSyslogReader";
            this.Text = "Encrypted Syslog Reader";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.groupDecryptedSyslog.ResumeLayout(false);
            this.groupDecryptedSyslog.PerformLayout();
            this.groupRawSyslog.ResumeLayout(false);
            this.groupRawSyslog.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem menuOpenToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem menuCloseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem menuExitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem menuSetPGPKeyToolStripMenuItem;
        private System.Windows.Forms.Label lblConfigFile;
        private System.Windows.Forms.TextBox txtPrivateKeyFileNameAndPath;
        private System.Windows.Forms.Label lblLogTime;
        private System.Windows.Forms.TextBox txtLogFileName;
        private System.Windows.Forms.OpenFileDialog openSyslogFileDialog;
        private System.Windows.Forms.OpenFileDialog setPrivateKeyFileDialog;
        private System.Windows.Forms.Label lblPassphrase;
        private System.Windows.Forms.TextBox txtPassphrase;
        private System.Windows.Forms.ToolStripMenuItem menuClearToolStripMenuItem;
        private System.Windows.Forms.TextBox txtDecryptedLog;
        private System.Windows.Forms.GroupBox groupDecryptedSyslog;
        private System.Windows.Forms.GroupBox groupRawSyslog;
        private System.Windows.Forms.TextBox txtRawSyslog;
    }
}

