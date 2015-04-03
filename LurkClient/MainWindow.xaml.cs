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

namespace LurkClient
{
    /// <summary>
    /// Class for the main window of the application
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// The current page being displayed on the main window
        /// </summary>
        private Page curPage;
        public MainWindow()
        {
            InitializeComponent();
            Globals.MainWin = this;
            curPage = new ConnectPage();
            SwitchPage(curPage);
        }

        /// <summary>
        /// Switches the page being displayed currently and adjusts the window's
        /// height and width settings appropriately
        /// </summary>
        /// <param name="newPage">
        /// The page to switch the current view to
        /// </param>
        public void SwitchPage(Page newPage)
        {
            this.Content = newPage;
            curPage = newPage;
            this.SizeToContent = SizeToContent.WidthAndHeight;
        }

        /// <summary>
        /// Closes the window, and if the game is currently playing, cleans up 
        /// the game
        /// </summary>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            curPage = curPage as GameWindow;
            GameWindow gWin = curPage as GameWindow;
            if (curPage != null)
            {
                gWin.CleanUp();
            }
        }
    }
}
