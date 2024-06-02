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
    public class AddRoomViewModel : AddRoomAndFurnitureBase
    {
        

        public ICommand AddRoomCommand => new DelegateCommand(AddRoom);

        private long houseId;       
        private int currentFloor;

        public AddRoomViewModel(IEventAggregator ea, IContainerProvider container) : base(ea,container)
        {
            _eventAggregator.GetEvent<OnCloseAddWindowResetTextBoxes>().Subscribe(ResetValues);
            _eventAggregator.GetEvent<OnSendHouseData>().Subscribe(payload =>
            {
                houseId = payload.Item1;
                currentFloor = payload.Item2;
            }, true);
            _eventAggregator.GetEvent<OnRightClickSendPoint>().Subscribe(
                point => position = point);
        }

        private void AddRoom()
        {
            var room = new Room()
            {
                Name = NameTextBox,
                HouseId = houseId,
                Floor = currentFloor,
                Width = int.Parse(WidthTextBox),
                Length = int.Parse(LengthTextBox),
                PositionInHouse = position
            };
            _eventAggregator.GetEvent<OnTryInsertingRoom>().Publish((room, true));


        }

    }
}
