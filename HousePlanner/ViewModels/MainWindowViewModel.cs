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

namespace HousePlanner.ViewModels
{
    class MainWindowViewModel : BindableBase
    {


        private int floorNumber;

        public int FloorNumber
        {
            get
            {
                return floorNumber;
            }
            set
            {
                floorNumber = value;
                RaisePropertiesChanged(nameof(FloorNumber));
            }
        }

        public ICommand<string> ChangeFloorCommand => new DelegateCommand<string>(ChangeFloor);
        public System.Windows.Input.ICommand AddNewRoomCommand => new DelegateCommand(AddRoom);


        private DBManager.DbManagerService dbManager;
        private IEventAggregator eventAggregator;
        private User user;
        private List<Room> roomsInHouse;


        public MainWindowViewModel(IEventAggregator ea, IContainerProvider container)
        {
            FloorNumber = 0;
            eventAggregator = ea;
            dbManager = container.Resolve<DbManagerService>();
            //    eventAggregator.GetEvent<OnLoginClosed>().Subscribe(
            //         async (payload) =>
            //         {
            //             user = payload;                    
            //         });
            //
            LoadAllRoomsFromHouse();
        }


        private async Task LoadAllRoomsFromHouse()
        {
            roomsInHouse = await dbManager.GetAll<Room>();
            LoadRooms();
        }
        private void LoadRooms()
        {
            var filteredRooms = roomsInHouse.Where(r => r.Floor == FloorNumber);
            foreach (Room room in filteredRooms)
            {
                eventAggregator.GetEvent<OnInsertedRoom>().Publish(room);
            }

        }
        private void ChangeFloor(string floorAction)
        {
            if (floorAction == "Up")
                FloorNumber++;
            else if (FloorNumber > 0)
                FloorNumber--;
            LoadRooms();
        }
        private void AddRoom()
        {
            eventAggregator.GetEvent<OnOpenAddRoomWindow>().Publish((0,FloorNumber));
        }
    }
}
