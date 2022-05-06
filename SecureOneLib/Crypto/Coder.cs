using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.Xml;
using System.IO;
using GostCryptography.Base;
using GostCryptography.Gost_28147_89;

namespace SecureOneLib.Crypto
{
    [SecurityCritical]
    public static class Coder
    {
        public static byte[] SignDetached(Stream dataStream, X509Certificate2 signerCert)
        {
            if (dataStream == null)
                throw new ArgumentNullException("dataStream");
            if (signerCert == null)
                throw new ArgumentNullException("signerCert");

            if (!signerCert.HasPrivateKey)
                throw new SOCryptographicException("Certificate for signing has not a private key.");

            byte[] hash;
            AsymmetricAlgorithm privateKey = signerCert.GetPrivateKeyAlgorithm();

            if (signerCert.IsGost())
            {
                using (var hashAlg = (((GostAsymmetricAlgorithm)privateKey).CreateHashAlgorithm()))
                {
                    hash = hashAlg.ComputeHash(dataStream);
                }
            }
            else
            {
                using (SHA256 hashalg = SHA256.Create())
                {
                    hash = hashalg.ComputeHash(dataStream);
                }
            }

            //ContentInfo content = new ContentInfo(new Oid("1.2.840.113549.1.7.5"),hash);
            ContentInfo content = new ContentInfo(hash);

            SignedCms signedCms = new SignedCms(content, true);
            CmsSigner signer = new CmsSigner(SubjectIdentifierType.IssuerAndSerialNumber, signerCert)
            {
                IncludeOption = X509IncludeOption.WholeChain
            };

            signedCms.ComputeSignature(signer);
            return signedCms.Encode();
        }

        public static byte[] SignAttached(byte[] data, X509Certificate2 signerCert)
        {
            if (data == null)
                throw new ArgumentNullException("data");
            if (signerCert == null)
                throw new ArgumentNullException("signerCert");

            if (!signerCert.HasPrivateKey)
                throw new SOCryptographicException("Certificate for signing has not a private key.");

            ContentInfo content = new ContentInfo(data);
            SignedCms signedCms = new SignedCms(content, false);
            CmsSigner signer = new CmsSigner(SubjectIdentifierType.IssuerAndSerialNumber, signerCert)
            {
                IncludeOption = X509IncludeOption.WholeChain
            };

            signedCms.ComputeSignature(signer);
            return signedCms.Encode();
        }

        /// <summary>
        /// Проверяет присоединенную подпись
        /// </summary>
        /// <param name="sign"></param>
        /// <param name="data"></param>
        public static void Verify(byte[] signedCmsData, bool verifySignatureOnly = false)
        {
            if (signedCmsData == null)
                throw new ArgumentNullException("signedCmsData");

            // Create a new, nondetached SignedCms message.
            SignedCms signedCms = new SignedCms();

            // encodedMessage is the encoded message received from
            // the sender.
            signedCms.Decode(signedCmsData);

            // Verify the signature without validating the
            // certificate.
            signedCms.CheckSignature(verifySignatureOnly);
        }

        /// <summary>
        /// Проверяет отсоединенную подпись
        /// </summary>
        /// <param name="sign"></param>
        /// <param name="data"></param>
        public static void Verify(byte[] signedCmsData, byte[] data, bool verifySignatureOnly = false)
        {
            if (signedCmsData == null)
                throw new ArgumentNullException("signedCmsData");
            if (data == null)
                throw new ArgumentNullException("data");

            // Create a ContentInfo object from the inner content obtained
            // independently from encodedMessage.
            ContentInfo contentInfo = new ContentInfo(data);

            // Create a new, detached SignedCms message.
            SignedCms signedCms = new SignedCms(contentInfo, true);

            // Декадируем данные подписи
            signedCms.Decode(signedCmsData);

            // Проверяем только подпись или подпись вместе со всей цепочкой сертификатов
            signedCms.CheckSignature(verifySignatureOnly);
        }

        /// <summary>
        /// Проверяет отсоединенную подпись
        /// </summary>
        /// <param name="sign"></param>
        /// <param name="data"></param>
        public static void Verify(byte[] signedCmsData, FileStream dataStream, bool verifySignatureOnly = false)
        {
            if (signedCmsData == null)
                throw new ArgumentNullException("signedCms");
            if (dataStream == null)
                throw new ArgumentNullException("dataStream");

            ContentInfo contentInfo = null;

            // Вслучае отсоединенной подписи сначала нужно понять, какой сертификат использовался для формирования подписи, 
            // чтобы в свою очередь расчитать хэш на соответствующем, правильном алгоритме
            {
                SignedCms cms = new SignedCms();
                cms.Decode(signedCmsData);

                if (cms.SignerInfos.Count == 0)
                    throw new SOInvalidFormatException("Подпись не содержит информации о подписантах.");

                if (cms.SignerInfos.Count > 1)
                    throw new SOInvalidFormatException("Подпись содержит более одного подписаната.");

                byte[] hash;
                SignerInfo si = cms.SignerInfos[0];
                // Т.к. сертификат извлекается из подписи, используем открытый ключ для получения алгоритма
                AsymmetricAlgorithm privateKey = si.Certificate.GetPublicKeyAlgorithm();

                if (si.Certificate.IsGost())
                {
                    using (var hashAlg = (((GostAsymmetricAlgorithm)privateKey).CreateHashAlgorithm()))
                    {
                        hash = hashAlg.ComputeHash(dataStream);
                    }
                }
                else
                {
                    using (SHA256 hashalg = SHA256.Create())
                    {
                        hash = hashalg.ComputeHash(dataStream);
                    }
                }

                //ContentInfo content = new ContentInfo(new Oid("1.2.840.113549.1.7.5"),hash);
                contentInfo = new ContentInfo(hash);
            }

            // Создаем целевой объект для отсоединенной подписи
            SignedCms signedCms = new SignedCms(contentInfo, true);

            // Декодируем
            signedCms.Decode(signedCmsData);

            // Проверяем только подпись или подпись вместе со всей цепочкой сертификатов
            signedCms.CheckSignature(verifySignatureOnly);
        }

        public static Stream Encrypt(Stream dataStream, X509Certificate2 recipientCert)
        {
            if (dataStream == null)
                throw new ArgumentNullException("dataStream");
            if (recipientCert == null)
                throw new ArgumentNullException("recipientCert");

            MemoryStream encryptedDataStream = new MemoryStream();
            AsymmetricAlgorithm publicKey = recipientCert.GetPublicKeyAlgorithm();

            SymmetricAlgorithm senderSessionKey = null;
            AsymmetricKeyExchangeFormatter formatter = null;

            byte[] sessionKey;

            if (recipientCert.IsGost())
            {
                // формируем случайный сессионный ключ
                senderSessionKey = new Gost_28147_89_SymmetricAlgorithm(((GostAsymmetricAlgorithm)publicKey).ProviderType);

                // Отправитель шифрует сессионный ключ на открытом ключе и передает его получателю
                formatter = ((GostAsymmetricAlgorithm)publicKey).CreateKeyExchangeFormatter();
                sessionKey = ((GostKeyExchangeFormatter)formatter).CreateKeyExchangeData(senderSessionKey);
            }
            else
            {
                // формируем случайный сессионный ключ
                senderSessionKey = Aes.Create();
                // Отправитель шифрует сессионный ключ на открытом ключе и передает его получателю
                formatter = new RSAPKCS1KeyExchangeFormatter(publicKey);
                sessionKey = formatter.CreateKeyExchange(senderSessionKey.Key);
            }

            // Отправитель передает получателю вектор инициализации
            SessionKey sk = new SessionKey(senderSessionKey.IV, sessionKey);

            // Отправитель шифрует данные с использованием сессионного ключа
            using (ICryptoTransform encryptor = senderSessionKey.CreateEncryptor())
            {
                CryptoStream cryptoStream = new CryptoStream(encryptedDataStream, encryptor, CryptoStreamMode.Write);

                dataStream.CopyTo(cryptoStream);
                cryptoStream.FlushFinalBlock();
            }

            encryptedDataStream.Position = 0;
            return sk.CopyTo(encryptedDataStream);
        }

        public static Stream Decrypt(Stream encryptedDataStream, X509Certificate2 recipientCert)
        {
            if (encryptedDataStream == null)
                throw new ArgumentNullException("encryptedDataStream");
            if (recipientCert == null)
                throw new ArgumentNullException("recipientCert");

            // Извлекаем из потока синхропосылку и зашифрованный сессионный ключ
            SessionKey sk = new SessionKey(encryptedDataStream);
            // устанавливаем "указатель" потока на начало шифрованных данных
            encryptedDataStream = sk.StreamTail;

            // создаем выходной поток
            MemoryStream decryptedDataStream = new MemoryStream();

            // Получаем закрытый ключ для расшифровки сессионного ключа
            AsymmetricAlgorithm privateKey = recipientCert.GetPrivateKeyAlgorithm();
            AsymmetricKeyExchangeDeformatter deformatter = null;
            SymmetricAlgorithm receiverSessionKey = null;

            if (recipientCert.IsGost())
            {
                deformatter = ((GostAsymmetricAlgorithm)privateKey).CreateKeyExchangeDeformatter();
                // Рассшифровываем сессионный ключ и устанавливаем его в качестве ключа симметричного алгоритма
                receiverSessionKey = ((GostKeyExchangeDeformatter)deformatter).DecryptKeyExchangeAlgorithm(sk.Key);
            }
            else
            {
                deformatter = new RSAPKCS1KeyExchangeDeformatter(privateKey);
                receiverSessionKey = Aes.Create();
                // Рассшифровываем сессионный ключ и устанавливаем его в качестве ключа симметричного алгоритма
                receiverSessionKey.Key = deformatter.DecryptKeyExchange(sk.Key);
            }

            // Устанавливаем вектор инициализации
            receiverSessionKey.IV = sk.IV;

            // Рассшифровываем данные с использованием сессионного ключа
            using (ICryptoTransform decryptor = receiverSessionKey.CreateDecryptor())
            {
                CryptoStream cryptoStream = new CryptoStream(encryptedDataStream, decryptor, CryptoStreamMode.Read);
                cryptoStream.CopyTo(decryptedDataStream);
            }

            decryptedDataStream.Position = 0;
            return decryptedDataStream;
        }
    
        /// <summary>
        /// Шифрует даннные по стандарту PKCS7 / CMS на открытом ключе получателя
        /// Объем данных должен быть ограничен  (менее ~50 Мб в зависимости от кол-ва оперативной памяти)
        /// </summary>
        /// <param name="data"></param>
        /// <param name="recipientCert"></param>
        /// <returns></returns>
        public static byte[] Encrypt(byte[] data, X509Certificate2 recipientCert)
        {
            if (data == null)
                throw new ArgumentNullException("data");

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

        public static byte[] SignEncrypt(byte[] data, X509Certificate2 recipientCert, X509Certificate2 signerCert)
        {
            if (data == null)
                throw new ArgumentNullException("data");
            if (recipientCert == null)
                throw new ArgumentNullException("recipientCert");
            if (signerCert == null)
                throw new ArgumentNullException("signerCert");

            ContentInfo contentInfo = new ContentInfo(data);

            EnvelopedCms envelopedCms = new EnvelopedCms(contentInfo);
            CmsRecipient recip = new CmsRecipient(SubjectIdentifierType.IssuerAndSerialNumber, recipientCert);
            envelopedCms.Encrypt(recip);

            SignedCms signedCms = new SignedCms(envelopedCms.ContentInfo, false);
            CmsSigner signer = new CmsSigner(SubjectIdentifierType.IssuerAndSerialNumber, signerCert);
            signer.IncludeOption = X509IncludeOption.WholeChain;
            signedCms.ComputeSignature(signer);
            return signedCms.Encode();
        }

        public static byte[] VerifyDecrypt(byte[] encodedSignedEnvelopedCmsData)
        {
            if (encodedSignedEnvelopedCmsData == null)
                throw new ArgumentNullException("encodedSignedEnvelopedCmsData");

            SignedCms signedCms = new SignedCms();
            signedCms.Decode(encodedSignedEnvelopedCmsData);
            signedCms.CheckSignature(false);

            // Создаем объект для декодирования и расшифрования.
            EnvelopedCms envelopedCms = new EnvelopedCms(signedCms.ContentInfo);

            foreach (RecipientInfo ri in envelopedCms.RecipientInfos)
            {
                X509Certificate2 cert = LoadCertificate2(StoreLocation.CurrentUser, ri.RecipientIdentifier);
                if (cert.HasPrivateKey)
                {
                    envelopedCms.Decrypt(ri);
                    break;
                }
            }

            // После вызова метода Decrypt в свойстве ContentInfo 
            // содержится расшифрованное сообщение.
            return envelopedCms.ContentInfo.Content;
        }

        /// <summary>
        /// Расшифровывает данные по стандарту PKCS7 / CMS производя поиск сертификата в хралище
        /// </summary>
        /// <param name="encodedEnvelopedCms"></param>
        /// <returns></returns>
        public static byte[] Decrypt(byte[] encodedEnvelopedCmsData)
        {
            if (encodedEnvelopedCmsData == null)
                throw new ArgumentNullException("encodedEnvelopedCmsData");

            // Создаем объект для декодирования и расшифрования.
            EnvelopedCms envelopedCms = new EnvelopedCms();

            // Декодируем сообщение.
            envelopedCms.Decode(encodedEnvelopedCmsData);

            foreach (RecipientInfo ri in envelopedCms.RecipientInfos)
            {
                X509Certificate2 cert = LoadCertificate2(StoreLocation.CurrentUser, ri.RecipientIdentifier);
                if (cert.HasPrivateKey)
                {
                    envelopedCms.Decrypt(ri);
                    break;
                }
            }

            // После вызова метода Decrypt в свойстве ContentInfo 
            // содержится расшифрованное сообщение.
            return envelopedCms.ContentInfo.Content;
        }

        public static X509Certificate2 LoadCertificate2(StoreLocation storeLocation, SubjectIdentifier subjIdentifier)
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
}
