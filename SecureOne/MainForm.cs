using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using SecureOneLib;
using SecureOneLib.Utilities;
using SecureOneLib.Crypto;

namespace SecureOne
{
    public partial class MainForm : Form
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private Settings _options = null;

        public MainForm()
        {
            InitializeComponent();
            //string path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        }

        #region Обработчики событий формы и меню

        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                _options = new Settings();
            }
            catch(Exception ex)
            {
                Utils.MessageHelper.Error(this, ex, "Ошибка при загрузке настроек приложения.");
            }

            signButton.Enabled = false;
            encryptButton.Enabled = false;
            verifyDecryptButton.Enabled = false;
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFormOptions();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog.InitialDirectory = "c:\\";
            openFileDialog.Filter = "Encypted files (*.p7s;*.enс;*.p7m;*.p7s.p7m;*.sig)|*.p7s;*.enс;*.p7m;*.p7s.p7m;*.sig|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.Multiselect = true;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                plainDataListBox.Items.Clear();

                Regex rx = new Regex("p7s|p7m|enc|.p7s.p7m|sig");

                foreach (var fn in openFileDialog.FileNames)
                {
                    string ext = Path.GetExtension(fn);
                    if (rx.IsMatch(ext))
                        encodedDataListBox.Items.Add(new FileWrapper(fn));
                    else
                        plainDataListBox.Items.Add(new PackageWrapper(fn));
                }
            }
        }

        #endregion  // Обработчики событий формы и меню

        #region Обработчики событий для списков (ListBox)

        private void plainDataListBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && plainDataListBox.SelectedIndex != -1)
            {
                plainDataListBox.Items.RemoveAt(plainDataListBox.SelectedIndex);
            }
        }

        private void encodedDataListBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (encodedDataListBox.SelectedIndex != -1)
            {
                if (e.KeyCode == Keys.Delete)
                {
                    encodedDataListBox.Items.RemoveAt(plainDataListBox.SelectedIndex);
                }
                else if (e.KeyCode == Keys.Enter)
                {

                }
            }
        }

        private void plainDataListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool enabled = plainDataListBox.SelectedItems.Count > 0;
            signButton.Enabled = enabled;
            encryptButton.Enabled = enabled;
        }

        private void encodedDataListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool enabled = encodedDataListBox.SelectedItems.Count > 0;
            verifyDecryptButton.Enabled = enabled;
        }

        #endregion // Обработчики событий для списков (ListBox)

        #region Обработчики событий для кнопок

        private void signButton_Click(object sender, EventArgs e)
        {
            try
            {
                //foreach( var obj in plainDataListBox.Items )
                //{

                //}

                //Encrypt("d:\\temp\\1.txt", "d:\\temp\\1.enc", _options.SenderCertificate);
                //Decrypt("d:\\temp\\1.enc", "d:\\temp\\1dec.txt", _options.SenderCertificate);

                //Encrypt("d:\\temp\\4.zip", "d:\\temp\\4.zip.enc", _options.SenderCertificate);
                //Decrypt("d:\\temp\\4.zip.enc", "d:\\temp\\4dec.zip", _options.SenderCertificate);

                string tfn = "1.txt";
                //string dtfn = "1dec.txt";

                using (FileStream input = File.Open("d:\\temp\\" + tfn, FileMode.Open))
                {
                    //{
                    //    //Stream output = Coder.Encrypt(input, _options.SenderCertificate.Value);
                    //    byte[] sign = Signer.Sign(input, _options.SenderCertificate.Value);
                    //    using (FileStream outputFileStream = new FileStream("d:\\temp\\" + tfn + ".p7s", FileMode.Create))
                    //    {
                    //        outputFileStream.Write(sign, 0, sign.Length);
                    //    }
                    //}

                    byte[] buffer = File.ReadAllBytes("d:\\temp\\" + tfn + ".p7s");
                    input.Position = 0;
                    Signer.Verify(buffer, input, _options.SenderCertificate.Value);
                }

                //using (FileStream input = File.Open("d:\\temp\\" + tfn + ".enc", FileMode.Open))
                //{
                //    Stream output = Coder.Decrypt(input, _options.SenderCertificate.Value);

                //    using (FileStream outputFileStream = new FileStream("d:\\temp\\" + dtfn, FileMode.Create))
                //    {
                //        output.CopyTo(outputFileStream);
                //        outputFileStream.Flush(true);
                //    }
                //}
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Ошибка при шифровании и подписи файлов.");
                Utils.MessageHelper.Error(this, ex);
            }
        }

        #endregion  // Обработчики событий для кнопок

        #region Служебные методы

        private void OpenFormOptions()
        {
            try
            {
                OptionsForm oform = new OptionsForm(_options);
                if (oform.ShowDialog() == DialogResult.OK)
                {
                    _options.AlwaysUseAttachedSign = oform.AlwaysUseAttachedSign;
                    _options.AlwaysUseGost = oform.AlwaysUseGost;
                    _options.SenderCertificate = oform.SenderCertificate;
                    _options.RecipientsCertificatesCollection = oform.RecipientsCertificatesCollection;

                    _options.Save();
                }

            }
            catch (Exception ex)
            {
                Utils.MessageHelper.Error(this, ex, "Ошибка при сохранении настроек приложения.");
            }
        }

        private void Encrypt(string inputpath, string outputpath, CertificateWrapper recipient)
        {
            FileStream input = File.Open(inputpath, FileMode.Open);

            Stream output = Coder.Encrypt(input, recipient.Value);
            using (FileStream outputFileStream = new FileStream(outputpath, FileMode.OpenOrCreate))
            {
                output.CopyTo(outputFileStream);
            }
        }

        private void Decrypt(string inputpath, string outputpath, CertificateWrapper recipient)
        {
            FileStream input = File.Open(inputpath, FileMode.Open);

            Stream output = Coder.Decrypt(input, recipient.Value);

            using (FileStream outputFileStream = new FileStream(outputpath, FileMode.OpenOrCreate))
            {
                output.CopyTo(outputFileStream);
            }
        }

        #endregion  // Служебные методы
    }
}
