using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SecureOneLib;

namespace SecureOne
{
    /// <summary>
    /// Реализует набор системных настроек приложения
    /// </summary>
    public class Settings
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();    // Объект логирования

        /// <summary>
        /// Конструирует объект настроек
        /// </summary>
        public Settings()
        {
            // Загружаем данные найтроек из файла
            Load();
        }

        /// <summary>
        /// Рабочий каталог
        /// </summary>
        public string OwnerWorkingFolder { get; set; }
        /// <summary>
        /// Сертификатр владельца
        /// </summary>
        public CertificateWrapper OwnerCertificate { get; set; }
        /// <summary>
        /// Коллекция сертификатов контрагентов
        /// </summary>
        public CertificateCollectionWrapper RecipientsCertificatesCollection { get; set; }
        /// <summary>
        /// Флаг - всегда использовать отсоединенную подпись
        /// </summary>
        public bool AllwaysUseDetachedSign { get; set; }
        /// <summary>
        /// Флаг - всегда использовать собственный формат шифрования
        /// </summary>
        public bool AllwaysUseCustomEncFrmt { get; set; }

        /// <summary>
        /// Сбрасывает настройки
        /// </summary>
        public void Reset()
        {
            Properties.Settings.Default.Reset();
            logger.Info("Настройки приложения перегружены");

            Load();
        }

        /// <summary>
        /// Загружает настройки
        /// </summary>
        public void Load()
        {
            try
            {
                logger.Info("Начинаем загружать системные настройки");

                OwnerWorkingFolder = Properties.Settings.Default.OwnerWorkingFolder;
                AllwaysUseDetachedSign = Properties.Settings.Default.AllwaysUseDetachedSign;
                AllwaysUseCustomEncFrmt = Properties.Settings.Default.AllwaysUseCustomEncFrmt;
                OwnerCertificate = CertificateWrapper.Parse(Properties.Settings.Default.OwnerCertificate);
                RecipientsCertificatesCollection = new CertificateCollectionWrapper(Properties.Settings.Default.RecipientsCertificatesCollection);

                logger.Info("Системные настройки успешно загружены");
            }
            catch(Exception ex)
            {
                logger.Error(ex, "Ошибка во время загрузки системных настроек");
                throw;
            }
        }

        /// <summary>
        /// Сохраняет настройки
        /// </summary>
        public void Save()
        {
            try
            {
                Properties.Settings.Default.OwnerWorkingFolder = OwnerWorkingFolder;
                Properties.Settings.Default.AllwaysUseDetachedSign = AllwaysUseDetachedSign;
                Properties.Settings.Default.AllwaysUseCustomEncFrmt = AllwaysUseCustomEncFrmt;
                Properties.Settings.Default.OwnerCertificate = OwnerCertificate?.ToString() ?? string.Empty;
                Properties.Settings.Default.RecipientsCertificatesCollection = RecipientsCertificatesCollection?.ToString() ?? string.Empty;

                logger.Info("Системные настройки успешно сохранены");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Ошибка во время сохранения системных настроек");
                throw;
            }

            Properties.Settings.Default.Save();
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

    }
}
