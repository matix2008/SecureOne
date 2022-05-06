using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using GostCryptography;

namespace UnitTests
{
    static class TestConfig
    {
        public const StoreName DefaultStoreName = StoreName.My;
        public const StoreLocation DefaultStoreLocation = StoreLocation.CurrentUser;

        static TestConfig()
        {
            DefaultFileRootLocation = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            GostPKCert = FindCertificate(DefaultStoreName, DefaultStoreLocation, filter: c => (c.IsGost() && c.HasPrivateKey));
            GostPKCert2 = FindCertificate(DefaultStoreName, DefaultStoreLocation, filter: c => (c.IsGost() && c.HasPrivateKey && (GostPKCert == null || !c.SerialNumber.Equals(GostPKCert.SerialNumber))));
            GostPubKCert = FindCertificate(DefaultStoreName, DefaultStoreLocation, filter: c => (c.IsGost() && !c.HasPrivateKey));

            NoGostPKCert = FindCertificate(DefaultStoreName, DefaultStoreLocation, filter: c => (!c.IsGost() && c.HasPrivateKey));
            NoGostPKCert2 = FindCertificate(DefaultStoreName, DefaultStoreLocation, filter: c => (!c.IsGost() && c.HasPrivateKey && (NoGostPKCert == null || !c.SerialNumber.Equals(NoGostPKCert.SerialNumber))));
            NoGostPubKCert = FindCertificate(DefaultStoreName, DefaultStoreLocation, filter: c => (!c.IsGost() && !c.HasPrivateKey));

            BigFile = FindFile(DefaultFileRootLocation, "*.*", filter: f => f.Length > 104857600); // больше 100 Мб
            SmallFile = FindFile(DefaultFileRootLocation, "*.*", filter: f => f.Length < 102400);  // меньше 100 Кб
        }

        public static string DefaultFileRootLocation { get; }

        public static X509Certificate2 GostPKCert { get;  }
        public static X509Certificate2 GostPKCert2 { get; }
        public static X509Certificate2 GostPubKCert { get; }
        public static X509Certificate2 NoGostPKCert { get; }
        public static X509Certificate2 NoGostPKCert2 { get; }
        public static X509Certificate2 NoGostPubKCert { get; }

        public static FileStream BigFile { get; }
        public static FileStream SmallFile { get; }

        public static string GetTempFileName()
        {
            if (Directory.Exists(DefaultFileRootLocation))
                return Path.Combine(DefaultFileRootLocation, string.Format(@"{0}.tmp", Guid.NewGuid()));

            return String.Empty;
        }

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

        public static bool CompareStreams(Stream s1, Stream s2)
        {
            if (s1.Length != s2.Length)
                return false;

            s1.Position = 0;
            s2.Position = 0;

            MemoryStream ms1 = new MemoryStream();
            s1.CopyTo(ms1);

            MemoryStream ms2 = new MemoryStream();
            s2.CopyTo(ms2);

            var msArray1 = ms1.ToArray();
            var msArray2 = ms2.ToArray();

            return msArray1.SequenceEqual(msArray2);
        }
    }
}
