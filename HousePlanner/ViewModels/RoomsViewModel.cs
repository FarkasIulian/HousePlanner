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
        private DBManager.DbManagerService dbManager;
        private IEventAggregator eventAggregator;

        public RoomsViewModel(IEventAggregator ea, IContainerProvider container) 
        {
            eventAggregator = ea;
            dbManager = container.Resolve<DbManagerService>();
            
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
            SelectedRoom.Name = SelectedRoomName;
            SelectedRoom.Width = int.Parse(SelectedRoomWidth);
            SelectedRoom.Length = int.Parse(SelectedRoomLength);
            eventAggregator.GetEvent<OnModifiedRoom>().Publish(SelectedRoom);
        }

        private async void DeleteRoom()
        {
            await dbManager.Delete(SelectedRoom);
            eventAggregator.GetEvent<OnDeletedRoom>().Publish();
        }

        private void OpenRoom()
        {
            eventAggregator.GetEvent<OnOpenRoom>().Publish(SelectedRoom);
        }


    }
}
