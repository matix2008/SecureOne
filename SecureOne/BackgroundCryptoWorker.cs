using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security;
using System.Windows.Forms;
using System.ComponentModel;
using SecureOneLib;
using SecureOneLib.Crypto;
using System.Security.Cryptography;

namespace SecureOne
{
    public class BackgroundCryptoWorker : BackgroundWorker
    {
        public enum AsyncCryptoOpration { Unknown, Sign, SignEncrypt, Verify, Decrypt, VerifyEncode, CheckSettigs };

        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        protected Form _owner;

        protected object _lockObj = new object();

        protected AsyncCryptoOpration _asyncCryptoOpration;
        protected FileWrapper _inputFileWrapper;
        protected string _fileName;
        protected CertificateWrapper _signerCertificate;
        protected CertificateWrapper _recipientCertificate;
        protected bool _forсeDetachedSign;
        protected bool _customEncryptionFormat;
        protected Settings _settings;

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

        public bool StartDecrypt(PackageWrapper ipw, string ofn, CertificateWrapper recipientCert)
        {
            if (CheckNotBusy())
            {
                lock (_lockObj)
                {
                    _asyncCryptoOpration = AsyncCryptoOpration.Decrypt;
                    _inputFileWrapper = ipw;
                    _signerCertificate = null;
                    _recipientCertificate = recipientCert;
                    _fileName = ofn;
                }

                RunWorkerAsync();
                return true;
            }

            return false;
        }

        public bool StartVerifyEncode(PackageWrapper ipw, string ofn)
        {
            if (CheckNotBusy())
            {
                lock (_lockObj)
                {
                    _asyncCryptoOpration = AsyncCryptoOpration.VerifyEncode;
                    _inputFileWrapper = ipw;
                    _signerCertificate = null;
                    _recipientCertificate = null;
                    _fileName = ofn;
                }

                RunWorkerAsync();
                return true;
            }

            return false;
        }

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

        protected bool CheckNotBusy()
        {
            if (this.IsBusy)
            {
                Utils.MessageHelper.Warning(_owner, "Предыдущая операция еще не закончена. Дождитесь ее заверешения!");
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
            
            BackgroundCryptoWorker worker = sender as BackgroundCryptoWorker;
            if (worker.CancellationPending == true)
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
                    catch(Exception)
                    {
                        throw;
                    }
                }
            }
        }

        protected PackageWrapper[] Encrypt()
        {
            // Инициализируем объект-состоянение
            SOState sos = new SOState();

            // Лямбда для оповещения о состоянии
            Action<SOState.CryptoState, string> _report = (state, message) =>
            {
                logger.Info(message);

                sos.State = state;
                sos.Message = message;

                ReportProgress(0, sos);
            };

            // Короткая лямбда
            Action<string> report = message => _report(SOState.CryptoState.InProgress, message);
            Action<string> log = message => logger.Info(message);

            _report(SOState.CryptoState.Start, $"Начинаем шифрование и подпись файла: {_inputFileWrapper}");

            List<PackageWrapper> opwlst = new List<PackageWrapper>();

            using (FileStream ifs = _inputFileWrapper.OpenRead())
            {
                try
                {
                    // очищаем память
                    GC.Collect();

                    // Если мы должны использовать собственный формат или размер данных потока больше или равно 2 ^ 32
                    if (_customEncryptionFormat || ifs.Length > Int32.MaxValue)
                        throw new OutOfMemoryException();   // генерируем исключение

                    // Пытаемся аллоцировать буффер нужного размера
                    byte[] buffer = new byte[ifs.Length];

                    // Читаем данные в буффер
                    ifs.Read(buffer, 0, (int)ifs.Length);

                    // ссылка на шифрованный массив байтов
                    byte[] carr = null;

                    if (_signerCertificate != null)
                    {
                        log("Указан сертификат владельца - шифруем и подписываем");

                        carr = Coder.SignEncrypt(buffer, _recipientCertificate.Value, _signerCertificate.Value);

                        log($"Сохраняем шифрованные данные в файл: {_inputFileWrapper + ".p7sm"}");

                        // Сохраняем шифрованные и подписанные данные в файл CMS / PKCS#7
                        using (FileStream ofs = _inputFileWrapper.OpenWrite(".p7sm"))
                        {
                            ofs.Write(carr, 0, carr.Length);
                        }

                        opwlst.Add(new PackageWrapper(_inputFileWrapper + ".p7sm"));
                    }
                    else
                    {
                        log("Сертификат владельца не указан - не подписываем. Только шифруем");

                        // Шифруем и подписываем
                        carr = Coder.Encrypt(buffer, _recipientCertificate.Value);

                        log($"Сохраняем шифрованные данные в файл: {_inputFileWrapper + ".p7m"}");

                        // Сохраняем шифрованные данные в файл CMS / PKCS#7
                        using (FileStream ofs = _inputFileWrapper.OpenWrite(".p7m"))
                        {
                            ofs.Write(carr, 0, carr.Length);
                        }

                        opwlst.Add(new PackageWrapper(_inputFileWrapper + ".p7m"));
                    }
                }
                catch (Exception ex)
                {
                    // очищаем память
                    GC.Collect();

                    if (ex is OutOfMemoryException ||
                        (ex is CryptographicException && ((uint)ex.HResult) == 0x80093106))
                    {
                        // Шифруем в своем формате

                        if (_customEncryptionFormat)
                            log($"Шифруем в собственном формате");
                        else
                            log($"Файл: {_inputFileWrapper} слишком большой: {ifs.Length}. Шифруем в собственном формате");

                        // Сохраняем шифрованные данные в файл
                        using (FileStream ofs = _inputFileWrapper.OpenWrite(".enc"))
                        {
                            log($"Шифруем и сохраняем шифрованные данные в файл: {_inputFileWrapper + ".enc"}");

                            // Восстанавливаем позицию потока в начало
                            ifs.Position = 0;

                            // Шифруем и сохраняем данные в выходной поток
                            Coder.Encrypt(ifs, ofs, _recipientCertificate.Value);
                        }

                        // Добавляем шифрованный файл в список
                        opwlst.Add(new PackageWrapper(_inputFileWrapper + ".enc"));

                        if (_signerCertificate != null)
                        {
                            log("Указан сертификат владельца - формируем отсоединенную подпись");

                            // Восстанавливаем позицию потока в начало
                            ifs.Position = 0;

                            // Формируем отсоединенную подпись
                            byte[] sign = Coder.SignDetached(ifs, _signerCertificate.Value);

                            log($"Сохраняем данные подписи в файл: {_inputFileWrapper + ".sig"}");

                            // Сохраняем данные подписи в файл CMS / PKCS#7
                            using (FileStream ofs = _inputFileWrapper.OpenWrite(".sig"))
                            {
                                ofs.Write(sign, 0, sign.Length);
                            }

                            opwlst.Add(new PackageWrapper(_inputFileWrapper + ".sig"));
                        }
                    }
                    else throw;
                }
            }

            _report( SOState.CryptoState.Completed, $"Шифрование и подпись файла успешно завершены. Файл: {_inputFileWrapper}");
            return opwlst.ToArray();
        }

        protected PackageWrapper Sign()
        {
            // Инициализируем объект-состоянение
            SOState sos = new SOState();

            // Лямбда для оповещения о состоянии
            Action<SOState.CryptoState, string> _report = (state, message) =>
            {
                logger.Info(message);

                sos.State = state;
                sos.Message = message;

                ReportProgress(0, sos);
            };

            // Короткая лямбда
            Action<string> report = message => _report(SOState.CryptoState.InProgress, message);
            Action<string> log = message => logger.Info(message);

            _report(SOState.CryptoState.Start, $"Подписываем файл: {_inputFileWrapper}");

            PackageWrapper opw = null;
            using (FileStream ifs = _inputFileWrapper.OpenRead())
            {
                string ext = ".p7s";
                byte[] sign = null;

                try
                {
                    // очищаем память
                    GC.Collect();

                    // Если мы должны сформировать присоединенную подпись или размер данных потока больше или равно 2 ^ 32
                    if (_forсeDetachedSign || ifs.Length > Int32.MaxValue)
                        throw new OutOfMemoryException();   // генерируем исключение

                    // Пытаемся аллоцировать буфер нужной длины
                    byte[] buffer = new byte[ifs.Length];
                    // Читаем данные в буффер
                    ifs.Read(buffer, 0, (int)ifs.Length);

                    log($"Формируем присоединенную подпись");

                    sign = Coder.SignAttached(buffer, _signerCertificate.Value);
                }
                catch (Exception ex)
                {
                    // очищаем память
                    GC.Collect();

                    if (ex is OutOfMemoryException ||
                        (ex is System.Security.Cryptography.CryptographicException && ((uint)ex.HResult) == 0x80093106))
                    {
                        log($"Формируем отсоединенную подпись");

                        // Если мы тут формируем отсоединенную подпись
                        sign = Coder.SignDetached(ifs, _signerCertificate.Value);
                        ext = ".sig";
                    }
                    else throw;
                }

                log($"Сохраняем данные подписи в файл: {_inputFileWrapper + ext}");

                // Сохраняем данные подписи в файл CMS / PKCS#7
                using (FileStream ofs = _inputFileWrapper.OpenWrite(ext))
                {
                    ofs.Write(sign, 0, sign.Length);
                }

                opw = new PackageWrapper(_inputFileWrapper + ext);
            }

            _report(SOState.CryptoState.Completed, $"Подпись файла: {_inputFileWrapper} сформирована");
            return opw;
        }

        protected string Decrypt()
        {
            // Инициализируем объект-состоянение
            SOState sos = new SOState();

            // Лямбда для оповещения о состоянии
            Action<SOState.CryptoState, string> _report = (state, message) =>
            {
                logger.Info(message);

                sos.State = state;
                sos.Message = message;

                ReportProgress(0, sos);
            };

            // Короткая лямбда
            Action<string> report = message => _report(SOState.CryptoState.InProgress, message);
            Action<string> log = message => logger.Info(message);

            // очищаем память
            GC.Collect();

            _report(SOState.CryptoState.Start, $"Расшифровка и проверка подписи файла: {_inputFileWrapper}");

            // Здесь именно FileMode = CreateNew, т.к. файл с данными уже может существовать  с тем же именем
            using (FileStream ofs = new FileStream(_fileName, FileMode.CreateNew))
            {
                if (((PackageWrapper)_inputFileWrapper).Type == PackageWrapper.PackageType.ENC)
                {
                    // Если мы тут, значит это шифрованный файл собственного формата
                    using (FileStream ifs = _inputFileWrapper.OpenRead())
                    {
                        log($"Рассшифровываем и сохраняем данные в файл: {_fileName}");
                        
                        // Расшифровываем его и сохраняем расшифрованные данные в файл
                        Coder.Decrypt(ifs, ofs, _recipientCertificate.Value);
                    }
                }
                else
                {
                    log($"Читаем в массив данные файла и расшифровываем его: {_inputFileWrapper}");

                    // Если мы тут, значит это p7m или p7sm файл
                    byte[] encdata = _inputFileWrapper.ReadAllBytes();

                    byte[] decrypted = ((PackageWrapper)_inputFileWrapper).Type == PackageWrapper.PackageType.P7M 
                        ? Coder.Decrypt(encdata) : Coder.VerifyDecrypt(encdata);

                    //if (((PackageWrapper)_inputFileWrapper).Type == PackageWrapper.PackageType.P7M)
                    //    decrypted = Coder.Decrypt(encdata); // Расшифровываем
                    //else
                    //    decrypted = Coder.VerifyDecrypt(encdata);   // Проверяем и расшифровываем

                    log($"Сохраняем расшифрованные данные в файл: {_fileName}");

                    // Сохраняем расшифрованные данные в файл
                    ofs.Write(decrypted, 0, decrypted.Length);
                }
            }

            _report(SOState.CryptoState.Completed, "Расшифровка и проверка подписи файла завершены");
            return _fileName;
        }

        /// <summary>
        /// Проверяет и возвращает результат проверки отсоединенной подписи
        /// </summary>
        /// <returns>True- в случае успеха</returns>
        protected bool Verify()
        {
            // Инициализируем объект-состоянение
            SOState sos = new SOState();

            // Лямбда для оповещения о состоянии
            Action<SOState.CryptoState, string> _report = (state, message) =>
            {
                logger.Info(message);

                sos.State = state;
                sos.Message = message;

                ReportProgress(0, sos);
            };

            // Короткая лямбда
            Action<string> report = message => _report(SOState.CryptoState.InProgress, message);
            Action<string> log = message => logger.Info(message);

            // очищаем память
            GC.Collect();

            _report(SOState.CryptoState.Start, $"Проверка подписи файла: {_inputFileWrapper}");

            try
            {
                // Читаем данные подписи
                byte[] sign = _inputFileWrapper.ReadAllBytes();

                System.Diagnostics.Debug.Assert(_fileName.Length > 0);

                // Открываем и читаем данные файла для проверки подписи
                using (FileStream ifs = File.OpenRead(_fileName))
                {
                    log($"Начинаем проверку отсоединенной подписи");
                    Coder.Verify(sign, ifs);
                }

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
        /// Проверяет присоединенную подпись и возвращает данные
        /// </summary>
        /// <returns>В случае успешной проверки подписанные данные. В противном случае null</returns>
        protected string VerifyEncode()
        {
            // Инициализируем объект-состоянение
            SOState sos = new SOState();

            // Лямбда для оповещения о состоянии
            Action<SOState.CryptoState, string> _report = (state, message) =>
            {
                logger.Info(message);

                sos.State = state;
                sos.Message = message;

                ReportProgress(0, sos);
            };

            // Короткая лямбда
            Action<string> report = message => _report(SOState.CryptoState.InProgress, message);
            Action<string> log = message => logger.Info(message);

            // очищаем память
            GC.Collect();

            _report(SOState.CryptoState.Start, $"Проверка присоединенной подписи файла: {_inputFileWrapper}");

            byte[] sign = _inputFileWrapper.ReadAllBytes();
            try
            {
                log($"Начинаем проверку присоединенной подписи");

                byte[] decoded = Coder.Verify(sign);

                _report(SOState.CryptoState.Completed, $"Подпись файла {_inputFileWrapper} успешно проверена");

                // Здесь именно FileMode = CreateNew, т.к. файл с данными уже может существовать  с тем же именем
                using (FileStream ofs = new FileStream(_fileName, FileMode.CreateNew))
                {
                    ofs.Write(decoded, 0, decoded.Length);
                }

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
        /// Сбрасывает некорректные настройки в их значения по умолчанию
        /// </summary>
        public bool CheckIncorrectSettings()
        {
            // Инициализируем объект-состоянение
            SOState _sos = new SOState();

            // Лямбда для оповещения о состоянии
            Action<SOState.CryptoState, string> _report = (state, message) =>
            {
                logger.Info(message);

                _sos.State = state;
                _sos.Message = message;

                ReportProgress(0, _sos);
            };

            // Короткая лямбда
            Action<string> report = message => _report(SOState.CryptoState.InProgress, message);
            Action<string> log = message => logger.Info(message);

            _report(SOState.CryptoState.Start, "Проверка корректности настроек системы");

            bool result = true;

            if (_settings.OwnerWorkingFolder.Length == 0 || !Directory.Exists(_settings.OwnerWorkingFolder))
            {
                _settings.OwnerWorkingFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                log($"Установлено новое значение рабочего каталога: {_settings.OwnerWorkingFolder}");
                result = false;
            }

            if (_settings.OwnerCertificate != null)
            {
                if (!_settings.OwnerCertificate.Value.HasPrivateKey || !_settings.OwnerCertificate.Value.Verify())
                {
                    _settings.OwnerCertificate = null;
                    log($"Сертификат владельца отозван или истек его срока действия.");
                    result = false;
                }
            }

            _report(SOState.CryptoState.Completed, "Проверка настроек закончена");
            return result;
        }
    }
}
