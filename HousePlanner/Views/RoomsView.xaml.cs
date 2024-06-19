    using DBManager;
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
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HousePlanner.Views
{
    /// <summary>
    /// Interaction logic for RoomsView.xaml
    /// </summary>
    public partial class RoomsView : UserControl
    {
        private Point clickedPoint;
        private IEventAggregator _eventAggregator;
        private DbManagerService _dbManager;
        private Button selectedButton;
        private Point initialDragPoint = new Point();
        public RoomsView(IEventAggregator ea,IContainerProvider container,DbManagerService db)
        {
            InitializeComponent();
            _eventAggregator = ea;
            _dbManager = db;
            ea.GetEvent<OnTryInsertingRoom>().Subscribe(payload => AddRoomToCanvas(payload.Item1, payload.Item2));
            ea.GetEvent<OnResetCanvas>().Subscribe(RoomsGrid.Children.Clear);
            ea.GetEvent<OnModifiedRoom>().Subscribe(async (room) =>
            {
                var index = RoomsGrid.Children.IndexOf(selectedButton);
                if (index == -1)
                {
                    foreach (var roomInHouse in RoomsGrid.Children)
                    {
                        var button = roomInHouse as Button;
                        if (button != null)
                        {
                            if (button.Name == $"_{room.Id}")
                            {
                                index = RoomsGrid.Children.IndexOf(button);
                                break;
                            }
                        }
                    }
                }

                var newButton = new Button()
                {
                    Name = $"_{room.Id}",
                    Content = room.Name,
                    Width = room.Length,
                    Height = room.Width
                };
                if (!IsHittingExistingRoom(new Point(room.PositionInHouse.X, room.PositionInHouse.Y), newButton))
                {
                    ((Button)RoomsGrid.Children[index]).Content = room.Name;
                    ((Button)RoomsGrid.Children[index]).Width = room.Length;
                    ((Button)RoomsGrid.Children[index]).Height = room.Width;
                    selectedButton = newButton;
                    await _dbManager.Update(room);
                }
            });
            ea.GetEvent<OnDeletedRoom>().Subscribe(() =>
            {
                if(RoomsGrid.Children.Contains(selectedButton))
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

            newRoom.MouseMove += MoveMouse;
            newRoom.PreviewMouseRightButtonDown += EnableModifyOptions;
            Canvas.SetLeft(newRoom, room.PositionInHouse.X);
            Canvas.SetTop(newRoom, room.PositionInHouse.Y);

            if (!IsHittingExistingRoom(new Point(room.PositionInHouse.X, room.PositionInHouse.Y), newRoom))
            {
                RoomsGrid.Children.Add(newRoom);
                if (insertIntoDb)
                {
                    var id = await _dbManager.Insert(room);
                    room.Id = id;
                    _eventAggregator.GetEvent<OnRoomValidForInsertion>().Publish(room);
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
            if (!IsHittingExistingRoom(clickedPoint, hitboxButton, false))
                _eventAggregator.GetEvent<OnRoomRightClicked>().Publish(-1);
        }






        private bool IsHittingExistingRoom(Point clickedPoint, Button buttonToAdd, bool showMessageBox = true)
        {
            Rect hitbox = new Rect(clickedPoint.X, clickedPoint.Y, buttonToAdd.Width, buttonToAdd.Height);

            foreach (var child in RoomsGrid.Children)
            {
                if (child.GetType() == buttonToAdd.GetType())
                {
                    if (((Button)child).Name.Equals(buttonToAdd.Name)) continue;
                    var rect = new Rect(Canvas.GetLeft(child as Button), Canvas.GetTop(child as Button), ((Button)child).ActualWidth, ((Button)child).ActualHeight);
                    if (hitbox.IntersectsWith(rect))
                    {
                        if (showMessageBox)
                            XtraMessageBox.Show($"Collides with room: {((Button)child).Content}", "Room Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                        return true;
                    }

                }

            }
            if (Canvas.GetLeft(buttonToAdd) + buttonToAdd.Width > RoomsGrid.Width || Canvas.GetTop(buttonToAdd) + buttonToAdd.Height > RoomsGrid.Height)
            {
                if (showMessageBox)
                    XtraMessageBox.Show("Out of house layout", "Room Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return true;
            }

            return false;
        }

        private void RoomsGrid_Drop(object sender, DragEventArgs e)
        {
            var dropLocation = e.GetPosition(RoomsGrid);
            if (!IsHittingExistingRoom(dropLocation, selectedButton))
            {
                Canvas.SetLeft(selectedButton, dropLocation.X);
                Canvas.SetTop(selectedButton, dropLocation.Y);
                _eventAggregator.GetEvent<OnChangedRoomPosition>().Publish((int.Parse(selectedButton.Name.Trim('_')), dropLocation.X, dropLocation.Y));
            }
            else
            {
                Canvas.SetLeft(selectedButton, initialDragPoint.X);
                Canvas.SetTop(selectedButton, initialDragPoint.Y);
            }
        }
        private void MoveMouse(object sender, MouseEventArgs e)
        {
            var button = sender as Button;

            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                selectedButton = button;
                initialDragPoint.X = Canvas.GetLeft(selectedButton);
                initialDragPoint.Y = Canvas.GetTop(selectedButton);
                DragDrop.DoDragDrop(button, button, DragDropEffects.Move);
            }
        }
        private void RoomsGrid_DragOver(object sender, DragEventArgs e)
        {
            var dropLocation = e.GetPosition(RoomsGrid);
            Canvas.SetLeft(selectedButton, dropLocation.X);
            Canvas.SetTop(selectedButton, dropLocation.Y);
        }

        
    }
}
