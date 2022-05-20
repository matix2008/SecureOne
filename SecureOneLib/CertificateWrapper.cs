using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.Xml;
using System.Text.RegularExpressions;

namespace SecureOneLib
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
            Value = cert ?? throw new SOCertificateNotFoundException($"Can't find valid certificate with this subject: '{subject}'.");
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

        /// <summary>
        /// Создает сертификат и возвращает обертку на основе строки сериализации
        /// </summary>
        /// <param name="certstr"></param>
        /// <returns>Обертка сертификата</returns>
        public static CertificateWrapper Parse(string certstr)
        {
            if (certstr.Length == 0)
                return null;

            Regex rx = new Regex("CN:.+SN", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);
            MatchCollection matches = rx.Matches(certstr);
            if (matches.Count != 1)
                throw new SOCertificateNotFoundException("Invalid certificate string.");

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
        /// Ищет сертификат в заданном хранилище по идентификатору субъекта
        /// </summary>
        /// <param name="storeLocation">Хранилище</param>
        /// <param name="subjIdentifier">Идентификатор субъекта</param>
        /// <returns>Сертификат или null</returns>
        public static X509Certificate2 FindCertificateBySubjectIdentifier(StoreLocation storeLocation, SubjectIdentifier subjIdentifier)
        {
            if (subjIdentifier == null)
                throw new ArgumentNullException("subjIdentifier");

            X509Store store = new X509Store(storeLocation);
            store.Open(OpenFlags.ReadOnly);
            X509Certificate2Collection certCollection = store.Certificates;
            X509Certificate2 x509 = null;

            string SerialNumber = String.Empty;
            string IssuerName = String.Empty;

            X509IssuerSerial issuerSerial;

            if (subjIdentifier.Type == SubjectIdentifierType.IssuerAndSerialNumber)
            {
                issuerSerial = (X509IssuerSerial)subjIdentifier.Value;
            }

            foreach (X509Certificate2 c in certCollection)
            {
                if (c.SerialNumber == issuerSerial.SerialNumber && c.Issuer == issuerSerial.IssuerName)
                {
                    x509 = c;
                    break;
                }
            }

            store.Close();
            return x509;
        }
    }

    /// <summary>
    /// Реализует обертку над коллекцией сертификатов X509Certificate2Collection
    /// </summary>
    public class CertificateCollectionWrapper
    {
        /// <summary>
        /// Конструирует объект обертку
        /// </summary>
        /// <param name="coll">Коллекция сертификатов</param>
        public CertificateCollectionWrapper(X509Certificate2Collection coll)
        {
            Value = coll ?? throw new ArgumentNullException("cert");
            Count = Value.Count;
        }
        /// <summary>
        /// Конструирует объект обертку
        /// </summary>
        /// <param name="arr">Массив сертификатов</param>
        public CertificateCollectionWrapper(CertificateWrapper[] arr)
        {
            if (arr == null)
                throw new ArgumentNullException("arr");

            Value = new X509Certificate2Collection();

            foreach (var cw in arr)
                Value.Add(cw.Value);

            Count = Value.Count;
        }
        /// <summary>
        /// Конструирует объект обертку
        /// </summary>
        /// <param name="collstr">Строка сериализации</param>
        public CertificateCollectionWrapper(string collstr)
        {
            List<CertificateWrapper> cwl = new List<CertificateWrapper>();

            string[] elems = collstr.Split(new string[] { "%%" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var s in elems)
                cwl.Add(CertificateWrapper.Parse(s));

            Value = new X509Certificate2Collection(cwl.Select(x => x.Value).ToArray());
            Count = Value.Count;
        }

        /// <summary>
        /// Коллекция сертификатов
        /// </summary>
        public X509Certificate2Collection Value { get; }

        /// <summary>
        /// Коллекция сертификатов
        /// </summary>
        public int Count { get; }

        /// <summary>
        /// Возвращает строку представляющую коллекцию
        /// </summary>
        /// <returns>Строка сериализации</returns>
        public override string ToString()
        {
            string result = String.Empty;
            foreach (var c in Value)
                result += (CertificateWrapper.GetCertShortInfo(c) + "%%");
            return result;
        }
    }
}
