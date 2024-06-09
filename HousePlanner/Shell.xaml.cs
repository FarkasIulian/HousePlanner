using DevExpress.Xpf.Core;
using DevExpress.XtraEditors;
using System.ComponentModel;
using System.Windows.Forms;

namespace HousePlanner
{
    /// <summary>
    /// Interaction logic for Shell.xaml
    /// </summary>
    public partial class Shell : ThemedWindow
    {
        public Shell()
        {
            InitializeComponent();
        }

        private void ThemedWindow_Closing(object sender, CancelEventArgs e)
        {
            XtraMessageBoxArgs args = new XtraMessageBoxArgs()
            {
                Caption = "Confirmation",
                Text = "Do you want to close the application?",
                Buttons = new DialogResult[] { System.Windows.Forms.DialogResult.Yes, System.Windows.Forms.DialogResult.No },
                DefaultButtonIndex = 1,
                AutoCloseOptions = new AutoCloseOptions()
                {
                    Delay = 5000,
                    ShowTimerOnDefaultButton = true,
                }
            };
            if (XtraMessageBox.Show(args) == System.Windows.Forms.DialogResult.No)
                e.Cancel = true;
        }

        
    }
}
