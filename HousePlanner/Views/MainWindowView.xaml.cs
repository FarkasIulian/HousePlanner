using DBManager;
using DevExpress.Xpf.Core;
using HousePlannerCore.Events;
using HousePlannerCore.Models;
using Prism.Events;
using Prism.Ioc;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HousePlanner.Views
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MainWindowView : UserControl
    {

        public MainWindowView(IEventAggregator ea, IContainerProvider container)
        {
            InitializeComponent();
        }
    }
}
