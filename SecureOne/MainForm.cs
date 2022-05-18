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

                // получаем имя файла для расшифровки и проверки данных
                string ofn = ipw.GetNativeFilePath();

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
                    _backgroundCryptoWorker.StartDecrypt(ipw, ofn, _options.OwnerCertificate);
                }
                else if (ipw.Type == PackageWrapper.PackageType.P7S)
                {
                    StartReporting();
                    _backgroundCryptoWorker.StartVerifyEncode(ipw, ofn);
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
                    openFileDialog.FileName = ofn;
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
                Utils.MessageHelper.Error(this, $"Произошла фатальная ошибка: {e.Error}");
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

        ///// <summary>
        ///// Шифрует файл на случайном симметричном ключе, зашированном в свою очередь на ассиметричном ключе сертификата. 
        ///// Если указан сертификат подписанта, то формирует электронную подпись
        ///// </summary>
        ///// <remarks>
        ///// Зашифрованный файл добавляет соответствующее расширение к полному имени исходного файла:
        ///// file.ext.p7sm - шифрованные и подписанные данные в файл CMS / PKCS#7
        ///// file.ext.p7m - шифрованные данные в файл CMS / PKCS#7
        ///// file.ext.enc - шифрованные данные в формате SecureOne
        ///// file.ext.sig - отсоединенная подпись в формате CMS / PKCS#7
        ///// </remarks>
        ///// <param name="ifw">Ссылка на обертку шифруемого файла</param>
        ///// <param name="recipientCert">Ссылка на обертку для сертификата получателя</param>
        ///// <param name="signerCert">Ссылка на обертку для сертификата подписывающего. Если null, подпись не формируется</param>
        ///// <param name="cfrmt">Если true, функция всегда ширует файл в формате SecureOne. 
        ///// Если false, функция пытается создать шифрованный CMS / PKCS7 контейнер, если памяти недостаточно шифрует в формате SecureOne.</param>
        //[SecuritySafeCritical]
        //protected PackageWrapper[] Encrypt(FileWrapper ifw, CertificateWrapper recipientCert, CertificateWrapper signerCert, bool cfrmt = false)
        //{
        //    Reporter($"Начинаем шифрование и подпись файла: {ifw.FilePathString}");

        //    List<PackageWrapper> opwlst = new List<PackageWrapper>();

        //    using (FileStream ifs = File.OpenRead(ifw.FilePathString))
        //    {
        //        try
        //        {
        //            // очищаем память
        //            GC.Collect();

        //            // Если мы должны использовать собственный формат или размер данных потока больше или равно 2 ^ 32
        //            if (cfrmt || ifs.Length > Int32.MaxValue)
        //                throw new OutOfMemoryException();   // генерируем исключение

        //            // Пытаемся аллоцировать буффер нужного размера
        //            byte[] buffer = new byte[ifs.Length];
                    
        //            // Читаем данные в буффер
        //            ifs.Read(buffer, 0, (int)ifs.Length);

        //            // ссылка на шифрованный массив байтов
        //            byte[] carr = null;

        //            if (signerCert != null)
        //            {
        //                Reporter("Указан сертификат владельца - шифруем и подписываем");

        //                carr = Coder.SignEncrypt(buffer, recipientCert.Value, signerCert.Value);

        //                Reporter($"Сохраняем шифрованные данные в файл: {ifw.FilePathString + ".p7sm"}");

        //                // Сохраняем шифрованные и подписанные данные в файл CMS / PKCS#7
        //                using (FileStream ofs = new FileStream(ifw.FilePathString + ".p7sm", FileMode.CreateNew))
        //                {
        //                    ofs.Write(carr, 0, carr.Length);
        //                }

        //                opwlst.Add(new PackageWrapper(ifw.FilePathString + ".p7sm"));
        //            }
        //            else
        //            {
        //                Reporter("Сертификат владельца не указан - не подписываем. Только шифруем");

        //                // Шифруем и подписываем
        //                carr = Coder.Encrypt(buffer, recipientCert.Value);

        //                Reporter($"Сохраняем шифрованные данные в файл: {ifw.FilePathString + ".p7m"}");
        //                // Сохраняем шифрованные данные в файл CMS / PKCS#7
        //                using (FileStream ofs = new FileStream(ifw.FilePathString + ".p7m", FileMode.CreateNew))
        //                {
        //                    ofs.Write(carr, 0, carr.Length);
        //                }

        //                opwlst.Add(new PackageWrapper(ifw.FilePathString + ".p7m"));
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            // очищаем память
        //            GC.Collect();

        //            if (ex is OutOfMemoryException ||
        //                (ex is System.Security.Cryptography.CryptographicException && ((uint)ex.HResult) == 0x80093106))
        //            {
        //                // Шифруем в своем формате

        //                if (cfrmt)
        //                    Reporter($"Шифруем в собственном формате");
        //                else
        //                    Reporter($"Файл: {ifw.FilePathString} слишком большой: {ifs.Length}. Шифруем в собственном формате");

        //                // Сохраняем шифрованные данные в файл
        //                using (FileStream ofs = new FileStream(ifw.FilePathString + ".enc", FileMode.CreateNew))
        //                {
        //                    Reporter($"Шифруем и сохраняем шифрованные данные в файл: {ifw.FilePathString + ".enc"}");

        //                    // Восстанавливаем позицию потока в начало
        //                    ifs.Position = 0;

        //                    // Шифруем и сохраняем данные в выходной поток
        //                    Coder.Encrypt(ifs, ofs, recipientCert.Value);
        //                }

        //                // Добавляем шифрованный файл в список
        //                opwlst.Add(new PackageWrapper(ifw.FilePathString + ".enc"));

        //                if (signerCert != null)
        //                {
        //                    Reporter("Указан сертификат владельца - формируем отсоединенную подпись");

        //                    // Восстанавливаем позицию потока в начало
        //                    ifs.Position = 0;

        //                    // Формируем отсоединенную подпись
        //                    byte[] sign = Coder.SignDetached(ifs, signerCert.Value);

        //                    Reporter($"Сохраняем данные подписи в файл: {ifw.FilePathString + ".sig"}");

        //                    // Сохраняем данные подписи в файл CMS / PKCS#7
        //                    using (FileStream ofs = new FileStream(ifw.FilePathString + ".sig", FileMode.CreateNew))
        //                    {
        //                        ofs.Write(sign, 0, sign.Length);
        //                    }

        //                    opwlst.Add(new PackageWrapper(ifw.FilePathString + ".sig"));
        //                }
        //            }
        //            else throw;
        //        }
        //    }

        //    Reporter($"Шифрование и подпись файла успешно завершены. Файл: {ifw.FilePathString}");
        //    return opwlst.ToArray();
        //}

        ///// <summary>
        ///// Расшифровывает файл на сертификате получателя. В случае если контейнер содержит подпись - проверяет ее.
        ///// </summary>
        ///// <remarks>
        ///// <para>
        ///// 1. Процедура работает с файлами .p7sm, .p7m, .enc.
        ///// </para>
        ///// <para>
        ///// 2. Файл указанный в параметре ofpath не должен существовать. В противном случае возникнет исключение IOException.
        ///// </para>
        ///// </remarks>
        ///// <param name="pw">Ссылка на файл контейнера</param>
        ///// <param name="ofpath">Путь к файлу с расшифрованными данными</param>
        ///// <param name="recipientCert">Ссылка на обертку для сертификата получателя</param>
        //[SecuritySafeCritical]
        //protected void Decrypt(PackageWrapper pw, string ofpath, CertificateWrapper recipientCert)
        //{
        //    // очищаем память
        //    GC.Collect();

        //    Reporter($"Начинаем проверку подписи файла и его расшифровку: {pw.FilePathString}");

        //    Reporter($"Создаем новый файл для расшифрованных данных: {ofpath}");
        //    using (FileStream ofs = new FileStream(ofpath, FileMode.CreateNew))
        //    {
        //        if (pw.Type == PackageWrapper.PackageType.ENC)
        //        {
        //            // Если мы тут, значит это шифрованный файл собственного формата
        //            using (FileStream ifs = File.OpenRead(pw.FilePathString))
        //            {
        //                Reporter($"Рассшифровываем и сохраняем данные в файл: {ofpath}");
        //                // Расшифровываем его и сохраняем расшифрованные данные в файл
        //                Coder.Decrypt(ifs, ofs, recipientCert.Value);
        //            }
        //        }
        //        else
        //        {
        //            Reporter($"Читаем в массив данные файла: {pw.FilePathString}");

        //            // Если мы тут, значит это p7m или p7sm файл
        //            byte[] encdata = File.ReadAllBytes(pw.FilePathString);

        //            Reporter($"Рассшифровываем данные файла: {pw.FilePathString}");

        //            byte[] decrypted = null;
        //            // Расшифровываем
        //            if (pw.Type == PackageWrapper.PackageType.P7M)
        //                decrypted = Coder.Decrypt(encdata);
        //            else
        //                decrypted = Coder.VerifyDecrypt(encdata);

        //            Reporter($"Сохраняем расшифрованные данные в файл: {ofpath}");

        //            // Сохраняем расшифрованные данные в файл
        //            ofs.Write(decrypted, 0, decrypted.Length);
        //        }
        //    }

        //    Reporter($"Проверка подписи файла и его расшифровка успешна завершены: {pw.FilePathString}");
        //}

        ///// <summary>
        ///// Формирует электронную подпись
        ///// </summary>
        ///// <remarks>
        ///// <para>
        ///// 1. Процедура создает присоединенную .p7s или отсоединенную .sig подписи.
        ///// </para>
        ///// <para>
        ///// 2. Если параметр <paramref name="forсeDetached"/> = true - всегда создает отсоединенную подпись. Если false, пытается создать присоединенную, если не получается, тогда отсоединенную.
        ///// <para>
        ///// </remarks>
        ///// <param name="fw">Ссылка на обертку подписываемого файла</param>
        ///// <param name="signerCert">Ссылка на обертку сертификата подписанта</param>
        ///// <param name="forсeDetached">Если true - всегда создает отсоединенную подпись.</param>
        //[SecuritySafeCritical]
        //protected PackageWrapper Sign(FileWrapper ifw, CertificateWrapper signerCert, bool forсeDetached = false)
        //{
        //    Reporter($"Начинаем подпись файла: {ifw.FilePathString}");

        //    PackageWrapper opw = null;
        //    using (FileStream ifs = File.OpenRead(ifw.FilePathString))
        //    {
        //        string ext = ".p7s";
        //        byte[] sign = null;

        //        try
        //        {
        //            // очищаем память
        //            GC.Collect();

        //            // Если мы должны сформировать присоединенную подпись или размер данных потока больше или равно 2 ^ 32
        //            if (forсeDetached || ifs.Length > Int32.MaxValue)
        //                throw new OutOfMemoryException();   // генерируем исключение

        //            // Пытаемся аллоцировать буфер нужной длины
        //            byte[] buffer = new byte[ifs.Length];
        //            // Читаем данные в буффер
        //            ifs.Read(buffer, 0, (int)ifs.Length);

        //            Reporter($"Формируем присоединенную подпись");

        //            sign = Coder.SignAttached(buffer, signerCert.Value);
        //        }
        //        catch(Exception ex)
        //        {
        //            // очищаем память
        //            GC.Collect();

        //            if (ex is OutOfMemoryException || 
        //                (ex is System.Security.Cryptography.CryptographicException && ((uint)ex.HResult) == 0x80093106))
        //            {
        //                Reporter($"Формируем отсоединенную подпись");

        //                // Если мы тут формируем отсоединенную подпись
        //                sign = Coder.SignDetached(ifs, signerCert.Value);
        //                ext = ".sig";
        //            }
        //            else throw;
        //        }

        //        Reporter($"Сохраняем данные подписи в файл: {ifw.FilePathString + ext}");

        //        // Сохраняем данные подписи в файл CMS / PKCS#7
        //        using (FileStream ofs = new FileStream(ifw.FilePathString + ext, FileMode.CreateNew))
        //        {
        //            ofs.Write(sign, 0, sign.Length);
        //        }

        //        opw = new PackageWrapper(ifw.FilePathString + ext);
        //    }

        //    Reporter($"Подпись файла: {ifw.FilePathString} успешно завершена");
        //    return opw;
        //}

        ///// <summary>
        ///// Проверяет верность электронной присоединенной или отсоединенной подписи в формате CMS / PKCS#7
        ///// </summary>
        ///// <param name="pw">Ссылка на файл контейнера</param>
        ///// <param name="datafname">Имя файла подписанных данных для случая отсоединенной подписи. Или пустая строка.</param>
        ///// <returns>True - подпись верна. False - подпись не прошла проверку, в том числе по причине не криптографических ошибок.</returns>
        //[SecuritySafeCritical]
        //protected bool Verify(PackageWrapper pw, string datafname)
        //{
        //    // очищаем память
        //    GC.Collect();

        //    Reporter($"Начинаем проверку подписи: {pw.FilePathString}");

        //    byte[] sign = File.ReadAllBytes(pw.FilePathString);
        //    try
        //    {
        //        if (datafname.Length > 0)
        //        {
        //            using (FileStream ifs = File.OpenRead(datafname))
        //            {
        //                Reporter($"Начинаем проверку отсоединенной подписи");
        //                Coder.Verify(sign, ifs);
        //            }
        //        }
        //        else
        //        {
        //            Reporter($"Начинаем проверку присоединенной подписи");
        //            Coder.Verify(sign);
        //        }

        //        Reporter($"Подпись {pw.FilePathString} успешно проверена");
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        Reporter(ex, $"При проверке файла подписи: {pw.FilePathString} возникло исключение");
        //    }

        //    return false;
        //}

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
