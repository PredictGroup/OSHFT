
using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Xml.Serialization;
using System.Linq;

namespace OSHFT_Q_R
{
    static class cfg
    {
        // **********************************************************************
        // *                        Constants & Properties                      *
        // **********************************************************************

        public const string ProgName = "OSHFT_Q_R";
        public static readonly string FullProgName;

        // **********************************************************************

        public const int TryConnectInterval = 1000;
        public const int QuikTryConnectInterval = 1000;

        // **********************************************************************

        public static readonly TimeSpan RefreshInterval = new TimeSpan(0, 0, 0, 0, 1);
        public static readonly TimeSpan SbUpdateInterval = new TimeSpan(0, 0, 0, 1, 000);

        // **********************************************************************

        public static UserSettings u { get; private set; }

        // **********************************************************************

        public static int DataRcvTimeout = 3000;

        // **********************************************************************

        public static int WorkSecKey { get; private set; }

        public static string MainFormTitle { get; private set; }

        public static CultureInfo BaseCulture { get; private set; }
        public static NumberFormatInfo PriceFormat { get; private set; }

        // **********************************************************************

        public const string UserCfgFileExt = "conf";

        // **********************************************************************

        public static readonly string AsmPath;

        public static readonly string ExecFile;
        public static readonly string UserCfgFile;
        public static readonly string StatCfgFile;


        // **********************************************************************
        // *                             Constructor                            *
        // **********************************************************************

        static cfg()
        {
            // ------------------------------------------------------------

            Version ver = Assembly.GetExecutingAssembly().GetName().Version;
            FullProgName = ProgName + " " + ver.Major.ToString() + "." + ver.Minor.ToString();

            // ------------------------------------------------------------

            ExecFile = Assembly.GetExecutingAssembly().Location;
            string fs = ExecFile.Remove(ExecFile.LastIndexOf('.') + 1);

            UserCfgFile = fs + UserCfgFileExt;
            StatCfgFile = fs + "sc";


            // ------------------------------------------------------------

            BaseCulture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            BaseCulture.NumberFormat.NumberDecimalDigits = 0;

            PriceFormat = (NumberFormatInfo)NumberFormatInfo.CurrentInfo.Clone();

            // ------------------------------------------------------------

#if DEBUG
            u = new UserSettings();
            Reinit();
#endif

            // ------------------------------------------------------------
        }


        // **********************************************************************
        // *                          Properties reinit                         *
        // **********************************************************************

        public static void Reinit()
        {
            WorkSecKey = Security.GetKey(cfg.u.SecCode, cfg.u.ClassCode);

            MainFormTitle = u.SecCode.Length > 0 ? u.SecCode + " - " + cfg.FullProgName : cfg.FullProgName;

            PriceFormat.NumberDecimalDigits = (int)Math.Log10(u.PriceRatio);

        }

        // **********************************************************************
        // *                        User config methods                         *
        // **********************************************************************

        public static void SaveUserConfig(string fn)
        {
            try
            {
                using (Stream fs = new FileStream(fn, FileMode.Create, FileAccess.Write, FileShare.Read))
                {
                    XmlSerializer xs = new XmlSerializer(typeof(UserSettings));
                    xs.Serialize(fs, u);
                }
            }
            catch (Exception e)
            {
                OSHFT_Q_RMain.ShowMessage("Ошибка сохранения конфигурационного файла:\n"
                + e.Message);
            }
        }

        // **********************************************************************

        public static void LoadUserConfig(string fn)
        {
            try
            {
                using (Stream fs = File.OpenRead(fn))
                {
                    XmlSerializer xs = new XmlSerializer(typeof(UserSettings));
                    u = (UserSettings)xs.Deserialize(fs);
                }

                Reinit();
            }
            catch (Exception e)
            {
                if (!(u == null && e is FileNotFoundException))
                    OSHFT_Q_RMain.ShowMessage("Ошибка загрузки конфигурационного файла:\n" + e.Message
                    + "\nИспользованы исходные настройки.");

                if (u == null)
                {
                    u = new UserSettings();
                    Reinit();
                }
            }
        }

        // **********************************************************************
    }
}
