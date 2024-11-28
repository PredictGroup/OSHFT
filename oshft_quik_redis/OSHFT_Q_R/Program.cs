using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OSHFT_Q_R
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                bool first;

                using (Mutex mutex = new Mutex(true, cfg.ExecFile.Replace('\\', ':'), out first))
                {
                    if (first)
                    {
                        // ------------------------------------------------------

                        //cfg.LoadUserConfig(cfg.UserCfgFile);

                        Application.EnableVisualStyles();
                        Application.SetCompatibleTextRenderingDefault(false);
                        Application.Run(new OSHFT_Q_RMain());

                        cfg.SaveUserConfig(cfg.UserCfgFile);

                        // ------------------------------------------------------
                    }
                    else
                        ShowMessage("Окно программы уже запущено:\n"
                          + cfg.ExecFile);
                }
            }
            catch (Exception exp)
            {
                UnhandledException(null, new UnhandledExceptionEventArgs(exp, true));
            }
        }

        // **********************************************************************

        static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.IsTerminating)
                AppDomain.CurrentDomain.UnhandledException -= UnhandledException;

            Exception ex = e.ExceptionObject as Exception;

            ShowMessage("В ходе выполнения программы произошла критическая ошибка:\n\n"
              + (ex == null ? e.ToString() : ex.ToString()));
        }

        // **********************************************************************

        public static void ShowMessage(string text)
        {
            MessageBox.Show(text, cfg.FullProgName);
        }

    }
}
