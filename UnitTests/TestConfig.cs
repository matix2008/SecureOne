using System;
using System.Linq;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace UnitTests
{
    /// <summary>
    /// Модуль конфигурации
    /// </summary>
    static class TestConfig
    {
        public const long _minFileSize = 1024;                  // 1 Кб
        public const long _maxSmallFileSize = 1024 * 100;       // 100 Кб
        public const long _mimBigFileSize = 1024 * 1024 * 100;  // 100 Мб
        public const long _maxFileSize = 1024 * 1024 * 800;     // 800 Мб

        public const StoreName DefaultStoreName = StoreName.My;
        public const StoreLocation DefaultStoreLocation = StoreLocation.CurrentUser;

        /// <summary>
        /// Статический конструктор - инициализирует конифигурационный модуль для выполнения тестов
        /// </summary>
        static TestConfig()
        {
            //DefaultFileRootLocation = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            DefaultFileRootLocation = @"d:\temp";

            GostPKCert = FindCertificate(DefaultStoreName, DefaultStoreLocation, filter: c => (c.IsGost() && c.HasPrivateKey));
            GostPubKCert = FindCertificate(DefaultStoreName, DefaultStoreLocation, filter: c => (c.IsGost() && !c.HasPrivateKey));

            NoGostPKCert = FindCertificate(DefaultStoreName, DefaultStoreLocation, filter: c => (!c.IsGost() && c.HasPrivateKey));
            NoGostPKCert2 = FindCertificate(DefaultStoreName, DefaultStoreLocation, filter: c => (!c.IsGost() && c.HasPrivateKey && (NoGostPKCert == null || !c.SerialNumber.Equals(NoGostPKCert.SerialNumber))));
            NoGostPubKCert = FindCertificate(DefaultStoreName, DefaultStoreLocation, filter: c => (!c.IsGost() && !c.HasPrivateKey));

            BigFile = FindFile(DefaultFileRootLocation, "*.*", filter: f => (f.Length >= _mimBigFileSize && f.Length < _maxFileSize)); // больше 100 и меньше 800 Мб
            SmallFile = FindFile(DefaultFileRootLocation, "*.*", filter: f => (f.Length >= _minFileSize && f.Length <= _maxSmallFileSize));  // больше 1 и меньше 100 Кб
        }

        /// <summary>
        /// Рабочий каталог для поиска файлов и создания временных файлов
        /// </summary>
        public static string DefaultFileRootLocation { get; }

        /// <summary>
        /// Сертификат №1 с закрытым ключом по ГОСТ
        /// </summary>
        public static X509Certificate2 GostPKCert { get;  }
        /// <summary>
        /// Сертификат без закрытого ключа по ГОСТ
        /// </summary>
        public static X509Certificate2 GostPubKCert { get; }
        /// <summary>
        /// Сертификат #1 закрытого ключа (не ГОСТ)
        /// </summary>
        public static X509Certificate2 NoGostPKCert { get; }
        /// <summary>
        /// Сертификат #2 закрытого ключа (не ГОСТ)
        /// </summary>
        public static X509Certificate2 NoGostPKCert2 { get; }
        /// <summary>
        /// Сертификат без закрытого ключа (не ГОСТ)
        /// </summary>
        public static X509Certificate2 NoGostPubKCert { get; }

        /// <summary>
        /// Большой файл
        /// </summary>
        public static FileStream BigFile { get; }
        /// <summary>
        /// Маленький файл
        /// </summary>
        public static FileStream SmallFile { get; }

        /// <summary>
        /// Формирует случайное имя файла
        /// </summary>
        /// <returns>Полный путь к файлу с новым именем</returns>
        public static string GetTempFileName()
        {
            if (Directory.Exists(DefaultFileRootLocation))
                return Path.Combine(DefaultFileRootLocation, string.Format(@"{0}.tmp", Guid.NewGuid()));

            return String.Empty;
        }

        /// <summary>
        /// Ищет сертификат по условию
        /// </summary>
        /// <param name="storeName">Хранилище</param>
        /// <param name="storeLocation">Расположение</param>
        /// <param name="filter">Фильтр</param>
        /// <returns>Найденый сертификат</returns>
        public static X509Certificate2 FindCertificate(StoreName storeName = DefaultStoreName, StoreLocation storeLocation = DefaultStoreLocation, Predicate<X509Certificate2> filter = null)
        {
            var store = new X509Store(storeName, storeLocation);
            store.Open(OpenFlags.OpenExistingOnly | OpenFlags.ReadOnly);

            try
            {
                foreach (var certificate in store.Certificates)
                {
                    if (filter == null || filter(certificate))
                    {
                        return certificate;
                    }
                }
            }
            finally
            {
                store.Close();
            }

            return null;
        }

        /// <summary>
        /// Ищет файл в заданной директории по условию и открывает его для чтения
        /// </summary>
        /// <param name="path">Верхнеуровневый каталог</param>
        /// <param name="pattern">Шаблон имени файла</param>
        /// <param name="filter">Фильтр</param>
        /// <returns>Поток связанный с данным файлом</returns>
        public static FileStream FindFile(string path = "", string pattern = "*.*", Predicate<FileInfo> filter = null)
        {
            if (path.Length == 0)
                path = DefaultFileRootLocation;

            if (Directory.Exists(path))
            {
                foreach( var file in Directory.EnumerateFiles(path, pattern, SearchOption.AllDirectories))
                {
                    FileInfo fi = new FileInfo(file);

                    if (filter == null || filter(fi))
                    {
                        return File.OpenRead(file);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Сравнивает два пока между собой
        /// </summary>
        /// <param name="s1">Поток 1</param>
        /// <param name="s2">Поток 1</param>
        /// <returns>Результат сравнения. True - содержимое потоков одинаково. False - потоки отличаются.</returns>
        public static bool CompareStreams(Stream s1, Stream s2)
        {
            if (s1.Length != s2.Length)
                return false;

            s1.Position = 0;
            s2.Position = 0;

            byte[] hash1 = null;
            byte[] hash2 = null;

            using (SHA1 sha1 = SHA1.Create())
            {
                hash1 = sha1.ComputeHash(s1);
                hash2 = sha1.ComputeHash(s2);
            }

            return hash1.SequenceEqual(hash2);
        }
    }
}
