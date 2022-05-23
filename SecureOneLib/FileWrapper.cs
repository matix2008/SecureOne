using System;
using System.Collections.Generic;
using System.IO;

namespace SecureOneLib
{
    /// <summary>
    /// Реализует обертку над абстрактным файлом
    /// </summary>
    public class FileWrapper
    {
        /// <summary>
        /// Конструирует обертку
        /// </summary>
        /// <param name="filepath"></param>
        public FileWrapper(string filepath)
        {
            if (!File.Exists(filepath))
                throw new IOException("File is not exists.");

            FilePathString = filepath;
            FInfo = new FileInfo(filepath);
        }

        /// <summary>
        /// Возвращает путь к файлу
        /// </summary>
        public string FilePathString { get; }
        /// <summary>
        /// Возвращает информацию о файле
        /// </summary>
        public FileInfo FInfo { get; }

        /// <summary>
        /// Возвращает строку представляющую файл (полный путь)
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return FilePathString;
        }

        /// <summary>
        /// Возвращает массив строк - реквизиты файла
        /// </summary>
        /// <returns></returns>
        public virtual string[] GetRequisites()
        {
            List<string> requisites = new List<string>();

            requisites.Add("Каталог:");
            requisites.Add(FInfo.DirectoryName);

            requisites.Add("Имя файла:");
            requisites.Add(FInfo.Name);

            string prefix = "(байт)";
            float size = FInfo.Length;

            if (FInfo.Length < 1024)
            {
                // ничего не делаем
            }
            else if (FInfo.Length < 1048576)
            {
                prefix = "(Кб)";
                size /= 1024;
            }
            else if (FInfo.Length < 1073741824)
            {
                prefix = "(Мб)";
                size /= 1048576;
            }
            else
            {
                prefix = "(Гб)";
                size /= 1073741824;
            }

            requisites.Add($"Размер {prefix}");
            requisites.Add(size.ToString());

            requisites.Add("Дата модификации:");
            requisites.Add(FInfo.LastWriteTime.ToShortDateString());

            return requisites.ToArray();
        }


        #region Криптографические методы

        /// <summary>
        /// Подписывает файл, создавая присоединенную или отсоединенную подпись
        /// </summary>
        /// <param name="signerCert">Сертификат владельца</param>
        /// <param name="detached">Признак отсоединенной подписи</param>
        /// <returns>Имя созданного файла</returns>
        public string Sign(CertificateWrapper signerCert, bool detached)
        {
            byte[] sign = null;
            string ext = ".sig";

            if (detached)
                sign = Scrambler.SignDetached(File.OpenRead(FilePathString), signerCert.Value);
            else
            { 
                sign = Scrambler.SignAttached(File.ReadAllBytes(FilePathString), signerCert.Value);
                ext = ".p7s";
            }

            Write(sign, FilePathString + ext, false);

            return FilePathString + ext;
        }

        /// <summary>
        /// Подписывает и шифрует файл
        /// </summary>
        /// <param name="signerCert">Сертификат владельца</param>
        /// <param name="recipientCert">Сертификат получателя</param>
        /// <returns>Имя созданного файла</returns>
        public string SignEncrypt(CertificateWrapper signerCert, CertificateWrapper recipientCert)
        {
            byte[] carr = null;
            string ext = ".p7sm";

            if (signerCert != null)
                carr = Scrambler.SignEncrypt(File.ReadAllBytes(FilePathString), recipientCert.Value, signerCert.Value);
            else
            {
                carr = Scrambler.Encrypt(File.ReadAllBytes(FilePathString), recipientCert.Value);
                ext = ".p7m";
            }

            Write(carr, FilePathString + ext, false);

            return FilePathString + ext;
        }
        /// <summary>
        /// Шифрует файл
        /// </summary>
        /// <param name="recipientCert">Сертификат получателя</param>
        /// <returns>Имя созданного файла</returns>
        public string Encrypt(CertificateWrapper recipientCert)
        {
            using (FileStream ofs = new FileStream(FilePathString + ".enc", FileMode.Create))
            {
                Scrambler.Encrypt(File.OpenRead(FilePathString), ofs, recipientCert.Value);
            }

            return FilePathString + ".enc";
        }

        #endregion

        #region Статические методы
        /// <summary>
        /// Записывае данные файл
        /// </summary>
        /// <param name="data">Данные</param>
        /// <param name="filename">Имя файла</param>
        /// <param name="createnew">Признак нового файла</param>
        /// <returns>Имя созданного файла</returns>
        public static string Write(byte[] data, string filename, bool createnew)
        {
            using (FileStream fs = new FileStream(filename, (createnew) ? FileMode.CreateNew : FileMode.Create))
            {
                fs.Write(data, 0, data.Length);
            }

            return filename;
        }

        #endregion

    }

    /// <summary>
    /// Реализует обертку над файлом - крипто-контейнером
    /// </summary>
    public class PackageWrapper : FileWrapper
    {
        /// <summary>
        /// Поддерживаемые типы контейнеров
        /// </summary>
        public enum PackageType { Unknown, ENC, SIG, P7S, P7M, P7SM };

        /// <summary>
        /// Конструирует объект обертку крипто-контейнера
        /// </summary>
        /// <param name="filepath"></param>
        public PackageWrapper(string filepath)
            : base(filepath)
        {
            Type = PackageType.Unknown;
        }

        /// <summary>
        /// Перегруженная функция. Возвращает реквизиты крипто-контейнера
        /// </summary>
        /// <returns>Массив строк - реквизитов</returns>
        public override string[] GetRequisites()
        {
            List<string> requisites = new List<string>(base.GetRequisites());

            string ext = Path.GetExtension(FilePathString).ToLower();
            string description = String.Empty;

            switch(ext)
            {
                default: Type = PackageType.Unknown; description = "Неизвестный"; break;
                case ".enc": Type = PackageType.ENC; description = "Шифрованный файл - SecureOne"; break;
                case ".sig": Type = PackageType.SIG; description = "Отсоединенная электронная подпись - CMS / PKCS#7"; break;
                case ".p7s": Type = PackageType.P7S; description = "Присоединенная электронная подпись - CMS / PKCS#7"; break;
                case ".p7m": Type = PackageType.P7M; description = "Шифрованный файл - CMS / PKCS#7"; break;
                case ".p7sm": Type = PackageType.P7SM; description = "Шифрованный файл с электронной подписью - CMS / PKCS#7"; break;
            }

            requisites.Add("Тип контейнера:");
            requisites.Add(description);

            return requisites.ToArray();
        }

        /// <summary>
        /// Возвращает тип крипто-контейнера
        /// </summary>
        public PackageType Type { get; protected set;  }

        /// <summary>
        /// Возвращает исходное имя файла
        /// </summary>
        /// <returns></returns>
        public string GetNativeFilePath()
        {
            string native = Path.GetFileNameWithoutExtension(FilePathString);
            string path = Path.GetDirectoryName(FilePathString);

            return Path.Combine(path, native);
        }

        #region Криптографические методы
        /// <summary>
        /// Расшифровывает данные из крипто-контейнера
        /// </summary>
        /// <param name="recipientCert">Сертификат получателя</param>
        /// <returns>Имя расшифрованного файла</returns>
        public string Decrypt(CertificateWrapper recipientCert)
        {
            string ofn = GetNativeFilePath();

            using (FileStream ofs = new FileStream(ofn, FileMode.CreateNew))
            {
                if (Type == PackageWrapper.PackageType.ENC)
                    Scrambler.Decrypt(File.OpenRead(FilePathString), ofs, recipientCert.Value);
                else
                {
                    byte[] decrypted = Type == PackageWrapper.PackageType.P7M ? Scrambler.Decrypt(File.ReadAllBytes(FilePathString)) : Scrambler.VerifyDecrypt(File.ReadAllBytes(FilePathString));
                    ofs.Write(decrypted, 0, decrypted.Length);
                }
            }

            return ofn;
        }

        /// <summary>
        /// Проверяет присоединенную подпись (сам файл)
        /// </summary>
        /// <param name="verifySignatureOnly"></param>
        /// <returns>Подписанные данные, если проверка прошла успешно</returns>
        public byte[] Verify(bool verifySignatureOnly = false)
        {
            return Scrambler.Verify(File.ReadAllBytes(FilePathString), verifySignatureOnly);
        }

        /// <summary>
        /// Проверяет отсоединенную подпись (сам файл)
        /// </summary>
        /// <param name="datafilename">Файл данных для проверки подписи</param>
        /// <param name="verifySignatureOnly">True - если нужно проверить только подспись</param>
        public void Verify(string datafilename, bool verifySignatureOnly = false)
        {
            Scrambler.Verify(File.ReadAllBytes(FilePathString), File.OpenRead(datafilename), verifySignatureOnly);
        }

        #endregion
    }
}
