using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Karaoke_Kingpin
{
    /// <summary>
    /// App.xaml 的互動邏輯
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 這裡設置你的 Xceed LicenseKey
            //Xceed.Wpf.Toolkit.Licenser.LicenseKey = "WTK46P7AYAZU4KA2AFA";
        }
    }
}
