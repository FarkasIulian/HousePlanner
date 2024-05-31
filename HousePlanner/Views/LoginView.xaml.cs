﻿using DevExpress.Xpf.Core;
using HousePlannerCore;
using HousePlannerCore.Events;
using Prism.Events;
using System;
using System.Collections.Generic;
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
        public LoginView(IEventAggregator ea)
        {
            InitializeComponent();
            ea.GetEvent<OnLoginClosed>().Subscribe(
                (payload) =>
                {
                    this.DialogResult = true;
                    this.Close();
                });
        }


        
    }
}
