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
    /// Interaction logic for ConnectPage.xaml
    /// </summary>
    public partial class ConnectPage : Page
    {
        /// <summary>
        /// The port to connect to for the game
        /// </summary>
        private int port;

        /// <summary>
        /// The host to connect to
        /// </summary>
        private string host;

        public ConnectPage()
        {
            InitializeComponent();
            hostEntry.Focus();
        }

        /// <summary>
        /// Try to connect to the server using the user-provided hostname and port
        /// </summary>
        private void ConnectToServer(object sender, RoutedEventArgs e)
        {
            try
            {
                port = Int32.Parse(portEntry.Text);

            }
            catch
            {
                MessageBox.Show("The port number you entered is invalid", "Invalid port number", MessageBoxButton.OK,
                                MessageBoxImage.Exclamation);
                return;
            }

            host = hostEntry.Text;

            if (host.Length == 0)
            {
                MessageBox.Show("You didn't enter a hostname!", "No hostname entered", MessageBoxButton.OK,
                                MessageBoxImage.Exclamation);
                return;
            }

            try
            {
                Globals.ClientSocket.Connect(host, port);
                //MessageBox.Show("We connected successfully!", "Successfully connected!", MessageBoxButton.OK);
            }
            catch
            {
                MessageBox.Show(String.Format("We were unable to connect to {0} on port {1}", host, port.ToString()), "Unable to connect",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Globals.MainWin.SwitchPage(new GameWindow());
        }
    }
}
