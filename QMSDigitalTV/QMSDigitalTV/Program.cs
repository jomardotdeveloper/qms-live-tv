using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QMSDigitalTV
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            

            if (new FormProductKey().IsInstalled())
            {
                Application.Run(new FormSignage());
            }
            else
            {
                Application.Run(new FormProductKey());
            }
        }
    }
}
