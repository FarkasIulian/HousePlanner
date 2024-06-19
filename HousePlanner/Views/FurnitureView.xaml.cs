using DBManager;
using DevExpress.Xpf.Core;
using DevExpress.XtraEditors;
using HousePlannerCore.Events;
using HousePlannerCore.Models;
using Prism.Events;
using Prism.Ioc;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
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
        private Image selectedFurniture = null;
        private System.Windows.Point initialDragPoint = new System.Windows.Point();

        public FurnitureView(IEventAggregator ea, IContainerProvider container, DbManagerService db)
        {
            InitializeComponent();
            _eventAggregator = ea;
            _dbManager = db;
            ea.GetEvent<OnTryInsertingFurniture>().Subscribe(payload => AddFurnitureToCanvas(payload.Item1, payload.Item2));
            ea.GetEvent<OnResetFurnitureCanvas>().Subscribe(FurnitureGrid.Children.Clear);
            ea.GetEvent<OnModifiedFurniture>().Subscribe(async (furniture) =>
            {
                var index = FurnitureGrid.Children.IndexOf(selectedFurniture);
                if (index == -1)
                {
                    foreach (var furnitureInRoom in FurnitureGrid.Children)
                    {
                        var image = furnitureInRoom as Image;
                        if (image != null)
                        {
                            if (image.Uid == $"_{furniture.Id}")
                            {
                                index = FurnitureGrid.Children.IndexOf(image);
                                break;
                            }
                        }
                    }
                }
                var newFurniture = new Image()
                {
                    Uid = $"_{furniture.Id}",
                    Name = furniture.Name,
                    Width = furniture.Length,
                    Height = furniture.Width

                };
                newFurniture.Stretch = System.Windows.Media.Stretch.None;
                // if (!IsHittingExistingFurniture(new System.Windows.Point(furniture.PositionInRoom.X, furniture.PositionInRoom.Y), newFurniture))
                {
                    ((Image)FurnitureGrid.Children[index]).Name = furniture.Name;
                    ((Image)FurnitureGrid.Children[index]).Width = furniture.Length;
                    ((Image)FurnitureGrid.Children[index]).Height = furniture.Width;
                    selectedFurniture = newFurniture;
                    await _dbManager.Update(furniture);
                }
            });
            ea.GetEvent<OnDeletedFurniture>().Subscribe(() =>
            {
                FurnitureGrid.Children.Remove(selectedFurniture);
            });
            ea.GetEvent<OnOpenRoom>().Subscribe(room => this.Show());


        }

        private async void AddFurnitureToCanvas(Furniture furniture, bool insertIntoDb = false)
        {
            Image newFurniture = new Image()
            {
                Uid = $"_{furniture.Id}",
                Name = furniture.Name,
                Height = furniture.Width,
                Width = furniture.Length
            };
            newFurniture.Stretch = System.Windows.Media.Stretch.Fill;
            if (!File.Exists(furniture.Picture))
                await _dbManager.DownloadPicture(furniture.Picture);
            newFurniture.Source = new BitmapImage(new Uri(Environment.CurrentDirectory + "\\" + furniture.Picture, UriKind.RelativeOrAbsolute));
            newFurniture.MouseMove += MoveMouse;
            newFurniture.PreviewMouseRightButtonDown += EnableModifyOptions;
            Canvas.SetLeft(newFurniture, furniture.PositionInRoom.X);
            Canvas.SetTop(newFurniture, furniture.PositionInRoom.Y);

            FurnitureGrid.Children.Add(newFurniture);
            if (insertIntoDb)
            {
                furniture.Picture = Path.GetFileName(furniture.Picture);
                var id = await _dbManager.Insert(furniture);
                newFurniture.Uid = $"_{id}";
            }
        }

        private void MoveMouse(object sender, MouseEventArgs e)
        {
            try
            {
                var furniture = sender as Image;

                if (e.MiddleButton == MouseButtonState.Pressed)
                {
                    selectedFurniture = furniture;
                    FurnitureGrid.Children.Remove(selectedFurniture);
                    FurnitureGrid.Children.Add(selectedFurniture);
                    initialDragPoint.X = Canvas.GetLeft(selectedFurniture);
                    initialDragPoint.Y = Canvas.GetTop(selectedFurniture);
                    DragDrop.DoDragDrop(furniture, furniture, DragDropEffects.Move);
                }
            }
            catch (Exception ex)
            {

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

        private bool IsHittingExistingFurniture(System.Windows.Point clickedPoint, Image furnitureToAdd, bool showMessageBox = true)
        {
            Rect hitbox = new Rect(clickedPoint.X, clickedPoint.Y, furnitureToAdd.Width, furnitureToAdd.Height);

            foreach (var child in FurnitureGrid.Children)
            {
                if (child.GetType() == furnitureToAdd.GetType())
                {
                    if (((Image)child).Uid.Equals(furnitureToAdd.Uid)) continue;
                    var rect = new Rect(Canvas.GetLeft(child as Image), Canvas.GetTop(child as Image), ((Image)child).ActualWidth, ((Image)child).ActualHeight);
                    if (hitbox.IntersectsWith(rect))
                    {
                        if (showMessageBox)
                            XtraMessageBox.Show($"Collides with furniture", "Furniture Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                        return true;
                    }

                }

            }
            if (Canvas.GetLeft(furnitureToAdd) + furnitureToAdd.Width > FurnitureGrid.Width || Canvas.GetTop(furnitureToAdd) + furnitureToAdd.Height > FurnitureGrid.Height)
            {
                if (showMessageBox)
                    XtraMessageBox.Show("Out of room layout", "Floor Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return true;
            }

            return false;
        }

        private void FurnitureGrid_Drop(object sender, DragEventArgs e)
        {
            var dropLocation = e.GetPosition(FurnitureGrid);




            Canvas.SetLeft(selectedFurniture, dropLocation.X);
            Canvas.SetTop(selectedFurniture, dropLocation.Y);
            _eventAggregator.GetEvent<OnChangedFurniturePosition>().Publish((int.Parse(selectedFurniture.Uid.Trim('_')), dropLocation.X, dropLocation.Y));



        }

        private void FurnitureGrid_DragOver(object sender, DragEventArgs e)
        {
            var dropLocation = e.GetPosition(FurnitureGrid);
            Canvas.SetLeft(selectedFurniture, dropLocation.X);
            Canvas.SetTop(selectedFurniture, dropLocation.Y);
        }
    }
}
