﻿using System;
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
                throw new SOCryptographicException("У сертификата подписи нет закрытого ключа.");

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
                throw new SOCryptographicException("У сертификата подписи нет закрытого ключа.");

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

        public static void Encrypt(Stream dataStream, FileStream encryptedFileStream, X509Certificate2 recipientCert)
        {
            if (dataStream == null)
                throw new ArgumentNullException("dataStream");
            if (encryptedFileStream == null)
                throw new ArgumentNullException("encryptedFileStream");
            if (recipientCert == null)
                throw new ArgumentNullException("recipientCert");
            if (!encryptedFileStream.CanWrite)
                throw new ArgumentException("Поток для шифрованных данных не доступен для записи.");

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
                senderSessionKey.Padding = PaddingMode.PKCS7;
                // Отправитель шифрует сессионный ключ на открытом ключе и передает его получателю
                formatter = new RSAPKCS1KeyExchangeFormatter(publicKey);
                sessionKey = formatter.CreateKeyExchange(senderSessionKey.Key);
            }

            // Отправитель передает получателю вектор инициализации
            //SessionKey sk = new SessionKey(senderSessionKey.IV, sessionKey);

            // Отправитель шифрует данные с использованием сессионного ключа
            using (ICryptoTransform encryptor = senderSessionKey.CreateEncryptor())
            {
                /*
                 * Заголовок:
                 * Волшебное слово (2 байта) - 0xABBA
                 * Длинна синхропосылки (4 байта)
                 * Синхропосылка (senderSessionKey.IV.Length)
                 * Длинна шифрованного ключа (4 байта)
                 * Шифрованный сессионный ключ (senderSessionKey.Key.Length)
                 */

                var offset = sizeof(UInt16) +
                    sizeof(Int32) + senderSessionKey.IV.Length +
                    sizeof(Int32) + sessionKey.Length;

                // Резервируем место для заголовка
                encryptedFileStream.SetLength(offset);
                encryptedFileStream.Position = offset;

                // Создаем криптографический поток
                using (var encryptCryptoStream = new CryptoStream(encryptedFileStream, encryptor, CryptoStreamMode.Write))
                {
                    // Шируем
                    dataStream.CopyTo(encryptCryptoStream);
                    // Сохраняем позицию !!!
                    long oldpos = encryptedFileStream.Position;

                    // Дописываем в начало потока заголовок, данные ключа и синхропосылки
                    encryptedFileStream.Position = 0;

                    // "волшебное" слово
                    byte[] magic = BitConverter.GetBytes((UInt16)0xABBA);
                    encryptedFileStream.Write(magic, 0, magic.Length);
                    // длина сихропосылки
                    byte[] iv_len = BitConverter.GetBytes((Int32)senderSessionKey.IV.Length);
                    encryptedFileStream.Write(iv_len, 0, iv_len.Length);
                    // синхропосыка
                    encryptedFileStream.Write(senderSessionKey.IV, 0, senderSessionKey.IV.Length);
                    // длина шифрованного сессионного ключа
                    byte[] key_len = BitConverter.GetBytes((Int32)sessionKey.Length);
                    encryptedFileStream.Write(key_len, 0, key_len.Length);
                    // шифрованный сессионный ключ
                    encryptedFileStream.Write(sessionKey, 0, sessionKey.Length);

                    // Восстанавливаем позицию!!!
                    encryptedFileStream.Position = oldpos;

                    // Обновляем базовый источник данных
                    encryptCryptoStream.FlushFinalBlock();
                }
            }
        }

        public static void EncryptEx(Stream dataStream, FileStream encryptedFileStream, X509Certificate2 recipientCert, out byte[] IV, out byte[] CKey)
        {
            if (dataStream == null)
                throw new ArgumentNullException("dataStream");
            if (encryptedFileStream == null)
                throw new ArgumentNullException("encryptedFileStream");
            if (recipientCert == null)
                throw new ArgumentNullException("recipientCert");
            if (!encryptedFileStream.CanWrite)
                throw new ArgumentException("Поток для шифрованных данных не доступен для записи.");

            AsymmetricAlgorithm publicKey = recipientCert.GetPublicKeyAlgorithm();

            SymmetricAlgorithm senderSessionKey = null;
            AsymmetricKeyExchangeFormatter formatter = null;

            if (recipientCert.IsGost())
            {
                // формируем случайный сессионный ключ
                senderSessionKey = new Gost_28147_89_SymmetricAlgorithm(((GostAsymmetricAlgorithm)publicKey).ProviderType);

                // Отправитель шифрует сессионный ключ на открытом ключе и передает его получателю
                formatter = ((GostAsymmetricAlgorithm)publicKey).CreateKeyExchangeFormatter();
                CKey = ((GostKeyExchangeFormatter)formatter).CreateKeyExchangeData(senderSessionKey);
            }
            else
            {
                // формируем случайный сессионный ключ
                senderSessionKey = Aes.Create();
                senderSessionKey.Padding = PaddingMode.PKCS7;
                // Отправитель шифрует сессионный ключ на открытом ключе и передает его получателю
                formatter = new RSAPKCS1KeyExchangeFormatter(publicKey);
                CKey = formatter.CreateKeyExchange(senderSessionKey.Key);
            }

            IV = senderSessionKey.IV;

            // Отправитель шифрует данные с использованием сессионного ключа
            using (ICryptoTransform encryptor = senderSessionKey.CreateEncryptor())
            {
                // Создаем криптографический поток
                using (var encryptCryptoStream = new CryptoStream(encryptedFileStream, encryptor, CryptoStreamMode.Write))
                {
                    // Шируем
                    dataStream.CopyTo(encryptCryptoStream);
                    encryptCryptoStream.FlushFinalBlock();
                }
            }
        }

        public static void Decrypt(Stream encryptedDataStream, FileStream dataFileStream, X509Certificate2 recipientCert)
        {
            if (encryptedDataStream == null)
                throw new ArgumentNullException("encryptedDataStream");
            if (dataFileStream == null)
                throw new ArgumentNullException("dataFileStream");
            if (recipientCert == null)
                throw new ArgumentNullException("recipientCert");
            if (!dataFileStream.CanWrite)
                throw new ArgumentException("Поток для расшифрованных данных не доступен для записи.");

            /*
             * Заголовок:
             * Волшебное слово (2 байта) - 0xABBA
             * Длинна синхропосылки (4 байта)
             * Синхропосылка (senderSessionKey.IV.Length)
             * Длинна шифрованного ключа (4 байта)
             * Шифрованный сессионный ключ (senderSessionKey.Key.Length)
             */

            // Извлекаем из потока синхропосылку и зашифрованный сессионный ключ
            encryptedDataStream.Position = 0;

            // буффер для "волшебного" слова
            byte[] _magic = new byte[sizeof(UInt16)];
            // заполняем буфер из потока
            encryptedDataStream.Read(_magic, 0, sizeof(UInt16));
            // сравниваем
            if (BitConverter.ToUInt16(_magic, 0) != 0xABBA)
                throw new SOInvalidFormatException("Поток с шифрованными данными имеет неверный формат. Неверное значение 'магического' числа.");
            
            // буфер для длины синхропосылки или шифрованного сессионного ключа
            byte[] _len = new byte[sizeof(Int32)];
            // читаем из потока длину синхропосылки
            encryptedDataStream.Read(_len, 0, sizeof(Int32));

            int len = BitConverter.ToInt32(_len, 0);
            // буффер для синхропосылки
            byte[] _IV = new byte[len];
            // читаем из потока синхропосылку
            encryptedDataStream.Read(_IV, 0, len);

            // читаем из потока длину шифрованного сессионного ключа
            encryptedDataStream.Read(_len, 0, sizeof(Int32));
            len = BitConverter.ToInt32(_len, 0);

            // буффер для шифрованного сессионного ключа
            byte[] _KEY = new byte[len];
            // читаем из потока шифрованный сессионный ключ
            encryptedDataStream.Read(_KEY, 0, len);

            // Получаем закрытый ключ для расшифровки сессионного ключа
            AsymmetricAlgorithm privateKey = recipientCert.GetPrivateKeyAlgorithm();
            AsymmetricKeyExchangeDeformatter deformatter = null;
            SymmetricAlgorithm receiverSessionKey = null;

            if (recipientCert.IsGost())
            {
                deformatter = ((GostAsymmetricAlgorithm)privateKey).CreateKeyExchangeDeformatter();
                // Рассшифровываем сессионный ключ и устанавливаем его в качестве ключа симметричного алгоритма
                receiverSessionKey = ((GostKeyExchangeDeformatter)deformatter).DecryptKeyExchangeAlgorithm(_KEY);
            }
            else
            {
                deformatter = new RSAPKCS1KeyExchangeDeformatter(privateKey);
                receiverSessionKey = Aes.Create();
                receiverSessionKey.Padding = PaddingMode.PKCS7;
                // Рассшифровываем сессионный ключ и устанавливаем его в качестве ключа симметричного алгоритма
                receiverSessionKey.Key = deformatter.DecryptKeyExchange(_KEY);
            }

            // Устанавливаем вектор инициализации
            receiverSessionKey.IV = _IV;

            // Рассшифровываем данные с использованием сессионного ключа
            using (ICryptoTransform decryptor = receiverSessionKey.CreateDecryptor())
            {
                CryptoStream cryptoStream = new CryptoStream(encryptedDataStream, decryptor, CryptoStreamMode.Read);
                cryptoStream.CopyTo(dataFileStream);
            }
        }

        public static void DecryptEx(Stream encryptedDataStream, FileStream dataFileStream, X509Certificate2 recipientCert, byte[] IV, byte[] CKey)
        {
            if (encryptedDataStream == null)
                throw new ArgumentNullException("encryptedDataStream");
            if (dataFileStream == null)
                throw new ArgumentNullException("dataFileStream");
            if (recipientCert == null)
                throw new ArgumentNullException("recipientCert");
            if (!dataFileStream.CanWrite)
                throw new ArgumentException("Поток для расшифрованных данных не доступен для записи.");
            if (IV == null)
                throw new ArgumentNullException("IV");
            if (CKey == null)
                throw new ArgumentNullException("CKey");

            // Получаем закрытый ключ для расшифровки сессионного ключа
            AsymmetricAlgorithm privateKey = recipientCert.GetPrivateKeyAlgorithm();
            AsymmetricKeyExchangeDeformatter deformatter = null;
            SymmetricAlgorithm receiverSessionKey = null;

            if (recipientCert.IsGost())
            {
                deformatter = ((GostAsymmetricAlgorithm)privateKey).CreateKeyExchangeDeformatter();
                // Рассшифровываем сессионный ключ и устанавливаем его в качестве ключа симметричного алгоритма
                receiverSessionKey = ((GostKeyExchangeDeformatter)deformatter).DecryptKeyExchangeAlgorithm(CKey);
            }
            else
            {
                deformatter = new RSAPKCS1KeyExchangeDeformatter(privateKey);
                receiverSessionKey = Aes.Create();
                receiverSessionKey.Padding = PaddingMode.PKCS7;
                // Рассшифровываем сессионный ключ и устанавливаем его в качестве ключа симметричного алгоритма
                receiverSessionKey.Key = deformatter.DecryptKeyExchange(CKey);
            }

            // Устанавливаем вектор инициализации
            receiverSessionKey.IV = IV;

            // Рассшифровываем данные с использованием сессионного ключа
            using (ICryptoTransform decryptor = receiverSessionKey.CreateDecryptor())
            {
                CryptoStream cryptoStream = new CryptoStream(encryptedDataStream, decryptor, CryptoStreamMode.Read);
                cryptoStream.CopyTo(dataFileStream);
            }
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
                X509Certificate2 cert = CertificateWrapper.FindCertificateBySubjectIdentifier(StoreLocation.CurrentUser, ri.RecipientIdentifier);
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
                X509Certificate2 cert = CertificateWrapper.FindCertificateBySubjectIdentifier(StoreLocation.CurrentUser, ri.RecipientIdentifier);
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
    }
}
