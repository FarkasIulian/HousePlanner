using DBManager;
using DevExpress.Mvvm;
using HousePlanner.Models;
using HousePlannerCore;
using HousePlannerCore.Events;
using Prism.Events;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace HousePlanner.ViewModels
{
    public class LoginViewModel : BindableBase
    {
        private string username = "";
        private string password = "";
        public string UsernameTextBox
        {
            get => username;
            set
            {
                if (value != null)
                {
                    username = value;
                    RaisePropertiesChanged(nameof(UsernameTextBox));
                }
            }
        }
        public string PasswordTextBox
        {
            get => password;
            set
            {
                if (value != null)
                {
                    password = value;
                    RaisePropertiesChanged(nameof(PasswordTextBox));
                }
            }
        }

        public Visibility IsUsernameValid
        {
            get => GetValue<Visibility>();
            set => SetValue(value);
        }
        public Visibility IsPasswordValid
        {
            get => GetValue<Visibility>();
            set => SetValue(value);
        }

        public ICommand LoginCommand => new DelegateCommand(LoginLogic);
        private DbManagerService dbManager;
        private IEventAggregator eventAggregator;

        public LoginViewModel(IEventAggregator ea, IContainerProvider provider)
        {
            dbManager = provider.Resolve<DbManagerService>();
            eventAggregator = ea;
            IsUsernameValid = Visibility.Hidden;
            IsPasswordValid = Visibility.Hidden;

        }


        private bool ValidateCredentials()
        {
            var valid = true;
            if (string.IsNullOrEmpty(username))
            {
                IsUsernameValid = Visibility.Visible;
                valid &= false;
            }
            if (string.IsNullOrEmpty(password))
            {
                IsPasswordValid = Visibility.Visible;
                valid &= false;
            }
            if (!valid)
            {
                Task.Run(async () =>
                {
                    await Task.Delay(5000);
                    IsUsernameValid = Visibility.Hidden;
                    IsPasswordValid = Visibility.Hidden;
                });
            }
            return valid;
        }

        private async void LoginLogic()
        {
            if (!ValidateCredentials())
            //var user = await dbManager.GetFiltered<User>(nameof(User.Email), username.Trim());
            //if (user.Count() == 0)
            //{
            //    return;
            //}
            HandleLogin();
        }

        private void HandleLogin()
        {
            eventAggregator.GetEvent<OnLoginClosed>().Publish();
        }
    }
}
