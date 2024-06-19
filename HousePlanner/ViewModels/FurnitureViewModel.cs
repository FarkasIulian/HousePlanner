using DBManager;
using DevExpress.Mvvm;
using DevExpress.XtraEditors;
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

        public Item SelectedItem
        {
            get => GetValue<Item>();
            set => SetValue(value);
        }



        public System.Windows.Input.ICommand AddNewFurnitureCommand => new DelegateCommand(AddFurniture);
        public ICommand ModifyFurnitureCommand => new DelegateCommand(ModifyFurniture);

        public ICommand DeleteFurnitureCommand => new DelegateCommand(DeleteFurniture);

        public ICommand AddItemCommand => new DelegateCommand(AddItem);

        public ICommand DeleteItemCommand => new DelegateCommand(DeleteItem);

        public ICommand ShowItemsCommand => new DelegateCommand(ShowItems);

        public ObservableCollection<Item> ItemsInFurniture { get; set; }

        private Room openedRoom;
        private Furniture selectedFurniture;
        private DBManager.DbManagerService dbManager;
        private IEventAggregator eventAggregator;


        public FurnitureViewModel(IEventAggregator ea, IContainerProvider container, DbManagerService manager)
        {
            eventAggregator = ea;
            eventAggregator.GetEvent<OnOpenRoom>().Subscribe(room =>
            {
                eventAggregator.GetEvent<OnResetFurnitureCanvas>().Publish();
                openedRoom = room;
                LoadFurniture(openedRoom);
                OpenedRoomName = openedRoom.Name;

            });
            dbManager = manager;

            eventAggregator.GetEvent<OnFurnitureRightClicked>().Subscribe(async (furnitureId) =>
            {
                if (furnitureId != -1)
                {
                    ItemsInFurniture = new ObservableCollection<Item>();
                    selectedFurniture = (await dbManager.GetFiltered<Furniture>(nameof(Furniture.Id), furnitureId.ToString())).First();
                    SelectedFurnitureWidth = selectedFurniture.Width.ToString();
                    SelectedFurnitureLength = selectedFurniture.Length.ToString();
                    SelectedFurnitureName = selectedFurniture.Name;
                    ModifyFurnitureOptions = Visibility.Visible;
                    ItemsInFurniture.AddRange(await dbManager.GetFiltered<Item>(nameof(Item.FurnitureId), selectedFurniture.Id.ToString()));
                    RaisePropertiesChanged(nameof(ItemsInFurniture));

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
            int width, length;
            bool parsedWidth = int.TryParse(SelectedFurnitureWidth, out width);
            bool parsedLength = int.TryParse(SelectedFurnitureLength, out length);
            if (parsedWidth && parsedLength)
            {
                selectedFurniture.Name = SelectedFurnitureName;
                selectedFurniture.Width = width;
                selectedFurniture.Length = length;
                eventAggregator.GetEvent<OnModifiedFurniture>().Publish(selectedFurniture);
            }
            else
                XtraMessageBox.Show("Invalid values entered into width or length!", "Modify Furniture", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);

        }

        private async void DeleteFurniture()
        {
            await dbManager.Delete(selectedFurniture);
            foreach (var item in ItemsInFurniture)
                await dbManager.Delete(item);
            eventAggregator.GetEvent<OnDeletedFurniture>().Publish();
        }

        private async void LoadFurniture(Room room)
        {
            var furnitureInRoom = await dbManager.GetFiltered<Furniture>(nameof(Furniture.RoomId), room.Id.ToString());
            foreach (var furniture in furnitureInRoom)
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
                var insertId = await dbManager.Insert(item);
                if (insertId != -1)
                {
                    item.Id = insertId;
                    ItemsInFurniture.Add(item);
                    XtraMessageBox.Show("Added succesfully!", "Items in Furniture", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);

                }

            }
        }


        private async void DeleteItem()
        {
            if (SelectedItem != null)
            {
                if (await dbManager.Delete(SelectedItem))
                {
                    
                    XtraMessageBox.Show("Deleted succesfully!", "Items in Furniture", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                    var index = ItemsInFurniture.IndexOf(SelectedItem);
                    if(index != -1) 
                        ItemsInFurniture.RemoveAt(index);
                    SelectedItem = null;

                }
            }
        }

        private void ShowItems()
        {

            string displayItems = $"Items in {selectedFurniture.Name}:\n";
            foreach (var item in ItemsInFurniture)
                displayItems += $"  - {item.Name}\n";

            XtraMessageBox.Show(displayItems, "Items in Furniture", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);

        }



    }
}
