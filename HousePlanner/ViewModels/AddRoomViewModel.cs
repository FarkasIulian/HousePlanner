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
using System.Windows;
using System.Windows.Input;

namespace HousePlanner.ViewModels
{
    public class AddRoomViewModel : BindableBase
    {
        public string NameTextBox
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public string WidthTextBox
        {
            get => GetValue<string>();
            set => SetValue(value);
        }
        public string LengthTextBox
        {
            get => GetValue<string>();
            set => SetValue(value);
        }
      
        public string Errors
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public ICommand AddRoomCommand => new DelegateCommand(AddRoom);

        private long houseId;
        private DbManagerService _dbManager;
        private IEventAggregator _eventAggregator;
        private System.Drawing.Point roomPosition;
        private int currentFloor;


        public AddRoomViewModel(IEventAggregator ea, IContainerProvider container)
        {
            _dbManager = container.Resolve<DBManager.DbManagerService>();
            _eventAggregator = ea;
            
            _eventAggregator.GetEvent<OnCloseAddWindowResetTextBoxes>().Subscribe(ResetValues);
            _eventAggregator.GetEvent<OnOpenAddRoomWindow>().Subscribe(payload =>
            {
                houseId = payload.Item1;
                currentFloor = payload.Item2;
            });
            _eventAggregator.GetEvent<OnRightClickSendPoint>().Subscribe(point => roomPosition = point);

        }

        private void ResetValues()
        {
            NameTextBox = "";
            WidthTextBox = "";
            LengthTextBox = "";
            Errors = "";
        }

        private async void AddRoom()
        {
            var room = new Room()
            {
                Name = NameTextBox,
                HouseId = houseId,
                Floor = currentFloor,
                Width = int.Parse(WidthTextBox),
                Length = int.Parse(LengthTextBox),
                PositionInHouse = roomPosition
            };

            if(await _dbManager.Insert(room) != -1)
            {
                MessageBox.Show("Inserted room");
                _eventAggregator.GetEvent<OnInsertedRoom>().Publish(room);
            }

        }

    }
}
