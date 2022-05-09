namespace SecureOne
{
    partial class OptionsForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.ownerCertificateTextBox = new System.Windows.Forms.TextBox();
            this.addSenderCertificateButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.addRecipientCertificateButton = new System.Windows.Forms.Button();
            this.recipientsCertificatesListBox = new System.Windows.Forms.ListBox();
            this.OKButton = new System.Windows.Forms.Button();
            this.CnclButton = new System.Windows.Forms.Button();
            this.alwaysUseDetachedSignCheckBox = new System.Windows.Forms.CheckBox();
            this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.label3 = new System.Windows.Forms.Label();
            this.workingFolderTextBox = new System.Windows.Forms.TextBox();
            this.setupWorkingFolderButton = new System.Windows.Forms.Button();
            this.clearButton = new System.Windows.Forms.Button();
            this.alwaysUseCustomEncFrmtCheckBox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 41);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(125, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Сертификат владельца";
            // 
            // ownerCertificateTextBox
            // 
            this.ownerCertificateTextBox.Location = new System.Drawing.Point(169, 41);
            this.ownerCertificateTextBox.Name = "ownerCertificateTextBox";
            this.ownerCertificateTextBox.ReadOnly = true;
            this.ownerCertificateTextBox.Size = new System.Drawing.Size(484, 20);
            this.ownerCertificateTextBox.TabIndex = 1;
            // 
            // addSenderCertificateButton
            // 
            this.addSenderCertificateButton.Location = new System.Drawing.Point(659, 39);
            this.addSenderCertificateButton.Name = "addSenderCertificateButton";
            this.addSenderCertificateButton.Size = new System.Drawing.Size(40, 23);
            this.addSenderCertificateButton.TabIndex = 2;
            this.addSenderCertificateButton.Text = "...";
            this.addSenderCertificateButton.UseVisualStyleBackColor = true;
            this.addSenderCertificateButton.Click += new System.EventHandler(this.addSenderCertificateButton_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 67);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(148, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Сертификаты контрагентов";
            // 
            // addRecipientCertificateButton
            // 
            this.addRecipientCertificateButton.Location = new System.Drawing.Point(659, 191);
            this.addRecipientCertificateButton.Name = "addRecipientCertificateButton";
            this.addRecipientCertificateButton.Size = new System.Drawing.Size(40, 23);
            this.addRecipientCertificateButton.TabIndex = 2;
            this.addRecipientCertificateButton.Text = "...";
            this.addRecipientCertificateButton.UseVisualStyleBackColor = true;
            this.addRecipientCertificateButton.Click += new System.EventHandler(this.addRecipientCertificateButton_Click);
            // 
            // recipientsCertificatesListBox
            // 
            this.recipientsCertificatesListBox.FormattingEnabled = true;
            this.recipientsCertificatesListBox.Location = new System.Drawing.Point(169, 67);
            this.recipientsCertificatesListBox.Name = "recipientsCertificatesListBox";
            this.recipientsCertificatesListBox.Size = new System.Drawing.Size(484, 147);
            this.recipientsCertificatesListBox.TabIndex = 3;
            // 
            // OKButton
            // 
            this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OKButton.Location = new System.Drawing.Point(624, 288);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(75, 23);
            this.OKButton.TabIndex = 4;
            this.OKButton.Text = "&OK";
            this.OKButton.UseVisualStyleBackColor = true;
            // 
            // CnclButton
            // 
            this.CnclButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.CnclButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CnclButton.Location = new System.Drawing.Point(12, 288);
            this.CnclButton.Name = "CnclButton";
            this.CnclButton.Size = new System.Drawing.Size(75, 23);
            this.CnclButton.TabIndex = 4;
            this.CnclButton.Text = "Отмена";
            this.CnclButton.UseVisualStyleBackColor = true;
            // 
            // alwaysUseDetachedSignCheckBox
            // 
            this.alwaysUseDetachedSignCheckBox.AutoSize = true;
            this.alwaysUseDetachedSignCheckBox.Location = new System.Drawing.Point(169, 224);
            this.alwaysUseDetachedSignCheckBox.Name = "alwaysUseDetachedSignCheckBox";
            this.alwaysUseDetachedSignCheckBox.Size = new System.Drawing.Size(262, 17);
            this.alwaysUseDetachedSignCheckBox.TabIndex = 5;
            this.alwaysUseDetachedSignCheckBox.Text = "Всегда использовать отсоединенную подпись";
            this.alwaysUseDetachedSignCheckBox.UseVisualStyleBackColor = true;
            // 
            // folderBrowserDialog
            // 
            this.folderBrowserDialog.Description = "Выбор рабочего каталога";
            this.folderBrowserDialog.RootFolder = System.Environment.SpecialFolder.MyComputer;
            this.folderBrowserDialog.ShowNewFolderButton = false;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(15, 15);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(92, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Рабочий каталог";
            // 
            // workingFolderTextBox
            // 
            this.workingFolderTextBox.Location = new System.Drawing.Point(169, 15);
            this.workingFolderTextBox.Name = "workingFolderTextBox";
            this.workingFolderTextBox.ReadOnly = true;
            this.workingFolderTextBox.Size = new System.Drawing.Size(484, 20);
            this.workingFolderTextBox.TabIndex = 1;
            // 
            // setupWorkingFolderButton
            // 
            this.setupWorkingFolderButton.Location = new System.Drawing.Point(659, 13);
            this.setupWorkingFolderButton.Name = "setupWorkingFolderButton";
            this.setupWorkingFolderButton.Size = new System.Drawing.Size(40, 23);
            this.setupWorkingFolderButton.TabIndex = 2;
            this.setupWorkingFolderButton.Text = "...";
            this.setupWorkingFolderButton.UseVisualStyleBackColor = true;
            this.setupWorkingFolderButton.Click += new System.EventHandler(this.setupWorkingFolderButton_Click);
            // 
            // clearButton
            // 
            this.clearButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.clearButton.Location = new System.Drawing.Point(543, 288);
            this.clearButton.Name = "clearButton";
            this.clearButton.Size = new System.Drawing.Size(75, 23);
            this.clearButton.TabIndex = 6;
            this.clearButton.Text = "Сброс";
            this.clearButton.UseVisualStyleBackColor = true;
            this.clearButton.Click += new System.EventHandler(this.clearButton_Click);
            // 
            // alwaysUseCustomEncFrmtCheckBox
            // 
            this.alwaysUseCustomEncFrmtCheckBox.AutoSize = true;
            this.alwaysUseCustomEncFrmtCheckBox.Location = new System.Drawing.Point(169, 247);
            this.alwaysUseCustomEncFrmtCheckBox.Name = "alwaysUseCustomEncFrmtCheckBox";
            this.alwaysUseCustomEncFrmtCheckBox.Size = new System.Drawing.Size(315, 17);
            this.alwaysUseCustomEncFrmtCheckBox.TabIndex = 5;
            this.alwaysUseCustomEncFrmtCheckBox.Text = "Всегда использовать собственный формат шифрования";
            this.alwaysUseCustomEncFrmtCheckBox.UseVisualStyleBackColor = true;
            this.alwaysUseCustomEncFrmtCheckBox.CheckedChanged += new System.EventHandler(this.options_CheckedChanged);
            // 
            // OptionsForm
            // 
            this.AcceptButton = this.OKButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.CnclButton;
            this.ClientSize = new System.Drawing.Size(711, 323);
            this.Controls.Add(this.clearButton);
            this.Controls.Add(this.alwaysUseCustomEncFrmtCheckBox);
            this.Controls.Add(this.alwaysUseDetachedSignCheckBox);
            this.Controls.Add(this.CnclButton);
            this.Controls.Add(this.OKButton);
            this.Controls.Add(this.recipientsCertificatesListBox);
            this.Controls.Add(this.addRecipientCertificateButton);
            this.Controls.Add(this.setupWorkingFolderButton);
            this.Controls.Add(this.addSenderCertificateButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.workingFolderTextBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.ownerCertificateTextBox);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OptionsForm";
            this.Text = "Настройки программы";
            this.Load += new System.EventHandler(this.OptionsForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox ownerCertificateTextBox;
        private System.Windows.Forms.Button addSenderCertificateButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button addRecipientCertificateButton;
        private System.Windows.Forms.ListBox recipientsCertificatesListBox;
        private System.Windows.Forms.Button OKButton;
        private System.Windows.Forms.Button CnclButton;
        private System.Windows.Forms.CheckBox alwaysUseDetachedSignCheckBox;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox workingFolderTextBox;
        private System.Windows.Forms.Button setupWorkingFolderButton;
        private System.Windows.Forms.Button clearButton;
        private System.Windows.Forms.CheckBox alwaysUseCustomEncFrmtCheckBox;
    }
}