using DevExpress.Xpf.Core;
using HousePlannerCore;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HousePlanner.Views
{
    /// <summary>
    /// Interaction logic for LoginView.xaml
    /// </summary>
    public partial class LoginView : ThemedWindow
    {
        private IEventAggregator _eventAggregator;
        public LoginView(IEventAggregator ea)
        {
           
            InitializeComponent();
            _eventAggregator = ea;
            ea.GetEvent<OnLoginClosed>().Subscribe(
                (payload) =>
                {
                    this.DialogResult = true;
                    this.Close();
                });
            ea.GetEvent<OnUpdateLogInPasswordBox>().Subscribe(password => passwordBox.Password = password);
        }

        private void passwordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            _eventAggregator.GetEvent<OnSendPassword>().Publish(passwordBox.Password);
        }
    }
}
