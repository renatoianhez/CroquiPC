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
            // Registrar conversor global de booleanos para Sim/Não no PropertyGrid
            System.ComponentModel.TypeDescriptor.AddAttributes(typeof(bool), new System.ComponentModel.TypeConverterAttribute(typeof(Utils.SimNaoConverter)));

            // Registrar Syncfusion Community License
            // Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1JGaF1cXmhKYVJzWmFZfVhgcF9EYlZRRmY/P1ZhSXxVdkZjUX5YcnZVRGZZV019XEA=");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Forms.FormPrincipal());
        }
    }
}