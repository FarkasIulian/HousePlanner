using DBManager;
using DevExpress.Xpf.Core;
using HousePlannerCore.Events;
using HousePlannerCore.Models;
using Prism.Events;
using Prism.Ioc;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HousePlanner.Views
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MainWindowView : UserControl
    {

        private Point clickedPoint;
        private IEventAggregator _eventAggregator;
        private DbManagerService _dbManager;
        private Button selectedButton;

        public MainWindowView(IEventAggregator ea, IContainerProvider container)
        {
            InitializeComponent();
            _eventAggregator = ea;
            _dbManager = container.Resolve<DbManagerService>();
            ea.GetEvent<OnTryInsertingRoom>().Subscribe(payload => AddRoomToCanvas(payload.Item1, payload.Item2));
            ea.GetEvent<ResetCanvas>().Subscribe(RoomsGrid.Children.Clear);
            ea.GetEvent<OnModifiedRoom>().Subscribe((room) =>
            {
                var index = RoomsGrid.Children.IndexOf(selectedButton);
                ((Button)RoomsGrid.Children[index]).Content = room.Name;
                ((Button)RoomsGrid.Children[index]).Width = room.Length;
                ((Button)RoomsGrid.Children[index]).Height = room.Width;
            });
            ea.GetEvent<OnDeletedRoom>().Subscribe(() =>
            {
                RoomsGrid.Children.Remove(selectedButton);
            });
        }

        private async void AddRoomToCanvas(Room room, bool insertIntoDb = false)
        {
            Button newRoom = new Button()
            {
                Name = $"_{room.Id}",
                Content = room.Name,
                FontSize = 16,
                Height = room.Width,
                Width = room.Length
            };
            newRoom.Click += X_Click;
            newRoom.PreviewMouseRightButtonDown += EnableModifyOptions;
            Canvas.SetLeft(newRoom, room.PositionInHouse.X);
            Canvas.SetTop(newRoom, room.PositionInHouse.Y);

            if (!IsHittingExistingRoom(new Point(room.PositionInHouse.X, room.PositionInHouse.Y), newRoom))
            {
                RoomsGrid.Children.Add(newRoom);
                if (insertIntoDb)
                {
                    _eventAggregator.GetEvent<OnRoomValidForInsertion>().Publish(room);
                    var id = await _dbManager.Insert(room);
                    newRoom.Name = $"_{id}";

                }
            }


        }

        private void EnableModifyOptions(object sender, MouseButtonEventArgs e)
        {
            selectedButton = sender as Button;
            _eventAggregator.GetEvent<OnRoomRightClicked>().Publish(int.Parse(selectedButton.Name.Trim('_')));
        }

        private void RoomsGrid_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            clickedPoint = e.GetPosition(RoomsGrid);

            var point = new System.Drawing.Point((int)clickedPoint.X, (int)clickedPoint.Y); // convert point to System.Drawing.Point

            _eventAggregator.GetEvent<OnRightClickSendPoint>().Publish(point);
            var hitboxButton = new Button() { Width = 1, Height = 1 };
            Canvas.SetLeft(hitboxButton, clickedPoint.X);
            Canvas.SetTop(hitboxButton, clickedPoint.Y);
            if (!IsHittingExistingRoom(clickedPoint, hitboxButton,false))
                _eventAggregator.GetEvent<OnRoomRightClicked>().Publish(-1);
        }




        private void X_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var ceva = button.GetPosition(RoomsGrid);
            MessageBox.Show(ceva.X + " " + ceva.Y);


        }


        private bool IsHittingExistingRoom(Point clickedPoint, Button buttonToAdd,bool showMessageBox = true)
        {
            Rect hitbox = new Rect(clickedPoint.X, clickedPoint.Y, buttonToAdd.Width, buttonToAdd.Height);

            foreach (var child in RoomsGrid.Children)
            {
                if (child.GetType() == buttonToAdd.GetType())
                {

                    var rect = new Rect(Canvas.GetLeft(child as Button), Canvas.GetTop(child as Button), ((Button)child).ActualWidth, ((Button)child).ActualHeight);
                    if (hitbox.IntersectsWith(rect))
                    {
                        if (showMessageBox)
                            MessageBox.Show($"Collides with room: {((Button)child).Content}","Room Error",MessageBoxButton.OK,MessageBoxImage.Error);
                        return true;
                    }

                }

            }
            if (Canvas.GetLeft(buttonToAdd) + buttonToAdd.Width > RoomsGrid.Width || Canvas.GetTop(buttonToAdd) + buttonToAdd.Height > RoomsGrid.Height)
            {
                if (showMessageBox)
                    MessageBox.Show("Out of house layout", "Room Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return true;
            }

            return false;
        }

    }
}
