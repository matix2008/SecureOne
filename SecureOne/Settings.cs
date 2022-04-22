using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecureOneLib;
using SecureOneLib.Utilities;

namespace SecureOne
{
    /// <summary>
    /// Набор установок приложения
    /// </summary>
    public class Settings : IDisposable
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public Settings()
        {
            Load();
        }

        public CertificateWrapper SenderCertificate { get; set; }
        public CertificateCollectionWrapper RecipientsCertificatesCollection { get; set; }
        public bool AlwaysUseAttachedSign { get; set; }
        public bool AlwaysUseGost { get; set; }

        public bool CheckRequiredFieldsAreFilled()
        {
            return (SenderCertificate != null);
        }

        public void Clear()
        {
            SenderCertificate = null;
            RecipientsCertificatesCollection = null;
            AlwaysUseAttachedSign = true;
            AlwaysUseGost = true;
        }

        public void Reset()
        {
            Properties.Settings.Default.Reset();
            logger.Info("Aplication settings was reseted.");
            Load();
        }

        public void Load()
        {
            SenderCertificate = null;
            RecipientsCertificatesCollection = null;

            try
            {
                AlwaysUseAttachedSign = Properties.Settings.Default.AlwaysUseAttachedSign;
                AlwaysUseGost = Properties.Settings.Default.AlwaysUseGost;

                if (Properties.Settings.Default.SenderCertificate.Length != 0)
                    SenderCertificate = CertificateWrapper.Parse(Properties.Settings.Default.SenderCertificate);

                if (Properties.Settings.Default.RecipientsCertificatesCollection.Length != 0)
                    RecipientsCertificatesCollection = new CertificateCollectionWrapper(Properties.Settings.Default.RecipientsCertificatesCollection);

                logger.Info("Aplication settings successfully loaded.");
            }
            catch(Exception ex)
            {
                logger.Error(ex, "Exeption during loading application settings.");
                throw ex;
            }
        }

        public void Save()
        {
            try
            {
                Properties.Settings.Default.AlwaysUseAttachedSign = AlwaysUseAttachedSign;
                Properties.Settings.Default.AlwaysUseGost = AlwaysUseGost;
                Properties.Settings.Default.SenderCertificate = SenderCertificate?.ToString() ?? string.Empty;
                Properties.Settings.Default.RecipientsCertificatesCollection = RecipientsCertificatesCollection?.ToString() ?? string.Empty;
                Properties.Settings.Default.AllRequiredFieldsAreFilled = CheckRequiredFieldsAreFilled();

                logger.Info("Aplication settings successfully saved.");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exeption during saving application settings.");
                throw ex;
            }

            Properties.Settings.Default.Save();
        }

        public void Dispose()
        {
            Load();
        }
    }
}
