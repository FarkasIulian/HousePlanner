using DBManager;
using DevExpress.Mvvm;
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

        public string SelectedRoomWidth
        {
            get => GetValue<string>();
            set => SetValue(value);
        }
        public string SelectedRoomLength
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public string SelectedRoomName
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public House SelectedHouse
        {
            get => GetValue<House>();
            set => SetValue(value);
        }

        public Visibility ModifyRoomOptions
        {
            get => GetValue<Visibility>();
            set => SetValue(value);
        }

        public ICommand<string> ChangeFloorCommand => new DelegateCommand<string>(ChangeFloor);
        public System.Windows.Input.ICommand AddNewRoomCommand => new DelegateCommand(AddRoom);
        public System.Windows.Input.ICommand AddNewHouseCommand => new DelegateCommand(AddHouse);

        public ICommand ModifyRoomCommand => new DelegateCommand(ModifyRoom);

        public ICommand DeleteRoomCommand => new DelegateCommand(DeleteRoom);

        public ICommand HouseSelectionChanged => new DelegateCommand<House>(HandleSelectedHouseChange);


        public ObservableCollection<House> Houses { get; set; } = new ObservableCollection<House>();

        private DBManager.DbManagerService dbManager;
        private IEventAggregator eventAggregator;
        private List<Room> roomsInHouse = new List<Room>();
        private Room SelectedRoom;
        private bool loading = false;
        private User currentUser;

        private async void HandleSelectedHouseChange(House newSelection)
        {
            eventAggregator.GetEvent<ResetCanvas>().Publish();
            LoadRooms();
        }

        public MainWindowViewModel(IEventAggregator ea, IContainerProvider container)
        {
            FloorNumber = 0;
            eventAggregator = ea;
            dbManager = container.Resolve<DbManagerService>();
            eventAggregator.GetEvent<OnRoomValidForInsertion>().Subscribe((room) =>
            {
                roomsInHouse.Add(room);
            }, true);
            eventAggregator.GetEvent<OnInsertedHouse>().Subscribe(Houses.Add,true);

            eventAggregator.GetEvent<OnRoomRightClicked>().Subscribe(async (roomId) =>
            {
                if (roomId != -1)
                {
                    SelectedRoom = (await dbManager.GetFiltered<Room>(nameof(Room.Id), roomId.ToString())).First();
                    SelectedRoomWidth = SelectedRoom.Width.ToString();
                    SelectedRoomLength = SelectedRoom.Length.ToString();
                    SelectedRoomName = SelectedRoom.Name;
                    ModifyRoomOptions = Visibility.Visible;
                }
                else
                {
                    SelectedRoom = null;
                    ModifyRoomOptions = Visibility.Collapsed;
                }
            });
            eventAggregator.GetEvent<OnSendUserInformation>().Subscribe((user) => currentUser = user) ;
            eventAggregator.GetEvent<OnRequestUserEmail>().Publish();
            LoadHouseLayouts();
        }


        private async void LoadRooms()
        {
            loading = true;
            roomsInHouse = await dbManager.GetFiltered<Room>(nameof(Room.HouseId), SelectedHouse.Id.ToString());
            var filteredRooms = roomsInHouse.Where(r => r.Floor == FloorNumber);
            foreach (Room room in filteredRooms)
                Application.Current.Dispatcher.Invoke(delegate{
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
                FloorNumber++;
            else if (FloorNumber > 0)
                FloorNumber--;
            if (initialFloor != FloorNumber && !loading)
            {
                eventAggregator.GetEvent<ResetCanvas>().Publish();
                LoadRooms();
            }
        }
        private void AddRoom()
        {
            eventAggregator.GetEvent<OnSendHouseData>().Publish((0, FloorNumber));
            eventAggregator.GetEvent<OnOpenAddRoomWindow>().Publish();
        }

        private void AddHouse()
        {
            eventAggregator.GetEvent<OnOpenAddHouseWindow>().Publish();
        }


        private async void ModifyRoom()
        {
            SelectedRoom.Name = SelectedRoomName;
            SelectedRoom.Width = int.Parse(SelectedRoomWidth);
            SelectedRoom.Length = int.Parse(SelectedRoomLength);

            eventAggregator.GetEvent<OnModifiedRoom>().Publish(SelectedRoom);
            await dbManager.Update(SelectedRoom);
        }

        private async void DeleteRoom()
        {
            await dbManager.Delete(SelectedRoom);
            eventAggregator.GetEvent<OnDeletedRoom>().Publish();
        }

    }

}
