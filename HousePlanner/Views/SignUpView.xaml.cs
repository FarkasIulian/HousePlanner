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
    /// Interaction logic for SignUpView.xaml
    /// </summary>
    public partial class SignUpView : ThemedWindow
    {
        private IEventAggregator _eventAggregator;

        public SignUpView(IEventAggregator ea)
        {
            InitializeComponent();
            _eventAggregator = ea;
            _eventAggregator.GetEvent<OnOpenSignUpWindow>().Subscribe(() => this.ShowDialog());
            _eventAggregator.GetEvent<OnUpdateSignUpPasswordBoxes>().Subscribe((payload) =>
            {
                switch (payload.Item2)
                {
                    case 1:
                        firstPasswordBox.Password = payload.Item1;
                        break;
                    case 2:
                        secondPasswordBox.Password = payload.Item1;
                        break;
                }
            });
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _eventAggregator.GetEvent<OnCloseAddWindowResetTextBoxes>().Publish();
            this.Close();
        }

        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            _eventAggregator.GetEvent<OnSendSignUpPassword>().Publish((firstPasswordBox.Password, 1));
        }

        private void secondPasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            _eventAggregator.GetEvent<OnSendSignUpPassword>().Publish((secondPasswordBox.Password, 2));

        }
    }
}
