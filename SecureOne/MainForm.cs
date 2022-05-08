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
using System.Security;
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

        private void MainForm_Shown(object sender, EventArgs e)
        {
            if (!_options.CheckRequiredFieldsAreFilled())
            {
                Utils.MessageHelper.Warning(this, "Чать обязательных настроек отсутствует. Проведите найстройку системы.");
                OpenFormOptions();
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
                PackageWrapper opw = Sign(ifw, _options.OwnerCertificate, _options.AllwaysUseDetachedSign);
                AddPackages(opw);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Ошибка при формировании подписи файла.");
                Utils.MessageHelper.Error(this, ex);
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

                FileWrapper fw = plainDataListBox.Items[plainDataListBox.SelectedIndex] as FileWrapper;
                PackageWrapper[] opwarr = Encrypt(fw, recipientCert, _options.OwnerCertificate, 
                    _options.AllwaysUseCustomEncFrmt);
                AddPackages(opwarr);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Ошибка при шифровании / подписи файла.");
                Utils.MessageHelper.Error(this, ex);
            }
        }

        private void verifyDecryptButton_Click(object sender, EventArgs e)
        {
            try
            {
                PackageWrapper pw = encodedDataListBox.Items[encodedDataListBox.SelectedIndex] as PackageWrapper;
                System.Diagnostics.Debug.Assert(pw.Type != PackageWrapper.PackageType.Unknown);

                // получаем имя файла для расшифровки и проверки данных
                string ofn = pw.GetNativeFilePath();

                if (pw.Type == PackageWrapper.PackageType.ENC ||
                    pw.Type == PackageWrapper.PackageType.P7M ||
                    pw.Type == PackageWrapper.PackageType.P7SM)
                {
                    // проверяем подпись (если есть), расшифровываем и сохраняем расшифрованные данные
                    Decrypt(pw, ofn, _options.OwnerCertificate);

                    if (Utils.MessageHelper.QuestionYN(this, "Файл успешно проверен и расшифрован. Открыть расшифрованный файл?") == DialogResult.Yes)
                        System.Diagnostics.Process.Start(ofn);
                }
                else if (pw.Type == PackageWrapper.PackageType.SIG)
                {
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

                    if (Verify(pw, openFileDialog.FileName))
                        Utils.MessageHelper.Info(this, "Подпись верна!");
                    else
                        Utils.MessageHelper.Warning(this, "Подпись не верна!");
                }
                else
                {
                    System.Diagnostics.Debug.Assert(pw.Type == PackageWrapper.PackageType.P7S);

                    if (Verify(pw, String.Empty))
                        Utils.MessageHelper.Info(this, "Подпись верна!");
                    else
                        Utils.MessageHelper.Warning(this, "Подпись не верна!");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Ошибка при проверке подписи файла");
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
                    _options.OwnerWorkingFolder = oform.OwnerWorkingFolder;
                    _options.AllwaysUseDetachedSign = oform.AllwaysUseDetachedSign;
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

        /// <summary>
        /// Шифрует файл на случайном симметричном ключе, зашированном в свою очередь на ассиметричном ключе сертификата. 
        /// Если указан сертификат подписанта, то формирует электронную подпись
        /// </summary>
        /// <remarks>
        /// Зашифрованный файл добавляет соответствующее расширение к полному имени исходного файла:
        /// file.ext.p7sm - шифрованные и подписанные данные в файл CMS / PKCS#7
        /// file.ext.p7m - шифрованные данные в файл CMS / PKCS#7
        /// file.ext.enc - шифрованные данные в формате SecureOne
        /// file.ext.sig - отсоединенная подпись в формате CMS / PKCS#7
        /// </remarks>
        /// <param name="ifw">Ссылка на обертку шифруемого файла</param>
        /// <param name="recipientCert">Ссылка на обертку для сертификата получателя</param>
        /// <param name="signerCert">Ссылка на обертку для сертификата подписывающего. Если null, подпись не формируется</param>
        /// <param name="cfrmt">Если true, функция всегда ширует файл в формате SecureOne. 
        /// Если false, функция пытается создать шифрованный CMS / PKCS7 контейнер, если памяти недостаточно шифрует в формате SecureOne.</param>
        [SecuritySafeCritical]
        protected PackageWrapper[] Encrypt(FileWrapper ifw, CertificateWrapper recipientCert, CertificateWrapper signerCert, bool cfrmt = false)
        {
            logger.Info($"Начинаем шифрование и подпись файла: {ifw.FilePathString}");

            List<PackageWrapper> opwlst = new List<PackageWrapper>();

            using (FileStream ifs = File.OpenRead(ifw.FilePathString))
            {
                try
                {
                    // Если мы должны использовать собственный формат или размер данных потока больше или равно 2 ^ 32
                    if (cfrmt || ifs.Length > Int32.MaxValue)
                        throw new OutOfMemoryException();   // генерируем исключение

                    // Пытаемся аллоцировать буффер нужного размера
                    byte[] buffer = new byte[ifs.Length];
                    
                    // Читаем данные в буффер
                    ifs.Read(buffer, 0, (int)ifs.Length);

                    // ссылка на шифрованный массив байтов
                    byte[] carr = null;

                    if (signerCert != null)
                    {
                        logger.Info("Указан сертификат владельца - шифруем и подписываем");

                        carr = Coder.SignEncrypt(buffer, recipientCert.Value, signerCert.Value);

                        logger.Info($"Сохраняем шифрованные данные в файл: {ifw.FilePathString + ".p7sm"}");

                        // Сохраняем шифрованные и подписанные данные в файл CMS / PKCS#7
                        using (FileStream ofs = new FileStream(ifw.FilePathString + ".p7sm", FileMode.CreateNew))
                        {
                            ofs.Write(carr, 0, carr.Length);
                        }

                        opwlst.Add(new PackageWrapper(ifw.FilePathString + ".p7sm"));
                    }
                    else
                    {
                        logger.Info("Сертификат владельца не указан - не подписываем. Только шифруем");

                        carr = Coder.Encrypt(buffer, recipientCert.Value);

                        logger.Info($"Сохраняем шифрованные данные в файл: {ifw.FilePathString + ".p7m"}");
                        // Сохраняем шифрованные данные в файл CMS / PKCS#7
                        using (FileStream ofs = new FileStream(ifw.FilePathString + ".p7m", FileMode.CreateNew))
                        {
                            ofs.Write(carr, 0, carr.Length);
                        }

                        opwlst.Add(new PackageWrapper(ifw.FilePathString + ".p7m"));
                    }
                }
                catch (OutOfMemoryException)
                {
                    if (cfrmt)
                        logger.Info($"Шифруем в собственном формате");
                    else
                        logger.Info($"Файл: {ifw.FilePathString} слишком большой: {ifs.Length}. Шифруем в собственном формате");

                    // Если пямяти нет шифруем в своем формате
                    Stream strm = Coder.Encrypt(ifs, recipientCert.Value);

                    logger.Info($"Сохраняем шифрованные данные в файл: {ifw.FilePathString + ".enc"}");

                    // Сохраняем шифрованные данные в файл
                    using (FileStream ofs = new FileStream(ifw.FilePathString + ".enc", FileMode.CreateNew))
                    {
                        strm.CopyTo(ofs);
                    }

                    opwlst.Add(new PackageWrapper(ifw.FilePathString + ".enc"));

                    if (signerCert != null)
                    {
                        logger.Info("Указан сертификат владельца - формируем отсоединенную подпись");

                        // Восстанавливаем позицию потока в начало
                        ifs.Position = 0;

                        // Формируем отсоединенную подпись
                        byte[] sign = Coder.SignDetached(ifs, signerCert.Value);

                        logger.Info($"Сохраняем данные подписи в файл: {ifw.FilePathString + ".sig"}");

                        // Сохраняем данные подписи в файл CMS / PKCS#7
                        using (FileStream ofs = new FileStream(ifw.FilePathString + ".sig", FileMode.CreateNew))
                        {
                            ofs.Write(sign, 0, sign.Length);
                        }

                        opwlst.Add(new PackageWrapper(ifw.FilePathString + ".sig"));
                    }
                }
            }

            logger.Info($"Шифрование и подпись файла успешно завершены. Файл: {ifw.FilePathString}");
            return opwlst.ToArray();
        }

        /// <summary>
        /// Расшифровывает файл на сертификате получателя. В случае если контейнер содержит подпись - проверяет ее.
        /// </summary>
        /// <remarks>
        /// <para>
        /// 1. Процедура работает с файлами .p7sm, .p7m, .enc.
        /// </para>
        /// <para>
        /// 2. Файл указанный в параметре ofpath не должен существовать. В противном случае возникнет исключение IOException.
        /// </para>
        /// </remarks>
        /// <param name="pw">Ссылка на файл контейнера</param>
        /// <param name="ofpath">Путь к файлу с расшифрованными данными</param>
        /// <param name="recipientCert">Ссылка на обертку для сертификата получателя</param>
        [SecuritySafeCritical]
        protected void Decrypt(PackageWrapper pw, string ofpath, CertificateWrapper recipientCert)
        {
            logger.Info($"Начинаем проверку подписи файла и его расшифровку: {pw.FilePathString}");

            logger.Info($"Создаем новый файл для расшифрованных данных: {ofpath}");
            using (FileStream ofs = new FileStream(ofpath, FileMode.CreateNew))
            {
                if (pw.Type == PackageWrapper.PackageType.ENC)
                {
                    // Если мы тут, значит это шифрованный файл собственного формата
                    using (FileStream ifs = File.OpenRead(pw.FilePathString))
                    {
                        logger.Info($"Рассшифровываем данные файла: {pw.FilePathString}");
                        // Расшифровываем его
                        Stream strm = Coder.Decrypt(ifs, recipientCert.Value);

                        logger.Info($"Сохраняем расшифрованные данные в файл: {ofpath}");
                        // Сохраняем расшифрованные данные в файл
                        strm.CopyTo(ofs);
                    }
                }
                else
                {
                    logger.Info($"Читаем в массив данные файла: {pw.FilePathString}");

                    // Если мы тут, значит это p7m или p7sm файл
                    byte[] encdata = File.ReadAllBytes(pw.FilePathString);

                    logger.Info($"Рассшифровываем данные файла: {pw.FilePathString}");

                    // Расшифровываем
                    byte[] decrypted = Coder.Decrypt(encdata);

                    logger.Info($"Сохраняем расшифрованные данные в файл: {ofpath}");

                    // Сохраняем расшифрованные данные в файл
                    ofs.Write(decrypted, 0, decrypted.Length);
                }
            }

            logger.Info($"Проверка подписи файла и его расшифровка успешна завершены: {pw.FilePathString}");
        }

        /// <summary>
        /// Формирует присоединенную электронную подпись
        /// </summary>
        /// <remarks>
        /// <para>
        /// 1. Процедура создает присоединенную .p7s или отсоединенную .sig подписи.
        /// </para>
        /// <para>
        /// 2. Если параметр <paramref name="forсeDetached"/> = true - всегда создает отсоединенную подпись. Если false, пытается создать присоединенную, если не получается, тогда отсоединенную.
        /// <para>
        /// </remarks>
        /// <param name="fw">Ссылка на обертку подписываемого файла</param>
        /// <param name="signerCert">Ссылка на обертку сертификата подписанта</param>
        /// <param name="forсeDetached">Если true - всегда создает отсоединенную подпись.</param>
        [SecuritySafeCritical]
        protected PackageWrapper Sign(FileWrapper ifw, CertificateWrapper signerCert, bool forсeDetached = false)
        {
            logger.Info($"Начинаем подпись файла: {ifw.FilePathString}");

            PackageWrapper opw = null;
            using (FileStream ifs = File.OpenRead(ifw.FilePathString))
            {
                string ext = ".p7s";
                byte[] sign = null;

                try
                {
                    // Если мы должны сформировать присоединенную подпись или размер данных потока больше или равно 2 ^ 32
                    if (forсeDetached || ifs.Length > Int32.MaxValue)
                        throw new OutOfMemoryException();   // генерируем исключение

                    // Пытаемся аллоцировать буфер нужной длины
                    byte[] buffer = new byte[ifs.Length];
                    // Читаем данные в буффер
                    ifs.Read(buffer, 0, (int)ifs.Length);

                    logger.Info($"Формируем присоединенную подпись");

                    sign = Coder.SignAttached(buffer, signerCert.Value);
                }
                catch(OutOfMemoryException)
                {
                    logger.Info($"Формируем отсоединенную подпись");

                    // Если мы тут формируем отсоединенную подпись
                    sign = Coder.SignDetached(ifs, signerCert.Value);
                    ext = ".sig";
                }

                logger.Info($"Сохраняем данные подписи в файл: {ifw.FilePathString + ext}");

                // Сохраняем данные подписи в файл CMS / PKCS#7
                using (FileStream ofs = new FileStream(ifw.FilePathString + ext, FileMode.CreateNew))
                {
                    ofs.Write(sign, 0, sign.Length);
                }

                opw = new PackageWrapper(ifw.FilePathString + ext);
            }

            logger.Info($"Подпись файла: {ifw.FilePathString} успешно завершена");
            return opw;
        }

        /// <summary>
        /// Проверяет верность электронной присоединенной или отсоединенной подписи в формате CMS / PKCS#7
        /// </summary>
        /// <param name="pw">Ссылка на файл контейнера</param>
        /// <param name="datafname">Имя файла подписанных данных для случая отсоединенной подписи. Или пустая строка.</param>
        /// <returns>True - подпись верна. False - подпись не прошла проверку, в том числе по причине не криптографических ошибок.</returns>
        [SecuritySafeCritical]
        protected bool Verify(PackageWrapper pw, string datafname)
        {
            logger.Info($"Начинаем проверку подписи: {pw.FilePathString}");

            byte[] sign = File.ReadAllBytes(pw.FilePathString);
            try
            {
                if (datafname.Length > 0)
                {
                    using (FileStream ifs = File.OpenRead(datafname))
                    {
                        logger.Info($"Начинаем проверку отсоединенной подписи");
                        Coder.Verify(sign, ifs);
                    }
                }
                else
                {
                    logger.Info($"Начинаем проверку присоединенной подписи");
                    Coder.Verify(sign);
                }

                logger.Info($"Подпись {pw.FilePathString} успешно проверена");
                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"При проверке файла подписи: {pw.FilePathString} возникло исключение");
            }

            return false;
        }

        #endregion  // Служебные методы
    }
}
