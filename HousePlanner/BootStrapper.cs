using DBManager;
using DevExpress.Xpf.Core;
using HousePlanner.Views;
using HousePlannerCore;
using Prism.DryIoc;
using Prism.Events;
using Prism.Ioc;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HousePlanner
{
    public class BootStrapper : PrismBootstrapper
    {
        protected override DependencyObject CreateShell()
        {
            ApplicationThemeHelper.ApplicationThemeName = Theme.VS2019DarkName;
            
            Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            
            var rm = Container.Resolve<IRegionManager>();
            
            Container.Resolve<AddFurnitureView>();
            Container.Resolve<AddRoomView>();
            Container.Resolve<AddHouseView>();
            Container.Resolve<SignUpView>();
            
            var loginView = Container.Resolve<LoginView>();
            
            
            if (!(bool)loginView.ShowDialog())
                Environment.Exit(0);

            Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;

            rm.RegisterViewWithRegion<MainWindowView>("MainWindowViewRegion");

            var shell = Container.Resolve<Shell>();

            Application.Current.MainWindow = shell;

            return shell;
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            //to do when get home, remove reference to DbManagerService, initialize it in Core
            EventAggregatorProvider.EventAggregator = Container.Resolve<IEventAggregator>();
        }
    }
}
