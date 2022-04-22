using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Pkcs;
using System.IO;
using GostCryptography.Base;
using GostCryptography.Gost_28147_89;


namespace SecureOneLib.Crypto
{
    public static class Coder
    {
        public static Stream Encrypt(Stream dataStream, X509Certificate2 recipientCert)
        {
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

        /// <summary>
        /// Расшифровывает данные по стандарту PKCS7 / CMS производя поиск сертификата в хралище
        /// </summary>
        /// <param name="encodedEnvelopedCms"></param>
        /// <returns></returns>
        public static byte[] Decrypt(byte[] encodedEnvelopedCms)
        {
            // Создаем объект для декодирования и расшифрования.
            EnvelopedCms envelopedCms = new EnvelopedCms();

            // Декодируем сообщение.
            envelopedCms.Decode(encodedEnvelopedCms);

            //foreach(var ri in envelopedCms.RecipientInfos)
            //{
            //    SubjectIdentifier si = ri.RecipientIdentifier;
            //}

            //TODO: Нужен код для выбора правильного получателя
            //envelopedCms.Decrypt(envelopedCms.RecipientInfos[0]);
            envelopedCms.Decrypt();

            // После вызова метода Decrypt в свойстве ContentInfo 
            // содержится расшифрованное сообщение.
            return envelopedCms.ContentInfo.Content;
        }
    }
}
