using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Pkcs;
using GostCryptography.Base;
using System.IO;

namespace SecureOneLib.Crypto
{
    public static class Signer
    {
        public static byte[] Sign(Stream dataStream, X509Certificate2 signerCert, bool detached)
        {
            if (!signerCert.HasPrivateKey)
                throw new CryptographicException("Certificate for signing has not a private key.");

            ContentInfo content = null;

            if (detached)
            {
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
                content = new ContentInfo(hash);
            }
            else
            {
                byte[] data = new byte[dataStream.Length];
                dataStream.Read(data, 0, (int)dataStream.Length);
                content = new ContentInfo(data);
            }

            SignedCms signedCms = new SignedCms(content, detached);
            CmsSigner signer = new CmsSigner(SubjectIdentifierType.IssuerAndSerialNumber, signerCert);
            signer.IncludeOption = X509IncludeOption.WholeChain;
            signedCms.ComputeSignature(signer);
            return signedCms.Encode();
        }

        public static SignerInfoCollection GetSignerInfos(byte[] sign)
        {
            SignedCms signedCms = new SignedCms();
            signedCms.Decode(sign);
            return signedCms.SignerInfos;
        }

        public static X509Certificate2Collection FindCertsForSign(byte[] sign, StoreName sn = StoreName.My, StoreLocation sl = StoreLocation.CurrentUser)
        {
            X509Certificate2Collection coll = new X509Certificate2Collection();

            SignedCms signedCms = new SignedCms();
            signedCms.Decode(sign);

            X509Store store = new X509Store(sn, sl);
            store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);

            X509Certificate2Collection fcollection = (X509Certificate2Collection)store.Certificates.
                Find(X509FindType.FindByTimeValid, DateTime.Now, false);

            foreach (var si in signedCms.SignerInfos)
            {
                foreach (var cert in fcollection)
                    if (cert.Subject.Contains(si.Certificate.Subject))
                        coll.Add(cert);
            }

            store.Close();
            return coll;
        }

        public static void Verify(byte[] sign, Stream dataStream, X509Certificate2 signerCert)
        {
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

        public static void Verify(byte[] sign, X509Certificate2 signerCert)
        {
            SignedCms signedCms = new SignedCms();
            signedCms.Decode(sign);
            signedCms.CheckSignature(false);
        }
    }
}
