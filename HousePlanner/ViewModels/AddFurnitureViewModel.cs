using DBManager;
using DevExpress.Mvvm;
using HousePlannerCore.Events;
using HousePlannerCore.Models;
using Prism.Events;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Shapes;

namespace HousePlanner.ViewModels
{
    public class AddFurnitureViewModel : AddRoomAndFurnitureBase
    {
        private long roomId;
        public ICommand SelectPictureCommand => new DelegateCommand(SelectPicture);
        public ICommand AddFurnitureCommand => new DelegateCommand(AddFurniture);

        public string ImageTextBlock
        {
            get => GetValue<string>();
            set => SetValue(value);
        }


        public AddFurnitureViewModel(IEventAggregator ea, IContainerProvider container) : base(ea, container) 
        {
            _eventAggregator.GetEvent<OnCloseAddWindowResetTextBoxes>().Subscribe(ResetValues);
            _eventAggregator.GetEvent<OnOpenRoom>().Subscribe(payload =>
            {
                roomId = payload.Id;
            }, true);
            _eventAggregator.GetEvent<OnRightClickSendPoint>().Subscribe(
                point => position = point);
            ResetValues();
        }

        protected override void CheckForErrors()
        {
            base.CheckForErrors();
            if (!File.Exists(ImageTextBlock))
                Errors += "\nImage doesn't exist!";
        }

        protected override void ResetValues()
        {
            base.ResetValues();
            ImageTextBlock = "";
        }

        private async void AddFurniture()
        {
            Errors = "";
            CheckForErrors();
            if (Errors == "")
            {
                var furniture = new Furniture()
                {
                    RoomId = roomId,
                    Name = NameTextBox,
                    Width = int.Parse(WidthTextBox),
                    Length = int.Parse(LengthTextBox),
                    PositionInRoom = position,
                    Picture = System.IO.Path.GetFileName(ImageTextBlock).ToString()
                };

                var folders = ImageTextBlock.Split("\\");
                string path = System.IO.Path.Combine(folders);
                await _dbManager.SaveImageToBlob(path);
                _eventAggregator.GetEvent<OnTryInsertingFurniture>().Publish((furniture, true));
            }
        }

        private void SelectPicture()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.ShowDialog();
            ImageTextBlock = dialog.FileName;
        }

    }
}
