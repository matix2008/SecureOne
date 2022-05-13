using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SecureOneLib.Crypto;
using SecureOneLib.Utilities;

namespace UnitTests
{
    [TestClass]
    public class CryproTests
    {
        #region Signs

        [TestMethod]
        public void SignAttachedVerifyBytes_nogost()
        {
            SignAttachedVerifyBytes(TestConfig.NoGostPKCert, "Some data to sign");
        }

        [TestMethod]
        public void SignAttachedVerifyBytes_gost()
        {
            SignAttachedVerifyBytes(TestConfig.GostPKCert, "Some data to sign");
        }

        [TestMethod]
        public void SignDetachedVerifyStream_nogostsmall()
        {
            SignDetachedVerifyStream(TestConfig.NoGostPKCert, TestConfig.SmallFile);
        }

        [TestMethod]
        public void SignDetachedVerifyStream_gostsmall()
        {
            SignDetachedVerifyStream(TestConfig.GostPKCert, TestConfig.SmallFile);
        }

        [TestMethod]
        public void SignDetachedVerifyStream_nogostbig()
        {
            SignDetachedVerifyStream(TestConfig.NoGostPKCert, TestConfig.BigFile);
        }

        [TestMethod]
        public void SignDetachedVerifyStream_gostbig()
        {
            SignDetachedVerifyStream(TestConfig.NoGostPKCert, TestConfig.BigFile);
        }

        #endregion

        #region WithoutIntermadiateKeysStoring

        [TestMethod]
        public void EncryptDecryptStreams_WithoutIntermadiateKeysStoring_ValidCerts_nogostbig()
        {
            EncryptDecryptStreams_WithoutIntermadiateKeysStoring(TestConfig.NoGostPKCert, TestConfig.BigFile);
        }

        [TestMethod]
        public void EncryptDecryptStreams_WithoutIntermadiateKeysStoring_ValidCerts_nogostsmall()
        {
            EncryptDecryptStreams_WithoutIntermadiateKeysStoring(TestConfig.NoGostPKCert, TestConfig.SmallFile);
        }

        [TestMethod]
        public void EncryptDecryptStreams_WithoutIntermadiateKeysStoring_ValidCerts_gostbig()
        {
            EncryptDecryptStreams_WithoutIntermadiateKeysStoring(TestConfig.GostPKCert, TestConfig.BigFile);
        }

        [TestMethod]
        public void EncryptDecryptStreams_WithoutIntermadiateKeysStoring_ValidCerts_gostsmall()
        {
            EncryptDecryptStreams_WithoutIntermadiateKeysStoring(TestConfig.GostPKCert, TestConfig.SmallFile);
        }

        #endregion

        #region EncryptDecryptStreams

        [TestMethod]
        public void EncryptDecryptStreams_ValidCerts_gostsmall()
        {
            EncryptDecryptStreams(TestConfig.GostPKCert, TestConfig.SmallFile);
        }

        [TestMethod]
        public void EncryptDecryptStreams_ValidCerts_gostbig()
        {
            EncryptDecryptStreams(TestConfig.GostPKCert, TestConfig.BigFile);
        }

        [TestMethod]
        public void EncryptDecryptStreams_ValidCerts_nogostsmall()
        {
            EncryptDecryptStreams(TestConfig.NoGostPKCert, TestConfig.SmallFile);
        }

        [TestMethod]
        public void EncryptDecryptStreams_ValidCerts_nogostbig()
        {
            EncryptDecryptStreams(TestConfig.NoGostPKCert, TestConfig.BigFile);
        }

        [TestMethod]
        public void EncryptDecryptStreams_InvalidCerts_nogostsmall()
        {
            EncryptDecryptStreams_InvalidCerts(TestConfig.NoGostPKCert, TestConfig.NoGostPKCert2, TestConfig.SmallFile);
        }

        #endregion

        #region EncryptDecryptBytes

        [TestMethod]
        public void EncryptDecryptBytes_ValidCerts_gost()
        {
            EncryptDecryptBytes(TestConfig.GostPKCert, "Some string to encrypt");
        }

        [TestMethod]
        public void EncryptDecryptBytes_ValidCerts_nogost()
        {
            EncryptDecryptBytes(TestConfig.NoGostPKCert, "Some string to encrypt");
        }

        #endregion

        #region EncryptSignDecryptBytes

        [TestMethod]
        public void EncryptSignDecryptBytes_ValidOneCerts_gost()
        {
            EncryptSignDecryptBytes(TestConfig.GostPKCert, TestConfig.GostPKCert, "Some string to encrypt with 123456");
        }

        [TestMethod]
        public void EncryptSignDecryptBytes_ValidOneCerts_nogost()
        {
            EncryptSignDecryptBytes(TestConfig.NoGostPKCert, TestConfig.NoGostPKCert, "Some string to encrypt with 123456");
        }

        #endregion

        #region Служебные методы
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

        public void EncryptDecryptBytes(X509Certificate2 cert, string str)
        {
            byte[] input = Encoding.ASCII.GetBytes(str);

            byte[] encrypted = Coder.Encrypt(input, cert);
            byte[] output = Coder.Decrypt(encrypted);

            Assert.IsTrue(output.SequenceEqual(input));
        }

        public void EncryptSignDecryptBytes(X509Certificate2 cert1, X509Certificate2 cert2, string str)
        {
            byte[] input = Encoding.ASCII.GetBytes(str);

            byte[] encrypted = Coder.SignEncrypt(input, cert1, cert2);
            byte[] output = Coder.VerifyDecrypt(encrypted);

            Assert.IsTrue(output.SequenceEqual(input));
        }

        public void SignAttachedVerifyBytes(X509Certificate2 cert, string str)
        {
            byte[] data = Encoding.ASCII.GetBytes(str);
            byte[] sign = Coder.SignAttached(data, cert);
            Coder.Verify(sign, true);
        }

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
