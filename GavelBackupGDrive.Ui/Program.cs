using DevExpress.LookAndFeel;
using DevExpress.Skins;
using DevExpress.UserSkins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace GavelBackupGDrive.Ui
{
    internal static class Program
    {
        static Mutex mutex = new Mutex(true, "MyApplicationName");

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (!mutex.WaitOne(TimeSpan.Zero, true))
            {
                MessageBox.Show("Another instance of the application is already running.");
                return;
            }

            try
            {
                Application.Run(new MainForm());
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }
    }
}
