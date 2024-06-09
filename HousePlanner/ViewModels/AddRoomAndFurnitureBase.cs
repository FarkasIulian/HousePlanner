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

        public AddRoomAndFurnitureBase(IEventAggregator ea,IContainerProvider container,DbManagerService db)
        {
            _dbManager = db;
            _eventAggregator = ea;
        }

        protected virtual void ResetValues()
        {
            NameTextBox = "";
            WidthTextBox = "";
            LengthTextBox = "";
            Errors = "";
        }
        protected virtual void CheckForErrors()
        {
            int parsed;
            if (!int.TryParse(WidthTextBox, out parsed))
                Errors += "Width needs to be a number\n";
            if (!int.TryParse(LengthTextBox, out parsed))
                Errors += "Length needs to be a number\n";
            if (NameTextBox == "" || WidthTextBox == "" || LengthTextBox == "")
                Errors += "Fill in all fields";
            
        }
    }
}
