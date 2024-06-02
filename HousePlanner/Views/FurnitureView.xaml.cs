using DBManager;
using DevExpress.Xpf.Core;
using HousePlannerCore.Events;
using HousePlannerCore.Models;
using Prism.Events;
using Prism.Ioc;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Image = System.Windows.Controls.Image;

namespace HousePlanner.Views
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class FurnitureView : ThemedWindow
    {

        private System.Windows.Point clickedPoint;
        private IEventAggregator _eventAggregator;
        private DbManagerService _dbManager;
        private System.Windows.Controls.Image selectedFurniture;
        private System.Windows.Point initialDragPoint = new System.Windows.Point();

        public FurnitureView(IEventAggregator ea, IContainerProvider container)
        {
            InitializeComponent();
            _eventAggregator = ea;
            _dbManager = container.Resolve<DbManagerService>();
            ea.GetEvent<OnTryInsertingFurniture>().Subscribe(payload => AddRoomToCanvas(payload.Item1, payload.Item2));
            ea.GetEvent<ResetFurnitureCanvas>().Subscribe(FurnitureGrid.Children.Clear);
            ea.GetEvent<OnModifiedFurniture>().Subscribe(async (furniture) =>
            {
                var index = FurnitureGrid.Children.IndexOf(selectedFurniture);
                var newFurniture = new System.Windows.Controls.Image()
                {
                    Name = furniture.Name,
                    Width = furniture.Length,
                    Height = furniture.Width
                };
                if (!IsHittingExistingFurniture(new System.Windows.Point(furniture.PositionInRoom.X, furniture.PositionInRoom.Y), newFurniture))
                {
                    ((Button)FurnitureGrid.Children[index]).Content = furniture.Name;
                    ((Button)FurnitureGrid.Children[index]).Width = furniture.Length;
                    ((Button)FurnitureGrid.Children[index]).Height = furniture.Width;
                    selectedFurniture = newFurniture;
                    await _dbManager.Update(furniture);
                }
            });
            ea.GetEvent<OnDeletedFurniture>().Subscribe(() =>
            {
                FurnitureGrid.Children.Remove(selectedFurniture);
            });
            ea.GetEvent<OnOpenRoom>().Subscribe(room => this.ShowDialog());
        }

        private async void AddRoomToCanvas(Furniture furniture, bool insertIntoDb = false)
        {
            Image newFurniture = new Image()
            {
                Name = furniture.Name,
                Source = new BitmapImage(new Uri(furniture.Picture, UriKind.Relative)),
                Height = furniture.Width,
                Width = furniture.Length
            };
            Button testingBtn = new Button()
            {
                Width = 200,
                Height = 200
            };
            newFurniture.MouseMove += MoveMouse;
            newFurniture.PreviewMouseRightButtonDown += EnableModifyOptions;
            Canvas.SetLeft(newFurniture, furniture.PositionInRoom.X);
            Canvas.SetTop(newFurniture, furniture.PositionInRoom.Y);

            if (!IsHittingExistingFurniture(new System.Windows.Point(furniture.PositionInRoom.X, furniture.PositionInRoom.Y), newFurniture))
            {
                FurnitureGrid.Children.Add(newFurniture);
                FurnitureGrid.Children.Add(testingBtn);
                if (insertIntoDb)
                {
                    _eventAggregator.GetEvent<OnFurnitureValidForInsertion>().Publish(furniture);
                    var id = await _dbManager.Insert(furniture);
                    newFurniture.Uid = $"_{id}";

                } 
            }


        }

        private void MoveMouse(object sender, MouseEventArgs e)
        {
            var furniture = sender as Image;

            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                selectedFurniture = furniture;
                initialDragPoint.X = Canvas.GetLeft(selectedFurniture);
                initialDragPoint.Y = Canvas.GetTop(selectedFurniture);
                DragDrop.DoDragDrop(furniture, furniture, DragDropEffects.Move);
            }
        }

        private void EnableModifyOptions(object sender, MouseButtonEventArgs e)
        {
            selectedFurniture = sender as Image;
            _eventAggregator.GetEvent<OnFurnitureRightClicked>().Publish(int.Parse(selectedFurniture.Uid.Trim('_')));
        }

        private void FurnitureGrid_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            clickedPoint = e.GetPosition(FurnitureGrid);

            var point = new System.Drawing.Point((int)clickedPoint.X, (int)clickedPoint.Y); // convert point to System.Drawing.Point

            _eventAggregator.GetEvent<OnRightClickSendPoint>().Publish(point);
            var hitboxImage = new Image() { Width = 1, Height = 1 };
            Canvas.SetLeft(hitboxImage, clickedPoint.X);
            Canvas.SetTop(hitboxImage, clickedPoint.Y);
            if (!IsHittingExistingFurniture(clickedPoint, hitboxImage, false))
                _eventAggregator.GetEvent<OnFurnitureRightClicked>().Publish(-1);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }




        private bool IsHittingExistingFurniture(System.Windows.Point clickedPoint, Image furnitureToAdd,bool showMessageBox = true)
        {
            Rect hitbox = new Rect(clickedPoint.X, clickedPoint.Y, furnitureToAdd.Width, furnitureToAdd.Height);

            foreach (var child in FurnitureGrid.Children)
            {
                if (child.GetType() == furnitureToAdd.GetType())
                {
                    if (((Image)child).Name.Equals(furnitureToAdd.Name)) continue;
                    var rect = new Rect(Canvas.GetLeft(child as Image), Canvas.GetTop(child as Image), ((Image)child).ActualWidth, ((Image)child).ActualHeight);
                    if (hitbox.IntersectsWith(rect))
                    {
                        if (showMessageBox)
                            MessageBox.Show($"Collides with furniture","Furniture Error",MessageBoxButton.OK,MessageBoxImage.Error);
                        return true;
                    }

                }

            }
            if (Canvas.GetLeft(furnitureToAdd) + furnitureToAdd.Width > FurnitureGrid.Width || Canvas.GetTop(furnitureToAdd) + furnitureToAdd.Height > FurnitureGrid.Height)
            {
                if (showMessageBox)
                    MessageBox.Show("Out of room layout", "Floor Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return true;
            }

            return false;
        }

        private void FurnitureGrid_Drop(object sender, DragEventArgs e)
        {
            var dropLocation = e.GetPosition(FurnitureGrid);

            if (!IsHittingExistingFurniture(dropLocation, selectedFurniture))
            {
                Canvas.SetLeft(selectedFurniture, dropLocation.X);
                Canvas.SetTop(selectedFurniture, dropLocation.Y);
                _eventAggregator.GetEvent<OnChangedFurniturePosition>().Publish((int.Parse(selectedFurniture.Name.Trim('_')),dropLocation.X,dropLocation.Y));
            }
            else
            {

                Canvas.SetLeft(selectedFurniture, initialDragPoint.X);
                Canvas.SetTop(selectedFurniture, initialDragPoint.Y);
            }


        }

        private void FurnitureGrid_DragOver(object sender, DragEventArgs e)
        {
            var dropLocation = e.GetPosition(FurnitureGrid);
            Canvas.SetLeft(selectedFurniture, dropLocation.X);
            Canvas.SetTop(selectedFurniture, dropLocation.Y);
        }
    }
}
