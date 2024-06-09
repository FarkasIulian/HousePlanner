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
using System.Windows.Controls;
using System.Windows.Input;

namespace HousePlanner.ViewModels
{
    public class FurnitureViewModel : BindableBase
    {
        public string SelectedFurnitureName
        {
            get => GetValue<string>();
            set => SetValue(value);
        }
        public string SelectedFurnitureWidth
        {
            get => GetValue<string>();
            set => SetValue(value);
        }
        public string SelectedFurnitureLength
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public string OpenedRoomName
        {
            get => GetValue<string>();
            set => SetValue(value);
        }
        public Image SelectedFurniturePicture
        {
            get => GetValue<Image>();
            set => SetValue(value);
        }
        public Visibility ModifyFurnitureOptions
        {
            get => GetValue<Visibility>();
            set => SetValue(value);
        }

        public string ItemName
        {
            get => GetValue<string>();
            set => SetValue(value); 
        }
        


        public System.Windows.Input.ICommand AddNewFurnitureCommand => new DelegateCommand(AddFurniture);
        public ICommand ModifyFurnitureCommand => new DelegateCommand(ModifyFurniture);

        public ICommand DeleteFurnitureCommand => new DelegateCommand(DeleteFurniture);

        public ICommand AddItemCommand => new DelegateCommand(AddItem);

        public ICommand ShowItemsCommand => new DelegateCommand(ShowItems);

        private Room openedRoom;
        private Furniture selectedFurniture;
        private DBManager.DbManagerService dbManager;
        private IEventAggregator eventAggregator;

        public FurnitureViewModel(IEventAggregator ea, IContainerProvider container)
        {
            eventAggregator = ea;
            eventAggregator.GetEvent<OnOpenRoom>().Subscribe(room => 
            {
                eventAggregator.GetEvent<OnResetFurnitureCanvas>().Publish();
                openedRoom = room;
                LoadFurniture(openedRoom);
                OpenedRoomName = openedRoom.Name;

            });
            dbManager = container.Resolve<DbManagerService>();

            eventAggregator.GetEvent<OnFurnitureRightClicked>().Subscribe(async (furnitureId) =>
            {
                if (furnitureId != -1)
                {
                    selectedFurniture = (await dbManager.GetFiltered<Furniture>(nameof(Furniture.Id), furnitureId.ToString())).First();
                    SelectedFurnitureWidth = selectedFurniture.Width.ToString();
                    SelectedFurnitureLength = selectedFurniture.Length.ToString();
                    SelectedFurnitureName = selectedFurniture.Name;
                    ModifyFurnitureOptions = Visibility.Visible;
                }
                else
                {
                    selectedFurniture = null;
                    ModifyFurnitureOptions = Visibility.Collapsed;
                }
            });

            eventAggregator.GetEvent<OnChangedFurniturePosition>().Subscribe(async (payload) =>
            {
                var movedFurniture = (await dbManager.GetFiltered<Furniture>(nameof(Furniture.Id), payload.Item1.ToString())).FirstOrDefault();
                if (movedFurniture != null)
                {
                    movedFurniture.PositionInRoom = new System.Drawing.Point((int)payload.Item2, (int)payload.Item3);
                    await dbManager.Update(movedFurniture);
                }

            });



        }

        private void AddFurniture()
        {
            eventAggregator.GetEvent<OnOpenAddFurnitureWindow>().Publish(openedRoom);
        }
        private async void ModifyFurniture()
        {

            selectedFurniture.Name = SelectedFurnitureName;
            selectedFurniture.Width = int.Parse(SelectedFurnitureWidth);
            selectedFurniture.Length = int.Parse(SelectedFurnitureLength);
            eventAggregator.GetEvent<OnModifiedFurniture>().Publish(selectedFurniture);
        }

        private async void DeleteFurniture()
        {
            await dbManager.Delete(selectedFurniture);
            //await dbManager.DeleteFromBlob(selectedFurniture.Picture);
            eventAggregator.GetEvent<OnDeletedFurniture>().Publish();
        }

        private async void LoadFurniture(Room room)
        {
            var furnitureInRoom = await dbManager.GetFiltered<Furniture>(nameof(Furniture.RoomId), room.Id.ToString());
            foreach(var furniture in furnitureInRoom)
            {
                Application.Current.Dispatcher.Invoke(delegate
                {
                    eventAggregator.GetEvent<OnTryInsertingFurniture>().Publish((furniture, false));
                });
            }

        }

        private async void AddItem()
        {
            if (!string.IsNullOrEmpty(ItemName))
            {
                var item = new Item()
                {
                    Name = ItemName,
                    FurnitureId = selectedFurniture.Id
                };

                await dbManager.Insert(item);
            }
        }

        private async void ShowItems()
        {
            var items = await dbManager.GetFiltered<Item>(nameof(Item.FurnitureId), selectedFurniture.Id.ToString());

        }



    }
}
