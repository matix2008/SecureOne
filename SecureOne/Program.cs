using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SecureOne
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            NLog.LogManager.GetCurrentClassLogger().Info("SecureOne is starting...");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());

            NLog.LogManager.GetCurrentClassLogger().Info("SecureOne was finished.");
        }
    }
}
