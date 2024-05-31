using DevExpress.Xpf.Core;
using HousePlannerCore.Events;
using HousePlannerCore.Models;
using Prism.Events;
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

        public MainWindowView(IEventAggregator ea)
        {
            InitializeComponent();
            _eventAggregator = ea;
            ea.GetEvent<OnInsertedRoom>().Subscribe(AddRoomToCanvas);

        }

        private void AddRoomToCanvas(Room room)
        {
            Button newRoom = new Button()
            {
                Content = room.Name,
                FontSize = 16,
                Height = room.Width,
                Width = room.Length
            };
            newRoom.Click += X_Click;
            Canvas.SetLeft(newRoom, room.PositionInHouse.X);
            Canvas.SetTop(newRoom, room.PositionInHouse.Y);
            
            if (!IsHittingExistingRoom(new Point(room.PositionInHouse.X, room.PositionInHouse.Y), newRoom))
                RoomsGrid.Children.Add(newRoom);

        }


        private void RoomsGrid_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            clickedPoint = e.GetPosition(RoomsGrid);
            
            var point = new System.Drawing.Point((int)clickedPoint.X, (int)clickedPoint.Y); // convert point to System.Drawing.Point

            _eventAggregator.GetEvent<OnRightClickSendPoint>().Publish(point);
        }

        private void X_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var ceva = button.GetPosition(RoomsGrid);
            MessageBox.Show(ceva.X + " " + ceva.Y);


        }


        private bool IsHittingExistingRoom(Point clickedPoint, Button buttonToAdd)
        {
            Rect hitbox = new Rect(clickedPoint.X, clickedPoint.Y, buttonToAdd.Width, buttonToAdd.Height);

            foreach (var child in RoomsGrid.Children)
            {
                if (child.GetType() == buttonToAdd.GetType())
                {
                    
                    var rect = new Rect(Canvas.GetLeft(child as Button), Canvas.GetTop(child as Button), ((Button)child).ActualWidth, ((Button)child).ActualHeight);
                    if (hitbox.IntersectsWith(rect))
                    {
                        MessageBox.Show("Loveste");
                        return true;
                    }
                    if (hitbox.X + rect.Width > RoomsGrid.Width || hitbox.Y + rect.Height > RoomsGrid.Height)
                    {
                        MessageBox.Show("Out of grid");
                        return true;
                    }
                }
                
            }
            return false;
        }

    }
}
