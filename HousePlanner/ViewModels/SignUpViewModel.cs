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
using HousePlanner.Models;

namespace HousePlanner.ViewModels
{
    public class SignUpViewModel : BindableBase
    {
        private IEventAggregator _eventAggregator;

        public string EmailTextBox
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public string PasswordTextBox
        {
            get => GetValue<string>();
            set => SetValue(value);
        }
        public string RepeatPasswordTextBox
        {
            get => GetValue<string>();
            set => SetValue(value);
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

        public ICommand SignupCommand => new DelegateCommand(SignUp);


        private DBManager.DbManagerService _dbManager;
        

        public SignUpViewModel(IEventAggregator ea,IContainerProvider containerProvider) 
        {
            _dbManager = containerProvider.Resolve<DBManager.DbManagerService>();
            _eventAggregator = ea;
            _eventAggregator.GetEvent<OnCloseSignUpResetTextBoxes>().Subscribe(() => MessageBox.Show("ceva"));


        }

        private async void SignUp()
        {
            var user = new User()
            {
                Email = EmailTextBox,
                Password = PasswordTextBox,
                FirstName = FirstNameTextBox,
                LastName = LastNameTextBox,

            };
            if(await _dbManager.Insert(user) != -1)
            {
                MessageBox.Show("Inserted");
            }
        }

    }
}
