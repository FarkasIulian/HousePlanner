using DevExpress.Xpf.Core;
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
using System.Windows.Shapes;

namespace HousePlanner.Views
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MainWindowView : UserControl
    {

        private Point clickedPoint;

        public MainWindowView()
        {
            InitializeComponent();
        }

        private void RoomsGrid_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            clickedPoint = e.GetPosition(RoomsGrid);
            var ceva = e.GetPosition(RoomsGrid);
            Button x = new Button()
            {
                Content = "Testing",
                FontSize = 16,
                Height = 100,
                Width = 200
            };
           
            x.Click += X_Click;
            Canvas.SetLeft(x, ceva.X);
            Canvas.SetTop(x, ceva.Y);

           
            if(!IsHittingExistingRoom(ceva,x))
                RoomsGrid.Children.Add(x);

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

            foreach (Button child in RoomsGrid.Children)
            {
                var rect = new Rect(Canvas.GetLeft(child), Canvas.GetTop(child), child.ActualWidth, child.ActualHeight);
                if (hitbox.IntersectsWith(rect))
                {
                    MessageBox.Show("Loveste");
                    return true;
                }
            }
            return false;
        }

    }
}
