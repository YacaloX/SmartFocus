using System.Windows;
using SmartFocus.Core;

namespace SmartFocus
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            AliasManager.Load();
            HistoryTracker.Load();
        }
    }
}