﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography.X509Certificates;
using SecureOneLib;

namespace SecureOne
{
    /// <summary>
    /// Реализует диалог настройки параметров приложения
    /// </summary>
    public partial class OptionsForm : Form
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();    // Объект логирования

        /// <summary>
        /// Конструирует форму
        /// </summary>
        /// <param name="opt">Ссылка на настройки приложения</param>
        public OptionsForm(Settings opt)
        {
            InitializeComponent();

            OwnerWorkingFolder = opt.OwnerWorkingFolder;
            OwnerCertificate = opt.OwnerCertificate;
            RecipientsCertificatesCollection = opt.RecipientsCertificatesCollection;
            AllwaysUseDetachedSign = opt.AllwaysUseDetachedSign;
            AllwaysUseCustomEncFrmt = opt.AllwaysUseCustomEncFrmt;

        }

        /// <summary>
        /// Рабочий каталог
        /// </summary>
        public string OwnerWorkingFolder { get; protected set; }
        /// <summary>
        /// Сертификат владельца
        /// </summary>
        public CertificateWrapper OwnerCertificate { get; protected set; }
        /// <summary>
        /// Коллекция сертификатов контрагентов
        /// </summary>
        public CertificateCollectionWrapper RecipientsCertificatesCollection { get; protected set; }
        /// <summary>
        /// Флаг использования отсоединенной подписи
        /// </summary>
        public bool AllwaysUseDetachedSign { get; protected set; }
        /// <summary>
        /// Флаг использования собственного формата шифрования
        /// </summary>
        public bool AllwaysUseCustomEncFrmt { get; protected set; }

        /// <summary>
        /// Обрабатывает событие загрузки формы
        /// </summary>
        private void OptionsForm_Load(object sender, EventArgs e)
        {
            try
            {
                recipientsCertificatesListBox.Items.Clear();

                
                alwaysUseDetachedSignCheckBox.Checked = AllwaysUseDetachedSign;
                alwaysUseCustomEncFrmtCheckBox.Checked = AllwaysUseCustomEncFrmt;

                ownerCertificateTextBox.Text = OwnerCertificate?.ToString() ?? "Выберите сертификат с закрытым ключом.";
                workingFolderTextBox.Text = OwnerWorkingFolder.Length == 0 ? "Выберите рабочий каталог." : OwnerWorkingFolder;

                if (RecipientsCertificatesCollection != null)
                {
                    foreach (X509Certificate2 cert in RecipientsCertificatesCollection.Value)
                    {
                        CertificateWrapper cw = new CertificateWrapper(cert);
                        recipientsCertificatesListBox.Items.Add(cw);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error during loading application settings to the options form.");
                MessageBox.Show($"Error: {ex.Message}");
            }

            this.alwaysUseDetachedSignCheckBox.CheckedChanged += new System.EventHandler(this.options_CheckedChanged);
        }

        /// <summary>
        /// Обрабоатывает событие нажатия на копку выбора рабочего каталога
        /// </summary>
        private void setupWorkingFolderButton_Click(object sender, EventArgs e)
        {
            // Открываем диалог выбора каталога
            if (folderBrowserDialog.ShowDialog(this) == DialogResult.OK)
            {
                OwnerWorkingFolder = folderBrowserDialog.SelectedPath;
                workingFolderTextBox.Text = OwnerWorkingFolder;
            }
        }
        /// <summary>
        /// Обрабоатывает событие нажатия на копку выбора сертификата владельца
        /// </summary>
        private void addSenderCertificateButton_Click(object sender, EventArgs e)
        {
            // Открываем форму для выбора одного сертификата
            ChooseCertForm ccf = new ChooseCertForm(null, true, false);
            if (ccf.ShowDialog() == DialogResult.OK)
            {
                OwnerCertificate = ccf.SelectedCertificates[0];
                ownerCertificateTextBox.Text = OwnerCertificate.ToString();
            }
        }
        /// <summary>
        /// Обрабоатывает событие нажатия на копку выбора сертификата контагента
        /// </summary>
        private void addRecipientCertificateButton_Click(object sender, EventArgs e)
        {
            // Открываем форму для выбора списка сертификатов
            ChooseCertForm ccf = new ChooseCertForm(null, false, true);
            if (ccf.ShowDialog() == DialogResult.OK)
            {
                var arr = ccf.SelectedCertificates.ToArray();
                RecipientsCertificatesCollection = new CertificateCollectionWrapper(arr);

                recipientsCertificatesListBox.Items.Clear();
                recipientsCertificatesListBox.Items.AddRange(arr);
            }
        }
        /// <summary>
        /// Обрабоатывает событие изменения флагов
        /// </summary>
        private void options_CheckedChanged(object sender, EventArgs e)
        {
            AllwaysUseDetachedSign = alwaysUseDetachedSignCheckBox.Checked;
            AllwaysUseCustomEncFrmt = alwaysUseCustomEncFrmtCheckBox.Checked;
        }
        /// <summary>
        /// Обрабоатывает событие нажатия на копку сброса установленных параметров
        /// </summary>
        private void clearButton_Click(object sender, EventArgs e)
        {
            OwnerWorkingFolder = "";
            workingFolderTextBox.Text = "Выберите рабочий каталог.";

            OwnerCertificate = null;
            ownerCertificateTextBox.Text = "Выберите сертификат с закрытым ключом.";

            RecipientsCertificatesCollection = null;
            recipientsCertificatesListBox.Items.Clear();

            AllwaysUseDetachedSign = false;
            AllwaysUseCustomEncFrmt = false;
        }
    }
}
