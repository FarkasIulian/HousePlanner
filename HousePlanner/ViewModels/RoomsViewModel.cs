using DBManager;
using DevExpress.Mvvm;
using DevExpress.XtraEditors;
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
    public class RoomsViewModel : BindableBase
    {

        public System.Windows.Input.ICommand AddNewRoomCommand => new DelegateCommand(AddRoom);
        public ICommand ModifyRoomCommand => new DelegateCommand(ModifyRoom);

        public ICommand DeleteRoomCommand => new DelegateCommand(DeleteRoom);

        public ICommand OpenRoomCommand => new DelegateCommand(OpenRoom);

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
        public Visibility ModifyRoomOptions
        {
            get => GetValue<Visibility>();
            set => SetValue(value);
        }

        public Visibility CanAddRoom
        {
            get => GetValue<Visibility>();
            set => SetValue(value);
        }

        private Room SelectedRoom;
        private DbManagerService dbManager;
        private IEventAggregator eventAggregator;

        public RoomsViewModel(IEventAggregator ea, IContainerProvider container,DbManagerService db) 
        {
            eventAggregator = ea;
            dbManager = db;
            
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
            eventAggregator.GetEvent<OnSendHouseData>().Subscribe((payload) =>
            {
                if (payload.Item1 == -1) 
                    CanAddRoom = Visibility.Collapsed;
                else CanAddRoom = Visibility.Visible;

            });
            eventAggregator.GetEvent<OnChangedRoomPosition>().Subscribe(async (payload) =>
            {
                var movedRoom = (await dbManager.GetFiltered<Room>(nameof(Room.Id), payload.Item1.ToString())).FirstOrDefault();
                if (movedRoom != null)
                {
                    movedRoom.PositionInHouse = new System.Drawing.Point((int)payload.Item2, (int)payload.Item3);
                    await dbManager.Update(movedRoom);
                }

            });
        }
        private void AddRoom()
        {
            eventAggregator.GetEvent<OnOpenAddRoomWindow>().Publish();
        }
        private void ModifyRoom()
        {
            int width, length;
            bool parsedWidth = int.TryParse(SelectedRoomWidth, out width);
            bool parsedLength = int.TryParse(SelectedRoomLength, out length);
            if (parsedWidth && parsedLength)
            {
                SelectedRoom.Name = SelectedRoomName;
                SelectedRoom.Width = width;
                SelectedRoom.Length = length;
                eventAggregator.GetEvent<OnModifiedRoom>().Publish(SelectedRoom);
            }
            else
                XtraMessageBox.Show("Invalid values entered into width or length!", "Modify Room", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);

        }

        private async void DeleteRoom()
        {
            await dbManager.Delete(SelectedRoom);
            var furnitureList = await dbManager.GetFiltered<Furniture>(nameof(Furniture.RoomId), SelectedRoom.Id.ToString());
            foreach (var furniture in furnitureList)
            {
                var items = await dbManager.GetFiltered<Item>(nameof(Item.FurnitureId), furniture.Id.ToString());
                foreach (var item in items)
                    await dbManager.Delete(item);
                await dbManager.Delete(furniture);
            }
            eventAggregator.GetEvent<OnDeletedRoom>().Publish();
        }

        private void OpenRoom()
        {
            eventAggregator.GetEvent<OnOpenRoom>().Publish(SelectedRoom);
        }


    }
}
