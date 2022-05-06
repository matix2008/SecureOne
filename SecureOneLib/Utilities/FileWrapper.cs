using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Pkcs;
using System.IO;
using System.Formats.Asn1;
using SecureOneLib.Crypto;

namespace SecureOneLib.Utilities
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
        /// Возвращает строку представляющую файл
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return FilePathString;
        }

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

    }

    public class PackageWrapper : FileWrapper
    {
        public enum PackageType { Unknown, ENC, SIG, P7S, P7M, P7SM };
        public enum SignStatus  { NotFound, UnknownDetached, Ok, Failed }

        public PackageWrapper(string filepath, CertificateWrapper recipientCert)
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
    }
}
