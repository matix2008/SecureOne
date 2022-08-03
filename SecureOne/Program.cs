using System;
using System.Windows.Forms;

namespace SecureOne
{
    /// <summary>
    /// Реализует основной цикл программы
    /// </summary>
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            NLog.LogManager.GetCurrentClassLogger().Info("Начало работы приложения");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());

            NLog.LogManager.GetCurrentClassLogger().Info("Заверешение работы приложения");
        }
    }
}
