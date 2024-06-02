using DBManager;
using DevExpress.Mvvm;
using HousePlanner.Views;
using HousePlannerCore.Events;
using HousePlannerCore.Models;
using Prism.Events;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

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

      

        public House SelectedHouse
        {
            get => GetValue<House>();
            set => SetValue(value);
        }


        public ICommand<string> ChangeFloorCommand => new DelegateCommand<string>(ChangeFloor);
        public System.Windows.Input.ICommand AddNewHouseCommand => new DelegateCommand(AddHouse);

        public ICommand HouseSelectionChanged => new DelegateCommand<House>(HandleSelectedHouseChange);

        public ObservableCollection<House> Houses { get; set; } = new ObservableCollection<House>();

        public object RoomsView { get; set; }

        
        private bool loading = false;
        private User currentUser;
        private DBManager.DbManagerService dbManager;
        private IEventAggregator eventAggregator;
        private List<Room> roomsInHouse = new List<Room>();
        private async void HandleSelectedHouseChange(House newSelection)
        {
            eventAggregator.GetEvent<ResetCanvas>().Publish();
            FloorNumber = 0;
            LoadRooms();
        }

        public MainWindowViewModel(IEventAggregator ea, IContainerProvider container)
        {
            FloorNumber = 0;
            eventAggregator = ea;
            RoomsView = container.Resolve<RoomsView>();
            dbManager = container.Resolve<DbManagerService>();
            eventAggregator.GetEvent<OnInsertedHouse>().Subscribe(Houses.Add, true);
            eventAggregator.GetEvent<OnRoomValidForInsertion>().Subscribe((room) =>
            {
                roomsInHouse.Add(room);
            }, true);
            eventAggregator.GetEvent<OnSendUserInformation>().Subscribe((user) => currentUser = user);
            eventAggregator.GetEvent<OnModifiedRoom>().Subscribe((room) =>
            {
                try
                {
                    roomsInHouse.First(r => r.Id == room.Id).Name = room.Name;
                    roomsInHouse.First(r => r.Id == room.Id).Width = room.Width;
                    roomsInHouse.First(r => r.Id == room.Id).Length = room.Length;
                }
                catch (Exception)
                {
                    //LoadRooms();
                }
            });
            eventAggregator.GetEvent<OnRequestUserEmail>().Publish();
            LoadHouseLayouts();
        }


        private async void LoadRooms()
        {
            loading = true;
            eventAggregator.GetEvent<OnSendHouseData>().Publish((SelectedHouse.Id, FloorNumber));

            roomsInHouse = await dbManager.GetFiltered<Room>(nameof(Room.HouseId), SelectedHouse.Id.ToString());
            var filteredRooms = roomsInHouse.Where(r => r.Floor == FloorNumber);
            foreach (Room room in filteredRooms)
                Application.Current.Dispatcher.Invoke(delegate
                {
                    eventAggregator.GetEvent<OnTryInsertingRoom>().Publish((room, false));
                });
            loading = false;

        }

        private async Task LoadHouseLayouts()
        {
            var houses = await dbManager.GetFiltered<House>(nameof(House.OwnerEmail), currentUser.Email);
            Houses.AddRange(houses);


        }

        private void ChangeFloor(string floorAction)
        {
            var initialFloor = FloorNumber;
            if (floorAction == "Up")
                if (FloorNumber < SelectedHouse.NumberOfFloors)
                    FloorNumber++;
                else MessageBox.Show("Reached last floor");
            else if (FloorNumber > 0)
                FloorNumber--;
            if (initialFloor != FloorNumber && !loading)
            {
                eventAggregator.GetEvent<ResetCanvas>().Publish();
                LoadRooms();
                eventAggregator.GetEvent<OnSendHouseData>().Publish((SelectedHouse.Id, FloorNumber));
            }
        }
        

        private void AddHouse()
        {
            eventAggregator.GetEvent<OnOpenAddHouseWindow>().Publish();
        }
    }

}
