using DevExpress.Xpf.Core;
using HousePlannerCore.Events;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HousePlanner.Views
{
    /// <summary>
    /// Interaction logic for AddRoomView.xaml
    /// </summary>
    public partial class AddRoomView : ThemedWindow
    {
        private IEventAggregator _eventAggregator;

        public AddRoomView(IEventAggregator ea)
        {
            InitializeComponent();
            _eventAggregator = ea;
            _eventAggregator.GetEvent<OnOpenAddRoomWindow>().Subscribe(() => this.ShowDialog());
            _eventAggregator.GetEvent<OnTryInsertingRoom>().Subscribe(payload => CloseAndReset());
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CloseAndReset();
        }

        private void CloseAndReset()
        {
            _eventAggregator.GetEvent<OnCloseAddWindowResetTextBoxes>().Publish();
            this.Close();
        }

    }
}
