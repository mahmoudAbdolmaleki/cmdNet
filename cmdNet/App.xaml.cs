using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using OfficeOpenXml;

namespace cmdNet
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        public App()
        {
            
    }
        protected override void OnStartup(StartupEventArgs e)
        {
            ExcelPackage.License.SetNonCommercialPersonal("Mahmoud Maleki");
            //MessageBox.Show("License Set");
            base.OnStartup(e);
        }
    }
}
