namespace SecureOne
{
    partial class ChooseCertForm
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
            this.CertificatesListBox = new System.Windows.Forms.ListBox();
            this.OKButton = new System.Windows.Forms.Button();
            this.CnclButton = new System.Windows.Forms.Button();
            this.verifyButton = new System.Windows.Forms.Button();
            this.validDatesTextBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.issuerTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.algorithmTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.serialNumberTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.subjectNameTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // CertificatesListBox
            // 
            this.CertificatesListBox.FormattingEnabled = true;
            this.CertificatesListBox.Location = new System.Drawing.Point(12, 12);
            this.CertificatesListBox.Name = "CertificatesListBox";
            this.CertificatesListBox.Size = new System.Drawing.Size(441, 394);
            this.CertificatesListBox.TabIndex = 0;
            this.CertificatesListBox.SelectedIndexChanged += new System.EventHandler(this.CertificatesListBox_SelectedIndexChanged);
            // 
            // OKButton
            // 
            this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OKButton.Location = new System.Drawing.Point(713, 415);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(75, 23);
            this.OKButton.TabIndex = 1;
            this.OKButton.Text = "&OK";
            this.OKButton.UseVisualStyleBackColor = true;
            // 
            // CnclButton
            // 
            this.CnclButton.Location = new System.Drawing.Point(12, 415);
            this.CnclButton.Name = "CnclButton";
            this.CnclButton.Size = new System.Drawing.Size(75, 23);
            this.CnclButton.TabIndex = 1;
            this.CnclButton.Text = "Отмена";
            this.CnclButton.UseVisualStyleBackColor = true;
            this.CnclButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // verifyButton
            // 
            this.verifyButton.Location = new System.Drawing.Point(469, 242);
            this.verifyButton.Name = "verifyButton";
            this.verifyButton.Size = new System.Drawing.Size(157, 23);
            this.verifyButton.TabIndex = 16;
            this.verifyButton.Text = "Проверить сертификат";
            this.verifyButton.UseVisualStyleBackColor = true;
            this.verifyButton.Click += new System.EventHandler(this.verifyButton_Click);
            // 
            // validDatesTextBox
            // 
            this.validDatesTextBox.Location = new System.Drawing.Point(469, 205);
            this.validDatesTextBox.Name = "validDatesTextBox";
            this.validDatesTextBox.ReadOnly = true;
            this.validDatesTextBox.Size = new System.Drawing.Size(309, 20);
            this.validDatesTextBox.TabIndex = 11;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(466, 189);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(111, 13);
            this.label5.TabIndex = 6;
            this.label5.Text = "Дейсвителен с ... по";
            // 
            // issuerTextBox
            // 
            this.issuerTextBox.Location = new System.Drawing.Point(469, 160);
            this.issuerTextBox.Name = "issuerTextBox";
            this.issuerTextBox.ReadOnly = true;
            this.issuerTextBox.Size = new System.Drawing.Size(309, 20);
            this.issuerTextBox.TabIndex = 12;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(466, 144);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(125, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Издатель сертификата";
            // 
            // algorithmTextBox
            // 
            this.algorithmTextBox.Location = new System.Drawing.Point(469, 115);
            this.algorithmTextBox.Name = "algorithmTextBox";
            this.algorithmTextBox.ReadOnly = true;
            this.algorithmTextBox.Size = new System.Drawing.Size(309, 20);
            this.algorithmTextBox.TabIndex = 13;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(466, 99);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(56, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Алгоритм";
            // 
            // serialNumberTextBox
            // 
            this.serialNumberTextBox.Location = new System.Drawing.Point(469, 69);
            this.serialNumberTextBox.Name = "serialNumberTextBox";
            this.serialNumberTextBox.ReadOnly = true;
            this.serialNumberTextBox.Size = new System.Drawing.Size(309, 20);
            this.serialNumberTextBox.TabIndex = 14;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(466, 53);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(93, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "Серийный номер";
            // 
            // subjectNameTextBox
            // 
            this.subjectNameTextBox.Location = new System.Drawing.Point(469, 28);
            this.subjectNameTextBox.Name = "subjectNameTextBox";
            this.subjectNameTextBox.ReadOnly = true;
            this.subjectNameTextBox.Size = new System.Drawing.Size(309, 20);
            this.subjectNameTextBox.TabIndex = 15;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(466, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(49, 13);
            this.label1.TabIndex = 10;
            this.label1.Text = "Субъект";
            // 
            // ChooseCertForm
            // 
            this.AcceptButton = this.OKButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.verifyButton);
            this.Controls.Add(this.validDatesTextBox);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.issuerTextBox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.algorithmTextBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.serialNumberTextBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.subjectNameTextBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.CnclButton);
            this.Controls.Add(this.OKButton);
            this.Controls.Add(this.CertificatesListBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ChooseCertForm";
            this.Text = "Выбор сертификатов";
            this.Load += new System.EventHandler(this.ChooseCertForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox CertificatesListBox;
        private System.Windows.Forms.Button OKButton;
        private System.Windows.Forms.Button CnclButton;
        private System.Windows.Forms.Button verifyButton;
        private System.Windows.Forms.TextBox validDatesTextBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox issuerTextBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox algorithmTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox serialNumberTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox subjectNameTextBox;
        private System.Windows.Forms.Label label1;
    }
}