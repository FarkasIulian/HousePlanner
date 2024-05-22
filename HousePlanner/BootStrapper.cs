using DevExpress.Xpf.Core;
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
            
            var rm = Container.Resolve<IRegionManager>();
            
            rm.RegisterViewWithRegion<MainWindow>("MainWindowRegion");
            
            var shell = Container.Resolve<Shell>();
            
            return shell;
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            EventAggregatorProvider.EventAggregator = Container.Resolve<IEventAggregator>();
        }
    }
}
