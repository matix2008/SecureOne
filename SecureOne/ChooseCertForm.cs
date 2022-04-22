using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Security.Cryptography.X509Certificates;
using SecureOneLib;
using SecureOneLib.Utilities;

namespace SecureOne
{
    public partial class ChooseCertForm : Form
    {
        public ChooseCertForm(bool multi)
        {
            InitializeComponent();

            SelectedCertificates = new List<CertificateWrapper>();

            verifyButton.Enabled = false;
            OKButton.Enabled = false;

            this.CertificatesListBox.SelectionMode = multi ? SelectionMode.MultiSimple : SelectionMode.One;
        }

        public List<CertificateWrapper> SelectedCertificates { get; protected set; }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            SelectedCertificates.Clear();
            this.Close();
        }

        private void verifyButton_Click(object sender, EventArgs e)
        {
            if (CertificatesListBox.SelectedIndex != -1)
            {
                try
                {
                    CertificateWrapper cw = CertificatesListBox.Items[CertificatesListBox.SelectedIndex] as CertificateWrapper;

                    X509Chain ch = new X509Chain();
                    ch.ChainPolicy.RevocationMode = X509RevocationMode.Online;

                    Cursor.Current = Cursors.WaitCursor;

                    if (ch.Build(cw.Value))
                    {
                        // Сертификат проверен
                        Utils.MessageHelper.Info(this, "Certificate is valid");
                    }
                    else
                    {
                        // Сохраняем подробный статус ошибки
                        string message = String.Empty;

                        foreach (var status in ch.ChainStatus)
                            message += status.StatusInformation;

                        Utils.MessageHelper.Warning(this, "Certificate is not valid. " + message);
                    }
                }
                catch(Exception ex)
                {
                    Utils.MessageHelper.Error(this, ex);
                }

                Cursor.Current = Cursors.Default;
            }
        }

        private void ChooseCertForm_Load(object sender, EventArgs e)
        {
            LoadCertificates();
        }

        private void LoadCertificates()
        {
            try
            {
                X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                store.Open(OpenFlags.ReadWrite | OpenFlags.OpenExistingOnly);

                X509Certificate2Collection fcollection = (X509Certificate2Collection)store.Certificates.
                    Find(X509FindType.FindByTimeValid, DateTime.Now, false);

                foreach (var cert in fcollection)
                {
                    if (cert.HasPrivateKey)
                        CertificatesListBox.Items.Add(new CertificateWrapper(cert));
                }

                store.Close();
            }
            catch(Exception ex)
            {
                Utils.MessageHelper.Error(this, ex);
            }
        }

        private void CertificatesListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (CertificatesListBox.SelectedIndex != -1)
            {
                SelectedCertificates.Clear();
                verifyButton.Enabled = false;
                OKButton.Enabled = false;

                if (CertificatesListBox.SelectedItems.Count == 1 )
                {
                    CertificateWrapper cw = CertificatesListBox.Items[CertificatesListBox.SelectedIndex] as CertificateWrapper;

                    // Отображаем информацию о выбранном сертификате
                    subjectNameTextBox.Text = cw.Value.Subject;
                    algorithmTextBox.Text = cw.Value.SignatureAlgorithm.FriendlyName;
                    serialNumberTextBox.Text = cw.Value.SerialNumber;
                    issuerTextBox.Text = cw.Value.Issuer;
                    validDatesTextBox.Text = String.Format("from {0} to {1}", cw.Value.NotBefore.ToShortDateString(), cw.Value.NotAfter.ToShortDateString());

                    verifyButton.Enabled = true;
                    OKButton.Enabled = true;

                    SelectedCertificates.Add(cw);
                }
                else
                {
                    subjectNameTextBox.Text = "";
                    algorithmTextBox.Text = "";
                    serialNumberTextBox.Text = "";
                    issuerTextBox.Text = "";
                    validDatesTextBox.Text = "";

                    foreach (var obj in CertificatesListBox.SelectedItems)
                        SelectedCertificates.Add(obj as CertificateWrapper);

                    OKButton.Enabled = true;
                }
            }
        }
    }
}
