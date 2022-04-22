using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Pkcs;
using GostCryptography.Base;
using GostCryptography.Native;
using GostCryptography.Pkcs;
using GostCryptography.Gost_R3410;
using System.IO;

namespace SecureOneLib.Crypto
{
    public class MySigner
    {
        public MySigner()
        {

        }

        /// <summary>
        /// Подписывает хэш потока и формирует отсоединенную подпись
        /// </summary>
        /// <param name="dataStream">Поток подписываемых данных</param>
        /// <param name="signerCert">Сертификат закрытого ключа подписывающего</param>
        /// <returns>Данные подписи</returns>
        public byte[] Sign(Stream dataStream, X509Certificate2 signerCert)
        {
            if (!signerCert.HasPrivateKey)
                throw new CryptographicException("Certificate for signing has not a private key.");

            byte[] hash;
            AsymmetricAlgorithm privateKey = signerCert.GetPrivateKeyAlgorithm();
            AsymmetricSignatureFormatter formatter = null;

            if (signerCert.IsGost())
            {
                using (var hashAlg = (((GostAsymmetricAlgorithm)privateKey).CreateHashAlgorithm()))
                {
                    hash = hashAlg.ComputeHash(dataStream);
                }
            }
            else
            {
                string str = privateKey.SignatureAlgorithm;

                using (SHA256 hashalg = SHA256.Create())
                {
                    hash = hashalg.ComputeHash(dataStream);
                }
            }

            //ContentInfo content = new ContentInfo(new Oid("1.2.840.113549.1.7.5"),hash);
            ContentInfo content = new ContentInfo(hash);
            SignedCms signedCms = new SignedCms(content, true);
            CmsSigner signer = new CmsSigner(SubjectIdentifierType.IssuerAndSerialNumber, signerCert);
            signer.IncludeOption = X509IncludeOption.WholeChain;
            signedCms.ComputeSignature(signer);
            return signedCms.Encode();
        }

        public void Verify(byte[] sign, Stream dataStream)
        {
            SignedCms signedCms1 = new SignedCms();
            signedCms1.Decode(sign);
            X509Certificate2 signerCert = null;

            foreach (SignerInfo si in signedCms1.SignerInfos)
            {
                si.CheckSignature(true);
                break;
            }


            byte[] hash;
            AsymmetricAlgorithm privateKey = signerCert.GetPublicKeyAlgorithm();

            if (signerCert.IsGost())
            {
                using (var hashAlg = (((GostAsymmetricAlgorithm)privateKey).CreateHashAlgorithm()))
                {
                    hash = hashAlg.ComputeHash(dataStream);
                }
            }
            else
            {
                string str = privateKey.SignatureAlgorithm;

                using (SHA256 hashalg = SHA256.Create())
                {
                    hash = hashalg.ComputeHash(dataStream);
                }

            }

            ContentInfo contentInfo = new ContentInfo(hash);
            SignedCms signedCms = new SignedCms(contentInfo, true);
            signedCms.Decode(sign);
            signedCms.CheckSignature(false);
        }

        /// <summary>
        /// Подписывает блок данных на сертификате и формирует присоединенную подпись
        /// </summary>
        /// <param name="data">Данные</param>
        /// <param name="signerCert">Сертификат подписывающего с закрытым ключом</param>
        /// <returns>Данные присоединенной подписи</returns>
        public byte[] Sign(byte[] data, X509Certificate2 signerCert)
        {
            // Подписываем данные на закрытом ключе отправителя
            // Получаетель будет проверять подпись с помощью открытого ключа отправителя
           return Sign(data, signerCert, true);
        }

        /// <summary>
        /// Проверяет подпись
        /// </summary>
        /// <param name="data">Данные присоединенной подписи</param>
        /// <returns>Подписанные данные</returns>
        public byte[] Verify(byte[] data)
        {
            // Создаем SignedCms для декодирования и проверки.
            SignedCms signedCms = new SignedCms();

            signedCms.Decode(data);
            signedCms.CheckSignature(true);

            return signedCms.ContentInfo.Content;
        }

        public byte[] SignEncrypt(byte[] data, X509Certificate2 signerCert, X509Certificate2 recipientCert)
        {
            // Подписываем данные на закрытом ключе отправителя
            // Получаетель будет проверять подпись с помощью открытого ключа отправителя
            byte[] encodedSignedCms = Sign(data, signerCert, true);

            // Запечатываем (шифруем) подписанное сообщение на открытом ключе получателя
            byte[] encodedEnvelopedCms = Encrypt(encodedSignedCms, recipientCert);

            return encodedEnvelopedCms;
        }

        public byte[] VerifyDecrypt(byte[] encodedEnvelopedCms, X509Certificate2 signerCert, X509Certificate2 recipientCert)
        {
            // Распечатываем конверт (расшифровываем сообщение) на закрытом ключе получателя
            byte[] encodedSignedCms = Decrypt(encodedEnvelopedCms);

            // Создаем SignedCms для декодирования и проверки.
            SignedCms signedCms = new SignedCms();

            signedCms.Decode(encodedSignedCms);
            signedCms.CheckSignature(true);

            return signedCms.ContentInfo.Content;
        }


        #region STATIC METHODS

        protected static byte[] Encrypt(byte[] data, X509Certificate2 recipientCert)
        {
            // Помещаем сообщение в объект ContentInfo 
            // Это требуется для создания объекта EnvelopedCms.
            ContentInfo contentInfo = new ContentInfo(data);

            // Создаем объект EnvelopedCms, передавая ему
            // только что созданный объект ContentInfo.
            // Используем идентификацию получателя (SubjectIdentifierType)
            // по умолчанию (IssuerAndSerialNumber).
            // Не устанавливаем алгоритм зашифрования тела сообщения:
            // ContentEncryptionAlgorithm устанавливается в 
            // RSA_DES_EDE3_CBC, несмотря на это, при зашифровании
            // сообщения в адрес получателя с ГОСТ сертификатом,
            // будет использован алгоритм GOST 28147-89.
            EnvelopedCms envelopedCms = new EnvelopedCms(contentInfo);

            // Создаем объект CmsRecipient, который 
            // идентифицирует получателя зашифрованного сообщения.
            CmsRecipient recip = new CmsRecipient(SubjectIdentifierType.IssuerAndSerialNumber, recipientCert);

            // Зашифровываем сообщение.
            envelopedCms.Encrypt(recip);

            // Закодированное EnvelopedCms сообщение содержит
            // зашифрованный текст сообщения и информацию
            // о каждом получателе данного сообщения.
            return envelopedCms.Encode();
        }

        protected static byte[] Decrypt(byte[] encodedEnvelopedCms)
        {
            // Создаем объект для декодирования и расшифрования.
            EnvelopedCms envelopedCms = new EnvelopedCms();

            // Декодируем сообщение.
            envelopedCms.Decode(encodedEnvelopedCms);

            //TODO: Нужен код для выбора правильного получателя
            envelopedCms.Decrypt(envelopedCms.RecipientInfos[0]);

            // После вызова метода Decrypt в свойстве ContentInfo 
            // содержится расшифрованное сообщение.
            return envelopedCms.ContentInfo.Content;
        }

        //public static byte[] SignGost(byte[] data, X509Certificate2 cert, bool wholeChain)
        //{
        //    if (data == null)
        //        throw new ArgumentNullException("data");

        //    // Создание объекта для подписи сообщения
        //    var signedCms = new GostSignedCms(new ContentInfo(message));

        //    // Создание объект с информацией о подписчике
        //    var signer = new CmsSigner(certificate);

        //    // Включение информации только о конечном сертификате (только для теста)
        //    signer.IncludeOption = X509IncludeOption.EndCertOnly;

        //    // Создание подписи для сообщения CMS/PKCS#7
        //    signedCms.ComputeSignature(signer);

        //    // Создание сообщения CMS/PKCS#7
        //    return signedCms.Encode();
        //}

        public static byte[] Sign(byte[] data, X509Certificate2 cert, bool wholeChain)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            // setup the data to sign
            ContentInfo content = new ContentInfo(data);
            //SignedCms signedCms = new SignedCms(content, true);
            SignedCms signedCms = new SignedCms(content);
            CmsSigner signer = new CmsSigner(SubjectIdentifierType.IssuerAndSerialNumber, cert);
            if (wholeChain)
                signer.IncludeOption = X509IncludeOption.WholeChain;
            // create the signature
            signedCms.ComputeSignature(signer);
            return signedCms.Encode();
        }
        /// <summary>
        /// Проверяет подпись
        /// </summary>
        /// <param name="data">Данные</param>
        /// <param name="sign">Данные подписи</param>
        /// <param name="verifyCerts">Признак проверки сертификатов</param>
        public static void Verify(byte[] data, byte[] sign, bool verifyCerts)
        {
            if (data == null)
                throw new ArgumentNullException("data");
            if (data == null)
                throw new ArgumentNullException("sign");

            ContentInfo contentInfo = new ContentInfo(data);
            SignedCms signedCms = new SignedCms(contentInfo, true);
            signedCms.Decode(sign);
            signedCms.CheckSignature(verifyCerts);
        }

        //static byte[] Verify(byte[] data)
        //{
        //    // Создаем SignedCms для декодирования и проверки.
        //    SignedCms signedCms = new SignedCms();

        //    signedCms.Decode(data);
        //    signedCms.CheckSignature(true);

        //    return signedCms.ContentInfo.Content;
        //}

        #endregion // STATIC METHODS
    }

}
