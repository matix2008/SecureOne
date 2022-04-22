using System;
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
using SecureOneLib.Utilities;

namespace SecureOne
{
    //TODO: Добавить обработку опций: "Автоматически искать сертификат для проверки подписи"
    //TODO: Добавить обработку опций: "Использовать только сертификаты контрагентов"
    //TODO: Опция "Всегда использовать ГОСТ" не логична, т.к. криптография определяется сертификатом отправителя
    //TODO: Добавить обработку опций: "Очищать список незащищенных файлов при очередной загрузке"
    //TODO: Добавить обработку опций: "Путь к рабочему каталогу"
    public partial class OptionsForm : Form
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Конструирует форму
        /// </summary>
        /// <param name="opt">Ссылка на настройки приложения</param>
        public OptionsForm(Settings opt)
        {
            InitializeComponent();

            SenderCertificate = opt.SenderCertificate;
            RecipientsCertificatesCollection = opt.RecipientsCertificatesCollection;
            AlwaysUseAttachedSign = opt.AlwaysUseAttachedSign;
            AlwaysUseGost = opt.AlwaysUseGost;
        }

        public CertificateWrapper SenderCertificate { get; protected set; }
        public CertificateCollectionWrapper RecipientsCertificatesCollection { get; protected set; }
        public bool AlwaysUseAttachedSign { get; protected set; }
        public bool AlwaysUseGost { get; protected set; }

        private void OptionsForm_Load(object sender, EventArgs e)
        {
            try
            {
                recipientsCertificatesListBox.Items.Clear();

                alwaysUseAttachedSignCheckBox.Checked = AlwaysUseAttachedSign;
                alwaysUseGostCheckBox.Checked = AlwaysUseGost;

                sendersCertificateTextBox.Text = SenderCertificate?.ToString() ?? "Выберите сертификат с закрытым ключом.";

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

            this.alwaysUseAttachedSignCheckBox.CheckedChanged += new System.EventHandler(this.options_CheckedChanged);
            this.alwaysUseGostCheckBox.CheckedChanged += new System.EventHandler(this.options_CheckedChanged);
        }

        private void addSenderCertificateButton_Click(object sender, EventArgs e)
        {
            // Открываем форму для выбора одного сертификата
            ChooseCertForm ccf = new ChooseCertForm(false);
            if (ccf.ShowDialog() == DialogResult.OK)
            {
                SenderCertificate = ccf.SelectedCertificates[0];
                sendersCertificateTextBox.Text = SenderCertificate.ToString();
            }
        }

        private void addRecipientCertificateButton_Click(object sender, EventArgs e)
        {
            // Открываем форму для выбора списка сертификатов
            ChooseCertForm ccf = new ChooseCertForm(true);
            if (ccf.ShowDialog() == DialogResult.OK)
            {
                var arr = ccf.SelectedCertificates.ToArray();
                RecipientsCertificatesCollection = new CertificateCollectionWrapper(arr);

                recipientsCertificatesListBox.Items.Clear();
                recipientsCertificatesListBox.Items.AddRange(arr);
            }
        }

        private void options_CheckedChanged(object sender, EventArgs e)
        {
            AlwaysUseAttachedSign = alwaysUseAttachedSignCheckBox.Checked;
            AlwaysUseGost = alwaysUseGostCheckBox.Checked;
        }

        private void clearButton_Click(object sender, EventArgs e)
        {
            SenderCertificate = null;
            RecipientsCertificatesCollection = null;
            sendersCertificateTextBox.Text = "Выберите сертификат с закрытым ключом.";
            recipientsCertificatesListBox.Items.Clear();
            AlwaysUseAttachedSign = true;
            AlwaysUseGost = true;
        }
    }
}
