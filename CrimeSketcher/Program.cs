// Program.cs
using System;
using System.Windows.Forms;

namespace CrimeSketcher
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            // Registrar Syncfusion Community License
            // Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1JGaF1cXmhKYVJzWmFZfVhgcF9EYlZRRmY/P1ZhSXxVdkZjUX5YcnZVRGZZV019XEA=");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Forms.FormPrincipal());
        }
    }
}