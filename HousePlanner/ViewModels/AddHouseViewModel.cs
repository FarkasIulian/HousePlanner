using DBManager;
using DevExpress.Mvvm;
using HousePlannerCore.Events;
using HousePlannerCore.Models;
using Prism.Events;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HousePlanner.ViewModels
{
    public class AddHouseViewModel : BindableBase
    {
        public string EmailTextBox
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public string LayoutNameTextBox
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public string NumberOfFloorsTextBox
        {
            get => GetValue<string>();
            set => SetValue(value);
        }
        
        public string Errors
        {
            get => GetValue<string>();
            set => SetValue(value);
        }


        public ICommand AddHouseCommand => new DelegateCommand(AddHouse);

        private DbManagerService _dbManager;
        private IEventAggregator _eventAggregator;

        public AddHouseViewModel(IEventAggregator ea, IContainerProvider container )
        {
            _dbManager = container.Resolve<DBManager.DbManagerService>();
            _eventAggregator = ea;
            _eventAggregator.GetEvent<OnSendUserInformation>().Subscribe((user) => EmailTextBox = user.Email,true);
            _eventAggregator.GetEvent<OnCloseAddWindowResetTextBoxes>().Subscribe(ResetValues);
            ResetValues();
           
        }

        private async void AddHouse()
        {
            Errors = "";
            int parsedValue;
            bool parsed = int.TryParse(NumberOfFloorsTextBox,out parsedValue);
            if (!parsed)
                Errors += "Insert number in number of floors!";
            if (LayoutNameTextBox == "" || NumberOfFloorsTextBox == "")
                Errors += "\nFill in all fields!";
            if (Errors == "")
            {
                var house = new House()
                {
                    OwnerEmail = EmailTextBox,
                    Name = LayoutNameTextBox,
                    NumberOfFloors = parsedValue
                };
                var id = await _dbManager.Insert(house);
                if (id != -1)
                {
                    house.Id = id;
                    _eventAggregator.GetEvent<OnInsertedHouse>().Publish(house);

                }
            }
        }


        private void ResetValues()
        {
            LayoutNameTextBox = "";
            NumberOfFloorsTextBox = "";
        }
    }
}
