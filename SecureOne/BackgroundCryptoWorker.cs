using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.ComponentModel;
using SecureOneLib;
using System.Security.Cryptography;

namespace SecureOne
{
    /// <summary>
    /// Реализует комполнент для выполнения фоновых, асинхронных операций
    /// </summary>
    public class BackgroundCryptoWorker : BackgroundWorker
    {
        // типы поддерживаемых операций
        public enum AsyncCryptoOpration { Unknown, Sign, SignEncrypt, Verify, Decrypt, VerifyEncode, CheckSettigs };

        // ссылка на логер
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        protected Form _owner;                                  // форма владелец

        protected object _lockObj = new object();               // объект синхронизации

        protected AsyncCryptoOpration _asyncCryptoOpration;     // тип асинхронной операции
        protected FileWrapper _inputFileWrapper;                // входной файл
        protected string _fileName;                             // дополнительное имя файла
        protected CertificateWrapper _signerCertificate;        // сертификат владельца
        protected CertificateWrapper _recipientCertificate;     // сертификат контрагента
        protected bool _forсeDetachedSign;                      // флаг отсоединенной подписи
        protected bool _customEncryptionFormat;                 // флаг собственного формата
        protected Settings _settings;                           // ссылка на настройки

        /// <summary>
        /// Конструирует объект для асинхронных операций
        /// </summary>
        /// <param name="owner"></param>
        public BackgroundCryptoWorker(Form owner)
        {
            _owner = owner;
            _asyncCryptoOpration = AsyncCryptoOpration.Unknown;
            _inputFileWrapper = null;
            _fileName = String.Empty;
            _signerCertificate = null;
            _recipientCertificate = null;
            _forсeDetachedSign = false;
            _customEncryptionFormat = false;
            _settings = null;

            this.WorkerSupportsCancellation = true;
            this.WorkerReportsProgress = true;

            this.DoWork += BackgroundCryptoWorker_DoWork;
        }

        /// <summary>
        /// Возвращает тип операции
        /// </summary>
        public AsyncCryptoOpration Operation
        {
            get
            {
                lock (_lockObj)
                {
                    return _asyncCryptoOpration;
                }
            }
        }

        /// <summary>
        /// Начинает асихронную проверку настроек
        /// </summary>
        public bool StartCheckSettings(Settings settings)
        {
            if (CheckNotBusy())
            {
                lock (settings)
                {
                    _asyncCryptoOpration = AsyncCryptoOpration.CheckSettigs;
                    _settings = settings;
                }

                RunWorkerAsync();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Начинает асихронную операцию подписи
        /// </summary>
        public bool StartSign(FileWrapper ifw, CertificateWrapper signerCert, bool forсeDetached = false)
        {
            if (CheckNotBusy())
            {
                lock (_lockObj)
                {
                    _asyncCryptoOpration = AsyncCryptoOpration.Sign;
                    _inputFileWrapper = ifw;
                    _signerCertificate = signerCert;
                    _forсeDetachedSign = forсeDetached;
                    _fileName = String.Empty;
                }

                RunWorkerAsync();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Начинает асихронную операцию подписи и шифрования
        /// </summary>
        public bool StartSignEncrypt(FileWrapper ifw, CertificateWrapper signerCert, 
            CertificateWrapper recipientCert, bool cfrmt = false)
        {
            if (CheckNotBusy())
            {
                lock (_lockObj)
                {
                    _asyncCryptoOpration = AsyncCryptoOpration.SignEncrypt;
                    _inputFileWrapper = ifw;
                    _signerCertificate = signerCert;
                    _recipientCertificate = recipientCert;
                    _customEncryptionFormat = cfrmt;
                    _fileName = String.Empty;
                }

                RunWorkerAsync();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Начинает асихронную операцию расшифровки
        /// </summary>
        public bool StartDecrypt(PackageWrapper ipw, CertificateWrapper recipientCert)
        {
            if (CheckNotBusy())
            {
                lock (_lockObj)
                {
                    _asyncCryptoOpration = AsyncCryptoOpration.Decrypt;
                    _inputFileWrapper = ipw;
                    _signerCertificate = null;
                    _recipientCertificate = recipientCert;
                    _fileName = ipw.GetNativeFilePath();
                }

                RunWorkerAsync();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Начинает асихронную операцию проверки присоединенной подписи
        /// </summary>
        public bool StartVerifyEncode(PackageWrapper ipw)
        {
            if (CheckNotBusy())
            {
                lock (_lockObj)
                {
                    _asyncCryptoOpration = AsyncCryptoOpration.VerifyEncode;
                    _inputFileWrapper = ipw;
                    _signerCertificate = null;
                    _recipientCertificate = null;
                    _fileName = ipw.GetNativeFilePath();
                }

                RunWorkerAsync();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Начинает асихронную операцию проверки отсоединенной подписи
        /// </summary>
        public bool StartVerify(PackageWrapper ipw, string datafname)
        {
            if (CheckNotBusy())
            {
                lock (_lockObj)
                {
                    _asyncCryptoOpration = AsyncCryptoOpration.Verify;
                    _inputFileWrapper = ipw;
                    _signerCertificate = null;
                    _recipientCertificate = null;
                    _fileName = datafname;
                }

                RunWorkerAsync();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Проверяет не занят ли объект
        /// </summary>
        protected bool CheckNotBusy()
        {
            if (this.IsBusy)
            {
                MessageHelper.Warning(_owner, "Предыдущая операция еще не закончена. Дождитесь ее заверешения!");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Выполняет криптографические операции в фоновом режиме
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackgroundCryptoWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            // Освобождаем память
            GC.Collect();
            
            if (CancellationPending)
            {
                e.Cancel = true;
            }
            else
            {
                lock (_lockObj)
                {
                    try
                    {
                        if (_asyncCryptoOpration == AsyncCryptoOpration.Sign)
                            e.Result = Sign();
                        else if (_asyncCryptoOpration == AsyncCryptoOpration.SignEncrypt)
                            e.Result = Encrypt();
                        else if (_asyncCryptoOpration == AsyncCryptoOpration.Decrypt)
                            e.Result = Decrypt();
                        else if (_asyncCryptoOpration == AsyncCryptoOpration.Verify)
                            e.Result = Verify();
                        else if (_asyncCryptoOpration == AsyncCryptoOpration.VerifyEncode)
                            e.Result = VerifyEncode();
                        else if (_asyncCryptoOpration == AsyncCryptoOpration.CheckSettigs)
                            e.Result = CheckIncorrectSettings();
                        else
                            return;
                    }   
                    catch(Exception ex)
                    {
                        logger.Error(ex, ex.Message);
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Операция шифрования
        /// </summary>
        /// <returns>Список созданных артефактов</returns>
        protected PackageWrapper[] Encrypt()
        {
            Action<SOState.CryptoState, string> _report = (state, message)
                => Report(SOState.Create(state, message));

            // очищаем память
            GC.Collect();

            List<PackageWrapper> opwlst = new List<PackageWrapper>();

            try
            {
                _report(SOState.CryptoState.Start, $"Начинаем шифрование и подпись файла: {_inputFileWrapper}");

                // Если мы должны использовать собственный формат или размер данных потока больше или равно 2 ^ 32
                if (_customEncryptionFormat || _inputFileWrapper.FInfo.Length > Int32.MaxValue)
                    throw new OutOfMemoryException();   // генерируем исключение

                if (CancellationPending)
                {
                    logger.Info("Операция отменена пользователем");
                    return null;
                }

                opwlst.Add(new PackageWrapper(_inputFileWrapper.SignEncrypt(_signerCertificate, _recipientCertificate)));
            }
            catch (Exception ex)
            {
                // очищаем память
                GC.Collect();

                if (ex is OutOfMemoryException ||
                    (ex is CryptographicException && ((uint)ex.HResult) == 0x80093106))
                {
                    // Шифруем в своем формате

                    if (CancellationPending)
                    {
                        logger.Info("Операция отменена пользователем");
                        return null;
                    }

                    if (_customEncryptionFormat)
                        logger.Info("Шифруем в собственном формате");
                    else
                        logger.Info($"Файл: {_inputFileWrapper} слишком большой: {_inputFileWrapper.FInfo.Length}. Шифруем в собственном формате");

                    // Добавляем шифрованный файл в список
                    opwlst.Add(new PackageWrapper(_inputFileWrapper.Encrypt(_recipientCertificate)));

                    if (_signerCertificate != null)
                    {
                        logger.Info("Указан сертификат владельца - формируем отсоединенную подпись");

                        opwlst.Add(new PackageWrapper(_inputFileWrapper.Sign(_signerCertificate,true)));
                    }
                }
                else throw;
            }

            _report( SOState.CryptoState.Completed, $"Шифрование и подпись файла успешно завершены. Файл: {_inputFileWrapper}");
            return opwlst.ToArray();
        }

        /// <summary>
        /// Операция подписи
        /// </summary>
        /// <returns>Созданный артефакт</returns>
        protected PackageWrapper Sign()
        {
            Action<SOState.CryptoState, string> _report = (state, message)
                => Report(SOState.Create(state, message));

            // очищаем память
            GC.Collect();

            string filename = String.Empty;

            try
            {
                _report(SOState.CryptoState.Start, $"Подписываем файл: {_inputFileWrapper}");

                // Если мы должны сформировать присоединенную подпись или размер данных потока больше или равно 2 ^ 32
                if (_forсeDetachedSign || _inputFileWrapper.FInfo.Length > Int32.MaxValue)
                    throw new OutOfMemoryException();   // генерируем исключение

                if (CancellationPending)
                {
                    logger.Info("Операция отменена пользователем");
                    return null;
                }

                logger.Info("Формируем присоединенную подпись");

                filename = _inputFileWrapper.Sign(_signerCertificate, false);

            }
            catch (Exception ex)
            {
                // очищаем память
                GC.Collect();

                if (ex is OutOfMemoryException ||
                    (ex is System.Security.Cryptography.CryptographicException && ((uint)ex.HResult) == 0x80093106))
                {
                    if (CancellationPending)
                    {
                        logger.Info("Операция отменена пользователем");
                        return null;
                    }

                    logger.Info("Формируем отсоединенную подпись");

                    filename = _inputFileWrapper.Sign(_signerCertificate, true);
                }
                else throw;
            }

            _report(SOState.CryptoState.Completed, $"Подпись файла: {_inputFileWrapper} сформирована");

            return new PackageWrapper(filename);
        }

        /// <summary>
        /// Операция расшифровки
        /// </summary>
        /// <returns>Имя файла</returns>
        protected string Decrypt()
        {
            Action<SOState.CryptoState, string> _report = (state, message)
                => Report(SOState.Create(state, message));

            // очищаем память
            GC.Collect();

            _report(SOState.CryptoState.Start, $"Расшифровка и проверка файла: {_inputFileWrapper}");

            if (CancellationPending)
            {
                logger.Info("Операция отменена пользователем");
                return String.Empty;
            }

            ((PackageWrapper)_inputFileWrapper).Decrypt(_recipientCertificate);

            _report(SOState.CryptoState.Completed, "Расшифровка и проверка файла завершены");

            return _fileName;
        }

        /// <summary>
        /// Операция проверки отсоединенной подписи
        /// </summary>
        protected bool Verify()
        {
            Action<SOState.CryptoState, string> _report = (state, message) 
                => Report(SOState.Create(state, message));

            // очищаем память
            GC.Collect();

            try
            {
                PackageWrapper pw = _inputFileWrapper as PackageWrapper;

                _report(SOState.CryptoState.Start, $"Проверка отсоединенной подписи файла: {_inputFileWrapper}");

                System.Diagnostics.Debug.Assert(_fileName.Length > 0);

                if (CancellationPending)
                {
                    logger.Info("Операция отменена пользователем");
                    return false;
                }

                // Проверяем отсоединенную подпись для файла _fileName
                pw.Verify(_fileName);

                _report(SOState.CryptoState.Completed, $"Подпись файла {_inputFileWrapper} успешно проверена");

                return true;
            }
            catch (CryptographicException ex)
            {
                if (((uint)ex.HResult) == 0xC000A000)
                {
                    _report(SOState.CryptoState.Completed, $"Подпись файла {_inputFileWrapper} не прошла проверку");
                }
                else throw;
            }

            return false;
        }

        /// <summary>
        /// Операция проверки присоединенной подписи
        /// </summary>
        /// <returns>В случае успешной проверки имя файла.</returns>
        protected string VerifyEncode()
        {
            Action<SOState.CryptoState, string> _report = (state, message)
                => Report(SOState.Create(state, message));

            // очищаем память
            GC.Collect();

            try
            {
                PackageWrapper pw = _inputFileWrapper as PackageWrapper;

                _report(SOState.CryptoState.Start, $"Проверка присоединенной подписи файла: {_inputFileWrapper}");

                if (CancellationPending)
                {
                    logger.Info("Операция отменена пользователем");
                    return String.Empty;
                }

                // Проверяем присоeдиненную подпись и записываем подписанные данные в отдельный файл _fileName
                // Здесь именно CreateNew = true, т.к. файл с данными уже может существовать  с тем же именем
                FileWrapper.Write(pw.Verify(), _fileName, true);

                _report(SOState.CryptoState.Completed, $"Подпись файла {_inputFileWrapper} успешно проверена");

                return _fileName;
            }
            catch (CryptographicException ex)
            {
                if (((uint)ex.HResult) == 0xC000A000)
                {
                    _report(SOState.CryptoState.Completed, $"Подпись файла {_inputFileWrapper} не прошла проверку");
                }
                else throw;
            }

            return String.Empty;
        }

        /// <summary>
        /// Операция сбороса некорректных настроек в их значения по умолчанию
        /// </summary>
        public bool CheckIncorrectSettings()
        {
            Action<SOState.CryptoState, string> _report = (state, message)
                => Report(SOState.Create(state, message));

            _report(SOState.CryptoState.Start, "Проверка корректности настроек системы");

            bool result = true;

            if (_settings.OwnerWorkingFolder.Length == 0 || !Directory.Exists(_settings.OwnerWorkingFolder))
            {
                _settings.OwnerWorkingFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                logger.Info($"Установлено новое значение рабочего каталога: {_settings.OwnerWorkingFolder}");

                result = false;
            }

            if (_settings.OwnerCertificate != null)
            {
                if (!_settings.OwnerCertificate.Value.HasPrivateKey || !_settings.OwnerCertificate.Value.Verify())
                {
                    _settings.OwnerCertificate = null;

                    logger.Info("Сертификат владельца отозван или истек его срока действия.");

                    result = false;
                }
            }

            _report(SOState.CryptoState.Completed, "Проверка настроек закончена");
            return result;
        }

        /// <summary>
        /// Оповещает основное окно о выполнении операции
        /// </summary>
        /// <param name="state"></param>
        protected void Report(SOState state)
        {
            logger.Info(state.Message);
            ReportProgress(0, state);
        }
    }
}
