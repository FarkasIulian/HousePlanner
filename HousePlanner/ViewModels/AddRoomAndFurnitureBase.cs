using DBManager;
using DevExpress.Mvvm;
using Prism.Events;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HousePlanner.ViewModels
{
    public class AddRoomAndFurnitureBase : BindableBase
    {
        public string NameTextBox
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public string WidthTextBox
        {
            get => GetValue<string>();
            set => SetValue(value);
        }
        public string LengthTextBox
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public string Errors
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        protected IEventAggregator _eventAggregator;
        protected DbManagerService _dbManager;
        protected System.Drawing.Point position;

        public AddRoomAndFurnitureBase(IEventAggregator ea,IContainerProvider container)
        {
            _dbManager = container.Resolve<DBManager.DbManagerService>();
            _eventAggregator = ea;
        }

        protected void ResetValues()
        {
            NameTextBox = "";
            WidthTextBox = "";
            LengthTextBox = "";
            Errors = "";
        }

    }
}
