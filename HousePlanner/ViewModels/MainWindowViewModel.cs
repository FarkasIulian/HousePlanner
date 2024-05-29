using DevExpress.Mvvm;
using Prism.Events;
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


        private IEventAggregator eventAggregator;
        

        public MainWindowViewModel(IEventAggregator ea)
        {
            FloorNumber = 0;
            eventAggregator = ea;
        }

        private void ChangeFloor(string floorAction)
        {
            if (floorAction == "Up")
                FloorNumber++;
            else FloorNumber--;
        }
        private void AddRoom()
        {
            MessageBox.Show("Ceva");
        }
    }
}
