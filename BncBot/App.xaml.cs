using System.Globalization;
using System.Threading;
using System.Windows;

namespace BncBot
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var cultura = new CultureInfo("pt-BR");

            CultureInfo.DefaultThreadCurrentCulture = cultura;
            CultureInfo.DefaultThreadCurrentUICulture = cultura;

            base.OnStartup(e);
        }
    }
}