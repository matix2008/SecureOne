using System.IO;
using System.Text;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SecureOneLib.Crypto;

namespace UnitTests
{
    [TestClass]
    public class CryproTests
    {
        #region Signatures
        /// <summary>
        /// Тестирует создание и проверку присоединенной подписи (не ГОСТ)
        /// </summary>
        [TestMethod]
        public void SignAttachedVerifyBytes_nogost()
        {
            SignAttachedVerifyBytes(TestConfig.NoGostPKCert, "Some data to sign");
        }
        /// <summary>
        /// Тестирует создание и проверку присоединенной подписи (ГОСТ)
        /// </summary>
        [TestMethod]
        public void SignAttachedVerifyBytes_gost()
        {
            SignAttachedVerifyBytes(TestConfig.GostPKCert, "Some data to sign");
        }
        /// <summary>
        /// Тестирует создание и проверку отсоединенной подписи для небольшого файла (не ГОСТ)
        /// </summary>
        [TestMethod]
        public void SignDetachedVerifyStream_nogostsmall()
        {
            SignDetachedVerifyStream(TestConfig.NoGostPKCert, TestConfig.SmallFile);
        }
        /// <summary>
        /// Тестирует создание и проверку отсоединенной подписи для небольшого файла (ГОСТ)
        /// </summary>
        [TestMethod]
        public void SignDetachedVerifyStream_gostsmall()
        {
            SignDetachedVerifyStream(TestConfig.GostPKCert, TestConfig.SmallFile);
        }
        /// <summary>
        /// Тестирует создание и проверку отсоединенной подписи для большого файла (не ГОСТ)
        /// </summary>
        [TestMethod]
        public void SignDetachedVerifyStream_nogostbig()
        {
            SignDetachedVerifyStream(TestConfig.NoGostPKCert, TestConfig.BigFile);
        }
        /// <summary>
        /// Тестирует создание и проверку отсоединенной подписи для большого файла (ГОСТ)
        /// </summary>
        [TestMethod]
        public void SignDetachedVerifyStream_gostbig()
        {
            SignDetachedVerifyStream(TestConfig.NoGostPKCert, TestConfig.BigFile);
        }

        #endregion

        #region WithoutIntermadiateKeysStoring
        /// <summary>
        /// Тестирует потоковое шифрование и расшифрову без промежуточного хранения ключей для большого файла (не ГОСТ)
        /// </summary>
        [TestMethod]
        public void EncryptDecryptStreams_WithoutIntermadiateKeysStoring_ValidCerts_nogostbig()
        {
            EncryptDecryptStreams_WithoutIntermadiateKeysStoring(TestConfig.NoGostPKCert, TestConfig.BigFile);
        }
        /// <summary>
        /// Тестирует потоковое шифрование и расшифрову без промежуточного хранения ключей для небольшого файла (не ГОСТ)
        /// </summary>
        [TestMethod]
        public void EncryptDecryptStreams_WithoutIntermadiateKeysStoring_ValidCerts_nogostsmall()
        {
            EncryptDecryptStreams_WithoutIntermadiateKeysStoring(TestConfig.NoGostPKCert, TestConfig.SmallFile);
        }
        /// <summary>
        /// Тестирует потоковое шифрование и расшифрову без промежуточного хранения ключей для большого файла (ГОСТ)
        /// </summary>
        [TestMethod]
        public void EncryptDecryptStreams_WithoutIntermadiateKeysStoring_ValidCerts_gostbig()
        {
            EncryptDecryptStreams_WithoutIntermadiateKeysStoring(TestConfig.GostPKCert, TestConfig.BigFile);
        }
        /// <summary>
        /// Тестирует потоковое шифрование и расшифрову без промежуточного хранения ключей для небольшого файла (ГОСТ)
        /// </summary>
        [TestMethod]
        public void EncryptDecryptStreams_WithoutIntermadiateKeysStoring_ValidCerts_gostsmall()
        {
            EncryptDecryptStreams_WithoutIntermadiateKeysStoring(TestConfig.GostPKCert, TestConfig.SmallFile);
        }

        #endregion

        #region EncryptDecryptStreams
        /// <summary>
        /// Тестирует потоковое шифрование и расшифрову для небольшого файла (ГОСТ)
        /// </summary>
        [TestMethod]
        public void EncryptDecryptStreams_ValidCerts_gostsmall()
        {
            EncryptDecryptStreams(TestConfig.GostPKCert, TestConfig.SmallFile);
        }
        /// <summary>
        /// Тестирует потоковое шифрование и расшифрову для большого файла (ГОСТ)
        /// </summary>
        [TestMethod]
        public void EncryptDecryptStreams_ValidCerts_gostbig()
        {
            EncryptDecryptStreams(TestConfig.GostPKCert, TestConfig.BigFile);
        }
        /// <summary>
        /// Тестирует потоковое шифрование и расшифрову для небольшого файла (не ГОСТ)
        /// </summary>
        [TestMethod]
        public void EncryptDecryptStreams_ValidCerts_nogostsmall()
        {
            EncryptDecryptStreams(TestConfig.NoGostPKCert, TestConfig.SmallFile);
        }
        /// <summary>
        /// Тестирует потоковое шифрование и расшифрову для большого файла (не ГОСТ)
        /// </summary>
        [TestMethod]
        public void EncryptDecryptStreams_ValidCerts_nogostbig()
        {
            EncryptDecryptStreams(TestConfig.NoGostPKCert, TestConfig.BigFile);
        }
        /// <summary>
        /// Тестирует потоковое шифрование и расшифрову для небольшого файла (не ГОСТ) с неверным сертификатом при расшифровке
        /// </summary>
        [TestMethod]
        public void EncryptDecryptStreams_InvalidCerts_nogostsmall()
        {
            EncryptDecryptStreams_InvalidCerts(TestConfig.NoGostPKCert, TestConfig.NoGostPKCert2, TestConfig.SmallFile);
        }

        #endregion

        #region EncryptDecryptBytes
        /// <summary>
        /// Тестирует шифрование и расшивроку по стандарту CMS / PKCS7 (ГОСТ)
        /// </summary>
        [TestMethod]
        public void EncryptDecryptBytes_ValidCerts_gost()
        {
            EncryptDecryptBytes(TestConfig.GostPKCert, "Some string to encrypt");
        }
        /// <summary>
        /// Тестирует шифрование и расшивроку по стандарту CMS / PKCS7 (не ГОСТ)
        /// </summary>
        [TestMethod]
        public void EncryptDecryptBytes_ValidCerts_nogost()
        {
            EncryptDecryptBytes(TestConfig.NoGostPKCert, "Some string to encrypt");
        }

        #endregion

        #region EncryptSignDecryptBytes
        /// <summary>
        /// Тестирует создание подписи, шифрование, проверку подписи, расшивроку по стандарту CMS / PKCS7 (ГОСТ)
        /// </summary>
        [TestMethod]
        public void EncryptSignDecryptBytes_ValidOneCerts_gost()
        {
            EncryptSignDecryptBytes(TestConfig.GostPKCert, TestConfig.GostPKCert, "Some string to encrypt with 123456");
        }
        /// <summary>
        /// Тестирует создание подписи, шифрование, проверку подписи, расшивроку по стандарту CMS / PKCS7 (не ГОСТ)
        /// </summary>
        [TestMethod]
        public void EncryptSignDecryptBytes_ValidOneCerts_nogost()
        {
            EncryptSignDecryptBytes(TestConfig.NoGostPKCert, TestConfig.NoGostPKCert, "Some string to encrypt with 123456");
        }

        #endregion

        #region Служебные методы
        /// <summary>
        /// Тестирует потоковое шифрование и расшифрову без промежуточного хранения ключей
        /// </summary>
        /// <param name="cert"></param>
        /// <param name="input"></param>
        private void EncryptDecryptStreams_WithoutIntermadiateKeysStoring(X509Certificate2 cert, Stream input)
        {
            input.Position = 0;

            {
                byte[] IV = null;
                byte[] CKey = null;

                string enctempfile = TestConfig.GetTempFileName();
                using (FileStream ofs1 = File.Create(enctempfile))
                {
                    Coder.EncryptEx(input, ofs1, cert, out IV, out CKey);
                }

                string dectempfile = TestConfig.GetTempFileName();
                using (FileStream ifs = File.OpenRead(enctempfile))
                {
                    using (FileStream ofs2 = File.Create(dectempfile))
                    {
                        Coder.DecryptEx(ifs, ofs2, cert, IV, CKey);
                    }
                }

                using (Stream decrypted = File.OpenRead(dectempfile))
                    Assert.IsTrue(TestConfig.CompareStreams(input, decrypted));
            }
        }
        /// <summary>
        /// Тестирует потоковое шифрование с одним сертификатом для шифрования и расшифровывания
        /// </summary>
        /// <param name="cert"></param>
        /// <param name="input"></param>
        public void EncryptDecryptStreams(X509Certificate2 cert, Stream input)
        {
            input.Position = 0;

            {
                string enctempfile = TestConfig.GetTempFileName();
                using (FileStream ofs1 = File.Create(enctempfile))
                {
                    Coder.Encrypt(input, ofs1, cert);
                }

                string dectempfile = TestConfig.GetTempFileName();
                using (FileStream ifs = File.OpenRead(enctempfile))
                {
                    using (FileStream ofs2 = File.Create(dectempfile))
                    {
                        Coder.Decrypt(ifs, ofs2, cert);
                    }
                }

                using (Stream decrypted = File.OpenRead(dectempfile))
                    Assert.IsTrue(TestConfig.CompareStreams(input, decrypted));
            }
        }
        /// <summary>
        /// Тестирует потоковое шифрование с сертификатом для шифрования и другим сертификатом для расшифровывания
        /// </summary>
        /// <param name="validcert"></param>
        /// <param name="invalidcert"></param>
        /// <param name="input"></param>
        public void EncryptDecryptStreams_InvalidCerts(X509Certificate2 validcert, X509Certificate2 invalidcert, Stream input)
        {
            input.Position = 0;

            {
                string enctempfile = TestConfig.GetTempFileName();
                using (FileStream ofs1 = File.Create(enctempfile))
                {
                    Coder.Encrypt(input, ofs1, validcert);
                }

                using (FileStream ifs = File.OpenRead(enctempfile))
                {
                    try
                    {
                        string dectempfile = TestConfig.GetTempFileName();
                        using (FileStream ofs2 = File.Create(dectempfile))
                        {
                            Coder.Decrypt(ifs, ofs2, invalidcert);
                        }
                    }
                    catch (CryptographicException)
                    {
                        // Если мы тут, значит расшифровка сессионного ключа не прошла
                        // на неверном сертификате
                        return;
                    }
                }

                // При расшифровке не возникло проблем
                Assert.IsFalse(true);
            }
        }
        /// <summary>
        /// Тестирует шифрование и расшифровку по стандарту CMS / PKCS#7 блока даннных в виде строки
        /// </summary>
        /// <param name="cert"></param>
        /// <param name="str"></param>
        public void EncryptDecryptBytes(X509Certificate2 cert, string str)
        {
            byte[] input = Encoding.ASCII.GetBytes(str);

            byte[] encrypted = Coder.Encrypt(input, cert);
            byte[] output = Coder.Decrypt(encrypted);

            Assert.IsTrue(output.SequenceEqual(input));
        }
        /// <summary>
        /// Тестирует создание подписи, шифрование, проверку подписи и расшифровку по стандарту CMS / PKCS#7 блока даннных в виде строки
        /// </summary>
        /// <param name="cert1"></param>
        /// <param name="cert2"></param>
        /// <param name="str"></param>
        public void EncryptSignDecryptBytes(X509Certificate2 cert1, X509Certificate2 cert2, string str)
        {
            byte[] input = Encoding.ASCII.GetBytes(str);

            byte[] encrypted = Coder.SignEncrypt(input, cert1, cert2);
            byte[] output = Coder.VerifyDecrypt(encrypted);

            Assert.IsTrue(output.SequenceEqual(input));
        }
        /// <summary>
        /// Тестирует формирование присоединенной подписи по стандарту CMS / PKCS7 и ее проверку
        /// </summary>
        /// <param name="cert"></param>
        /// <param name="str"></param>
        public void SignAttachedVerifyBytes(X509Certificate2 cert, string str)
        {
            byte[] data = Encoding.ASCII.GetBytes(str);
            byte[] sign = Coder.SignAttached(data, cert);
            Coder.Verify(sign, true);
        }
        /// <summary>
        /// Тестирует формирование отсоединенной подписи по стандарту CMS / PKCS7 и ее проверку
        /// </summary>
        /// <param name="cert"></param>
        /// <param name="input"></param>
        public void SignDetachedVerifyStream(X509Certificate2 cert, FileStream input)
        {
            input.Position = 0;
            byte[] sign = Coder.SignDetached(input, cert);
            input.Position = 0;
            Coder.Verify(sign, input, true);
        }
        #endregion
    }
}
