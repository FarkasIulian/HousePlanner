using DBManager;
using DevExpress.Mvvm;
using DevExpress.XtraEditors;
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
using System.Windows.Controls;
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

        public string NameOfSearchedObject
        {
            get => GetValue<string>();
            set => SetValue(value);
        }


        public ICommand<string> ChangeFloorCommand => new DelegateCommand<string>(ChangeFloor);
        public System.Windows.Input.ICommand AddNewHouseCommand => new DelegateCommand(AddHouse);

        public ICommand HouseSelectionChanged => new DelegateCommand<House>(HandleSelectedHouseChange);

        public ICommand DeleteHouseCommand => new DelegateCommand(DeleteHouse);

        public ICommand SearchCommand => new DelegateCommand<string>(Search);


        public ObservableCollection<House> Houses { get; set; } = new ObservableCollection<House>();

        public object RoomsView { get; set; }


        private bool loading = false;
        private User currentUser;
        private DBManager.DbManagerService dbManager;
        private IEventAggregator eventAggregator;
        private List<Room> roomsInHouse = new List<Room>();
        private async void HandleSelectedHouseChange(House newSelection)
        {
            eventAggregator.GetEvent<OnResetCanvas>().Publish();
            FloorNumber = 0;
            LoadRooms();
        }

        public MainWindowViewModel(IEventAggregator ea, IContainerProvider container, DbManagerService db)
        {
            FloorNumber = 0;
            eventAggregator = ea;
            RoomsView = container.Resolve<RoomsView>();
            dbManager = db;
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
            if (SelectedHouse != null)
            {
                eventAggregator.GetEvent<OnSendHouseData>().Publish((SelectedHouse.Id, FloorNumber));

                roomsInHouse = await dbManager.GetFiltered<Room>(nameof(Room.HouseId), SelectedHouse.Id.ToString());
                var filteredRooms = roomsInHouse.Where(r => r.Floor == FloorNumber);
                foreach (Room room in filteredRooms)
                    Application.Current.Dispatcher.Invoke(delegate
                    {
                        eventAggregator.GetEvent<OnTryInsertingRoom>().Publish((room, false));
                    });
            }
            else
                eventAggregator.GetEvent<OnSendHouseData>().Publish((-1, -1));

            loading = false;

        }

        private async Task LoadHouseLayouts()
        {
            var houses = await dbManager.GetFiltered<House>(nameof(House.OwnerEmail), currentUser.Email);
            Houses.AddRange(houses);
            if (houses.Count() == 0)
            {
                XtraMessageBox.Show("Add new house layout to add rooms!\n Or select an existing layout!", "Add house", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                eventAggregator.GetEvent<OnSendHouseData>().Publish((-1, -1));

            }
        }

        private void ChangeFloor(string floorAction)
        {
            if (SelectedHouse != null)
            {
                var initialFloor = FloorNumber;
                if (floorAction == "Up")
                    if (FloorNumber < SelectedHouse.NumberOfFloors)
                        FloorNumber++;
                    else XtraMessageBox.Show("Reached last floor");
                else if (FloorNumber > 0)
                    FloorNumber--;
                if (initialFloor != FloorNumber && !loading)
                {
                    eventAggregator.GetEvent<OnResetCanvas>().Publish();
                    LoadRooms();
                }
                else
                    eventAggregator.GetEvent<OnSendHouseData>().Publish((-1, -1));
            }
        }


        private void AddHouse()
        {
            eventAggregator.GetEvent<OnOpenAddHouseWindow>().Publish();
        }


        private async void DeleteHouse()
        {
            if (SelectedHouse != null)
            {
                foreach (var room in roomsInHouse)
                {
                    var furnitureList = await dbManager.GetFiltered<Furniture>(nameof(Furniture.RoomId), room.Id.ToString());
                    foreach (var furniture in furnitureList)
                    {
                        var items = await dbManager.GetFiltered<Item>(nameof(Item.FurnitureId), furniture.Id.ToString());
                        foreach (var item in items)
                            await dbManager.Delete(item);
                        await dbManager.Delete(furniture);
                    }
                    await dbManager.Delete(room);
                }

                await dbManager.Delete(SelectedHouse);
                Houses.Remove(SelectedHouse);
                eventAggregator.GetEvent<OnResetCanvas>().Publish();
            }
        }

        private async void Search(string type)
        {
            if (string.IsNullOrEmpty(NameOfSearchedObject))
                return;
            var classType = Type.GetType("HousePlannerCore.Models." + type +", HousePlannerCore");
            string displayFoundObjects = "";
            if(classType == typeof(Room))
            {
                var foundRooms = roomsInHouse.Where(r => r.Name.Equals(NameOfSearchedObject));
                displayFoundObjects = $"Found {foundRooms.Count()} that match provided name!\n";
                foreach(var room in foundRooms)
                    displayFoundObjects += $"Name: {room.Name} - Floor: {room.Floor}\n";
            }
            else if(classType == typeof(Furniture))
            {
                var foundFurniture = await dbManager.GetFiltered<Furniture>(nameof(Furniture.Name), NameOfSearchedObject);
                displayFoundObjects = $"Found {foundFurniture.Count()} that match provided name!\n";
                foreach (var furniture in foundFurniture)
                {
                    var room = roomsInHouse.Where(r => r.Id == furniture.RoomId).First();
                    displayFoundObjects += $"Name: {furniture.Name}  - Room Name: {room.Name} - Floor: {room.Floor}\n";
                }
            }
            else
            {
                var foundItems = await dbManager.GetFiltered<Item>(nameof(Item.Name), NameOfSearchedObject);
                displayFoundObjects = $"Found {foundItems.Count()} that match provided name!\n";
                foreach(var item in foundItems)
                {
                    var furniture = (await dbManager.GetFiltered<Furniture>(nameof(Furniture.Id), item.FurnitureId.ToString())).First();
                    var room = (await dbManager.GetFiltered<Room>(nameof(Room.Id), furniture.RoomId.ToString())).First();
                    displayFoundObjects += $"Name: {item.Name}  - Furniture Name: {furniture.Name}  - Room Name: {room.Name} - Floor: {room.Floor}\n";
                }
            }
            XtraMessageBox.Show(displayFoundObjects, "Search Results", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
        }


    }
}
