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
            this.sendersCertificateTextBox = new System.Windows.Forms.TextBox();
            this.addSenderCertificateButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.addRecipientCertificateButton = new System.Windows.Forms.Button();
            this.recipientsCertificatesListBox = new System.Windows.Forms.ListBox();
            this.OKButton = new System.Windows.Forms.Button();
            this.CnclButton = new System.Windows.Forms.Button();
            this.alwaysUseAttachedSignCheckBox = new System.Windows.Forms.CheckBox();
            this.alwaysUseGostCheckBox = new System.Windows.Forms.CheckBox();
            this.clearButton = new System.Windows.Forms.Button();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.checkBox3 = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(135, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Сертификат отправителя";
            // 
            // sendersCertificateTextBox
            // 
            this.sendersCertificateTextBox.Location = new System.Drawing.Point(166, 9);
            this.sendersCertificateTextBox.Name = "sendersCertificateTextBox";
            this.sendersCertificateTextBox.ReadOnly = true;
            this.sendersCertificateTextBox.Size = new System.Drawing.Size(484, 20);
            this.sendersCertificateTextBox.TabIndex = 1;
            // 
            // addSenderCertificateButton
            // 
            this.addSenderCertificateButton.Location = new System.Drawing.Point(656, 7);
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
            this.label2.Location = new System.Drawing.Point(12, 35);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(148, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Сертификаты контрагентов";
            // 
            // addRecipientCertificateButton
            // 
            this.addRecipientCertificateButton.Location = new System.Drawing.Point(656, 159);
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
            this.recipientsCertificatesListBox.Location = new System.Drawing.Point(166, 35);
            this.recipientsCertificatesListBox.Name = "recipientsCertificatesListBox";
            this.recipientsCertificatesListBox.Size = new System.Drawing.Size(484, 147);
            this.recipientsCertificatesListBox.TabIndex = 3;
            // 
            // OKButton
            // 
            this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OKButton.Location = new System.Drawing.Point(624, 366);
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
            this.CnclButton.Location = new System.Drawing.Point(12, 366);
            this.CnclButton.Name = "CnclButton";
            this.CnclButton.Size = new System.Drawing.Size(75, 23);
            this.CnclButton.TabIndex = 4;
            this.CnclButton.Text = "Отмена";
            this.CnclButton.UseVisualStyleBackColor = true;
            // 
            // alwaysUseAttachedSignCheckBox
            // 
            this.alwaysUseAttachedSignCheckBox.AutoSize = true;
            this.alwaysUseAttachedSignCheckBox.Checked = true;
            this.alwaysUseAttachedSignCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.alwaysUseAttachedSignCheckBox.Location = new System.Drawing.Point(166, 192);
            this.alwaysUseAttachedSignCheckBox.Name = "alwaysUseAttachedSignCheckBox";
            this.alwaysUseAttachedSignCheckBox.Size = new System.Drawing.Size(354, 17);
            this.alwaysUseAttachedSignCheckBox.TabIndex = 5;
            this.alwaysUseAttachedSignCheckBox.Text = "Всегда использовать присоединенную подписи, если возможно";
            this.alwaysUseAttachedSignCheckBox.UseVisualStyleBackColor = true;
            // 
            // alwaysUseGostCheckBox
            // 
            this.alwaysUseGostCheckBox.AutoSize = true;
            this.alwaysUseGostCheckBox.Checked = true;
            this.alwaysUseGostCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.alwaysUseGostCheckBox.Location = new System.Drawing.Point(166, 215);
            this.alwaysUseGostCheckBox.Name = "alwaysUseGostCheckBox";
            this.alwaysUseGostCheckBox.Size = new System.Drawing.Size(252, 17);
            this.alwaysUseGostCheckBox.TabIndex = 5;
            this.alwaysUseGostCheckBox.Text = "Всегда использовать ГОСТ, если возможно";
            this.alwaysUseGostCheckBox.UseVisualStyleBackColor = true;
            // 
            // clearButton
            // 
            this.clearButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.clearButton.Location = new System.Drawing.Point(543, 366);
            this.clearButton.Name = "clearButton";
            this.clearButton.Size = new System.Drawing.Size(75, 23);
            this.clearButton.TabIndex = 6;
            this.clearButton.Text = "Сброс";
            this.clearButton.UseVisualStyleBackColor = true;
            this.clearButton.Click += new System.EventHandler(this.clearButton_Click);
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Checked = true;
            this.checkBox1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox1.Location = new System.Drawing.Point(166, 250);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(322, 17);
            this.checkBox1.TabIndex = 5;
            this.checkBox1.Text = "Автоматически искать сертификат для проверки подписи";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Checked = true;
            this.checkBox2.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox2.Location = new System.Drawing.Point(166, 273);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(280, 17);
            this.checkBox2.TabIndex = 5;
            this.checkBox2.Text = "Использовать только сертификаты контрагентов";
            this.checkBox2.UseVisualStyleBackColor = true;
            // 
            // checkBox3
            // 
            this.checkBox3.AutoSize = true;
            this.checkBox3.Checked = true;
            this.checkBox3.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox3.Location = new System.Drawing.Point(166, 294);
            this.checkBox3.Name = "checkBox3";
            this.checkBox3.Size = new System.Drawing.Size(359, 17);
            this.checkBox3.TabIndex = 5;
            this.checkBox3.Text = "Очищать список незащищенных файлов при очередной загрузке";
            this.checkBox3.UseVisualStyleBackColor = true;
            // 
            // OptionsForm
            // 
            this.AcceptButton = this.OKButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.CnclButton;
            this.ClientSize = new System.Drawing.Size(711, 401);
            this.Controls.Add(this.clearButton);
            this.Controls.Add(this.checkBox3);
            this.Controls.Add(this.checkBox2);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.alwaysUseGostCheckBox);
            this.Controls.Add(this.alwaysUseAttachedSignCheckBox);
            this.Controls.Add(this.CnclButton);
            this.Controls.Add(this.OKButton);
            this.Controls.Add(this.recipientsCertificatesListBox);
            this.Controls.Add(this.addRecipientCertificateButton);
            this.Controls.Add(this.addSenderCertificateButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.sendersCertificateTextBox);
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
        private System.Windows.Forms.TextBox sendersCertificateTextBox;
        private System.Windows.Forms.Button addSenderCertificateButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button addRecipientCertificateButton;
        private System.Windows.Forms.ListBox recipientsCertificatesListBox;
        private System.Windows.Forms.Button OKButton;
        private System.Windows.Forms.Button CnclButton;
        private System.Windows.Forms.CheckBox alwaysUseAttachedSignCheckBox;
        private System.Windows.Forms.CheckBox alwaysUseGostCheckBox;
        private System.Windows.Forms.Button clearButton;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.CheckBox checkBox3;
    }
}