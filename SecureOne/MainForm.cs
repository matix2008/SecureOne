using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using SecureOneLib;

namespace SecureOne
{
    public partial class MainForm : Form
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private BackgroundCryptoWorker _backgroundCryptoWorker = null;
        private Settings _options = null;

        public MainForm()
        {
            InitializeComponent();

            _backgroundCryptoWorker = new BackgroundCryptoWorker(this);
            _backgroundCryptoWorker.ProgressChanged += backgroundWorker_ProgressChanged;
            _backgroundCryptoWorker.RunWorkerCompleted += backgroundWorker_RunWorkerCompleted;
        }

        #region Обработчики событий формы и меню

        private void MainForm_Load(object sender, EventArgs e)
        {
            signButton.Enabled = false;
            encryptButton.Enabled = false;
            verifyDecryptButton.Enabled = false;
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            try
            {
                _options = new Settings();

                //if (!_options.CheckRequiredFieldsAreFilled())
                //{
                //    Utils.MessageHelper.Warning(this, "Чаcть обязательных настроек отсутствует. Проведите найстройку системы.");
                //    OpenFormOptions();
                //}
                //else
                {
                    StopReporting();
                    _backgroundCryptoWorker.StartCheckSettings(_options);
                }
            }
            catch (Exception ex)
            {
                Utils.MessageHelper.Error(this, ex, "Ошибка при загрузке настроек приложения.");
            }
            finally
            {
                StopReporting();
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_backgroundCryptoWorker.IsBusy)
            {
                _backgroundCryptoWorker.CancelAsync();
                e.Cancel = true;
            }
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
            openFileDialog.InitialDirectory = _options.OwnerWorkingFolder;
            openFileDialog.Filter = "Encypted files (*.p7s;*.enс;*.p7m;*.p7s.p7m;*.sig)|*.p7s;*.enс;*.p7m;*.p7s.p7m;*.sig|All files (*.*)|*.*";
            openFileDialog.CheckFileExists = true;
            openFileDialog.FilterIndex = 2;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.Multiselect = true;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                ClearPlainDataListBox();
                ClearEncodedDataListBox();

                Regex rx = new Regex("p7s|p7m|enc|.p7s.p7m|sig",RegexOptions.IgnoreCase);

                foreach (var fn in openFileDialog.FileNames)
                {
                    string ext = Path.GetExtension(fn);

                    try
                    {
                        if (rx.IsMatch(ext))
                            encodedDataListBox.Items.Add(new PackageWrapper(fn));
                        else
                            plainDataListBox.Items.Add(new FileWrapper(fn));
                    }
                    catch(Exception ex)
                    {
                        logger.Error(ex, $"Ошибка при загрузке файла: {fn}");
                        Utils.MessageHelper.Error(this, ex);
                    }
                }

                if (plainDataListBox.Items.Count > 0)
                    plainDataListBox.SelectedIndex = 0;
                else if (encodedDataListBox.Items.Count > 0)
                    encodedDataListBox.SelectedIndex = 0;
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox about = new AboutBox();
            about.ShowDialog();
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
                    encodedDataListBox.Items.RemoveAt(encodedDataListBox.SelectedIndex);
                }
            }
        }

        private void plainDataListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool enabled = plainDataListBox.SelectedItems.Count > 0;
            signButton.Enabled = enabled;
            encryptButton.Enabled = enabled;

            if (plainDataListBox.SelectedItems.Count == 1)
                SetFileInfo(plainDataListBox.Items[plainDataListBox.SelectedIndex] as FileWrapper);
            else
                SetFileInfo(null);
        }

        private void encodedDataListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool enabled = encodedDataListBox.SelectedItems.Count > 0;
            verifyDecryptButton.Enabled = enabled;

            if (encodedDataListBox.SelectedItems.Count == 1)
                SetFileInfo(encodedDataListBox.Items[encodedDataListBox.SelectedIndex] as FileWrapper);
            else
                SetFileInfo(null);
        }

        #endregion // Обработчики событий для списков (ListBox)

        #region Обработчики событий для кнопок

        private void signButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (_options.OwnerCertificate == null)
                {
                    Utils.MessageHelper.Warning(this, "Сертификат владельца подписи не указан. Создание подписи невозможно. Произведите настройку системы!");
                    return;
                }

                FileWrapper ifw = plainDataListBox.Items[plainDataListBox.SelectedIndex] as FileWrapper;

                StartReporting();

                // Начинаем асинхронную операцию
                _backgroundCryptoWorker.StartSign(ifw, _options.OwnerCertificate, 
                    _options.AllwaysUseDetachedSign);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Ошибка при формировании подписи файла.");
                Utils.MessageHelper.Error(this, ex);
                StopReporting();
            }
        }

        private void encryptButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Для шифрования нужно запросить у пользователя сертификат получателя
                // Если сертификаты контрагентов определены в настройках, то выбираем из них. 
                // Если нет, то из всех (действительных) сертификатов хранилища.

                // Для подписи нужен сертификат владельца (отправителя)

                CertificateWrapper recipientCert = null;
                if (_options.RecipientsCertificatesCollection != null && _options.RecipientsCertificatesCollection.Count == 1)
                {
                    // Если в настройках указан только один сертификат контрагента, то используем его.
                    recipientCert = new CertificateWrapper(_options.RecipientsCertificatesCollection.Value[0]);
                }
                else
                {
                    ChooseCertForm ccf = new ChooseCertForm(_options.RecipientsCertificatesCollection, false, false);
                    if (ccf.ShowDialog(this) == DialogResult.OK)
                    {
                        recipientCert = ccf.SelectedCertificates[0];
                    }
                }

                if (recipientCert == null)
                {
                    Utils.MessageHelper.Warning(this, "Сертификат получателя не выбран. Ширование невозможно.");
                    return;
                }

                if (_options.OwnerCertificate == null)
                {
                    if (Utils.MessageHelper.QuestionYN(this,
                        "Сертификат владельца подписи не указан. Файл будет зашифрован, но не будет подписан. Продолжить?") == DialogResult.No)
                        return;
                }

                FileWrapper ifw = plainDataListBox.Items[plainDataListBox.SelectedIndex] as FileWrapper;

                StartReporting();

                // Начинаем асинхронную операцию
                _backgroundCryptoWorker.StartSignEncrypt(ifw, _options.OwnerCertificate, recipientCert, 
                    _options.AllwaysUseCustomEncFrmt);

            }
            catch (Exception ex)
            {
                logger.Error(ex, "Ошибка при шифровании / подписи файла.");
                Utils.MessageHelper.Error(this, ex);
                StopReporting();
            }
        }

        private void verifyDecryptButton_Click(object sender, EventArgs e)
        {
            try
            {
                PackageWrapper ipw = encodedDataListBox.Items[encodedDataListBox.SelectedIndex] as PackageWrapper;
                System.Diagnostics.Debug.Assert(ipw.Type != PackageWrapper.PackageType.Unknown);

                if (ipw.Type == PackageWrapper.PackageType.ENC ||
                    ipw.Type == PackageWrapper.PackageType.P7M ||
                    ipw.Type == PackageWrapper.PackageType.P7SM)
                {
                    if (_options.OwnerCertificate == null)
                    {
                        Utils.MessageHelper.Warning(this, "Сертификат собственника не указан. Расшифровка невозможна.");
                        return;
                    }

                    StartReporting();
                    _backgroundCryptoWorker.StartDecrypt(ipw, _options.OwnerCertificate);
                }
                else if (ipw.Type == PackageWrapper.PackageType.P7S)
                {
                    StartReporting();
                    _backgroundCryptoWorker.StartVerifyEncode(ipw);
                }
                else 
                {
                    System.Diagnostics.Debug.Assert(ipw.Type == PackageWrapper.PackageType.SIG);

                    // Если мы тут, значит это отсоединенная подпись
                    // Запрашиваем файл данных
                    openFileDialog.InitialDirectory = _options.OwnerWorkingFolder;
                    openFileDialog.Filter = "All files (*.*)|*.*";
                    openFileDialog.CheckFileExists = true;
                    openFileDialog.FilterIndex = 1;
                    openFileDialog.RestoreDirectory = true;
                    openFileDialog.Multiselect = false;
                    openFileDialog.FileName = "";
                    openFileDialog.Title = "Загрузите файл данных для которого была сформирована подпись.";

                    if (openFileDialog.ShowDialog() != DialogResult.OK)
                    {
                        Utils.MessageHelper.Warning(this, "Файл данных не загружен. Проверка подписи невозможна.");
                        return;
                    }

                    StartReporting();
                    _backgroundCryptoWorker.StartVerify(ipw, openFileDialog.FileName);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Ошибка при проверке подписи файла");
                Utils.MessageHelper.Error(this, ex);
                StopReporting();
            }
        }

        #endregion  // Обработчики событий для кнопок

        #region Асинхронные операции

        /// <summary>
        /// Изменение прогресса асинхронной операции
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            BackgroundCryptoWorker worker = sender as BackgroundCryptoWorker;
            SOState state = e.UserState as SOState;

            toolStripStatusLabel.Text = state.Message;
            toolStripProgressBar.Value = e.ProgressPercentage;
        }
        /// <summary>
        /// Завершение асинхронной операции
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            StopReporting();

            if (e.Cancelled == true)
            {
                Utils.MessageHelper.Warning(this, "Операция отменена пользователем");
            }
            else if (e.Error != null)
            {
                Utils.MessageHelper.Error(this, $"Произошла фатальная ошибка: {e.Error.Message}");

                logger.Info(e.Error);
            }
            else
            {
                BackgroundCryptoWorker worker = sender as BackgroundCryptoWorker;

                switch (worker.Operation)
                {
                    default:
                    case BackgroundCryptoWorker.AsyncCryptoOpration.Unknown: break;

                    case BackgroundCryptoWorker.AsyncCryptoOpration.Sign:
                        AddPackages(e.Result as PackageWrapper);
                        Utils.MessageHelper.Info(this, "Операция успешна завершена");
                        break;

                    case BackgroundCryptoWorker.AsyncCryptoOpration.SignEncrypt:
                        AddPackages(e.Result as PackageWrapper[]);
                        Utils.MessageHelper.Info(this, "Операция успешна завершена");
                        break;

                    case BackgroundCryptoWorker.AsyncCryptoOpration.Decrypt:
                        if (Utils.MessageHelper.QuestionYN(this, "Файл успешно проверен и расшифрован. Открыть расшифрованный файл?") == DialogResult.Yes)
                            System.Diagnostics.Process.Start(e.Result as string);
                        break;

                    case BackgroundCryptoWorker.AsyncCryptoOpration.VerifyEncode:
                        {
                            string filename = e.Result as string;
                            if (filename.Length == 0 )
                                Utils.MessageHelper.Warning(this, "Подпись не верна!");
                            else
                            if (Utils.MessageHelper.QuestionYN(this, "Файл успешно проверен. Открыть файл?") == DialogResult.Yes)
                                System.Diagnostics.Process.Start(e.Result as string);
                        }
                        break;

                    case BackgroundCryptoWorker.AsyncCryptoOpration.Verify:
                        if (((bool)e.Result))
                            Utils.MessageHelper.Info(this, "Подпись верна!");
                        else
                            Utils.MessageHelper.Warning(this, "Подпись не верна!");
                        break;

                    case BackgroundCryptoWorker.AsyncCryptoOpration.CheckSettigs:
                        if (!_options.CheckRequiredFieldsAreFilled())
                        {
                            Utils.MessageHelper.Warning(this, "Чаcть обязательных настроек отсутствует. Проведите найстройку системы.");
                            OpenFormOptions();
                        }
                        break;
                }
            }
        }

        #endregion

        #region Служебные методы

        private void OpenFormOptions()
        {
            try
            {
                OptionsForm oform = new OptionsForm(_options);
                if (oform.ShowDialog() == DialogResult.OK)
                {
                    _options.OwnerWorkingFolder = oform.OwnerWorkingFolder;
                    _options.AllwaysUseDetachedSign = oform.AllwaysUseDetachedSign;
                    _options.AllwaysUseCustomEncFrmt = oform.AllwaysUseCustomEncFrmt;
                    _options.OwnerCertificate = oform.OwnerCertificate;
                    _options.RecipientsCertificatesCollection = oform.RecipientsCertificatesCollection;

                    _options.Save();
                }

            }
            catch (Exception ex)
            {
                Utils.MessageHelper.Error(this, ex, "Ошибка при сохранении настроек приложения.");
            }
        }

        private void SetFileInfo(FileWrapper fw)
        {
            fileInfolistBox.Items.Clear();

            if (fw != null)
            {
                foreach (var r in fw.GetRequisites())
                {
                    fileInfolistBox.Items.Add(r);
                }
            }
        }

        private void ClearPlainDataListBox()
        {
            plainDataListBox.Items.Clear();
            signButton.Enabled = false;
            encryptButton.Enabled = false;
        }

        private void ClearEncodedDataListBox()
        {
            encodedDataListBox.Items.Clear();
            verifyDecryptButton.Enabled = false;
        }

        private void AddPackages(PackageWrapper pw)
        {
            encodedDataListBox.Items.Add(pw);
            verifyDecryptButton.Enabled = true;
        }

        private void AddPackages(PackageWrapper[] pwarr)
        {
            encodedDataListBox.Items.AddRange(pwarr);
            verifyDecryptButton.Enabled = true;
        }

        protected void StartReporting()
        {
            Cursor.Current = Cursors.WaitCursor;
            toolStripProgressBar.Visible = true;
        }

        protected void StopReporting()
        {
            toolStripProgressBar.Visible = false;
            toolStripStatusLabel.Text = "Готов";
            Cursor.Current = Cursors.Default;
        }

        #endregion
    }
}
