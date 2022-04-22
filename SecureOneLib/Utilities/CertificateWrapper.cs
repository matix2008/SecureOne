using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

namespace SecureOneLib.Utilities
{
    /// <summary>
    /// Реализует обертку над сертификатом X509Certificate2
    /// </summary>
    public class CertificateWrapper
    {
        /// <summary>
        /// Создает обертку над сертификатом
        /// </summary>
        /// <param name="cert"></param>
        public CertificateWrapper(X509Certificate2 cert)
        {
            Value = cert ?? throw new ArgumentNullException("cert");
        }

        public CertificateWrapper(string subject)
        {
            X509Certificate2 cert = FindCertificateBySubject(subject);
            Value = cert ?? throw new ArgumentException($"Can't find valid certificate with this subject: '{subject}'.");
        }

        /// <summary>
        /// Сертификат
        /// </summary>
        public X509Certificate2 Value { get; }

        /// <summary>
        /// Возвращает строку представляющую сертификат
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return GetCertShortInfo(Value);
        }

        public static CertificateWrapper Parse(string certstr)
        {
            Regex rx = new Regex("CN:.+SN", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);
            MatchCollection matches = rx.Matches(certstr);
            if (matches.Count != 1)
                throw new ArgumentException("Invalid certificate string.");
            string match = matches[0].Value;
            return new CertificateWrapper(match.Substring(4, match.Length - 7));
        }

        /// <summary>
        /// Возвращает строку представляющую сертификат
        /// </summary>
        /// <param name="cert">Сертификат</param>
        /// <returns>Строка</returns>
        public static string GetCertShortInfo(X509Certificate2 cert)
        {
            if (cert == null)
                throw new ArgumentNullException("cert");

            return "CN: " + cert.Subject + " SN: " + cert.SerialNumber;
        }
        /// <summary>
        /// Ищет сертификат по подстроке в названии субъекта
        /// </summary>
        /// <param name="subjectName">Имя субъекта</param>
        /// <returns>Сертификат или null</returns>
        public static X509Certificate2 FindCertificateBySubject(string subjectName)
        {
            X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);

            X509Certificate2Collection fcollection = (X509Certificate2Collection)store.Certificates.
                Find(X509FindType.FindByTimeValid, DateTime.Now, false);

            foreach (var cert in fcollection)
                if (cert.Subject.Contains(subjectName))
                    return cert;

            store.Close();
            return null;
        }
        /// <summary>
        /// Ищет сертификат по подстроке в названии субъекта
        /// </summary>
        /// <param name="sn">Серийный номер</param>
        /// <returns>Сертификат или null</returns>
        public static X509Certificate2 FindCertificateBySN(string sn)
        {
            X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);

            X509Certificate2Collection fcollection = (X509Certificate2Collection)store.Certificates.
                Find(X509FindType.FindByTimeValid, DateTime.Now, false);

            foreach (var cert in fcollection)
                if (cert.SerialNumber.Equals(sn))
                    return cert;

            store.Close();
            return null;
        }
    }

    /// <summary>
    /// Обертка над коллекцией сертификатов
    /// </summary>
    public class CertificateCollectionWrapper
    {
        public CertificateCollectionWrapper(X509Certificate2Collection coll)
        {
            Value = coll ?? throw new ArgumentNullException("cert");
        }

        public CertificateCollectionWrapper(CertificateWrapper[] arr)
        {
            if (arr == null)
                throw new ArgumentNullException("arr");

            Value = new X509Certificate2Collection();

            foreach (var cw in arr)
                Value.Add(cw.Value);
        }

        public CertificateCollectionWrapper(string collstr)
        {
            List<CertificateWrapper> cwl = new List<CertificateWrapper>();

            string[] elems = collstr.Split(new string[] { "%%" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var s in elems)
                cwl.Add(CertificateWrapper.Parse(s));

            Value = new X509Certificate2Collection(cwl.Select(x => x.Value).ToArray());
        }

        /// <summary>
        /// Коллекция сертификатов
        /// </summary>
        public X509Certificate2Collection Value { get; }

        /// <summary>
        /// Возвращает строку представляющую коллекцию
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string result = String.Empty;
            foreach (var c in Value)
                result += (CertificateWrapper.GetCertShortInfo(c) + "%%");
            return result;
        }
    }
}
