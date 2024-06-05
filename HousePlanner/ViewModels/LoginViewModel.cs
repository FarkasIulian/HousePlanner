﻿using DBManager;
using DevExpress.Mvvm;
using HousePlannerCore.Events;
using HousePlannerCore.Models;
using Prism.Events;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace HousePlanner.ViewModels
{
    public class LoginViewModel : BindableBase
    {
        public string UsernameTextBox
        {
            get => GetValue<string>();
            set => SetValue(value);
        }
        public string PasswordTextBox
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public Visibility IsErrorTextVisible
        {
            get => GetValue<Visibility>();
            set => SetValue(value);
        }

        public string ErrorText
        {
            get => GetValue<string>();
            set => SetValue(value);
        }


        public ICommand LoginCommand => new DelegateCommand(LoginLogic);
        public ICommand SignupCommand => new DelegateCommand(SignUp);

        public ICommand ForgotPasswordCommand => new DelegateCommand(ForgotPassword);




        private DbManagerService dbManager;
        private IEventAggregator eventAggregator;
        private bool  isExecuting = false;
        private User? user;


        public LoginViewModel(IEventAggregator ea, IContainerProvider provider)
        {            
            dbManager = provider.Resolve<DbManagerService>();
            eventAggregator = ea;
            IsErrorTextVisible = Visibility.Hidden;
            eventAggregator.GetEvent<OnRequestUserEmail>().Subscribe(() => eventAggregator.GetEvent<OnSendUserInformation>().Publish(user));
        }


        private bool ValidCredentials()
        {
            var valid = true;
            
            if (string.IsNullOrEmpty(UsernameTextBox) || string.IsNullOrEmpty(PasswordTextBox))
                valid &= false;   

            if (!valid) 
                SetErrorText("Fields cannot be empty!");
            
            return valid;
        }

        private async void LoginLogic()
        {
            try
            {
                if (!isExecuting)
                {
                    isExecuting = true;
                    if (!ValidCredentials())
                        return;
                    user = (await dbManager.GetFiltered<User>(nameof(User.Email), UsernameTextBox.Trim())).FirstOrDefault();
                    if (user == null)
                    {
                        SetErrorText("No user found! Please sign up!");
                        return;
                    }
                    HandleLogin(user);
                }
            }
            finally
            {
                isExecuting = false;
            }
        }


        private void SignUp()
        {
            eventAggregator.GetEvent<OnOpenSignUpWindow>().Publish();
        }


        private void SetErrorText(string text)
        {
            Task.Run(async () =>
            {

                ErrorText = text;
                IsErrorTextVisible = Visibility.Visible;
                await Task.Delay(5000);
                IsErrorTextVisible = Visibility.Hidden;
            });
        }


        private void HandleLogin(User user)
        {
            if (PasswordTextBox.Equals(user.Password))
            {
                MessageBox.Show("Login was succesful!","Succesful Login",MessageBoxButton.OK,MessageBoxImage.Information);
                eventAggregator.GetEvent<OnLoginClosed>().Publish(user);
                
            }
            else
                SetErrorText("Password is incorrect!");
        }

        private async void ForgotPassword()
        {
            if (string.IsNullOrEmpty(UsernameTextBox))
            {
                MessageBox.Show("Please fill in email!");
                return;
            }

            try
            {

                var user = (await dbManager.GetFiltered<User>(nameof(User.Email), UsernameTextBox)).First();
                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("higurashi.ryuk@gmail.com", "gzjl gzkq nwdp ppzr"),
                    EnableSsl = true,
                };
                var mailMessage = new MailMessage
                {
                    From = new MailAddress("higurashi.ryuk@gmail.com"),
                    Subject = "Password",
                    Body = "Your password is: " + user.Password,
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(UsernameTextBox);
                smtpClient.Send(mailMessage);
                MessageBox.Show("Check your email for the password!");
            }
            catch (SmtpException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
