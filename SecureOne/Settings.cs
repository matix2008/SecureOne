using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SecureOneLib;
using SecureOneLib.Utilities;

namespace SecureOne
{
    /// <summary>
    /// Реализует набор системных настроект приложения
    /// </summary>
    public class Settings : IDisposable
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public Settings()
        {
            // Загружаем данные найтроек из файла
            Load();

            // Переустанавливаем некорректные найтроки
            ResetIncorrectSettings();
        }

        public string OwnerWorkingFolder { get; set; }
        public CertificateWrapper OwnerCertificate { get; set; }
        public CertificateCollectionWrapper RecipientsCertificatesCollection { get; set; }
        public bool AllwaysUseDetachedSign { get; set; }
        public bool AllwaysUseCustomEncFrmt { get; set; }

        public void Reset()
        {
            Properties.Settings.Default.Reset();
            logger.Info("Aplication settings was reseted.");

            Load();
        }

        public void Load()
        {
            try
            {
                logger.Info("Aplication settings are going to load.");

                OwnerWorkingFolder = Properties.Settings.Default.OwnerWorkingFolder;
                AllwaysUseDetachedSign = Properties.Settings.Default.AllwaysUseDetachedSign;
                AllwaysUseCustomEncFrmt = Properties.Settings.Default.AllwaysUseCustomEncFrmt;
                OwnerCertificate = CertificateWrapper.Parse(Properties.Settings.Default.OwnerCertificate);
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
                Properties.Settings.Default.OwnerWorkingFolder = OwnerWorkingFolder;
                Properties.Settings.Default.AllwaysUseDetachedSign = AllwaysUseDetachedSign;
                Properties.Settings.Default.AllwaysUseCustomEncFrmt = AllwaysUseCustomEncFrmt;
                Properties.Settings.Default.OwnerCertificate = OwnerCertificate?.ToString() ?? string.Empty;
                Properties.Settings.Default.RecipientsCertificatesCollection = RecipientsCertificatesCollection?.ToString() ?? string.Empty;

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

        /// <summary>
        /// Проверяет, что установлены обязательные параметры
        /// </summary>
        /// <returns></returns>
        public bool CheckRequiredFieldsAreFilled()
        {
            return (OwnerCertificate != null &&
                    OwnerWorkingFolder.Length > 0);
        }

        /// <summary>
        /// Сбрасывает некорректные настройки в их значения по умолчанию
        /// </summary>
        protected void ResetIncorrectSettings()
        {
            logger.Info("Aplication settings are going to check for incorrect values.");

            if (OwnerWorkingFolder.Length == 0 || !Directory.Exists(OwnerWorkingFolder))
            {
                OwnerWorkingFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                logger.Info($"Working folder was reset to default value: {OwnerWorkingFolder}");
            }

            if (OwnerCertificate != null)
            {
                if (!OwnerCertificate.Value.HasPrivateKey || !OwnerCertificate.Value.Verify())
                {
                    OwnerCertificate = null;
                    logger.Info($"Owner's certificate was not passed verification and deleted.");
                }
            }

            //TODO: Должна быть проверка действительности установленных сертификатов

            logger.Info("All aplication settings was checked.");
        }
    }
}
