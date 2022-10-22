using Prism.Ioc;
using Prism.Modularity;
using Prism.Unity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace StockTraderRI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);



        }

        protected override Window CreateShell()
        {
            return Container.Resolve<Shell>();
        }
        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            // Load modules using code
            moduleCatalog.AddModule<StockTraderRI.Modules.Watch.WatchModule>();

        }

        protected override void InitializeShell(Window shell)
        {

        }
        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // containerRegistry.RegisterDialog<NotificationDialog, NotificationDialogViewModel>();
            // containerRegistry.RegisterSingleton<IDialogService, DialogService>();
            //    containerRegistry.RegisterSingleton<ITaskbarService, TaskbarService>();
        }
    }
}
