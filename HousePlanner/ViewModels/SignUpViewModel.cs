using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HousePlannerCore.Events;
using System.Windows;
using DevExpress.Mvvm;
using System.Windows.Input;
using Prism.Ioc;
using HousePlannerCore.Models;
using System.Text.RegularExpressions;
using System.Security;

namespace HousePlanner.ViewModels
{
    public class SignUpViewModel : BindableBase
    {

        public string EmailTextBox
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public string PasswordTextBox
        {
            get => GetValue<string>();
            set
            {
                SetValue(value);
                _eventAggregator.GetEvent<OnUpdateSignUpPasswordBoxes>().Publish((value, 1));


            }
        }
        public string RepeatPasswordTextBox
        {
            get => GetValue<string>();
            set
            {
                SetValue(value);
                _eventAggregator.GetEvent<OnUpdateSignUpPasswordBoxes>().Publish((value, 2));
            }
        }

        public string FirstNameTextBox
        {
            get => GetValue<string>();
            set => SetValue(value);
        }
        public string LastNameTextBox
        {
            get => GetValue<string>();
            set => SetValue(value);
        }
        public string Errors
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public bool ShowPassword
        {
            get => GetValue<bool>();
            set => SetValue(value);
        }

        public ICommand SignupCommand => new DelegateCommand(SignUp);

        private DBManager.DbManagerService _dbManager;
        private IEventAggregator _eventAggregator;


        public SignUpViewModel(IEventAggregator ea, IContainerProvider containerProvider)
        {
            _dbManager = containerProvider.Resolve<DBManager.DbManagerService>();
            _eventAggregator = ea;
            _eventAggregator.GetEvent<OnCloseAddWindowResetTextBoxes>().Subscribe(ResetValues);
            _eventAggregator.GetEvent<OnSendSignUpPassword>().Subscribe((payload) =>
            {
                switch (payload.Item2)
                {
                    case 1:
                        PasswordTextBox = payload.Item1;
                        break;
                    case 2:
                        RepeatPasswordTextBox = payload.Item1;
                        break;
                }

            });
            ShowPassword = false;
            ResetValues();
        }

        private async Task CheckForErrors()
        {
            Regex regex = new Regex("^[\\w-\\.]+@([\\w-]+\\.)+[\\w-]{2,4}$");
            Match m = regex.Match(EmailTextBox);
            if (!m.Success)
                Errors += "Add correct email!\n";
            else
            {
                var user = await _dbManager.GetFiltered<User>(nameof(User.Email), EmailTextBox);
                if (user.Count() != 0)
                    Errors += "Email already exists!\n";
            }
            if (!PasswordTextBox.Equals(RepeatPasswordTextBox))
                Errors += "Passwords do not match\n";
            if (FirstNameTextBox == "" || LastNameTextBox == "" || PasswordTextBox == "" || RepeatPasswordTextBox == "")
                Errors += "Fill in all fields";
        }

        private async void SignUp()
        {
            Errors = "";
            await CheckForErrors();
            if (Errors == "")
            {
                var user = new User()
                {
                    Email = EmailTextBox,
                    Password = PasswordTextBox,
                    FirstName = FirstNameTextBox,
                    LastName = LastNameTextBox,

                };
                if (await _dbManager.Insert(user) != -1)
                {
                    MessageBox.Show("Succesfully signed up!");
                    ResetValues();
                }
            }
        }

        private void ResetValues()
        {
            EmailTextBox = "";
            PasswordTextBox = "";
            RepeatPasswordTextBox = "";
            FirstNameTextBox = "";
            LastNameTextBox = "";
            Errors = "";
        }
    }
}
