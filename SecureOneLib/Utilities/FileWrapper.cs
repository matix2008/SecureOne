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
        public FileWrapper(string filepath)
        {
            if (!File.Exists(filepath))
                throw new ArgumentException("File is not exists.");

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

        //public FileStream OpenRead()
        //{
        //    return File.OpenRead(FilePathString);
        //}

        //public byte[] ReadAllBytes()
        //{
        //    return File.ReadAllBytes(FilePathString);
        //}

        //public FileStream OpenWrite(string ext, bool createnew = false)
        //{
        //    return new FileStream(FilePathString + ext, (createnew) ? FileMode.CreateNew : FileMode.Create);
        //}

        #region Криптографические методы

        public string Sign(CertificateWrapper signerCert, bool detached)
        {
            byte[] sign = null;
            string ext = ".sig";

            if (detached)
                sign = Coder.SignDetached(File.OpenRead(FilePathString), signerCert.Value);
            else
            { 
                sign = Coder.SignAttached(File.ReadAllBytes(FilePathString), signerCert.Value);
                ext = ".p7s";
            }

            Write(sign, FilePathString + ext, false);

            return FilePathString + ext;
        }

        public string SignEncrypt(CertificateWrapper signerCert, CertificateWrapper recipientCert)
        {
            byte[] carr = null;
            string ext = ".p7sm";

            if (signerCert != null)
                carr = Coder.SignEncrypt(File.ReadAllBytes(FilePathString), recipientCert.Value, signerCert.Value);
            else
            {
                carr = Coder.Encrypt(File.ReadAllBytes(FilePathString), recipientCert.Value);
                ext = ".p7m";
            }

            Write(carr, FilePathString + ext, false);

            return FilePathString + ext;
        }

        public string Encrypt(CertificateWrapper recipientCert)
        {
            using (FileStream ofs = new FileStream(FilePathString + ".enc", FileMode.Create))
            {
                Coder.Encrypt(File.OpenRead(FilePathString), ofs, recipientCert.Value);
            }

            return FilePathString + ".enc";
        }

        #endregion

        #region Статические методы

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

    public class PackageWrapper : FileWrapper
    {
        public enum PackageType { Unknown, ENC, SIG, P7S, P7M, P7SM };
        public enum SignStatus  { NotFound, UnknownDetached, Ok, Failed }

        public PackageWrapper(string filepath)
            : base(filepath)
        {
            Type = PackageType.Unknown;
        }

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

        public PackageType Type { get; protected set;  }

        public string GetNativeFilePath()
        {
            string native = Path.GetFileNameWithoutExtension(FilePathString);
            string path = Path.GetDirectoryName(FilePathString);

            return Path.Combine(path, native);
        }

        #region Криптографические методы

        public string Decrypt(CertificateWrapper recipientCert)
        {
            string ofn = GetNativeFilePath();

            using (FileStream ofs = new FileStream(ofn, FileMode.CreateNew))
            {
                if (Type == PackageWrapper.PackageType.ENC)
                    Coder.Decrypt(File.OpenRead(FilePathString), ofs, recipientCert.Value);
                else
                {
                    byte[] decrypted = Type == PackageWrapper.PackageType.P7M ? Coder.Decrypt(File.ReadAllBytes(FilePathString)) : Coder.VerifyDecrypt(File.ReadAllBytes(FilePathString));
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
            return Coder.Verify(File.ReadAllBytes(FilePathString), verifySignatureOnly);
        }

        /// <summary>
        /// Проверяет отсоединенную подпись (сам файл)
        /// </summary>
        /// <param name="datafilename">Файл данных для проверки подписи</param>
        /// <param name="verifySignatureOnly">True - если нужно проверить только подспись</param>
        public void Verify(string datafilename, bool verifySignatureOnly = false)
        {
            Coder.Verify(File.ReadAllBytes(FilePathString), File.OpenRead(datafilename), verifySignatureOnly);
        }

        #endregion
    }
}
