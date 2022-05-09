using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SecureOneLib.Crypto;
using SecureOneLib.Utilities;

namespace UnitTests
{
    [TestClass]
    public class CryproTests
    {
        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void EncryptDecryptStreams_ValidCerts_gostsmall()
        {
            var cert = TestConfig.GostPKCert;

            Stream input = TestConfig.SmallFile;
            input.Position = 0;

            {
                Stream encrypted = Coder.Encrypt(input, cert);

                string enctempfile = TestConfig.GetTempFileName();
                using (FileStream ofs1 = File.Create(enctempfile))
                {
                    encrypted.CopyTo(ofs1);
                }

                Stream decrypted = null;
                using (FileStream ifs = File.OpenRead(enctempfile))
                {
                    decrypted = Coder.Decrypt(ifs, cert);

                    string dectempfile = TestConfig.GetTempFileName();
                    using (FileStream ofs2 = File.Create(dectempfile))
                    {
                        decrypted.CopyTo(ofs2);
                    }
                }

                Assert.IsTrue(TestConfig.CompareStreams(input, decrypted));
            }
        }

        [TestMethod]
        public void EncryptDecryptStreams_ValidCerts_gostbig()
        {
            var cert = TestConfig.GostPKCert;

            Stream input = TestConfig.BigFile;
            input.Position = 0;

            {
                Stream encrypted = Coder.Encrypt(input, cert);

                string enctempfile = TestConfig.GetTempFileName();
                using (FileStream ofs1 = File.Create(enctempfile))
                {
                    encrypted.CopyTo(ofs1);
                }

                Stream decrypted = null;
                using (FileStream ifs = File.OpenRead(enctempfile))
                {
                    decrypted = Coder.Decrypt(ifs, cert);

                    string dectempfile = TestConfig.GetTempFileName();
                    using (FileStream ofs2 = File.Create(dectempfile))
                    {
                        decrypted.CopyTo(ofs2);
                    }
                }

                Assert.IsTrue(TestConfig.CompareStreams(input, decrypted));
            }
        }

        [TestMethod]
        public void EncryptDecryptStreams_ValidCerts_nogostsmall()
        {
            var cert = TestConfig.NoGostPKCert;

            Stream input = TestConfig.SmallFile;
            input.Position = 0;

            {
                Stream encrypted = Coder.Encrypt(input, cert);

                string enctempfile = TestConfig.GetTempFileName();
                using (FileStream ofs1 = File.Create(enctempfile))
                {
                    encrypted.CopyTo(ofs1);
                }

                Stream decrypted = null;
                using (FileStream ifs = File.OpenRead(enctempfile))
                {
                    decrypted = Coder.Decrypt(ifs, cert);

                    string dectempfile = TestConfig.GetTempFileName();
                    using (FileStream ofs2 = File.Create(dectempfile))
                    {
                        decrypted.CopyTo(ofs2);
                    }
                }

                Assert.IsTrue(TestConfig.CompareStreams(input, decrypted));
            }
        }

        [TestMethod]
        public void EncryptDecryptStreams_ValidCerts_nogostbig()
        {
            var cert = TestConfig.NoGostPKCert;

            Stream input = TestConfig.BigFile;
            input.Position = 0;

            {
                Stream encrypted = Coder.Encrypt(input, cert);

                string enctempfile = TestConfig.GetTempFileName();
                using (FileStream ofs1 = File.Create(enctempfile))
                {
                    encrypted.CopyTo(ofs1);
                }

                Stream decrypted = null;
                using (FileStream ifs = File.OpenRead(enctempfile))
                {
                    decrypted = Coder.Decrypt(ifs, cert);

                    string dectempfile = TestConfig.GetTempFileName();
                    using (FileStream ofs2 = File.Create(dectempfile))
                    {
                        decrypted.CopyTo(ofs2);
                    }
                }

                Assert.IsTrue(TestConfig.CompareStreams(input, decrypted));
            }
        }

        [TestMethod]
        public void EncryptDecryptStreams_InvalidCerts_nogostsmall()
        {
            var validcert = TestConfig.NoGostPKCert;
            var invalidcert = TestConfig.NoGostPKCert2;

            Stream input = TestConfig.SmallFile;
            input.Position = 0;

            {
                Stream encrypted = Coder.Encrypt(input, validcert);

                string enctempfile = TestConfig.GetTempFileName();
                using (FileStream ofs1 = File.Create(enctempfile))
                {
                    encrypted.CopyTo(ofs1);
                }

                Stream decrypted = null;
                using (FileStream ifs = File.OpenRead(enctempfile))
                {
                    try
                    {
                        decrypted = Coder.Decrypt(ifs, invalidcert);
                    }
                    catch(CryptographicException)
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

        [TestMethod]
        public void EncryptDecryptBytes_ValidCerts_gost()
        {
            var cert = TestConfig.GostPKCert;
            byte[] input = Encoding.ASCII.GetBytes("Some string to encrypt");

            byte[] encrypted = Coder.Encrypt(input, cert);
            byte[] output = Coder.Decrypt(encrypted);

            Assert.IsTrue(output.SequenceEqual(input));
        }

        [TestMethod]
        public void EncryptDecryptBytes_ValidCerts_nogost()
        {
            var cert = TestConfig.NoGostPKCert;
            byte[] input = Encoding.ASCII.GetBytes("Some string to encrypt");

            byte[] encrypted = Coder.Encrypt(input, cert);
            byte[] output = Coder.Decrypt(encrypted);

            Assert.IsTrue(output.SequenceEqual(input));
        }

        [TestMethod]
        public void EncryptSignDecryptBytes_ValidOneCerts_gost()
        {
            var cert = TestConfig.GostPKCert;
            byte[] input = Encoding.ASCII.GetBytes("Some string to encrypt with 123456");

            byte[] encrypted = Coder.SignEncrypt(input, cert, cert);
            byte[] output = Coder.VerifyDecrypt(encrypted);

            Assert.IsTrue(output.SequenceEqual(input));
        }

        [TestMethod]
        public void EncryptSignDecryptBytes_ValidOneCerts_nogost()
        {
            var cert = TestConfig.NoGostPKCert;
            byte[] input = Encoding.ASCII.GetBytes("Some string to encrypt with 123456");

            byte[] encrypted = Coder.SignEncrypt(input, cert, cert);
            byte[] output = Coder.VerifyDecrypt(encrypted);

            Assert.IsTrue(output.SequenceEqual(input));
        }
    }
}
