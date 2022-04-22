namespace SecureOne
{
    partial class MainForm
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.mainMenuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.plainDataListBox = new System.Windows.Forms.ListBox();
            this.encodedDataListBox = new System.Windows.Forms.ListBox();
            this.signButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.encryptButton = new System.Windows.Forms.Button();
            this.verifyDecryptButton = new System.Windows.Forms.Button();
            this.fileInfolistBox = new System.Windows.Forms.ListBox();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.mainMenuStrip.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainMenuStrip
            // 
            this.mainMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.optionsToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.mainMenuStrip.Location = new System.Drawing.Point(0, 0);
            this.mainMenuStrip.Name = "mainMenuStrip";
            this.mainMenuStrip.Size = new System.Drawing.Size(1214, 24);
            this.mainMenuStrip.TabIndex = 0;
            this.mainMenuStrip.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.toolStripMenuItem1,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(48, 20);
            this.fileToolStripMenuItem.Text = "Файл";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.openToolStripMenuItem.Text = "Открыть...";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(177, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Q)));
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.exitToolStripMenuItem.Text = "Выход";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(79, 20);
            this.optionsToolStripMenuItem.Text = "Настройки";
            this.optionsToolStripMenuItem.Click += new System.EventHandler(this.optionsToolStripMenuItem_Click);
            // 
            // openFileDialog
            // 
            this.openFileDialog.FileName = "Загрузка файлов";
            // 
            // plainDataListBox
            // 
            this.plainDataListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.plainDataListBox.FormattingEnabled = true;
            this.plainDataListBox.Location = new System.Drawing.Point(12, 53);
            this.plainDataListBox.Name = "plainDataListBox";
            this.plainDataListBox.Size = new System.Drawing.Size(462, 511);
            this.plainDataListBox.Sorted = true;
            this.plainDataListBox.TabIndex = 1;
            this.plainDataListBox.SelectedIndexChanged += new System.EventHandler(this.plainDataListBox_SelectedIndexChanged);
            this.plainDataListBox.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.plainDataListBox_PreviewKeyDown);
            // 
            // encodedDataListBox
            // 
            this.encodedDataListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.encodedDataListBox.FormattingEnabled = true;
            this.encodedDataListBox.Location = new System.Drawing.Point(494, 53);
            this.encodedDataListBox.Name = "encodedDataListBox";
            this.encodedDataListBox.Size = new System.Drawing.Size(462, 511);
            this.encodedDataListBox.TabIndex = 1;
            this.encodedDataListBox.SelectedIndexChanged += new System.EventHandler(this.encodedDataListBox_SelectedIndexChanged);
            this.encodedDataListBox.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.encodedDataListBox_PreviewKeyDown);
            // 
            // signButton
            // 
            this.signButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.signButton.Location = new System.Drawing.Point(12, 584);
            this.signButton.Name = "signButton";
            this.signButton.Size = new System.Drawing.Size(168, 23);
            this.signButton.TabIndex = 2;
            this.signButton.Text = "Пописать";
            this.signButton.UseVisualStyleBackColor = true;
            this.signButton.Click += new System.EventHandler(this.signButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 34);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(130, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Незащищенные данные";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(491, 34);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(117, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Защищенные данные";
            // 
            // encryptButton
            // 
            this.encryptButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.encryptButton.Location = new System.Drawing.Point(306, 584);
            this.encryptButton.Name = "encryptButton";
            this.encryptButton.Size = new System.Drawing.Size(168, 23);
            this.encryptButton.TabIndex = 2;
            this.encryptButton.Text = "Зашифровать";
            this.encryptButton.UseVisualStyleBackColor = true;
            // 
            // verifyDecryptButton
            // 
            this.verifyDecryptButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.verifyDecryptButton.Location = new System.Drawing.Point(788, 584);
            this.verifyDecryptButton.Name = "verifyDecryptButton";
            this.verifyDecryptButton.Size = new System.Drawing.Size(168, 23);
            this.verifyDecryptButton.TabIndex = 2;
            this.verifyDecryptButton.Text = "Расшифровать";
            this.verifyDecryptButton.UseVisualStyleBackColor = true;
            // 
            // fileInfolistBox
            // 
            this.fileInfolistBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fileInfolistBox.BackColor = System.Drawing.SystemColors.Control;
            this.fileInfolistBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.fileInfolistBox.CausesValidation = false;
            this.fileInfolistBox.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.fileInfolistBox.FormattingEnabled = true;
            this.fileInfolistBox.Location = new System.Drawing.Point(972, 53);
            this.fileInfolistBox.Name = "fileInfolistBox";
            this.fileInfolistBox.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.fileInfolistBox.Size = new System.Drawing.Size(230, 507);
            this.fileInfolistBox.TabIndex = 4;
            this.fileInfolistBox.TabStop = false;
            this.fileInfolistBox.UseTabStops = false;
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 619);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(1214, 22);
            this.statusStrip.TabIndex = 5;
            this.statusStrip.Text = "statusStrip1";
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(68, 20);
            this.helpToolStripMenuItem.Text = "Помощь";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F1)));
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(204, 22);
            this.aboutToolStripMenuItem.Text = "О программе...";
            // 
            // toolStripStatusLabel
            // 
            this.toolStripStatusLabel.Name = "toolStripStatusLabel";
            this.toolStripStatusLabel.Size = new System.Drawing.Size(38, 17);
            this.toolStripStatusLabel.Text = "Готов";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1214, 641);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.fileInfolistBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.encryptButton);
            this.Controls.Add(this.verifyDecryptButton);
            this.Controls.Add(this.signButton);
            this.Controls.Add(this.encodedDataListBox);
            this.Controls.Add(this.plainDataListBox);
            this.Controls.Add(this.mainMenuStrip);
            this.MainMenuStrip = this.mainMenuStrip;
            this.MinimumSize = new System.Drawing.Size(1230, 680);
            this.Name = "MainForm";
            this.Text = "SecureOne";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.mainMenuStrip.ResumeLayout(false);
            this.mainMenuStrip.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip mainMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.ListBox plainDataListBox;
        private System.Windows.Forms.ListBox encodedDataListBox;
        private System.Windows.Forms.Button signButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button encryptButton;
        private System.Windows.Forms.Button verifyDecryptButton;
        private System.Windows.Forms.ListBox fileInfolistBox;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
    }
}

